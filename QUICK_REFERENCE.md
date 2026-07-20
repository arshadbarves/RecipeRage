# RecipeRage Quick Reference

> **Secondary doc:** For architecture truth prefer `wiki/` and
> `Documentation/Architecture/PROJECT_MEMORY.md`. This file may lag.
> Current DI scopes: `RootLifetimeScope` → `MenuLifetimeScope` → `MatchLifetimeScope`
> under `Assets/_KitchenClash/Composition/`. Engine: Unity 6.0. Navigation: `UIService`.

## Build Commands
```bash
# Build single project
dotnet build RecipeRage.Gameplay.csproj -nologo

# Run all tests (EditMode)
dotnet test RecipeRage.Tests.EditMode.csproj --no-build -nologo

# Run single test
dotnet test RecipeRage.Tests.EditMode.csproj --filter="ClassName" --no-build -nologo

# CI mode (non-interactive)
CI=true dotnet test RecipeRage.Tests.EditMode.csproj --no-build -nologo
```

## Architecture Layers

```
Root App Layer (RootLifetimeScope)
├─ State Machine, Auth, Config, Logging, UI Service
├─ Root Networking (NetworkManager, INetworkObjectPool)
└─ Root Services (PlayerData, Economy, Character)

Menu Layer (MenuLifetimeScope)
├─ Matchmaking, Lobby, Team, Friends
└─ (No networking primitives here!)

Match Layer (MatchLifetimeScope)
├─ Score, Orders, Abilities, Hazards, Bot AI
└─ MatchContext + Scene Bridge

Gameplay Scene Layer
├─ PlayerController, BotController, Stations
├─ OrderManager, ScoreManager, RoundTimer
└─ SpawnManager, IngredientNetworkSpawner

UI Layer (Root-owned)
├─ UIService, UIScreenStackManager
└─ Typed screens (auto-discovered)
```

## State Flow
```
BootstrapState
  ↓
LoginState or SessionLoadingState
  ↓
MainMenuState
  ↓
MatchmakingState (user clicks Play)
  ↓
GameplayState (match found, load Game.unity)
  ↓
GameOverState (match ended)
```

## DI Container Lifetimes
- **Singleton:** App-level services (auth, logging, state machine, UI)
- **Scoped:** Per-scope services (match services)
- **Transient:** One-time use (screens, states)

## Key DI Scopes

| Scope | File | Services |
|-------|------|----------|
| Root | `Assets/_KitchenClash/Composition/RootLifetimeScope.cs` | IGameStateManager, IUIService, IAuthService, IEventBus |
| Menu | `Assets/_KitchenClash/Composition/MenuLifetimeScope.cs` | IMatchmakingService, ILobbyManager, IPlayerManager |
| Match | `Assets/_KitchenClash/Composition/MatchLifetimeScope.cs` | IScoreService, IOrderService, IAbilityService, MatchContext |

## Service Interfaces (Most Used)

| Interface | Namespace | Purpose |
|-----------|-----------|---------|
| `IEventBus` | Domain | Publish/subscribe events |
| `IGameStateManager` | Application.State | Change game states |
| `IUIService` | Application.Services | Show/hide screens |
| `IScoreService` | Domain | Track scores |
| `IOrderService` | Domain | Manage orders |
| `IMatchContext` | Infrastructure.Network | Access scene objects |
| `IAuthService` | Application.Interfaces | EOS authentication |
| `IPlayerDataService` | Application.Interfaces | Player progression |
| `ILoggingService` | Domain | Logging |

## Event Bus Pattern
```csharp
// Publish
_eventBus.Publish(new MyEvent { Data = value });

// Subscribe
_eventBus.Subscribe<MyEvent>(evt => HandleEvent(evt));

// Unsubscribe
_eventBus.Unsubscribe<MyEvent>(handler);
```

## State Pattern
```csharp
public class MyState : BaseState
{
    public override void Enter()
    {
        base.Enter();
        // Initialize, subscribe to events
    }

    public override void Update()
    {
        if (shouldTransition)
            _stateManager.ChangeState<NextState>();
    }

    public override void Exit()
    {
        // Cleanup, unsubscribe
        base.Exit();
    }
}
```

## Network Object Pattern
```csharp
public class MyNetworkObject : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return; // Client-only init
        // Server/owner init
    }

    [Rpc]
    public void MyRpc(NetworkBehaviourSerialisationStream stream)
    {
        // Handle RPC
    }
}
```

## Testing Pattern
```csharp
public class MyServiceTests
{
    private MyService _svc;
    private SpyEventBus _eventBus;

    [SetUp]
    public void SetUp()
    {
        _eventBus = new SpyEventBus();
        _svc = new MyService(_eventBus);
    }

    [Test]
    public void Method_Condition_ExpectedResult()
    {
        _svc.DoSomething();
        Assert.That(_eventBus.PublishedEvents, Contains.Item(expectedEvent));
    }
}
```

## Entry Points
- **Game start:** `Assets/Scenes/Bootstrap.unity`
- **Bootstrap prefab:** `Assets/Prefabs/General/GameBootstrap.prefab`
- **Root DI:** `Assets/_KitchenClash/Composition/RootLifetimeScope.cs`
- **Game bootstrapper:** `GameBootstrapper.cs` (registered as IStartable)

## Key Files by Task

### Add a new state
- Create: `Assets/_KitchenClash/Infrastructure/States/MyState.cs`
- Auto-discovered by `RootLifetimeScope.RegisterGameStates()`
- Transition: `_stateManager.ChangeState<MyState>()`

### Add a new service
- Interface: `Assets/_KitchenClash/Domain/Interfaces/IMyService.cs`
- Implementation: `Assets/_KitchenClash/Infrastructure/Services/MyService.cs`
- Register in appropriate scope's Configure method
- Inject via constructor

### Add a new UI screen
- Create: `Assets/_KitchenClash/Presentation/Screens/MyScreen.cs`
- Mark with: `[UIScreen]`
- Auto-discovered by `RootLifetimeScope.RegisterScreens()`
- Show: `_uiService.ShowScreen<MyScreen>()`

### Add a new test
- File: `Assets/Scripts/Tests/EditMode/Gameplay/MyServiceTests.cs`
- Inherit: `NUnit.Framework.TestCase`
- Use: `[SetUp]`, `[Test]`, `Assert.That()`
- Run: `dotnet test RecipeRage.Tests.EditMode.csproj --no-build -nologo`

## Documentation (Source of Truth)
1. **Current code** (always wins)
2. `Documentation/Architecture/PROJECT_MEMORY.md` (architecture)
3. `Documentation/Architecture/CURRENT_CODEBASE_AUDIT.md` (implementation audit)
4. `Documentation/Guides/gameplay-scene-setup.md` (scene wiring)
5. `KitchenClash_GDD_v3.md` (game design)
6. `conductor/tech-stack.md` (tech decisions)

## Code Style
- **Naming:** `_privateField`, `PublicProperty`, `IInterface`, `ClassName`
- **Indentation:** 4 spaces
- **Line endings:** CRLF
- **No `this.`** on field/property/method access
- **var** only when type is obvious
- **Async:** Use UniTask instead of Task

## Common Patterns

### Async with cancellation
```csharp
async UniTask MyAsync()
{
    await UniTask.Delay(1000, cancellationToken: StateCancellationToken);
}

MyAsync().Forget(); // Fire and forget
```

### Scene object access (via MatchContext)
```csharp
if (_matchContext.ScoreManager != null)
    _matchContext.ScoreManager.UpdateScore(teamId, points);
```

### Service injection
```csharp
public class MyComponent
{
    private readonly IMyService _service;

    public MyComponent(IMyService service)
    {
        _service = service;
    }
}
```

## Troubleshooting

### Tests fail with "Can't find service"
→ Service not registered in LifetimeScope. Check `.Configure()` method.

### State not transitioning
→ StateManager not injected. Check constructor.

### Circular dependency error
→ Services depend on each other. Refactor to break cycle.

### Event not firing
→ Unsubscribed before publish. Ensure Subscribe/Unsubscribe timing.

### Network object not spawning
→ Check `INetworkObjectPool`, `INetworkGameManager` injection, not in session scope.

## Performance Tips
- Use ObjectPool for frequent spawns
- Event subscriptions: unsubscribe in OnDisable
- Singleton services: initialize once, reuse
- Match services: scoped lifetime, disposed after match

## Deployment Checklist
- [ ] Tests pass: `dotnet test ... --no-build -nologo`
- [ ] Coverage >80%: Check code coverage report
- [ ] No linting errors: EditorConfig enforced
- [ ] Documentation updated: Update related .md files
- [ ] Git notes attached to commits: `git notes add -m "..."`
- [ ] Plan.md updated: Mark task complete with SHA
