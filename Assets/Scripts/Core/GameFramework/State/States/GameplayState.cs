using UnityEngine;

namespace RecipeRage.Core.GameFramework.State.States
{
    /// <summary>
    /// State for gameplay.
    /// </summary>
    public class GameplayState : IState
    {
        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        public void Enter()
        {
            Debug.Log("[GameplayState] Entered");
            
            // Load the game scene if not already loaded
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Game")
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
            }
            
            // Show the gameplay UI
            var uiManager = FindFirstObjectByType<UI.UIManager>();
            if (uiManager != null)
            {
                uiManager.ShowGameplay();
            }
            
            // Initialize the game mode
            var gameModeManager = Core.GameModes.GameModeManager.Instance;
            if (gameModeManager != null)
            {
                gameModeManager.StartCurrentGameMode();
            }
            
            // Initialize the order system
            var orderManager = FindFirstObjectByType<Gameplay.Cooking.OrderManager>();
            if (orderManager != null)
            {
                orderManager.StartGeneratingOrders();
            }
            
            // Initialize the scoring system
            var scoreManager = FindFirstObjectByType<Gameplay.Scoring.ScoreManager>();
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
            Debug.Log("[GameplayState] Exited");
            
            // Hide the gameplay UI
            var uiManager = FindFirstObjectByType<UI.UIManager>();
            if (uiManager != null)
            {
                uiManager.HideGameplay();
            }
            
            // Stop the game mode
            var gameModeManager = Core.GameModes.GameModeManager.Instance;
            if (gameModeManager != null)
            {
                gameModeManager.StopCurrentGameMode();
            }
            
            // Stop the order system
            var orderManager = FindFirstObjectByType<Gameplay.Cooking.OrderManager>();
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
            var gameModeManager = Core.GameModes.GameModeManager.Instance;
            if (gameModeManager != null && gameModeManager.IsGameOver())
            {
                // Transition to the game over state
                var gameStateManager = GameStateManager.Instance;
                if (gameStateManager != null)
                {
                    gameStateManager.ChangeState(new GameOverState());
                }
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
