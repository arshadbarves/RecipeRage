using Core.GameModes;
using Gameplay.Cooking;
using Gameplay.Scoring;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.GameFramework.State.States
{
    /// <summary>
    /// State for gameplay.
    /// </summary>
    public class GameplayState : IState
    {
        /// <summary>
        /// Name of the state for debugging.
        /// </summary>
        public string StateName => GetType().Name;
        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        public void Enter()
        {
            Debug.Log($"[{StateName}] Entered");

            // Load the game scene if not already loaded
            if (SceneManager.GetActiveScene().name != "Game")
            {
                SceneManager.LoadScene("Game");
            }

            // Show the gameplay UI
            UIManager uiManager = Object.FindFirstObjectByType<UIManager>();
            if (uiManager != null)
            {
                uiManager.ShowGameplay();
            }

            // Initialize the game mode
            GameModeManager gameModeManager = GameModeManager.Instance;
            if (gameModeManager != null)
            {
                gameModeManager.StartCurrentGameMode();
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
        }

        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        public void Exit()
        {
            Debug.Log($"[{StateName}] Exited");

            // Hide the gameplay UI
            UIManager uiManager = Object.FindFirstObjectByType<UIManager>();
            if (uiManager != null)
            {
                uiManager.HideGameplay();
            }

            // Stop the game mode
            GameModeManager gameModeManager = GameModeManager.Instance;
            if (gameModeManager != null)
            {
                gameModeManager.StopCurrentGameMode();
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
        public void Update()
        {
            // Gameplay update logic

            // Check if the game is over
            GameModeManager gameModeManager = GameModeManager.Instance;
            if (gameModeManager == null || !gameModeManager.IsGameOver()) return;
            // Transition to the game over state
            GameStateManager gameStateManager = GameStateManager.Instance;
            if (gameStateManager != null)
            {
                gameStateManager.ChangeState(new GameOverState());
            }
        }

        /// <summary>
        /// Called at fixed intervals for physics updates.
        /// </summary>
        public void FixedUpdate()
        {
            // Gameplay physics update logic
        }
    }
}