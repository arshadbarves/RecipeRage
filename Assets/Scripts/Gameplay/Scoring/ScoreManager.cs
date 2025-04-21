using System;
using Core.GameModes;
using Gameplay.Cooking;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Scoring
{
    /// <summary>
    /// Manages the scoring system for RecipeRage.
    /// </summary>
    public class ScoreManager : NetworkBehaviour
    {
        [Header("Score Settings")]
        [SerializeField] private int _baseOrderPoints = 100;
        [SerializeField] private int _timeBonus = 50;
        [SerializeField] private int _perfectBonus = 25;
        [SerializeField] private int _comboBonus = 10;
        [SerializeField] private float _comboTimeWindow = 30f;

        [Header("References")]
        [SerializeField] private OrderManager _orderManager;
        [SerializeField] private GameModeManager _gameModeManager;

        /// <summary>
        /// Event triggered when the score changes.
        /// </summary>
        public event Action<int> OnScoreChanged;

        /// <summary>
        /// Event triggered when a combo is achieved.
        /// </summary>
        public event Action<int> OnComboAchieved;

        /// <summary>
        /// The current score.
        /// </summary>
        private NetworkVariable<int> _score = new NetworkVariable<int>(0);

        /// <summary>
        /// The current combo count.
        /// </summary>
        private NetworkVariable<int> _comboCount = new NetworkVariable<int>(0);

        /// <summary>
        /// The time of the last completed order.
        /// </summary>
        private float _lastOrderCompletionTime;

        /// <summary>
        /// Initialize the score manager.
        /// </summary>
        private void Awake()
        {
            // Find the order manager if not set
            if (_orderManager == null)
            {
                _orderManager = FindFirstObjectByType<OrderManager>();
            }

            // Find the game mode manager if not set
            if (_gameModeManager == null)
            {
                _gameModeManager = FindFirstObjectByType<GameModeManager>();
            }
        }

        /// <summary>
        /// Set up network variables when the network object spawns.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Subscribe to network variable changes
            _score.OnValueChanged += OnScoreValueChanged;
            _comboCount.OnValueChanged += OnComboValueChanged;

            // Subscribe to order manager events
            if (_orderManager != null)
            {
                _orderManager.OnOrderCompleted += HandleOrderCompleted;
            }
        }

        /// <summary>
        /// Clean up when the network object despawns.
        /// </summary>
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            // Unsubscribe from network variable changes
            _score.OnValueChanged -= OnScoreValueChanged;
            _comboCount.OnValueChanged -= OnComboValueChanged;

            // Unsubscribe from order manager events
            if (_orderManager != null)
            {
                _orderManager.OnOrderCompleted -= HandleOrderCompleted;
            }
        }

        /// <summary>
        /// Handle order completed event.
        /// </summary>
        /// <param name="order">The order that was completed</param>
        private void HandleOrderCompleted(RecipeOrderState order)
        {
            if (!IsServer)
            {
                return;
            }

            // Get base points from the order
            int basePoints = order.PointValue;

            // Calculate time bonus (if order has remaining time)
            int timeBonus = 0;
            if (order.TimeLimit > 0)
            {
                float timePercentage = order.RemainingTime / order.TimeLimit;
                timeBonus = Mathf.RoundToInt(_timeBonus * timePercentage);
            }

            // Check for combo
            float timeSinceLastOrder = Time.time - _lastOrderCompletionTime;
            bool isCombo = timeSinceLastOrder <= _comboTimeWindow;

            // Update combo count
            if (isCombo)
            {
                _comboCount.Value++;
            }
            else
            {
                _comboCount.Value = 1;
            }

            // Calculate combo bonus
            int comboBonus = (_comboCount.Value - 1) * _comboBonus;

            // Calculate total points
            int totalPoints = basePoints + timeBonus + comboBonus;

            // Add points to the score
            _score.Value += totalPoints;

            // Update last order completion time
            _lastOrderCompletionTime = Time.time;

            // Log the score breakdown
            Debug.Log($"[ScoreManager] Order completed: Base={basePoints}, Time={timeBonus}, Combo={comboBonus}, Total={totalPoints}");
        }

        /// <summary>
        /// Add points to the score.
        /// </summary>
        /// <param name="points">The points to add</param>
        public void AddPoints(int points)
        {
            if (!IsServer)
            {
                // Request points from the server
                AddPointsServerRpc(points);
                return;
            }

            _score.Value += points;
        }

        /// <summary>
        /// Get the current score.
        /// </summary>
        /// <returns>The current score</returns>
        public int GetScore()
        {
            return _score.Value;
        }

        /// <summary>
        /// Get the current combo count.
        /// </summary>
        /// <returns>The current combo count</returns>
        public int GetComboCount()
        {
            return _comboCount.Value;
        }

        /// <summary>
        /// Reset the score.
        /// </summary>
        public void ResetScore()
        {
            if (!IsServer)
            {
                // Request reset from the server
                ResetScoreServerRpc();
                return;
            }

            _score.Value = 0;
            _comboCount.Value = 0;
        }

        /// <summary>
        /// Handle score value changed.
        /// </summary>
        /// <param name="previousValue">The previous value</param>
        /// <param name="newValue">The new value</param>
        private void OnScoreValueChanged(int previousValue, int newValue)
        {
            // Trigger event
            OnScoreChanged?.Invoke(newValue);
        }

        /// <summary>
        /// Handle combo value changed.
        /// </summary>
        /// <param name="previousValue">The previous value</param>
        /// <param name="newValue">The new value</param>
        private void OnComboValueChanged(int previousValue, int newValue)
        {
            // Only trigger event if combo increased
            if (newValue > previousValue && newValue > 1)
            {
                OnComboAchieved?.Invoke(newValue);
            }
        }

        /// <summary>
        /// Request points from the server.
        /// </summary>
        /// <param name="points">The points to add</param>
        [ServerRpc(RequireOwnership = false)]
        private void AddPointsServerRpc(int points)
        {
            AddPoints(points);
        }

        /// <summary>
        /// Request reset from the server.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        private void ResetScoreServerRpc()
        {
            ResetScore();
        }

        /// <summary>
        /// Reset all scores for all players.
        /// </summary>
        public void ResetScores()
        {
            if (!IsServer)
            {
                Debug.LogWarning("[ScoreManager] Only the server can reset all scores.");
                return;
            }

            Debug.Log("[ScoreManager] Resetting all scores");

            // Reset the main score
            ResetScore();

            // TODO: When team or player-specific scores are implemented,
            // reset those here as well
        }
    }
}
