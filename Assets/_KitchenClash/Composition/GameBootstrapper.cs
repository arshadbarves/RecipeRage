using KitchenClash.Infrastructure.Logging;
using KitchenClash.Application.State;
using KitchenClash.Application.State.States;
using VContainer.Unity;

namespace KitchenClash.Composition
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
            BootstrapState bootstrapState = _stateFactory.Create<BootstrapState>();
            _gameStateManager.Initialize(bootstrapState);
        }
    }
}
