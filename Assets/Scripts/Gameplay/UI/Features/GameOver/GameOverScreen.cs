using UnityEngine.UIElements;
using VContainer;
using Core.UI;
using Core.UI.Interfaces;
using Core.UI.Core;
using Gameplay.Scoring;
using UnityEngine;
using Unity.Netcode;
using Core.Logging;
using UnityEngine.SceneManagement;

namespace Gameplay.UI.Features.GameOver
{
    [UIScreen(UIScreenCategory.Screen, "Screens/GameOverScreenTemplate")]
    public class GameOverScreen : BaseUIScreen
    {
        [Inject] private IUIService _uiService;
        
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
             var scoreManager = Object.FindFirstObjectByType<ScoreManager>();
             if (scoreManager != null)
             {
                 int score0 = scoreManager.GetScore(0);
                 int score1 = scoreManager.GetScore(1);
                 
                 if (_scoreTeam0 != null) _scoreTeam0.text = score0.ToString();
                 if (_scoreTeam1 != null) _scoreTeam1.text = score1.ToString();
                 
                 if (_winnerLabel != null)
                 {
                     if (score0 > score1)
                        _winnerLabel.text = "TEAM 1 WINS!";
                     else if (score1 > score0)
                        _winnerLabel.text = "TEAM 2 WINS!";
                     else
                        _winnerLabel.text = "DRAW!";
                 }
             }
        }
        
        private void OnLobbyButtonClicked()
        {
            GameLogger.Log("Returning to Lobby...");
            
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.Shutdown();
            }
            
            // Should probably use a proper SceneLoader service, but direct load for now
            SceneManager.LoadScene("Lobby");
        }
    }
}
