# LLM Developer Rules — Kitchen Clash

This page is the authoritative rule sheet for any LLM agent or developer working in this codebase.
It is derived from `SKILL.md v3` in the aspirational GDD and extended with project-specific protocols.

---

## Stack Reference

| Layer | Technology |
|-------|-----------|
| Engine | Unity 6.0 (6000.3.0f1) |
| Language | C# |
| Networking | Unity NGO over EOS P2P (EOSTransport) |
| Auth | EOS Connect (Google/Facebook/Apple ExternalCredentialType — production path; Firebase may exist for analytics/config) |
| UI | UI Toolkit + MVVM + UIService / UIScreenStackManager (RouterService is aspirational, not production) |
| DI | VContainer — Root → Menu → Match scopes |
| Async | UniTask |
| Config | Firebase Remote Config via IConfigService |
| Analytics | Firebase Analytics + Crashlytics |
| Player Data | EOS Player Data Storage (5 MB/player) |

---

## Architecture Rules

### Clean Architecture Layers

```
Domain        → pure C#, zero UnityEngine, 100% unit-testable
Application   → pure C#, depends on Domain interfaces only
Presentation  → UI Toolkit, depends on Application only
Infrastructure → Unity + EOS + Firebase + NGO implementations
```

**MonoBehaviour is allowed ONLY in these four files:**
- `UIDocumentRoot.cs` — mounts UIDocument, provides root VisualElement
- `InputReceiver.cs` — reads touch input, implements IDualStickInput
- `NetworkObjectAdapter.cs` — thin wrapper for NGO NetworkBehaviour
- `[Scene]LifetimeScope.cs` — VContainer composition roots

### VContainer Scope Hierarchy

```
RootLifetimeScope (app-lifetime, DontDestroyOnLoad) → app-lifetime singletons + gateways
  Session child (CreateSession only: lobby, MM, team, economy/wallet, INetSession, MatchRewardHandler)
  MenuLifetimeScope (MainMenu scene, empty Configure; MenuSceneBinder → Root gateways)
  MatchLifetimeScope (Game scene child of Root: score, orders, abilities, hazards, bots)
```

**Rule:** Never inject a child-scope service into a parent scope.

**Session installer rule:** `SessionManager.CreateSession` requires `ISessionScopeInstaller` (`MenuSessionScopeInstaller` → `MenuSessionRegistrations`). Never bare-create the **session** child with empty `Configure`. Scene Menu must **not** re-run `MenuSessionRegistrations` (orphan / double-credit). Scene scopes parent to Root (`FindParent` + TypeName).

### Remote Config Rule

**Every tunable value must be an `IConfigService.Get(key, fallback)` call.**  
No hardcoded balance numbers anywhere in logic code.

RC key namespaces: `score_*` | `chop_taps_*` | `match_*` | `ability_*` | `order_*` | `slot_*` | `trophy_*` | `ad_*` | `daily_streak_*`

---

## Forbidden Patterns

| Pattern | Why Forbidden |
|---------|--------------|
| Firebase Auth for production auth | EOS Connect handles auth directly via ExternalCredentialType (Firebase may exist for analytics/config) |
| Unity Relay | EOS P2P (EOSTransport) provides free relay — no cost |
| `EOS_P2P_SendPacket` for game state | NGO + EOSTransport handles this |
| Floating joystick | Fixed positions only in InputReceiver.cs |
| Hold-to-chop | Multi-tap right stick only |
| Static singletons | VContainer injection only |
| MonoBehaviour in Domain or Application | Those layers are pure C# |
| Hardcoded balance numbers | All values = IConfigService.Get |
| Ads during a match | No interstitials or banners mid-match |
| Interstitials for Battle Pass owners | Disabled for BP subscribers |
| `NetworkManager.Singleton` | Use injected NetworkManager instance |
| `FindObjectOfType` | Use MatchRuntimeSceneBinder / IMatchContext |
| MATCH-scope economy/wallet mutation | Wallet writes only SESSION via `IWalletLedger` |
| Presentation → EOS / NGO / `EOSManager` direct | Ports + Application/Infrastructure only |
| UnityEngine types in `Playcenter.Services` | Use `InputAxis2`, not `Vector2` |
| Game-side Ads/Analytics/IAP/RC **service** implementations | Shared logic is `Playcenter.Services`; game keeps seams only |
| Vendor SDK refs (`Firebase.`, `MaxSdk`, `UnityEngine.Purchasing`) outside `Playcenter.Services.Unity` (and game `FirebaseConfigProvider`) | Vendor firewall — adapters only |
| Bare `SessionManager` child without installer | Missing economy/wallet/net registrations |
| `MenuSessionRegistrations` from scene `MenuLifetimeScope` | Second install / orphan entry points / double wallet credit |
| Scene LifetimeScope with empty parent (orphan root) | Missing parent `IEventBus` / root services |
| FOFT-register MainMenu components inside `CreateSession` | MainMenu not loaded at login; use `MenuSceneBinder` + Root gateway |
| `LeaveParty` when only ending a match | Use `LeaveMatchLobby`; party survives match |
| Shell UI classes on `theme.uss` | Shell components belong on `DesignSystem.uss` (`pc-*`) |
| Ad-hoc NGO host/client start in new code | Use `INetSession.StartAsync` / `StopAsync` |
| Boot NTP/RC before connectivity gate | Step 0 is `IConnectivityService.IsOnline` |

---

## Playcenter Client OS — Required / Forbidden

Authoritative detail: `wiki/Technical.md` § Playcenter Client OS — Runtime Laws.  
Spec: `docs/superpowers/specs/2026-07-19-playcenter-client-os-design.md`.

### REQUIRED

- Connectivity gate (`IConnectivityService`) **before** network boot services (NTP, RC, force-update, maintenance, auth)
- `ISessionScopeInstaller` when `SessionManager.CreateSession` (**sole** `MenuSessionRegistrations` path)
- Scene Menu/Match parent to Root; scene bind-in via binders/gateways (not dual session install)
- Wallet writes only at **SESSION** via `IWalletLedger` (`EconomyService` dual-impl)
- Party lobby ≠ match lobby (`CurrentPartyLobby` / `CurrentMatchLobby`; `LeaveMatchLobby` ≠ `LeaveParty`)
- Net start/stop via `INetSession` in new code (`NgoEosNetSession` adapter)
- Shell UI classes on **`DesignSystem.uss`** (`pc-btn`, `pc-panel`, `pc-chip`, `pc-party-slot`) — not `theme.uss`
- `NotifyBootComplete` only if `IAppFlow.Current == Boot`; side-phase success → `CompleteSidePhase`
- Settings at ROOT (`ISettingsService` / `GameSettings`); gameplay input via `IGameplayInput` + `InputAxis2`

### FORBIDDEN (Client OS)

- MATCH-scope economy/wallet mutation
- Presentation referencing Epic / `NetworkManager` / `EOSManager`
- UnityEngine types in `Playcenter.Services` (no `Vector2` — use `InputAxis2`)
- Bare SessionManager child without installer
- `LeaveParty` when only ending a match
- Host migration assumptions in v1 reconnect (stop on forfeit/host-drop only)

---

## Playcenter Studio SDK — Required / Forbidden

Authoritative detail: `wiki/Technical.md` § Playcenter Studio SDK.  
Spec: `docs/superpowers/specs/2026-07-20-playcenter-studio-sdk-design.md`.  
Skill: `.github/skills/playcenter-sdk/SKILL.md`.

### REQUIRED

- `PlaycenterClient.RunAsync` as **sole** app init path after W2 (replaces `BootSequence` + `IAppFlow.StartColdBoot()`)
- `IGameEntry.OnPlaycenterReady(client)` for post-SDK game wiring (auth → CreateSession → IAppFlow → Home)
- SDK DI = **Builder + IServiceRegistry** inside `Assets/Playcenter/**`; game bridges via `client.Services.Get<T>()`
- Vendor adapters (`Playcenter.EOS`, NGO adapters) only in adapter assemblies — game Presentation/Application see ports only
- `ShellRef` / `BootRetryRef` holders in `RootLifetimeScope` to break AppFlow↔bootstrap DI cycle
- Session DI law unchanged: `CreateSession` + `ISessionScopeInstaller` (S9 unmodified)
- SDK shell UXML themed via tokens + USS override; never fork per-title UXML

### FORBIDDEN (Studio SDK)

| Pattern | Why |
|---------|-----|
| `using VContainer` in `Assets/Playcenter/**` | S2 — SDK DI is Builder + ServiceRegistry |
| `Epic.*` / `EOS.*` in game **Presentation** or **Application** | S4 vendor firewall (grep gate W5) |
| `BootSequence` class or `IAppFlow.StartColdBoot()` after W2 | S8/S13 hard cutover; no dual boot |
| Interactive login inside a module | Login only after `OnPlaycenterReady`; modules warm-only |
| Copying SDK UXML into game | Theme via tokens/USS override; one implementation per screen |
| Re-registering SDK singletons in VContainer | Bridge via `client.Services` (S3) |
| Orphan session DI inside SDK boot | Session DI law (S9) unchanged |

---

## Playcenter Shared Services — Required / Forbidden

Authoritative detail: `wiki/Technical.md` § Playcenter Shared Services.  
Spec: `docs/superpowers/specs/2026-07-22-playcenter-shared-services-design.md`.

### REQUIRED

- Ads / Analytics / IAP / RemoteConfig **flow** in `Playcenter.Services` (engine-free)
- Vendor adapters only in `Playcenter.Services.Unity` behind `#if` defines
- Game Composition wires facades + ports; game seams = grantor, RC provider, event bridge, catalogs/constants
- RC change notification via SDK C# events; bridge to `IEventBus` in game Infrastructure
- IAP grantor resolves economy lazily (session-scoped) — never assume root `IEconomyService`

### FORBIDDEN (Shared Services)

| Pattern | Why |
|---------|-----|
| `StubAdsService` / `StubIAPService` / game `*AnalyticsService` / `CompositeRemoteConfigService` / `FallbackRemoteConfigService` | Hard cutover — deleted; use SDK facades |
| Re-implementing Ads/Analytics/IAP/RC service classes in KitchenClash | Common logic is multi-title SDK |
| `VContainer` / `UnityEngine` / vendor SDKs inside `Playcenter.Services` Runtime | Pure C# only |
| Vendor SDK usings in Presentation / Application | Firewall — adapters + Infrastructure seams only |
| Dual-path feature flags keeping old stubs alive | AAA cutover; one implementation |

---

## Playcenter MobileCore — Required / Forbidden

Authoritative detail: `wiki/MobileCore.md`.
Spec: `docs/superpowers/specs/2026-07-24-playcenter-mobile-core-design.md`.

### REQUIRED

- `PlaycenterBootstrap` as the sole scene entry for the Playcenter stack; one prefab
- `IGameClock` for all time in Core — no `Time.`/`DateTime.` in Core logic
- Bot planning under `IBotBudget` time-slice — never unbounded scans per tick
- Reconnect via `ReconnectStateMachine` — no ad-hoc retry loops in game code
- Net start/stop via `NetSessionOrchestrator` — no direct `INetSession` in new code
- Seeded `Random` in `BotBrain` — deterministic bot behavior per match seed
- `InputFrame` version byte bumped on any wire-format change
- Session scope factory implemented game-side (`ISessionScopeFactory`) — module stays DI-neutral

### FORBIDDEN (MobileCore)

| Pattern | Why |
|---------|-----|
| `UnityEngine`/`VContainer`/`Netcode`/`Epic`/`Firebase`/`Cysharp` usings in `Core/` | Vendor firewall — CI grep gate |
| Second bootstrap MonoBehaviour for Playcenter stack | One entry point |
| Game-side reimplementation of dual-stick/gesture/reconnect/claim logic | Common logic lives in the module |
| Hardcoded timing/tuning in Core | Option structs + `mc_*` RC keys |
| Dual-path old/new subsystems | Hard cutover — delete in same commit |
| DI-container reference inside the module | Session factory is a game-side seam |

---

## Controls (Brawl Stars Fixed Dual-Joystick)

| Input | Action |
|-------|--------|
| Left stick | Move chef (8-directional) |
| Right stick | Aim direction. Release = interact with nearest aimed station |
| Right stick rapid multi-tap | Chop at prep station (tap count per ingredient via RC) |
| ABILITY button | Chef active ability |
| SUPER button | Charged super ability (charged by completing dishes) |
| GADGET button | 1-use gadget per match |

---

## Authentication Flow

```
1. Launch → EOS.Platform.Create() → EOS Connect.Login(DeviceId)  [Guest PUID assigned]
2. After 3rd match → show auth nudge modal (Google | Facebook | Apple | Stay Guest)
3. Google path:
   a. GoogleSignIn.DefaultInstance.SignIn() → idToken
   b. EOS Connect.Login(ExternalCredentialType.GoogleIdToken, token)
   c. EOS returns permanent ProductUserId
   d. EOS Connect.LinkAccount(DeviceId PUID → Google PUID)  [preserves guest data]
   e. Save PUID to PlayerPrefs for fast re-auth on next launch
4. Apple (required on iOS App Store): same flow, ExternalCredentialType.AppleIdToken
5. Facebook: ExternalCredentialType.FacebookAccessToken
```

---

## Connectivity Handling (Brawl Stars Style)

| State | UI Shown | Behaviour |
|-------|---------|-----------|
| Online | Nothing | Normal |
| Offline — Menu | Full-screen blocking overlay | Retries every 3s, auto-dismisses on restore |
| Offline — In Match | Semi-transparent overlay + countdown | 3 reconnect attempts × 5s each. Fail = forfeit + return to menu |
| Host dropped (v1) | 'Reconnecting...' overlay | **No host migration.** Reconnect window then forfeit/end; `NetSessionConnectivityBridge` → `INetSession.StopAsync` |

---

## Adding New Features (Checklists)

### New Chef
1. Create `[Chef]Passive.cs` implementing `IAbility` (Slot = Passive)
2. Create `[Chef]Active.cs` implementing `IAbility` (Slot = Active)
3. Create `[Chef]Super.cs` implementing `IAbility` (Slot = Super)
4. Register in `ChefAbilityRegistrySO`
5. Add RC keys for all cooldowns and tunable values
6. No changes to `AbilityService` — Open/Closed principle

### New Game State
1. Create `MyState : BaseState` in `Infrastructure/States/`
2. Namespace must be `KitchenClash.Infrastructure.States` for auto-registration
3. Trigger with `_stateManager.ChangeState<MyState>()`

### New UI Screen
1. Create UXML + USS in `_KitchenClash/UI/Screens/`
2. Create `MyScreen : BaseUIScreen` annotated with `[UIScreen]`
3. Auto-registered Transient by root scope reflection scan
4. Show with `_uiService.ShowScreen<MyScreen>()`

### New Service
1. Define `IMyService` in `Application/Interfaces/`
2. Implement in `Infrastructure/Services/`
3. Register in appropriate `LifetimeScope.Configure()`
4. Inject via constructor — VContainer resolves automatically

---

## Drift Warning Protocol

See [DRIFT-PROTOCOL.md](DRIFT-PROTOCOL.md) for the full procedure.

**Summary:** If any implementation decision contradicts this wiki, stop and warn the user before proceeding.

---

## Source Document

Derived from: `Documentation/KitchenClash_GDD_v3_aspirational.docx` — Section 18 (SKILL.md v3)  
Extended with: `CLAUDE.md`, `Documentation/Architecture/PROJECT_MEMORY.md`
