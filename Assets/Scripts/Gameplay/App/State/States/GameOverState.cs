using Gameplay.Scoring;
using Core.UI.Interfaces;
using UnityEngine;
using Gameplay.UI.Features.GameOver;
using Gameplay.UI.Features.Gameplay;
using Gameplay.GameModes;
using Gameplay.Shared;

namespace Gameplay.App.State.States
{
    public class GameOverState : BaseState
    {
        private readonly IUIService _uiService;
        private readonly IMatchContext _matchContext;

        public GameOverState(IUIService uiService, IMatchContext matchContext)
        {
            _uiService = uiService;
            _matchContext = matchContext;
        }

        public override void Enter()
        {
            base.Enter();

            _uiService?.HideHud<GameplayHudView>(false);
            _uiService?.SetRootScreen<GameOverScreen>(true);

            MatchResultState result = _matchContext.MatchResultSync?.CurrentResult ?? MatchResultState.None;
            if (!result.HasResult)
            {
                LogError("GameOverState entered without a synchronized final result.");
            }
            else if (result.IsDraw)
            {
                LogMessage($"Final result: Draw ({result.EndReason}) at {result.WinningScore} points");
            }
            else
            {
                LogMessage($"Final result: Team {result.WinningTeamId} won via {result.EndReason} with {result.WinningScore} points");
            }

            // Get the final scores
            _matchContext.Refresh();
            ScoreManager scoreManager = _matchContext.ScoreManager;
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
