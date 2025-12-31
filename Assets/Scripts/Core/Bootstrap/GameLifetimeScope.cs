using System;
using System.Collections.Generic;
using Core.Animation;
using Core.Events;
using Core.Logging;
using Core.Maintenance;
using Core.Networking;
using Core.RemoteConfig;
using Core.SaveSystem;
using Core.State;
using Core.State.States;
using RecipeRage.Modules.Auth.Core;
using UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Core.Bootstrap
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField]
        private UIDocumentProvider _uiDocumentProvider;

        protected override void Configure(IContainerBuilder builder)
        {
            // Foundation
            builder.Register<LoggingService>(Lifetime.Singleton).AsImplementedInterfaces().WithParameter("maxLogEntries", 10000);
            builder.Register<Events.EventBus>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<StorageProviderFactory>(Lifetime.Singleton);
            builder.Register<EncryptionService>(Lifetime.Singleton).AsImplementedInterfaces().WithParameter("key", "RecipeRage");
            builder.Register<SaveService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<NTPTimeService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<RemoteConfigService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<ConnectivityService>(Lifetime.Singleton).AsImplementedInterfaces();

            // UI System
            builder.Register<DOTweenUIAnimator>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<DOTweenTransformAnimator>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<AnimationService>(Lifetime.Singleton).AsImplementedInterfaces();
            
            builder.Register<UIService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.RegisterComponent(_uiDocumentProvider);

            // Core
            builder.Register<MaintenanceService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<GameStateManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<StateFactory>(Lifetime.Singleton).AsImplementedInterfaces();

            // Game States (Transient - created on demand)
            builder.Register<BootstrapState>(Lifetime.Transient);
            builder.Register<LoginState>(Lifetime.Transient);
            builder.Register<MainMenuState>(Lifetime.Transient);
            builder.Register<LobbyState>(Lifetime.Transient);
            builder.Register<MatchmakingState>(Lifetime.Transient);
            builder.Register<GameplayState>(Lifetime.Transient);
            builder.Register<GameOverState>(Lifetime.Transient);

            // UI Screens (Transient)
            builder.Register<UI.Screens.LoadingScreen>(Lifetime.Transient);
            builder.Register<UI.Screens.MainMenuScreen>(Lifetime.Transient);
            builder.Register<UI.Screens.ProfileScreen>(Lifetime.Transient);
            builder.Register<UI.Screens.MaintenanceScreen>(Lifetime.Transient);
            builder.Register<UI.Screens.SplashScreen>(Lifetime.Transient);
            builder.Register<UI.Screens.NotificationScreen>(Lifetime.Transient);
            builder.Register<UI.Screens.MatchmakingScreen>(Lifetime.Transient);
            builder.Register<UI.Screens.CharacterDetailsScreen>(Lifetime.Transient);
            builder.Register<UI.Screens.MapSelectionScreen>(Lifetime.Transient);
            builder.Register<UI.Popups.FriendsPopup>(Lifetime.Transient);
            builder.Register<UI.Popups.UsernamePopup>(Lifetime.Transient);
            builder.Register<UI.Popups.NoInternetPopup>(Lifetime.Transient);
            builder.Register<UI.JoystickEditorUI>(Lifetime.Transient);

            // Session Management
            builder.Register<SessionManager>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

            // Auth - Inject SessionManager.CreateSession delegate
            builder.Register<EOSAuthService>(Lifetime.Singleton).AsImplementedInterfaces()
                .WithParameter<Action>((container) => container.Resolve<SessionManager>().CreateSession);

            // Lifecycle Bridge
            builder.RegisterEntryPoint<RootBootstrapper>();
        }
    }

    /// <summary>
    /// Bridges VContainer's Startable with the project's custom IInitializable interface
    /// </summary>
    public class RootBootstrapper : IStartable
    {
        private readonly IEnumerable<IInitializable> _initializables;
        private readonly IUIService _uiService;
        private readonly UIDocumentProvider _uiDocumentProvider;
        private readonly ILoggingService _loggingService;

        public RootBootstrapper(
            IEnumerable<IInitializable> initializables, 
            IUIService uiService,
            UIDocumentProvider uiDocumentProvider,
            ILoggingService loggingService)
        {
            _initializables = initializables;
            _uiService = uiService;
            _uiDocumentProvider = uiDocumentProvider;
            _loggingService = loggingService;
        }

        public void Start()
        {
            _loggingService.LogInfo("[RootBootstrapper] Starting foundation initialization...");

            // 1. UI Binding (Critical first step from manual registration)
            _uiDocumentProvider.Initialize(_uiService);
            _uiService.InitializeScreens();

            // 2. Run all project-specific initializers
            foreach (var initializable in _initializables)
            {
                initializable.Initialize();
            }

            _loggingService.LogInfo("[RootBootstrapper] Foundation initialized.");
        }
    }
}