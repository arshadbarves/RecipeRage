# Code Standards

## SOLID Principles

All code must follow SOLID principles:

### Single Responsibility Principle (SRP)
Each class should have one reason to change. Keep classes focused on a single responsibility.

```csharp
// Good: Separate concerns
public class PlayerMovement
{
    public void Move(Vector3 direction) { }
}

public class PlayerHealth
{
    public void TakeDamage(int amount) { }
}

// Bad: Multiple responsibilities
public class Player
{
    public void Move(Vector3 direction) { }
    public void TakeDamage(int amount) { }
    public void SaveToDatabase() { }  // Should be separate
    public void RenderUI() { }  // Should be separate
}
```

### Open/Closed Principle (OCP)
Classes should be open for extension but closed for modification. Use interfaces and abstract classes.

```csharp
// Good: Extensible through inheritance
public abstract class GameModeBase
{
    public abstract void Initialize();
    public abstract void CalculateScore(Player player);
}

public class ClassicMode : GameModeBase
{
    public override void Initialize() { }
    public override void CalculateScore(Player player) { }
}

public class TimeAttackMode : GameModeBase
{
    public override void Initialize() { }
    public override void CalculateScore(Player player) { }
}
```

### Liskov Substitution Principle (LSP)
Derived classes must be substitutable for their base classes without breaking functionality.

```csharp
// Good: Derived class maintains base class contract
public interface IInteractable
{
    void Interact(Player player);
}

public class CookingStation : IInteractable
{
    public void Interact(Player player)
    {
        // Always safe to call
    }
}

public class Container : IInteractable
{
    public void Interact(Player player)
    {
        // Always safe to call
    }
}
```

### Interface Segregation Principle (ISP)
Clients should not be forced to depend on interfaces they don't use. Keep interfaces small and focused.

```csharp
// Good: Focused interfaces
public interface IMovable
{
    void Move(Vector3 direction);
}

public interface IDamageable
{
    void TakeDamage(int amount);
}

public interface IInteractable
{
    void Interact(Player player);
}

// Bad: Fat interface
public interface IGameEntity
{
    void Move(Vector3 direction);
    void TakeDamage(int amount);
    void Interact(Player player);
    void Render();
    void PlaySound();
    // Not all entities need all methods
}
```

### Dependency Inversion Principle (DIP)
Depend on abstractions, not concretions. Use interfaces and dependency injection.

```csharp
// Good: Depends on abstraction
public class GameController
{
    private readonly INetworkManager _networkManager;
    private readonly IScoreCalculator _scoreCalculator;
    
    public GameController(INetworkManager networkManager, IScoreCalculator scoreCalculator)
    {
        _networkManager = networkManager;
        _scoreCalculator = scoreCalculator;
    }
}

// Bad: Depends on concrete implementation
public class GameController
{
    private EOSNetworkManager _networkManager = new EOSNetworkManager();
    private DefaultScoreCalculator _scoreCalculator = new DefaultScoreCalculator();
}
```

## Naming Conventions

- **Classes/Interfaces**: PascalCase (`PlayerController`, `INetworkManager`)
- **Methods**: PascalCase (`Initialize()`, `CalculateScore()`)
- **Properties**: PascalCase (`PlayerCount`, `IsActive`)
- **Private fields**: _camelCase (`_playerCount`, `_isInitialized`)
- **Local variables**: camelCase (`playerCount`, `maxScore`)
- **Constants**: UPPER_SNAKE_CASE (`MAX_PLAYERS`, `DEFAULT_TIMEOUT`)

## Code Organization

Within each class, order members as follows:

1. Constants
2. Static fields
3. Private fields
4. Properties
5. Unity lifecycle methods (Awake, Start, Update, etc.)
6. Public methods
7. Protected methods
8. Private methods
9. Nested types

## Documentation

All public APIs must have XML documentation:

```csharp
/// <summary>
/// Manages player movement and physics interactions
/// </summary>
public class PlayerMovement
{
    /// <summary>
    /// Moves the player in the specified direction
    /// </summary>
    /// <param name="direction">Normalized movement direction</param>
    /// <param name="speed">Movement speed multiplier</param>
    public void Move(Vector3 direction, float speed)
    {
        // Implementation
    }
}
```

## Dependency Injection

Use VContainer for dependency injection. Register services in the bootstrap:

```csharp
public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<INetworkManager, EOSNetworkManager>(Lifetime.Singleton);
        builder.Register<IScoreCalculator, DefaultScoreCalculator>(Lifetime.Singleton);
        builder.RegisterComponentInHierarchy<PlayerController>();
    }
}
```

## Async/Await

Use UniTask for async operations instead of coroutines:

```csharp
// Good: UniTask
public async UniTask<bool> InitializeAsync()
{
    await UniTask.Delay(1000);
    return true;
}

// Avoid: Coroutines for complex async logic
public IEnumerator Initialize()
{
    yield return new WaitForSeconds(1f);
}
```

## Performance Guidelines

- Use object pooling for frequently instantiated objects
- Cache component references in Awake/Start
- Avoid GetComponent in Update/FixedUpdate
- Use StringBuilder for string concatenation
- Prefer structs for small data types
- Use readonly when possible

## Code Replacement Policy

When updating or replacing code:

- **Remove legacy code completely** - Don't leave old implementations
- **No backward compatibility** - Replace, don't add alongside
- **No deprecation warnings** - Just remove and replace
- **Clean slate approach** - New implementation replaces old entirely
- **Update all references** - Find and update all usages immediately

```csharp
// ❌ Don't do this - keeping old code with deprecation
[Obsolete("Use NewMethod instead")]
public void OldMethod() { }

public void NewMethod() { }

// ✅ Do this - complete replacement
public void NewMethod() { }
// OldMethod is completely removed
```

When refactoring:
1. Identify all usages of old code
2. Replace with new implementation
3. Delete old code entirely
4. No compatibility layers or bridges
5. Clean commit with complete replacement
