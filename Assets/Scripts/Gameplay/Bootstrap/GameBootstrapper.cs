using Core.Logging;
using Gameplay.App.State;
using Gameplay.App.State.States;
using VContainer;
using VContainer.Unity;

namespace Gameplay.Bootstrap
{
    public class GameBootstrapper : IStartable
    {
        private readonly IGameStateManager _gameStateManager;
        private readonly IObjectResolver _container;

        [Inject]
        public GameBootstrapper(IGameStateManager gameStateManager, IObjectResolver container)
        {
            _gameStateManager = gameStateManager;
            _container = container;
        }

        public void Start()
        {
            GameLogger.Log("GameBootstrapper starting...");

            // Create and enter the first state
            BootstrapState bootstrapState = _container.Resolve<BootstrapState>();
            _gameStateManager.Initialize(bootstrapState);
        }
    }
}
