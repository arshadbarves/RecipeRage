using Core.Logging;
using Gameplay.App.State;
using Gameplay.App.State.States;
using VContainer.Unity;

namespace Gameplay.Bootstrap
{
    public class GameBootstrapper : IStartable
    {
        private readonly IGameStateManager _gameStateManager;
        private readonly IStateFactory _stateFactory;

        public GameBootstrapper(IGameStateManager gameStateManager, IStateFactory stateFactory)
        {
            _gameStateManager = gameStateManager;
            _stateFactory = stateFactory;
        }

        public void Start()
        {
            GameLogger.Log("GameBootstrapper starting...");

            // Create and enter the first state
            BootstrapState bootstrapState = _stateFactory.Create<BootstrapState>();
            _gameStateManager.Initialize(bootstrapState);
        }
    }
}
