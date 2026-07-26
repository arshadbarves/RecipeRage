# GameFlow Hard Purge — Design (Phase 2)

**Date:** 2026-07-14  
**Branch:** `architecture-cleanup`  
**Status:** Design locked for implementation (product intent: delete dual worker SM; prior user approval “Yes please” + production-grade purge)  
**Predecessor:** `docs/superpowers/specs/2026-07-13-gameflow-production-migration-design.md` (Approach A complete — `IAppFlow` sole public navigator; workers internal)

---

## 1. Problem

Phase 1 made `IAppFlow` the only **public** product navigator. Internally, Flow ports still drive a second navigator:

| Layer | Role today |
|-------|------------|
| `IAppFlow` / `AppFlowController` | Legal phase graph + port Enter/Exit |
| `IGameStateManager` + `*State` | Parallel “current state” machine; ports call `ChangeState`; workers still dual-call `EnterSidePhase` + `ChangeState` |

That is not a clean production architecture:

1. **Two sources of “where am I?”** — `AppFlow.Current` vs `IGameStateManager.CurrentState`.
2. **Side phases have no port dispatch** — `AppFlowController.EnterPhase` ignores Login/Maintenance/Tutorial/etc.; workers enter via `ChangeState` only.
3. **Workers still navigate** — Bootstrap/Login/SessionLoading/Maintenance call `ChangeState` for sibling workers (internal dual graph).
4. **Dead DI surface** — `SessionManager` injects `IGameStateManager` unused; `GameBootstrapper` injects it unused; `NetworkingServiceContainer` only forwards it to `GameStarter` legacy fallback.
5. **~1.7k LOC** of state framework + workers that encode the same lifecycle ports already own.

**Goal:** Delete `IGameStateManager` and the `IState` framework from the product path. Ports (and port-owned handlers) own all Enter/Exit work. `AppFlowController` is the only phase owner.

---

## 2. Assumptions (locked)

1. **In scope:** Absorb worker Enter/Exit into Flow ports / handlers; wire side-phase Enter/Exit through AppFlow; delete `IGameStateManager`, `GameStateManager`, `StateMachine`, `IState`, `BaseState`, `IStateFactory`, `GameStateFactory`, all `Infrastructure/States/*State` classes; remove legacy `ChangeState` fallbacks; update DI, tests, wiki/`CLAUDE.md`.
2. **Out of scope:** Kitchen Brawler combat, map content, MatchContext `FindObjectOfType` / `NetworkManager.Singleton` cleanup, EOS feature work, PR auth, untracked v2 WIP.
3. **Do not change** product phase graph semantics from Phase 1 (Intro + Countdown required; always-resolve MM; soft-popup policy; side-phase return rules including Boot/Splash → Home upgrade).
4. **Null\* ports** remain for `AppFlowController` unit tests only — never in Composition.
5. **Autopilot / prior approval** of hard purge is treated as design-direction approval of Approach C below.

---

## 3. Approaches considered

### A — Delete SM, inline all logic into port classes

Move every `*State.Enter/Exit` body into the corresponding `*FlowPort`.

- **Pros:** Fewest types; obvious “port does the work.”
- **Cons:** Ports become 200–400 LOC god objects; hard to unit-test boot/MM without Unity; poor file cohesion.

### B — Keep a private mini state machine (rename only)

Hide `IGameStateManager` as `IPhaseWorkerHost` used only by ports; keep `*State` classes.

- **Pros:** Smallest diff.
- **Cons:** Dual “current phase” remains; side-phase gap remains unless also fixed; user asked to **purge** legacy, not rename it.

### C — Port-owned phase handlers + delete SM framework (recommended)

Convert each worker into a **handler** (plain class, no `IState`/`ChangeState`) with `Enter`/`Exit`/(optional)`Tick`. Ports inject handlers and call them. AppFlow remains sole phase owner. Side phases get a real port.

- **Pros:** Deletes dual navigator; keeps logic testable and file-sized; matches Phase 1 port boundaries; production-clean.
- **Cons:** Medium refactor (all states + DI + side-phase wiring); Matchmaking timeout tick must move to `ITickable` handler/port.

**Decision: Approach C.**

---

## 4. Target architecture

```
UI / ViewModels / Features
        │  intents only (IAppFlow)
        ▼
   AppFlowController  (sole phase owner + legal graph)
        │  Enter/Exit ports (including side phases)
        ▼
   Flow ports (Infrastructure.Flow)
        │  own handlers
        ▼
   Phase handlers (Infrastructure.Flow.Handlers or .Phases)
        │  no ChangeState — only IAppFlow notifies + services
        ▼
   Scenes / UIService / Session / Match / Network
```

**Deleted:**

- `Application/State/IGameStateManager.cs`
- `Application/State/IState.cs`
- `Application/State/IStateFactory.cs`
- `Application/State/BaseState.cs`
- `Application/State/StateMachine.cs`
- `Infrastructure/DI/GameStateManager.cs`
- `Infrastructure/DI/GameStateFactory.cs`
- `Infrastructure/States/*State.cs` (all 11)

**Retained concepts as handlers (new names):**

| Old worker | New owner | Handler (suggested) |
|------------|-----------|---------------------|
| `BootstrapState` + `SessionLoadingState` | `BootFlowPort` | `BootSequence` (init + session load) |
| `MainMenuState` | `HomeFlowPort` | `HomePhase` (menu scene + hub UI) |
| `MatchmakingState` | `MatchmakingFlowPort` | `MatchmakingPhase` (+ `ITickable` timeout) |
| `GameplayState` | `MatchRuntimeFlowPort` | `MatchRuntimePhase` (map load + StartRound gate) |
| `GameOverState` | `ResultsFlowPort` | `ResultsPhase` (events + results UI already partly in port) |
| `LoginState` | Side-phase port | `LoginPhase` |
| `MaintenanceState` | Side-phase port | `MaintenancePhase` |
| `NoConnectionState` | Side-phase port | `NoConnectionPhase` |
| `TutorialState` | Side-phase port | `TutorialPhase` |
| `AccountUpgradeState` | Side-phase port | `AccountUpgradePhase` |
| ForceUpdate (event-only today) | Side-phase port | UI via existing `ForceUpdateEvent` / thin `ForceUpdatePhase` if needed |

Handlers **must not** inject a state manager. Navigation out = `IAppFlow` only (`NotifyBootComplete`, `EnterSidePhase`, `CompleteSidePhase`, `ReturnHome`, `NotifyMatchResolved`, etc.).

---

## 5. Side-phase port (required gap fix)

Today `EnterPhase`/`ExitPhase` have **no cases** for side phases. Phase 2 adds:

```csharp
// Playcenter.GameFlow
public interface ISidePhasePort
{
    void EnterSidePhase(FlowPhaseId phase, FlowContext context);
    void ExitSidePhase(FlowPhaseId phase);
}
```

`AppFlowController`:

- Constructor gains optional `ISidePhasePort sidePhases` (null-ok for pure unit tests).
- `EnterPhase`: if `IsSidePhase(phase)` → `_sidePhases?.EnterSidePhase(phase, _context)`.
- `ExitPhase`: if `IsSidePhase(phase)` → `_sidePhases?.ExitSidePhase(phase)`.

KitchenClash implementation `SidePhaseFlowPort`:

- Maps phase → handler Enter/Exit.
- ForceUpdate: show/halt UI (event already published by boot); no worker SM.
- Unknown side phase: log + no-op (fail soft).

**Chaining** (Maintenance → Login) stays on `IAppFlow.EnterSidePhase` (Phase 1 fix: preserve `_sideReturnPhase`). Handlers never `ChangeState` to each other.

---

## 6. Boot pipeline (absorbs Bootstrap + SessionLoading)

`BootFlowPort.EnterBoot` runs `BootSequence.RunAsync(ct)`:

1. NTP sync (best-effort)
2. Remote config init + refresh
3. Force-update check → if required: `EnterSidePhase(ForceUpdate)` and **return** (do not complete boot)
4. Maintenance check → if active: `EnterSidePhase(Maintenance)` and return
5. Auth check → if unauthenticated: `EnterSidePhase(Login)` and return
6. Session load (`SessionManager.CreateSession` + existing session-context work from `SessionLoadingState`)
7. `NotifyBootComplete()` → AppFlow → Home

**Login success path** (side phase, AppFlow already left Boot):

1. `LoginPhase` on success → `SessionLoader.LoadAsync()`
2. `CompleteSidePhase()` → Home (return target Boot/Splash upgraded to Home per Phase 1)

**Do not** call `NotifyBootComplete` after Login — current phase is Login, not Boot. Phase 1 dual-call was for SessionLoading still under Boot; after purge, authenticated boot never leaves Boot until step 7.

**Cancellation:** `ExitBoot` / handler dispose cancels in-flight `RunAsync` (replace `IsStateActive` with `CancellationToken` / `_active` flag on handler).

---

## 7. Main-path ports (behavior parity)

### Home

- Load MainMenu scene if needed; show HomeScreen (existing Type.GetType pattern).
- Exit: no hard hide requirement beyond current MainMenuState.Exit.

### Matchmaking

- Apply queue params from `PlayRequest` / context.
- Show MatchmakingScreen; start search.
- On found → `NotifyMatchResolved(...)` (existing factory).
- On cancel/fail → `CancelMatchmaking` / `ReturnHome` as today.
- **Timeout bot-fill:** move `Update` loop to `MatchmakingPhase : ITickable` (or port `ITickable`) registered in Root — **only** tick source after SM deletion.
- Exit: hide screen + cancel in-flight (idempotent).

### Match runtime

- Idempotent map load (Gameplay scene) on `EnterMatch`.
- `StartRound` / pending flag parity with current `MatchRuntimeFlowPort` + `RequestStartRound` gate.
- Exit: teardown parity with `GameplayState.Exit`.

### Results

- Enter: match-end SFX/events (from GameOverState) + show ResultsScreen.
- Exit: hide ResultsScreen.

Splash / Intro / Countdown / Analytics: already port-native — no SM dependency; leave behavior unchanged.

---

## 8. Consumer cleanup

| Consumer | Action |
|----------|--------|
| `GameBootstrapper` | Inject **only** `IAppFlow`; drop `IGameStateManager` |
| `RootLifetimeScope` | Remove `RegisterGameStates` / SM registrations; register handlers + `ISidePhasePort`; ports construct with handlers |
| `SessionManager` | Drop unused `IGameStateManager` ctor param |
| `NetworkingServiceContainer` | Drop `IGameStateManager`; `GameStarter` no longer receives it |
| `GameStarter` | Remove legacy `ChangeState<MainMenuState>` fallback; **require** `IAppFlow` for leave-match → home |
| EditMode tests | Replace SM fakes with handler/port fakes; keep `AppFlowMigrationTests` / FakeAppFlow patterns |
| Reflection scan of `IState` | Delete |

---

## 9. File layout (target)

```
Assets/Playcenter/GameFlow/Runtime/Ports/IFlowPorts.cs  # + ISidePhasePort
Assets/Playcenter/GameFlow/Runtime/Core/AppFlowController.cs  # side phase dispatch

Assets/_KitchenClash/Infrastructure/Flow/
  BootFlowPort.cs, SplashFlowPort.cs, ...
  SidePhaseFlowPort.cs
  StateMachineFlowPorts.cs → split/rename to Home/Matchmaking/MatchRuntime/Results ports
  Handlers/
    BootSequence.cs
    HomePhase.cs
    MatchmakingPhase.cs
    MatchRuntimePhase.cs
    ResultsPhase.cs
    LoginPhase.cs
    MaintenancePhase.cs
    NoConnectionPhase.cs
    TutorialPhase.cs
    AccountUpgradePhase.cs
    SessionLoader.cs   # shared by BootSequence + LoginPhase
```

Optional: keep thin files; do not require one-type-per-file if a small side-phase file groups tiny handlers — prefer clarity over dogma.

---

## 10. Testing strategy

1. **Playcenter.GameFlow unit tests** (if any / extend): side phase Enter/Exit invokes `ISidePhasePort`; CompleteSidePhase still returns correctly; chaining preserves return phase.
2. **KitchenClash EditMode:**  
   - BootSequence branches (force update / maintenance / login / success → NotifyBootComplete).  
   - MatchmakingPhase timeout → FillMatchWithBots.  
   - MatchRuntimePhase StartRound gate.  
   - Grep gates (below).
3. **Build:** `dotnet build` Core/Gameplay/EditMode — 0 errors.
4. **Unity smoke (manual):** Cold boot → Home → Play → Intro → Countdown → Match → Results → Home; Login; Maintenance; ForceUpdate halt.

`dotnet test` does not discover Unity NUnit EditMode tests — verify via build + Unity Test Runner (same as Phase 1).

---

## 11. Success criteria (delete gates)

All must pass before calling Phase 2 complete:

1. **Zero product references** to `IGameStateManager`, `GameStateManager`, `IState`, `BaseState`, `IStateFactory`, `ChangeState<` outside deleted code / git history.
2. **Grep gate:**  
   `rg "IGameStateManager|GameStateManager|ChangeState<" Assets/_KitchenClash Assets/Scripts --glob '*.cs'` → no matches (except possibly comments in docs — prefer zero).  
   `rg "class \\w+State\\b" Assets/_KitchenClash/Infrastructure` → no state workers.
3. **Composition:** no Null\* production ports; `ISidePhasePort` registered with real `SidePhaseFlowPort`.
4. **Presentation:** still 0 `ChangeState` / 0 `IGameStateManager` (already true).
5. **EditMode assemblies build** with 0 errors.
6. **Wiki + CLAUDE.md** describe handlers/ports only — no “workers via IGameStateManager.”
7. **Behavior parity** with Phase 1 product path (no skipped Intro/Countdown; side-phase return rules intact).

---

## 12. Implementation order (plan will expand)

1. Add `ISidePhasePort` + AppFlowController dispatch + unit coverage.
2. Introduce `SessionLoader` + `BootSequence`; rewire `BootFlowPort`; stop using Bootstrap/SessionLoading states.
3. Side-phase handlers + `SidePhaseFlowPort`; remove dual ChangeState from login/maintenance paths.
4. Home / Matchmaking (incl. ITickable) / MatchRuntime / Results handlers; ports call handlers only.
5. Strip SM from DI, Bootstrapper, SessionManager, Networking, GameStarter.
6. Delete Application.State framework + Infrastructure.States + GameStateManager/Factory.
7. Tests, grep gates, wiki/`CLAUDE.md`, final review.

---

## 13. Risks & mitigations

| Risk | Mitigation |
|------|------------|
| Boot race / double Home | Single completion path: only `NotifyBootComplete` or `CompleteSidePhase`, never both for same journey |
| Matchmaking timeout silent-fail | Explicit `ITickable` registration test / log on tick start |
| Side phase enter with no UI | SidePhaseFlowPort maps every `FlowPhaseId` side value; ForceUpdate covered |
| Large PR | Ordered commits per step above; each builds green |
| Hidden ChangeState in features | Grep gate in Task N + final review |

---

## 14. Non-goals (explicit)

- Rewriting Playcenter.GameFlow phase graph.
- Moving gameplay systems into ports.
- Deleting `IAppFlow` proxy / cycle-break pattern.
- Match scene FindObjectOfType cleanup.

---

## 15. Decision record

| Decision | Choice |
|----------|--------|
| Approach | **C** — port-owned handlers; delete SM framework |
| Side phases | New `ISidePhasePort` dispatched by AppFlowController |
| Session load | Shared `SessionLoader`; Boot (auth) vs Login success |
| Tick | MatchmakingPhase `ITickable` in Root |
| Naming | `*Phase` / `BootSequence` / `SessionLoader` — not `*State` |
| Public API | Unchanged `IAppFlow` surface from Phase 1 |
