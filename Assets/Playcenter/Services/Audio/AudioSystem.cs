namespace Playcenter.Services
{
    /// <summary>
    /// Subscribes to gameplay events on the bus and maps them to SFX.
    /// Gameplay knows nothing about audio. Event subscriptions are added
    /// in Slice 1 when the gameplay event types exist (wired from the game
    /// assembly — Playcenter.Services must not reference game types).
    /// </summary>
    public sealed class AudioSystem
    {
        private readonly IAudioService _audio;

        public AudioSystem(IAudioService audio)
        {
            _audio = audio;
        }

        public void Initialize(IEventBus bus)
        {
            // Game-side wiring (GameplayAudioWiring in RecipeRage assembly)
            // subscribes gameplay events here in Slice 1+.
        }
    }
}
