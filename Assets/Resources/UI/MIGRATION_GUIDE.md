# UI Resources Migration Guide

## Overview

All UI resources have been reorganized from scattered locations into a professional, categorized structure within `Assets/Resources/UI/`.

## What Changed

### Old Structure
```
Assets/UI/USS/              # All USS files mixed together
Assets/Resources/UI/        # Templates and styles mixed
```

### New Structure
```
Assets/Resources/UI/
├── Styles/
│   ├── Core/           # BaseUIScreen, Common, GlobalButtons
│   ├── Screens/        # Screen-specific styles
│   ├── Components/     # Component styles
│   └── Popups/         # Popup/modal styles
├── Templates/
│   ├── Screens/        # Full screen templates
│   ├── Components/     # Reusable components
│   ├── Popups/         # Popup templates
│   └── Common/         # Shared elements
├── Images/             # UI images (unchanged)
└── Data/               # JSON data files
```

## Migration Steps for Developers

### 1. Update Resource Load Paths

**Old Code:**
```csharp
// Loading from old location
var styleSheet = Resources.Load<StyleSheet>("UI/MainMenu");
var template = Resources.Load<VisualTreeAsset>("UI/Templates/MainMenuTemplate");
```

**New Code:**
```csharp
// Loading from categorized location
var styleSheet = Resources.Load<StyleSheet>("UI/Styles/Screens/MainMenu");
var template = Resources.Load<VisualTreeAsset>("UI/Templates/Screens/MainMenuTemplate");
```

### 2. Path Mapping Reference

#### Styles (USS Files)

| Old Path | New Path | Category |
|----------|----------|----------|
| `UI/BaseUIScreen` | `UI/Styles/Core/BaseUIScreen` | Core |
| `UI/Common` | `UI/Styles/Core/Common` | Core |
| `UI/GlobalButtons` | `UI/Styles/Core/GlobalButtons` | Core |
| `UI/MainMenu` | `UI/Styles/Screens/MainMenu` | Screen |
| `UI/LoginScreen` | `UI/Styles/Screens/LoginScreen` | Screen |
| `UI/CharacterSelection` | `UI/Styles/Screens/CharacterSelection` | Screen |
| `UI/GameModeSelection` | `UI/Styles/Screens/GameModeSelection` | Screen |
| `UI/MapSelection` | `UI/Styles/Screens/MapSelection` | Screen |
| `UI/Profile` | `UI/Styles/Screens/Profile` | Screen |
| `UI/Settings` | `UI/Styles/Screens/Settings` | Screen |
| `UI/Shop` | `UI/Styles/Screens/Shop` | Screen |
| `UI/Skins` | `UI/Styles/Screens/Skins` | Screen |
| `UI/LoadingScreen` | `UI/Styles/Screens/LoadingScreen` | Screen |
| `UI/SplashScreen` | `UI/Styles/Screens/SplashScreen` | Screen |
| `UI/Maintenance` | `UI/Styles/Screens/Maintenance` | Screen |
| `UI/LobbyStyles` | `UI/Styles/Components/LobbyStyles` | Component |
| `UI/LobbyTab` | `UI/Styles/Components/LobbyTab` | Component |
| `UI/MatchmakingWidget` | `UI/Styles/Components/MatchmakingWidget` | Component |
| `UI/JoystickEditor` | `UI/Styles/Components/JoystickEditor` | Component |
| `UI/Friends` | `UI/Styles/Popups/Friends` | Popup |
| `UI/Modal` | `UI/Styles/Popups/Modal` | Popup |
| `UI/Popup` | `UI/Styles/Popups/Popup` | Popup |
| `UI/Notification` | `UI/Styles/Popups/Notification` | Popup |
| `UI/Toast` | `UI/Styles/Popups/Toast` | Popup |

#### Templates (UXML Files)

| Old Path | New Path | Category |
|----------|----------|----------|
| `UI/Templates/MainMenuTemplate` | `UI/Templates/Screens/MainMenuTemplate` | Screen |
| `UI/Templates/LoginScreenTemplate` | `UI/Templates/Screens/LoginScreenTemplate` | Screen |
| `UI/Templates/CharacterSelectionTemplate` | `UI/Templates/Screens/CharacterSelectionTemplate` | Screen |
| `UI/Templates/GameModeSelectionTemplate` | `UI/Templates/Screens/GameModeSelectionTemplate` | Screen |
| `UI/Templates/MapSelectionTemplate` | `UI/Templates/Screens/MapSelectionTemplate` | Screen |
| `UI/Templates/ProfileTemplate` | `UI/Templates/Screens/ProfileTemplate` | Screen |
| `UI/Templates/SettingsTemplate` | `UI/Templates/Screens/SettingsTemplate` | Screen |
| `UI/Templates/ShopTemplate` | `UI/Templates/Screens/ShopTemplate` | Screen |
| `UI/Templates/SkinsTemplate` | `UI/Templates/Screens/SkinsTemplate` | Screen |
| `UI/Templates/LoadingScreenTemplate` | `UI/Templates/Screens/LoadingScreenTemplate` | Screen |
| `UI/Templates/SplashScreenTemplate` | `UI/Templates/Screens/SplashScreenTemplate` | Screen |
| `UI/Templates/LobbyTemplate` | `UI/Templates/Screens/LobbyTemplate` | Screen |
| `UI/Templates/MaintenanceTemplate` | `UI/Templates/Screens/MaintenanceTemplate` | Screen |
| `UI/CharacterCard` | `UI/Templates/Components/CharacterCard` | Component |
| `UI/GameModeEntry` | `UI/Templates/Components/GameModeEntry` | Component |
| `UI/LobbyPlayerEntry` | `UI/Templates/Components/LobbyPlayerEntry` | Component |
| `UI/DebugConsole` | `UI/Templates/Components/DebugConsole` | Component |
| `UI/Templates/LobbyTabTemplate` | `UI/Templates/Components/LobbyTabTemplate` | Component |
| `UI/Templates/MatchmakingWidget` | `UI/Templates/Components/MatchmakingWidget` | Component |
| `UI/Templates/JoystickEditorTemplate` | `UI/Templates/Components/JoystickEditorTemplate` | Component |
| `UI/Templates/FriendsPopupTemplate` | `UI/Templates/Popups/FriendsPopupTemplate` | Popup |
| `UI/Templates/ModalTemplate` | `UI/Templates/Popups/ModalTemplate` | Popup |
| `UI/Templates/PopupTemplate` | `UI/Templates/Popups/PopupTemplate` | Popup |
| `UI/Templates/NotificationTemplate` | `UI/Templates/Popups/NotificationTemplate` | Popup |
| `UI/Templates/ToastTemplate` | `UI/Templates/Popups/ToastTemplate` | Popup |
| `UI/Templates/UsernamePopupTemplate` | `UI/Templates/Popups/UsernamePopupTemplate` | Popup |

### 3. Search and Replace

Use your IDE to find and replace old paths:

**Find:** `Resources.Load<StyleSheet>("UI/`
**Replace:** `Resources.Load<StyleSheet>("UI/Styles/Screens/`

**Find:** `Resources.Load<VisualTreeAsset>("UI/Templates/`
**Replace:** `Resources.Load<VisualTreeAsset>("UI/Templates/Screens/`

Then manually adjust paths for Components, Popups, and Core styles.

### 4. Common Patterns

#### Loading Core Styles
```csharp
// Always load core styles first
var commonStyles = Resources.Load<StyleSheet>("UI/Styles/Core/Common");
var baseStyles = Resources.Load<StyleSheet>("UI/Styles/Core/BaseUIScreen");
var buttonStyles = Resources.Load<StyleSheet>("UI/Styles/Core/GlobalButtons");

rootVisualElement.styleSheets.Add(commonStyles);
rootVisualElement.styleSheets.Add(baseStyles);
rootVisualElement.styleSheets.Add(buttonStyles);
```

#### Loading Screen Resources
```csharp
// Load screen template and styles
var template = Resources.Load<VisualTreeAsset>("UI/Templates/Screens/MainMenuTemplate");
var styles = Resources.Load<StyleSheet>("UI/Styles/Screens/MainMenu");

template.CloneTree(rootVisualElement);
rootVisualElement.styleSheets.Add(styles);
```

#### Loading Component Resources
```csharp
// Load component template and styles
var template = Resources.Load<VisualTreeAsset>("UI/Templates/Components/LobbyTabTemplate");
var styles = Resources.Load<StyleSheet>("UI/Styles/Components/LobbyTab");
```

#### Loading Popup Resources
```csharp
// Load popup template and styles
var template = Resources.Load<VisualTreeAsset>("UI/Templates/Popups/FriendsPopupTemplate");
var styles = Resources.Load<StyleSheet>("UI/Styles/Popups/Friends");
```

## Benefits of New Structure

1. **Better Organization**: Files grouped by purpose (Screens, Components, Popups)
2. **Easier Discovery**: Know exactly where to find files
3. **Scalability**: Easy to add new categories as project grows
4. **Consistency**: Follows professional project structure standards
5. **Reduced Clutter**: No more mixed files in single folder
6. **Clear Hierarchy**: Core → Screens → Components → Popups

## Troubleshooting

### Resource Not Found Error
```
NullReferenceException: Object reference not set to an instance of an object
```

**Solution:** Update the resource path to the new categorized location.

### Style Not Applied
If styles aren't being applied, ensure you're loading from the correct category:
- Core styles: `UI/Styles/Core/`
- Screen styles: `UI/Styles/Screens/`
- Component styles: `UI/Styles/Components/`
- Popup styles: `UI/Styles/Popups/`

### Template Not Loading
Check that template paths include the category:
- Screen templates: `UI/Templates/Screens/`
- Component templates: `UI/Templates/Components/`
- Popup templates: `UI/Templates/Popups/`

## Questions?

See `Assets/Resources/UI/README.md` for complete documentation of the new structure.
