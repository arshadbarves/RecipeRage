using KitchenClash.Application;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using KitchenClash.Presentation.Common;
using Playcenter.GameFlow;
using UnityEngine.UIElements;
using VContainer;
using Playcenter.Shell;
using Playcenter.UI;

namespace KitchenClash.Presentation.Screens
{
    [UIScreen(UIScreenCategory.Screen, "Screens/GameOverScreenTemplate")]
    public class ResultsScreen : BaseUIScreen
    {
        [Inject] private IUIService _uiService;
        [Inject] private ISessionContext _sessionContext;
        [Inject] private IMatchHudPort _matchHud;
        [Inject] private IAppFlow _appFlow;

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
            _matchHud?.Refresh();

            if (_matchHud != null)
            {
                if (_scoreTeam0 != null)
                {
                    _scoreTeam0.text = _matchHud.GetTeamScore(0).ToString();
                }

                if (_scoreTeam1 != null)
                {
                    _scoreTeam1.text = _matchHud.GetTeamScore(1).ToString();
                }
            }

            if (_winnerLabel == null)
            {
                return;
            }

            MatchResultSnapshot result = _matchHud != null
                ? _matchHud.CurrentMatchResult
                : MatchResultSnapshot.None;

            if (!result.HasResult)
            {
                GameLogger.LogError("[ResultsScreen] Missing synchronized match result. Showing neutral fallback text.");
                _winnerLabel.text = "MATCH COMPLETE";
                return;
            }

            _winnerLabel.text = GetWinnerText(result);
        }

        public static string GetWinnerText(MatchResultSnapshot result)
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
