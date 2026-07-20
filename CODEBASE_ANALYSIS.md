# RecipeRage Codebase Analysis

> **Secondary doc:** For architecture truth prefer `wiki/` and
> `Documentation/Architecture/PROJECT_MEMORY.md`. This file may lag.
> Current DI scopes: `RootLifetimeScope` → `MenuLifetimeScope` → `MatchLifetimeScope`
> under `Assets/_KitchenClash/Composition/`. Engine: Unity 6.0. Navigation: `UIService`.

## Executive Summary

RecipeRage is a Unity 6.0 multiplayer cooking-competition game (Kitchen Brawler v2 direction) built on a modern, layered architecture with strong separation of concerns. The codebase uses **VContainer** for dependency injection, **Netcode for GameObjects (NGO)** for multiplayer networking, **EOS** for services/authentication, and **UI Toolkit** for UI. It follows a **strict Two-Bucket Assembly Architecture** (Core/Gameplay layers) and is **state-driven** throughout.

---

## 1. Build System & Commands

### Project Type
- **Engine:** Unity 6.0 (6000.3.0f1)
- **Language:** C# (.NET 4.7.1)
- **Build Format:** MSBuild project files (.csproj)

### Build Commands

#### Single Csproj Build
```bash
dotnet build <ProjectName>.csproj -nologo
# Example: dotnet build RecipeRage.Gameplay.csproj -nologo
```

#### Full Project Build (from Unity Editor)
- File → Build Settings → Build (standard Unity build)
- Build scenes enabled: `Bootstrap`, `MainMenu`, `Game`

### Key Project Files
- **Root DI:** `RootLifetimeScope.cs`
- **Match DI:** `MatchLifetimeScope.cs`
- **Menu DI:** `MenuLifetimeScope.cs`
- **Game Bootstrapper:** `GameBootstrapper.cs`

### Build Configuration
- **MSBuild** projects generated and managed by Unity Rider/Visual Studio
- Build outputs to `Temp\Bin\Debug\` or `Temp\Bin\Release\`
- No custom Makefile; uses dotnet CLI or Unity Build Pipeline

---

## 2. Testing Framework & Commands

### Test Frameworks
- **Framework:** NUnit (Unity Test Framework)
- **Test Projects:**
  - `RecipeRage.Tests.EditMode.csproj` - Unit tests
  - `KitchenClash.Tests.PlayMode.csproj` - Play mode tests

### Test Location & Structure
```
Assets/Scripts/Tests/EditMode/
  ├── Gameplay/
  │   ├── ScoreServiceTests.cs
  │   ├── MatchServiceTests.cs
  │   ├── OrderServiceTests.cs
  │   ├── BotClaimRegistryTests.cs
  │   ├── BotTaskPlannerTests.cs
  │   ├── MatchEndEvaluatorTests.cs
  │   ├── MatchmakingFlowTests.cs
  │   └── Persistence/
  └── Core/
      └── Auth/
          └── AuthenticationServiceTests.cs

Assets/_KitchenClash/Tests/PlayMode/
  └── SmokeTest.cs
```

### Run Tests

#### All EditMode Tests
```bash
dotnet test RecipeRage.Tests.EditMode.csproj --no-build -nologo
```

#### Single Test File (via Unity Test Runner or CLI)
```bash
# Via CLI (requires test project file path)
dotnet test RecipeRage.Tests.EditMode.csproj --filter="ClassName.MethodName" --no-build -nologo

# Via Unity Editor: Window → Testing → Test Runner → Click test or folder → Run
```

#### CI Mode (Non-Interactive)
```bash
CI=true dotnet test RecipeRage.Tests.EditMode.csproj --no-build -nologo
```

### Test Pattern
```csharp
public class ServiceTests
{
    [SetUp]
    public void SetUp() { /* initialize test doubles */ }
    
    [Test]
    public void MethodName_Condition_ExpectedResult() { /* assertions */ }
}
```

**Key Testing Patterns:**
- Spy/mock pattern with `SpyEventBus`, `DictionaryConfigService`
- Dependency injection of test doubles
- SetUp/TearDown lifecycle
- Test naming: `Method_Condition_Result`

### Code Coverage
- Tool: Unity Test Framework Code Coverage
- Target: >80% for new code
- Run coverage: Enabled via `com.unity.testtools.codecoverage` package

---

## 3. Linting & Formatting Tools

### EditorConfig
**File:** `.editorconfig`

**Rules:**
- **Indentation:** 4 spaces for C#
- **Line endings:** CRLF for .cs files
- **Charset:** UTF-8
- **Insert final newline:** Required

**C# Specific Rules:**
```
- No `this.` qualification for fields/properties/methods (warning)
- var only for obvious types (warning)
- Explicit accessibility modifiers (warning)
- Readonly fields preferred (suggestion)
```

### Static Analysis & Linting
- **Built-in:** Unity Code Analyzer (via Unity Editor)
- **Editor config enforcement:** IDE-based (Rider, VS, Zed support EditorConfig)
- **No explicit linting tool** configured (e.g., no Roslyn analyzers explicitly configured in csproj)

### Formatting Commands
```bash
# No built-in format command; use IDE's Format Document:
# In Rider/VS: Code → Reformat Code
# In VS Code/Zed: Format Document (Ctrl+Shift+I)

# EditorConfig enforces style on save in most IDEs
```

### Quality Gates (Pre-Commit)
From `conductor/workflow.md`:
- [ ] All tests pass
- [ ] Code coverage >80%
- [ ] No linting errors
- [ ] Code follows project style guide (`code_styleguides/`)
- [ ] All public functions documented
- [ ] Type safety enforced
- [ ] No security vulnerabilities

---

## 4. High-Level Architecture

### Overall Design Pattern

**Two-Bucket Assembly Architecture:**
1. **Domain/Application Layer** (Business logic, interfaces, domain events)
2. **Infrastructure/Presentation Layer** (Unity, EOS, UI Toolkit, networking specifics)

**State-Driven Gameplay:**
```
BootstrapState 
  → LoginState/MainMenuState 
  → MatchmakingState 
  → GameplayState 
  → GameOverState
```

### Major Subsystems

#### 1. **Dependency Injection (VContainer)**
- **Root Scope:** `RootLifetimeScope` (app-level services)
- **Menu Scope:** `MenuLifetimeScope` (menu and logged-in player services)
- **Match Scope:** `MatchLifetimeScope` (per-match services)

#### 2. **State Machine & State Flow**
- **Interface:** `IState` (Enter/Update/Exit/FixedUpdate)
- **Base Class:** `BaseState` (cancellation token, logging, state lifecycle)
- **Manager:** `IGameStateManager` (changes between states)
- **States:** All live in `Infrastructure/States/` namespace

#### 3. **Networking (NGO + EOS)**
- **Transport:** Netcode for GameObjects (NGO)
- **Authentication:** Epic Online Services (EOS) Device ID auth
- **Services:** EOS Friends, P2P, Match Lobby
- **Network Manager:** Root-owned singleton, injected into services
- **Network Objects:** Spawned through `SpawnManager`, pooled via `INetworkObjectPool`

#### 4. **Event Bus (Publisher/Subscriber)**
- **Interface:** `IEventBus` (Subscribe/Unsubscribe/Publish/ClearAllSubscriptions)
- **Implementation:** `EventBus` (singleton, root-owned)
- **Pattern:** Generic type-safe events
- **Usage:** Decoupling systems (UI, gameplay, score events)

#### 5. **Gameplay Runtime Bridge**
- **MatchContext:** Holds references to active-scene gameplay objects
- **MatchRuntimeSceneBinder:** Discovers scene MonoBehaviours and registers them
- **Scene Objects Exposed:** OrderManager, ScoreManager, RoundTimer, SpawnManager, PlayerController, IngredientNetworkSpawner

#### 6. **UI System (UI Toolkit)**
- **UIService:** Root-owned singleton managing screen stacks
- **UIScreenStackManager:** Category-based screen stack
- **Base Class:** `BaseUIScreen`
- **Attribute:** `[UIScreen]` for automatic registration
- **Pattern:** Screen types auto-discovered via reflection during root DI setup

#### 7. **Services (Cross-Cutting Concerns)**
- **Authentication:** `IAuthService` (EOS Device ID flow)
- **Player Data:** `IPlayerDataService` (root-owned, exposed to session)
- **Economy:** `IEconomyService` (root-owned)
- **Matchmaking:** `IMatchmakingService` (session-owned)
- **Logging:** `ILoggingService` (custom, root-owned)
- **Config:** `IConfigService` / `IRemoteConfigService` (Firebase optional)
- **Analytics:** `IAnalyticsService` (Firebase-based)
- **Persistence:** `ISaveService`, `StorageProviderFactory`

#### 8. **Gameplay Systems (Match-Scoped)**
- **Score:** `IScoreService` (match-scoped)
- **Orders:** `IOrderService` (match-scoped)
- **Abilities:** `IAbilityService`, `AbilityFactory` (match-scoped)
- **Hazards:** `IHazardService` (match-scoped)
- **Bot AI:** `BotManager`, `BotClaimRegistry`, `BotTaskPlanner` (match-scoped)

#### 9. **Player Controller & Input**
- **PlayerController:** NetworkBehaviour registering with `PlayerNetworkManager`
- **PlayerMovementController:** Handles mobile/keyboard input
- **PlayerInteractionController:** Interaction state and interaction targeting
- **Input System:** Unity New Input System (configured in Packages)

#### 10. **Match Lifecycle Management**
- **MatchEndController:** Owns round start, score limit, timer expiry → writes `MatchResultSync`
- **GamePhaseSync:** Synced state (Pre-Game, Active, GameOver)
- **MatchResultSync:** Final winner/draw result
- **RoundTimer:** Time-based round progression

---

## 5. Key Interfaces & Service Patterns

### DI Container Setup Pattern
```csharp
public class RootLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<ServiceImpl>(Lifetime.Singleton)
            .As<IServiceInterface>();
    }
}
```

### Service Interface Pattern
```csharp
public interface IMyService
{
    void DoSomething();
    TResult Query();
}

public class MyService : IMyService
{
    private readonly IDependency _dep;
    
    public MyService(IDependency dep) // VContainer injects
    {
        _dep = dep;
    }
    
    public void DoSomething() { /* impl */ }
}
```

### Event Bus Pattern
```csharp
// Publishing
_eventBus.Publish(new ScoreEvent(teamId, points));

// Subscribing
_eventBus.Subscribe<ScoreEvent>(evt => HandleScore(evt));

// Unsubscribing
_eventBus.Unsubscribe<ScoreEvent>(handler);
```

### State Machine Pattern
```csharp
public class MyState : BaseState
{
    public override void Enter()
    {
        base.Enter(); // Sets up cancellation token
        // Initialize
    }
    
    public override void Update()
    {
        if (shouldTransition)
            _stateManager.ChangeState<NextState>();
    }
    
    public override void Exit()
    {
        // Cleanup
        base.Exit();
    }
}
```

### Network Object Pattern
```csharp
public class MyNetworkObject : NetworkBehaviour
{
    [Rpc]
    public void MyRpc(NetworkBehaviourSerialisationStream stream)
    {
        // Serialization
    }
    
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        // Client-only or owner-only init
    }
}
```

---

## 6. Entry Points & Important Directories

### Entry Points

#### 1. **Boot Scene**
- **Path:** `Assets/Scenes/Bootstrap.unity`
- **Purpose:** Game startup, root DI initialization
- **Contains:** `GameBootstrap.prefab`

#### 2. **Bootstrap Prefab**
- **Path:** `Assets/Prefabs/General/GameBootstrap.prefab`
- **Contains:**
  - `RootLifetimeScope` (DI)
  - `NetworkManager` (NGO)
  - EOS manager
  - Root `UIDocument`

#### 3. **Game Bootstrapper**
- **Path:** `Assets/_KitchenClash/Composition/GameBootstrapper.cs`
- **Entry Point:** Registered as `IStartable` in root DI
- **Responsibility:** Creates `GameStateManager`, starts `BootstrapState`

#### 4. **Main Menu Scene**
- **Path:** `Assets/Scenes/MainMenu.unity`
- **Loaded:** During `MainMenuState`
- **Contains:** Main menu UI, lobby screen

#### 5. **Game Scene**
- **Path:** `Assets/Scenes/Game.unity`
- **Loaded:** During `GameplayState`
- **Contains:** Base game objects, additively loads map scenes
- **Additive Maps:**
  - `KitchenArena`
  - `TeamArena`
  - `FFAKitchen`

### Directory Structure

```
Assets/
├── Scripts/
│   ├── Core/                    # Engine-level systems (minimal)
│   │   └── Input/
│   ├── Gameplay/                # App states, runtime, networking
│   │   ├── Bootstrap/           # Lifetime scopes
│   │   ├── App/
│   │   │   ├── State/           # State machines
│   │   │   └── Services/
│   │   ├── Shared/
│   │   │   └── MatchRuntimeSceneBinder.cs
│   │   └── Tests/
│   └── Tests/
│       └── EditMode/            # Unit tests
├── _KitchenClash/               # Main game code (namespaced)
│   ├── Application/             # Business logic, interfaces, domain events
│   │   ├── State/               # Game state interfaces
│   │   ├── Services/            # Service implementations
│   │   ├── Interfaces/          # Service interfaces
│   │   └── Models/
│   ├── Domain/                  # Domain models, events
│   │   └── Interfaces/
│   ├── Infrastructure/          # Unity/EOS/networking/persistence
│   │   ├── States/              # Concrete game states
│   │   ├── Network/             # Networking, player controllers
│   │   ├── Services/            # Infrastructure services
│   │   ├── EOS/                 # EOS integration
│   │   ├── Audio/               # Audio systems
│   │   ├── Localization/        # i18n
│   │   ├── Persistence/         # Save/load
│   │   ├── Logging/             # Custom logging
│   │   ├── Analytics/           # Analytics
│   │   ├── DI/                  # DI utilities
│   │   └── Gameplay/            # Gameplay systems
│   │       ├── Abilities/
│   │       ├── Cooking/
│   │       ├── Hazards/
│   │       └── Bot/
│   ├── Presentation/            # UI, view models, screens
│   │   ├── ViewModels/
│   │   ├── Screens/
│   │   └── Common/
│   ├── Composition/             # DI scopes
│   │   ├── RootLifetimeScope.cs
│   │   ├── MatchLifetimeScope.cs
│   │   └── MenuLifetimeScope.cs
│   ├── Data/                    # Data models, DTOs
│   ├── UI/                      # UI Toolkit templates, styles
│   ├── ScriptableObjects/       # Game data assets
│   └── Tests/                   # Play mode tests
├── Scenes/
│   ├── Bootstrap.unity
│   ├── MainMenu.unity
│   └── Game.unity
├── Prefabs/
│   ├── General/
│   │   └── GameBootstrap.prefab
│   ├── Network/
│   ├── Gameplay/
│   └── UI/
└── Resources/
    └── UI/                      # UI Toolkit resources
```

### Documentation Directories

```
Documentation/
├── README.md                                    # Index & source of truth
├── Architecture/
│   ├── PROJECT_MEMORY.md                       # Current architecture (priority)
│   ├── CURRENT_CODEBASE_AUDIT.md               # Code audit (secondary)
│   ├── GDD_ALIGNMENT_MATRIX.md                 # GDD vs implementation
│   ├── PHASE_ROADMAP.md                        # Practical roadmap
│   ├── FINAL_ARCHITECTURE.md                   # Historical
│   ├── PLAYER_CONTROLLER_ARCHITECTURE.md       # Historical
│   └── STATE_TRANSITION_FLOW.md                # Historical
├── Guides/
│   ├── gameplay-scene-setup.md                 # Scene wiring guide
│   └── [other active guides]
└── Archive/
    ├── 2026-03-cleanup/                        # Superseded docs
    └── [historical]

conductor/
├── product.md                                  # Product vision
├── tech-stack.md                               # Tech stack & decisions
├── workflow.md                                 # Development process
├── code_styleguides/                           # Coding standards
└── tracks.md                                   # Feature tracks
```

---

## 7. Dependency Injection (VContainer)

### Root Lifetime Scope

**File:** `Assets/_KitchenClash/Composition/RootLifetimeScope.cs`

**Services Registered:**
```csharp
// Core
- EventBus (Singleton) → IEventBus
- UnityLoggingService (Singleton) → ILoggingService
- EncryptionService (Singleton) → IEncryptionService
- NetworkConnectivityService (Singleton) → IConnectivityService, ITickable

// UI
- UIScreenStackManager (Singleton) → IUIScreenStackManager
- UIService (Singleton) → IUIService, IStartable, ITickable
- LocalizationManager (Singleton) → ILocalizationManager, IInitializable

// Infrastructure
- GameStateFactory (Singleton) → IStateFactory
- GameStateManager (Singleton) → IGameStateManager, ITickable
- PlayerDataService (Singleton) → IPlayerDataService
- StorageProviderFactory (Singleton)
- SaveService (Singleton) → ISaveService
- RemoteConfigService (Singleton) → IConfigService, IRemoteConfigService
- MaintenanceService (Singleton) → IMaintenanceService
- AuthenticationService (Singleton) → IAuthService

// Audio
- AudioVolumeController (Singleton) → IAudioVolumeController, IInitializable
- AudioPoolManager (Singleton)
- MusicPlayer (Singleton) → IMusicPlayer
- SFXPlayer (Singleton) → ISFXPlayer
- AudioService (Singleton) → IAudioService

// Database instances
- ChefDatabaseSO
- MapDatabaseSO
- ChefRegistry (Singleton)
- MapRegistry (Singleton)

// Auto-discovered via reflection:
- All BaseUIScreen subclasses (Transient)
- All IState implementations in KitchenClash.Infrastructure.States (Transient)

// Entry points:
- GameBootstrapper (IStartable)
- ConnectivityOverlayPresenter (IStartable)
```

**Lifetime Scope Strategy:**
- **Singleton:** Long-lived app services (auth, logging, state machine, UI)
- **Transient:** One-time use or per-request (screens, states, view models)

### Session Lifetime Scope

**File:** `Assets/_KitchenClash/Composition/MenuLifetimeScope.cs` (or integrated into session creation)

**Session-Scoped Services:**
```csharp
- INetworkingServices (via NetworkingServiceContainer)
- ILobbyManager
- IPlayerManager
- IMatchmakingService
- ITeamManager
- IGameStarter
```

**Ownership Rule:**
- Session scope does NOT own root network primitives (`INetworkObjectPool`, `INetworkGameManager`)
- Those remain root-owned and are injected where needed

### Match Lifetime Scope

**File:** `Assets/_KitchenClash/Composition/MatchLifetimeScope.cs`

**Match-Scoped Services:**
```csharp
- RecipeCatalog (Scoped)
- AbilityFactory (Scoped)
- AbilityEffectHandler (Scoped)
- ScoreService (Scoped) → IScoreService
- OrderService (Scoped) → IOrderService
- AbilityService (Scoped) → IAbilityService
- HazardService (Scoped) → IHazardService
- MatchContext (Scoped) → IMatchContext
- BotManager (Scoped)
- BotClaimRegistry (Scoped)
- BotTaskPlanner (Scoped)
- MatchConnectivityBridge (Scoped) → IStartable, IDisposable
```

### Service Resolution Order

1. VContainer resolves dependencies at construction time
2. Singleton instances cached per scope
3. Transient instances created fresh each time
4. Circular dependencies cause errors (by design)

### Accessing DI Container

```csharp
// In a LifetimeScope (auto-resolved)
public MyComponent(IDependency dep) { _dep = dep; }

// Manual resolve (rare):
var container = LifetimeScope.Find<MyContainer>();
var service = container.Container.Resolve<IMyService>();
```

---

## 8. Networking Architecture (NGO + EOS)

### Network Transport

**Framework:** Unity Netcode for GameObjects (NGO)
- **Transport:** IP-based (configurable)
- **Manager:** `NetworkManager` singleton at root
- **Lifetime:** Root-owned, persists across scenes

### Player Network Management

**PlayerNetworkManager:**
- Root-owned service tracking connected players
- Registers `PlayerController` instances when `IsPlayerObject = true`
- Used by HUD and gameplay systems to query player counts/teams

**PlayerController:**
- NetworkBehaviour inheriting from NGO
- Spawned by `SpawnManager` during gameplay setup
- Registers with `PlayerNetworkManager` on `OnNetworkSpawn()`
- Handles input serialization via `IPlayerInputData`

### Network Object Pool

**Interface:** `INetworkObjectPool`
- Root-owned service
- Pre-spawned network objects (ingredients, projectiles)
- Injected into `IngredientNetworkSpawner`

### Network Game Manager

**Interface:** `INetworkGameManager`
- Root-owned service
- Manages NGO lifecycle during gameplay
- Used by `SpawnManager` and bot spawners

### Match Context & Scene Bridge

**MatchContext:** Holds live references to scene objects
- `OrderManager`
- `ScoreManager`
- `RoundTimer`
- `SpawnManager`
- `GamePhaseSync` (synced state)
- `MatchResultSync` (synced result)

**MatchRuntimeSceneBinder:**
- Scene MonoBehaviour discovering and registering objects
- Called early in `GameplayState` initialization
- Allows app-layer code to avoid `FindObjectOfType()` repeatedly

### EOS Integration

**Services Used:**
- **Authentication:** Device ID-based login
- **Friends:** Friend list management
- **P2P:** Real-time messaging (not match data)
- **Match Lobby:** Creating/joining matches

**Configuration:** `UGSConfig` scriptable object, set in root inspector

---

## 9. State Machine Architecture

### State Interface

```csharp
public interface IState
{
    string StateName { get; }
    void Enter();
    void Exit();
    void Update();
    void FixedUpdate();
}
```

### Base State Implementation

```csharp
public abstract class BaseState : IState
{
    protected CancellationToken StateCancellationToken { get; }
    protected bool IsStateActive { get; }
    
    public virtual void Enter() { /* Setup cancellation token */ }
    public virtual void Exit() { /* Cancel token */ }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
}
```

### Game State Manager

**Interface:** `IGameStateManager`

**Methods:**
```csharp
void ChangeState<T>() where T : IState;
void Update();
void FixedUpdate();
```

**Implementation:** `GameStateFactory` creates state instances via DI

### Current Game States

```
BootstrapState
  → LoginState (if not authenticated)
  → SessionLoadingState (after auth)
  → MainMenuState (session ready)
  → MatchmakingState (user clicks Play)
  → GameplayState (match found)
  → GameOverState (match ended)
```

### State Transition Flow

```
BootstrapState.Enter()
  ├─ Splash screen (3.5s)
  ├─ NTP time sync
  ├─ Remote config download
  ├─ Auth check
  └─ Maintenance check
     ├─ → LoginState (if auth failed)
     └─ → SessionLoadingState (if auth succeeded)

LoginState (user authenticates via EOS)
  → SessionLoadingState

SessionLoadingState
  ├─ Create MenuLifetimeScope
  ├─ Sync player data
  ├─ Initialize economy
  └─ → MainMenuState

MainMenuState (load MainMenu.unity, show lobby UI)
  ├─ LobbyViewModel.Play() called
  └─ → MatchmakingState

MatchmakingState (start matchmaking service)
  ├─ Create/join EOS match lobby
  ├─ Wait for players (timeout 20-30s)
  ├─ Fill with bots if needed
  └─ → GameplayState

GameplayState
  ├─ Load Game.unity
  ├─ Load map additively (KitchenArena/TeamArena/FFAKitchen)
  ├─ GameStarter.StartGame() → spawn players
  ├─ MatchRuntimeSceneBinder registers scene objects
  └─ Gameplay runs...

MatchEndController signals end:
  ├─ Sets GamePhaseSync to GameOver
  ├─ Writes MatchResultSync
  └─ → GameOverState (after HUD detects sync)

GameOverState
  ├─ Show results screen
  ├─ Shutdown network
  └─ → MainMenuState
```

---

## 10. Runtime Verification Status

### Known Implementation Gaps

**From PROJECT_MEMORY.md (Phase 2: Runtime Verification):**

1. ✅ **State flow:** Fully implemented
2. ✅ **DI scopes:** Root, Session, Match scopes in place
3. ✅ **MatchContext:** Scene bridge implemented
4. ✅ **Networking:** NGO integration done, EOS auth ready
5. ⚠️ **Map loading:** Additive map scenes not in build settings (warns, continues)
6. ⚠️ **MatchEndController:** Implemented but not fully runtime-tested
7. ⚠️ **Score limits:** Code implemented but not runtime-verified
8. ⚠️ **Bot AI:** Planner in place but gameplay-level verification pending

### Not Yet Implemented

- **Planned Future:** Router service for message-driven architecture (post-Phase 2)
- **Planned Future:** Singleton-free networking architecture (post-Phase 3)
- **Planned Future:** Full external-provider auth flows (post-Phase 4)

---

## 11. Important Patterns & Conventions

### Naming Conventions

- **Interfaces:** `IMyService` (I prefix)
- **Implementations:** `MyService` (no special prefix, goes in Infrastructure folder)
- **Service Interfaces:** In `Domain/Interfaces/` or `Application/Interfaces/`
- **ViewModels:** `MyViewModel` suffix
- **Screens:** `MyScreen` suffix
- **States:** `MyState` suffix
- **Private fields:** `_myField` (underscore prefix)

### File Organization

```csharp
namespace KitchenClash.Domain.Interfaces
{
    public interface IMyService { }
}

namespace KitchenClash.Application.Services
{
    public class MyService : IMyService { }
}

namespace KitchenClash.Infrastructure.Services
{
    public class InfrastructureServiceImpl : IService { }
}
```

### Async Patterns

- **UniTask:** Used for async operations (preferred over Task)
- **Cancellation:** Via `CancellationToken` (states provide tokens)
- **Forget():** Called on fire-and-forget tasks to suppress warnings

```csharp
async UniTask MyMethod()
{
    await UniTask.Delay(1000, cancellationToken: StateCancellationToken);
}

MyMethod().Forget(); // Fire and forget
```

### Event Bus Usage

```csharp
// Publish
_eventBus.Publish(new PlayerJoinedEvent(playerId));

// Subscribe (typically in OnEnable or constructor)
_eventBus.Subscribe<PlayerJoinedEvent>(OnPlayerJoined);

// Unsubscribe (typically in OnDisable or destructor)
_eventBus.Unsubscribe<PlayerJoinedEvent>(OnPlayerJoined);
```

### Test Double Pattern

```csharp
public class SpyEventBus : IEventBus
{
    private List<object> _published = new();
    
    public void Publish<T>(T evt) where T : class
    {
        _published.Add(evt);
    }
    
    // Other methods...
}

[Test]
public void MyTest()
{
    var spy = new SpyEventBus();
    var svc = new MyService(spy);
    svc.DoSomething();
    Assert.Contains(typeof(MyEvent), spy.PublishedEvents.Select(e => e.GetType()));
}
```

---

## 12. Key Documentation Files (Source of Truth)

### Priority Reading Order

1. **PROJECT_MEMORY.md** (current architecture overview)
   - Location: `Documentation/Architecture/PROJECT_MEMORY.md`
   - Updated: When architecture changes
   - Scope: DI ownership, state flow, networking rules

2. **CURRENT_CODEBASE_AUDIT.md** (what exists now)
   - Location: `Documentation/Architecture/CURRENT_CODEBASE_AUDIT.md`
   - Updated: When implementation drifts from docs
   - Scope: File list, runtime flow, known risks

3. **gameplay-scene-setup.md** (scene and prefab wiring)
   - Location: `Documentation/Guides/gameplay-scene-setup.md`
   - Scope: Inspector setup, scene object relationships

4. **KitchenClash_GDD_v3.md** (current-state design)
   - Location: Root project folder
   - Scope: Game design, features, rules

5. **tech-stack.md** (framework and tool choices)
   - Location: `conductor/tech-stack.md`
   - Updates must document deviations

---

## 13. Quick Reference: Key Classes

| Class | Namespace | Purpose | Scope |
|-------|-----------|---------|-------|
| `RootLifetimeScope` | Composition | Root DI setup | Root |
| `MenuLifetimeScope` | Composition | Menu DI setup | Menu |
| `MatchLifetimeScope` | Composition | Match DI setup | Match |
| `GameBootstrapper` | Composition | Entry point | Root |
| `IGameStateManager` | Application.State | State machine driver | Root |
| `BaseState` | Application.State | State base class | All states |
| `MatchContext` | Infrastructure.Network | Scene object registry | Match |
| `MatchRuntimeSceneBinder` | Gameplay.Shared | Scene → MatchContext bridge | Match |
| `IEventBus` | Domain | Pub/sub event system | Root |
| `IUIService` | Application.Services | Screen management | Root |
| `PlayerController` | Infrastructure.Network | Player network object | Match |
| `IngredientNetworkSpawner` | Infrastructure.Network | Ingredient spawning | Match |
| `SpawnManager` | Infrastructure.Network | Player/bot spawning | Match |
| `IScoreService` | Domain | Score tracking | Match |
| `IOrderService` | Domain | Order management | Match |
| `AbilityService` | Application.Services | Ability effects | Match |

---

## 14. Common Tasks

### Adding a New Game State

1. Create `Assets/_KitchenClash/Infrastructure/States/MyState.cs`:
   ```csharp
   public class MyState : BaseState
   {
       private readonly IGameStateManager _stateManager;
       
       public MyState(IGameStateManager stateManager)
       {
           _stateManager = stateManager;
       }
       
       public override void Enter() { base.Enter(); }
       public override void Exit() { base.Exit(); }
   }
   ```

2. Auto-discovered by reflection in `RootLifetimeScope.RegisterGameStates()`

3. Transition via: `_stateManager.ChangeState<MyState>()`

### Adding a New Service

1. Create interface in `Domain/Interfaces/` or `Application/Interfaces/`
2. Create implementation in `Infrastructure/Services/`
3. Register in appropriate LifetimeScope:
   ```csharp
   builder.Register<MyService>(Lifetime.Singleton).As<IMyService>();
   ```
4. Inject via constructor in DI-managed classes

### Adding a New UI Screen

1. Create `Assets/_KitchenClash/Presentation/Screens/MyScreen.cs`:
   ```csharp
   [UIScreen]
   public class MyScreen : BaseUIScreen { }
   ```

2. Auto-discovered and registered by `RootLifetimeScope.RegisterScreens()`

3. Show via: `_uiService.ShowScreen<MyScreen>()`

### Writing a Test

1. Create file: `Assets/Scripts/Tests/EditMode/Gameplay/MyServiceTests.cs`
2. Reference assembly: Add to `RecipeRage.Tests.EditMode.asmdef` if needed
3. Write tests following NUnit + project patterns:
   ```csharp
   public class MyServiceTests
   {
       private MyService _svc;
       
       [SetUp]
       public void SetUp() { _svc = new MyService(...); }
       
       [Test]
       public void Feature_Condition_ExpectedResult() { }
   }
   ```

4. Run: `dotnet test RecipeRage.Tests.EditMode.csproj --no-build -nologo`

---

## Summary Table: Subsystems at a Glance

| System | Owner | Technology | Key Interface | Location |
|--------|-------|-----------|---|---|
| **DI** | VContainer | VContainer | IContainerBuilder | Composition/ |
| **State Machine** | Root | Custom | IGameStateManager | Infrastructure/States/ |
| **Event Bus** | Root | Custom | IEventBus | Application/Services/ |
| **Networking** | Root | NGO + EOS | NetworkManager | Infrastructure/Network/ |
| **Player Data** | Root | EOS Storage | IPlayerDataService | Application/Interfaces/ |
| **Scene Bridge** | Match | Custom | IMatchContext | Infrastructure/Network/ |
| **UI** | Root | UI Toolkit | IUIService | Application/Services/ |
| **Matchmaking** | Session | EOS Lobbies | IMatchmakingService | Application/Services/ |
| **Scoring** | Match | Custom | IScoreService | Domain/Interfaces/ |
| **Bot AI** | Match | Custom | BotManager | Infrastructure/Gameplay/Bot/ |
| **Audio** | Root | FMOD/Unity | IAudioService | Infrastructure/Audio/ |

---

## Final Notes

- **Always check current code first** when architecture docs conflict
- **Update PROJECT_MEMORY.md** when architecture changes
- **Run tests before committing** (`CI=true dotnet test ...`)
- **Use DI injection** instead of `FindObjectOfType()` or `Singleton` patterns
- **Follow state-driven flow** for major transitions
- **Publish events** instead of direct coupling between systems
- **Session scope exists** for player-session services only, not match services
