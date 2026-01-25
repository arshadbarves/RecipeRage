using Gameplay.Scoring;
using Core.UI.Interfaces;
using UnityEngine;
using Gameplay.UI.Features.GameOver;

namespace Gameplay.App.State.States
{
    public class GameOverState : BaseState
    {
        private readonly IUIService _uiService;

        public GameOverState(IUIService uiService)
        {
            _uiService = uiService;
        }

        public override void Enter()
        {
            base.Enter();

            // Show the game over UI
            _uiService?.Show<GameOverScreen>(true, false);

            // Get the final scores
            ScoreManager scoreManager = Object.FindFirstObjectByType<ScoreManager>();
            if (scoreManager != null)
            {
                int scoreTeam0 = scoreManager.GetScore(0);
                int scoreTeam1 = scoreManager.GetScore(1);
                LogMessage($"Final scores: Team 0: {scoreTeam0}, Team 1: {scoreTeam1}");
            }
        }

        public override void Exit()
        {
            base.Exit();

            // Hide the game over UI
            _uiService?.Hide<GameOverScreen>(true);
        }

        public override void Update()
        {
            // Game over update logic
        }

        public override void FixedUpdate()
        {
            // Game over physics update logic
        }
    }
}