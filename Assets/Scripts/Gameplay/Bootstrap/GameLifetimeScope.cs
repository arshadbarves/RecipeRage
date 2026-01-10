using System;
using Core.Animation;
using Core.Audio;
using Core.Auth;
using Core.Auth.Core;
using Core.Localization;
using Core.Logging;
using Core.Networking;
using Core.Networking.Services;
using Core.Persistence;
using Core.Persistence.Factory;
using Core.RemoteConfig;
using Core.RemoteConfig.Interfaces;
using Core.RemoteConfig.Providers;
using Core.RemoteConfig.Services;
using Core.Session;
using Core.Shared.Events;
using Core.UI;
using Core.UI.Interfaces;
using Gameplay.App.State;
using Gameplay.App.State.States;
using UnityEngine;
using VContainer;
using VContainer.Unity;

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
            // 1. Core Utilities
            // Logging
            builder.Register<LoggingService>(Lifetime.Singleton)
                .As<ILoggingService>()
                .As<IStartable>()
                .WithParameter(5000); // maxLogEntries

            // Entry Point
            builder.RegisterEntryPoint<GameBootstrapper>();

            // Event Bus
            builder.Register<EventBus>(Lifetime.Singleton).As<IEventBus>();

            // Localization
            builder.Register<LocalizationManager>(Lifetime.Singleton).As<ILocalizationManager>();

            // 2. Animation & UI
            builder.Register<DOTweenUIAnimator>(Lifetime.Singleton).As<IUIAnimator>();
            builder.Register<DOTweenTransformAnimator>(Lifetime.Singleton).As<ITransformAnimator>();
            builder.Register<AnimationService>(Lifetime.Singleton).As<IAnimationService>();

            // UIService
            builder.Register<UIService>(Lifetime.Singleton).As<IUIService>().As<IDisposable>();

            // 3. Persistence
            builder.Register<EncryptionService>(Lifetime.Singleton)
                .As<IEncryptionService>()
                .WithParameter("RecipeRage");
            builder.Register<StorageProviderFactory>(Lifetime.Singleton);
            builder.Register<SaveService>(Lifetime.Singleton).As<ISaveService>();

            // 4. Remote Config
            builder.Register<FirebaseConfigProvider>(Lifetime.Singleton).As<IConfigProvider>();
            builder.Register<RemoteConfigService>(Lifetime.Singleton) .As<IRemoteConfigService>() .As<IStartable>();

            // 5. Time
            builder.Register<NTPTimeService>(Lifetime.Singleton).As<INTPTimeService>().As<IInitializable>();

            // 6. Session & Connectivity
            builder.Register<SessionManager>(Lifetime.Singleton).AsSelf().As<IInitializable>().As<IDisposable>();

            builder.Register<ConnectivityService>(Lifetime.Singleton).As<IConnectivityService>().As<IDisposable>();

            builder.Register<MaintenanceService>(Lifetime.Singleton).As<IMaintenanceService>();

            // 6. State Management
            builder.Register<StateFactory>(Lifetime.Singleton).As<IStateFactory>();
            builder.Register<GameStateManager>(Lifetime.Singleton).As<IGameStateManager>().As<IDisposable>();

            // Register Games States
            builder.Register<BootstrapState>(Lifetime.Transient);
            builder.Register<LoginState>(Lifetime.Transient);
            builder.Register<MainMenuState>(Lifetime.Transient);
            builder.Register<LobbyState>(Lifetime.Transient);
            builder.Register<MatchmakingState>(Lifetime.Transient);
            builder.Register<SessionLoadingState>(Lifetime.Transient);
            builder.Register<GameplayState>(Lifetime.Transient);
            builder.Register<GameOverState>(Lifetime.Transient);

            // 7. Auth
            // EOS Auth
            builder.Register<EOSAuthService>(Lifetime.Singleton)
                .As<IAuthService>();

            // UGS Auth
            var ugsConfig = Resources.Load<UGSConfig>("UGSConfig");
            if (ugsConfig != null)
            {
                builder.RegisterInstance(ugsConfig);
                builder.Register<UGSAuthenticationManager>(Lifetime.Singleton).As<IAuthenticationManager>();
            }
            else
            {
                GameLogger.LogError("UGSConfig asset not found in Resources/! Please create it.");
            }

            // Networking Core (Low level)
            builder.Register<PlayerNetworkManager>(Lifetime.Singleton).As<IPlayerNetworkManager>();
        }

        private void RegisterAudioSystem(IContainerBuilder builder)
        {
            // Load Audio Settings
            Core.Audio.AudioSettings audioSettings = Resources.Load<Core.Audio.AudioSettings>("Audio/AudioSettings");
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

            GameLogger.Log("Audio System registered in GameLifetimeScope");
        }
    }
}
