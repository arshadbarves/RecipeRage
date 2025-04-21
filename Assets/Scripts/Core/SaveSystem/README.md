# Save System

The Save System provides a centralized way to save and load game data, including settings, player progress, and statistics.

## Features

- JSON-based serialization for robust data storage
- Optional encryption for sensitive data
- Separate data classes for different types of saved data
- Automatic migration from PlayerPrefs
- Event-based notification system for data changes

## Setup

1. Run the "RecipeRage/Create/All Manager Prefabs" menu item to create all manager prefabs including the SaveManager
   - Alternatively, run "RecipeRage/Save System/Create Save Manager Prefab" to create only the SaveManager prefab
2. Add the SaveManager prefab to your scene or let GameBootstrap initialize it

## Usage

### Accessing the Save Manager

```csharp
SaveManager saveManager = SaveManager.Instance;
```

### Getting Settings

```csharp
// Get current settings
GameSettingsData settings = saveManager.GetSettings();

// Use settings
float musicVolume = settings.MusicVolume;
bool fullscreen = settings.IsFullscreen;
```

### Saving Settings

```csharp
// Get current settings
GameSettingsData settings = saveManager.GetSettings();

// Modify settings
settings.MusicVolume = 0.8f;
settings.IsFullscreen = true;

// Save settings
saveManager.SaveSettings(settings);
```

### Updating Specific Settings

```csharp
// Update specific settings without changing others
saveManager.UpdateSettings(settings => {
    settings.MusicVolume = 0.8f;
    settings.SfxVolume = 0.9f;
});
```

### Player Progress

```csharp
// Get player progress
PlayerProgressData progress = saveManager.GetPlayerProgress();

// Check if a character is unlocked
bool isUnlocked = progress.IsCharacterUnlocked("chef_1");

// Unlock a character
progress.UnlockCharacter("chef_2");

// Save progress
saveManager.SavePlayerProgress(progress);
```

### Player Stats

```csharp
// Get player stats
PlayerStatsData stats = saveManager.GetPlayerStats();

// Add experience
bool leveledUp = stats.AddExperience(100);

// Add currency
stats.AddCoins(50);
stats.AddGems(5);

// Save stats
saveManager.SavePlayerStats(stats);
```

### Listening for Changes

```csharp
// Subscribe to settings changes
saveManager.OnSettingsChanged += OnSettingsChanged;

// Handle settings changes
private void OnSettingsChanged(GameSettingsData settings)
{
    // Update UI or apply settings
    ApplyAudioSettings(settings);
}
```

## Data Classes

### GameSettingsData

Contains all game settings, including:
- Audio settings (volume levels, mute)
- Graphics settings (resolution, quality, fullscreen)
- Gameplay settings (sensitivity, camera shake, etc.)

### PlayerProgressData

Tracks player progression through the game:
- Unlocked content (characters, maps, recipes)
- Game progress (high scores, best times)
- Achievements
- Tutorial progress

### PlayerStatsData

Stores player statistics:
- Player info (name, level, experience)
- Currency (coins, gems)
- Game stats (games played, orders completed, etc.)
- Character and game mode usage

## Advanced

### Deleting Save Data

```csharp
// Delete all saved data
saveManager.DeleteAllData();
```

### Importing from PlayerPrefs

```csharp
// Import settings from PlayerPrefs (for migration)
saveManager.ImportFromPlayerPrefs();
```
