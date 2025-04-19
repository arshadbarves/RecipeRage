using UnityEngine;

namespace RecipeRage.Core.GameFramework.State.States
{
    /// <summary>
    /// State for game over.
    /// </summary>
    public class GameOverState : IState
    {
        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        public void Enter()
        {
            Debug.Log("[GameOverState] Entered");
            
            // Show the game over UI
            var uiManager = FindFirstObjectByType<UI.UIManager>();
            if (uiManager != null)
            {
                uiManager.ShowGameOver();
            }
            
            // Get the final scores
            var scoreManager = FindFirstObjectByType<Gameplay.Scoring.ScoreManager>();
            if (scoreManager != null)
            {
                var scores = scoreManager.GetScores();
                Debug.Log($"[GameOverState] Final scores: {string.Join(", ", scores)}");
            }
        }
        
        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        public void Exit()
        {
            Debug.Log("[GameOverState] Exited");
            
            // Hide the game over UI
            var uiManager = FindFirstObjectByType<UI.UIManager>();
            if (uiManager != null)
            {
                uiManager.HideGameOver();
            }
        }
        
        /// <summary>
        /// Called every frame to update the state.
        /// </summary>
        public void Update()
        {
            // Game over update logic
        }
        
        /// <summary>
        /// Called at fixed intervals for physics updates.
        /// </summary>
        public void FixedUpdate()
        {
            // Game over physics update logic
        }
    }
}
