# To use this system effectively:

1. Create your audio mixer asset in Unity
2. Set up your playlists and SFX libraries
3. Configure the AudioContainer with all your audio assets
4. Use the AudioService in your game components
5. Trigger audio events using the defined constants

Example usage in gameplay components:

```csharp
// CookingStation.cs
public class CookingStation : MonoBehaviour
{
    [Inject] private AudioService audioService;
    
    private async void StartCooking()
    {
        await audioService.PlayGameSound(AudioEvents.COOKING_START, transform.position);
    }

    private async void CompleteCooking()
    {
        await audioService.PlayGameSound(AudioEvents.COOKING_COMPLETE, transform.position);
    }
}

// PlayerController.cs
public class PlayerController : MonoBehaviour
{
    [Inject] private AudioService audioService;
    private float footstepTimer;
    private const float FOOTSTEP_INTERVAL = 0.4f;

    private async void Update()
    {
        if (IsMoving())
        {
            footstepTimer += Time.deltaTime;
            if (footstepTimer >= FOOTSTEP_INTERVAL)
            {
                footstepTimer = 0f;
                await audioService.PlayGameSound(AudioEvents.PLAYER_MOVE, transform.position);
            }
        }
    }

    private async void OnDash()
    {
        await audioService.PlayGameSound(AudioEvents.PLAYER_DASH, transform.position);
    }
}

// UIButton.cs
public class UIButton : MonoBehaviour
{
    [Inject] private AudioService audioService;
    
    public void OnClick()
    {
        audioService.PlayUISound(AudioEvents.BUTTON_CLICK);
    }
}
```

5. Register the audio system in your dependency injection container:

```csharp
// GameInstaller.cs
public class GameInstaller : MonoInstaller
{
    [SerializeField] private AudioContainer audioContainer;
    
    public override void Install(IContainerBuilder builder)
    {
        // Register audio components
        builder.RegisterInstance(audioContainer);
        builder.Register<AudioMixer>(Lifetime.Singleton);
        builder.Register<AudioManager>(Lifetime.Singleton);
        builder.Register<AudioService>(Lifetime.Singleton);
    }
}
```

6. Create an AudioMixerPreset asset:

```csharp
// AudioMixerPreset.cs
[CreateAssetMenu(fileName = "AudioMixerPreset", menuName = "RecipeRage/Audio/MixerPreset")]
public class AudioMixerPreset : ScriptableObject
{
    [System.Serializable]
    public class SnapshotSettings
    {
        public string name;
        public float[] parameters;
    }

    public AudioMixerGroup masterGroup;
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup sfxGroup;
    public AudioMixerGroup voiceGroup;

    public SnapshotSettings[] snapshots;
}
```

This integration provides:
- Centralized audio event management
- Easy-to-use audio API for all game components
- Organized sound libraries
- Smooth transitions between game states
- Proper volume and mixer management
- Mobile-optimized audio playback
- Persistent audio settings
- Spatial audio support
- Event-based sound triggering

To use this system effectively:
1. Create your audio mixer asset in Unity
2. Set up your playlists and SFX libraries
3. Configure the AudioContainer with all your audio assets
4. Use the AudioService in your game components
5. Trigger audio events using the defined constants```