using System;
using System.Collections.Generic;
using Gameplay.Cooking;
using Core.Logging;
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

        /// <summary>
        /// Event triggered when a team's score changes.
        /// arg1: Team ID, arg2: New Score
        /// </summary>
        public event Action<int, int> OnTeamScoreChanged;

        /// <summary>
        /// Event triggered when a combo is achieved by a team.
        /// arg1: Team ID, arg2: Combo Count
        /// </summary>
        public event Action<int, int> OnTeamComboAchieved;

        // Scores for Team 0 and Team 1
        private NetworkList<int> _teamScores;
        private NetworkList<int> _teamComboCounts;

        // Track last completion time per team locally (server only)
        private Dictionary<int, float> _lastTeamOrderCompletionTime = new Dictionary<int, float>();

        private void Awake()
        {
            if (_orderManager == null)
            {
                _orderManager = FindFirstObjectByType<OrderManager>();
            }
            // Initialize NetworkLists
            _teamScores = new NetworkList<int>();
            _teamComboCounts = new NetworkList<int>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                // Ensure we have slots for 2 teams
                if (_teamScores.Count < 2)
                {
                    _teamScores.Add(0);
                    _teamScores.Add(0);
                }
                if (_teamComboCounts.Count < 2)
                {
                    _teamComboCounts.Add(0);
                    _teamComboCounts.Add(0);
                }
            }

            _teamScores.OnListChanged += OnTeamScoresChanged;
            _teamComboCounts.OnListChanged += OnTeamComboChanged;

            if (_orderManager != null)
            {
                _orderManager.OnOrderCompleted += HandleOrderCompleted;
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            _teamScores.OnListChanged -= OnTeamScoresChanged;
            _teamComboCounts.OnListChanged -= OnTeamComboChanged;

            if (_orderManager != null)
            {
                _orderManager.OnOrderCompleted -= HandleOrderCompleted;
            }
        }

        private void HandleOrderCompleted(RecipeOrderState order)
        {
            if (!IsServer) return;

            // Determine which team completed the order.
            // Requirement modification: RecipeOrderState needs 'CompletedByTeamId'
            // For now, if missing, default to 0 (Co-op)
            int teamId = order.CompletedByTeamId;

            if (teamId < 0 || teamId >= _teamScores.Count)
            {
                 GameLogger.LogWarning($"Invalid team ID {teamId} for scoring. Defaulting to 0.");
                 teamId = 0;
            }

            int basePoints = order.PointValue;
            int timeBonus = 0;
            if (order.TimeLimit > 0)
            {
                float timePercentage = order.RemainingTime / order.TimeLimit;
                timeBonus = Mathf.RoundToInt(_timeBonus * timePercentage);
            }

            // Check combo
            if (!_lastTeamOrderCompletionTime.ContainsKey(teamId))
            {
                _lastTeamOrderCompletionTime[teamId] = -999f;
            }

            float timeSinceLast = Time.time - _lastTeamOrderCompletionTime[teamId];
            bool isCombo = timeSinceLast <= _comboTimeWindow;

            if (isCombo)
            {
                _teamComboCounts[teamId]++;
            }
            else
            {
                _teamComboCounts[teamId] = 1;
            }

            int comboBonus = (_teamComboCounts[teamId] - 1) * _comboBonus;
            int totalPoints = basePoints + timeBonus + comboBonus;

            _teamScores[teamId] += totalPoints;
            _lastTeamOrderCompletionTime[teamId] = Time.time;

            GameLogger.Log($"Team {teamId} Score: {totalPoints} (Base:{basePoints}, Time:{timeBonus}, Combo:{comboBonus}). Total: {_teamScores[teamId]}");
        }

        // --- Public API ---

        public int GetScore(int teamId)
        {
            if (teamId >= 0 && teamId < _teamScores.Count)
                return _teamScores[teamId];
            return 0;
        }

        public void AddPoints(int teamId, int points)
        {
             if (!IsServer) return;
             if (teamId >= 0 && teamId < _teamScores.Count)
                _teamScores[teamId] += points;
        }

        public void ResetScores()
        {
            if (!IsServer) return;
            for(int i=0; i<_teamScores.Count; i++) _teamScores[i] = 0;
            for(int i=0; i<_teamComboCounts.Count; i++) _teamComboCounts[i] = 0;
            _lastTeamOrderCompletionTime.Clear();
        }

        // --- Events ---

        private void OnTeamScoresChanged(NetworkListEvent<int> changeEvent)
        {
            OnTeamScoreChanged?.Invoke(changeEvent.Index, changeEvent.Value);
        }

        private void OnTeamComboChanged(NetworkListEvent<int> changeEvent)
        {
            if (changeEvent.Value > 1)
                OnTeamComboAchieved?.Invoke(changeEvent.Index, changeEvent.Value);
        }
    }
}
