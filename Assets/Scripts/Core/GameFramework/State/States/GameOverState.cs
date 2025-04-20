using Gameplay.Scoring;
using UI;
using UnityEngine;

namespace Core.GameFramework.State.States
{
    /// <summary>
    /// State for game over.
    /// </summary>
    public class GameOverState : IState
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

            // Show the game over UI
            UIManager uiManager = Object.FindFirstObjectByType<UIManager>();
            if (uiManager != null)
            {
                uiManager.ShowGameOver();
            }

            // Get the final scores
            ScoreManager scoreManager = Object.FindFirstObjectByType<ScoreManager>();
            if (scoreManager != null)
            {
                int scores = scoreManager.GetScore();
                Debug.Log($"[{StateName}] Final scores: {string.Join(", ", scores)}");
            }
        }

        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        public void Exit()
        {
            Debug.Log($"[{StateName}] Exited");

            // Hide the game over UI
            UIManager uiManager = Object.FindFirstObjectByType<UIManager>();
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