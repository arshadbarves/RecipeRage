# Audio System

The Audio System is a modular component that handles all audio playback in the game. It is designed to be independent of gameplay and can be used in any Unity project.

## Features

- Centralized audio management
- Volume control for different audio channels (master, music, SFX)
- Audio pooling for performance
- Integration with the Save System for persistent audio settings
- Support for 3D positional audio
- Audio clip library for organized audio assets

## Components

### AudioManager

The `AudioManager` is a singleton that manages all audio playback in the game. It provides methods for:

- Playing music and sound effects
- Controlling volume levels
- Muting/unmuting audio
- Applying audio settings from the Save System

### AudioClipLibrary

The `AudioClipLibrary` is a ScriptableObject that stores and organizes audio clips. It categorizes clips into:

- Music
- Ambience
- UI
- Player
- Cooking
- Items
- Environment

## Usage

### Initializing the Audio System

The AudioManager is automatically initialized when the game starts. It loads settings from the SaveManager and sets up audio sources.

```csharp
// Get the AudioManager instance
AudioManager audioManager = AudioManager.Instance;
```

### Playing Music

```csharp
// Play music by name from the audio clip library
AudioManager.Instance.PlayMusicByName("MainMenuMusic");

// Play predefined music
AudioManager.Instance.PlayMainMenuMusic();
AudioManager.Instance.PlayGameplayMusic();
AudioManager.Instance.PlayVictoryMusic();
AudioManager.Instance.PlayDefeatMusic();

// Stop music
AudioManager.Instance.StopMusic();
```

### Playing Sound Effects

```csharp
// Play a sound effect by name from the audio clip library
AudioManager.Instance.PlaySfxByName("ButtonClick");

// Play a sound effect at a position in 3D space
AudioManager.Instance.PlaySfxAtPositionByName("Explosion", explosionPosition);
```

### Controlling Volume

```csharp
// Set volume levels
AudioManager.Instance.SetMasterVolume(0.8f);
AudioManager.Instance.SetMusicVolume(0.7f);
AudioManager.Instance.SetSfxVolume(0.9f);

// Mute/unmute audio
AudioManager.Instance.SetMuted(true);
```

## Integration with Save System

The Audio System automatically integrates with the Save System. When audio settings are changed in the SaveManager, the AudioManager applies those settings to the audio sources.

```csharp
// Update audio settings in the Save System
SaveManager.Instance.UpdateSetting("MusicVolume", 0.8f);
// The AudioManager will automatically apply this change
```

## Creating an Audio Clip Library

1. Right-click in the Project window
2. Select "Create > RecipeRage > Audio > Audio Clip Library"
3. Add audio clips to the library
4. Assign the library to the AudioManager in the Inspector
