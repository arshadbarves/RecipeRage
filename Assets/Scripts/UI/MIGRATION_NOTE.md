# UI System Migration Note

## Current Status
This project has two UI systems:

### Legacy UI (being phased out)
- `GameplayUIManager.cs`
- `LobbyUI.cs`
- `MainMenuUI.cs`
- Referenced in `Assets/Editor/Scenes/SceneSetupGenerator.cs`

### New UI System (current)
- Located in `NewUISystem/`
- Modern architecture with `BaseUIScreen`, `UIManager`, `UIScreenRegistry`
- Includes screens: MainMenuScreen, LoadingScreen, LoginScreen, SettingsScreen, etc.

## Migration Plan
1. Update `SceneSetupGenerator.cs` to use NewUISystem screens
2. Remove legacy UI files once migration is complete
3. Update prefabs and scenes to use new UI system

## DO NOT
- Add new features to legacy UI files
- Create new legacy UI components
- All new UI should use NewUISystem architecture
