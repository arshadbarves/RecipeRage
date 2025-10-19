# Project Structure & Conventions

## Code Organization

### Namespace Structure
```csharp
Core.Bootstrap          // Initialization and service management
Core.Audio             // Audio services (Music, SFX)
Core.Authentication    // EOS authentication
Core.Networking        // Lobby, matchmaking, network services
Core.SaveSystem        // Save/load, encryption
Core.Logging           // Logging service
Core.Input             // Input abstraction
Core.State             // Game state machine
Core.Characters        // Character system
Core.GameModes         // Game mode definitions
Gameplay.Cooking       // Cooking mechanics
Gameplay.Stations      // Interactive stations
Gameplay.Scoring       // Score calculation
UI                     // UI components (UI Toolkit)
```

### File Organization Rules

1. **One class per file** - File name matches class name
2. **Interfaces in separate files** - Prefix with 'I' (ILoggingService.cs)
3. **Related classes grouped** - Same namespace, same folder
4. **ScriptableObjects in Assets/ScriptableObjects/** - Data definitions
5. **UI Templates in Assets/Resources/UI/** - UXML and USS files

## Naming Conventions

### C# Code
```csharp
// Classes, Interfaces, Methods, Properties - PascalCase
public class GameBootstrap
public interface ILoggingService
public void InitializeGame()
public int PlayerCount { get; set; }

// Private fields - _camelCase with underscore
private int _playerCount;
private ILoggingService _logger;

// Local variables, parameters - camelCase
int maxPlayers = 4;
public void AddPlayer(string playerName)

// Constants - UPPER_SNAKE_CASE
public const string GAME_VERSION = "1.0.0";

// Events - PascalCase with 'On' prefix
public event Action OnGameStarted;
```

### Unity Assets
- **Scenes**: PascalCase (MainMenu.unity, Game.unity)
- **Prefabs**: PascalCase (PlayerController.prefab)
- **ScriptableObjects**: PascalCase with type suffix (ClassicMode.asset)
- **Materials**: PascalCase (Floor.mat, Wall.mat)
- **UI Templates**: PascalCase (MainMenuTemplate.uxml)
- **Styles**: PascalCase (LobbyStyles.uss)

## Architecture Patterns

### Service Registration
Services are registered in GameBootstrap.cs in dependency order:
```csharp
// 1. Core services (no dependencies)
_services.RegisterLoggingService(new LoggingService());
_services.RegisterSaveService(new SaveService());
_services.RegisterAudioService(new AudioService());

// 2. Game systems (depend on core)
_services.RegisterAuthenticationService(new AuthenticationService());
_services.RegisterNetworkingServices(new NetworkingServiceContainer());
_services.RegisterStateManager(new GameStateManager());
```

### Accessing Services

**Preferred (Dependency Injection)**:
```csharp
public class MyService
{
    private readonly ILoggingService _logger;
    
    public MyService(ILoggingService logger)
    {
        _logger = logger;
    }
}
```

**MonoBehaviours (Static Access)**:
```csharp
public class MyBehaviour : MonoBehaviour
{
    void Start()
    {
        var logger = GameBootstrap.Services.LoggingService;
        GameLogger.Log("Message"); // Static helper
    }
}
```

### Logging Conventions

Use category-specific logging:
```csharp
GameLogger.Log("General message");
GameLogger.Network.Log("Network event");
GameLogger.Audio.Log("Audio event");
GameLogger.UI.Log("UI event");
GameLogger.Gameplay.Log("Gameplay event");
```

Log levels:
- **Debug**: Development info (disabled in production)
- **Info**: General information
- **Warning**: Potential issues
- **Error**: Errors that don't crash the game
- **Exception**: Critical errors

### UI Toolkit Conventions

1. **UXML Templates**: Define structure in Assets/Resources/UI/
2. **USS Styles**: Define styling in Assets/Resources/UI/ (static styles only, no animations)
3. **C# Controllers**: BaseUIScreen-derived classes that bind to UXML
4. **Query Elements**: Use GetElement<T>() helper methods
```csharp
protected override void OnInitialize()
{
    var button = GetElement<Button>("my-button");
    button.clicked += OnButtonClicked;
}
```

### UI Animation Guidelines

**ALWAYS use AnimationService (DOTween) for UI animations - NEVER use CSS transitions/animations**

```csharp
// ✅ CORRECT - Use AnimationService
protected override void OnShow()
{
    var animationService = GameBootstrap.Services?.AnimationService;
    if (animationService != null && _loginCard != null)
    {
        // Slide in from right
        animationService.UI.SlideIn(_loginCard, SlideDirection.Right, 0.5f);
    }
}

protected override void OnHide()
{
    var animationService = GameBootstrap.Services?.AnimationService;
    if (animationService != null && _loginCard != null)
    {
        // Slide out to right
        animationService.UI.SlideOut(_loginCard, SlideDirection.Right, 0.4f);
    }
}

// ❌ WRONG - Don't use CSS classes for animations
Container?.AddToClassList("show"); // Don't do this
Container?.RemoveFromClassList("hide"); // Don't do this
```

**Available Animation Methods:**
- `FadeIn()` / `FadeOut()` - Opacity animations
- `SlideIn()` / `SlideOut()` - Position animations (Left, Right, Top, Bottom)
- `ScaleIn()` / `ScaleOut()` - Scale animations
- `Pulse()` - Pulsing effect
- `Shake()` - Shake effect

**Why AnimationService over CSS:**
- Consistent animation system across all UI
- Better performance with DOTween
- Easier to control programmatically
- Callbacks for animation completion
- Can be killed/interrupted cleanly

### ScriptableObject Data

All game data defined as ScriptableObjects:
- **GameModeData**: Game mode configuration
- **CharacterData**: Character stats and abilities
- **RecipeData**: Recipe definitions
- **AudioDatabase**: Audio clip references

Location: `Assets/ScriptableObjects/` or `Assets/Resources/Data/`

## SOLID Principles

The codebase follows SOLID principles:

### Single Responsibility Principle (SRP)
Each class has one reason to change:
```csharp
// AudioService delegates to specialized components
public class AudioService : IAudioService
{
    private readonly IMusicPlayer _musicPlayer;
    private readonly ISFXPlayer _sfxPlayer;
    private readonly IAudioVolumeController _volumeController;
}
```

### Open/Closed Principle (OCP)
Open for extension, closed for modification:
```csharp
// Extend via new IState implementations
public interface IState
{
    void Enter();
    void Exit();
    void Update();
}
```

### Liskov Substitution Principle (LSP)
Interfaces can be substituted with implementations:
```csharp
// Any IInputProvider can replace another
IInputProvider provider = InputProviderFactory.CreateForPlatform();
```

### Interface Segregation Principle (ISP)
Focused interfaces, not fat interfaces:
```csharp
// Separate interfaces for different concerns
public interface IMusicPlayer { }
public interface ISFXPlayer { }
public interface IAudioVolumeController { }
```

### Dependency Inversion Principle (DIP)
Depend on abstractions, not concretions:
```csharp
// Services depend on interfaces, not implementations
public class MyService
{
    private readonly ILoggingService _logger;
    
    public MyService(ILoggingService logger) // Interface, not LoggingService
    {
        _logger = logger;
    }
}
```

## Design Patterns

### Factory Pattern
Used for platform-specific creation:
```csharp
public static class InputProviderFactory
{
    public static IInputProvider CreateForPlatform()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return new KeyboardInputProvider();
#elif UNITY_IOS || UNITY_ANDROID
        return new TouchInputProvider();
#endif
    }
}
```

### Strategy Pattern
Used for matchmaking strategies:
```csharp
public interface IMatchmakingStrategy
{
    void Execute(GameMode gameMode, int minPlayers, int maxPlayers);
}

public class QuickMatchStrategy : IMatchmakingStrategy { }
public class RankedMatchStrategy : IMatchmakingStrategy { }
```

### State Pattern
Used for game state management:
```csharp
public interface IState
{
    void Enter();
    void Exit();
    void Update();
}

// Concrete states
public class MainMenuState : IState { }
public class LobbyState : IState { }
public class InGameState : IState { }
```

### Object Pool Pattern
Used for audio sources and frequently instantiated objects:
```csharp
public class AudioPoolManager
{
    private readonly Queue<AudioSource> _sfxPool;
    
    public AudioSource GetAudioSource() { }
    public void ReturnAudioSource(AudioSource source) { }
}
```

### Service Locator Pattern (Legacy)
Available in `Core.Utilities.Patterns.ServiceLocator` but **prefer ServiceContainer**:
```csharp
// ❌ Avoid - Legacy pattern
ServiceLocator.Instance.Register<IMyService>(myService);

// ✅ Prefer - Modern approach
GameBootstrap.Services.RegisterMyService(myService);
```

### Singleton Pattern (Avoid)
Utility classes exist in `Core.Utilities.Patterns` but **avoid using them**:
- Use ServiceContainer for services
- All services follow consistent naming (e.g., UIService, AudioService)

### Async Operations
Use UniTask for async operations:
```csharp
public async UniTask<bool> LoadDataAsync()
{
    await UniTask.Delay(1000);
    return true;
}
```

## Editor Extensions

Custom editor tools in `Assets/Editor/`:
- **DebugConsoleSetup.cs**: In-game debug console
- **DevelopmentTools.cs**: Development utilities
- Scene validation tools
- Prefab utilities

## Documentation

### XML Comments Required
All public APIs must have XML documentation:
```csharp
/// <summary>
/// Initializes the game with specified parameters
/// </summary>
/// <param name="playerCount">Number of players</param>
/// <returns>True if successful</returns>
public bool Initialize(int playerCount)
```

### README Files
Each major system should have a README.md:
- Assets/Scripts/Core/Logging/README.md
- Assets/Scripts/UI/README_UI_SYSTEM.md
- Module-specific documentation

## Testing

- Unit tests for services and utilities
- Integration tests for system interactions
- Test files mirror source structure
- Use Unity Test Framework

## Version Control

### Ignored Files
- Library/
- Temp/
- Logs/
- obj/
- *.csproj (generated)
- UserSettings/

### Important Files
- Assets/ (all game content)
- ProjectSettings/ (Unity configuration)
- Packages/manifest.json (dependencies)
