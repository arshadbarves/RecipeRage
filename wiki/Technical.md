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
  ISessionScopeInstaller→ MenuSessionScopeInstaller (Singleton) — REQUIRED for CreateSession
  MatchmakingPhaseHost  → MatchmakingPhaseHost   (Singleton + ITickable)
  IEOSManager           → EOSManager             (Singleton)
  IAuthService          → AuthenticationService  (Singleton)
  IConfigService / IRemoteConfigService → RemoteConfigService (Singleton; Playcenter.Services)
  IConfigProvider       → FirebaseConfigProvider | FallbackConfigProvider
  IAnalyticsService     → AnalyticsService + IAnalyticsSink (Singleton; Playcenter.Services)
  IAdsService           → AdsService + IAdNetwork (Singleton; Playcenter.Services)
  IIAPService           → IAPService + IStoreBackend + IIapRewardGrantor (Singleton)
  IConnectivityService  → NetworkConnectivitySvc (Singleton + ITickable)
  IPlayerDataService    → PlayerDataService      (Singleton)
  IUIService            → UIService              (Singleton)
  UIScreenStackManager  → UIScreenStackManager   (Singleton)
  ISettingsService      → PlayerPrefsSettingsService (Singleton) — GameSettings at ROOT
  ISettingsStore        → PlayerPrefsSettingsStore   (Singleton)
  IGameplayInput        → GameplayInputService   (Singleton) — dual-reg publisher
  IGameplayInputPublisher → GameplayInputService (Singleton)
  ICharacterPreviewService → CharacterPreviewGateway (Singleton) — scene binds via MenuSceneBinder
  IMatchHudPort            → NullMatchHudPort (match overrides)

  Session child (CreateSession ONLY — MenuSessionRegistrations; NOT scene MenuLifetimeScope)
    IMatchmakingService → EOSMatchmakingService (Scoped)
    IFriendsService     → EOSFriendsService     (Scoped)
    ITeamManager        → TeamManager           (Scoped)
    ILobbyManager       → EOSLobbyService       (Scoped) — party + match dual lobby
    INetworkingServices → NetworkingServiceContainer (Scoped)
    IEconomyService     → EconomyService        (Scoped) — dual IWallet + IWalletLedger
    IWallet             → EconomyService        (Scoped)
    IWalletLedger       → EconomyService        (Scoped)
    INetSession         → NgoEosNetSession      (Scoped)
    NetSessionConnectivityBridge → entry point (forfeit/host-drop → StopAsync)
    MatchRewardHandler  → entry point (credits via IWalletLedger only)
    ITutorialService    → TutorialService       (Scoped)
    // UIService.SetCurrentScope(SessionContainer) — screens resolve here (parent = Root)

  MenuLifetimeScope (MainMenu scene child of Root — EMPTY Configure)
    // parentReference / FindParent → RootLifetimeScope (never orphan root)
    // MenuSceneBinder binds CharacterPreviewManager → Root CharacterPreviewGateway
    // MUST NOT call MenuSessionRegistrations

  MatchLifetimeScope (Game scene child of Root — match services only)
    IScoreService   → ScoreService    (Scoped)
    IOrderService   → OrderService    (Scoped)
    IHazardService  → HazardService   (Scoped)
    IAbilityService → AbilityService  (Scoped)
    IMatchContext   → MatchContext    (Scoped)
    IMatchHudPort   → MatchHudPort    (Scoped) — Presentation match surface
    GameplayHudViewModel → Transient
    BotManager      → BotManager      (Scoped)
    // MATCH never owns wallet writes; cross-session via ISessionContext
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
| `Playcenter.Services` | **Shipped** | Engine-free multi-title service **contracts + shared implementations** (`Assets/Playcenter/Services`): config, analytics, ads, IAP, auth, encryption, maintenance, localization, storage, time, audio volume, remote-config. No VContainer, UnityEngine, or vendor SDKs |
| `Playcenter.Services.Unity` | **Shipped** | Vendor adapters only: `FirebaseAnalyticsSink`, `MaxAdNetwork`, `UnityIapStoreBackend`, `EditorFakeStoreBackend`. Behind `#if FIREBASE_ANALYTICS` / `APPLOVIN_MAX` / `UNITY_IAP` |
| `Playcenter.UI` | **Shipped** | Engine-free screen stack contracts (`IUIService`, `NotificationType`, `UIScreenCategory`); `Task`-based toasts; `SetCurrentScope(object)`. Adapter: Presentation `UIService` (UI Toolkit). KitchenClash Application/Domain originals deleted |
| New `Assets/Playcenter/*` | **Only if** engine-free **and** second consumer or legal-transition role | Prefer KitchenClash assembly splits + Domain/Application ports for single-title hardening |
| Clip audio / Platform / Async leaves | **Stay KitchenClash** | Unity-bound helpers (`AudioClip` playback, coroutines, platform glue) — not Playcenter |
| Economy / Cooking IP | **Never Playcenter** | Stay in KitchenClash (including `IIapRewardGrantor` + `IAPCatalog`) |

**Hard cutover rules (Shell + Services + UI):** no Domain dual APIs, type aliases, obsolete stubs, or Console fallbacks. Unwired `GameLogger` throws. GameFlow and Shell do **not** reference Services or UI (independent modules). UI and Services do not reference each other.

**Logging wire order:** `RootLifetimeScope` registers `UnityLoggingService` as `ILoggingService`, then `RegisterBuildCallback` → `GameLogger.SetService` (before any entry point), then `RegisterEntryPoint<LoggingBootstrap>` (idempotent re-wire + bootstrap log). Never call `GameLogger` from `Configure()` (pre-build); use `Debug.LogError` for missing inspector refs.

Specs: `docs/superpowers/specs/2026-07-14-playcenter-shell-extract-design.md`, `docs/superpowers/specs/2026-07-14-playcenter-services-extract-design.md`, `docs/superpowers/specs/2026-07-15-playcenter-foundation-extract-design.md`.

See also: `docs/superpowers/plans/2026-07-14-playcenter-module-extract-candidates.md` (foundation ports + UI shipped; clip-audio/Platform/Async still deferred).

## Playcenter Shared Services (Ads / Analytics / IAP / RemoteConfig)

**Spec:** `docs/superpowers/specs/2026-07-22-playcenter-shared-services-design.md`  
**Plan:** `docs/superpowers/plans/2026-07-22-playcenter-shared-services.md`

Common monetization/live-ops **logic** lives in the SDK so every title shares one implementation. Games keep only title-specific seams and Composition wiring.

| Layer | Assembly | Owns |
|-------|----------|------|
| Facades + flow | `Playcenter.Services` | `AnalyticsService`, `AdsService`, `IAPService`, `RemoteConfigService`, `FallbackConfigProvider`, ports |
| Vendor adapters | `Playcenter.Services.Unity` | Firebase Analytics sink, AppLovin MAX network, Unity IAP + editor fake store |
| Game seams | KitchenClash Infrastructure | `FirebaseConfigProvider`, `RecipeRageIapRewardGrantor`, `RemoteConfigEventBridge`, `AnalyticsEvents`, `IAPCatalog` |
| DI wire | Composition `RootLifetimeScope` | Register facade + port under `#if` vendor defines |

**Ports (SDK):** `IAnalyticsSink`, `IAdNetwork`, `IStoreBackend` (+ `StorePurchaseResult`), `IIapRewardGrantor`, `IConfigProvider`.  
**Public facades (unchanged contracts):** `IAnalyticsService`, `IAdsService`, `IIAPService`, `IRemoteConfigService`, `IConfigService`.

**IAP flow:** lazy store init once → purchase → grant via `IIapRewardGrantor` → analytics `iap_purchase_success` / `iap_purchase_fail` (`product_id`, `success`, `reason`). Grantor and analytics are null-safe.

**Remote config:** `RemoteConfigService` implements both `IRemoteConfigService` and `IConfigService`. Change notification is plain C# events (`OnConfigUpdated`, `OnHealthChanged`) — **not** `IEventBus`. Games attach `RemoteConfigEventBridge` to publish `ConfigUpdatedEvent` / `ConfigHealthStatusChangedEvent`.

**Root wiring pattern:**
```
#if FIREBASE_REMOTE_CONFIG → FirebaseConfigProvider #else FallbackConfigProvider
RemoteConfigService AsSelf + IConfigService + IRemoteConfigService
RemoteConfigEventBridge.Attach() Singleton
#if FIREBASE_ANALYTICS → FirebaseAnalyticsSink #else DebugAnalyticsSink → AnalyticsService
#if APPLOVIN_MAX → MaxAdNetwork #else NullAdNetwork → AdsService
#if UNITY_IAP → UnityIapStoreBackend #else EditorFakeStoreBackend
RecipeRageIapRewardGrantor (ISessionContext → EconomyService) → IAPService
```

**Deleted game stubs (hard cutover):** `StubAdsService`, `StubIAPService`, `FirebaseAnalyticsService` (Infrastructure + Firebase copies), `CompositeRemoteConfigService`, `FallbackRemoteConfigService`.

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
| Host dropped (v1) | 'Reconnecting...' overlay | **No host migration in v1.** Reconnect window then forfeit/end; `NetSessionConnectivityBridge` stops `INetSession` |

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

## Playcenter Client OS — Runtime Laws

**Program:** Tasks 1–11 implemented on `architecture-cleanup` (2026-07-19).  
**Spec:** `docs/superpowers/specs/2026-07-19-playcenter-client-os-design.md`  
**Plan:** `docs/superpowers/plans/2026-07-19-playcenter-client-os.md`

These laws match **shipped code**. Do not invent features beyond this surface.

### Boot law

1. **Step 0 connectivity gate** — `BootSequence` checks `IConnectivityService.IsOnline` **before** NTP / Remote Config / force-update / maintenance / auth.
2. Offline → `IAppFlow.EnterSidePhase(FlowPhaseId.NoConnection)` (do not continue boot services).
3. Authenticated success completion:
   - If `IAppFlow.Current == FlowPhaseId.Boot` → `NotifyBootComplete()` (main path → Home).
   - Else (retry from side phase, e.g. NoConnection / Login) → `CompleteSidePhase()` so return target stays Home.
4. Analytics: `boot_gate_offline` when the gate fires offline.

### Session DI law

1. `SessionManager.CreateSession` **requires** `ISessionScopeInstaller` (throws if null). Root registers `MenuSessionScopeInstaller`.
2. **Sole install path:** `MenuSessionRegistrations.Install(builder)` runs **only** from `MenuSessionScopeInstaller` during `CreateSession`. Scene `MenuLifetimeScope.Configure` is **empty** and must never call it.
3. Bare `CreateChild` with empty `Configure` for the **session** child is a **bug** — missing `IEconomyService` / wallet / net session. Scene Menu/Match scopes may be empty of session services; they parent to Root via `FindParent()` + `parentReference.TypeName = RootLifetimeScope`.
4. **Orphan law:** Scene LifetimeScopes must never become a second root (empty parent). Orphan roots double-install entry points (`MatchRewardHandler`) without parent `IEventBus` → VContainerException.
5. **Scene bind-in:** Presentation MonoBehaviours attach to Root gateways after load (`MenuSceneBinder` → `CharacterPreviewGateway`; match uses `MatchRuntimeSceneBinder` / `IMatchContext`). Do not FOFT-register scene components inside `CreateSession` (MainMenu is not loaded yet at login).
6. UI resolves from `SessionManager.SessionContainer` (`UIService.SetCurrentScope`); session inherits Root ports (gateway preview, event bus, config).

### Wallet law

1. Ports in **engine-free** `Playcenter.Services`: `IWallet`, `IWalletLedger`, `IWalletStore`, `CurrencyId`, `WalletSnapshot`.
2. `EconomyService` dual-implements `IEconomyService` + `IWallet` + `IWalletLedger` at **SESSION** (`MenuSessionRegistrations`).
3. **MATCH never owns wallet writes.** `MatchRewardHandler` (session entry point) credits only via `IWalletLedger`.
4. Reward path: `ResultsPhase` publishes `MatchEndedEvent` (from `MatchResultInfo` / HUD) → SESSION `MatchRewardHandler` → `IWalletLedger.Credit` + `MatchRewardEvent` + analytics. No `AwardMatchReward` mint API.
5. Analytics: `wallet_credit`, `purchase_success` / `purchase_fail`.

### Lobby law

1. `ILobbyManager` in `Playcenter.Services` exposes `CurrentPartyLobby` and `CurrentMatchLobby`.
2. **Party survives match end.** `LeaveMatchLobby()` ≠ `LeaveParty()`.
3. `EOSLobbyService` dual-tracks party vs match lobbies (EOS sample-style).
4. Brawl shell flow: party on Home → PLAY → MM → match lobby/VS → match → results → Home **with party intact**.

### Net law

1. Ports: `INetSession.StartAsync` / `StopAsync`, `INetTransportConfigurator`, `NetRole` (`Host` | `Client`) in `Playcenter.Services`.
2. Adapter: `NgoEosNetSession` (SESSION). `GameStarter` delegates start/stop to `INetSession` — not ad-hoc NGO host/client calls in new code.
3. Reconnect **v1**: stop session on forfeit / host-drop timeout via `NetSessionConnectivityBridge`; **no host migration**.
4. `NetworkManager` from match context / injected instance — **not** casual `NetworkManager.Singleton` (existing project rule).

### UI law

1. Shell components live on **`DesignSystem.uss`**: `pc-btn`, `pc-panel`, `pc-chip`, `pc-party-slot` (and match-shell helpers).
2. Do **not** edit `theme.uss` dual-brand tokens for shell work — DesignSystem is the locked shell theme.
3. Presentation must **not** reference Epic / `NetworkManager` / `EOSManager` (ports + Application only).

### Input / Settings law

1. `ISettingsService` + `GameSettings` at **ROOT** (`PlayerPrefsSettingsService` + `ISettingsStore`).
2. `IGameplayInput` + `InputAxis2` (no `Vector2` in `Playcenter.Services`). `GameplayInputService` dual-implements publisher; local `PlayerController` publishes each frame.

### Live-ops law

`AnalyticsEvents` constants (Application) with hooks:

| Constant | Event name |
|----------|------------|
| `BootGateOffline` | `boot_gate_offline` |
| `LoginSuccess` | `login_success` |
| `MatchStart` | `match_start` |
| `MatchEnd` | `match_end` |
| `WalletCredit` | `wallet_credit` |
| `PurchaseSuccess` / `PurchaseFail` | `purchase_success` / `purchase_fail` |

### Client OS commit map (Tasks 1–11)

| Task | Commit | Summary |
|------|--------|---------|
| 1 | `52dda74d`, `7366e318` | Connectivity-first boot + CompleteSidePhase from NoConnection |
| 2 | `268bebe8` | IWallet / IWalletLedger / IWalletStore ports |
| 3 | `9162a3e0` | EconomyService dual wallet; MatchRewardHandler via ledger |
| 4 | `07896178` | CreateSession requires ISessionScopeInstaller |
| 5 | `c75b4fe6` | Party vs match dual lobby |
| 6 | `5dfb2beb` | INetSession ports |
| 7 | `8c1a69c9` | NgoEosNetSession + GameStarter + forfeit bridge |
| 8 | `68993b1b` | DesignSystem shell components + Home |
| 9 | `61277f65` | Match lobby / VS / results DesignSystem |
| 10 | `864afeff` | IGameplayInput + ISettingsService |
| 11 | `70e45110` | Analytics hooks + presentation purity |

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
│   ├── Firebase/     FirebaseConfigProvider (RC provider seam only)
│   ├── RemoteConfig/ RemoteConfigEventBridge (SDK events → IEventBus)
│   ├── Services/     RecipeRageIapRewardGrantor (IAPCatalog → economy)
│   ├── Network/      KitchenNetworkState, ChefNetController
│   └── Platform/     GoogleSignInAdapter, FacebookAdapter
Assets/Playcenter/
├── Services/         ← pure C# facades + ports (Analytics/Ads/IAP/RC)
└── Services.Unity/   ← vendor adapters (Firebase/MAX/UnityIAP)
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

> **Testing amendment (2026-07-24):** Playcenter.MobileCore uses money-path testing —
> core state machines and planners only; adapters/DTOs/bootstrap verified by inspection.
> Approved by project owner. The blanket >80% rule still applies to all other new code.
> See `wiki/MobileCore.md`.

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

---

## Playcenter Studio SDK

**Program:** Wave W0 spec committed 2026-07-20; skill + wiki completed 2026-07-20. Waves W1–W6 pending.  
**Spec:** `docs/superpowers/specs/2026-07-20-playcenter-studio-sdk-design.md`  
**Skill:** `.github/skills/playcenter-sdk/SKILL.md` (mirrored to `.claude/skills/playcenter-sdk/SKILL.md`)

### Purpose

Studio-grade multi-game client SDK. Replaces `BootSequence` / `IAppFlow.StartColdBoot()` as the init path. Games implement `IGameEntry`; SDK owns shell, loading bar, gate screens, and vendor isolation.

### Laws (S1–S14 summary)

| # | Law |
|---|-----|
| S1 | Games reference **`Playcenter.SDK` only** for shell. Internal asmdefs are not public API. |
| S2 | **No VContainer in SDK** — DI = Builder + `IServiceRegistry`. |
| S3 | Game may keep VContainer for **game services only**; bridge SDK ports via `client.Services`. |
| S4 | **Vendor firewall** — `Epic.*`, raw NGO shell setup, store SDKs only in adapter assemblies. |
| S5 | **Ordered modules** — `IPlaycenterModule.InitializeAsync` + weighted loading bar. No interactive login mid-bar. |
| S6 | **Handoff** — `IGameEntry.OnPlaycenterReady(client)` (success) or `OnPlaycenterFailed(error)` (fail). |
| S7 | **Shell screens in SDK** — Splash, Loading, Settings, NoConnection, ForceUpdate, Maintenance. |
| S8 | **Full boot cutover** — `BootSequence` deleted; no dual boot. |
| S9 | **Session ownership unchanged** — `CreateSession` + installer; Menu/Match parent Root. |
| S10 | Multi-game: swap `IGameEntry` + Composition + theme per title. |
| S11 | Stack: Unity 6 + UniTask + UI Toolkit + NGO + EOS adapters; VContainer for game IP outside Playcenter. |
| S12 | **`IAppFlow` is sole post-ready navigator** — SDK never calls it internally. |
| S13 | **Hard cutover** — no legacy shims or parallel old-boot flags after each wave's delete gate. |
| S14 | DesignSystem tokens/USS = RecipeRage theme input into SDK theming (one brand per title). |

### Boot timeline (happy path)

1. Unity starts → `PlaycenterSdkBootstrap` (IStartable) constructs `PlaycenterClient` via builder.
2. SDK shows Splash (brief) → Loading.
3. Modules run in order (`logging → connectivity → ntp → remote_config → force_update → maintenance → auth_warmup → analytics → shell_ready`); progress bar advances by weight.
4. Ready → hide Loading → `IGameEntry.OnPlaycenterReadyAsync(client)`.
5. Game: auth UI if needed → `CreateSession` + installer → `IAppFlow` → Home.

**Failure path:** module failure → map to `BootFailureCode` → `IShellUi` shows gate screen → retry or `OnPlaycenterFailed`.

### Public facade types (`Playcenter.SDK`)

`PlaycenterClient`, `ClientOptions`, `IServiceRegistry`, `IPlaycenterModule`, `ModuleContext`, `IBootProgress`, `IGameEntry`, `BootFailure`, `BootFailureCode`, `IShellUi`, `ShellScreenId`, `IShellTheme`

### Delete list (normative after cutover)

| Remove | Replacement |
|--------|-------------|
| `BootSequence` + boot-only tests | Default modules + ModuleHost |
| `GameBootstrapper` → `IAppFlow.StartColdBoot()` | `PlaycenterClient.RunAsync` |
| Dual SDK service registrations in VContainer | `client.Services.Get<T>()` |
| Any `VContainer` ref under `Assets/Playcenter/**` | ServiceRegistry |
| Game duplicate Splash/Loading/NoInternet/Maintenance/Settings | SDK shell pack |

### Migration waves

| Wave | Key deliverable | Delete gate |
|------|----------------|-------------|
| W1 | `Playcenter.SDK` facade, `ServiceRegistry`, `ModuleHost`, progress | — |
| W2 | Default modules; `RunAsync` bootstrap | Delete `BootSequence`; no `StartColdBoot` init |
| W3 | Gate screens + Settings in SDK; theme tokens | Delete game duplicate shell screens |
| W4 | Strip VContainer from `Assets/Playcenter/**`; game bridges ports | `rg "using VContainer" Assets/Playcenter --glob '*.cs'` → 0 |
| W5 | Vendor firewall audit; wiki + skill final | `rg "Epic\." Assets/_KitchenClash/Presentation Assets/_KitchenClash/Application` → 0 |
| W6 | `IGameEntry` polish; auth/session after ready stable | Zero legacy boot symbols |


## Soft-launch release state (2026-07-24)

Guest-only soft launch tracked in `docs/release/` (branch `release-soft-launch`):

- **EOS environments:** all platform `eos_*_config.json` aligned to Dev sandbox for internal builds; promotion procedure + Stage/Live ids in `docs/release/EOS_ENVIRONMENTS.md`. `EpicOnlineServicesConfig.json` filled (deploymentID + clientID non-null). Versions: Unity `0.1.0-soft`, EOS ProductVersion `0.1.0`.
- **Persistence:** `PlayerDataService` round-trips `player_progress.json` / `player_stats.json` through `SaveService` (`CloudWithCache` → EOS Player Data Storage when logged in). `KitchenClashAuthLifecycleHooks` calls `ISaveService.OnUserLoggedIn/Out`.
- **Match runtime:** `MatchRuntimeBootstrap` (server-only, spawned by `MatchContext.TryWireWinCondition`) creates `MatchWinConditionCoordinator` per match and hands it the selected mode id (`rush_service` → `TugOfWarWinCondition`). `Map_RushService.unity` is in build settings.
- **Monetization:** ship without IAP/MAX for soft launch (`docs/release/MONETIZATION_DECISION.md`). No UI consumes `IIAPService`; rewarded CTA self-hides on `NullAdNetwork`.
- **Gate:** `docs/release/SOFT_LAUNCH_GATE.md` — human sign-off required before any store upload. Live cutover runbook only, gated (`docs/release/LIVE_PROMOTION_RUNBOOK.md`).
