using System;
using System.Collections.Generic;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Network.Cooking;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace KitchenClash.Infrastructure.Network
{
    /// <summary>
    /// Network-authoritative score manager. Delegates scoring calculations to IScoreService.
    /// </summary>
    public class ScoreManager : NetworkBehaviour
    {
        public event Action<int, int> OnTeamScoreChanged;

        [Header("References")]
        [SerializeField] private OrderManager _orderManager;

        [Inject] private IMatchContext _matchContext;
        [Inject] private IScoreService _scoreService;
        [Inject] private IEventBus _eventBus;

        private NetworkList<int> _teamScores;
        private NetworkList<int> _teamComboCounts;

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

            if (_eventBus != null)
            {
                _eventBus.Subscribe<ScoreChangedEvent>(HandleScoreChanged);
            }

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

            if (_eventBus != null)
            {
                _eventBus.Unsubscribe<ScoreChangedEvent>(HandleScoreChanged);
            }

            if (_orderManager != null)
            {
                _orderManager.OnOrderCompleted -= HandleOrderCompleted;
            }
        }

        private void HandleOrderCompleted(RecipeOrderState order)
        {
            if (!IsServer)
            {
                return;
            }

            int teamId = order.CompletedByTeamId;
            if (teamId < 0 || teamId >= _teamScores.Count)
            {
                GameLogger.LogWarning($"Invalid team ID {teamId} for scoring. Defaulting to 0.");
                teamId = 0;
            }

            // Delegate scoring calculation to ScoreService
            TeamId team = teamId == 0 ? TeamId.TeamA : TeamId.TeamB;
            var scoreEvent = new ScoreEvent(
                ScoreEventType.DishServed,
                order.RecipeTier,
                order.SpeedRatio,
                order.RhythmBonus,
                order.ComboCount
            );

            _scoreService.AddScore(team, scoreEvent);
        }

        private void HandleScoreChanged(ScoreChangedEvent e)
        {
            if (!IsServer)
            {
                return;
            }

            // Sync network state from ScoreService
            _teamScores[0] = _scoreService.TeamAScore;
            _teamScores[1] = _scoreService.TeamBScore;

            GameLogger.Log($"Score updated: TeamA={_scoreService.TeamAScore}, TeamB={_scoreService.TeamBScore}");
        }

        public int GetScore(int teamId)
        {
            if (teamId >= 0 && teamId < _teamScores.Count)
            {
                return _teamScores[teamId];
            }

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
             if (!IsServer)
             {
                 return;
             }

             if (teamId >= 0 && teamId < _teamScores.Count)
             {
                 _teamScores[teamId] += points;
             }
        }

        public void ResetScores()
        {
            if (!IsServer)
            {
                return;
            }

            for(int i=0; i<_teamScores.Count; i++)
            {
                _teamScores[i] = 0;
            }

            for(int i=0; i<_teamComboCounts.Count; i++)
            {
                _teamComboCounts[i] = 0;
            }
        }

        private void OnTeamScoresChanged(NetworkListEvent<int> changeEvent)
        {
            if (changeEvent.Type == NetworkListEvent<int>.EventType.Value)
            {
                OnTeamScoreChanged?.Invoke(changeEvent.Index, changeEvent.Value);
            }
        }

        private void OnTeamComboChanged(NetworkListEvent<int> changeEvent)
        {
        }
    }
}
