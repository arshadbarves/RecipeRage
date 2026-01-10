using Gameplay.Scoring;
using Core.UI.Interfaces;
using UnityEngine;

namespace Gameplay.App.State.States
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
            // TODO: GameOverScreen doesn't exist yet - implement when needed
            // _uiService?.Show<GameOverScreen>(true, false);

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
            // TODO: GameOverScreen doesn't exist yet
            // _uiService?.Hide<GameOverScreen>(true);
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