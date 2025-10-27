# UI Architecture Overview

## Component Hierarchy

```
GameBootstrap
    └── UIService
        ├── MainMenuScreen (Screen)
        │   ├── CurrencyDisplay (Component)
        │   ├── PlayerCard (Component)
        │   └── TabView
        │       ├── LobbyTabComponent
        │       │   ├── PlayButton
        │       │   ├── MapButton
        │       │   └── MatchmakingWidgetComponent
        │       ├── SkinsTabComponent
        │       ├── ShopTabComponent
        │       └── SettingsTabComponent
        ├── LoginScreen (Screen)
        ├── ProfileScreen (Screen)
        ├── LoadingScreen (Screen)
        └── Popups
            └── UsernamePopup
```

## Data Flow

```
User Action (Button Click)
    ↓
Tab Component (LobbyTabComponent)
    ↓
Service Interface (IMatchmakingService)
    ↓
Concrete Service (LobbyManager)
    ↓
Network/Game Logic
    ↓
Event Bus
    ↓
UI Updates (via event handlers)
```

## Dependency Injection Pattern

```csharp
// 1. Services registered in GameBootstrap
GameBootstrap.Services.RegisterMatchmakingService(matchmakingService);

// 2. MainMenuScreen gets services
var services = GameBootstrap.Services;

// 3. Tab components receive dependencies via constructor
_lobbyTab = new LobbyTabComponent(
    services.MatchmakingService,
    services.StateManager
);

// 4. Tab components use injected interfaces
_matchmakingService.StartMatchmaking(GameMode.Classic, 2, 4);
```

## Lifecycle Management

```
MainMenuState.Enter()
    ↓
UIService.ShowScreen(UIScreenType.Menu)
    ↓
MainMenuScreen.OnShow()
    ↓
MainMenuScreen.InitializeAllTabs()
    ↓
LobbyTabComponent.Initialize()
    ↓
MatchmakingWidgetComponent.Initialize()

... user interaction ...

MainMenuState.Exit()
    ↓
UIService.HideScreen(UIScreenType.Menu)
    ↓
MainMenuScreen.OnHide()
    ↓
MainMenuScreen.OnDispose()
    ↓
LobbyTabComponent.Dispose()
    ↓
MatchmakingWidgetComponent.Dispose()
```

## Component Responsibilities

### MainMenuScreen (Screen)
- **Responsibility:** Container for tab navigation
- **Manages:** Top bar (player card, currency), tab switching
- **Lifecycle:** Initializes and disposes tab components
- **Dependencies:** UIService, EventBus, SaveService, CurrencyService

### LobbyTabComponent (Tab Component)
- **Responsibility:** Home/lobby view content
- **Manages:** Play button, map selection, matchmaking widget
- **Lifecycle:** Initialized by MainMenuScreen, disposed on screen hide
- **Dependencies:** IMatchmakingService, IGameStateManager

### MatchmakingWidgetComponent (Widget Component)
- **Responsibility:** Matchmaking status overlay
- **Manages:** Search timer, player count, cancel button
- **Lifecycle:** Initialized by LobbyTabComponent, disposed with parent
- **Dependencies:** IMatchmakingService

### ShopTabComponent (Tab Component)
- **Responsibility:** Shop view content
- **Manages:** Item categories, purchase flow
- **Lifecycle:** Initialized by MainMenuScreen, disposed on screen hide
- **Dependencies:** CurrencyService

### SkinsTabComponent (Tab Component)
- **Responsibility:** Skin selection view
- **Manages:** Skin grid, equip button
- **Lifecycle:** Initialized by MainMenuScreen, disposed on screen hide
- **Dependencies:** None (uses PlayerPrefs)

### SettingsTabComponent (Tab Component)
- **Responsibility:** Settings view content
- **Manages:** Audio, graphics, controls, gameplay settings
- **Lifecycle:** Initialized by MainMenuScreen, disposed on screen hide
- **Dependencies:** ISaveService

## SOLID Principles Applied

### Single Responsibility Principle (SRP)
- Each component has one clear purpose
- MainMenuScreen manages tabs, not tab content
- Tab components manage their own content

### Open/Closed Principle (OCP)
- New tabs can be added without modifying MainMenuScreen
- New widgets can be added to tabs without changing screen

### Liskov Substitution Principle (LSP)
- All tab components follow same initialization pattern
- Can be swapped without breaking MainMenuScreen

### Interface Segregation Principle (ISP)
- Components depend on focused interfaces (IMatchmakingService, ISaveService)
- Not on fat interfaces with unused methods

### Dependency Inversion Principle (DIP)
- Components depend on abstractions (interfaces)
- Not on concrete implementations
- Dependencies injected via constructor

## Design Patterns Used

### 1. **Screen Pattern**
- Each full-screen UI is a Screen class
- Managed by UIService
- Lifecycle: Initialize → Show → Update → Hide → Dispose

### 2. **Component Pattern**
- Reusable UI components (tabs, widgets)
- Pure C# classes (no MonoBehaviour)
- Composable and testable

### 3. **Dependency Injection**
- Constructor injection for dependencies
- Loose coupling between components
- Easy to test and mock

### 4. **Observer Pattern (Event Bus)**
- Components communicate via events
- Decoupled from each other
- Easy to add new listeners

### 5. **State Pattern**
- Game states control UI flow
- MainMenuState shows MainMenuScreen
- Clean state transitions

## Best Practices

### ✅ DO
- Use constructor injection for dependencies
- Implement Dispose() for cleanup
- Use interfaces for services
- Keep components focused (SRP)
- Use events for cross-component communication

### ❌ DON'T
- Access singletons directly (use DI)
- Create GameObjects in pure C# components
- Mix UI logic with business logic
- Forget to unsubscribe from events
- Use MonoBehaviour unless necessary

## Testing Strategy

### Unit Tests
```csharp
[Test]
public void LobbyTab_PlayButton_StartsMatchmaking()
{
    // Arrange
    var mockMatchmaking = new Mock<IMatchmakingService>();
    var mockStateManager = new Mock<IGameStateManager>();
    var lobbyTab = new LobbyTabComponent(mockMatchmaking.Object, mockStateManager.Object);
    
    // Act
    lobbyTab.OnPlayClicked();
    
    // Assert
    mockMatchmaking.Verify(m => m.StartMatchmaking(GameMode.Classic, 2, 4), Times.Once);
}
```

### Integration Tests
- Test MainMenuScreen initialization
- Test tab switching
- Test component disposal
- Test event flow

## Performance Considerations

### Memory Management
- All components implement Dispose()
- Event handlers unsubscribed on dispose
- No GameObject leaks

### Lazy Loading
- Tabs initialized on first show
- Heavy resources loaded on demand
- Async loading for large assets

### Object Pooling
- UI elements reused where possible
- Avoid frequent instantiation
- Pool frequently created elements

## Future Improvements

1. **Tab Interface**
   ```csharp
   public interface ITabComponent
   {
       void Initialize(VisualElement root);
       void OnShow();
       void OnHide();
       void Dispose();
   }
   ```

2. **Tab Lazy Loading**
   - Load tabs only when first accessed
   - Reduce initial load time

3. **Tab State Persistence**
   - Remember last selected tab
   - Restore tab state on return

4. **Tab Transition Animations**
   - Smooth transitions between tabs
   - Fade/slide effects

5. **Component Registry**
   - Auto-discover tab components
   - Reduce manual registration

---

**Architecture Version:** 2.0  
**Last Updated:** October 26, 2025  
**Status:** ✅ Production Ready
