# Playcenter GameFlow SDK

In-repo product shell for Brawl-class multiplayer games. First consumer: **RecipeRage**.

**Location:** `Assets/Playcenter/GameFlow/`  
**Assembly:** `Playcenter.GameFlow` (`noEngineReferences: true`, **zero** KitchenClash refs)  
**Public API:** `IAppFlow`

---

## Player journey (exact beats)

```
Studio Splash (Playcenter HTML matte)
  → Boot / Load (silent auth + config + progress)
  → HOME HUB (mode chip, currencies, BIG PLAY, bottom nav)
  → PLAY (1 tap; last mode/chef remembered)
  → Matchmaking (queue UI; AlwaysResolve → bots after timeout)
  → Match Intro (found / map card / load bar; map preloads)
  → Countdown 3-2-1-GO (input locked)
  → Match (kitchen plugin; HUD after GO)
  → Results (outcome, scores, PLAY AGAIN | HOME)
  → Home hub (loop)
```

### Brawl design rules (enforced by module)

1. Returning player: no login wall (silent auth during Boot).
2. Home is the product: PLAY one tap; last mode/chef remembered (`RememberedQueuePolicy`).
3. Matchmaking always resolves (`AlwaysResolveMatchPolicy` + game port bot fill).
4. Match load is a beat (Intro + Countdown before `StartRound`).
5. Results funnel: Play Again re-queues; Home returns hub.
6. Soft popups never block first PLAY (`SoftPopupPolicy` / `IAppFlow.CanShowSoftPopup()`).

---

## Module layout

```
Assets/Playcenter/GameFlow/
  Runtime/
    Playcenter.GameFlow.asmdef
    Core/       IAppFlow, AppFlowController, FlowContext, DTOs, FlowPhaseId
    Ports/      ISplashPort … IResultsPort, IPopupPolicyPort, IFlowAnalyticsPort
    Policies/   AlwaysResolveMatchPolicy, SoftPopupPolicy, RememberedQueuePolicy
  README.md
```

Game adapters live **outside** the module:

```
Assets/_KitchenClash/Infrastructure/Flow/           # port adapters + AppFlowProxy + SidePhaseFlowPort
Assets/_KitchenClash/Infrastructure/Flow/Handlers/  # port-owned phase handlers (BootSequence, *Phase)
Assets/_KitchenClash/Presentation/Screens/          # shell skins (Home / MM / Results / Intro / Countdown)
```

`IGameStateManager` / `IState` / `Infrastructure/States/*` were **hard-purged** (Phase 2). Ports own handlers; AppFlow is the sole phase owner.

---

## GameFlow vs phase handlers

| Layer | Role |
|-------|------|
| **`IAppFlow` / `AppFlowController`** | Public product navigator. Owns legal transitions (main + side phases). Fail-closed to Home. |
| **Ports** | Thin adapters. Enter/Exit delegates to handlers. GameFlow never loads scenes itself. |
| **Handlers (`*Phase` / `BootSequence` / `SessionLoader`)** | Port-owned work units (scene/UI/net). No state machine. |
| **`ISidePhasePort`** | Dispatches Login / Maintenance / NoConnection / Tutorial / AccountUpgrade / ForceUpdate. |

UI and features call **only** `IAppFlow` intents (`RequestPlay`, `ReturnHome`, `RequestPlayAgain`, `EnterSidePhase`, …).

---

## DI (RootLifetimeScope)

```csharp
builder.Register<SessionManager>(Lifetime.Singleton).AsSelf().As<IInitializable>();
builder.Register<SessionContext>(Lifetime.Singleton).As<ISessionContext>();
builder.Register<MatchmakingPhaseHost>(Lifetime.Singleton).AsSelf().As<ITickable>();

builder.Register<IAppFlow>(resolver =>
{
    AppFlowController flow = null;
    IAppFlow Proxy() => flow;

    var ui = resolver.Resolve<IUIService>();
    var analytics = resolver.Resolve<IAnalyticsService>();
    // … resolve boot/session deps; TryResolve optional menu-scoped services …

    var appFlowProxy = new AppFlowProxy(Proxy);
    var sessionLoader = new SessionLoader(sessionManager, sessionContext);
    var bootSequence = new BootSequence(/* ntp, rc, auth, maintenance, eventBus, appFlowProxy, sessionLoader */);
    var homePhase = new HomePhase(eventBus);
    var matchmakingPhase = new MatchmakingPhase(/* … */, appFlowProxy, matchmakingService);
    matchmakingHost.Phase = matchmakingPhase;
    var matchRuntimePhase = new MatchRuntimePhase(/* … */);
    var resultsPhase = new ResultsPhase(eventBus, economy, matchContext);
    var sidePhases = new SidePhaseFlowPort(login, maintenance, noConnection, tutorial, accountUpgrade);

    flow = new AppFlowController(
        splash: new SplashFlowPort(appFlowProxy),
        boot: new BootFlowPort(bootSequence),
        home: new HomeFlowPort(homePhase),
        matchmaking: new MatchmakingFlowPort(matchmakingPhase, ui),
        matchIntro: new MatchIntroFlowPort(ui, appFlowProxy),
        countdown: new CountdownFlowPort(ui, appFlowProxy),
        matchRuntime: new MatchRuntimeFlowPort(matchRuntimePhase),
        results: new ResultsFlowPort(resultsPhase, ui),
        popupPolicy: new SoftPopupPolicy(),
        analytics: new AnalyticsFlowPort(analytics),
        sidePhases: sidePhases);

    return flow;
}, Lifetime.Singleton);
```

**Boot rules:** authenticated cold boot ends with `NotifyBootComplete` only. Login success: `SessionLoader` then `CompleteSidePhase` only (never dual Complete+Notify).

Also at Root (core services):

```csharp
builder.Register<UnityLoggingService>(Lifetime.Singleton).As<ILoggingService>();
builder.Register<LoggingBootstrap>(Lifetime.Singleton).As<IInitializable>(); // GameLogger.SetService
```

---

## Logging (product shell)

GameFlow is engine-free and does **not** call `Debug.Log`. Logging contracts live in **`Playcenter.Shell`** (`ILoggingService`, `GameLogger`, `LogLevel`, `LogEntry`). Unity adapters stay in `KitchenClash.Infrastructure.Logging`.

| Path | What you see |
|------|----------------|
| `LoggingBootstrap` | Wires static `GameLogger` → `ILoggingService` / `UnityLoggingService` at Root init |
| `GameLogger.*` in handlers / UI | Unity Console + `OnLogAdded` (DebugConsole) |
| `AnalyticsFlowPort.TrackPhaseChanged` | `[AppFlow] {from} → {to}` on every phase change |

Without `LoggingBootstrap`, `GameLogger` **throws** `InvalidOperationException` (fail-closed — no Console fallback).

`Playcenter.GameFlow` must **not** reference `Playcenter.Shell` (keeps zero deps). Handlers/UI use Shell via game assemblies.

---

## Module extract (UPM)

`Playcenter.GameFlow` and `Playcenter.Shell` are in-repo assemblies with zero KitchenClash refs. **Do not** extract to UPM/git until a second title needs them. Handlers stay in `KitchenClash.Infrastructure.Flow` (game-specific). See `Assets/Playcenter/GameFlow/README.md` and `Assets/Playcenter/Shell/README.md`.

---

## Policies

| Policy | Contract |
|--------|----------|
| `AlwaysResolveMatchPolicy` | `ShouldFillWithBots(searchSeconds, timeout)` — default timeout 30s |
| `RememberedQueuePolicy` | Empty `PlayRequest` → last mode/team/chef from `FlowContext` |
| `SoftPopupPolicy` | Soft offers only after `HasCompletedFirstPlay` |

`MatchmakingPhase` uses `AlwaysResolveMatchPolicy` for bot-fill timeout (ticked via Root `MatchmakingPhaseHost`). Soft offers query `IAppFlow.CanShowSoftPopup()`.

---

## Runtime UI truth (shell screens)

Presenters load **Resources** templates, not draft UXML under `_KitchenClash/UI/`:

| Screen | Template | Style |
|--------|----------|-------|
| Home | `MainMenuViewTemplate` + `LobbyTemplate` | `MainMenu.uss`, `Lobby.uss` |
| Matchmaking | `MatchmakingViewTemplate` | `Matchmaking.uss` |
| Results | `GameOverScreenTemplate` | `GameOver.uss` |
| Match Intro | MatchIntro template | `MatchIntro.uss` |
| Countdown | Countdown overlay | `CountdownOverlay.uss` |
| Splash / Loading | Splash/Loading templates | Playcenter matte (carbon / mint / Space Grotesk) |

**Tokens:** carbon `#08090c`, mint `#00f099`, chalk white, Space Grotesk. DesignSystem exposes `--pc-*` / `--theme-mint`.

---

## Production platform architecture (full release target)

```
┌─────────────────────────────────────────────────────────────┐
│  Presentation (UI Toolkit)                                  │
│  Screens call IAppFlow only for product navigation          │
└───────────────────────────┬─────────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────────┐
│  Playcenter.GameFlow (IAppFlow)                             │
│  Product FSM · policies · ports                             │
└───────────────────────────┬─────────────────────────────────┘
                            │ ports
┌───────────────────────────▼─────────────────────────────────┐
│  Platform modules (interfaces @ Application/Domain)         │
│  Auth · Config/RC · Economy · IAP · Ads · Analytics · Save  │
│  Lobby / Matchmaking · Networking primitives (root)         │
└───────────────────────────┬─────────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────────┐
│  Game plugin (RecipeRage kitchen)                           │
│  MatchLifetimeScope: Score, Orders, Abilities, Hazards, Bots  │
│  MatchContext / registry bridge — no FindObjectOfType       │
└─────────────────────────────────────────────────────────────┘
```

### DI scopes (production)

| Scope | Owns |
|-------|------|
| **Root** | `IAppFlow`, auth, config, analytics, UI, player data, network primitives, IAP/Ads stubs→prod |
| **Menu / Session** | Lobby, matchmaking, team, friends, session networking container |
| **Match** | Score, orders, abilities, hazards, bots, `IMatchContext` |

### Modular surfaces (extract later as packages)

| Module | Now | Later |
|--------|-----|-------|
| GameFlow | In-repo `Assets/Playcenter/GameFlow` | Submodule / UPM |
| Auth | EOS in Infrastructure | `Playcenter.Auth` |
| Config / RC | Composite + Firebase | `Playcenter.Config` |
| Economy / IAP / Ads | Services + stubs | `Playcenter.Monetization` |
| Analytics | Firebase + `AnalyticsFlowPort` | `Playcenter.Analytics` |
| Kitchen combat | Game-specific forever | Never in GameFlow |

---

## Future extract (deferred)

When a second game needs GameFlow:

1. Move `Assets/Playcenter/GameFlow` → repo `playcenter-gameflow`
2. Consume via git submodule or UPM; pin tag `v0.x`
3. RecipeRage keeps only adapters under `_KitchenClash/Infrastructure/Flow/`

**Do not create remote/submodule until the vertical slice is proven and a second consumer exists.**

---

## Success criteria

1. Cold start → Home with PLAY, no login wall.
2. PLAY → Queue → Intro → Countdown → Match → Results → Home.
3. Play Again re-queues same mode.
4. Home / MM / Results use Playcenter matte (Brawl hierarchy, not Supercell IP).
5. `Playcenter.GameFlow` has zero KitchenClash references.
6. Kitchen combat unchanged by shell work.
7. Editor vertical slice passes.

---

## Related

- Module README: `Assets/Playcenter/GameFlow/README.md`
- UI language: [UI-UX](UI-UX.md), [Art-Direction](Art-Direction.md)
- Architecture layers: [Technical](Technical.md)
- Drift rules: [DRIFT-PROTOCOL](DRIFT-PROTOCOL.md)
