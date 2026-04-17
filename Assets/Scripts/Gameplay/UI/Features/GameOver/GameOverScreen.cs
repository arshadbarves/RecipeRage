using UnityEngine.UIElements;
using VContainer;
using Core.UI;
using Core.UI.Interfaces;
using Core.UI.Core;
using Core.Logging;
using Core.Session;
using Gameplay.GameModes;
using Gameplay.Shared;

namespace Gameplay.UI.Features.GameOver
{
    [UIScreen(UIScreenCategory.Screen, "Screens/GameOverScreenTemplate")]
    public class GameOverScreen : BaseUIScreen
    {
        [Inject] private IUIService _uiService;
        [Inject] private ISessionContext _sessionContext;
        [Inject] private IMatchContext _matchContext;
        
        private Label _winnerLabel;
        private Label _scoreTeam0;
        private Label _scoreTeam1;
        private Button _lobbyButton;
        
        protected override void OnInitialize()
        {
            _winnerLabel = GetElement<Label>("winner-label");
            _scoreTeam0 = GetElement<Label>("score-team-0");
            _scoreTeam1 = GetElement<Label>("score-team-1");
            _lobbyButton = GetElement<Button>("lobby-btn");
            
            if (_lobbyButton != null)
            {
                _lobbyButton.clicked += OnLobbyButtonClicked;
            }
        }

        protected override void OnShow()
        {
            base.OnShow();
            UpdateScores();
        }

        private void UpdateScores()
        {
            _matchContext.Refresh();
            var scoreManager = _matchContext.ScoreManager;
            if (scoreManager != null)
            {
                int score0 = scoreManager.GetScore(0);
                int score1 = scoreManager.GetScore(1);

                if (_scoreTeam0 != null) _scoreTeam0.text = score0.ToString();
                if (_scoreTeam1 != null) _scoreTeam1.text = score1.ToString();
            }

            if (_winnerLabel == null)
            {
                return;
            }

            MatchResultState result = _matchContext.MatchResultSync?.CurrentResult ?? MatchResultState.None;
            if (!result.HasResult)
            {
                GameLogger.LogError("[GameOverScreen] Missing synchronized match result. Showing neutral fallback text.");
                _winnerLabel.text = "MATCH COMPLETE";
                return;
            }

            _winnerLabel.text = GetWinnerText(result);
        }

        public static string GetWinnerText(MatchResultState result)
        {
            if (!result.HasResult)
            {
                return "MATCH COMPLETE";
            }

            if (result.IsDraw)
            {
                return "DRAW!";
            }

            return result.WinningTeamId == 0 ? "TEAM 1 WINS!" : "TEAM 2 WINS!";
        }
        
        private void OnLobbyButtonClicked()
        {
            GameLogger.Log("Returning to Lobby...");
            _sessionContext?.GameStarter?.EndGame();
        }
    }
}
