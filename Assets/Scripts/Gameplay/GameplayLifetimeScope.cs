using Gameplay.Cooking;
using Gameplay.Scoring;
using Gameplay.GameModes;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Gameplay
{
    /// <summary>
    /// Lifetime scope for the gameplay scene.
    /// Registers gameplay-specific dependencies and systems.
    /// </summary>
    public class GameplayLifetimeScope : LifetimeScope
    {
        [Header("Scene References")]
        [SerializeField] private OrderManager _orderManager;
        [SerializeField] private ScoreManager _scoreManager;
        [SerializeField] private GamePhaseSync _phaseSync;
        
        protected override void Configure(IContainerBuilder builder)
        {
            // Register Scene MonoBehaviour instances
            if (_orderManager != null)
            {
                builder.RegisterInstance(_orderManager);
            }
            
            if (_scoreManager != null)
            {
                builder.RegisterInstance(_scoreManager);
            }
            
            if (_phaseSync != null)
            {
                builder.RegisterInstance(_phaseSync);
            }

            // Register GameModeController as ITickable
            builder.Register<GameModeController>(Lifetime.Singleton).As<ITickable>();

            // Register factory for IGameModeLogic
            builder.Register<IGameModeLogic>(container =>
            {
                var gameModeService = container.Resolve<IGameModeService>();
                var logic = gameModeService.CreateGameModeLogic();
                
                // Initialize the logic with managers
                var orderManager = container.Resolve<OrderManager>();
                var scoreManager = container.Resolve<ScoreManager>();
                logic?.Initialize(gameModeService.SelectedGameMode, orderManager, scoreManager);
                
                return logic;
            }, Lifetime.Singleton);
        }
    }
}