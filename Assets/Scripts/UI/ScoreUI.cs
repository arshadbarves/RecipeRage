using Gameplay.Scoring;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
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
                _scoreManager.OnScoreChanged += HandleScoreChanged;
                _scoreManager.OnComboAchieved += HandleComboAchieved;
                
                // Initialize the score
                _currentScore = _scoreManager.GetScore();
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
                _scoreManager.OnScoreChanged -= HandleScoreChanged;
                _scoreManager.OnComboAchieved -= HandleComboAchieved;
            }
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
        /// <param name="newScore">The new score</param>
        private void HandleScoreChanged(int newScore)
        {
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
        /// <param name="comboCount">The combo count</param>
        private void HandleComboAchieved(int comboCount)
        {
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
