using VContainer;
using VContainer.Unity;
using Core.Audio;
using Core.Logging;
using UnityEngine;

namespace Gameplay.Bootstrap
{
    public class GameLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            RegisterAudioSystem(builder);
        }

        private void RegisterAudioSystem(IContainerBuilder builder)
        {
            // Load Audio Settings
            var audioSettings = Resources.Load<Core.Audio.AudioSettings>("Audio/AudioSettings");
            if (audioSettings != null)
            {
                builder.RegisterInstance(audioSettings);
            }
            else
            {
                GameLogger.LogError("AudioSettings asset not found in Resources/Audio/! Please create it.");
            }

            // Register Audio Components
            // AudioPoolManager needs a parent transform, we use this scope's transform
            builder.Register<AudioPoolManager>(Lifetime.Singleton).WithParameter(transform);

            builder.Register<AudioVolumeController>(Lifetime.Singleton).As<IAudioVolumeController>();
            builder.Register<SFXPlayer>(Lifetime.Singleton).As<ISFXPlayer>();
            builder.Register<MusicPlayer>(Lifetime.Singleton).As<IMusicPlayer>();
            builder.Register<AudioService>(Lifetime.Singleton).As<IAudioService>();

            GameLogger.Log("Audio System registered in GameLifetimeScope");
        }
    }
}
