using Gameplay.Scoring;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI
{
    /// <summary>
    /// UI component for displaying the score and combo.
    /// </summary>
    public class ScoreUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ScoreManager _scoreManager;
        
        [Header("Score UI")]
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _scoreChangeText;
        [SerializeField] private float _scoreChangeDisplayTime = 1.5f;
        
        [Header("Combo UI")]
        [SerializeField] private GameObject _comboPanel;
        [SerializeField] private TextMeshProUGUI _comboText;
        [SerializeField] private Image _comboTimerFill;
        [SerializeField] private float _comboDisplayTime = 3f;
        
        /// <summary>
        /// The current score.
        /// </summary>
        private int _currentScore;
        
        /// <summary>
        /// Timer for displaying score change.
        /// </summary>
        private float _scoreChangeTimer;
        
        /// <summary>
        /// Timer for displaying combo.
        /// </summary>
        private float _comboTimer;
        
        /// <summary>
        /// Initialize the score UI.
        /// </summary>
        private void Start()
        {
            // Find the score manager if not set
            if (_scoreManager == null)
            {
                _scoreManager = FindFirstObjectByType<ScoreManager>();
            }
            
            // Subscribe to score manager events
            if (_scoreManager != null)
            {
                _scoreManager.OnTeamScoreChanged += HandleScoreChanged;
                _scoreManager.OnTeamComboAchieved += HandleComboAchieved;
                
                // Initialize the score
                _currentScore = _scoreManager.GetScore(GetLocalPlayerTeamId());
                UpdateScoreUI();
            }
            
            // Hide the combo panel
            if (_comboPanel != null)
            {
                _comboPanel.SetActive(false);
            }
            
            // Hide the score change text
            if (_scoreChangeText != null)
            {
                _scoreChangeText.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Clean up when the score UI is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            // Unsubscribe from score manager events
            if (_scoreManager != null)
            {
                _scoreManager.OnTeamScoreChanged -= HandleScoreChanged;
                _scoreManager.OnTeamComboAchieved -= HandleComboAchieved;
            }
        }

        private int GetLocalPlayerTeamId()
        {
            if (Unity.Netcode.NetworkManager.Singleton != null && 
                Unity.Netcode.NetworkManager.Singleton.LocalClient != null &&
                Unity.Netcode.NetworkManager.Singleton.LocalClient.PlayerObject != null)
            {
                 var player = Unity.Netcode.NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Gameplay.Characters.PlayerController>();
                 if (player != null) return player.TeamId;
            }
            return 0; // Default to Team 0 if not found
        }
        
        /// <summary>
        /// Update the score UI.
        /// </summary>
        private void Update()
        {
            // Update score change timer
            if (_scoreChangeTimer > 0)
            {
                _scoreChangeTimer -= Time.deltaTime;
                
                if (_scoreChangeTimer <= 0)
                {
                    // Hide the score change text
                    if (_scoreChangeText != null)
                    {
                        _scoreChangeText.gameObject.SetActive(false);
                    }
                }
            }
            
            // Update combo timer
            if (_comboTimer > 0)
            {
                _comboTimer -= Time.deltaTime;
                
                // Update combo timer fill
                if (_comboTimerFill != null)
                {
                    _comboTimerFill.fillAmount = _comboTimer / _comboDisplayTime;
                }
                
                if (_comboTimer <= 0)
                {
                    // Hide the combo panel
                    if (_comboPanel != null)
                    {
                        _comboPanel.SetActive(false);
                    }
                }
            }
        }
        
        /// <summary>
        /// Handle score changed event.
        /// </summary>
        /// <param name="teamId">The team ID</param>
        /// <param name="newScore">The new score</param>
        private void HandleScoreChanged(int teamId, int newScore)
        {
            // Only update if it's our team
            int localTeamId = GetLocalPlayerTeamId();
            if (teamId != localTeamId) return;

            // Calculate score change
            int scoreChange = newScore - _currentScore;
            
            // Update current score
            _currentScore = newScore;
            
            // Update the score UI
            UpdateScoreUI();
            
            // Show score change
            if (scoreChange != 0 && _scoreChangeText != null)
            {
                _scoreChangeText.text = scoreChange > 0 ? $"+{scoreChange}" : $"{scoreChange}";
                _scoreChangeText.color = scoreChange > 0 ? Color.green : Color.red;
                _scoreChangeText.gameObject.SetActive(true);
                
                // Reset the timer
                _scoreChangeTimer = _scoreChangeDisplayTime;
            }
        }
        
        /// <summary>
        /// Handle combo achieved event.
        /// </summary>
        /// <param name="teamId">The team ID</param>
        /// <param name="comboCount">The combo count</param>
        private void HandleComboAchieved(int teamId, int comboCount)
        {
            // Only update if it's our team
            int localTeamId = GetLocalPlayerTeamId();
            if (teamId != localTeamId) return;

            // Show combo
            if (_comboPanel != null)
            {
                _comboPanel.SetActive(true);
            }
            
            // Update combo text
            if (_comboText != null)
            {
                _comboText.text = $"Combo x{comboCount}";
            }
            
            // Reset the timer
            _comboTimer = _comboDisplayTime;
            
            // Reset the combo timer fill
            if (_comboTimerFill != null)
            {
                _comboTimerFill.fillAmount = 1f;
            }
        }
        
        /// <summary>
        /// Update the score UI.
        /// </summary>
        private void UpdateScoreUI()
        {
            if (_scoreText != null)
            {
                _scoreText.text = $"Score: {_currentScore}";
            }
        }
    }
}
