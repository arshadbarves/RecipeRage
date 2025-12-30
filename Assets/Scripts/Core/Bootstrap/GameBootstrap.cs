using System;
using Core.Animation;
using Core.Events;
using Core.Logging;
using Core.Maintenance;
using Core.Networking;
using Core.RemoteConfig;
using Core.SaveSystem;
using Core.State;
using Core.State.States;
using Cysharp.Threading.Tasks;
using RecipeRage.Modules.Auth.Core;
using UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Core.Bootstrap
{
    /// <summary>
    /// Single entry point for the entire game - bootstraps all services using VContainer
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        public static GameBootstrap Instance { get; private set; }

        public static IObjectResolver Container { get; private set; }

        /// <summary>
        /// Temporary shim to maintain compatibility with code still using GameBootstrap.Services.
        /// Will be removed in Phase 3.
        /// </summary>
        public class ServicesShim
        {
            public IEventBus EventBus => Container?.Resolve<IEventBus>();
            public IUIService UIService => Container?.Resolve<IUIService>();
            public IAnimationService AnimationService => Container?.Resolve<IAnimationService>();
            public ILoggingService LoggingService => Container?.Resolve<ILoggingService>();
            public ISaveService SaveService => Container?.Resolve<ISaveService>();
            public IAuthService AuthService => Container?.Resolve<IAuthService>();
            public IMaintenanceService MaintenanceService => Container?.Resolve<IMaintenanceService>();
            public IRemoteConfigService RemoteConfigService => Container?.Resolve<IRemoteConfigService>();
            public INTPTimeService NTPTimeService => Container?.Resolve<INTPTimeService>();
            public IConnectivityService ConnectivityService => Container?.Resolve<IConnectivityService>();
            public IGameStateManager StateManager => Container?.Resolve<IGameStateManager>();
            
            // Session handled via SessionManager in VContainer
            public GameSession Session => Container?.Resolve<SessionManager>()?.CurrentSession;
        }

        public static ServicesShim Services { get; } = new ServicesShim();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            InitializeGame();
        }

        private void InitializeGame()
        {
            // VContainer handles service construction via GameLifetimeScope.
            // We just need to resolve the entry state.
            Container = LifetimeScope.Find<GameLifetimeScope>(gameObject.scene).Container;

            var eventBus = Container.Resolve<IEventBus>();
            var stateManager = Container.Resolve<IGameStateManager>();

            // Subscribe to log out handler for full reboot
            eventBus.Subscribe<LogoutEvent>(HandleLogoutAsync);

            var bootstrapState = new BootstrapState(
                Container.Resolve<IUIService>(),
                Container.Resolve<INTPTimeService>(),
                Container.Resolve<IRemoteConfigService>(),
                Container.Resolve<IAuthService>(),
                Container.Resolve<ISaveService>(),
                Container.Resolve<IMaintenanceService>(),
                stateManager,
                eventBus
            );

            stateManager.Initialize(bootstrapState);
        }

        private async void HandleLogoutAsync(LogoutEvent evt)
        {
            try
            {
                await UniTask.Yield();

                var eventBus = Container.Resolve<IEventBus>();
                var stateManager = Container.Resolve<IGameStateManager>();

                stateManager.Initialize(new LoginState(
                    Container.Resolve<IUIService>(),
                    eventBus,
                    stateManager
                ));
            }
            catch (Exception e)
            {
                GameLogger.LogException(e);
            }
        }
    }
}
