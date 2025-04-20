# Save System

The Save System is a modular component that handles saving and loading game settings and data. It is designed to be independent of gameplay and can be used in any Unity project.

## Features

- Save and load player settings (audio, graphics, gameplay)
- Automatic migration from PlayerPrefs to JSON-based save system
- Event-based notification system for settings changes
- Extensible design for adding new settings

## Components

### SaveManager

The `SaveManager` is a singleton that manages saving and loading settings and game data. It provides methods for:

- Getting and updating player settings
- Saving and loading settings to/from disk
- Migrating settings from PlayerPrefs
- Applying settings to the game

### PlayerSettings

The `PlayerSettings` class stores player settings that persist between game sessions, including:

- Audio settings (volume levels, mute)
- Graphics settings (fullscreen, quality, resolution)
- Gameplay settings (camera shake, auto-pickup, etc.)
- Player info (name)

## Usage

### Initializing the Save System

The SaveManager is automatically initialized when the game starts. It loads settings from disk or creates default settings if none exist.

```csharp
// Get the SaveManager instance
SaveManager saveManager = SaveManager.Instance;
```

### Getting and Updating Settings

```csharp
// Get the current settings
PlayerSettings settings = SaveManager.Instance.GetSettings();

// Update a setting
SaveManager.Instance.UpdateSetting("MusicVolume", 0.8f);
```

### Subscribing to Settings Changes

```csharp
// Subscribe to settings changed event
SaveManager.Instance.OnSettingsChanged += HandleSettingsChanged;

// Handle settings changed
private void HandleSettingsChanged()
{
    // Apply settings to your systems
    PlayerSettings settings = SaveManager.Instance.GetSettings();
    // ...
}
```

### Migrating from PlayerPrefs

If you're transitioning from PlayerPrefs to this save system, you can migrate existing settings:

```csharp
SaveManager.Instance.MigrateFromPlayerPrefs();
```

## Integration with Other Systems

The Save System is designed to work with other systems in the game, particularly the Audio System. When settings are changed, the Audio System automatically applies the new settings.
