# GameFlow Hard Purge (Phase 2) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Delete `IGameStateManager` / `IState` / all `*State` workers; ports own phase handlers; `AppFlowController` is the sole phase owner including side phases.

**Architecture:** Approach C from `docs/superpowers/specs/2026-07-14-gameflow-hard-purge-design.md`. Convert workers into plain handlers (`*Phase` / `BootSequence` / `SessionLoader`). Add `ISidePhasePort` so Login/Maintenance/etc. enter via AppFlow port dispatch. Strip SM from DI and consumers; delete Application.State framework.

**Tech Stack:** Unity 6, VContainer, UniTask, Playcenter.GameFlow, NUnit EditMode tests, C# / .NET for assembly builds.

## Global Constraints

- Branch: `architecture-cleanup` only (do not commit to main).
- Public product API remains `IAppFlow` only — no new public navigators.
- Do **not** register Null\* ports in Composition; Null\* only for pure unit tests.
- Handlers **must not** inject or call `IGameStateManager` / `ChangeState`.
- Navigation out of a phase = `IAppFlow` only (`NotifyBootComplete`, `EnterSidePhase`, `CompleteSidePhase`, `ReturnHome`, `NotifyMatchResolved`, `CancelMatchmaking`, etc.).
- Phase graph semantics from Phase 1 unchanged (Intro+Countdown required; side-phase return Boot/Splash → Home upgrade; chaining preserves `_sideReturnPhase`).
- Out of scope: combat/maps, MatchContext FindObjectOfType, EOS features, untracked WIP, `gh` PR.
- Commit trailer: `Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>`
- Commit style: `type(scope): description` (feat/fix/refactor/test/docs/chore).
- After each task: `dotnet build RecipeRage.Gameplay.csproj -nologo` (or Core + EditMode) must be 0 errors. Unity NUnit is not discovered by `dotnet test` — build is the CI gate; note Unity Test Runner for manual.
- Grep success (Task 9): zero product hits for `IGameStateManager|GameStateManager|ChangeState<` under `Assets/_KitchenClash` and `Assets/Scripts` (except this plan/spec docs outside Assets).
- Prefer surgical ports: keep Splash/Intro/Countdown/Analytics as-is unless they reference SM.
- Create Unity `.meta` files for every new `.cs` (copy GUID style from sibling; unique GUID).

## File map

| Path | Responsibility |
|------|----------------|
| `Assets/Playcenter/GameFlow/Runtime/Ports/IFlowPorts.cs` | Add `ISidePhasePort` |
| `Assets/Playcenter/GameFlow/Runtime/Core/AppFlowController.cs` | Side-phase Enter/Exit dispatch; ctor param |
| `Assets/_KitchenClash/Infrastructure/Flow/Handlers/SessionLoader.cs` | Create session + economy/player init |
| `Assets/_KitchenClash/Infrastructure/Flow/Handlers/BootSequence.cs` | NTP/config/force/maint/auth + session load |
| `Assets/_KitchenClash/Infrastructure/Flow/Handlers/HomePhase.cs` | Menu scene + music |
| `Assets/_KitchenClash/Infrastructure/Flow/Handlers/MatchmakingPhase.cs` | Queue + timeout tick |
| `Assets/_KitchenClash/Infrastructure/Flow/Handlers/MatchRuntimePhase.cs` | Map load + StartRound gate |
| `Assets/_KitchenClash/Infrastructure/Flow/Handlers/ResultsPhase.cs` | Match-end music/SFX/rewards |
| `Assets/_KitchenClash/Infrastructure/Flow/Handlers/LoginPhase.cs` | Login UI + events → SessionLoader → CompleteSidePhase |
| `Assets/_KitchenClash/Infrastructure/Flow/Handlers/MaintenancePhase.cs` | Maintenance UI + poll → Login side phase |
| `Assets/_KitchenClash/Infrastructure/Flow/Handlers/NoConnectionPhase.cs` | No-connection UI → CompleteSidePhase / retry |
| `Assets/_KitchenClash/Infrastructure/Flow/Handlers/TutorialPhase.cs` | Tutorial scene → CompleteSidePhase |
| `Assets/_KitchenClash/Infrastructure/Flow/Handlers/AccountUpgradePhase.cs` | Upgrade UI → ReturnHome |
| `Assets/_KitchenClash/Infrastructure/Flow/SidePhaseFlowPort.cs` | Maps FlowPhaseId → handlers |
| `Assets/_KitchenClash/Infrastructure/Flow/BootFlowPort.cs` | Drive BootSequence |
| `Assets/_KitchenClash/Infrastructure/Flow/StateMachineFlowPorts.cs` | Home/MM/Match/Results ports → handlers (rename optional) |
| `Assets/_KitchenClash/Composition/RootLifetimeScope.cs` | Register handlers; drop SM |
| `Assets/_KitchenClash/Composition/GameBootstrapper.cs` | IAppFlow only |
| `Assets/_KitchenClash/Infrastructure/DI/SessionManager.cs` | Drop SM param |
| `Assets/_KitchenClash/Infrastructure/Network/*` | Drop SM |
| Delete | All `Application/State/*`, `GameStateManager`, `GameStateFactory`, `Infrastructure/States/*` |
| Tests | `AppFlowControllerTests` side-phase; handler unit tests as needed |
| Docs | `wiki/GameFlow-SDK.md`, `wiki/Technical.md`, `CLAUDE.md` |

---

### Task 1: ISidePhasePort + AppFlowController dispatch

**Files:**
- Modify: `Assets/Playcenter/GameFlow/Runtime/Ports/IFlowPorts.cs`
- Modify: `Assets/Playcenter/GameFlow/Runtime/Core/AppFlowController.cs`
- Modify: `Assets/Scripts/Tests/EditMode/Gameplay/AppFlowControllerTests.cs`
- Modify: any other `new AppFlowController(` call sites if compile breaks (add optional param — default null)

**Interfaces:**
- Produces: `ISidePhasePort` with `void EnterSidePhase(FlowPhaseId phase, FlowContext context)` and `void ExitSidePhase(FlowPhaseId phase)`
- Produces: `AppFlowController(..., ISidePhasePort sidePhases = null)` last optional param after analytics (or before analytics — **use last param after analytics** for minimal churn)

- [ ] **Step 1: Write the failing test**

Append to `AppFlowControllerTests.cs`:

```csharp
private sealed class RecordingSidePhases : ISidePhasePort
{
    public FlowPhaseId LastEnter = FlowPhaseId.None;
    public FlowPhaseId LastExit = FlowPhaseId.None;
    public int EnterCount;
    public int ExitCount;

    public void EnterSidePhase(FlowPhaseId phase, FlowContext context)
    {
        EnterCount++;
        LastEnter = phase;
    }

    public void ExitSidePhase(FlowPhaseId phase)
    {
        ExitCount++;
        LastExit = phase;
    }
}

[Test]
public void EnterSidePhase_DispatchesSidePhasePort_AndCompleteReturnsHome()
{
    var home = new RecordingHome();
    var sides = new RecordingSidePhases();
    var flow = new AppFlowController(home: home, sidePhases: sides);
    flow.StartColdBoot();
    flow.ReturnHome();
    Assert.AreEqual(FlowPhaseId.Home, flow.Current);

    flow.EnterSidePhase(FlowPhaseId.Login);
    Assert.AreEqual(FlowPhaseId.Login, flow.Current);
    Assert.AreEqual(1, sides.EnterCount);
    Assert.AreEqual(FlowPhaseId.Login, sides.LastEnter);

    flow.CompleteSidePhase();
    Assert.AreEqual(FlowPhaseId.Home, flow.Current);
    Assert.AreEqual(1, sides.ExitCount);
    Assert.AreEqual(FlowPhaseId.Login, sides.LastExit);
    Assert.GreaterOrEqual(home.EnterCount, 2);
}

[Test]
public void EnterSidePhase_Chained_PreservesReturnAndExitsPrevious()
{
    var home = new RecordingHome();
    var sides = new RecordingSidePhases();
    var flow = new AppFlowController(home: home, sidePhases: sides);
    flow.StartColdBoot();
    // Stay on Boot if possible: StartColdBoot → Splash → (null splash auto Boot)
    // Force Boot then side:
    flow.ReturnHome();
    flow.EnterSidePhase(FlowPhaseId.Maintenance);
    Assert.AreEqual(FlowPhaseId.Maintenance, sides.LastEnter);

    flow.EnterSidePhase(FlowPhaseId.Login);
    Assert.AreEqual(FlowPhaseId.Login, flow.Current);
    Assert.AreEqual(FlowPhaseId.Maintenance, sides.LastExit);
    Assert.AreEqual(FlowPhaseId.Login, sides.LastEnter);

    flow.CompleteSidePhase();
    Assert.AreEqual(FlowPhaseId.Home, flow.Current);
}
```

- [ ] **Step 2: Run build (tests compile-check)**

```bash
dotnet build RecipeRage.Tests.EditMode.csproj -nologo
```

Expected: FAIL — `ISidePhasePort` / `sidePhases` named arg missing.

- [ ] **Step 3: Add interface**

In `IFlowPorts.cs` after `IFlowAnalyticsPort`:

```csharp
/// <summary>Side phases: Login, Maintenance, ForceUpdate, Tutorial, etc.</summary>
public interface ISidePhasePort
{
    void EnterSidePhase(FlowPhaseId phase, FlowContext context);
    void ExitSidePhase(FlowPhaseId phase);
}
```

- [ ] **Step 4: Wire AppFlowController**

Add field `private readonly ISidePhasePort _sidePhases;`

Ctor: add `ISidePhasePort sidePhases = null` as **last** parameter; assign `_sidePhases = sidePhases;`

In `EnterPhase`, after the main switch (or default), add before closing:

```csharp
// After existing switch cases, add:
case FlowPhaseId.ForceUpdate:
case FlowPhaseId.Maintenance:
case FlowPhaseId.NoConnection:
case FlowPhaseId.Login:
case FlowPhaseId.Tutorial:
case FlowPhaseId.AccountUpgrade:
    _sidePhases?.EnterSidePhase(phase, _context);
    break;
```

In `ExitPhase`, same cases:

```csharp
case FlowPhaseId.ForceUpdate:
case FlowPhaseId.Maintenance:
case FlowPhaseId.NoConnection:
case FlowPhaseId.Login:
case FlowPhaseId.Tutorial:
case FlowPhaseId.AccountUpgrade:
    _sidePhases?.ExitSidePhase(phase);
    break;
```

- [ ] **Step 5: Build green**

```bash
dotnet build RecipeRage.Tests.EditMode.csproj -nologo
```

Expected: 0 errors.

- [ ] **Step 6: Commit**

```bash
git add Assets/Playcenter/GameFlow/Runtime/Ports/IFlowPorts.cs \
  Assets/Playcenter/GameFlow/Runtime/Core/AppFlowController.cs \
  Assets/Scripts/Tests/EditMode/Gameplay/AppFlowControllerTests.cs
git commit -m "$(cat <<'EOF'
feat(gameflow): dispatch side phases through ISidePhasePort

AppFlowController Enter/Exit now invoke ISidePhasePort for Login,
Maintenance, and other side phases.

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

### Task 2: SessionLoader + BootSequence + BootFlowPort

**Files:**
- Create: `Assets/_KitchenClash/Infrastructure/Flow/Handlers/SessionLoader.cs` (+ .meta)
- Create: `Assets/_KitchenClash/Infrastructure/Flow/Handlers/BootSequence.cs` (+ .meta)
- Modify: `Assets/_KitchenClash/Infrastructure/Flow/BootFlowPort.cs`
- Create: `Assets/Scripts/Tests/EditMode/Gameplay/BootSequenceTests.cs` (+ .meta) — pure logic with fakes where possible
- Modify: `Assets/_KitchenClash/Composition/RootLifetimeScope.cs` — register SessionLoader/BootSequence; BootFlowPort uses them (still may keep SM until Task 8 for other ports)

**Interfaces:**
- Produces:
  - `SessionLoader.LoadAsync(CancellationToken ct)` → creates session if needed, inits economy/player, short delay
  - `BootSequence.RunAsync(CancellationToken ct)` → boot pipeline; calls IAppFlow for side phases / NotifyBootComplete
  - `BootFlowPort(BootSequence sequence)` — no IGameStateManager

**Behavior (port from BootstrapState + SessionLoadingState):**

`SessionLoader`:
```csharp
// Namespace: KitchenClash.Infrastructure.Flow.Handlers
public sealed class SessionLoader
{
    private readonly SessionManager _sessionManager;
    private readonly ISessionContext _sessionContext;

    public SessionLoader(SessionManager sessionManager, ISessionContext sessionContext)
    {
        _sessionManager = sessionManager;
        _sessionContext = sessionContext;
    }

    public async UniTask LoadAsync(CancellationToken ct = default)
    {
        if (!_sessionManager.IsSessionActive)
        {
            _sessionManager.CreateSession();
        }
        ct.ThrowIfCancellationRequested();

        _sessionContext.EconomyService?.Initialize();
        ct.ThrowIfCancellationRequested();

        _sessionContext.PlayerDataService?.Initialize();
        ct.ThrowIfCancellationRequested();

        await UniTask.Delay(300, cancellationToken: ct);
    }
}
```

`BootSequence` (inject: INTPTimeService, IRemoteConfigService, IAuthService, IMaintenanceService, ForceUpdateChecker, IAppFlow, SessionLoader — same deps as BootstrapState minus IGameStateManager/IUIService if unused):

```csharp
public sealed class BootSequence
{
    // fields...
    private CancellationTokenSource _cts;

    public void Start()
    {
        Cancel();
        _cts = new CancellationTokenSource();
        RunAsync(_cts.Token).Forget();
    }

    public void Cancel()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }

    private async UniTask RunAsync(CancellationToken ct)
    {
        try
        {
            // 1 NTP best-effort (5s) — copy BootstrapState
            // 2 remote config Initialize + RefreshConfig
            // 3 force update → if required: _appFlow.EnterSidePhase(ForceUpdate); return;
            // 4 maintenance → if active: EnterSidePhase(Maintenance); return;
            // 5 auth empty → EnterSidePhase(Login); return;
            // 6 await _sessionLoader.LoadAsync(ct);
            // 7 _appFlow.NotifyBootComplete();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            GameLogger.LogException(ex);
            _appFlow?.EnterSidePhase(FlowPhaseId.Login);
        }
    }
}
```

**Critical:** Do **not** call both CompleteSidePhase and NotifyBootComplete from BootSequence. Authenticated cold boot only `NotifyBootComplete`. Login path is Task 3.

`BootFlowPort`:
```csharp
public sealed class BootFlowPort : IBootPort
{
    private readonly BootSequence _bootSequence;

    public BootFlowPort(BootSequence bootSequence)
    {
        _bootSequence = bootSequence;
    }

    public void EnterBoot(FlowContext context)
    {
        _ = context;
        _bootSequence.Start();
    }

    public void ExitBoot()
    {
        _bootSequence.Cancel();
    }
}
```

- [ ] **Step 1: Implement SessionLoader + BootSequence + BootFlowPort** (TDD: if pure fakes are heavy due to Unity services, implement first then add a focused test that BootSequence calls EnterSidePhase(Login) when auth empty — use hand-rolled fakes for IAuthService/IAppFlow).

Minimal test with FakeAppFlow + stub auth:

```csharp
[Test]
public void BootSequence_WhenNotAuthenticated_EntersLoginSidePhase()
{
    // Arrange fakes: auth ProductUserId null/empty; remote config no-op; force update false; maintenance false
    // Act: sequence.Start(); wait UniTask with PlayerLoopTiming or run sync if you extract RunAsync public for test
    // Assert: fakeAppFlow.LastEnterSidePhase == Login; NotifyBootComplete not called
}
```

If UniTask.Forget makes EditMode hard, expose `internal UniTask RunAsyncForTests(CancellationToken ct)` or make `RunAsync` public for tests via `InternalsVisibleTo` — prefer **public `RunAsync`** used by Start, tests call `await sequence.RunAsync(...)` if test runner supports async; otherwise use `.GetAwaiter().GetResult()` only if no Unity sync context issues. Prefer:

```csharp
public UniTask RunAsync(CancellationToken ct) { ... }
public void Start() { Cancel(); _cts = new(); RunAsync(_cts.Token).Forget(); }
```

- [ ] **Step 2: Wire RootLifetimeScope RegisterAppFlow**

```csharp
var sessionLoader = new SessionLoader(
    resolver.Resolve<SessionManager>(),
    resolver.Resolve<ISessionContext>());
// Prefer Register<> singletons instead of new in factory when possible:

// In RegisterInfrastructure or new RegisterFlowHandlers:
builder.Register<SessionLoader>(Lifetime.Singleton);
builder.Register<BootSequence>(Lifetime.Singleton);

// In RegisterAppFlow factory:
boot: new BootFlowPort(resolver.Resolve<BootSequence>()),
// sidePhases: still null until Task 3 — OK
```

Ensure `BootSequence` ctor can resolve `IAppFlow` via `AppFlowProxy` to break cycle:

```csharp
// BootSequence should take Func<IAppFlow> or IAppFlow from proxy:
// Register:
builder.Register<BootSequence>(Lifetime.Singleton);
// BootSequence(IAppFlow appFlow, ...) — AppFlowProxy already registered? 
// Pattern: same as ports — construct BootSequence inside RegisterAppFlow factory with appFlowProxy.
```

**Preferred cycle break (match existing):** construct `BootSequence` inside the `RegisterAppFlow` factory with `appFlowProxy`, **or** register BootSequence with `AppFlowProxy` injected. Do **not** resolve `IAppFlow` from BootSequence before flow is assigned.

Simplest: construct in factory:

```csharp
var sessionLoader = new SessionLoader(
    resolver.Resolve<SessionManager>(),
    resolver.Resolve<ISessionContext>());
var bootSequence = new BootSequence(
    resolver.Resolve<INTPTimeService>(),
    resolver.Resolve<IRemoteConfigService>(),
    resolver.Resolve<IAuthService>(),
    resolver.Resolve<IMaintenanceService>(),
    resolver.Resolve<ForceUpdateChecker>(),
    appFlowProxy,
    sessionLoader);
// store bootSequence if needed elsewhere — or RegisterInstance after create
flow = new AppFlowController(
    splash: new SplashFlowPort(appFlowProxy),
    boot: new BootFlowPort(bootSequence),
    ...
);
```

If `ForceUpdateChecker` / `INTPTimeService` resolution fails, match BootstrapState ctor exactly from DI.

- [ ] **Step 3: Build**

```bash
dotnet build RecipeRage.Gameplay.csproj -nologo
dotnet build RecipeRage.Tests.EditMode.csproj -nologo
```

Expected: 0 errors. BootstrapState may still exist unused by Boot port.

- [ ] **Step 4: Commit**

```bash
git add Assets/_KitchenClash/Infrastructure/Flow/Handlers \
  Assets/_KitchenClash/Infrastructure/Flow/BootFlowPort.cs \
  Assets/_KitchenClash/Composition/RootLifetimeScope.cs \
  Assets/Scripts/Tests/EditMode/Gameplay/BootSequenceTests.cs \
  Assets/Scripts/Tests/EditMode/Gameplay/BootSequenceTests.cs.meta
git commit -m "$(cat <<'EOF'
feat(gameflow): BootSequence + SessionLoader replace BootstrapState port path

BootFlowPort drives BootSequence; authenticated boot ends with NotifyBootComplete only.

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

### Task 3: Side-phase handlers + SidePhaseFlowPort

**Files:**
- Create handlers under `Assets/_KitchenClash/Infrastructure/Flow/Handlers/`:
  - `LoginPhase.cs`, `MaintenancePhase.cs`, `NoConnectionPhase.cs`, `TutorialPhase.cs`, `AccountUpgradePhase.cs`
  - Optional thin `ForceUpdatePhase.cs` (no-op enter if UI is event-driven; Exit no-op)
- Create: `Assets/_KitchenClash/Infrastructure/Flow/SidePhaseFlowPort.cs` (+ .meta)
- Modify: `RootLifetimeScope.RegisterAppFlow` — pass `sidePhases: new SidePhaseFlowPort(...)`

**Interfaces:**
- Consumes: `SessionLoader`, `IAppFlow` (proxy), UI/event services from old states
- Produces: `SidePhaseFlowPort : ISidePhasePort`

**LoginPhase** (from LoginState — **no ChangeState**):

```csharp
// On LoginSuccess:
// await _sessionLoader.LoadAsync(ct);
// _appFlow.CompleteSidePhase();  // NOT NotifyBootComplete
```

**MaintenancePhase:** port MaintenanceState; when maintenance ends → `_appFlow.EnterSidePhase(FlowPhaseId.Login)` only (no ChangeState).

**NoConnectionPhase:** on retry success path → `_appFlow.CompleteSidePhase()` (old code also ChangeState SessionLoading — **replace** with SessionLoader + CompleteSidePhase, or CompleteSidePhase only if session already loaded; prefer: `await SessionLoader.LoadAsync` then `CompleteSidePhase`).

**TutorialPhase:** on complete → SessionLoader optional + `CompleteSidePhase`.

**AccountUpgradePhase:** on done → `_appFlow.ReturnHome()` only.

**SidePhaseFlowPort:**

```csharp
public sealed class SidePhaseFlowPort : ISidePhasePort
{
    private readonly LoginPhase _login;
    // ... other phases
    private FlowPhaseId _active = FlowPhaseId.None;

    public void EnterSidePhase(FlowPhaseId phase, FlowContext context)
    {
        switch (phase)
        {
            case FlowPhaseId.Login: _login.Enter(); break;
            case FlowPhaseId.Maintenance: _maintenance.Enter(); break;
            // ...
            case FlowPhaseId.ForceUpdate: break; // event UI already shown
            default:
                GameLogger.LogWarning($"[SidePhaseFlowPort] Unhandled side phase {phase}");
                break;
        }
        _active = phase;
    }

    public void ExitSidePhase(FlowPhaseId phase)
    {
        switch (phase)
        {
            case FlowPhaseId.Login: _login.Exit(); break;
            // ...
        }
        if (_active == phase) _active = FlowPhaseId.None;
    }
}
```

Port Enter/Exit bodies from existing state files line-for-line except navigation.

- [ ] **Step 1: Implement handlers + SidePhaseFlowPort**
- [ ] **Step 2: Wire into AppFlowController construction**
- [ ] **Step 3: Build 0 errors**
- [ ] **Step 4: Commit**

```bash
git commit -m "$(cat <<'EOF'
feat(gameflow): SidePhaseFlowPort + phase handlers (no ChangeState)

Login/Maintenance/etc. enter via ISidePhasePort; Login success uses
SessionLoader + CompleteSidePhase.

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

---

### Task 4: HomePhase + HomeFlowPort

**Files:**
- Create: `Handlers/HomePhase.cs` (+ .meta)
- Modify: `StateMachineFlowPorts.cs` `HomeFlowPort` — inject `HomePhase`, no SM

**HomePhase:** copy MainMenuState Enter/Exit (music + load MainMenu scene + delay). Use `CancellationTokenSource` instead of `IsStateActive`.

```csharp
public sealed class HomeFlowPort : IHomePort
{
    private readonly HomePhase _home;
    public HomeFlowPort(HomePhase home) => _home = home;
    public void EnterHome(FlowContext context) { _ = context; _home.Enter(); }
    public void ExitHome() => _home.Exit();
}
```

Also show HomeScreen if MainMenuState did not — check Presentation: if HomeScreen shown elsewhere, keep parity. Current MainMenuState does **not** show HomeScreen in the snippet; HomeFlowPort comment said MainMenuState shows it via Type.GetType — verify and preserve whatever is real in code at implement time.

- [ ] Implement, wire Root factory `home: new HomeFlowPort(homePhase)`, build, commit:

```bash
git commit -m "feat(gameflow): HomePhase owns menu scene enter/exit

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 5: MatchmakingPhase (ITickable) + MatchmakingFlowPort

**Files:**
- Create: `Handlers/MatchmakingPhase.cs` (+ .meta)
- Modify: `MatchmakingFlowPort` in `StateMachineFlowPorts.cs`
- Modify: `RootLifetimeScope` — `builder.Register<MatchmakingPhase>(Lifetime.Singleton).AsSelf().As<ITickable>();` **or** register entry tick via existing ITickable pattern

**MatchmakingPhase:** port MatchmakingState fully:
- `SetQueueParameters` / apply from Enter(PlayRequest)
- Enter: music, subscribe events, show screen, maintenance check, FindMatch
- `Tick(float dt)` or `void Update()` called from `ITickable.Tick()` — timeout → FillMatchWithBots
- Exit: unsubscribe, cancel search, hide screen (port ExitMatchmaking already hides — keep idempotent)
- Events → IAppFlow only

```csharp
public sealed class MatchmakingPhase : ITickable
{
    private bool _active;
    // ...
    public void Enter(PlayRequest request, FlowContext context) { ... }
    public void Exit() { ... }
    public void Tick()
    {
        if (!_active || !_isMatchmakingInProgress) return;
        // timeout bot fill — use Time.time as today
    }
}
```

VContainer `ITickable` uses `void Tick()` — match project’s GameStateManager ITickable signature:

```csharp
// Check VContainer.Unity.ITickable — typically void Tick()
```

`MatchmakingFlowPort`:
```csharp
public void EnterMatchmaking(FlowContext context, PlayRequest request) => _phase.Enter(request, context);
public void ExitMatchmaking() { /* hide UI */ _phase.Exit(); }
public void Cancel() { /* optional; Exit cancels */ }
```

- [ ] Implement, wire, build, commit:

```bash
git commit -m "feat(gameflow): MatchmakingPhase ITickable replaces MatchmakingState

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 6: MatchRuntimePhase + MatchRuntimeFlowPort

**Files:**
- Create: `Handlers/MatchRuntimePhase.cs`
- Modify: `MatchRuntimeFlowPort`

**MatchRuntimePhase:** port GameplayState:
- `Enter()` — music, load Game scene + map, set `_sceneLoadComplete`, honor pending start
- `RequestStartRound()` — gate identical to GameplayState
- `Exit()` — UnloadCurrentMapAsync
- Idempotent re-Enter: if already entered and scene loaded, only apply pending start (port already special-cases)

```csharp
public sealed class MatchRuntimeFlowPort : IMatchRuntimePort
{
    private readonly MatchRuntimePhase _phase;
    private bool _pendingStartRound;

    public void EnterMatch(FlowContext context)
    {
        _ = context;
        _phase.Enter();
        if (_pendingStartRound)
        {
            _pendingStartRound = false;
            _phase.RequestStartRound();
        }
    }

    public void StartRound(FlowContext context)
    {
        _ = context;
        if (_phase.IsEntered)
        {
            _phase.RequestStartRound();
            return;
        }
        _pendingStartRound = true;
    }

    public void ExitMatch()
    {
        _pendingStartRound = false;
        _phase.Exit();
    }
}
```

- [ ] Implement, wire, build, commit:

```bash
git commit -m "feat(gameflow): MatchRuntimePhase owns map load and StartRound gate

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 7: ResultsPhase + ResultsFlowPort

**Files:**
- Create: `Handlers/ResultsPhase.cs`
- Modify: `ResultsFlowPort`

**ResultsPhase.Enter:** GameOverState body (Refresh, music, SFX, AwardMatchReward).  
**ResultsFlowPort:** call phase then show ResultsScreen (already in port). Exit: hide screen + phase.Exit.

- [ ] Implement, wire, build, commit:

```bash
git commit -m "feat(gameflow): ResultsPhase owns match-end rewards and audio

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 8: Strip SM from DI and consumers

**Files:**
- Modify: `GameBootstrapper.cs` — only `IAppFlow`
- Modify: `SessionManager.cs` — remove `_stateManager` field/ctor param
- Modify: `NetworkingServiceContainer.cs` — remove SM; GameStarter ctor without SM
- Modify: `GameStarter.cs` — remove `_stateManager` and `ChangeState<MainMenuState>` fallback; if `_appFlow == null` log error and return
- Modify: `RootLifetimeScope.cs`:
  - Remove `GameStateFactory` / `GameStateManager` registration
  - Remove `RegisterGameStates` call and method
  - Ensure no port still needs SM
- Modify: any remaining ctor that required SM for states still registered — if states still exist until Task 9, they may break DI scan. **Order:** either delete state registration first and leave state files unregistered (orphaned) then Task 9 deletes files, **or** combine 8+9. Prefer: Task 8 stops registering states/SM; Task 9 deletes files. Orphaned state `.cs` files still compile if they reference IGameStateManager — **keep Application.State until Task 9** OR update states to not compile... States will still compile as long as Application.State exists. Unregistered is fine.

- [ ] **Step 1: Update consumers**
- [ ] **Step 2: Update RegisterAppFlow** — zero `Resolve<IGameStateManager>()`
- [ ] **Step 3: Build**

```bash
dotnet build RecipeRage.Gameplay.csproj -nologo
dotnet build RecipeRage.Tests.EditMode.csproj -nologo
```

Fix test fakes that construct GameStarter with SM.

- [ ] **Step 4: Commit**

```bash
git commit -m "refactor(gameflow): remove IGameStateManager from DI and consumers

GameBootstrapper, SessionManager, Networking, GameStarter use IAppFlow only.

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 9: Delete SM framework + state workers + grep gates

**Files to delete (and their .meta):**
- `Assets/_KitchenClash/Application/State/IGameStateManager.cs`
- `Assets/_KitchenClash/Application/State/IState.cs`
- `Assets/_KitchenClash/Application/State/IStateFactory.cs`
- `Assets/_KitchenClash/Application/State/BaseState.cs`
- `Assets/_KitchenClash/Application/State/StateMachine.cs`
- `Assets/_KitchenClash/Infrastructure/DI/GameStateManager.cs`
- `Assets/_KitchenClash/Infrastructure/DI/GameStateFactory.cs`
- All `Assets/_KitchenClash/Infrastructure/States/*.cs` (+ .meta)

**Also:**
- Fix any remaining usings / references
- Update tests that referenced states
- Run grep gates

```bash
rg -n "IGameStateManager|GameStateManager|IStateFactory|ChangeState<" Assets/_KitchenClash Assets/Scripts --glob '*.cs'
rg -n "class \w+State\b" Assets/_KitchenClash/Infrastructure --glob '*.cs'
```

Expected: **no matches** (or only comments in non-code — prefer zero).

```bash
dotnet build RecipeRage.Core.csproj -nologo
dotnet build RecipeRage.Gameplay.csproj -nologo
dotnet build RecipeRage.Tests.EditMode.csproj -nologo
```

Expected: 0 errors.

- [ ] Commit:

```bash
git commit -m "refactor(gameflow): delete IGameStateManager and all phase workers

Ports and handlers own shell lifecycle; AppFlow is sole phase owner.

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

---

### Task 10: Wiki + CLAUDE + final verification

**Files:**
- Modify: `wiki/GameFlow-SDK.md` — remove worker/IGameStateManager guidance; document handlers + ISidePhasePort
- Modify: `wiki/Technical.md` — DI table without GameStateManager; note handlers
- Modify: `CLAUDE.md` — state flow section → IAppFlow phases; remove “Adding a new game state” or replace with “Adding a phase handler”
- Modify: `.superpowers/sdd/progress.md` — Phase 2 tasks complete
- Append: `wiki/log.md` updated note

- [ ] **Step 1: Doc updates** (accurate to code)
- [ ] **Step 2: Final grep + build**
- [ ] **Step 3: Commit**

```bash
git commit -m "docs(gameflow): wiki and CLAUDE for hard-purge architecture

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

- [ ] **Step 4: Push** (if network allows)

```bash
git push -u origin architecture-cleanup
```

---

## Self-review (plan vs spec)

| Spec section | Task |
|--------------|------|
| §3 Approach C handlers | 2–7 |
| §5 ISidePhasePort | 1, 3 |
| §6 Boot pipeline | 2 |
| §7 Main-path ports | 4–7 |
| §8 Consumer cleanup | 8 |
| §9 File layout | 2–7 creates |
| §10 Testing | 1 tests; 2 BootSequence test; build gates |
| §11 Delete gates | 9–10 |
| §12 Order | Tasks 1→10 |
| ForceUpdate side enter | 2 BootSequence + 3 port |
| Matchmaking ITickable | 5 |
| Login CompleteSidePhase only | 3 |
| No dual Complete+Notify on login | 2–3 |

**Placeholder scan:** none intentional.  
**Type consistency:** `ISidePhasePort.EnterSidePhase(FlowPhaseId, FlowContext)` / `ExitSidePhase(FlowPhaseId)` used in Tasks 1 and 3.

---

## Execution

Autopilot / product intent: execute with **subagent-driven-development** immediately after this plan is committed. Do not wait for a second human approval of the plan text unless a task is BLOCKED.
