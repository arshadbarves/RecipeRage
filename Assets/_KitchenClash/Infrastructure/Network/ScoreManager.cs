using System;
using System.Collections.Generic;
using Gameplay.Cooking;
using Gameplay.Shared;
using Core.Logging;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace KitchenClash.Infrastructure.Network
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

        public event Action<int, int> OnTeamScoreChanged;
        public event Action<int, int> OnTeamComboAchieved;

        [Inject] private IMatchContext _matchContext;

        private NetworkList<int> _teamScores;
        private NetworkList<int> _teamComboCounts;

        private Dictionary<int, float> _lastTeamOrderCompletionTime = new Dictionary<int, float>();

        private void Awake()
        {
            LifetimeScope scope = LifetimeScope.Find<LifetimeScope>();
            if (scope != null)
            {
                scope.Container.Inject(this);
            }

            ResolveRuntimeDependencies();
            _teamScores = new NetworkList<int>();
            _teamComboCounts = new NetworkList<int>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            ResolveRuntimeDependencies();

            if (IsServer)
            {
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

        private void ResolveRuntimeDependencies()
        {
            _matchContext?.Refresh();
            _orderManager ??= _matchContext?.OrderManager;
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

            int teamId = order.CompletedByTeamId;

            if (teamId < 0 || teamId >= _teamScores.Count)
            {
                 GameLogger.LogWarning($"Invalid team ID {teamId} for scoring. Defaulting to 0.");
                 teamId = 0;
            }

            int basePoints = Mathf.Max(0, order.PointValue);

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
            int totalPoints = basePoints + comboBonus;

            _teamScores[teamId] += totalPoints;
            _lastTeamOrderCompletionTime[teamId] = Time.time;

            GameLogger.Log($"Team {teamId} Score: {totalPoints} (Base:{basePoints}, Combo:{comboBonus}). Total: {_teamScores[teamId]}");
        }

        public int GetScore(int teamId)
        {
            if (teamId >= 0 && teamId < _teamScores.Count)
                return _teamScores[teamId];
            return 0;
        }

        public IReadOnlyList<int> GetTeamScores()
        {
            List<int> scores = new List<int>(_teamScores.Count);
            for (int i = 0; i < _teamScores.Count; i++)
            {
                scores.Add(_teamScores[i]);
            }

            return scores;
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
