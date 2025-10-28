# UI Resources Organization

This folder contains all UI Toolkit resources organized in a professional, categorized structure.

## Folder Structure

```
Assets/Resources/UI/
├── Styles/              # USS stylesheets
│   ├── Core/           # Base styles, common utilities, global buttons
│   ├── Screens/        # Screen-specific styles
│   ├── Components/     # Component styles (tabs, widgets, lobby)
│   └── Popups/         # Popup, modal, notification, toast styles
├── Templates/           # UXML templates
│   ├── Screens/        # Full screen templates
│   ├── Components/     # Reusable component templates
│   ├── Popups/         # Popup and modal templates
│   └── Common/         # Shared UI elements
├── Images/              # UI images and icons
└── Data/                # JSON data files (maps, configs)
```

## Style Categories

### Core Styles (`Styles/Core/`)
- `BaseUIScreen.uss` - Base styles for all screens
- `Common.uss` - Common utilities and shared styles
- `GlobalButtons.uss` - Global button styles

### Screen Styles (`Styles/Screens/`)
Screen-specific stylesheets for:
- Character Selection
- Game Mode Selection
- Loading Screen
- Login Screen
- Main Menu
- Maintenance
- Map Selection
- Profile
- Settings
- Shop
- Skins
- Splash Screen

### Component Styles (`Styles/Components/`)
- `JoystickEditor.uss` - Joystick editor component
- `LobbyStyles.uss` - Lobby-related styles
- `LobbyTab.uss` - Lobby tab component
- `MatchmakingWidget.uss` - Matchmaking widget

### Popup Styles (`Styles/Popups/`)
- `Friends.uss` - Friends popup
- `Modal.uss` - Modal dialogs
- `Notification.uss` - Notification system
- `Popup.uss` - Generic popup styles
- `Toast.uss` - Toast notifications

## Template Categories

### Screen Templates (`Templates/Screens/`)
Full-screen UXML templates:
- CharacterSelectionTemplate.uxml
- GameModeSelectionTemplate.uxml
- LoadingScreenTemplate.uxml
- LoginScreenTemplate.uxml
- MainMenuTemplate.uxml
- MaintenanceTemplate.uxml
- MapSelectionTemplate.uxml
- ProfileTemplate.uxml
- SettingsTemplate.uxml
- ShopTemplate.uxml
- SkinsTemplate.uxml
- SplashScreenTemplate.uxml
- LobbyTemplate.uxml

### Component Templates (`Templates/Components/`)
Reusable component templates:
- CharacterCard.uxml - Character card component
- DebugConsole.uxml - Debug console
- GameModeEntry.uxml - Game mode list entry
- JoystickEditorTemplate.uxml - Joystick editor
- LobbyPlayerEntry.uxml - Lobby player list entry
- LobbyTabTemplate.uxml - Lobby tab component
- MatchmakingWidget.uxml - Matchmaking widget

### Popup Templates (`Templates/Popups/`)
- FriendsPopupTemplate.uxml - Friends list popup
- ModalTemplate.uxml - Modal dialog
- NotificationTemplate.uxml - Notification
- PopupTemplate.uxml - Generic popup
- ToastTemplate.uxml - Toast notification
- UsernamePopupTemplate.uxml - Username input popup

### Common Templates (`Templates/Common/`)
Shared UI elements used across multiple screens.

## Usage Guidelines

### Loading Styles
```csharp
// Load from categorized location
var styleSheet = Resources.Load<StyleSheet>("UI/Styles/Screens/MainMenu");
rootVisualElement.styleSheets.Add(styleSheet);
```

### Loading Templates
```csharp
// Load from categorized location
var template = Resources.Load<VisualTreeAsset>("UI/Templates/Screens/MainMenuTemplate");
template.CloneTree(rootVisualElement);
```

### Best Practices

1. **Core Styles First**: Always load core styles before screen-specific styles
2. **Consistent Naming**: Use PascalCase for all files
3. **Categorize Properly**: Place new files in the correct category
4. **Avoid Duplication**: Reuse common styles from Core folder
5. **Document Changes**: Update this README when adding new categories

## Migration Notes

All USS files previously in `Assets/UI/USS/` have been moved to `Assets/Resources/UI/Styles/` with proper categorization.

All UXML templates are now organized by type (Screens, Components, Popups) for better discoverability.

## Related Documentation

- See `Assets/Scripts/UI/README_UI_SYSTEM.md` for UI system architecture
- See `.kiro/steering/structure.md` for project structure conventions
