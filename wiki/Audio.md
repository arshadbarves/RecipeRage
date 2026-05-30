# Audio

## Audio Architecture

VContainer-managed audio system with pooled SFX and music playback.

### Services

| Service | Interface | Lifetime |
|---------|-----------|----------|
| AudioService | IAudioService | Singleton |
| MusicPlayer | IMusicPlayer | Singleton |
| SFXPlayer | ISFXPlayer | Singleton |
| AudioVolumeController | IAudioVolumeController | Singleton (IInitializable) |
| AudioPoolManager | — | Singleton |

### AudioEventListener

Listens to domain events and triggers appropriate SFX/music cues.

## Audio Settings

ScriptableObject-based configuration for volume levels, audio clips, and mixer groups.

## Implementation Notes

- All audio services registered in `RootLifetimeScope`
- `AudioPoolManager` uses object pooling for SFX
- Volume control via `AudioVolumeController` with per-category levels
