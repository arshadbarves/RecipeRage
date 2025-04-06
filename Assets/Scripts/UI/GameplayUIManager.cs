using System.Collections.Generic;
using RecipeRage.Core.Characters;
using RecipeRage.Gameplay.Cooking;
using RecipeRage.Gameplay.Scoring;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace RecipeRage.UI
{
    /// <summary>
    /// Manages the gameplay UI.
    /// </summary>
    public class GameplayUIManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController _localPlayer;
        [SerializeField] private OrderManager _orderManager;
        [SerializeField] private ScoreManager _scoreManager;

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
            _interactionPromptPanel.SetActive(false);
            _score = 0;
            _gameTime = _totalGameTime;

            UpdateScoreUI();
            UpdateTimerUI();

            // Subscribe to events
            if (_orderManager != null)
            {
                _orderManager.OnOrderCreated += HandleOrderCreated;
                _orderManager.OnOrderCompleted += HandleOrderCompleted;
                _orderManager.OnOrderExpired += HandleOrderExpired;
            }

            // Subscribe to score manager events
            if (_scoreManager != null)
            {
                _scoreManager.OnScoreChanged += HandleScoreChanged;
                _scoreManager.OnComboAchieved += HandleComboAchieved;
            }
        }

        /// <summary>
        /// Clean up when the gameplay UI manager is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (_orderManager != null)
            {
                _orderManager.OnOrderCreated -= HandleOrderCreated;
                _orderManager.OnOrderCompleted -= HandleOrderCompleted;
                _orderManager.OnOrderExpired -= HandleOrderExpired;
            }

            // Unsubscribe from score manager events
            if (_scoreManager != null)
            {
                _scoreManager.OnScoreChanged -= HandleScoreChanged;
                _scoreManager.OnComboAchieved -= HandleComboAchieved;
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
        /// Update the game timer.
        /// </summary>
        private void UpdateGameTimer()
        {
            // Update the game time
            _gameTime -= Time.deltaTime;

            // Clamp the game time
            _gameTime = Mathf.Max(0f, _gameTime);

            // Update the timer UI
            UpdateTimerUI();

            // Check if the game is over
            if (_gameTime <= 0f)
            {
                // TODO: End the game
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
        /// Handle score changed event.
        /// </summary>
        /// <param name="newScore">The new score.</param>
        private void HandleScoreChanged(int newScore)
        {
            // Update the score
            _score = newScore;

            // Update the score UI
            UpdateScoreUI();
        }

        /// <summary>
        /// Handle combo achieved event.
        /// </summary>
        /// <param name="comboCount">The combo count.</param>
        private void HandleComboAchieved(int comboCount)
        {
            // Update the combo count
            _comboCount = comboCount;

            // Show combo notification
            // TODO: Implement combo notification
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
    }
}
