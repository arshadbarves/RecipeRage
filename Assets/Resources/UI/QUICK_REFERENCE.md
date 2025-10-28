# UI Resources Quick Reference

## Resource Loading Patterns

### Core Styles (Load First)
```csharp
Resources.Load<StyleSheet>("UI/Styles/Core/Common")
Resources.Load<StyleSheet>("UI/Styles/Core/BaseUIScreen")
Resources.Load<StyleSheet>("UI/Styles/Core/GlobalButtons")
```

### Screen Resources
```csharp
// Template
Resources.Load<VisualTreeAsset>("UI/Templates/Screens/MainMenuTemplate")
Resources.Load<VisualTreeAsset>("UI/Templates/Screens/LoginScreenTemplate")
Resources.Load<VisualTreeAsset>("UI/Templates/Screens/CharacterSelectionTemplate")

// Styles
Resources.Load<StyleSheet>("UI/Styles/Screens/MainMenu")
Resources.Load<StyleSheet>("UI/Styles/Screens/LoginScreen")
Resources.Load<StyleSheet>("UI/Styles/Screens/CharacterSelection")
```

### Component Resources
```csharp
// Template
Resources.Load<VisualTreeAsset>("UI/Templates/Components/LobbyTabTemplate")
Resources.Load<VisualTreeAsset>("UI/Templates/Components/MatchmakingWidget")
Resources.Load<VisualTreeAsset>("UI/Templates/Components/CharacterCard")

// Styles
Resources.Load<StyleSheet>("UI/Styles/Components/LobbyTab")
Resources.Load<StyleSheet>("UI/Styles/Components/MatchmakingWidget")
Resources.Load<StyleSheet>("UI/Styles/Components/LobbyStyles")
```

### Popup Resources
```csharp
// Template
Resources.Load<VisualTreeAsset>("UI/Templates/Popups/FriendsPopupTemplate")
Resources.Load<VisualTreeAsset>("UI/Templates/Popups/ModalTemplate")
Resources.Load<VisualTreeAsset>("UI/Templates/Popups/ToastTemplate")

// Styles
Resources.Load<StyleSheet>("UI/Styles/Popups/Friends")
Resources.Load<StyleSheet>("UI/Styles/Popups/Modal")
Resources.Load<StyleSheet>("UI/Styles/Popups/Toast")
```

### Images
```csharp
Resources.Load<Texture2D>("UI/Images/logo")
Resources.Load<Texture2D>("UI/Images/character-icon")
```

### Data
```csharp
Resources.Load<TextAsset>("UI/Data/Maps")
```

## Folder Structure

```
UI/
├── Styles/
│   ├── Core/          → Common, BaseUIScreen, GlobalButtons
│   ├── Screens/       → MainMenu, LoginScreen, CharacterSelection, etc.
│   ├── Components/    → LobbyTab, MatchmakingWidget, JoystickEditor
│   └── Popups/        → Friends, Modal, Toast, Notification
├── Templates/
│   ├── Screens/       → MainMenuTemplate, LoginScreenTemplate, etc.
│   ├── Components/    → LobbyTabTemplate, CharacterCard, etc.
│   ├── Popups/        → FriendsPopupTemplate, ModalTemplate, etc.
│   └── Common/        → Shared elements
├── Images/            → All UI images
└── Data/              → JSON data files
```

## Category Decision Tree

**Where should I put my file?**

1. Is it a USS stylesheet?
   - Core utility/base? → `Styles/Core/`
   - Full screen? → `Styles/Screens/`
   - Reusable component? → `Styles/Components/`
   - Popup/modal/toast? → `Styles/Popups/`

2. Is it a UXML template?
   - Full screen? → `Templates/Screens/`
   - Reusable component? → `Templates/Components/`
   - Popup/modal/toast? → `Templates/Popups/`
   - Shared element? → `Templates/Common/`

3. Is it an image? → `Images/`

4. Is it JSON data? → `Data/`

## Naming Conventions

- **Screens**: `MainMenuTemplate.uxml`, `MainMenu.uss`
- **Components**: `LobbyTabTemplate.uxml`, `LobbyTab.uss`
- **Popups**: `FriendsPopupTemplate.uxml`, `Friends.uss`
- **Core**: `Common.uss`, `BaseUIScreen.uss`, `GlobalButtons.uss`

## Complete File List

### Core Styles
- BaseUIScreen.uss
- Common.uss
- GlobalButtons.uss

### Screen Styles
- CharacterSelection.uss
- CharacterSelectionScreen.uss
- CompanySplashScreen.uss
- GameLogoSplashScreen.uss
- GameModeSelection.uss
- GameModeSelectionScreen.uss
- LoadingScreen.uss
- LoginScreen.uss
- MainMenu.uss
- MainMenuUI.uss
- Maintenance.uss
- MapSelection.uss
- Profile.uss
- Settings.uss
- SettingsScreen.uss
- Shop.uss
- Skins.uss
- SplashScreen.uss

### Component Styles
- JoystickEditor.uss
- LobbyStyles.uss
- LobbyTab.uss
- MatchmakingWidget.uss

### Popup Styles
- Friends.uss
- Modal.uss
- Notification.uss
- Popup.uss
- Toast.uss

### Screen Templates
- CharacterSelectionTemplate.uxml
- GameModeSelectionTemplate.uxml
- LoadingScreenTemplate.uxml
- LobbyTemplate.uxml
- LoginScreenTemplate.uxml
- MainMenuTemplate.uxml
- MaintenanceTemplate.uxml
- MapSelectionTemplate.uxml
- ProfileTemplate.uxml
- SettingsTemplate.uxml
- ShopTemplate.uxml
- SkinsTemplate.uxml
- SplashScreenTemplate.uxml

### Component Templates
- CharacterCard.uxml
- DebugConsole.uxml
- GameModeEntry.uxml
- JoystickEditorTemplate.uxml
- LobbyPlayerEntry.uxml
- LobbyTabTemplate.uxml
- MatchmakingWidget.uxml

### Popup Templates
- FriendsPopupTemplate.uxml
- ModalTemplate.uxml
- NotificationTemplate.uxml
- PopupTemplate.uxml
- ToastTemplate.uxml
- UsernamePopupTemplate.uxml
