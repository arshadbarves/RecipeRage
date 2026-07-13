using KitchenClash.Domain;
using KitchenClash.Application.State;
using KitchenClash.Infrastructure.States;
using Playcenter.GameFlow;
using VContainer.Unity;

namespace KitchenClash.Composition
{
    public class GameBootstrapper : IStartable
    {
        private readonly IAppFlow _appFlow;
        private readonly IGameStateManager _gameStateManager;

        public GameBootstrapper(IAppFlow appFlow, IGameStateManager gameStateManager)
        {
            _appFlow = appFlow;
            _gameStateManager = gameStateManager;
        }

        public void Start()
        {
            GameLogger.Log("GameBootstrapper starting AppFlow cold boot...");
            // State machine starts empty until Boot port enters BootstrapState.
            _appFlow.StartColdBoot();
        }
    }
}
