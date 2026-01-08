using Gameplay.Characters;
using Gameplay.Interaction;
using Modules.Networking.Interfaces;
using Modules.Networking.Services;
using Gameplay.Cooking;
using Gameplay.Scoring;
using Gameplay.Stations;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Gameplay
{
    /// <summary>
    /// Lifetime scope for the gameplay scene.
    /// Registers gameplay-specific dependencies and services.
    /// </summary>
    public class GameplayLifetimeScope : LifetimeScope
    {
        [Header("Scene References")]
        [SerializeField] private OrderManager _orderManager;
        [SerializeField] private ScoreManager _scoreManager;
        [SerializeField] private RoundTimer _roundTimer;
        
        protected override void Configure(IContainerBuilder builder)
        {
            // Register Managers if they exist in the scene
            if (_orderManager != null)
            {
                builder.RegisterInstance(_orderManager);
            }
            
            if (_scoreManager != null)
            {
                builder.RegisterInstance(_scoreManager);
            }
            
            if (_roundTimer != null)
            {
                builder.RegisterInstance(_roundTimer);
            }

            // Register Interaction Service (if not already handled by PlayerController internally)
            // In this architecture, PlayerController creates its own InteractionController, 
            // but we might want a global registry for debug or AI.
            
            // Register Factories (if needed for dynamic spawning not handled by Netcode)
        }
    }
}