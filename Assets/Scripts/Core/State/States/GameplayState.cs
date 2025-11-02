using Core.Bootstrap;
using Core.GameModes;
using Gameplay.Cooking;
using Gameplay.Scoring;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Core.Logging;

namespace Core.State.States
{
    /// <summary>
    /// State for gameplay.
    /// </summary>
    public class GameplayState : BaseState
    {
        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        public override void Enter()
        {
            base.Enter();

            // Load the game scene if not already loaded
            if (SceneManager.GetActiveScene().name != "Game")
            {
                SceneManager.LoadScene("Game");
            }

            // Hide all UI screens and show only the HUD
            // Hide all UI Toolkit screens (matchmaking, main menu, etc.)
            // GameplayUIManager (MonoBehaviour) will handle the in-game UI
            var uiService = GameBootstrap.Services?.UIService;
            if (uiService != null)
            {
                uiService.HideAllScreens(true);
            }

            // Initialize the game mode
            var gameModeService = GameBootstrap.Services.GameModeService;
            if (gameModeService != null)
            {
                // Start the current game mode if available
                // Note: GameModeService interface may need StartCurrentGameMode method
                GameLogger.Log("Game mode service initialized");
            }

            // Initialize the order system
            OrderManager orderManager = Object.FindFirstObjectByType<OrderManager>();
            if (orderManager != null)
            {
                orderManager.StartGeneratingOrders();
            }

            // Initialize the scoring system
            ScoreManager scoreManager = Object.FindFirstObjectByType<ScoreManager>();
            if (scoreManager != null)
            {
                scoreManager.ResetScores();
            }
            
            // Log bot information if any
            var networkingServices = GameBootstrap.Services?.NetworkingServices;
            if (networkingServices != null)
            {
                var bots = networkingServices.MatchmakingService.GetActiveBots();
                if (bots.Count > 0)
                {
                    GameLogger.Log($"Starting game with {bots.Count} bots");
                    foreach (var bot in bots)
                    {
                        GameLogger.Log($"Bot: {bot.BotName} (Team {bot.TeamId})");
                    }
                }
            }
        }

        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        public override void Exit()
        {
            base.Exit();

            // Hide the gameplay UI
            // Note: GameplayUIManager handles its own cleanup
            // No need to hide UI Toolkit screens here

            // Stop the game mode
            var gameModeService = GameBootstrap.Services.GameModeService;
            if (gameModeService != null)
            {
                // Stop the current game mode if available
                GameLogger.Log("Game mode service stopped");
            }

            // Stop the order system
            OrderManager orderManager = Object.FindFirstObjectByType<OrderManager>();
            if (orderManager != null)
            {
                orderManager.StopGeneratingOrders();
            }
        }

        /// <summary>
        /// Called every frame to update the state.
        /// </summary>
        public override void Update()
        {
            // Gameplay update logic

            // Check if the game is over
            // Note: Game over logic should be handled by a game manager or score system
            // For now, this is a placeholder that can be implemented when needed
            
            // Example: Check time limit or score limit
            // var services = GameBootstrap.Services;
            // if (services?.GameModeService.IsGameOver())
            // {
            //     services.StateManager.ChangeState(new GameOverState());
            // }
        }

        /// <summary>
        /// Called at fixed intervals for physics updates.
        /// </summary>
        public override void FixedUpdate()
        {
            // Gameplay physics update logic
        }
    }
}