# Design Patterns & SOLID Principles

## SOLID Principles

This codebase strictly follows SOLID principles for maintainable, testable code.

### 1. Single Responsibility Principle (SRP)
**Each class should have one reason to change.**

✅ **Good Example:**
```csharp
// AudioService delegates to specialized components
public class AudioService : IAudioService
{
    private readonly IMusicPlayer _musicPlayer;
    private readonly ISFXPlayer _sfxPlayer;
    private readonly IAudioVolumeController _volumeController;

    // Each component handles one responsibility
}
```

❌ **Avoid:**
```csharp
// God class that does everything
public class AudioManager
{
    void PlayMusic() { }
    void PlaySFX() { }
    void SaveSettings() { }
    void LoadSettings() { }
    void HandleNetworkAudio() { }
}
```

### 2. Open/Closed Principle (OCP)
**Open for extension, closed for modification.**

✅ **Good Example:**
```csharp
// Add new states without modifying existing code
public interface IState
{
    void Enter();
    void Exit();
    void Update();
}

public class NewGameModeState : IState { } // Extend
```

❌ **Avoid:**
```csharp
// Modifying existing class for new behavior
public class GameManager
{
    void Update()
    {
        if (mode == "classic") { }
        else if (mode == "timeattack") { }
        else if (mode == "newmode") { } // Modification
    }
}
```

### 3. Liskov Substitution Principle (LSP)
**Derived classes must be substitutable for their base classes.**

✅ **Good Example:**
```csharp
IInputProvider provider = InputProviderFactory.CreateForPlatform();
// Any IInputProvider implementation works the same way
```

### 4. Interface Segregation Principle (ISP)
**Many specific interfaces are better than one general-purpose interface.**

✅ **Good Example:**
```csharp
// Focused interfaces
public interface IMusicPlayer
{
    void PlayMusic(AudioClip clip);
    void StopMusic();
}

public interface ISFXPlayer
{
    AudioSource PlaySFX(AudioClip clip);
}
```

❌ **Avoid:**
```csharp
// Fat interface
public interface IAudioSystem
{
    void PlayMusic();
    void StopMusic();
    void PlaySFX();
    void SetVolume();
    void SaveSettings();
    void LoadSettings();
    // Too many responsibilities
}
```

### 5. Dependency Inversion Principle (DIP)
**Depend on abstractions, not concretions.**

✅ **Good Example:**
```csharp
public class AuthenticationService
{
    private readonly ISaveService _saveService; // Interface

    public AuthenticationService(ISaveService saveService)
    {
        _saveService = saveService;
    }
}
```

❌ **Avoid:**
```csharp
public class AuthenticationService
{
    private SaveService _saveService; // Concrete class

    public AuthenticationService()
    {
        _saveService = new SaveService(); // Hard dependency
    }
}
```

## Design Patterns

### Factory Pattern
**Creates objects without specifying exact class.**

```csharp
public static class InputProviderFactory
{
    public static IInputProvider CreateForPlatform()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return new KeyboardInputProvider();
#elif UNITY_IOS || UNITY_ANDROID
        return new TouchInputProvider();
#else
        return new InputSystemProvider();
#endif
    }
}

// Usage
var input = InputProviderFactory.CreateForPlatform();
```

**When to use:**
- Platform-specific implementations
- Complex object creation
- Decoupling creation from usage

### Strategy Pattern
**Defines a family of algorithms, encapsulates each one.**

```csharp
public interface IMatchmakingStrategy
{
    void Execute(GameMode gameMode, int minPlayers, int maxPlayers);
}

public class QuickMatchStrategy : IMatchmakingStrategy
{
    public void Execute(GameMode gameMode, int minPlayers, int maxPlayers)
    {
        // Quick match logic
    }
}

public class RankedMatchStrategy : IMatchmakingStrategy
{
    public void Execute(GameMode gameMode, int minPlayers, int maxPlayers)
    {
        // Ranked match logic
    }
}

// Usage
IMatchmakingStrategy strategy = isRanked
    ? new RankedMatchStrategy()
    : new QuickMatchStrategy();
strategy.Execute(gameMode, 2, 4);
```

**When to use:**
- Multiple algorithms for same task
- Runtime algorithm selection
- Avoiding conditional logic

### State Pattern
**Allows object to alter behavior when internal state changes.**

```csharp
public interface IState
{
    string StateName { get; }
    void Enter();
    void Exit();
    void Update();
    void FixedUpdate();
}

public class MainMenuState : IState
{
    public string StateName => "MainMenu";

    public void Enter()
    {
        // Load main menu UI
    }

    public void Exit()
    {
        // Cleanup
    }

    public void Update() { }
    public void FixedUpdate() { }
}

// Usage
_stateManager.ChangeState(new LobbyState());
```

**When to use:**
- Complex state-dependent behavior
- State transitions with lifecycle
- Game flow management

### Object Pool Pattern
**Reuses objects instead of creating/destroying.**

```csharp
public class AudioPoolManager
{
    private readonly Queue<AudioSource> _sfxPool = new Queue<AudioSource>();
    private readonly int _maxPoolSize = 20;

    public AudioSource GetAudioSource()
    {
        if (_sfxPool.Count > 0)
            return _sfxPool.Dequeue();

        return CreateAudioSource();
    }

    public void ReturnAudioSource(AudioSource source)
    {
        source.Stop();
        source.clip = null;
        _sfxPool.Enqueue(source);
    }
}

// Usage
var source = _poolManager.GetAudioSource();
source.PlayOneShot(clip);
_poolManager.ReturnAudioSource(source);
```

**When to use:**
- Frequently created/destroyed objects
- Performance-critical systems
- Audio sources, particles, projectiles

### Dependency Injection Pattern
**Injects dependencies rather than creating them.**

```csharp
// Constructor injection (preferred)
public class NetworkService
{
    private readonly ILoggingService _logger;
    private readonly IAuthenticationService _auth;

    public NetworkService(ILoggingService logger, IAuthenticationService auth)
    {
        _logger = logger;
        _auth = auth;
    }
}

// Registration in GameBootstrap
_services.RegisterNetworkService(new NetworkService(
    _services.LoggingService,
    _services.AuthenticationService
));
```

**When to use:**
- All services and systems
- Testable code
- Decoupled dependencies

### Observer Pattern (Events)
**Notifies multiple objects about state changes.**

```csharp
public class GameStateManager
{
    public event Action<IState, IState> OnStateChanged;

    public void ChangeState(IState newState)
    {
        var previous = CurrentState;
        CurrentState = newState;
        OnStateChanged?.Invoke(previous, newState);
    }
}

// Usage
_stateManager.OnStateChanged += (prev, curr) =>
{
    Debug.Log($"State changed from {prev.StateName} to {curr.StateName}");
};
```

**When to use:**
- Event-driven systems
- Decoupled notifications
- UI updates

## Anti-Patterns to Avoid

### ❌ Singleton Pattern
**Don't use singletons** - Use ServiceContainer instead.

```csharp
// ❌ Avoid
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
}

// ✅ Use ServiceContainer instead
GameBootstrap.Services.GameModeService
GameBootstrap.Services.UIService
GameBootstrap.Services.AnimationService
```

**No exceptions** - All services use ServiceContainer

### ❌ Service Locator (Removed)
**ServiceLocator has been removed from the codebase.**

```csharp
// ❌ No longer exists
// ServiceLocator.Instance.Register<IMyService>(myService);

// ✅ Use ServiceContainer
_services.RegisterMyService(myService);
var service = GameBootstrap.Services.MyService;
```

### ❌ God Objects
**Avoid classes that do too much.**

```csharp
// ❌ Avoid
public class GameManager
{
    void HandleInput() { }
    void UpdateAudio() { }
    void UpdateNetwork() { }
    void UpdateUI() { }
    void SaveGame() { }
    // Too many responsibilities
}

// ✅ Split into focused services
IInputService, IAudioService, INetworkService, ISaveService
```

## Pattern Selection Guide

| Need | Pattern | Example |
|------|---------|---------|
| Platform-specific creation | Factory | InputProviderFactory |
| Multiple algorithms | Strategy | IMatchmakingStrategy |
| State-dependent behavior | State | GameStateManager |
| Object reuse | Object Pool | AudioPoolManager |
| Decouple dependencies | Dependency Injection | ServiceContainer |
| Event notifications | Observer | OnStateChanged events |
| Lifecycle management | Bootstrap | GameBootstrap |

## Best Practices

1. **Always use interfaces** for services and dependencies
2. **Constructor injection** for dependencies (not property injection)
3. **Composition over inheritance** - favor interfaces and composition
4. **Keep classes small** - Single Responsibility Principle
5. **Avoid static state** - Use ServiceContainer for shared state
6. **Use events** for decoupled communication
7. **Pool expensive objects** - Audio sources, particles, network objects
8. **Document patterns** - XML comments explaining pattern usage
