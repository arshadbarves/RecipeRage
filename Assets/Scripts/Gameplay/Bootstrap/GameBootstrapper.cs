using Core.Logging;
using Gameplay.App.State;
using Gameplay.App.State.States; // Assuming BootstrapState is here
using VContainer;
using VContainer.Unity;

namespace Gameplay.Bootstrap
{
    public class GameBootstrapper : IStartable
    {
        private readonly IGameStateManager _gameStateManager;
        private readonly IStateFactory _stateFactory;

        [Inject]
        public GameBootstrapper(IGameStateManager gameStateManager, IStateFactory stateFactory)
        {
            _gameStateManager = gameStateManager;
            _stateFactory = stateFactory;
        }

        public void Start()
        {
            GameLogger.Log("GameBootstrapper starting...");

            // Create and enter the first state
            BootstrapState bootstrapState = _stateFactory.CreateState<BootstrapState>();
            _gameStateManager.Initialize(bootstrapState);
        }
    }
}
