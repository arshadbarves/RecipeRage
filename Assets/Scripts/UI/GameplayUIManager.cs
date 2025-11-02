using System.Collections.Generic;
using Core.Characters;
using Core.Logging;
using Core.State;
using Gameplay;
using Gameplay.Cooking;
using Gameplay.Scoring;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Manages the gameplay UI with network synchronization.
    /// Integrates with NetworkScoreManager, RoundTimer, and NetworkGameStateManager.
    /// </summary>
    public class GameplayUIManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController _localPlayer;
        [SerializeField] private OrderManager _orderManager;

        [Header("Network Managers (Auto-found)")]
        private NetworkScoreManager _networkScoreManager;
        private RoundTimer _roundTimer;
        private NetworkGameStateManager _networkGameStateManager;

        [Header("Interaction UI")]
        [SerializeField] private GameObject _interactionPromptPanel;
        [SerializeField] private TextMeshProUGUI _interactionPromptText;

        [Header("Order UI")]
        [SerializeField] private GameObject _orderListPanel;
        [SerializeField] private GameObject _orderItemPrefab;
        [SerializeField] private Transform _orderListContent;

        [Header("Score UI")]
        [SerializeField] private TextMeshProUGUI _scoreText;

        [Header("Timer UI")]
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private Image _timerFill;

        /// <summary>
        /// The current score.
        /// </summary>
        private int _score;

        /// <summary>
        /// The current combo count.
        /// </summary>
        private int _comboCount;

        /// <summary>
        /// The current game time.
        /// </summary>
        private float _gameTime;

        /// <summary>
        /// The total game time.
        /// </summary>
        private float _totalGameTime = 300f; // 5 minutes

        /// <summary>
        /// Dictionary of order UI items.
        /// </summary>
        private Dictionary<int, GameObject> _orderUIItems = new Dictionary<int, GameObject>();

        /// <summary>
        /// Initialize the gameplay UI manager.
        /// </summary>
        private void Start()
        {
            // Initialize UI
            if (_interactionPromptPanel != null)
            {
                _interactionPromptPanel.SetActive(false);
            }
            _score = 0;
            _gameTime = _totalGameTime;

            UpdateScoreUI();
            UpdateTimerUI();

            // Find network managers
            _networkScoreManager = FindFirstObjectByType<NetworkScoreManager>();
            _roundTimer = FindFirstObjectByType<RoundTimer>();
            _networkGameStateManager = FindFirstObjectByType<NetworkGameStateManager>();

            // Subscribe to order manager events
            if (_orderManager != null)
            {
                _orderManager.OnOrderCreated += HandleOrderCreated;
                _orderManager.OnOrderCompleted += HandleOrderCompleted;
                _orderManager.OnOrderExpired += HandleOrderExpired;
            }

            // Subscribe to network score manager events
            if (_networkScoreManager != null)
            {
                _networkScoreManager.OnPlayerScoreUpdated += HandleNetworkScoreUpdated;
                _networkScoreManager.OnScoreboardUpdated += HandleScoreboardUpdated;
                GameLogger.Log("Subscribed to NetworkScoreManager events");
            }
            else
            {
                GameLogger.LogWarning("NetworkScoreManager not found. Network score updates will not work.");
            }

            // Subscribe to round timer events
            if (_roundTimer != null)
            {
                _roundTimer.OnTimeUpdated += HandleTimeUpdated;
                _roundTimer.OnTimerExpired += HandleTimerExpired;
                GameLogger.Log("Subscribed to RoundTimer events");
            }
            else
            {
                GameLogger.LogWarning("RoundTimer not found. Timer updates will not work.");
            }

            // Subscribe to game state manager events
            if (_networkGameStateManager != null)
            {
                _networkGameStateManager.OnPhaseChanged += HandlePhaseChanged;
                GameLogger.Log("Subscribed to NetworkGameStateManager events");
            }
            else
            {
                GameLogger.LogWarning("NetworkGameStateManager not found. Phase updates will not work.");
            }
        }

        /// <summary>
        /// Clean up when the gameplay UI manager is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            // Unsubscribe from order manager events
            if (_orderManager != null)
            {
                _orderManager.OnOrderCreated -= HandleOrderCreated;
                _orderManager.OnOrderCompleted -= HandleOrderCompleted;
                _orderManager.OnOrderExpired -= HandleOrderExpired;
            }

            // Unsubscribe from network score manager events
            if (_networkScoreManager != null)
            {
                _networkScoreManager.OnPlayerScoreUpdated -= HandleNetworkScoreUpdated;
                _networkScoreManager.OnScoreboardUpdated -= HandleScoreboardUpdated;
            }

            // Unsubscribe from round timer events
            if (_roundTimer != null)
            {
                _roundTimer.OnTimeUpdated -= HandleTimeUpdated;
                _roundTimer.OnTimerExpired -= HandleTimerExpired;
            }

            // Unsubscribe from game state manager events
            if (_networkGameStateManager != null)
            {
                _networkGameStateManager.OnPhaseChanged -= HandlePhaseChanged;
            }
        }

        /// <summary>
        /// Update the gameplay UI manager.
        /// </summary>
        private void Update()
        {
            // Update the interaction prompt
            UpdateInteractionPrompt();

            // Update the game timer
            UpdateGameTimer();
        }

        /// <summary>
        /// Update the interaction prompt.
        /// </summary>
        private void UpdateInteractionPrompt()
        {
            if (_localPlayer == null)
            {
                // Try to find the local player
                if (NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClient != null)
                {
                    NetworkObject localPlayerObject = NetworkManager.Singleton.LocalClient.PlayerObject;
                    if (localPlayerObject != null)
                    {
                        _localPlayer = localPlayerObject.GetComponent<PlayerController>();
                    }
                }

                if (_localPlayer == null)
                {
                    _interactionPromptPanel.SetActive(false);
                    return;
                }
            }

            // Cast a ray in front of the player
            Ray ray = new Ray(_localPlayer.transform.position, _localPlayer.transform.forward);
            RaycastHit hit;

            // Check if ray hits an interactable object
            if (Physics.Raycast(ray, out hit, 2f))
            {
                // Check if the hit object has an interactable component
                IInteractable interactable = hit.collider.gameObject.GetComponent<IInteractable>();
                if (interactable != null && interactable.CanInteract(_localPlayer))
                {
                    // Show the interaction prompt
                    _interactionPromptPanel.SetActive(true);
                    _interactionPromptText.text = interactable.GetInteractionPrompt();
                    return;
                }
            }

            // Hide the interaction prompt
            _interactionPromptPanel.SetActive(false);
        }

        /// <summary>
        /// Update the game timer (legacy - now handled by RoundTimer).
        /// </summary>
        private void UpdateGameTimer()
        {
            // If we have a network round timer, use that instead
            if (_roundTimer != null && _roundTimer.IsRunning)
            {
                _gameTime = _roundTimer.TimeRemaining;
                UpdateTimerUI();
                return;
            }

            // Fallback to local timer
            _gameTime -= Time.deltaTime;
            _gameTime = Mathf.Max(0f, _gameTime);
            UpdateTimerUI();

            if (_gameTime <= 0f)
            {
                // Game over
                GameLogger.Log("Game time expired");
            }
        }

        /// <summary>
        /// Update the score UI.
        /// </summary>
        private void UpdateScoreUI()
        {
            if (_scoreText != null)
            {
                _scoreText.text = $"Score: {_score}";
            }
        }

        /// <summary>
        /// Update the timer UI.
        /// </summary>
        private void UpdateTimerUI()
        {
            if (_timerText != null)
            {
                int minutes = Mathf.FloorToInt(_gameTime / 60f);
                int seconds = Mathf.FloorToInt(_gameTime % 60f);
                _timerText.text = $"{minutes:00}:{seconds:00}";
            }

            if (_timerFill != null)
            {
                _timerFill.fillAmount = _gameTime / _totalGameTime;
            }
        }

        /// <summary>
        /// Handle order created event.
        /// </summary>
        /// <param name="order">The order that was created.</param>
        private void HandleOrderCreated(RecipeOrderState order)
        {
            // Create a new order UI item
            if (_orderItemPrefab != null && _orderListContent != null)
            {
                GameObject orderItem = Instantiate(_orderItemPrefab, _orderListContent);

                // Get the recipe
                Recipe recipe = _orderManager.GetRecipeById(order.RecipeId);
                if (recipe != null)
                {
                    // Set the order UI item data
                    OrderUIItem orderUIItem = orderItem.GetComponent<OrderUIItem>();
                    if (orderUIItem != null)
                    {
                        orderUIItem.SetOrder(order, recipe);
                    }
                }

                // Add the order UI item to the dictionary
                _orderUIItems[order.OrderId] = orderItem;
            }
        }

        /// <summary>
        /// Handle order completed event.
        /// </summary>
        /// <param name="order">The order that was completed.</param>
        private void HandleOrderCompleted(RecipeOrderState order)
        {
            // Remove the order UI item
            if (_orderUIItems.TryGetValue(order.OrderId, out GameObject orderItem))
            {
                Destroy(orderItem);
                _orderUIItems.Remove(order.OrderId);
            }
        }

        /// <summary>
        /// Handle order expired event.
        /// </summary>
        /// <param name="order">The order that expired.</param>
        private void HandleOrderExpired(RecipeOrderState order)
        {
            // Remove the order UI item
            if (_orderUIItems.TryGetValue(order.OrderId, out GameObject orderItem))
            {
                Destroy(orderItem);
                _orderUIItems.Remove(order.OrderId);
            }
        }

        #region Network Event Handlers

        /// <summary>
        /// Handle network score updated event.
        /// </summary>
        /// <param name="playerId">The player ID whose score was updated</param>
        /// <param name="score">The new score</param>
        private void HandleNetworkScoreUpdated(ulong playerId, int score)
        {
            // Only update UI for local player
            if (NetworkManager.Singleton != null && playerId == NetworkManager.Singleton.LocalClientId)
            {
                _score = score;
                UpdateScoreUI();
                GameLogger.Log($"Local player score updated: {score}");
            }
        }

        /// <summary>
        /// Handle scoreboard updated event.
        /// </summary>
        private void HandleScoreboardUpdated()
        {
            // Refresh scoreboard display
            // In a full implementation, you'd update a leaderboard UI here
            GameLogger.Log("Scoreboard updated");
        }

        /// <summary>
        /// Handle time updated event from RoundTimer.
        /// </summary>
        /// <param name="timeRemaining">The time remaining in seconds</param>
        private void HandleTimeUpdated(float timeRemaining)
        {
            _gameTime = timeRemaining;
            UpdateTimerUI();
        }

        /// <summary>
        /// Handle timer expired event.
        /// </summary>
        private void HandleTimerExpired()
        {
            GameLogger.Log("Round timer expired - Game Over!");
            // Show game over UI
            // TODO: Implement game over screen
        }

        /// <summary>
        /// Handle game phase changed event.
        /// </summary>
        /// <param name="newPhase">The new game phase</param>
        private void HandlePhaseChanged(GamePhase newPhase)
        {
            GameLogger.Log($"Game phase changed to: {newPhase}");

            switch (newPhase)
            {
                case GamePhase.Waiting:
                    // Show waiting for players UI
                    break;

                case GamePhase.Preparation:
                    // Show countdown UI
                    break;

                case GamePhase.Playing:
                    // Show gameplay UI
                    break;

                case GamePhase.Results:
                    // Show results/scoreboard UI
                    break;
            }
        }

        #endregion
    }
}
