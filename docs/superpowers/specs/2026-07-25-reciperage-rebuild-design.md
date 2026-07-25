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
3. **Fair competition** — Mirrored kitchens, same recipe list, utility-only upgrades
4. **Mobile-first** — Dual-stick controls, landscape UI, 2-5 minute sessions

**Target Audience:** Casual to mid-core mobile gamers (Brawl Stars, Overcooked fans)

**Platform:** iOS/Android (mobile now, PC later)

**Monetization:** IAP (chefs, coins, cosmetics) + Ads (rewarded videos, interstitials)

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

**Recipe Difficulty:**
- **Easy (T1):** 2 ingredients, 8 taps chop, 12s cook, 5s burn grace → 50 points
- **Medium (T2):** 3 ingredients, 10 taps chop, 15s cook, 4s burn grace → 150 points
- **Hard (T3):** 3 ingredients, 12 taps chop, 18s cook, 3s burn grace → 300 points

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

### Station Layout (Per Team)

```
┌─────────────────────────────────────────────────────────────┐
│ TEAM KITCHEN (Top-Down View)                               │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  [Ingredient Crate 1]  [Ingredient Crate 2]                │
│                                                             │
│  [Cutting Board 1]     [Cutting Board 2]                   │
│                                                             │
│  [Counter/Prep Table]  (holds 2 items, temporary storage)  │
│                                                             │
│  [Stove 1]             [Stove 2]                           │
│                                                             │
│  [Serving Counter]                                          │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

**Station Types:**
- **Ingredient Crate (2x):** Spawn raw ingredients (instant pickup)
- **Cutting Board (2x):** Chop ingredients (tap-burst minigame)
- **Counter/Prep Table (1x):** Temporary storage (holds 2 items, no processing)
- **Stove (2x):** Cook ingredients (autonomous, burn risk)
- **Serving Counter (1x):** Deliver completed recipes (instant)

### Chef System

**6 Chefs** with rarity tiers and utility-only stats:

| Chef | Rarity | Unlock Cost | Passive Bonus |
|------|--------|-------------|---------------|
| **Gordon** | Common | Starter | +5% movement speed |
| **Julia** | Common | Starter | +5% interaction range |
| **Marco** | Rare | 500 coins | +10% carry capacity |
| **Yuki** | Rare | 500 coins | +10% burn grace |
| **Gustavo** | Epic | 2,000 coins | +15% movement when carrying |
| **Remy** | Legendary | 5,000 coins | +20% interaction range |

**Upgradeable Stats (Level 1 → 10):**
- **Movement Speed:** 5.0 m/s → 6.5 m/s (+30%, +0.15 per level)
- **Interaction Range:** 2.0m → 2.6m (+30%, +0.06m per level)
- **Carry Capacity:** 2 items → 3 items (+1 at L5, +1 at L10)
- **Burn Grace Window:** 5.0s → 6.5s (+30%, +0.15s per level)

**NOT Upgradeable (Skill-Based):**
- Chop speed (pure tapping skill)
- Cook time (station property, not chef)

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
- Core services (DI, EventBus, Audio, Save, Config, Logging, Time, SceneLoading, Input, StateMachine)

**Slice 1: Core Gameplay Loop (Week 2-3)**
- Player, Stations, Recipes, Cooking, Match (single-player, no networking)

**Slice 2: Multiplayer (Week 4)**
- NGO + EOS integration, network sync, lobby/matchmaking

**Slice 3: Bots (Week 5)**
- BotController, task evaluators, adaptive difficulty

**Slice 4: Progression (Week 6)**
- Chef unlock/upgrade, wallet, persistence

**Slice 5: Monetization (Week 7)**
- IAP, ads, cosmetics

**Polish (Week 8+)**
- UI animations, audio mixing, VFX, optimization

### Folder Structure

```
Assets/
├── Game/                           # RecipeRage game code
│   ├── Core/                       # Foundation (Phase 0)
│   │   ├── DI/                     # CompositionRoot, ServiceLocator
│   │   ├── Events/                 # IEventBus, EventBus (custom pub/sub)
│   │   ├── Audio/                  # IAudioService, UnityAudioService
│   │   ├── Save/                   # ISaveService, LocalSaveService
│   │   ├── Config/                 # IConfigService, FirebaseConfigService
│   │   ├── Time/                   # ITimeService, UnityTimeService
│   │   ├── Logging/                # ILoggingService, UnityLoggingService
│   │   ├── StateMachine/           # IGameState, GameStateMachine
│   │   ├── SceneLoading/           # ISceneLoader, AddressablesSceneLoader
│   │   └── Input/                  # IInputService, DualStickInputService
│   │
│   ├── Gameplay/                   # Core gameplay (Slice 1)
│   │   ├── Player/                 # PlayerController, PlayerMovement
│   │   ├── Ingredient/             # IngredientItem, IngredientType
│   │   ├── Station/                # StationBase, CuttingStation, CookingStation, ServingStation, CounterStation
│   │   ├── Recipe/                 # Recipe, RecipeCatalog, RecipeDefinition
│   │   ├── Cooking/                # CookingController, ChopController, ServeController
│   │   └── Match/                  # MatchController, ScoreTracker, WinCondition
│   │
│   ├── Network/                    # Multiplayer (Slice 2)
│   │   ├── NetworkPlayer.cs        # NGO NetworkBehaviour wrapper
│   │   ├── NetworkStation.cs       # Station network sync
│   │   ├── NetworkMatch.cs         # Match state sync
│   │   └── EOSLobby.cs             # EOS lobby/matchmaking
│   │
│   ├── Bots/                       # Bot AI (Slice 3)
│   │   ├── BotController.cs        # Bot player controller
│   │   ├── BotBrain.cs             # Task planner integration
│   │   ├── Evaluators/             # FetchEvaluator, ChopEvaluator, CookEvaluator, ServeEvaluator
│   │   └── AdaptiveDifficulty.cs   # Skill matching logic
│   │
│   ├── Progression/                # Progression (Slice 4)
│   │   ├── Chef/                   # ChefDefinition, ChefUnlock, ChefUpgrade
│   │   ├── Wallet/                 # IWallet, CoinWallet
│   │   └── Persistence/            # SaveLoadChefProgress
│   │
│   ├── Monetization/               # Monetization (Slice 5)
│   │   ├── IAP/                    # IIAPService, UnityIAPService
│   │   ├── Ads/                    # IAdsService, AdMobService
│   │   └── Cosmetics/              # ChefSkin, KitchenTheme
│   │
│   └── UI/                         # UI (Polish)
│       ├── Screens/                # MainMenuScreen, LobbyScreen, HUDScreen, ResultsScreen, ChefsScreen, ShopScreen
│       ├── Components/             # RecipeListItem, ChefCard, WalletDisplay
│       └── Animations/             # UITransition, UITween
│
├── Playcenter/                     # Playcenter SDK (rebuilt from scratch)
│   ├── Core/
│   │   ├── Boot/                   # BootSequence, IGameEntry
│   │   ├── Events/                 # IEventBus, EventBus
│   │   ├── Logging/                # ILoggingService
│   │   └── Time/                   # ITimeService
│   ├── Services/
│   │   ├── Auth/                   # IAuthService, EOSAuthService
│   │   ├── Config/                 # IConfigService, FirebaseConfigService
│   │   ├── Storage/                # IStorageService, EOSStorageService
│   │   ├── Analytics/              # IAnalyticsService, FirebaseAnalyticsService
│   │   ├── Ads/                    # IAdsService, AdMobService
│   │   ├── IAP/                    # IIAPService, UnityIAPService
│   │   └── Friends/                # IFriendsService, UnityGamingServicesFriends
│   ├── UI/
│   │   ├── IUIService/             # Screen stack management
│   │   └── UIToolkit/              # BaseUIScreen, UIScreenAttribute
│   └── Net/
│       ├── INetService/            # Network abstraction
│       └── EOS/                    # EOS transport, lobby, matchmaking
│
└── Art/                            # Art assets
    ├── Characters/                 # 3D chef models (for main menu)
    ├── Kitchens/                   # 2D kitchen sprites
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

### Dependency Injection (Manual Composition Root)

**Example:**
```csharp
// Assets/Game/Core/DI/CompositionRoot.cs
public sealed class CompositionRoot : MonoBehaviour
{
    private void Awake()
    {
        // Core services
        var eventBus = new EventBus();
        var audioService = new UnityAudioService(audioMixer);
        var saveService = new LocalSaveService();
        var configService = new FirebaseConfigService();
        var loggingService = new UnityLoggingService();
        var timeService = new UnityTimeService();
        var sceneLoader = new AddressablesSceneLoader();
        var inputService = new DualStickInputService();
        var stateMachine = new GameStateMachine();

        // Gameplay services
        var recipeCatalog = new RecipeCatalog(recipeScriptableObjects);
        var cookingController = new CookingController(eventBus, configService);
        var matchController = new MatchController(eventBus, recipeCatalog);

        // Audio system (event-driven)
        var audioSystem = new AudioSystem(audioService);
        audioSystem.Initialize(eventBus);

        // Register in service locator
        ServiceLocator.Register<IEventBus>(eventBus);
        ServiceLocator.Register<IAudioService>(audioService);
        ServiceLocator.Register<ISaveService>(saveService);
        ServiceLocator.Register<IConfigService>(configService);
        ServiceLocator.Register<ILoggingService>(loggingService);
        ServiceLocator.Register<ITimeService>(timeService);
        ServiceLocator.Register<ISceneLoader>(sceneLoader);
        ServiceLocator.Register<IInputService>(inputService);
        ServiceLocator.Register<IGameStateMachine>(stateMachine);
        ServiceLocator.Register<IRecipeCatalog>(recipeCatalog);
        ServiceLocator.Register<ICookingController>(cookingController);
        ServiceLocator.Register<IMatchController>(matchController);
    }
}
```

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

### Screen Flow

```
Boot → Login → Main Menu → Game Mode Select → Chef Select → Matchmaking → Match → Results
                     ↓
              [Chefs] [Shop] [Events] [Settings] [Friends]
```

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
│            [🔵 Sign in with Epic Games]                         │
│            [⚫ Sign in with Apple]                              │
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

#### 5. Chef Select (Pre-Match)
```
┌─────────────────────────────────────────────────────────────────┐
│ [← Back]  Select Your Chef              Team: 2/2 players      │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Your Team:                    Enemy Team:                      │
│  [Player1: Gordon L5]          [???]                            │
│  [You: Select Chef ▼]          [???]                            │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │              [3D Chef Model - Rotating]                 │   │
│  │                                                         │   │
│  │  Gordon                                                 │   │
│  │  Level 5/10                                             │   │
│  │  Speed: ████████░░ 5.6 m/s                              │   │
│  │  Range: ██████░░░░ 2.2m                                 │   │
│  │  Carry: ████░░░░░░ 2 items                              │   │
│  │                                                         │   │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                 │
│  Chef Grid (scrollable horizontal):                            │
│  [Gordon] [Julia] [Marco] [Yuki] [Gustavo] [Remy]            │
│    L5       L3      L7      L1      L2        L0🔒            │
│                                                                 │
│  [Ready!]                                                       │
└─────────────────────────────────────────────────────────────────┘
```

**Animations:**
- 3D chef model rotates when selected
- Chef cards: horizontal scroll with snap-to-center
- Selected card: scale up 1.1x with glow
- Ready button: pulse when all players ready

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

#### 7. In-Match HUD (Landscape, Minimal)
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

#### 8. Results Screen
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
│  Rewards:                                                       │
│  +50 Coins (Win)                                                │
│  +40 Coins (8 recipes × 5)                                      │
│  +10 Coins (Perfect recipes bonus)                              │
│  ─────────────────────                                          │
│  Total: +100 Coins                                              │
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
- Rewards: count up with coin fly animation
- XP bar: fills smoothly with glow pulse
- Buttons: slide in from bottom with stagger

#### 9. Chefs Screen (Collection)
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
│  │                  │  Stats:                                  │
│  │                  │  Speed: ████████░░ 5.6 m/s (+12%)        │
│  │                  │  Range: ██████░░░░ 2.2m (+10%)           │
│  │                  │  Carry: ████░░░░░░ 2 items               │
│  │                  │  Burn:  ███████░░░ 5.5s (+10%)           │
│  │                  │                                          │
│  │                  │  [Upgrade to L6: 1,100 Coins]            │
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

#### 10. Shop Screen
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
- Special offer banner: shimmer effect
- Buy buttons: scale down 0.95x + haptic feedback + coin fly animation

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
   - CounterStation (temporary storage)
   - ServingStation (deliver recipes)
3. Implement Recipes (ScriptableObjects, catalog)
4. Implement Cooking Controller (chop, cook, serve logic)
5. Implement Match Controller (score, win condition)
6. Create test kitchen scene
7. Playtest and iterate

**Deliverable:** Playable single-player prototype (fetch → chop → cook → serve → score)

### Slice 2: Multiplayer (Week 4)

**Tasks:**
1. Integrate NGO (NetworkManager, NetworkBehaviour)
2. Implement NetworkPlayer
3. Implement NetworkStation
4. Implement NetworkMatch
5. Integrate EOS (lobby, matchmaking)
6. Test 2v2 and 3v3 matches

**Deliverable:** Playable multiplayer prototype (2v2, 3v3)

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
3. Implement Persistence (save/load chef progress)
4. Test progression flow

**Deliverable:** Working progression system (unlock chefs, upgrade stats, earn coins)

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
4. 3D character models (main menu, chef select)
5. Friends system (Unity Gaming Services)
6. Performance optimization
7. Bug fixing

**Deliverable:** Production-ready game

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
