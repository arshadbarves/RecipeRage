# Technical Architecture

## Clean Architecture

| Layer | Description | Dependencies |
|-------|-------------|--------------|
| Presentation | UI Toolkit screens (UXML+USS), ViewModels, `UIService` + `UIScreenStackManager` | Application only |
| Application | Use Cases / Presenters (pure C#, VContainer IStartable) | Domain interfaces only |
| Domain | Pure C# models + interfaces. NO Unity deps. | None |
| Infrastructure | EOS, Firebase, Google/FB/Apple adapters, NGO NetworkBehaviours | Domain interfaces |

**MonoBehaviour used ONLY in:**
- `UIDocumentRoot.cs` — mounts UIDocument, provides root VisualElement
- `InputReceiver.cs` — reads touch, implements IDualStickInput
- `NetworkObjectAdapter.cs` — thin wrapper for NGO NetworkBehaviour
- `[Scene]LifetimeScope.cs` — VContainer composition roots

## VContainer Scope Tree

> **Historical docs may say `GameLifetimeScope` / `SessionLifetimeScope`. Code names are**
> **`RootLifetimeScope` / `MenuLifetimeScope` / `MatchLifetimeScope` under**
> **`Assets/_KitchenClash/Composition/`.**

```
RootLifetimeScope (app-lifetime, DontDestroyOnLoad)
  IAppFlow              → AppFlowController      (Singleton) — PUBLIC product navigator
  SessionManager        → SessionManager         (Singleton) — cold-boot session scope
  ISessionContext       → SessionContext         (Singleton)
  MatchmakingPhaseHost  → MatchmakingPhaseHost   (Singleton + ITickable)
  IEOSManager           → EOSManager             (Singleton)
  IAuthService          → AuthenticationService  (Singleton)
  IConfigService        → CompositeRemoteConfig  (Singleton)
  IAnalyticsService     → FirebaseAnalyticsSvc   (Singleton)
  IConnectivityService  → NetworkConnectivitySvc (Singleton + ITickable)
  IPlayerDataService    → PlayerDataService      (Singleton)
  IUIService            → UIService              (Singleton)
  UIScreenStackManager  → UIScreenStackManager   (Singleton)

  MenuLifetimeScope (child, active: session/menu/lobby/matchmaking)
    IMatchmakingService → EOSMatchmakingService (Scoped)
    IFriendsService     → EOSFriendsService     (Scoped)
    ITeamManager        → TeamManager           (Scoped)
    ILobbyManager       → LobbyManager          (Scoped)
    INetworkingServices → NetworkingServiceContainer (Scoped)
    IEconomyService     → EconomyService        (Scoped)
    ITutorialService    → TutorialService       (Scoped)

    MatchLifetimeScope (child, active: during a match only)
      IScoreService   → ScoreService    (Scoped)
      IOrderService   → OrderService    (Scoped)
      IHazardService  → HazardService   (Scoped)
      IAbilityService → AbilityService  (Scoped)
      IMatchContext   → MatchContext    (Scoped)
      BotManager      → BotManager      (Scoped)
```

## Product Navigation Architecture

**Public API:** `IAppFlow` (Playcenter.GameFlow) — sole navigator for features and UI.

| Component | Role |
|-----------|------|
| **IAppFlow** | Public product navigator. UI/features call `RequestPlay()`, `ReturnHome()`, `RequestPlayAgain()`, `EnterSidePhase()`. Owns main + side phase transitions; fail-closed to Home. |
| **Flow Ports** | Thin adapters (`BootFlowPort`, `HomeFlowPort`, …, `SidePhaseFlowPort`). Enter/Exit only. |
| **Handlers** | Port-owned work: `BootSequence`, `SessionLoader`, `HomePhase`, `MatchmakingPhase`, `MatchRuntimePhase`, `ResultsPhase`, side `*Phase`. No `IGameStateManager`. |

**Migration status:** Phase 1 (IAppFlow public cutover) + Phase 2 hard purge complete. `Application/State` and `Infrastructure/States` deleted.

## Architecture hardening (GameFlow-quality systems)

GameFlow fixed product navigation. Remaining mess is inverted deps, vendor leaks on Application ports, mega-Infrastructure, and god files.

**Program design:** `docs/superpowers/specs/2026-07-14-architecture-hardening-design.md`

| Phase | Focus | Delete / compile gate |
|-------|--------|------------------------|
| 1 | Session shell + dependency laws | Presentation must not reference Infrastructure; Application must not reference EOS packages |
| 2 | UI navigation purity | Animation/localization via Application ports; shrink `UIService` |
| 3 | Infrastructure assembly walls | Split Flow / EOS / Network / Persistence (minimum) |
| 4 | Match gameplay ports | HUD uses Application match ports only; shrink `PlayerController` / stations |
| 5 | Domain kernel hygiene (optional) | Shell ports vs cooking models if still noisy |

### Dependency laws (end state)

| From → To | Allowed? |
|-----------|----------|
| Presentation → Application, Domain, GameFlow | Yes |
| Presentation → Infrastructure.* | **No** |
| Application → Domain | Yes |
| Application → Epic / PlayEveryWare / NGO / Infrastructure | **No** |
| Infrastructure → Application, Domain, GameFlow | Yes |
| Composition → all | Yes |

### Playcenter extract policy

| Module | Status | Notes |
|--------|--------|-------|
| `Playcenter.GameFlow` | **Shipped** | Sole product navigator (`IAppFlow`); ports + policies; adapters in `Infrastructure/Flow` |
| New `Assets/Playcenter/*` | **Only if** engine-free **and** second consumer or legal-transition role | Prefer KitchenClash assembly splits + Domain/Application ports for single-title hardening |
| EventBus / Logging / Config as Playcenter | **Not required** | Domain ports + Infra adapters; logging fixed via `LoggingBootstrap` |
| Economy / Cooking IP | **Never Playcenter** | Stay in KitchenClash |

See also: `docs/superpowers/plans/2026-07-14-playcenter-module-extract-candidates.md` (partially superseded by hardening design for shell systems).

## SOLID Summary

| Principle | Implementation |
|-----------|----------------|
| Single Responsibility | ScoreService only scores. OrderService only manages orders. |
| Open/Closed | New chef ability = new IAbility class. No ScoreService changes. |
| Liskov Substitution | IAbility, IScoreService, IAuthService — any impl substitutes safely. |
| Interface Segregation | IReadOnlyMatchState ≠ IMatchController. |
| Dependency Inversion | All presenters depend on interfaces injected by VContainer. |

## Core Interfaces

```csharp
public interface IScoreService {
    int  TeamAScore { get; }
    int  TeamBScore { get; }
    void AddScore(TeamId team, ScoreEvent evt);
    IObservable<ScoreChangedEvent> ScoreStream { get; }
}

public interface IOrderService {
    IReadOnlyList<OrderModel> ActiveOrders { get; }
    OrderModel   GenerateOrder(float matchTimeRemaining);
    CompleteResult CompleteOrder(Guid id, float timeLeft, int combo);
    void         ExpireOrder(Guid id);
}

public interface IAbilityService {
    AbilityResult TryActivate(AbilitySlot slot, ChefId chef, AbilityContext ctx);
    void          ChargeSuper(ChefId chef, int dishesServed);
    float         GetCooldownRemaining(ChefId chef, AbilitySlot slot);
}

public interface IConfigService {
    T    Get<T>(string key, T fallback);
    Task FetchAsync();
}
```

## Networking

| Concern | Tool | Notes |
|---------|------|-------|
| Matchmaking discovery | EOS Sessions API | Create/search sessions with map+trophy attributes |
| NAT traversal | EOS P2P (via EOSTransport.cs) | Free relay. No Unity Relay cost. |
| Game state sync | Unity NGO (NetworkVariables + RPCs) | Never call EOS SendPacket for game state directly |
| Social/Friends | EOS Friends + Custom Invites | Party panel, invite links |
| Player data | EOS Player Data Storage | Trophies, streak, settings (5MB/player) |
| Auth linking | EOS Connect | Links external tokens to ProductUserId — project auth path; Firebase may exist for analytics/config |

## Connectivity (Brawl Stars Style)

| State | UI | Behaviour |
|-------|-----|-----------|
| Online | Nothing | Normal |
| Offline — Menu | Full-screen overlay, blocks all input | Retries every 3s, auto-dismisses on restore |
| Offline — In Match | Semi-transparent overlay + countdown | 3 reconnect attempts (5s each). Fail = forfeit + return menu |
| Host dropped | 'Reconnecting...' overlay | EOS host migration. 3s timeout then end match early |

```csharp
public sealed class NetworkConnectivityService : IConnectivityService, ITickable {
    private bool _prev = true;
    public  bool IsOnline { get; private set; } = true;
    public event Action<bool> OnConnectivityChanged;

    void ITickable.Tick() {
        bool now = Application.internetReachability != NetworkReachability.NotReachable;
        if (now == _prev) return;
        _prev = now; IsOnline = now;
        OnConnectivityChanged?.Invoke(now);
    }
}
```

## Firebase Remote Config

### Score Keys

| Key | Default | Type |
|-----|---------|------|
| score_base | 10 | int |
| score_speed_max | 5 | int |
| score_rhythm | 1 | int |
| score_combo | 2 | int |
| score_tier2_mult | 1.5 | float |
| score_tier3_mult | 2.0 | float |
| score_burn_penalty | 2 | int |
| score_fire_penalty | 5 | int |
| score_plate_pct | 0.10 | float |

### Gameplay Keys

| Key | Default | Type |
|-----|---------|------|
| match_duration_sec | 180 | int |
| rush_start_sec | 60 | int |
| rush_order_mult | 1.5 | float |
| chop_taps_lettuce | 3 | int |
| chop_taps_carrot | 4 | int |
| chop_taps_fish | 3 | int |
| chop_taps_meat | 5 | int |
| chop_tap_cap_per_sec | 10 | int |
| order_gen_rate_normal | 1.0 | float |
| fire_extinguish_window_sec | 5 | float |
| bot_fill_delay_sec | 40 | int |
| ability_cooldown_default | 10 | float |

### Matchmaking Keys

| Key | Default | Type |
|-----|---------|------|
| trophy_win_dominant | 35 | int |
| trophy_win_standard | 25 | int |
| trophy_win_close | 20 | int |
| trophy_loss_close | -15 | int |
| trophy_loss_standard | -20 | int |
| trophy_disconnect | -30 | int |
| trophy_bracket_tight | 200 | int |

### Monetization Keys

| Key | Default | Type |
|-----|---------|------|
| ad_interstitial_enabled | true | bool |
| ad_interstitial_frequency | 3 | int |
| ad_interstitial_min_gap_sec | 180 | int |
| ad_rewarded_enabled | true | bool |
| battle_pass_duration_days | 56 | int |
| daily_streak_cycle_days | 60 | int |

## Project Structure

```
Assets/_KitchenClash/
├── Domain/           ← PURE C#. No UnityEngine. 100% testable.
│   ├── Models/       MatchState, OrderModel, ChefDefinition
│   ├── Interfaces/   IScoreService, IOrderService, IAbilityService
│   └── Abilities/    IAbility, AbilityResult, AbilityContext
├── Application/      ← Pure C#. Depends on Domain only.
│   ├── Services/     ScoreService, OrderService, AbilityService
│   └── ViewModels/   HomeScreenVM, MatchHUDVM, StoreVM
├── Infrastructure/   ← Unity + external SDK implementations
│   ├── EOS/          EOSManager, EOSAuthService, EOSTransport
│   ├── Firebase/     FirebaseRemoteConfigSvc, FirebaseAnalyticsSvc
│   ├── Network/      KitchenNetworkState, ChefNetController
│   └── Platform/     GoogleSignInAdapter, FacebookAdapter
├── Presentation/     ← UI Toolkit presentation
│   ├── Screens/      HomeScreen, StoreScreen, MatchHUD (BaseUIScreen subclasses)
│   ├── Overlays/     ConnectivityOverlay, DailyStreakPopup
│   ├── ViewModels/   HomeScreenVM, StoreVM, MatchHUDVM
│   └── Common/       ObservableProperty<T>, UIDocumentRoot
├── Composition/      ← VContainer LifetimeScopes
│   ├── RootLifetimeScope.cs
│   ├── MenuLifetimeScope.cs
│   └── MatchLifetimeScope.cs
├── UI/               ← UXML + USS assets
├── ScriptableObjects/
├── Scenes/
└── Tests/
```

## Forbidden

- Hardcoded balance (all = IConfigService.Get with RC key + fallback)
- Firebase Auth for production auth (EOS Connect ExternalCredentialType is production path)
- Unity Relay (EOS P2P only via EOSTransport)
- Manual EOS_P2P_SendPacket for game state
- Floating joystick (fixed positions in InputReceiver.cs)
- Hold-to-chop (multi-tap right stick only)
- Static singletons (VContainer injection only)
- MonoBehaviour in Domain or Application layers
- Ads during match; interstitials for Battle Pass owners
