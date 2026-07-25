# RecipeRage Rebuild — Production Design Document

**Date:** 2026-07-25  
**Status:** Draft  
**Authors:** AI Assistant + Project Owner

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Game Design](#game-design)
3. [Technical Architecture](#technical-architecture)
4. [UI/UX Design](#uiux-design)
5. [Economy & Progression](#economy--progression)
6. [Monetization](#monetization)
7. [Audio Design](#audio-design)
8. [Implementation Plan](#implementation-plan)
9. [Wiki Structure](#wiki-structure)
10. [Cleanup Strategy](#cleanup-strategy)

---

## Executive Summary

**RecipeRage** is a top-down 2D multiplayer cooking competition game for mobile (landscape orientation). Two teams (2v2 or 3v3) race to complete a recipe list within 5 minutes. The first team to finish all recipes wins, or the team with the most recipes completed when time expires.

**Core Pillars:**
1. **Fast-paced cooking** — 5-minute matches, always moving, never idle
2. **Skill-based gameplay** — Chop speed is pure tapping skill; no pay-to-win
3. **Fair competition** — Mirrored kitchens, same recipe list, personal utility abilities only
4. **Mobile-first** — Dual-stick controls, landscape UI, 2-5 minute sessions
5. **Visual variety** — 4-5 themed maps at launch (Beach, Forest, Boat, etc.), Overcooked-style layouts
6. **Premium polish** — Bright colorful palette, soft shadows, smooth animations, 3D character showcase

**Target Audience:** Casual to mid-core mobile gamers (Brawl Stars, Overcooked fans)

**Platform:** iOS/Android (mobile now, PC later)

**Monetization:** IAP (chefs, coins, cosmetics) + Ads (rewarded videos, interstitials)

**Trophy System:** Win +15 trophies, Loss -8 trophies (Brawl Stars-style, encourages playing)

---

## Game Design

### Core Loop

```
FETCH (instant) → CHOP (tap-burst, fixed count) → COOK (autonomous timer) → SERVE (instant)
```

**Example: Tomato Soup (2 ingredients)**
1. Player walks to Ingredient Crate → taps Interact → receives Tomato + Onion
2. Player walks to Cutting Board → places Tomato → taps Chop button 8 times → Tomato is chopped
3. Player walks to Stove → places chopped Tomato → stove cooks automatically (15s)
4. Player walks to Ingredient Crate → fetches Garlic
5. Player walks to Cutting Board → chops Garlic (8 taps)
6. Player returns to Stove → collects cooked Tomato (before it burns)
7. Player walks to Serving Counter → delivers all ingredients → +100 points

### Match Rules

**Duration:** 5 minutes (300 seconds)

**Recipe List:**
- **2v2:** 12 recipes (4 easy, 4 medium, 4 hard)
- **3v3:** 18 recipes (6 easy, 6 medium, 6 hard)

**Win Condition:**
- First team to complete all recipes wins immediately, OR
- Team with most recipes completed when timer expires wins

**Trophy Rewards:**
- Win: +15 trophies
- Loss: -8 trophies
- Coins: 50 (win) / 20 (loss) + 5 per recipe completed

**Match Start Sequence:**
1. Matchmaking completes → Show team compositions (5 seconds)
2. 3-2-1 countdown (3 seconds)
3. Match starts

**Recipe Difficulty:**
- **Easy (T1):** 2 ingredients, 8 taps chop, 12s cook, 5s burn grace
- **Medium (T2):** 3 ingredients, 10 taps chop, 15s cook, 4s burn grace
- **Hard (T3):** 3 ingredients, 12 taps chop, 18s cook, 3s burn grace

**Note:** Points are tracked internally for progression (XP, coins) but NOT shown during match. Focus is purely on completing recipes fast.

### Cooking Mechanics

#### Chopping (Active)
- **Input:** Tap-burst (fixed count: 8/10/12 taps based on recipe tier)
- **Feedback:** Knife-cut animation per tap, faster tapping = faster completion
- **Skill:** Pure tapping speed (no upgrades affect this)

#### Cooking (Autonomous)
- **Input:** Place ingredient on stove, walk away
- **Timer:** 12s/15s/18s based on recipe tier
- **Burn:** If not collected within grace window (5s/4s/3s), ingredient burns
- **Penalty:** Burnt ingredient is wasted (no score penalty, just lost time)

#### Serving (Instant)
- **Input:** Walk to Serving Counter with all required ingredients → tap Interact
- **Validation:** Checks all ingredients match recipe (correct type, chopped, cooked)
- **Reward:** Points awarded, recipe marked complete

### Plate System

**How it works:**
1. Player fetches ingredients from crate → ingredients go into inventory
2. Player chops ingredients at cutting board → chopped ingredients go into inventory
3. Player cooks ingredients at stove → cooked ingredients go into inventory
4. Player walks to **Plate Station** → taps Interact → takes a plate
5. Player arranges ingredients on plate (tap to place each ingredient)
6. Player carries plate to **Serving Counter** → taps Interact → recipe validated and served

**Plate Mechanics:**
- Plates are physical objects you carry (like ingredients)
- Each plate can hold up to 4 ingredients
- Ingredients must be arranged on plate before serving
- Plate is consumed when recipe is served
- Empty plates can be returned to Plate Station for reuse

**Why plates:**
- Adds physicality to cooking (feels more like real cooking)
- Creates interesting routing decisions (fetch plate before or after cooking?)
- Prevents "ingredient soup" (all ingredients in one inventory slot)
- Visual clarity (see exactly what's on the plate)

### Station Layout (Map-Based, Overcooked-Style)

**Maps are themed and have different station counts/placements.**

**Example Maps:**

#### Map 1: Beach BBQ
```
┌─────────────────────────────────────────────────────────────┐
│ BEACH BBQ (Top-Down View)                                  │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  [Ingredient Crate 1]  [Ingredient Crate 2]                │
│                                                             │
│  [Cutting Board 1]     [Cutting Board 2]                   │
│                                                             │
│  [Plate Station]       [Counter/Prep Table]                │
│                                                             │
│  [BBQ Grill 1]         [BBQ Grill 2]                       │
│                                                             │
│  [Serving Counter]                                          │
│                                                             │
│  Dynamic Element: Moving platform (rotates slowly)         │
└─────────────────────────────────────────────────────────────┘
```

#### Map 2: Forest Campfire
```
┌─────────────────────────────────────────────────────────────┐
│ FOREST CAMPFIRE (Top-Down View)                            │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  [Ingredient Crate 1]  [Ingredient Crate 2]  [Crate 3]     │
│                                                             │
│  [Cutting Board 1]     [Cutting Board 2]                   │
│                                                             │
│  [Plate Station]                                            │
│                                                             │
│  [Campfire 1]          [Campfire 2]          [Campfire 3]  │
│                                                             │
│  [Serving Counter 1]   [Serving Counter 2]                 │
│                                                             │
│  Dynamic Element: Falling leaves (obscure vision)          │
└─────────────────────────────────────────────────────────────┘
```

#### Map 3: Pirate Ship
```
┌─────────────────────────────────────────────────────────────┐
│ PIRATE SHIP (Top-Down View)                                │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  [Ingredient Crate 1]  [Ingredient Crate 2]                │
│                                                             │
│  [Cutting Board 1]     [Cutting Board 2]                   │
│                                                             │
│  [Plate Station]       [Counter/Prep Table]                │
│                                                             │
│  [Stove 1]             [Stove 2]                           │
│                                                             │
│  [Serving Counter]                                          │
│                                                             │
│  Dynamic Element: Ship tilts (stations slide slightly)     │
└─────────────────────────────────────────────────────────────┘
```

**Map Features:**
- **4-5 maps at launch** (Beach, Forest, Boat, plus 1-2 more)
- **Different station counts** (some maps have 3 stoves, some have 2)
- **Dynamic elements** (moving platforms, falling leaves, ship tilting)
- **Bright colorful palette** (high saturation, Overcooked-style)
- **Daily/weekly rotation** (like Brawl Stars)
- **1 Tutorial map** (forced on first launch, teaches core mechanics)

**Station Types:**
- **Ingredient Crate:** Spawn raw ingredients (instant pickup)
- **Cutting Board:** Chop ingredients (tap-burst minigame)
- **Plate Station:** Take empty plate, arrange ingredients
- **Counter/Prep Table:** Temporary storage (holds 2 items)
- **Stove/Grill/Campfire:** Cook ingredients (autonomous, burn risk)
- **Serving Counter:** Deliver completed recipes (instant)

### Tutorial Map (First Launch)

**Forced interactive tutorial** — must complete before playing multiplayer.

**Tutorial Flow:**
1. **Welcome Screen** — "Welcome to RecipeRage! Let's learn how to cook."
2. **Movement Tutorial** — "Use the left stick to move." (Player moves to highlighted area)
3. **Fetch Tutorial** — "Walk to the crate and tap Interact to fetch ingredients." (Player fetches tomato)
4. **Chop Tutorial** — "Walk to the cutting board, place the tomato, and tap Chop 8 times." (Player chops tomato)
5. **Cook Tutorial** — "Walk to the stove, place the chopped tomato, and wait for it to cook." (Player cooks tomato, sees progress bar)
6. **Plate Tutorial** — "Walk to the plate station and take a plate." (Player takes plate)
7. **Arrange Tutorial** — "Place the cooked tomato on the plate." (Player arranges tomato on plate)
8. **Serve Tutorial** — "Walk to the serving counter and deliver the plate." (Player serves recipe)
9. **Burn Tutorial** — "Be careful! If you leave food on the stove too long, it burns." (Player sees burn warning)
10. **Complete** — "Great job! You're ready to compete. Let's play!"

**Tutorial Map Layout:**
```
┌─────────────────────────────────────────────────────────────┐
│ TUTORIAL KITCHEN (Top-Down View)                           │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  [Ingredient Crate]                                         │
│                                                             │
│  [Cutting Board]                                            │
│                                                             │
│  [Plate Station]                                            │
│                                                             │
│  [Stove]                                                    │
│                                                             │
│  [Serving Counter]                                          │
│                                                             │
│  (Simple layout, one of each station, no time pressure)    │
└─────────────────────────────────────────────────────────────┘
```

**Tutorial Features:**
- **Guided steps** — Highlighted areas, arrows, text prompts
- **No time limit** — Learn at your own pace
- **Can't fail** — Burned ingredients reset, no penalties
- **Skippable after first completion** — Can replay from settings
- **Teaches core loop** — Fetch → Chop → Cook → Plate → Serve
- **Teaches burn mechanic** — Shows what happens if you leave food too long

### Chef System

**4 Chefs at Launch** (6 slots, 2 locked as "Coming Soon") with rarity tiers and **personal utility abilities** (fair for multiplayer):

| Chef | Rarity | Unlock Cost | Ability | Ability Type |
|------|--------|-------------|---------|--------------|
| **Gordon** | Common | Starter | +10% movement speed | Passive |
| **Julia** | Common | Starter | +15% pickup/drop speed | Passive |
| **Marco** | Rare | 500 coins | +1 carry capacity (max 3 items) | Passive |
| **Gustavo** | Epic | 2,000 coins | Dash forward 3m (30s cooldown) | Active (1x per match) |
| **???** | Rare | Coming Soon | ??? | ??? |
| **???** | Legendary | Coming Soon | ??? | ??? |

**Default Mechanics (ALL Chefs):**
- **Carry capacity:** 2 items max (not an ability, just the default)
- **Burn grace:** Progress bar shown above stoves for ALL players
- **Recipe timers:** Progress bar shown above stoves for ALL players
- **Plate capacity:** 1 plate at a time

**Why these abilities are fair:**
- ✅ **Personal only** — Abilities affect YOU, not shared stations
- ✅ **No pay-to-win** — Abilities are convenience (movement, pickup speed), not power (chop/cook speed)
- ✅ **Skill expression** — Faster movement = better routing (skill-based)
- ✅ **Team-friendly** — Doesn't create "carry" dynamic

**Upgradeable Stats (L1→L10):**
- **Gordon:** +1% movement speed per level (max +10%)
- **Julia:** +1.5% pickup/drop speed per level (max +15%)
- **Marco:** +1 carry capacity at L5, +1 at L10 (max 4 items)
- **Gustavo:** -2s dash cooldown per level (max -20s, min 10s cooldown)

**NOT Upgradeable (Skill-Based):**
- Chop speed (pure tapping skill)
- Cook time (station property, not chef)
- Recipe completion (team effort)
- Carry capacity (default 2 items, Marco's ability adds +1/+2)

**Chef Selection (Brawl Stars-Style):**
- Select chef in **Lobby** (before matchmaking)
- Team can see each other's chef selections
- Once you click **Play**, goes directly to **Matchmaking**
- **No pre-match chef select screen** — chef is locked in when you queue

### Controls (Mobile, Landscape)

**Dual-Stick:**
- **Left Stick:** Move character
- **Right Stick:** Aim/interact (context-sensitive)
- **Interact Button:** Appears near stations (tap to interact)
- **Chop Button:** Appears when at Cutting Board (tap rapidly to chop)

**UI Layout (Landscape):**
```
┌─────────────────────────────────────────────────────────────────┐
│ [Team: 5/12]  [⏱️ 3:24]  [Enemy: 4/12]                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│                                                                 │
│                      [Game World - Top Down]                    │
│                                                                 │
│                                                                 │
│                                                                 │
│  Current Recipe: Tomato Soup (2/3)                             │
│  ✅ Tomato (chopped)  ✅ Onion (cooked)  ⬜ Garlic             │
│                                                                 │
├─────────────────────────────────────────────────────────────────┤
│ [Left Stick]                              [Interact] [Chop]    │
└─────────────────────────────────────────────────────────────────┘
```

---

## Technical Architecture

### Architecture Pattern

**Hybrid (Foundation + Vertical Slices)**

**Phase 0: Foundation (Week 1)**
- Playcenter SDK core services (DI, EventBus, Audio, Save, Config, Logging, Time, SceneLoading, Input, StateMachine)
- Separate composition roots (PlaycenterCompositionRoot → GameplayCompositionRoot)

**Slice 1: Core Gameplay Loop (Week 2-3)**
- Player, Stations, Recipes, Cooking, Match (single-player, no networking)

**Slice 2: Multiplayer (Week 4)**
- NGO + EOS integration, network sync, lobby/matchmaking
- EOS Cloud Storage for save data

**Slice 3: Bots (Week 5)**
- BotController, task evaluators, adaptive difficulty

**Slice 4: Progression (Week 6)**
- Chef unlock/upgrade, wallet, persistence
- Trophy system (win +15, loss -8)

**Slice 5: Monetization (Week 7)**
- IAP, ads, cosmetics

**Polish (Week 8+)**
- UI animations, audio mixing, VFX, optimization
- 4-5 themed maps (Beach, Forest, Boat, etc.)

### Folder Structure

```
Assets/
├── Playcenter/                     # Playcenter SDK (rebuilt from scratch)
│   ├── Core/
│   │   ├── DI/                     # PlaycenterCompositionRoot, ServiceLocator
│   │   ├── Events/                 # IEventBus, EventBus (custom pub/sub)
│   │   ├── Logging/                # ILoggingService, UnityLoggingService
│   │   └── Time/                   # ITimeService, UnityTimeService
│   ├── Services/
│   │   ├── Auth/                   # IAuthService, FacebookAuthService, GoogleAuthService, GuestAuthService (FULL LOGIC)
│   │   ├── Config/                 # IConfigService, FirebaseConfigService (FULL LOGIC)
│   │   ├── Storage/                # IStorageService, EOSCloudStorageService (FULL LOGIC)
│   │   ├── Analytics/              # IAnalyticsService, FirebaseAnalyticsService (FULL LOGIC)
│   │   ├── Ads/                    # IAdsService, AdMobService (FULL LOGIC)
│   │   ├── IAP/                    # IIAPService, UnityIAPService (FULL LOGIC)
│   │   ├── Friends/                # IFriendsService, UnityGamingServicesFriends (FULL LOGIC)
│   │   ├── Audio/                  # IAudioService, UnityAudioService (FULL LOGIC)
│   │   ├── Save/                   # ISaveService, EOSCloudSaveService (FULL LOGIC)
│   │   └── Wallet/                 # IWalletService, CoinWalletService (FULL LOGIC)
│   ├── UI/
│   │   ├── IUIService/             # Screen stack management
│   │   └── UIToolkit/              # BaseUIScreen, UIScreenAttribute
│   └── Net/
│       ├── INetService/            # Network abstraction
│       └── EOS/                    # EOS transport, lobby, matchmaking (FULL LOGIC)
│
├── Game/                           # RecipeRage game code (gameplay logic ONLY)
│   ├── DI/                         # GameplayCompositionRoot (listens to PlaycenterCompositionRoot)
│   ├── Gameplay/                   # Core gameplay (Slice 1)
│   │   ├── Player/                 # PlayerController, PlayerMovement
│   │   ├── Ingredient/             # IngredientItem, IngredientType
│   │   ├── Station/                # StationBase, CuttingStation, CookingStation, ServingStation, PlateStation, CounterStation
│   │   ├── Recipe/                 # Recipe, RecipeCatalog, RecipeDefinition
│   │   ├── Cooking/                # CookingController, ChopController, ServeController, PlateController
│   │   ├── Match/                  # MatchController, ScoreTracker, WinCondition, TrophyTracker
│   │   └── Tutorial/               # TutorialController, TutorialStep, TutorialMap
│   ├── Network/                    # Multiplayer (Slice 2)
│   │   ├── NetworkPlayer.cs        # NGO NetworkBehaviour wrapper
│   │   ├── NetworkStation.cs       # Station network sync
│   │   ├── NetworkMatch.cs         # Match state sync
│   │   └── EOSLobby.cs             # EOS lobby/matchmaking
│   ├── Bots/                       # Bot AI (Slice 3)
│   │   ├── BotController.cs        # Bot player controller
│   │   ├── BotBrain.cs             # Task planner integration
│   │   ├── Evaluators/             # FetchEvaluator, ChopEvaluator, CookEvaluator, ServeEvaluator, PlateEvaluator
│   │   └── AdaptiveDifficulty.cs   # Skill matching logic
│   ├── Progression/                # Progression (Slice 4)
│   │   ├── Chef/                   # ChefDefinition, ChefUnlock, ChefUpgrade (GAME-SPECIFIC LOGIC)
│   │   └── Trophy/                 # TrophyService, TrophyTracker (GAME-SPECIFIC LOGIC)
│   ├── Monetization/               # Monetization (Slice 5)
│   │   └── Cosmetics/              # ChefSkin, KitchenTheme (GAME-SPECIFIC LOGIC)
│   └── UI/                         # UI (Polish)
│       ├── Screens/                # MainMenuScreen, LobbyScreen, HUDScreen, ResultsScreen, ChefsScreen, ShopScreen
│       ├── Components/             # RecipeListItem, ChefCard, WalletDisplay
│       └── Animations/             # UITransition, UITween
│
└── Art/                            # Art assets
    ├── Characters/                 # 3D chef models (for main menu)
    ├── Maps/                       # 4-5 themed maps (Beach, Forest, Boat, etc.)
    ├── UI/                         # UI sprites, icons
    └── VFX/                        # Particles, shaders
```

### Assembly Structure

```
RecipeRage.Core.dll                 # Foundation (no Unity dependencies)
RecipeRage.Gameplay.dll             # Core gameplay logic
RecipeRage.Network.dll              # Networking (NGO + EOS)
RecipeRage.Bots.dll                 # Bot AI
RecipeRage.Progression.dll          # Chef/wallet/persistence
RecipeRage.Monetization.dll         # IAP/ads/cosmetics
RecipeRage.UI.dll                   # UI Toolkit screens

Playcenter.Core.dll                 # Playcenter SDK core
Playcenter.Services.dll             # Playcenter services
Playcenter.UI.dll                   # Playcenter UI
Playcenter.Net.dll                  # Playcenter networking
```

### Dependency Injection (Separate Composition Roots)

**Two composition roots: PlaycenterCompositionRoot (SDK) → GameplayCompositionRoot (Game)**

**PlaycenterCompositionRoot (SDK):**
```csharp
// Assets/Playcenter/Core/DI/PlaycenterCompositionRoot.cs
public sealed class PlaycenterCompositionRoot : MonoBehaviour
{
    public static event Action OnPlaycenterInitialized;

    private void Awake()
    {
        // Core services
        var eventBus = new EventBus();
        var loggingService = new UnityLoggingService();
        var timeService = new UnityTimeService();

        // SDK services (FULL LOGIC)
        var authService = new AuthService(); // Supports Facebook, Google, Guest
        var configService = new FirebaseConfigService();
        var storageService = new EOSCloudStorageService();
        var analyticsService = new FirebaseAnalyticsService();
        var adsService = new AdMobService();
        var iapService = new UnityIAPService();
        var friendsService = new UnityGamingServicesFriends();
        var audioService = new UnityAudioService(audioMixer);
        var saveService = new EOSCloudSaveService(storageService);
        var walletService = new CoinWalletService(saveService, analyticsService);

        // Register in service locator
        ServiceLocator.Register<IEventBus>(eventBus);
        ServiceLocator.Register<ILoggingService>(loggingService);
        ServiceLocator.Register<ITimeService>(timeService);
        ServiceLocator.Register<IAuthService>(authService);
        ServiceLocator.Register<IConfigService>(configService);
        ServiceLocator.Register<IStorageService>(storageService);
        ServiceLocator.Register<IAnalyticsService>(analyticsService);
        ServiceLocator.Register<IAdsService>(adsService);
        ServiceLocator.Register<IIAPService>(iapService);
        ServiceLocator.Register<IFriendsService>(friendsService);
        ServiceLocator.Register<IAudioService>(audioService);
        ServiceLocator.Register<ISaveService>(saveService);
        ServiceLocator.Register<IWalletService>(walletService);

        // Initialize SDK
        StartCoroutine(InitializeSDK());
    }

    private IEnumerator InitializeSDK()
    {
        // Initialize services in order
        yield return ServiceLocator.Get<IAuthService>().Initialize();
        yield return ServiceLocator.Get<IConfigService>().Initialize();
        yield return ServiceLocator.Get<IStorageService>().Initialize();
        yield return ServiceLocator.Get<IAnalyticsService>().Initialize();

        // Fire event when SDK is ready
        OnPlaycenterInitialized?.Invoke();
    }
}
```

**GameplayCompositionRoot (Game):**
```csharp
// Assets/Game/DI/GameplayCompositionRoot.cs
public sealed class GameplayCompositionRoot : MonoBehaviour
{
    private void Awake()
    {
        // Listen to Playcenter SDK initialization
        PlaycenterCompositionRoot.OnPlaycenterInitialized += OnPlaycenterReady;
    }

    private void OnPlaycenterReady()
    {
        // Get SDK services
        var eventBus = ServiceLocator.Get<IEventBus>();
        var saveService = ServiceLocator.Get<ISaveService>();
        var configService = ServiceLocator.Get<IConfigService>();
        var walletService = ServiceLocator.Get<IWalletService>();
        var analyticsService = ServiceLocator.Get<IAnalyticsService>();

        // Game-specific services (GAME LOGIC ONLY)
        var recipeCatalog = new RecipeCatalog(recipeScriptableObjects);
        var cookingController = new CookingController(eventBus, configService);
        var matchController = new MatchController(eventBus, recipeCatalog);
        var chefProgressionService = new ChefProgressionService(walletService, saveService, analyticsService);
        var trophyService = new TrophyService(saveService, analyticsService);

        // Audio system (event-driven)
        var audioSystem = new AudioSystem(ServiceLocator.Get<IAudioService>());
        audioSystem.Initialize(eventBus);

        // Register game services
        ServiceLocator.Register<IRecipeCatalog>(recipeCatalog);
        ServiceLocator.Register<ICookingController>(cookingController);
        ServiceLocator.Register<IMatchController>(matchController);
        ServiceLocator.Register<IChefProgressionService>(chefProgressionService);
        ServiceLocator.Register<ITrophyService>(trophyService);

        // Start game
        var stateMachine = new GameStateMachine();
        ServiceLocator.Register<IGameStateMachine>(stateMachine);
        stateMachine.ChangeState(new MainMenuState());
    }

    private void OnDestroy()
    {
        PlaycenterCompositionRoot.OnPlaycenterInitialized -= OnPlaycenterReady;
    }
}
```

**Why separate composition roots:**
- ✅ **Clear separation** — SDK initializes first, game initializes after
- ✅ **Easy to plug in** — Just add GameplayCompositionRoot to scene, it listens to SDK
- ✅ **Testable** — Can test game logic without SDK (mock OnPlaycenterInitialized)
- ✅ **Reusable** — SDK can be used in other games without game logic

### Event-Driven Audio Architecture

**Pattern:** Gameplay publishes events → AudioSystem subscribes and plays sounds

**Example:**
```csharp
// Gameplay (no audio knowledge)
public class CuttingStation : StationBase
{
    private readonly IEventBus _eventBus;

    private void OnChopComplete()
    {
        _eventBus.Publish(new IngredientChoppedEvent(ingredientId));
    }
}

// Audio (no gameplay knowledge)
public class AudioSystem
{
    private readonly IAudioService _audioService;

    public void Initialize(IEventBus bus)
    {
        bus.Subscribe<IngredientChoppedEvent>(OnChopped);
        bus.Subscribe<IngredientCookedEvent>(OnCooked);
        bus.Subscribe<RecipeServedEvent>(OnServed);
    }

    private void OnChopped(IngredientChoppedEvent e)
    {
        _audioService.Play(SfxId.KnifeChop);
    }

    private void OnCooked(IngredientCookedEvent e)
    {
        _audioService.Play(SfxId.CookingDone);
    }

    private void OnServed(RecipeServedEvent e)
    {
        _audioService.Play(SfxId.RecipeComplete);
    }
}
```

### State Machine (Game Flow)

**States:**
```
BootState → MainMenuState → LobbyState → MatchState → ResultsState → MainMenuState
```

**Example:**
```csharp
public interface IGameState
{
    void Enter();
    void Exit();
    void Update(float deltaTime);
}

public sealed class GameStateMachine
{
    private IGameState _currentState;

    public void ChangeState(IGameState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();
    }

    public void Update(float deltaTime)
    {
        _currentState?.Update(deltaTime);
    }
}
```

---

## UI/UX Design

### Design Principles

1. **Mobile-first, landscape orientation** — All UI optimized for horizontal play
2. **Brawl Stars-inspired** — Proven mobile game UX patterns
3. **Premium polish** — Smooth animations, satisfying feedback, high-quality visuals
4. **Minimal HUD** — Only essential info during match (recipe list, timer, score)
5. **3D character showcase** — Main menu features 3D chef model (like Brawl Stars)
6. **Bright colorful palette** — High saturation, Overcooked-style (flat colors, no gradients)
7. **Soft shadows** — Subtle depth, still flat colors (UI Toolkit-supported styles only)

### Screen Flow

```
Boot → Login → Main Menu → Lobby (Chef Select) → Matchmaking → Team Compositions (5s) → Countdown (3-2-1) → Match → Results
                     ↓
              [Chefs] [Shop] [Events] [Settings] [Friends]
```

**Note:** Chef selection happens in **Lobby** (before matchmaking), not in a separate pre-match screen. Once you click **Play**, you go directly to matchmaking.

### Key Screens (Landscape, Premium UI)

#### 1. Login Screen
```
┌─────────────────────────────────────────────────────────────────┐
│                                                                 │
│                      [RecipeRage Logo]                          │
│                   (Animated, premium)                           │
│                                                                 │
│                 "Cook. Compete. Conquer."                       │
│                                                                 │
│                                                                 │
│            [🔵 Sign in with Facebook]                           │
│            [🔴 Sign in with Google]                             │
│            [⚪ Play as Guest]                                   │
│                                                                 │
│                                                                 │
│  By continuing, you agree to our Terms of Service              │
└─────────────────────────────────────────────────────────────────┘
```

**Animations:**
- Logo fades in with scale bounce (0.3s)
- Buttons slide in from bottom with stagger (0.1s delay each)
- Button press: scale down 0.95x + haptic feedback

**Auth Providers:**
- Facebook
- Google
- Guest (anonymous)
- **No Epic account login** (EOS used for backend, not auth)

#### 2. Main Menu (Landscape, 3D Character)
```
┌─────────────────────────────────────────────────────────────────┐
│ [👤 Profile]  [💰 1,234]  [💎 56]  [⚙️]  [👥 Friends]         │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌──────────────────┐                                          │
│  │                  │         [PLAY - Large Button]            │
│  │  3D Chef Model   │         (Pulsing glow, premium)          │
│  │  (Rotating)      │                                          │
│  │                  │         [Daily Rewards: Day 3]           │
│  │  Gordon L5       │         [Claim 100 Coins]                │
│  │  ★★☆             │                                          │
│  └──────────────────┘                                          │
│                                                                 │
├─────────────────────────────────────────────────────────────────┤
│ [👨‍🍳 Chefs]  [🛒 Shop]  [🎁 Events]  [📊 Stats]              │
└─────────────────────────────────────────────────────────────────┘
```

**Animations:**
- 3D chef model rotates slowly (idle animation)
- PLAY button pulses with glow effect (1s loop)
- Daily rewards bounce when claimable
- Tab buttons: icon + label, active tab highlighted with underline slide

**3D Character:**
- Rendered in RenderTexture, displayed in UI
- Tap to rotate, pinch to zoom
- Swipe left/right to cycle through unlocked chefs
- Idle animation (breathing, occasional gesture)

#### 3. Friends Screen (Unity Gaming Services)
```
┌─────────────────────────────────────────────────────────────────┐
│ [← Back]  Friends                    [+ Invite Friends]        │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Online (3):                                                    │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │ 🟢 PlayerName123  •  In Main Menu        [Invite]      │   │
│  │ 🟢 CoolChef42     •  In Match (2:34)     [Spectate]    │   │
│  │ 🟢 CookingKing    •  In Lobby            [Join]        │   │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                 │
│  Offline (12):                                                  │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │ ⚫ Friend1  •  Last online 2h ago                       │   │
│  │ ⚫ Friend2  •  Last online 1d ago                       │   │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                 │
│  [+ Add Friend by Code]  [📋 My Friend Code: ABC123]          │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

**Unity Gaming Services Integration:**
- Uses Unity Friends Service (not EOS, since EOS requires Epic account)
- Friend codes for adding friends
- Online status (In Main Menu, In Match, In Lobby)
- Invite to party, join party, spectate match

#### 4. Game Mode Select
```
┌─────────────────────────────────────────────────────────────────┐
│ [← Back]  Select Game Mode                                      │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │ 🏆 RANKED MATCH                                         │   │
│  │ 2v2 or 3v3 • 5 minutes • Trophy rewards                 │   │
│  │ [Play]                                                  │   │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │ 🎮 CASUAL MATCH                                         │   │
│  │ 2v2 or 3v3 • 5 minutes • No trophies                    │   │
│  │ [Play]                                                  │   │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │ 🤖 PRACTICE MODE                                        │   │
│  │ Solo vs Bots • No time limit • Learn recipes            │   │
│  │ [Play]                                                  │   │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

**Animations:**
- Cards slide in from right with stagger (0.1s delay each)
- Hover/tap: card scales up 1.05x with shadow
- Play button: scale down 0.95x + haptic feedback

#### 5. Lobby Screen (Chef Select, Brawl Stars-Style)
```
┌─────────────────────────────────────────────────────────────────┐
│ [← Back]  Lobby                          Team: 2/2 players      │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Your Team:                    Enemy Team: (hidden until match) │
│  [Player1: Gordon L5]          [???]                            │
│  [You: Select Chef ▼]          [???]                            │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │              [3D Chef Model - Rotating]                 │   │
│  │                                                         │   │
│  │  Gordon                                                 │   │
│  │  Level 5/10                                             │   │
│  │  Ability: +10% movement speed                           │   │
│  │  ████████░░ 5.6 m/s                                     │   │
│  │                                                         │   │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                 │
│  Chef Grid (scrollable horizontal):                            │
│  [Gordon] [Julia] [Marco] [Yuki] [Gustavo] [Remy]            │
│    L5       L3      L7      L1      L2        L0🔒            │
│                                                                 │
│  [Play - Large Button]  (goes directly to matchmaking)        │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

**Animations:**
- 3D chef model rotates when selected
- Chef cards: horizontal scroll with snap-to-center
- Selected card: scale up 1.1x with soft shadow
- Play button: pulse when all players ready

**Note:** Chef is locked in when you click **Play**. No pre-match chef select screen.

#### 6. Matchmaking
```
┌─────────────────────────────────────────────────────────────────┐
│                                                                 │
│              "Finding players..."                               │
│                                                                 │
│         [Animated cooking pot with steam]                       │
│                                                                 │
│              Players found: 3/4                                 │
│              ████████████░░░░ 75%                               │
│                                                                 │
│              Estimated wait: 15s                                │
│                                                                 │
│                                                                 │
│              [Cancel]                                           │
│                                                                 │
│  💡 Tip: Tap rapidly to chop vegetables faster!                │
└─────────────────────────────────────────────────────────────────┘
```

**Animations:**
- Cooking pot bounces with steam particles
- Progress bar fills smoothly
- Tips rotate every 5s with fade transition
- Cancel button: scale down 0.95x + haptic feedback

#### 7. Team Compositions (5 seconds)
```
┌─────────────────────────────────────────────────────────────────┐
│                                                                 │
│                    MATCH FOUND!                                 │
│                                                                 │
│  Your Team:                    Enemy Team:                      │
│  ┌──────────────────┐          ┌──────────────────┐            │
│  │ Player1          │          │ Enemy1           │            │
│  │ Gordon L5        │          │ Julia L7         │            │
│  │ ★★☆              │          │ ★★★              │            │
│  └──────────────────┘          └──────────────────┘            │
│  ┌──────────────────┐          ┌──────────────────┐            │
│  │ You              │          │ Enemy2           │            │
│  │ Marco L7         │          │ Yuki L3          │            │
│  │ ★★★              │          │ ★☆☆              │            │
│  └──────────────────┘          └──────────────────┘            │
│                                                                 │
│  Map: Beach BBQ                                                 │
│                                                                 │
│  Starting in 5... 4... 3... 2... 1...                          │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

**Animations:**
- Team cards slide in from left/right (0.3s)
- Chef models fade in with scale bounce (0.2s stagger)
- Countdown numbers scale up + fade out (1s each)
- Map name fades in at bottom

#### 8. Countdown (3-2-1)
```
┌─────────────────────────────────────────────────────────────────┐
│                                                                 │
│                                                                 │
│                         3                                       │
│                  (Large, centered)                              │
│                                                                 │
│                                                                 │
│                                                                 │
│                                                                 │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

**Animations:**
- Number scales up from 0.5x to 2x (0.5s)
- Number fades out (0.5s)
- Next number appears immediately
- "Cook!" text appears after 1 (0.3s scale bounce)

#### 9. In-Match HUD (Landscape, Minimal)
```
┌─────────────────────────────────────────────────────────────────┐
│ [Team: 5/12]  [⏱️ 3:24]  [Enemy: 4/12]                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│                                                                 │
│                      [Game World - Top Down]                    │
│                                                                 │
│                                                                 │
│                                                                 │
│  Current Recipe: Tomato Soup (2/3)                             │
│  ✅ Tomato (chopped)  ✅ Onion (cooked)  ⬜ Garlic             │
│                                                                 │
├─────────────────────────────────────────────────────────────────┤
│ [Left Stick]                              [Interact] [Chop]    │
└─────────────────────────────────────────────────────────────────┘
```

**Animations:**
- Score updates: number counts up with bounce
- Timer: pulses red when < 30s remaining
- Recipe progress: checkmarks animate in with scale bounce
- Interact/Chop buttons: appear/disappear with fade + slide

**Note:** No points shown in HUD. Focus is purely on completing recipes fast.

#### 10. Results Screen
```
┌─────────────────────────────────────────────────────────────────┐
│                                                                 │
│                      VICTORY! 🎉                                │
│                  (Confetti particles)                           │
│                                                                 │
│  Your Team: 12 recipes  ★★★  Enemy Team: 10 recipes           │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │ MVP: You! (8 recipes completed)                         │   │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                 │
│  Trophies:                                                      │
│  +15 🏆 (Win)                                                   │
│  ─────────────────────                                          │
│  Total: 1,234 🏆                                                │
│                                                                 │
│  Coins:                                                         │
│  +50 (Win)                                                      │
│  +40 (8 recipes × 5)                                            │
│  ─────────────────────                                          │
│  Total: +90 💰                                                  │
│                                                                 │
│  Chef XP:                                                       │
│  Gordon: +25 XP (Level 5 → 6 progress: 725/1100)              │
│  ████████████░░░░░░░░ 66%                                       │
│                                                                 │
│  [Play Again]  [Change Chef]  [Main Menu]                     │
│                                                                 │
│  [📺 Watch Ad for 2x Coins]                                    │
└─────────────────────────────────────────────────────────────────┘
```

**Animations:**
- "VICTORY!" text: scale bounce with confetti particles
- Stars: animate in one-by-one with rotation
- Trophies: count up with trophy icon fly animation
- Coins: count up with coin fly animation
- XP bar: fills smoothly with soft shadow pulse
- Buttons: slide in from bottom with stagger

**Note:** Trophies are gained/lost (win +15, loss -8). Coins are only earned and spent (never lost).

#### 11. Chefs Screen (Collection)
```
┌─────────────────────────────────────────────────────────────────┐
│ [← Back]  My Chefs                      [Sort ▼] [Filter ▼]    │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌──────────────────┐                                          │
│  │                  │                                          │
│  │  3D Chef Model   │  Gordon                                  │
│  │  (Rotating)      │  Level 5/10                              │
│  │                  │  ████████░░░░░░░░░░░░  725/1100 XP       │
│  │                  │                                          │
│  │                  │  Ability: +10% movement speed            │
│  │                  │  ████████░░ 5.6 m/s (+6%)                │
│  │                  │                                          │
│  │                  │  [Upgrade to L6: 1,100 Coins]            │
│  │                  │  (+1% movement speed)                    │
│  │                  │                                          │
│  └──────────────────┘                                          │
│                                                                 │
│  Chef Grid (scrollable horizontal):                            │
│  [Gordon] [Julia] [Marco] [Yuki] [Gustavo] [Remy]            │
│    L5       L3      L7      L1      L2        L0🔒            │
│                                                                 │
├─────────────────────────────────────────────────────────────────┤
│ [👨‍🍳 Chefs]  [🛒 Shop]  [🎁 Events]  [📊 Stats]              │
└─────────────────────────────────────────────────────────────────┘
```

**Animations:**
- 3D chef model rotates when selected
- Chef cards: horizontal scroll with snap-to-center
- Stats bars: animate in with stagger when chef selected
- Upgrade button: pulse when affordable, gray out when not

#### 12. Shop Screen
```
┌─────────────────────────────────────────────────────────────────┐
│ [← Back]  Shop                                                  │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Daily Deals (refresh in 4h 23m):                              │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐            │
│  │ 100 Coins   │  │ 50 XP Boost │  │ Chef Skin   │            │
│  │ 📺 Watch Ad │  │  25 Coins   │  │  100 Gems   │            │
│  │  [Free!]    │  │  [Buy]      │  │  [Buy]      │            │
│  └─────────────┘  └─────────────┘  └─────────────┘            │
│                                                                 │
│  Special Offers:                                                │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │ 🎁 STARTER PACK                                         │   │
│  │ 1,000 Coins + Unlock Marco + Exclusive Skin             │   │
│  │ $4.99 (was $9.99)  [Buy]                                │   │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                 │
│  Coin Packs:                                                    │
│  [500: $0.99]  [1,200: $1.99]  [3,000: $4.99]  [8,000: $9.99] │
│                                                                 │
│  Chef Unlocks:                                                  │
│  [Marco: 500 Coins]  [Yuki: 500 Coins]  [Gustavo: 2,000]      │
│                                                                 │
├─────────────────────────────────────────────────────────────────┤
│ [👨‍🍳 Chefs]  [🛒 Shop]  [🎁 Events]  [📊 Stats]              │
└─────────────────────────────────────────────────────────────────┘
```

**Animations:**
- Daily deal cards: rotate/flip when refreshing
- Special offer banner: soft shadow pulse
- Buy buttons: scale down 0.95x + haptic feedback + coin fly animation

**UI Style:**
- **Flat colors only** (no gradients, UI Toolkit-supported styles)
- **Soft shadows** (subtle depth, still flat colors)
- **Bright colorful palette** (high saturation, Overcooked-style)

### UI Toolkit Premium Animations

**Library:** Custom UI animation system (no DOTween dependency for UI)

**Core Animations:**
```csharp
// Assets/Game/UI/Animations/UIAnimation.cs
public static class UIAnimation
{
    // Fade
    public static void FadeIn(VisualElement element, float duration = 0.3f)
    public static void FadeOut(VisualElement element, float duration = 0.3f)

    // Scale
    public static void ScaleBounce(VisualElement element, float duration = 0.3f)
    public static void ScalePulse(VisualElement element, float duration = 1.0f)

    // Slide
    public static void SlideInFromRight(VisualElement element, float duration = 0.3f)
    public static void SlideInFromBottom(VisualElement element, float duration = 0.3f)

    // Stagger
    public static void StaggerChildren(VisualElement parent, float delay = 0.1f)

    // Glow
    public static void GlowPulse(VisualElement element, float duration = 1.0f)
}
```

**Example Usage:**
```csharp
// Main menu screen
public class MainMenuScreen : BaseUIScreen
{
    protected override void OnShow()
    {
        var playButton = Root.Q<Button>("play-button");
        UIAnimation.ScalePulse(playButton, 1.0f); // Loop

        var dailyRewards = Root.Q<VisualElement>("daily-rewards");
        UIAnimation.ScaleBounce(dailyRewards, 0.3f); // Once

        var tabButtons = Root.Q<VisualElement>("tab-bar");
        UIAnimation.StaggerChildren(tabButtons, 0.1f);
    }
}
```

---

## Economy & Progression

### Coin Economy (Brawl Stars Model)

**Upgrade Costs (Level 1 → 10):**

| Level | Coins | Cumulative |
|-------|-------|------------|
| 1→2   | 100   | 100        |
| 2→3   | 200   | 300        |
| 3→4   | 400   | 700        |
| 4→5   | 700   | 1,400      |
| 5→6   | 1,100 | 2,500      |
| 6→7   | 1,700 | 4,200      |
| 7→8   | 2,600 | 6,800      |
| 8→9   | 4,000 | 10,800     |
| 9→10  | 6,200 | 17,000     |
| **TOTAL** | **17,000 coins** | — |

**Formula:** Each level costs ~1.6x the previous level (exponential curve)

### Coin Sources

**Per Match:**
- Win: 50 coins
- Loss: 20 coins
- Per recipe completed: 5 coins
- Perfect recipe (no burns): +10 bonus

**Daily Income (Casual, 5 matches):**
- 3 wins × 50 = 150
- 2 losses × 20 = 40
- 40 recipes × 5 = 200
- **Total: ~390 coins/day**

**Coins are earned only, never lost** (no penalty for losing matches beyond lower rewards)

### Progression Pacing

- **Max 1 chef (17,000 coins):** ~43 days casual (aggressive but achievable)
- **Unlock all 6 chefs (~8,000 coins):** ~20 days
- **Full completion (max all chefs):** ~60 days (2 months, good retention target)

### Chef Unlocks

| Chef | Rarity | Unlock Cost |
|------|--------|-------------|
| Gordon | Common | Starter (free) |
| Julia | Common | Starter (free) |
| Marco | Rare | 500 coins |
| Yuki | Rare | 500 coins |
| Gustavo | Epic | 2,000 coins |
| Remy | Legendary | 5,000 coins |

---

## Monetization

### IAP (In-App Purchases)

**Coin Packs:**
- 500 coins: $0.99
- 1,200 coins: $1.99
- 3,000 coins: $4.99
- 8,000 coins: $9.99

**Chef Unlocks (alternative to coins):**
- Marco: $0.99 (or 500 coins)
- Yuki: $0.99 (or 500 coins)
- Gustavo: $2.99 (or 2,000 coins)
- Remy: $4.99 (or 5,000 coins)

**Starter Pack:**
- 1,000 coins + Unlock Marco + Exclusive Skin: $4.99 (was $9.99)

**Cosmetics (Chef Skins):**
- Common skins: 100 gems
- Rare skins: 250 gems
- Epic skins: 500 gems
- Legendary skins: 1,000 gems

### Ads (Rewarded Videos)

**Placement:**
- Main menu: Watch ad for 100 coins (daily limit: 3)
- Results screen: Watch ad for 2x coins (daily limit: 5)
- Shop: Watch ad for free daily deal (daily limit: 1)

**Interstitials:**
- After every 3rd match (non-intrusive, skippable after 5s)

### No Pay-to-Win

**Utility stats only** — no chop speed, cook time, or other gameplay-affecting upgrades

---

## Audio Design

### Audio Architecture (Event-Driven)

**Pattern:** Gameplay publishes events → AudioSystem subscribes and plays sounds

**Example:**
```csharp
// Gameplay publishes event
_eventBus.Publish(new IngredientChoppedEvent(ingredientId));

// AudioSystem subscribes and plays sound
public class AudioSystem
{
    public void Initialize(IEventBus bus)
    {
        bus.Subscribe<IngredientChoppedEvent>(OnChopped);
    }

    private void OnChopped(IngredientChoppedEvent e)
    {
        _audioService.Play(SfxId.KnifeChop);
    }
}
```

### Sound Effects

| Event | Sound ID | Description |
|-------|----------|-------------|
| IngredientChoppedEvent | KnifeChop | Knife cutting vegetable |
| IngredientCookedEvent | CookingDone | Stove ding |
| IngredientBurntEvent | Burning | Fire sizzle |
| RecipeServedEvent | RecipeComplete | Success chime |
| MatchWonEvent | Victory | Triumphant fanfare |
| MatchLostEvent | Defeat | Sad trombone |
| ButtonPressedEvent | ButtonClick | UI click |
| CoinEarnedEvent | CoinCollect | Coin pickup |

### Music

- **Main menu:** Upbeat cooking theme (loop)
- **Match:** Energetic cooking music (loop, fades out when timer < 30s)
- **Victory:** Triumphant fanfare (one-shot)
- **Defeat:** Sad trombone (one-shot)

### Audio Mixer

**Groups:**
- **Master** (volume: 1.0)
  - **Music** (volume: 0.7)
  - **SFX** (volume: 1.0)
  - **UI** (volume: 0.8)

---

## Implementation Plan

### Phase 0: Foundation (Week 1)

**Tasks:**
1. Create folder structure (Assets/Game/, Assets/Playcenter/)
2. Implement core services:
   - CompositionRoot (manual DI)
   - EventBus (custom pub/sub)
   - AudioService (Unity Audio Mixer)
   - SaveService (local persistence)
   - ConfigService (Firebase Remote Config)
   - LoggingService (debug logging)
   - TimeService (game clock)
   - SceneLoader (Addressables)
   - InputService (dual-stick)
   - StateMachine (game flow)
3. Create test scene to verify foundation

**Deliverable:** Test scene with all core services working

### Slice 1: Core Gameplay Loop (Week 2-3)

**Tasks:**
1. Implement Player (movement, interaction)
2. Implement Stations:
   - IngredientCrate (spawn ingredients)
   - CuttingStation (tap-burst chop)
   - CookingStation (autonomous cook, burn)
   - PlateStation (take plate, arrange ingredients)
   - CounterStation (temporary storage)
   - ServingStation (deliver recipes)
3. Implement Recipes (ScriptableObjects, catalog)
4. Implement Cooking Controller (chop, cook, serve logic)
5. Implement Plate Controller (arrange ingredients on plate)
6. Implement Match Controller (score, win condition)
7. Create Tutorial Map (guided steps, forced on first launch)
8. Create test kitchen scene
9. Playtest and iterate

**Deliverable:** Playable single-player prototype with tutorial (fetch → chop → cook → plate → serve → score)

### Slice 2: Multiplayer (Week 4)

**Tasks:**
1. Integrate NGO (NetworkManager, NetworkBehaviour)
2. Implement NetworkPlayer
3. Implement NetworkStation
4. Implement NetworkMatch
5. Integrate EOS (lobby, matchmaking)
6. Implement EOS Cloud Storage for save data
7. Test 2v2 and 3v3 matches

**Deliverable:** Playable multiplayer prototype (2v2, 3v3) with cloud save

### Slice 3: Bots (Week 5)

**Tasks:**
1. Implement BotController
2. Implement task evaluators (fetch, chop, cook, serve)
3. Implement adaptive difficulty (match player skill)
4. Test bot matches

**Deliverable:** Playable bot matches (solo vs bots)

### Slice 4: Progression (Week 6)

**Tasks:**
1. Implement Chef system (definitions, unlock, upgrade)
2. Implement Wallet (coin management)
3. Implement Trophy system (win +15, loss -8)
4. Implement Persistence (save/load chef progress via EOS Cloud Storage)
5. Test progression flow

**Deliverable:** Working progression system (unlock chefs, upgrade stats, earn coins/trophies)

### Slice 5: Monetization (Week 7)

**Tasks:**
1. Integrate Unity IAP (coin packs, chef unlocks, starter pack)
2. Integrate AdMob (rewarded videos, interstitials)
3. Implement Cosmetics (chef skins, kitchen themes)
4. Test monetization flow

**Deliverable:** Working monetization (IAP, ads, cosmetics)

### Polish (Week 8+)

**Tasks:**
1. UI polish (animations, transitions, premium feel)
2. Audio polish (mixing, effects, music)
3. VFX (particles, shaders)
4. 3D character models (main menu, lobby, chef select)
5. Friends system (Unity Gaming Services)
6. 4-5 themed maps (Beach, Forest, Boat, etc.) with dynamic elements
7. Performance optimization
8. Bug fixing

**Deliverable:** Production-ready game with 4-5 maps, premium UI, 3D characters

---

## Wiki Structure

```
wiki/
├── index.md                    # Table of contents
├── GameDesign.md               # Core loop, recipes, chefs, progression, match rules
├── Technical.md                # Architecture, DI, events, networking, assemblies
├── Characters.md               # Chef roster, stats, upgrades, unlock costs
├── Maps.md                     # Kitchen layouts, station placement, counter usage
├── UI-UX.md                    # All screens, flows, wireframes, Brawl Stars patterns
├── Screens/
│   ├── Login.md                # Login screen flow, auth providers
│   ├── MainMenu.md             # Main menu layout, 3D character, navigation
│   ├── ChefSelect.md           # Pre-match chef selection
│   ├── Matchmaking.md          # Matchmaking UX, tips, cancel
│   ├── HUD.md                  # In-match HUD, recipe list, timer
│   ├── Results.md              # Post-match rewards, XP, coins
│   ├── Chefs.md                # Chef collection, upgrade, skins
│   ├── Shop.md                 # IAP, coin packs, daily deals
│   ├── Friends.md              # Friends list, invite, Unity Gaming Services
│   └── Settings.md             # Settings, controls, audio, account
├── Monetization.md             # IAP strategy, ad placement, cosmetics
├── Analytics.md                # Events, funnels, KPIs, retention metrics
├── Audio.md                    # Audio architecture, sound design, mixing
├── Economy.md                  # Coin economy, progression pacing, Brawl Stars model
├── LLM-Rules.md                # Forbidden patterns, arch laws, checklists
└── DRIFT-PROTOCOL.md           # Drift warning format, severity levels
```

---

## Cleanup Strategy

### Old Codebase Removal (After New Code is Working)

**Delete:**
- `Assets/_KitchenClash/` (380 files, ~36.7k LOC)
- `Assets/Scripts/Tests/` (4.5k LOC)
- `MockTests.csproj`
- `CODEBASE_ANALYSIS.md`
- `ANALYSIS_INDEX.md`
- `QUICK_REFERENCE.md`
- `KitchenClash_GDD_v3.md`
- `_bmad/`, `_bmad-output/`
- `conductor/` (old workflow)
- `docs/release/`
- `Documentation/` (old GDD, level design)
- `results.xml`
- `test_log.txt`
- `etc/`
- Old wiki (`wiki/` — after new wiki is complete)

**Keep:**
- `Assets/Playcenter/` (rebuilt from scratch)
- `Assets/Game/` (new game code)
- New wiki (`wiki/` — rebuilt from scratch)
- `conductor/` (new workflow, if needed)
- `docs/superpowers/` (specs, plans)

---

## Open Questions

1. **Retention mechanics:** Daily rewards, battle pass, events (research Brawl Stars' retention strategy)
2. **Matchmaking algorithm:** ELO, skill-based matchmaking (research how Brawl Stars matches players)
3. **Onboarding flow:** First-time user experience, tutorial (research Brawl Stars' FTUE)
4. **Analytics events:** Define key events (match started, match completed, chef upgraded, coin earned, etc.)

---

## Next Steps

1. **Review this design doc** — Confirm all decisions are correct
2. **Write the new wiki** — Create all wiki pages based on this design
3. **Start Phase 0** — Implement foundation (CompositionRoot, EventBus, Audio, Save, Config, Logging, Time, SceneLoader, Input, StateMachine)
4. **Delete old codebase** — After new code is working

---

**Status:** Ready for implementation
