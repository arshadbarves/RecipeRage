# Technical Architecture

## Clean Architecture

| Layer | Description | Dependencies |
|-------|-------------|--------------|
| Presentation | UI Toolkit screens (UXML+USS), ViewModels, `UIService` + `UIScreenStackManager` | Application, Domain, GameFlow only (no Infrastructure / Netcode) |
| Application | Use Cases / Presenters (pure C#, VContainer IStartable) | Domain only (no EOS / NGO / Infrastructure) |
| Domain | Pure C# models + interfaces. NO Unity deps. | None |
| Infrastructure | EOS, Firebase, Google/FB/Apple adapters, NGO NetworkBehaviours | Application, Domain, GameFlow |

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
      IMatchHudPort   → MatchHudPort    (Scoped) — Presentation match surface
      GameplayHudViewModel → Transient
      BotManager      → BotManager      (Scoped)

  Root also registers null defaults for match/menu-only ports:
    IMatchHudPort            → NullMatchHudPort
    ICharacterPreviewService → NullCharacterPreviewService
  MenuLifetimeScope registers CharacterPreviewManager as ICharacterPreviewService when present in scene.
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

| Phase | Focus | Delete / compile gate | Status |
|-------|--------|------------------------|--------|
| 1 | Session shell + dependency laws | Presentation must not reference Infrastructure; Application must not reference EOS packages | **Complete** |
| 2 | UI navigation purity | Animation/localization via Application ports; shrink `UIService` | **Complete** |
| 3 | Infrastructure assembly walls | Split Flow / EOS / Network / Persistence (minimum) | **3a–3d complete** (Persistence/Audio/Flow/EOS leaves); Network still mega |
| 4 | Match gameplay ports | Expand match ports beyond HUD; shrink `PlayerController` / stations | **Complete** (scoped criteria) |
| 5 | Domain kernel hygiene (optional) | Shell ports vs cooking models if still noisy | **Closed partial** (`SlideDirection` fixed); broader split not required for hardened shell |

**Phase 4 shipped:**
- `PlayerController` partials: core (~393 lines) / InputMovement / Character / Skins / Carrying
- Existing SOLID collaborators retained (state, movement, input, network, interaction)
- `BotTaskPlanner` confirmed Domain-only in Application — no Infra deps, no relocation
- Match HUD remains on `IMatchHudPort` (Phase 1); Presentation still zero Network usings

**Phase 3d shipped (EOS leaf):**
- Leaf: `KitchenClash.Infrastructure.EOS` (Domain + Application + Configuration + UniTask + Netcode + EOS/UGS packages)
- Composition registers EOS adapters from the leaf; mega Infrastructure no longer compiles EOS sources or references PlayEveryWare/Epic/Friends packages
- Network remains mega (Network↔Gameplay cycle: `PlayerController` / stations ↔ abilities / validators)

**Phase 3c shipped (optional walls):**
- Leaf: `KitchenClash.Infrastructure.Audio` (Domain + Application + Platform + VContainer)
- Leaf: `KitchenClash.Infrastructure.Flow` (Domain + Application + Configuration + GameFlow + UniTask + VContainer)
- `CoroutineRunner` moved Network → Platform (Audio no longer depends on Network)
- Application port: `ISessionLifecycle` — Flow `SessionLoader` never references `SessionManager`
- `ForceUpdateChecker` moved Services → Flow.Handlers (BootSequence-only consumer)
- EditMode tests reference Flow leaf for `MatchmakingPhase`

**Phase 5 partial:**
- `SlideDirection` moved Domain → Animation leaf (was wrong namespace under Domain with `noEngineReferences`)

**Phase 3b shipped:**
- Application ports: `ICloudStorageProvider`, `IFriendsServiceFactory`, `ILocalNetworkIdentity`, `IClientTransportConfigurator`
- EOS adapters only; Network/Persistence/Flow no longer construct EOS concretes
- `UGSConfig` in Configuration; `ResultsPhase` on `IMatchHudPort` only
- Leaf: `KitchenClash.Infrastructure.Persistence` (Domain + Application + UniTask)

**Phase 3a shipped:**
- Leaf Infrastructure assemblies: Logging, Localization, Animation, Configuration, Platform, Async
- Composition + Editor wired; AnimationService DI registered at Root

**Phase 2 shipped:**
- `UIService` partials: core / Navigation / ScreenOps (documented responsibilities)
- Confirmed: localization, maintenance, animation already Application-port or Presentation-local (no new ports)

**Phase 1 shipped contracts:**
- `ISessionContext` in Application (interface-only facade)
- `LobbyOpResult` — Application lobby ops free of Epic `Result`
- `IMatchHudPort` + Domain `MatchResultSnapshot` — Presentation HUD/Results never touch `IMatchContext`
- `ICharacterPreviewService` — menu preview without Infrastructure usings
- Presentation-local `TweenExtensions` so DOTween→UniTask does not require Infrastructure.Animation

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
| `Playcenter.Shell` | **Shipped** | Engine-free logging + event bus + connectivity contracts (`Assets/Playcenter/Shell`); Domain/Application reference Shell; adapters stay game-side (`UnityLoggingService`, `LoggingBootstrap`, `NetworkConnectivityService`) |
| `Playcenter.Services` | **Shipped** | Engine-free multi-title service contracts (`Assets/Playcenter/Services`): config, analytics, ads, IAP, auth, encryption, maintenance, **localization, storage, time, audio volume, remote-config**. Domain/Application originals deleted; adapters stay game-side (Firebase, EOS, stubs) |
| `Playcenter.UI` | **Shipped** | Engine-free screen stack contracts (`IUIService`, `NotificationType`, `UIScreenCategory`); `Task`-based toasts; `SetCurrentScope(object)`. Adapter: Presentation `UIService` (UI Toolkit). KitchenClash Application/Domain originals deleted |
| New `Assets/Playcenter/*` | **Only if** engine-free **and** second consumer or legal-transition role | Prefer KitchenClash assembly splits + Domain/Application ports for single-title hardening |
| Clip audio / Platform / Async leaves | **Stay KitchenClash** | Unity-bound helpers (`AudioClip` playback, coroutines, platform glue) — not Playcenter |
| Economy / Cooking IP | **Never Playcenter** | Stay in KitchenClash |

**Hard cutover rules (Shell + Services + UI):** no Domain dual APIs, type aliases, obsolete stubs, or Console fallbacks. Unwired `GameLogger` throws. GameFlow and Shell do **not** reference Services or UI (independent modules). UI and Services do not reference each other.

**Logging wire order:** `RootLifetimeScope` registers `UnityLoggingService` as `ILoggingService`, then `RegisterBuildCallback` → `GameLogger.SetService` (before any entry point), then `RegisterEntryPoint<LoggingBootstrap>` (idempotent re-wire + bootstrap log). Never call `GameLogger` from `Configure()` (pre-build); use `Debug.LogError` for missing inspector refs.

Specs: `docs/superpowers/specs/2026-07-14-playcenter-shell-extract-design.md`, `docs/superpowers/specs/2026-07-14-playcenter-services-extract-design.md`, `docs/superpowers/specs/2026-07-15-playcenter-foundation-extract-design.md`.

See also: `docs/superpowers/plans/2026-07-14-playcenter-module-extract-candidates.md` (foundation ports + UI shipped; clip-audio/Platform/Async still deferred).

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

### Phase 3a — Infrastructure leaf assemblies (complete)

Compile-time walls for zero-cross-dep folders (folder-level asmdefs):

- `KitchenClash.Infrastructure.Logging` — `UnityLoggingService`, `LoggingBootstrap`
- `KitchenClash.Infrastructure.Localization` — `LocalizationManager`
- `KitchenClash.Infrastructure.Animation` — `AnimationService` + DOTween animators
- `KitchenClash.Infrastructure.Configuration` — `GameConstants`, `GameSettingsConfig`
- `KitchenClash.Infrastructure.Platform` — `PlatformUtils`, `CoroutineRunner`
- `KitchenClash.Infrastructure.Async` — `TaskExtensions`
- `KitchenClash.Infrastructure.Persistence` — cloud save adapters (Domain + Application + UniTask)
- `KitchenClash.Infrastructure.Audio` — music/SFX/pool (Domain + Application + Platform + VContainer)
- `KitchenClash.Infrastructure.Flow` — AppFlow ports + phase handlers (Domain + Application + Configuration + GameFlow + UniTask + VContainer)

Mega `KitchenClash.Infrastructure` retains Network / EOS / Gameplay / Services / DI. Phase 3b ports broke Network↔EOS / Persistence→EOS at source level; Phase 3c extracted Audio + Flow leaves after moving `CoroutineRunner` to Platform and introducing `ISessionLifecycle`. Network/EOS asmdef splits remain deferred (Gameplay still references Network types). Composition references all leaves for DI. `RootLifetimeScope` registers `AnimationService` + DOTween animators as `IAnimationService`, plus cloud/friends/identity/transport/`ISessionLifecycle` ports.

