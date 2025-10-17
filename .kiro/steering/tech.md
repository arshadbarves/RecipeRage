# Technology Stack

## Engine & Version

- **Unity**: 6000.0.58f2 (Unity 6)
- **Scripting**: C# (.NET 4.x)
- **Rendering**: Universal Render Pipeline (URP) 17.0.4

## Key Dependencies

### Networking & Services
- **Epic Online Services (EOS)**: Authentication, matchmaking, lobbies, P2P networking
- **Unity Netcode for GameObjects**: 2.5.1 - Network synchronization
- **Unity Multiplayer Tools**: 2.2.6 - Debugging and profiling

### Core Libraries
- **UniTask**: Async/await support for Unity
- **DOTween (Demigiant)**: Animation and tweening (used by AnimationService)
- **Custom ServiceContainer**: Lightweight dependency injection (replaces VContainer)

### Unity Packages
- **Addressables**: 2.7.2 - Asset management
- **Input System**: 1.14.2 - Cross-platform input
- **Cinemachine**: 3.1.4 - Camera control
- **UI Toolkit**: Built-in - Modern UI system (not UGUI)

## Project Structure

```
Assets/
├── Scripts/
│   ├── Core/              # Core systems (Bootstrap, Networking, Audio, etc.)
│   ├── Gameplay/          # Game-specific logic (Cooking, Stations, Scoring)
│   ├── UI/                # UI components (UI Toolkit based)
│   ├── Modules/           # Reusable modules
│   └── Examples/          # Example implementations
├── Prefabs/               # Game prefabs
├── ScriptableObjects/     # Data assets (GameModes, Characters)
├── Resources/             # Runtime-loaded assets
│   ├── Data/              # ScriptableObject data
│   └── UI/                # UXML templates and USS styles
├── Scenes/                # Unity scenes
├── Editor/                # Editor tools and extensions
└── Settings/              # Project settings and profiles
```

## Architecture Patterns

### Bootstrap System
- **GameBootstrap**: Single entry point, initializes all services
- **ServiceContainer**: Dependency injection container (replaces singletons)
- Services initialized in dependency order

### Service-Based Architecture
All major systems are services registered in ServiceContainer:
- ISaveService, IAudioService, IInputService
- ILoggingService, IAuthenticationService, IAnimationService
- IUIService, INetworkingServices, IGameModeService
- ICharacterService, IGameStateManager

### State Management
- **State Pattern**: GameStateManager controls game flow
- States: MainMenuState, LobbyState, InGameState, GameOverState
- Clean state transitions with Enter/Exit/Update lifecycle

### Design Patterns in Use
- **Factory Pattern**: InputProviderFactory for platform-specific input
- **Strategy Pattern**: IMatchmakingStrategy for different matchmaking modes
- **Object Pool Pattern**: AudioPoolManager for audio source reuse
- **Dependency Injection**: Constructor injection throughout services
- **Interface Segregation**: Focused interfaces (IMusicPlayer, ISFXPlayer, etc.)

### Pattern Utilities
All legacy pattern utilities (ServiceLocator, Singleton, MonoBehaviourSingleton) have been removed.
Use ServiceContainer for all service management and dependency injection.

## Common Commands

### Unity Editor
- **Play Mode**: Cmd/Ctrl + P
- **Build**: Cmd/Ctrl + B
- **Open Scene**: Startup.unity (entry point)

### Development
```bash
# Open project in Unity
open -a Unity RecipeRage.sln

# Build (via Unity Editor)
# File > Build Settings > Build
```

### Testing
- Unity Test Framework integrated
- Run tests: Window > General > Test Runner

## Build Configuration

### Development Builds
- Logging enabled via `DEVELOPMENT_BUILD` or `UNITY_EDITOR` defines
- Debug console available in-game (~ key)

### Production Builds
- Logging disabled for performance
- Encryption enabled for save data
- Anti-cheat measures active

## Performance Targets

- **Mobile**: 60 FPS on mid-range devices
- **Memory**: < 500MB on mobile
- **Network**: < 100ms latency for optimal experience
- **Load Times**: < 3 seconds for scene transitions
