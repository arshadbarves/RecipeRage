using VContainer;
using VContainer.Unity;
using Core.Audio;
using Core.Logging;
using Core.Localization;
using Core.Auth;
using Core.Networking;
using Core.Networking.Services;
using Core.Networking.Interfaces;
using UnityEngine;

namespace Gameplay.Bootstrap
{
    public class GameLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            RegisterCoreSystems(builder);
            RegisterAudioSystem(builder);
        }

        private void RegisterCoreSystems(IContainerBuilder builder)
        {
            // Logging
            builder.Register<LoggingService>(Lifetime.Singleton)
                .As<ILoggingService>()
                .WithParameter(5000); // maxLogEntries
            builder.RegisterEntryPoint<GameLoggerInitializer>();

            // Localization
            builder.Register<LocalizationManager>(Lifetime.Singleton).As<ILocalizationManager>();
            // Logging via GameLogger is not available here yet.

            // Auth
            var ugsConfig = Resources.Load<UGSConfig>("UGSConfig");
            if (ugsConfig != null)
            {
                builder.RegisterInstance(ugsConfig);
                builder.Register<UGSAuthenticationManager>(Lifetime.Singleton).As<IAuthenticationManager>();
            }
            else
            {
                // Fallback to Unity Debug as GameLogger is not ready
                Debug.LogError("UGSConfig asset not found in Resources/! Please create it.");
            }

            // Networking Core
            builder.Register<PlayerNetworkManager>(Lifetime.Singleton).As<IPlayerNetworkManager>();
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
                Debug.LogError("AudioSettings asset not found in Resources/Audio/! Please create it.");
            }

            // Register Audio Components
            // AudioPoolManager needs a parent transform, we use this scope's transform
            builder.Register<AudioPoolManager>(Lifetime.Singleton).WithParameter(transform);

            builder.Register<AudioVolumeController>(Lifetime.Singleton).As<IAudioVolumeController>();
            builder.Register<SFXPlayer>(Lifetime.Singleton).As<ISFXPlayer>();
            builder.Register<MusicPlayer>(Lifetime.Singleton).As<IMusicPlayer>();
            builder.Register<AudioService>(Lifetime.Singleton).As<IAudioService>();

            // GameLogger.Log("Audio System registered in GameLifetimeScope");
        }
    }
}
