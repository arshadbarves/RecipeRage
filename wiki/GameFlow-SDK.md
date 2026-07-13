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
Assets/_KitchenClash/Infrastructure/Flow/   # port adapters + AppFlowProxy
Assets/_KitchenClash/Presentation/Screens/  # shell skins (Home / MM / Results / Intro / Countdown)
Assets/_KitchenClash/Infrastructure/States/ # phase workers during migration
```

---

## GameFlow vs game states

| Layer | Role |
|-------|------|
| **`IAppFlow` / `AppFlowController`** | Public product navigator. Owns legal transitions. Fail-closed to Home. |
| **Ports** | Scene/UI/net work. GameFlow never loads scenes itself. |
| **`IGameStateManager` + states** | Phase **workers** during migration (Home/MM/Match/Results adapters). Not the public API. |

UI and features call **only** `IAppFlow` intents (`RequestPlay`, `ReturnHome`, `RequestPlayAgain`, …).

---

## DI (RootLifetimeScope)

```csharp
builder.Register<IAppFlow>(resolver =>
{
    AppFlowController flow = null;
    IAppFlow Proxy() => flow;

    var stateManager = resolver.Resolve<IGameStateManager>();
    var stateFactory = resolver.Resolve<IStateFactory>();
    var ui = resolver.Resolve<IUIService>();
    var analytics = resolver.Resolve<IAnalyticsService>();

    var appFlowProxy = new AppFlowProxy(Proxy);

    flow = new AppFlowController(
        splash: new SplashFlowPort(appFlowProxy),
        boot: new BootFlowPort(stateManager, stateFactory),
        home: new HomeFlowPort(stateManager),
        matchmaking: new MatchmakingFlowPort(stateManager, stateFactory, ui),
        matchIntro: new MatchIntroFlowPort(ui, appFlowProxy),
        countdown: new CountdownFlowPort(ui, appFlowProxy),
        matchRuntime: new MatchRuntimeFlowPort(stateManager, stateFactory),
        results: new ResultsFlowPort(stateManager, ui),
        popupPolicy: new SoftPopupPolicy(),
        analytics: new AnalyticsFlowPort(analytics));

    return flow;
}, Lifetime.Singleton);
```

---

## Policies

| Policy | Contract |
|--------|----------|
| `AlwaysResolveMatchPolicy` | `ShouldFillWithBots(searchSeconds, timeout)` — default timeout 30s |
| `RememberedQueuePolicy` | Empty `PlayRequest` → last mode/team/chef from `FlowContext` |
| `SoftPopupPolicy` | Soft offers only after `HasCompletedFirstPlay` |

MatchmakingState uses `AlwaysResolveMatchPolicy` for bot-fill timeout. Soft offers query `IAppFlow.CanShowSoftPopup()`.

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
