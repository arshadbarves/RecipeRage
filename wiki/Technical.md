# Technical Architecture

## Clean Architecture

| Layer | Description | Dependencies |
|-------|-------------|--------------|
| Presentation | UI Toolkit screens (UXML+USS), ViewModels, RouterService | Application only |
| Application | Use Cases / Presenters (pure C#, VContainer IStartable) | Domain interfaces only |
| Domain | Pure C# models + interfaces. NO Unity deps. | None |
| Infrastructure | EOS, Firebase, Google/FB/Apple adapters, NGO NetworkBehaviours | Domain interfaces |

**MonoBehaviour used ONLY in:**
- `UIDocumentRoot.cs` — mounts UIDocument, provides root VisualElement
- `InputReceiver.cs` — reads touch, implements IDualStickInput
- `NetworkObjectAdapter.cs` — thin wrapper for NGO NetworkBehaviour
- `[Scene]LifetimeScope.cs` — VContainer composition roots

## VContainer Scope Tree

```
RootLifetimeScope (DontDestroyOnLoad)
  IEOSManager          → EOSManager           (Singleton)
  IAuthService         → EOSAuthService        (Singleton)
  IConfigService       → FirebaseRemoteConfigSvc (Singleton)
  IAnalyticsService    → FirebaseAnalyticsSvc   (Singleton)
  IConnectivityService → NetworkConnectivitySvc (Singleton + ITickable)
  IPlayerDataService   → EOSPlayerDataService   (Singleton)
  IRouterService       → RouterService          (Singleton)

  MenuLifetimeScope (child, active: home/store/lobby)
    IMatchmakingService → EOSMatchmakingService (Scoped)
    IFriendsService     → EOSFriendsService     (Scoped)

    MatchLifetimeScope (child, active: during a match only)
      IScoreService   → ScoreService    (Scoped)
      IOrderService   → OrderService    (Scoped)
      IHazardService  → HazardService   (Scoped)
      IAbilityService → AbilityService  (Scoped)
```

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
| Auth linking | EOS Connect | Links external tokens to ProductUserId |

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
│   ├── Router/       RouterService, IScreen, ScreenPresenter<T>
│   ├── Screens/      HomeScreen, StoreScreen, MatchHUD
│   ├── Overlays/     ConnectivityOverlay, DailyStreakPopup
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
- Firebase Auth (use EOS Connect ExternalCredentialType)
- Unity Relay (EOS P2P only via EOSTransport)
- Manual EOS_P2P_SendPacket for game state
- Floating joystick (fixed positions in InputReceiver.cs)
- Hold-to-chop (multi-tap right stick only)
- Static singletons (VContainer injection only)
- MonoBehaviour in Domain or Application layers
- Ads during match; interstitials for Battle Pass owners
