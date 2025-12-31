using System;
using Core.Events;
using Core.Logging;
using Core.State;
using Core.State.States;
using Cysharp.Threading.Tasks;
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
            var stateFactory = Container.Resolve<IStateFactory>();

            // Subscribe to log out handler for full reboot
            eventBus.Subscribe<LogoutEvent>(HandleLogoutAsync);

            var bootstrapState = stateFactory.CreateState<BootstrapState>();

            stateManager.Initialize(bootstrapState);
        }

        private async void HandleLogoutAsync(LogoutEvent evt)
        {
            try
            {
                await UniTask.Yield();

                var stateManager = Container.Resolve<IGameStateManager>();
                var stateFactory = Container.Resolve<IStateFactory>();

                stateManager.Initialize(stateFactory.CreateState<LoginState>());
            }
            catch (Exception e)
            {
                GameLogger.LogException(e);
            }
        }
    }
}
