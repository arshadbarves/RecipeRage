using Modules.UI;
using Gameplay.Scoring;
using UI;
using UnityEngine;

namespace App.State.States
{
    /// <summary>
    /// State for game over.
    /// </summary>
    public class GameOverState : BaseState
    {
        private readonly IUIService _uiService;

        public GameOverState(IUIService uiService)
        {
            _uiService = uiService;
        }

        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        public override void Enter()
        {
            base.Enter();

            // Show the game over UI
            _uiService?.ShowScreen(UIScreenType.GameOver, true, false);

            // Get the final scores
            ScoreManager scoreManager = Object.FindFirstObjectByType<ScoreManager>();
            if (scoreManager != null)
            {
                int scores = scoreManager.GetScore();
                LogMessage($"Final scores: {string.Join(", ", scores)}");
            }
        }

        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        public override void Exit()
        {
            base.Exit();

            // Hide the game over UI
            _uiService?.HideScreen(UIScreenType.GameOver, true);
        }

        /// <summary>
        /// Called every frame to update the state.
        /// </summary>
        public override void Update()
        {
            // Game over update logic
        }

        /// <summary>
        /// Called at fixed intervals for physics updates.
        /// </summary>
        public override void FixedUpdate()
        {
            // Game over physics update logic
        }
    }
}