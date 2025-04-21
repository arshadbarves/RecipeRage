# Audio System

The Audio System provides a centralized way to manage all audio in the game, including music, sound effects, voice, and UI sounds.

## Features

- Audio source pooling for performance optimization
- Sound categories (Music, SFX, Voice, UI)
- Spatial audio support
- Audio mixer integration
- Fade in/out effects
- Integration with the Save System for audio settings

## Setup

1. Run the "RecipeRage/Audio/Create Audio Mixer" menu item to create the audio mixer
2. Run the "RecipeRage/Audio/Create Audio Database" menu item to create the audio database
3. Run the "RecipeRage/Audio/Create Audio Manager Prefab" menu item to create the AudioManager prefab
4. Add the AudioManager prefab to your scene or let GameBootstrap initialize it

## Usage

### Accessing the Audio Manager

```csharp
AudioManager audioManager = AudioManager.Instance;
```

### Playing Music

```csharp
// Play music with fade in
audioManager.PlayMusic(musicClip, fadeTime: 2f, volume: 0.8f, loop: true);

// Stop music with fade out
audioManager.StopMusic(fadeTime: 2f);

// Pause and resume music
audioManager.PauseMusic();
audioManager.ResumeMusic();
```

### Playing Sound Effects

```csharp
// Play a 2D sound effect
audioManager.PlaySFX(sfxClip, volume: 0.9f, pitch: 1.0f);

// Play a 3D sound effect at a position
audioManager.PlaySFXAtPosition(sfxClip, transform.position, volume: 0.9f, pitch: 1.0f);

// Stop a sound
audioManager.StopSound(audioSource, fadeTime: 0.5f);
```

### Playing Voice

```csharp
// Play a voice clip
audioManager.PlayVoice(voiceClip, volume: 1.0f);

// Play a voice clip at a position
audioManager.PlayVoiceAtPosition(voiceClip, transform.position);
```

### Playing UI Sounds

```csharp
// Play a UI sound
audioManager.PlayUISound(buttonClickSound);
```

### Controlling Volume

```csharp
// Set volume levels
audioManager.SetMasterVolume(0.8f);
audioManager.SetMusicVolume(0.7f);
audioManager.SetSFXVolume(0.9f);
audioManager.SetVoiceVolume(0.85f);

// Mute/unmute all audio
audioManager.SetMute(true);
```

### Using the Audio Database

```csharp
// Get the audio database
AudioDatabase audioDatabase = Resources.Load<AudioDatabase>("AudioDatabase");

// Get clips by ID
AudioClip musicClip = audioDatabase.GetMusicClip("main_theme");
AudioClip sfxClip = audioDatabase.GetSFXClip("explosion");
AudioClip voiceClip = audioDatabase.GetVoiceClip("player_greeting");
AudioClip uiClip = audioDatabase.GetUIClip("button_click");

// Play clips
audioManager.PlayMusic(musicClip);
audioManager.PlaySFX(sfxClip);
```

## Audio Mixer

The audio mixer has the following groups:
- Master: Controls overall volume
- Music: Controls music volume
- SFX: Controls sound effects volume
- Voice: Controls voice volume
- UI: Controls UI sound volume (child of SFX)

## Audio Database

The audio database is a ScriptableObject that stores references to audio clips with unique IDs. It provides a convenient way to access audio clips by ID rather than direct references.

### Adding Clips to the Database

1. Select the AudioDatabase asset in the Project window
2. Add clips to the appropriate lists (Music, SFX, Voice, UI)
3. Assign a unique ID and description for each clip

## Integration with Save System

The Audio System automatically integrates with the Save System to save and load audio settings. When the SaveManager changes settings, the AudioManager will apply those changes to the audio mixer.

```csharp
// Update audio settings through the save system
SaveManager.Instance.UpdateSettings(settings => {
    settings.MasterVolume = 0.8f;
    settings.MusicVolume = 0.7f;
    settings.SfxVolume = 0.9f;
    settings.VoiceVolume = 0.85f;
    settings.IsMuted = false;
});
```
