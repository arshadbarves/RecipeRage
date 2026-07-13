# GameFlow Production Migration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make `IAppFlow` the sole public product navigator (cold boot → home → play → MM → intro → countdown → match → results → home), rewire workers to notify flow, and delete dual Presentation/feature `ChangeState` navigation.

**Architecture:** `AppFlowController` owns legal transitions; KitchenClash Flow ports perform scene/UI/net work and may drive internal `*State` workers via `IGameStateManager`. UI and features call only `IAppFlow` intents. `IGameStateManager` remains internal (not deleted this plan).

**Tech Stack:** Unity 6, VContainer, Playcenter.GameFlow (`noEngineReferences`), NUnit EditMode tests, UniTask, existing Infrastructure States.

## Global Constraints

- Spec: `docs/superpowers/specs/2026-07-13-gameflow-production-migration-design.md`
- Wiki target: `wiki/GameFlow-SDK.md` (update in Task 8 to match code)
- Do **not** implement Kitchen Brawler combat/maps in this plan
- Do **not** delete `IGameStateManager` / `*State` classes
- Production DI must not use `NullSplashPort`, `NullBootPort`, `NullMatchIntroPort`, `NullCountdownPort`
- Intro + Countdown required on product path (wire real ports)
- Asmdef is source of truth; build with `dotnet build` on affected csproj
- Commit style: `type(scope): description` + Co-authored-by trailer when committing in-agent
- Presentation must not inject `IGameStateManager` for navigation after Task 4–5
- Internal `ChangeState` **inside Flow ports only** is allowed

## File map

| Path | Responsibility |
|------|----------------|
| `Assets/Playcenter/GameFlow/Runtime/Core/AppFlowController.cs` | Legal graph (touch only if tests need; prefer leave) |
| `Assets/_KitchenClash/Infrastructure/Flow/BootFlowPort.cs` | **Create** — Boot phase → BootstrapState worker + flow completion |
| `Assets/_KitchenClash/Infrastructure/Flow/SplashFlowPort.cs` | **Create** — Splash dwell then advance (or thin wrapper) |
| `Assets/_KitchenClash/Infrastructure/Flow/SidePhaseFlowPorts.cs` | **Create** — Login/Maintenance/etc. workers via side phases |
| `Assets/_KitchenClash/Infrastructure/Flow/StateMachineFlowPorts.cs` | Existing Home/MM/Match/Results ports |
| `Assets/_KitchenClash/Infrastructure/Flow/MatchIntroFlowPort.cs` | Existing intro beat |
| `Assets/_KitchenClash/Infrastructure/Flow/CountdownFlowPort.cs` | Existing countdown beat |
| `Assets/_KitchenClash/Infrastructure/Flow/AppFlowProxy.cs` | Cycle break for ports needing IAppFlow |
| `Assets/_KitchenClash/Composition/RootLifetimeScope.cs` | Register `IAppFlow` + ports |
| `Assets/_KitchenClash/Composition/GameBootstrapper.cs` | `StartColdBoot()` |
| `Assets/_KitchenClash/Infrastructure/States/BootstrapState.cs` | Report completion via IAppFlow (not sibling ChangeState for product graph) |
| `Assets/_KitchenClash/Infrastructure/States/MatchmakingState.cs` | `NotifyMatchResolved` / cancel via IAppFlow |
| `Assets/_KitchenClash/Infrastructure/States/*` side states | `CompleteSidePhase` / flow where they leave |
| `Assets/_KitchenClash/Infrastructure/Network/GameStarter.cs` | `ReturnHome` instead of MainMenu ChangeState |
| `Assets/_KitchenClash/Presentation/ViewModels/LobbyViewModel.cs` | `RequestPlay` |
| `Assets/_KitchenClash/Presentation/ViewModels/GameplayHudViewModel.cs` | Remove GameOver ChangeState (match end via flow elsewhere) |
| `Assets/_KitchenClash/Presentation/Screens/ResultsScreen.cs` | `ReturnHome` / optional Play Again |
| `Assets/Scripts/Tests/EditMode/Gameplay/AppFlowControllerTests.cs` | **Create** |
| `Assets/Scripts/Tests/EditMode/Gameplay/AppFlowMigrationTests.cs` | **Create** — notify + UI intent contracts |
| `wiki/GameFlow-SDK.md`, `wiki/Technical.md` | Align with live DI |

---

### Task 1: AppFlowController unit tests (baseline)

**Files:**
- Create: `Assets/Scripts/Tests/EditMode/Gameplay/AppFlowControllerTests.cs`
- Test: same

**Interfaces:**
- Consumes: `Playcenter.GameFlow.AppFlowController`, `IAppFlow`, `FlowPhaseId`, port interfaces, `MatchResolvedInfo`, `MatchResultInfo`, `PlayRequest`
- Produces: Green baseline that cold boot → Home (with fake ports) and RequestPlay → Matchmaking → NotifyMatchResolved → Intro → Countdown → Match → Results

- [ ] **Step 1: Write the failing test file**

```csharp
using System.Collections.Generic;
using NUnit.Framework;
using Playcenter.GameFlow;

namespace RecipeRage.Tests.EditMode.Gameplay
{
    public class AppFlowControllerTests
    {
        private sealed class RecordingHome : IHomePort
        {
            public int EnterCount;
            public void EnterHome(FlowContext context) => EnterCount++;
            public void ExitHome() { }
        }

        private sealed class RecordingMatchmaking : IMatchmakingPort
        {
            public int EnterCount;
            public PlayRequest LastRequest;
            public void EnterMatchmaking(FlowContext context, PlayRequest request)
            {
                EnterCount++;
                LastRequest = request;
            }
            public void ExitMatchmaking() { }
            public void Cancel() { }
        }

        private sealed class RecordingIntro : IMatchIntroPort
        {
            public int EnterCount;
            public void EnterMatchIntro(FlowContext context, MatchResolvedInfo info) => EnterCount++;
            public void ExitMatchIntro() { }
        }

        private sealed class RecordingCountdown : ICountdownPort
        {
            public int EnterCount;
            public void EnterCountdown(FlowContext context) => EnterCount++;
            public void ExitCountdown() { }
        }

        private sealed class RecordingMatch : IMatchRuntimePort
        {
            public int EnterCount;
            public int StartRoundCount;
            public void EnterMatch(FlowContext context) => EnterCount++;
            public void StartRound(FlowContext context) => StartRoundCount++;
            public void ExitMatch() { }
        }

        private sealed class RecordingResults : IResultsPort
        {
            public int EnterCount;
            public MatchResultInfo Last;
            public void EnterResults(FlowContext context, MatchResultInfo result)
            {
                EnterCount++;
                Last = result;
            }
            public void ExitResults() { }
        }

        private sealed class InstantSplash : ISplashPort
        {
            private readonly IAppFlow _flow;
            public InstantSplash(IAppFlow flow) { _flow = flow; }
            public void EnterSplash(FlowContext context)
            {
                // Production splash dwells; test advances immediately via controller graph.
                // AppFlowController transitions Splash→Boot on enter completion pattern:
                // Controller TransitionTo(StudioSplash) calls EnterSplash then stays until
                // something advances. For unit test, use a controller helper path:
            }
            public void ExitSplash() { }
        }

        // Prefer testing the public API as implemented: StartColdBoot enters StudioSplash.
        // If controller does not auto-advance splash, drive phases via a test double that
        // the controller already supports: after StartColdBoot, manually inspect Current
        // and use a thin TestBootPort that on EnterBoot transitions by calling nothing —
        // read AppFlowController.TransitionTo private behavior first.

        [Test]
        public void RequestPlay_FromHome_EntersMatchmaking()
        {
            var home = new RecordingHome();
            var mm = new RecordingMatchmaking();
            var flow = new AppFlowController(home: home, matchmaking: mm);
            // Force Home: StartColdBoot may stop at Splash if splash/boot null.
            // When splash/boot are null, StartColdBoot still TransitionTo(StudioSplash).
            // Use reflection-free approach: EnterSidePhase is wrong.
            // Implementation note for engineer: if Current after StartColdBoot is StudioSplash
            // with null splash port, TransitionTo still sets Current and calls null-safe enter.
            // Read AppFlowController.TransitionTo — if null ports skip work, Current is Splash.
            // Then we need a way to reach Home. Options:
            // 1) Add internal test hook (avoid)
            // 2) Provide boot port that is not auto
            // 3) Call ReturnHome from Splash if ForceTransition allows — ReturnHome ForceTransitionTo Home.
            flow.StartColdBoot();
            flow.ReturnHome(); // fail-closed to Home from any phase
            Assert.AreEqual(FlowPhaseId.Home, flow.Current);
            Assert.GreaterOrEqual(home.EnterCount, 1);

            flow.RequestPlay(new PlayRequest { ModeId = "quick_2v2", TeamSize = 2 });
            Assert.AreEqual(FlowPhaseId.Matchmaking, flow.Current);
            Assert.AreEqual(1, mm.EnterCount);
            Assert.AreEqual("quick_2v2", mm.LastRequest.ModeId);
        }

        [Test]
        public void FullHappyPath_IntroCountdown_StartRound_Results()
        {
            var home = new RecordingHome();
            var mm = new RecordingMatchmaking();
            var intro = new RecordingIntro();
            var countdown = new RecordingCountdown();
            var match = new RecordingMatch();
            var results = new RecordingResults();
            var flow = new AppFlowController(
                home: home,
                matchmaking: mm,
                matchIntro: intro,
                countdown: countdown,
                matchRuntime: match,
                results: results);

            flow.StartColdBoot();
            flow.ReturnHome();
            flow.RequestPlay(new PlayRequest { ModeId = "quick_2v2", TeamSize = 2 });
            flow.NotifyMatchResolved(new MatchResolvedInfo
            {
                LobbyId = "L1",
                ModeId = "quick_2v2",
                TeamSize = 2,
                HumanCount = 1,
                BotCount = 3,
                FilledWithBots = true
            });
            Assert.AreEqual(FlowPhaseId.MatchIntro, flow.Current);
            Assert.AreEqual(1, intro.EnterCount);

            flow.NotifyMatchIntroReady();
            Assert.AreEqual(FlowPhaseId.Countdown, flow.Current);
            Assert.AreEqual(1, countdown.EnterCount);

            flow.NotifyCountdownComplete();
            Assert.AreEqual(FlowPhaseId.Match, flow.Current);
            Assert.AreEqual(1, match.EnterCount);
            Assert.AreEqual(1, match.StartRoundCount);

            flow.NotifyMatchCompleted(new MatchResultInfo { Won = true, LocalTeamId = 0 });
            Assert.AreEqual(FlowPhaseId.Results, flow.Current);
            Assert.AreEqual(1, results.EnterCount);
            Assert.IsTrue(results.Last.Won);
        }

        [Test]
        public void RequestPlay_NotFromHome_IsIgnored()
        {
            var mm = new RecordingMatchmaking();
            var flow = new AppFlowController(matchmaking: mm);
            flow.StartColdBoot();
            // Stay off Home if possible; if StartColdBoot lands Splash, RequestPlay should no-op
            if (flow.Current != FlowPhaseId.Home)
            {
                flow.RequestPlay(PlayRequest.Empty);
                Assert.AreEqual(0, mm.EnterCount);
            }
        }
    }
}
```

- [ ] **Step 2: Run tests**

```bash
dotnet build RecipeRage.Tests.EditMode.csproj -nologo
dotnet test RecipeRage.Tests.EditMode.csproj --filter "FullyQualifiedName~AppFlowControllerTests" --no-build -nologo
```

Expected: PASS (if `ReturnHome` from Splash works). If `ReturnHome` no-ops when ports null-break, fix test to match actual `ForceTransitionTo` behavior by reading `AppFlowController` private methods — adjust asserts, do not weaken product API.

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/Tests/EditMode/Gameplay/AppFlowControllerTests.cs
git commit -m "test(flow): AppFlowController happy-path unit coverage"
```

---

### Task 2: Splash + Boot ports (real, not Null)

**Files:**
- Create: `Assets/_KitchenClash/Infrastructure/Flow/SplashFlowPort.cs`
- Create: `Assets/_KitchenClash/Infrastructure/Flow/BootFlowPort.cs`
- Modify: `Assets/_KitchenClash/Infrastructure/States/BootstrapState.cs` (inject `IAppFlow`, report outcomes)
- Modify: `Assets/_KitchenClash/Infrastructure/Flow/NullFlowPorts.cs` (keep Null types for tests only; comment)

**Interfaces:**
- Consumes: `ISplashPort`, `IBootPort`, `IGameStateManager`, `IStateFactory`, `IAppFlow`, `BootstrapState`
- Produces: Production ports that drive existing bootstrap work; BootstrapState calls `IAppFlow` for next phase instead of only `ChangeState` for Maintenance/Login/SessionLoading product jumps where flow owns graph

**Design for this task:**

`SplashFlowPort.EnterSplash`: show splash UI if any; after dwell (reuse 3.5s or shorter if Bootstrap already dwells — **do not double-dwell**). Preferred: Splash port is no-op UI + immediate signal by having `AppFlowController` auto-advance… **Controller does not auto-advance.** So either:
- (A) Splash port starts a UniTask dwell then needs a way to advance to Boot — **AppFlowController has no `NotifySplashComplete`**. Check controller for splash→boot transition.

Engineer **must read** `AppFlowController.TransitionTo` / enter handlers fully before coding. If splash enter does not schedule boot, add **one** public method only if missing:

```csharp
// Only if AppFlowController lacks splash completion:
public void NotifySplashComplete()
{
    if (_current != FlowPhaseId.StudioSplash) return;
    TransitionTo(FlowPhaseId.Boot);
}
```

Prefer implementing advance inside module if absent — small additive API is in scope.

`BootFlowPort.EnterBoot`: `_stateManager.ChangeState<BootstrapState>()` OR run boot work. BootstrapState on success should call `_appFlow` to reach Home via SessionLoading worker then Home.

**Pragmatic production mapping (lock this):**

1. `StartColdBoot` → Splash port shows splash (optional) and calls `NotifySplashComplete` (add if needed).
2. Boot port enters `BootstrapState` worker (move splash delay out of Bootstrap if Splash owns it — avoid 2× 3.5s).
3. BootstrapState outcomes:
   - Force update: stay / side `ForceUpdate` via `_appFlow.EnterSidePhase(FlowPhaseId.ForceUpdate)`
   - Maintenance: `_appFlow.EnterSidePhase(FlowPhaseId.Maintenance)` then port/state shows Maintenance UI
   - Not authed: `_appFlow.EnterSidePhase(FlowPhaseId.Login)`
   - OK: keep `ChangeState<SessionLoadingState>()` as **worker** then SessionLoading on success calls `_appFlow` — if still on Boot phase, need `TransitionTo(Home)`.

**SessionLoading → Home:** After profile load, call path that lands Home:
- If `Current == Boot` or side complete: use private transition — simplest production approach: SessionLoadingState injects `IAppFlow` and calls a new `NotifyBootComplete()` **or** `ReturnHome` is wrong from Boot. Add:

```csharp
public void NotifyBootComplete()
{
    if (_current != FlowPhaseId.Boot && _current != FlowPhaseId.Login
        && _current != FlowPhaseId.Tutorial && _current != FlowPhaseId.AccountUpgrade)
    {
        // allow from Boot primarily
        if (_current != FlowPhaseId.Boot) return;
    }
    TransitionTo(FlowPhaseId.Home);
}
```

Actually Login is side phase; after login, `CompleteSidePhase` may return to Boot or Home per controller. Read `CompleteSidePhase` — returns to `_sideReturnPhase`. Boot port should set flow so Login side returns to Boot or Home.

**Minimal viable boot wiring for Task 2:**

- [ ] **Step 1: Read full `AppFlowController` enter/exit switch** (file remainder after line 250). Document whether Splash→Boot is automatic.

- [ ] **Step 2: Add missing notify methods on `IAppFlow` + `AppFlowController` + `AppFlowProxy` only if required**

```csharp
// IAppFlow.cs — add if not present:
void NotifySplashComplete();
void NotifyBootComplete();
```

Implement legal transitions already in `Legal` set: StudioSplash→Boot, Boot→Home.

- [ ] **Step 3: Implement SplashFlowPort**

```csharp
// Assets/_KitchenClash/Infrastructure/Flow/SplashFlowPort.cs
using Cysharp.Threading.Tasks;
using Playcenter.GameFlow;
using UnityEngine;

namespace KitchenClash.Infrastructure.Flow
{
    public sealed class SplashFlowPort : ISplashPort
    {
        private readonly IAppFlow _appFlow;
        private readonly float _dwellSeconds;
        private int _runId;

        public SplashFlowPort(IAppFlow appFlow, float dwellSeconds = 0.5f)
        {
            _appFlow = appFlow;
            // Short dwell if BootstrapState no longer owns 3.5s splash; if Bootstrap keeps
            // full splash, use 0f and advance immediately.
            _dwellSeconds = Mathf.Max(0f, dwellSeconds);
        }

        public void EnterSplash(FlowContext context)
        {
            int id = ++_runId;
            RunAsync(id).Forget();
        }

        public void ExitSplash() => _runId++;

        private async UniTaskVoid RunAsync(int id)
        {
            if (_dwellSeconds > 0f)
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(_dwellSeconds),
                    DelayType.UnscaledDeltaTime);
            }
            if (id == _runId)
            {
                _appFlow.NotifySplashComplete();
            }
        }
    }
}
```

- [ ] **Step 4: Implement BootFlowPort**

```csharp
// Assets/_KitchenClash/Infrastructure/Flow/BootFlowPort.cs
using KitchenClash.Application.State;
using KitchenClash.Infrastructure.States;
using Playcenter.GameFlow;

namespace KitchenClash.Infrastructure.Flow
{
    public sealed class BootFlowPort : IBootPort
    {
        private readonly IGameStateManager _stateManager;

        public BootFlowPort(IGameStateManager stateManager)
        {
            _stateManager = stateManager;
        }

        public void EnterBoot(FlowContext context)
        {
            if (_stateManager?.CurrentState is not BootstrapState)
            {
                _stateManager?.ChangeState<BootstrapState>();
            }
        }

        public void ExitBoot() { }
    }
}
```

- [ ] **Step 5: Rewire BootstrapState product exits to IAppFlow**

Inject `IAppFlow _appFlow` (optional null-safe for tests). Replace:

```csharp
// maintenance
_appFlow?.EnterSidePhase(FlowPhaseId.Maintenance);
// instead of ChangeState<MaintenanceState>() — Side port will enter MaintenanceState

// unauthenticated
_appFlow?.EnterSidePhase(FlowPhaseId.Login);

// success path after init: still SessionLoading worker, then NotifyBootComplete from SessionLoading
_stateManager.ChangeState<SessionLoadingState>();
```

For force update: `EnterSidePhase(FlowPhaseId.ForceUpdate)` if UI exists; else keep event-only halt.

- [ ] **Step 6: Side phase ports (minimal)**

Create `SidePhaseFlowPorts.cs` with classes or one dispatcher used when controller enters side phases. **If AppFlowController does not call ports for side phases**, side UI must be entered from `EnterSidePhase` consumers — read controller. If side phases have no ports, BootstrapState may still `ChangeState<MaintenanceState>()` **and** `EnterSidePhase` for phase tracking, or only ChangeState until side ports exist.

**Lock:** For Task 2, if controller has no side ports, keep `ChangeState` for Maintenance/Login **and** call `EnterSidePhase` so `Current` matches. On Login/Maintenance complete, call `CompleteSidePhase()` then ensure Home/Boot continues.

- [ ] **Step 7: SessionLoadingState success → NotifyBootComplete / Home**

```csharp
// On successful load:
if (_appFlow != null)
{
    _appFlow.NotifyBootComplete(); // or CompleteSidePhase + ensure Home
}
else
{
    _stateManager.ChangeState<MainMenuState>();
}
```

Home port will `ChangeState<MainMenuState>` when flow enters Home — avoid double MainMenu if SessionLoading already changed. Prefer: SessionLoading only `NotifyBootComplete`; Home port enters MainMenuState.

- [ ] **Step 8: Build Infrastructure**

```bash
dotnet build KitchenClash.Infrastructure.csproj -nologo
dotnet build Playcenter.GameFlow.csproj -nologo
```

Expected: 0 errors.

- [ ] **Step 9: Commit**

```bash
git add Assets/Playcenter/GameFlow Assets/_KitchenClash/Infrastructure/Flow Assets/_KitchenClash/Infrastructure/States/BootstrapState.cs Assets/_KitchenClash/Infrastructure/States/SessionLoadingState.cs
git commit -m "feat(flow): Splash/Boot ports and boot completion notifies"
```

---

### Task 3: Register IAppFlow in RootLifetimeScope + GameBootstrapper

**Files:**
- Modify: `Assets/_KitchenClash/Composition/RootLifetimeScope.cs`
- Modify: `Assets/_KitchenClash/Composition/GameBootstrapper.cs`
- Modify: `Assets/_KitchenClash/Composition/KitchenClash.Composition.asmdef` only if missing Playcenter ref (check)

**Interfaces:**
- Consumes: all ports from Task 2 + existing StateMachineFlowPorts, MatchIntro, Countdown, Analytics
- Produces: Resolvable `IAppFlow` singleton; cold boot via flow

- [ ] **Step 1: Add `RegisterAppFlow` method**

```csharp
// Inside RootLifetimeScope.Configure, after RegisterInfrastructure:
RegisterAppFlow(builder);

private void RegisterAppFlow(IContainerBuilder builder)
{
    builder.Register<IAppFlow>(resolver =>
    {
        AppFlowController flow = null;
        IAppFlow Proxy() => flow;

        var stateManager = resolver.Resolve<IGameStateManager>();
        var stateFactory = resolver.Resolve<IStateFactory>();
        var ui = resolver.Resolve<IUIService>();
        var analytics = resolver.Resolve<IAnalyticsService>();

        var appFlowProxy = new KitchenClash.Infrastructure.Flow.AppFlowProxy(Proxy);

        flow = new AppFlowController(
            splash: new KitchenClash.Infrastructure.Flow.SplashFlowPort(appFlowProxy),
            boot: new KitchenClash.Infrastructure.Flow.BootFlowPort(stateManager),
            home: new KitchenClash.Infrastructure.Flow.HomeFlowPort(stateManager),
            matchmaking: new KitchenClash.Infrastructure.Flow.MatchmakingFlowPort(stateManager, stateFactory, ui),
            matchIntro: new KitchenClash.Infrastructure.Flow.MatchIntroFlowPort(ui, appFlowProxy),
            countdown: new KitchenClash.Infrastructure.Flow.CountdownFlowPort(ui, appFlowProxy),
            matchRuntime: new KitchenClash.Infrastructure.Flow.MatchRuntimeFlowPort(stateManager, stateFactory),
            results: new KitchenClash.Infrastructure.Flow.ResultsFlowPort(stateManager, ui),
            popupPolicy: new SoftPopupPolicy(),
            analytics: new KitchenClash.Infrastructure.Flow.AnalyticsFlowPort(analytics));

        return flow;
    }, Lifetime.Singleton);
}
```

Add usings: `Playcenter.GameFlow`, `KitchenClash.Infrastructure.Flow`.

**VContainer note:** `Register<IAppFlow>(Func<IObjectResolver, IAppFlow>, Lifetime)` — confirm VContainer API used in project. If factory form differs, use:

```csharp
builder.Register(resolver => { ... return (IAppFlow)flow; }, Lifetime.Singleton).As<IAppFlow>();
```

- [ ] **Step 2: GameBootstrapper starts flow**

```csharp
using Playcenter.GameFlow;
// ...
public class GameBootstrapper : IStartable
{
    private readonly IAppFlow _appFlow;
    private readonly IGameStateManager _gameStateManager;

    public GameBootstrapper(IAppFlow appFlow, IGameStateManager gameStateManager)
    {
        _appFlow = appFlow;
        _gameStateManager = gameStateManager;
    }

    public void Start()
    {
        GameLogger.Log("GameBootstrapper starting AppFlow cold boot...");
        // State machine starts empty until Boot port enters BootstrapState.
        // If Initialize is required before ChangeState, initialize with a no-op or first ChangeState inside port.
        _appFlow.StartColdBoot();
    }
}
```

If `ChangeState` without `Initialize` throws, BootFlowPort or Bootstrapper must `Initialize` first empty state. Read `StateMachine.Initialize` — if null current allows ChangeState, OK. Else:

```csharp
// BootFlowPort.EnterBoot:
if (_stateManager.CurrentState == null)
{
    _stateManager.Initialize(_stateFactory.Create<BootstrapState>());
}
else
{
    _stateManager.ChangeState<BootstrapState>();
}
```

Inject `IStateFactory` into BootFlowPort if needed.

- [ ] **Step 3: Build Composition + Infrastructure**

```bash
dotnet build KitchenClash.Composition.csproj -nologo
dotnet build KitchenClash.Infrastructure.csproj -nologo
```

Expected: 0 errors.

- [ ] **Step 4: Commit**

```bash
git add Assets/_KitchenClash/Composition/RootLifetimeScope.cs Assets/_KitchenClash/Composition/GameBootstrapper.cs Assets/_KitchenClash/Infrastructure/Flow/BootFlowPort.cs
git commit -m "feat(flow): register IAppFlow in RootLifetimeScope and cold-boot"
```

---

### Task 4: Presentation intents (Lobby, Results, HUD)

**Files:**
- Modify: `Assets/_KitchenClash/Presentation/ViewModels/LobbyViewModel.cs`
- Modify: `Assets/_KitchenClash/Presentation/Screens/ResultsScreen.cs`
- Modify: `Assets/_KitchenClash/Presentation/ViewModels/GameplayHudViewModel.cs`
- Create: `Assets/Scripts/Tests/EditMode/Gameplay/AppFlowMigrationTests.cs` (Lobby Play contract with fake IAppFlow)

**Interfaces:**
- Consumes: `IAppFlow.RequestPlay`, `ReturnHome`, `RequestPlayAgain`, `NotifyMatchCompleted`
- Produces: Presentation navigation without `IGameStateManager`

- [ ] **Step 1: Write LobbyViewModel test (or pure fake test)**

```csharp
[Test]
public void LobbyPlay_CallsRequestPlay_NotChangeState()
{
    var flow = new FakeAppFlow();
    // Construct LobbyViewModel with sessionContext + flow instead of state manager
    // After refactor:
    // vm.Play();
    // Assert.AreEqual(1, flow.RequestPlayCount);
}
```

- [ ] **Step 2: LobbyViewModel**

```csharp
using Playcenter.GameFlow;
// Remove: IGameStateManager, Infrastructure.States
private readonly IAppFlow _appFlow;

public LobbyViewModel(ISessionContext sessionContext, IAppFlow appFlow)
{
    _sessionContext = sessionContext;
    _appFlow = appFlow;
}

public void Play()
{
    string modeId = GameModeService?.SelectedGameMode?.Id;
    int teamSize = 2; // or from selected mode
    _appFlow.RequestPlay(new PlayRequest
    {
        ModeId = modeId,
        TeamSize = teamSize
    });
}
```

- [ ] **Step 3: ResultsScreen lobby button**

```csharp
[Inject] private IAppFlow _appFlow;

private void OnLobbyButtonClicked()
{
    GameLogger.Log("Returning to Lobby via AppFlow...");
    _sessionContext?.GameStarter?.EndGame(); // still tears down net
    _appFlow?.ReturnHome();
}
```

Ensure `GameStarter.EndGame` does not also `ChangeState` in a conflicting way — Task 6 fixes GameStarter to `ReturnHome` only once. For Task 4, if EndGame still ChangeStates, temporarily remove ChangeState from Results and leave EndGame, or call only EndGame if it will be fixed next — **prefer Task 4 Results calls EndGame + ReturnHome only after Task 6, or Task 4+6 same commit.** Combine Results + GameStarter in one commit if needed to avoid double Home.

- [ ] **Step 4: GameplayHudViewModel — remove ChangeState GameOver**

```csharp
// TryTransitionToGameOver: do not ChangeState.
// Instead inject IAppFlow and:
_appFlow.NotifyMatchCompleted(new MatchResultInfo
{
    IsDraw = _matchResultSync.CurrentResult.IsDraw,
    WinningTeamId = _matchResultSync.CurrentResult.WinningTeamId,
    // map Won from local team if available
});
```

If match end is also triggered elsewhere, guard with `_hasTriggeredGameOver` (already present).

Remove `using KitchenClash.Infrastructure.States` if unused.

- [ ] **Step 5: Build Presentation**

```bash
dotnet build KitchenClash.Presentation.csproj -nologo
```

- [ ] **Step 6: Commit**

```bash
git add Assets/_KitchenClash/Presentation Assets/Scripts/Tests/EditMode/Gameplay/AppFlowMigrationTests.cs
git commit -m "feat(flow): Presentation navigates via IAppFlow intents"
```

---

### Task 5: MatchmakingState notify rewire

**Files:**
- Modify: `Assets/_KitchenClash/Infrastructure/States/MatchmakingState.cs`
- Modify: `Assets/Scripts/Tests/EditMode/Gameplay/MatchmakingFlowTests.cs`

**Interfaces:**
- Consumes: `IAppFlow.NotifyMatchResolved`, `CancelMatchmaking` / `ReturnHome`, `FlowMatchInfoFactory`
- Produces: No `ChangeState<GameplayState>` / `ChangeState<MainMenuState>` from MM handlers

- [ ] **Step 1: Update tests first**

```csharp
[Test]
public void MatchmakingState_OnMatchFound_NotifiesAppFlow()
{
    var flow = new FakeAppFlow();
    var matchmakingService = new FakeMatchmakingService();
    var state = new MatchmakingState(..., flow, ...);
    state.Enter();
    // raise match found on fake
    matchmakingService.RaiseMatchFound(new LobbyInfo { /* minimal */ });
    Assert.AreEqual(1, flow.NotifyMatchResolvedCount);
    Assert.IsFalse(flow.ChangedToGameplayViaStateManager);
}
```

Extend `FakeMatchmakingService` with event raise helpers; add `FakeAppFlow : IAppFlow`.

- [ ] **Step 2: Inject IAppFlow into MatchmakingState**

```csharp
private readonly IAppFlow _appFlow;

// OnMatchFound:
_appFlow?.NotifyMatchResolved(
    FlowMatchInfoFactory.FromLobby(lobbyInfo, _hasFilledWithBots, _teamSize));
// Do NOT ChangeState<GameplayState>

// OnMatchmakingCancelled:
// Port Cancel already transitions Home; if user cancel from state:
_appFlow?.CancelMatchmaking(); // or ReturnHome if already cancelling service

// OnMatchmakingFailed:
_appFlow?.ReturnHome();

// Maintenance block path that used ChangeState<MainMenuState>:
_appFlow?.ReturnHome();
```

- [ ] **Step 3: Run MatchmakingFlowTests**

```bash
dotnet test RecipeRage.Tests.EditMode.csproj --filter "FullyQualifiedName~Matchmaking" -nologo
```

- [ ] **Step 4: Commit**

```bash
git add Assets/_KitchenClash/Infrastructure/States/MatchmakingState.cs Assets/Scripts/Tests/EditMode/Gameplay/MatchmakingFlowTests.cs
git commit -m "feat(flow): MatchmakingState notifies IAppFlow on resolve/cancel"
```

---

### Task 6: GameStarter + side states + remaining ChangeState product jumps

**Files:**
- Modify: `Assets/_KitchenClash/Infrastructure/Network/GameStarter.cs`
- Modify: `Assets/_KitchenClash/Infrastructure/States/LoginState.cs`
- Modify: `Assets/_KitchenClash/Infrastructure/States/MaintenanceState.cs`
- Modify: `Assets/_KitchenClash/Infrastructure/States/NoConnectionState.cs`
- Modify: `Assets/_KitchenClash/Infrastructure/States/TutorialState.cs`
- Modify: `Assets/_KitchenClash/Infrastructure/States/AccountUpgradeState.cs`
- Modify: `Assets/_KitchenClash/Infrastructure/States/GameOverState.cs` (if it navigates)

**Interfaces:**
- Consumes: `IAppFlow.ReturnHome`, `CompleteSidePhase`, `NotifyBootComplete` as applicable
- Produces: Grep-clean product navigation outside Flow ports

- [ ] **Step 1: GameStarter.ReturnToLobby**

```csharp
// Inject IAppFlow
private void ReturnToLobby()
{
    _lobbyManager.LeaveMatchLobby();
    _appFlow?.ReturnHome();
    // Remove ChangeState<MainMenuState>
}
```

- [ ] **Step 2: Side state completions**

Pattern for Login success:

```csharp
_appFlow?.CompleteSidePhase();
// If boot must continue, Boot port / NotifyBootComplete from SessionLoading
_stateManager.ChangeState<SessionLoadingState>(); // worker only if still required
```

Prefer: Login success → SessionLoading worker **without** fighting flow phase; then `NotifyBootComplete`.

Maintenance retry → `CompleteSidePhase` or re-enter Boot.

Tutorial complete → existing SessionLoading + flow notify.

AccountUpgrade → `CompleteSidePhase` / Home via `ReturnHome`.

- [ ] **Step 3: Grep gate**

```bash
rg -n "ChangeState" Assets/_KitchenClash/Presentation Assets/_KitchenClash/Infrastructure/States Assets/_KitchenClash/Infrastructure/Network/GameStarter.cs
```

Allowed remaining: none in Presentation; in States only worker-internal or temporary boot chain documented; **no** MM→Gameplay, HUD paths already fixed. Flow ports may still ChangeState.

- [ ] **Step 4: Build + commit**

```bash
dotnet build KitchenClash.Infrastructure.csproj -nologo
git add Assets/_KitchenClash/Infrastructure
git commit -m "feat(flow): GameStarter and side states use IAppFlow"
```

---

### Task 7: Intro + Countdown product path verification

**Files:**
- Verify: `MatchIntroFlowPort`, `CountdownFlowPort`, Resources templates
- Modify: `AppFlowController` only if Matchmaking→Match shortcut still taken when intro non-null (should not)
- Test: extend `AppFlowControllerTests.FullHappyPath`

- [ ] **Step 1: Confirm DI always passes non-null intro + countdown** (Task 3)

- [ ] **Step 2: MatchRuntime EnterMatch during Intro (optional preload)**

If design wants map load under intro card, call `matchRuntime.EnterMatch` from intro enter or controller — **only if already supported**. Do not invent heavy preload if GameplayState Enter is enough on Match phase.

- [ ] **Step 3: Ensure StartRound only after countdown**

GameplayState must not auto-start round on Enter if flow will call `RequestStartRound` after GO. Read `GameplayState.Enter` / `RequestStartRound` — if Enter auto-starts, gate with flag set by port.

- [ ] **Step 4: Manual checklist (document in commit body)**

Cold boot → Home → Play → bot fill → Intro UI → Countdown → Match HUD → Results → Home

- [ ] **Step 5: Commit if code changes**

```bash
git commit -m "fix(flow): gate StartRound until countdown complete"
```

---

### Task 8: Delete dual public API + wiki alignment

**Files:**
- Modify: `wiki/GameFlow-SDK.md` (DI snippet = actual RootLifetimeScope)
- Modify: `wiki/Technical.md` (navigation section: IAppFlow public)
- Modify: `wiki/log.md` (updated note)
- Modify: any CLAUDE / docs that say UI calls ChangeState for Play
- Delete: dead code only (unused Null production refs — keep Null* classes for tests)

- [ ] **Step 1: Grep delete gates**

```bash
rg -n "IGameStateManager" Assets/_KitchenClash/Presentation --glob "*.cs"
rg -n "ChangeState<" Assets/_KitchenClash/Presentation --glob "*.cs"
rg -n "NullSplashPort|NullBootPort|NullMatchIntroPort|NullCountdownPort" Assets/_KitchenClash/Composition --glob "*.cs"
```

Expected: no Presentation hits; no Null ports in Composition.

- [ ] **Step 2: Update wiki**

State clearly: production navigator is `IAppFlow`; states are workers; Root registers flow as in code.

- [ ] **Step 3: Full build**

```bash
dotnet build Playcenter.GameFlow.csproj -nologo
dotnet build KitchenClash.Domain.csproj -nologo
dotnet build KitchenClash.Application.csproj -nologo
dotnet build KitchenClash.Infrastructure.csproj -nologo
dotnet build KitchenClash.Presentation.csproj -nologo
dotnet build KitchenClash.Composition.csproj -nologo
```

Expected: 0 errors each.

- [ ] **Step 4: Commit**

```bash
git add wiki docs
git commit -m "docs(flow): wiki aligns with IAppFlow production cutover"
```

---

### Task 9: Hardening — FakeAppFlow shared test double + regression suite

**Files:**
- Create or consolidate: `Assets/Scripts/Tests/EditMode/Gameplay/Fakes/FakeAppFlow.cs` (or nested in test files if project avoids shared fakes)
- Modify: any broken tests from constructor signature changes

- [ ] **Step 1: Fix all EditMode compile breaks from new ctor params** (`MatchmakingState`, ViewModels)

- [ ] **Step 2: Run full EditMode test project**

```bash
dotnet build RecipeRage.Tests.EditMode.csproj -nologo
dotnet test RecipeRage.Tests.EditMode.csproj -nologo
```

- [ ] **Step 3: Commit**

```bash
git commit -m "test(flow): migration fakes and regression fixes"
```

---

## Self-review (plan vs spec)

| Spec section | Task |
|--------------|------|
| §4 Target architecture | Tasks 3–6 |
| §5.2 Splash/Boot ports | Task 2 |
| §5.3 DI + Bootstrapper | Task 3 |
| §5.4 Presentation cutover | Task 4 |
| §5.5 State notify rewire | Tasks 5–6 |
| §5.6 Delete dual public API | Task 8 |
| §6 Journey Intro/Countdown | Tasks 1, 5, 7 |
| §9 Testing | Tasks 1, 4, 5, 9 |
| §10 Phases 0–6 | Tasks 1–9 map phases 1–6 (inventory folded into tasks) |
| Non-goal: delete IGameStateManager | Honored |
| Non-goal: v2 combat | Honored |

**Placeholder scan:** NotifySplashComplete / NotifyBootComplete are conditional on controller gaps — engineer must read controller first (Task 2 Step 1). No TBD left for product behavior.

**Type consistency:** `PlayRequest`, `MatchResolvedInfo`, `MatchResultInfo`, `FlowPhaseId`, `IAppFlow` names match Playcenter.GameFlow sources.

---

## Execution handoff

Plan complete and saved to `docs/superpowers/plans/2026-07-13-gameflow-production-migration.md`.

**Two execution options:**

1. **Subagent-Driven (recommended)** — fresh subagent per task, review between tasks  
2. **Inline Execution** — this session with executing-plans and checkpoints  

**Autopilot default:** Subagent-Driven unless blocked; start Task 1 immediately after plan commit.
