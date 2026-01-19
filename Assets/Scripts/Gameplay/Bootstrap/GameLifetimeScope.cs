using System;
using Core.Animation;
using Core.Audio;
using Core.Auth;
using Core.Auth;
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
using Gameplay.UI.Features.Auth;
using Gameplay.UI.Features.Character;
using Gameplay.UI.Features.Loading;
using Gameplay.UI.Features.Lobby;
using Gameplay.UI.Features.MainMenu;
using Gameplay.UI.Features.Maps;
using Gameplay.UI.Features.Matchmaking;
using Gameplay.UI.Features.Profile;
using Gameplay.UI.Features.Settings;
using Gameplay.UI.Features.Shop;
using Gameplay.UI.Features.Social;
using Gameplay.UI.Features.System;
using Gameplay.UI.Features.User;
using Gameplay.App.State;
using Gameplay.App.State.States;
using Gameplay.UI;
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



            // Event Bus
            builder.Register<EventBus>(Lifetime.Singleton).As<IEventBus>();

            // Localization
            builder.Register<LocalizationManager>(Lifetime.Singleton).As<ILocalizationManager>().As<IInitializable>();

            // 2. Animation & UI
            builder.Register<DOTweenUIAnimator>(Lifetime.Singleton).As<IUIAnimator>();
            builder.Register<DOTweenTransformAnimator>(Lifetime.Singleton).As<ITransformAnimator>();
            builder.Register<AnimationService>(Lifetime.Singleton).As<IAnimationService>();

            // UIService
            // Find UIDocument in the scene
            var uiDocument = GameObject.FindObjectOfType<UnityEngine.UIElements.UIDocument>();
            if (uiDocument != null)
            {
                builder.RegisterInstance(uiDocument);
            }
            else
            {
                GameLogger.LogError("UIDocument not found in scene! UI will not work.");
            }
            builder.Register<UIService>(Lifetime.Singleton).As<IUIService>().As<IStartable>().As<IDisposable>();

            // Register UI Screens (Transient - instantiated by UIService)
            // System
            builder.Register<SplashView>(Lifetime.Transient);
            builder.Register<LoadingView>(Lifetime.Transient);
            builder.Register<MaintenanceView>(Lifetime.Transient);

            // Auth
            builder.Register<LoginView>(Lifetime.Transient);

            // Screens
            builder.Register<MainMenuView>(Lifetime.Transient);
            builder.Register<ProfileView>(Lifetime.Transient);
            builder.Register<CharacterDetailsView>(Lifetime.Transient);
            builder.Register<MapSelectionView>(Lifetime.Transient);
            builder.Register<MatchmakingView>(Lifetime.Transient);
            builder.Register<SettingsView>(Lifetime.Transient);

            // Popups & Components
            builder.Register<NotificationView>(Lifetime.Transient).AsImplementedInterfaces().AsSelf();
            builder.Register<UsernamePopup>(Lifetime.Transient);
            builder.Register<FriendsPopup>(Lifetime.Transient);
            builder.Register<NoInternetPopup>(Lifetime.Transient);
            builder.Register<JoystickEditorUI>(Lifetime.Transient);

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

            builder.Register<ConnectivityService>(Lifetime.Singleton).As<IConnectivityService>().As<IStartable>().As<IDisposable>();

            builder.Register<MaintenanceService>(Lifetime.Singleton).As<IMaintenanceService>();

            // 6. State Management
            builder.Register<GameStateManager>(Lifetime.Singleton).As<IGameStateManager>().As<IDisposable>().As<ITickable>();

            // Register Games States
            builder.Register<BootstrapState>(Lifetime.Transient);
            builder.Register<LoginState>(Lifetime.Transient);
            builder.Register<MainMenuState>(Lifetime.Transient);
            builder.Register<MatchmakingState>(Lifetime.Transient);
            builder.Register<SessionLoadingState>(Lifetime.Transient);
            builder.Register<GameplayState>(Lifetime.Transient);
            builder.Register<GameOverState>(Lifetime.Transient);

            // 7. Auth (Unified EOS + UGS)
            var ugsConfig = Resources.Load<UGSConfig>("UGSConfig");
            if (ugsConfig != null) builder.RegisterInstance(ugsConfig);
            else GameLogger.LogError("UGSConfig asset not found in Resources/! Please create it.");

            builder.Register<AuthenticationService>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            // Networking Core (Low level)
            builder.Register<PlayerNetworkManager>(Lifetime.Singleton).As<IPlayerNetworkManager>();

            // 9. Bootstrapper (EntryPoint)
            builder.Register<GameBootstrapper>(Lifetime.Singleton).As<IStartable>();
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

            // ViewModels
            builder.Register<LoadingViewModel>(Lifetime.Transient);
            builder.Register<LobbyViewModel>(Lifetime.Transient);
            builder.Register<MainMenuViewModel>(Lifetime.Transient);
            builder.Register<CharacterViewModel>(Lifetime.Transient);
            builder.Register<LoginViewModel>(Lifetime.Transient);
            builder.Register<MatchmakingViewModel>(Lifetime.Transient);
            builder.Register<SettingsViewModel>(Lifetime.Transient);
            builder.Register<ShopViewModel>(Lifetime.Transient);
            builder.Register<UsernameViewModel>(Lifetime.Transient);

            // Audio System
            // AudioPoolManager needs a parent transform, we use this scope's transform
            builder.Register<AudioPoolManager>(Lifetime.Singleton).WithParameter(transform);

            builder.Register<AudioVolumeController>(Lifetime.Singleton).As<IAudioVolumeController>().As<IInitializable>();
            builder.Register<SFXPlayer>(Lifetime.Singleton).As<ISFXPlayer>();
            builder.Register<MusicPlayer>(Lifetime.Singleton).As<IMusicPlayer>();
            builder.Register<AudioService>(Lifetime.Singleton).As<IAudioService>();

            GameLogger.Log("Audio System registered in GameLifetimeScope");
        }
    }
}
