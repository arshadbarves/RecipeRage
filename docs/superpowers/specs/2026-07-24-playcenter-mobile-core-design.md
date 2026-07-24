# Playcenter.MobileCore — Design Spec

**Date:** 2026-07-24
**Status:** Approved design (user-reviewed sections 1–5)
**Author:** Brainstorm session (Copilot CLI + arshadbarves)
**First consumer:** RecipeRage (reference port)
**Precedent specs:** `2026-07-20-playcenter-studio-sdk-design.md`, `2026-07-22-playcenter-shared-services-design.md`

---

## 1. Purpose

Reusable, production-grade mobile multiplayer core for Brawl-class titles:

1. **Input** — dual-stick model with gesture detection (multi-tap), network-ready versioned frames
2. **Session lifecycle** — fail-closed match-session FSM with DI-agnostic scope seam
3. **Bots** — generic task-planner framework (priority-chain evaluators, claim registry, CPU budget)
4. **Net glue** — reconnect state machine, connection quality tracking, `INetSession` orchestration

Greenfield design. RecipeRage behaviors (dual-stick + chop taps, `SessionManager` + installer law, `BotTaskPlanner` priority chains, `NgoEosNetSession` reconnect) port in as reference implementations. RecipeRage cutover is hard-cutover per subsystem (wiki law: one implementation, no dual-path).

---

## 2. Assembly Layout (single assembly + unified bootstrap)

```
Assets/Playcenter/MobileCore/
  Runtime/
    Playcenter.MobileCore.asmdef      # refs: Playcenter.SDK, Playcenter.Services, Playcenter.Shell,
                                      #       UnityEngine.CoreModule, Unity.InputSystem
                                      # (VContainer referenced via game-side adapter only — see note)
    Core/       # engine-free logic — CI grep gate: no UnityEngine/VContainer/Netcode/Epic/Firebase usings
      Input/        DualStickModel, TapGestureDetector, InputFrame (v1 DTO), InputMapper, InputButtons
      Session/      SessionLifecycleController, ISessionScopeFactory, ISessionScopeHandle, ISessionScopeInstaller
      Bots/         BotHost, BotBrain, TaskPlanner<TSnapshot,TTask>, ITaskEvaluator, ClaimRegistry<TKey>,
                    IBotBudget, IBotIntent
      Net/          ReconnectStateMachine, BackoffPolicy, ConnectionQualityTracker, NetSessionOrchestrator
      Clock/        IGameClock, ManualClock
      Telemetry/    IMobileCoreTelemetry + event DTOs
    Adapters/   # the ONLY folder where vendor usings appear
      Input/        TouchDualStickProvider (Unity InputSystem → core pointer events)
      Bots/         NetworkBotDriver (applies IBotIntent to a NetworkBehaviour)
      Net/          NgoEosNetSessionGlue (binds orchestrator to existing INetSession adapters)
      Clock/        UnityGameClock (Time.deltaTime bridge)
      # NOTE: VContainerSessionScopeFactory lives in the GAME's Composition layer, not here —
      # the module references no DI container. Games implement ISessionScopeFactory with
      # whichever container they use (RecipeRage: VContainer). This keeps the module DI-neutral.
    Bootstrap/  # PlaycenterBootstrap.cs — THE MonoBehaviour entry point

Assets/Scripts/Tests/EditMode/Playcenter/MobileCore/   # money-path tests only (see §8)
```

**Design decisions:**

- **One asmdef.** User direction: no separate `.Unity` adapter assembly. Core/Adapters separation is by folder + CI grep gate (same enforcement mechanism as the W5 SDK vendor-firewall gate). Assembly-level purity is traded for consumer simplicity; the gate preserves testability discipline.
- **References SDK/Services/Shell + Unity engine modules only.** The unified bootstrap needs SDK types (`PlaycenterClient`, `ClientOptions`, `IServiceRegistry`), so the earlier zero-dependency idea is dropped. No DI-container reference: the session scope factory seam (`ISessionScopeFactory`) is implemented game-side. `Core/` depends only on BCL types by convention + gate.
- **Config via constructor-injected option structs.** Games fill from `IConfigService` (RecipeRage) or any source. RC key namespace: `mc_*` (extends the wiki's RC namespace table).
- **Async style:** `System.Threading.Tasks.Task` in Core (matches SDK convention — no UniTask inside SDK assemblies; adapters may use UniTask at the game seam).

---

## 3. Unified Bootstrap

```csharp
// Assets/Playcenter/MobileCore/Runtime/Bootstrap/PlaycenterBootstrap.cs
[DefaultExecutionOrder(-1000)]
public sealed class PlaycenterBootstrap : MonoBehaviour
{
    public static PlaycenterBootstrap Instance { get; private set; }

    [SerializeField] private ClientOptionsAsset _sdkOptions;       // SDK modules, shell theme
    [SerializeField] private MobileCoreOptionsAsset _coreOptions;  // input maps, bot budget, reconnect tuning

    public IPlaycenterServices Services { get; private set; }      // SDK registry
    public MobileCoreContext Core { get; private set; }            // clock, input hub, session ctrl, bot host, net orchestrator

    // Awake:  singleton + DontDestroyOnLoad, build ServiceRegistry, construct MobileCoreContext (clock first)
    // Start:  run SDK boot pipeline → IGameEntry.OnPlaycenterReady(client) → game takes over
    // Update: IGameClock.Tick(Time.deltaTime) → input models, BotHost (budget-sliced), ReconnectStateMachine
    // OnApplicationPause/OnApplicationFocus: feeds connectivity + session lifecycle
}
```

- **One prefab, one entry point** for the whole Playcenter stack (SDK modules + MobileCore services).
- Game boot logic stays behind `IGameEntry` (SDK law). RecipeRage supplies `RecipeRageGameEntry`; future titles supply their own.
- **RecipeRage bridging:** existing `PlaycenterSdkBootstrap` (VContainer `IStartable` in game Composition) is slimmed to a shim that resolves the scene `PlaycenterBootstrap` and hands live VContainer service instances into the SDK registry — no duplicate construction, no dual boot *logic*. DI bridging is not boot logic; the wiki's hard-cutover law applies to the latter.
- `MobileCoreContext` is the facade games consume: `Core.Input`, `Core.Session`, `Core.Bots`, `Core.Net`, `Core.Clock`.

---

## 4. Input Subsystem

**Core (engine-free):**

| Type | Role |
|---|---|
| `DualStickModel` | State machine consuming raw pointer events; produces `InputFrame` per tick; deadzone + sensitivity from `DualStickConfig` |
| `TapGestureDetector` | Multi-tap window detection; tunables: `mc_input_tap_window_ms`, `mc_input_tap_idle_reset_ms` |
| `InputFrame` | Versioned DTO: `{ byte Version=1, uint SequenceNumber, float DeltaTime, InputAxis2 Move, InputAxis2 Aim, InputButtons Buttons }` |
| `InputButtons` | `[Flags]` byte: `Interact \| Ability \| Super \| Gadget \| ChopTap` |
| `InputMapper` | Pure static helpers (keyboard→axis, deadzone, normalization) — ported from `GameplayInputMapper` |

**Decisions:**

1. `InputFrame` replaces `PlayerInputData` — adds `Aim` (current network DTO drops aim; wrong for a brawler), uses `InputAxis2`, carries a version byte for wire-compat evolution.
2. Gesture detection lives in Core, not the provider — unit-testable without a device; PC titles feed mouse through the same detector.
3. Aim is first-class: both sticks in every frame for server-side hit validation.

**Public API:**

```csharp
public readonly struct PointerEvent { int Id; float X; float Y; PointerPhase Phase; }

public sealed class DualStickModel
{
    public DualStickModel(DualStickConfig config, IGameClock clock);
    public void OnPointer(in PointerEvent e);
    public InputFrame Tick();                 // one frame per clock tick
    public bool AimJustReleased { get; }
    public int ChopTapCount { get; }
}
```

**Adapter:** `TouchDualStickProvider` (Unity InputSystem touch/mouse → `PointerEvent` stream).

**RecipeRage cutover (deletes):** `IDualStickInput`, `GameplayInputMapper` (ported), `PlayerInputData` (replaced; NGO serialization wrapper lives in game adapter), `TouchInputProvider`, `InputSystemProvider` (merged into `TouchDualStickProvider`).

---

## 5. Session Lifecycle

**Core:**

- `SessionLifecycleController` — fail-closed FSM: `None → Creating → Active → TearingDown → None`. Illegal transitions throw `InvalidOperationException` (mirrors `AppFlowController` fail-closed-to-Home precedent). `Task`-returning `CreateAsync` / `TeardownAsync`; emits telemetry on every transition.
- `ISessionScopeFactory` — DI seam: `ISessionScopeHandle Create(ISessionScopeInstaller installer)`.
- `ISessionScopeHandle` — `{ IPlaycenterServices Services; void Dispose(); }`.
- `ISessionScopeInstaller` — moves from `KitchenClash.Application` unchanged (already a pure port).

**Adapter (game-side):** `VContainerSessionScopeFactory` — lives in RecipeRage's Composition layer (not the module). Wraps `LifetimeScope` child creation; enforces the installer law exactly as wiki mandates (`CreateSession` requires installer; sole `MenuSessionRegistrations` path; no bare children). Future titles implement `ISessionScopeFactory` with their own container.

**RecipeRage cutover:** `SessionManager` becomes a thin adapter delegating to `SessionLifecycleController`; `ISessionLifecycle` game-facing API unchanged for callers.

---

## 6. Bots — Task-Planner Framework

**Core:**

| Type | Role |
|---|---|
| `BotHost` | Owns N `BotBrain`s; ticks under `IBotBudget` (time-sliced; `mc_bot_budget_ms`, default 2) |
| `BotBrain` | Per-bot loop: snapshot → planner → claim → intent. Seeded `Random` (deterministic); difficulty as data (`BotDifficultyConfig` pattern generalized) |
| `TaskPlanner<TSnapshot,TTask>` | Ordered `ITaskEvaluator<TSnapshot,TTask>` chain; first non-null wins |
| `ITaskEvaluator<TSnapshot,TTask>` | `TTask Evaluate(TSnapshot snapshot)` — null = pass to next |
| `ClaimRegistry<TKey>` | Generic port of `BotClaimRegistry` (station/target claiming with TTL) |
| `IBotIntent` | `{ MoveTarget, InteractTarget, AbilityRequest }` — per-tick output DTO |

**Adapter:** `NetworkBotDriver` — applies `IBotIntent` to a `NetworkBehaviour` (movement + interaction). `BotKitchenSnapshot` stays in RecipeRage as `TSnapshot`.

**RecipeRage cutover (deletes):** `BotManager`, `BotTaskPlanner`, `BotClaimRegistry`. Kitchen evaluators (`ExtinguishFireEvaluator`, `DeliverOrderEvaluator`, `BringToCookingEvaluator`, `BringToPrepEvaluator`, …) are created in RecipeRage Application implementing `ITaskEvaluator` — preserving the existing priority chain (fire → deliver → cooking → prep → …). `BotController`/`BotSpawner` become adapters over `NetworkBotDriver`. Bots remain network objects but **not** NGO player objects (wiki law unchanged).

---

## 7. Net Glue + Reconnect

**Core:**

- `ReconnectStateMachine` — `Connected → Degraded → Reconnecting(attempt N) → Failed`. Implements the wiki connectivity table: menu = blocking overlay, retry every 3s indefinitely; in-match = 3 attempts × 5s then forfeit; host dropped = reconnect window then end (no host migration in v1). All timings RC-tunable (`mc_reconnect_*`).
- `BackoffPolicy` — exponential with jitter, seeded.
- `ConnectionQualityTracker` — RTT EMA + loss signal → `Good/Degraded/Poor` tiers for telemetry + UI badge.
- `NetSessionOrchestrator` — wraps any `INetSession` (from `Playcenter.Services`): start/stop sequencing, role validation, reconnect wiring; consumes `IGameClock`; emits telemetry (`reconnect_attempt`, `reconnect_outcome`).

**Adapter:** `NgoEosNetSessionGlue` — binds orchestrator to RecipeRage's `NgoEosNetSession`. `NetSessionConnectivityBridge` refactored to delegate to `ReconnectStateMachine`. Wiki laws unchanged: `INetSession.StartAsync/StopAsync` remain the sole net start/stop path; no `NetworkManager.Singleton`.

---

## 8. Testing Strategy (amended — money-path only)

**Drift resolution (2026-07-24):** User approved Option B of the drift warning — test the money paths only, not the wiki's blanket ">80% on all new code". Wiki testing section to be updated to record this module-specific amendment.

**Tested (pure-C# state machines where bugs are silent and expensive):**

- `SessionLifecycleController` — all legal/illegal transitions
- `ReconnectStateMachine` + `BackoffPolicy` — menu retry loop, in-match 3×5s then forfeit, deterministic seeded jitter
- `TapGestureDetector` + `DualStickModel` — multi-tap windows, idle reset, aim-release-once, deadzone, sequence increment, determinism under `ManualClock`
- `TaskPlanner` — priority chain order, null-pass-through, empty-chain → Idle
- `ClaimRegistry` — claim/contention/TTL expiry

**Not tested:** thin adapters, DTOs, `PlaycenterBootstrap` glue, option structs — verified by inspection + compile.

**Infra:** `ManualClock` (Core) drives all tests deterministically. Location: `Assets/Scripts/Tests/EditMode/Playcenter/MobileCore/`. NUnit. No new test tooling.

---

## 9. Telemetry & Config

- `IMobileCoreTelemetry` port (Core): events `session_transition`, `reconnect_attempt`, `reconnect_outcome`, `bot_decision_ms`, `input_frame_dropped`, `connection_quality_changed`. Game bridges to `IAnalyticsService` in its Composition layer.
- RC keys (new `mc_*` namespace): `mc_bot_budget_ms` (2), `mc_reconnect_menu_interval_ms` (3000), `mc_reconnect_match_attempts` (3), `mc_reconnect_match_interval_ms` (5000), `mc_reconnect_backoff_base_ms` (1000), `mc_input_tap_window_ms` (300), `mc_input_tap_idle_reset_ms` (500), `mc_input_deadzone` (0.15).

---

## 10. CI Gates

Extend the existing grep-gate pattern:

```
Assets/Playcenter/MobileCore/Runtime/Core/  must NOT match:
  using (UnityEngine|VContainer|Unity\.Netcode|Epic|Firebase|Cysharp)
```

Verified in the same CI step as the W5 SDK vendor-firewall gate. Build verification via `dotnet build` of generated csproj (repo convention).

---

## 11. Build Order

1. **Skeleton** — asmdef, folder layout, `PlaycenterBootstrap` shell, CI grep gate → build green
2. **Clock** — `IGameClock`, `ManualClock`, `UnityGameClock`
3. **Input** — Core + adapter + money-path tests (no game cutover yet)
4. **Session** — Core + adapter + tests
5. **Bots** — framework + tests
6. **Net glue** — Core + adapter + tests
7. **RecipeRage cutover** — one subsystem per commit (input → session → bots → net), old code deleted in the same commit (hard-cutover law), build + existing EditMode suite green after each
8. **Wiki update** — new `wiki/MobileCore.md`; LLM-Rules gains a "Playcenter MobileCore — Required/Forbidden" section; testing amendment recorded; log entry appended

---

## 12. Required / Forbidden (for wiki LLM-Rules)

### REQUIRED

- `PlaycenterBootstrap` as the sole scene entry for the Playcenter stack; one prefab per title
- `IGameClock` for all time in Core — no `Time.`/`DateTime.` in Core logic
- Bot planning under `IBotBudget` time-slice — never unbounded scans per tick
- Reconnect behavior via `ReconnectStateMachine` — no ad-hoc retry loops in game code
- Seeded `Random` in `BotBrain` — deterministic bot behavior per match seed
- `InputFrame` version byte bumped on any wire-format change

### FORBIDDEN

| Pattern | Why |
|---|---|
| `UnityEngine` / `VContainer` / `Netcode` / `Epic` / `Firebase` / `Cysharp` usings in `Core/` | Vendor firewall — CI grep gate |
| Second bootstrap MonoBehaviour for Playcenter stack | One entry point law |
| Game-side reimplementation of dual-stick/gesture/reconnect/claim logic | Common logic lives in the module |
| Bots as NGO player objects | Wiki law unchanged |
| Hardcoded timing/tuning in Core | Option structs + `mc_*` RC keys |
| Dual-path old/new subsystems during cutover | Hard-cutover: delete in same commit |

---

## 13. Success Criteria

1. `Playcenter.MobileCore` builds with zero KitchenClash references; Core/ passes the grep gate.
2. RecipeRage boots via `PlaycenterBootstrap`; `PlaycenterSdkBootstrap` reduced to DI bridge.
3. All four subsystems consumed by RecipeRage with old implementations deleted (no dual-path).
4. Money-path tests green in EditMode; existing test suite unbroken after each cutover commit.
5. A second title can consume the module by dropping one prefab + supplying an `IGameEntry` + evaluators.
6. Bot planner scans never exceed `mc_bot_budget_ms` per tick (telemetry-verified).
