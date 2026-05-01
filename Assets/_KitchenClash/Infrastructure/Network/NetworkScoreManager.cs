using System;
using System.Collections.Generic;
using Core.Logging;
using Unity.Netcode;

namespace KitchenClash.Infrastructure.Network
{
    /// <summary>
    /// Manages score synchronization across the network.
    /// </summary>
    public class NetworkScoreManager : NetworkBehaviour
    {
        private NetworkList<PlayerScore> _playerScores;

        private NetworkVariable<int> _teamAScore = new NetworkVariable<int>(0);
        private NetworkVariable<int> _teamBScore = new NetworkVariable<int>(0);

        public event Action<ulong, int> OnPlayerScoreUpdated;
        public event Action<int, int> OnTeamScoreUpdated;
        public event Action OnScoreboardUpdated;

        private void Awake()
        {
            _playerScores = new NetworkList<PlayerScore>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _playerScores.OnListChanged += OnPlayerScoresChanged;
            _teamAScore.OnValueChanged += OnTeamAScoreChanged;
            _teamBScore.OnValueChanged += OnTeamBScoreChanged;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            _playerScores.OnListChanged -= OnPlayerScoresChanged;
            _teamAScore.OnValueChanged -= OnTeamAScoreChanged;
            _teamBScore.OnValueChanged -= OnTeamBScoreChanged;
        }

        [ServerRpc(RequireOwnership = false)]
        public void AddScoreServerRpc(ulong playerId, int points, ScoreReason reason, ServerRpcParams rpcParams = default)
        {
            int index = FindPlayerScoreIndex(playerId);

            if (index >= 0)
            {
                PlayerScore score = _playerScores[index];
                score.Score += points;

                if (reason == ScoreReason.DishCompleted)
                {
                    score.DishesCompleted++;
                }
                else if (reason == ScoreReason.PerfectDish)
                {
                    score.PerfectDishes++;
                }

                _playerScores[index] = score;
            }
            else
            {
                PlayerScore newScore = new PlayerScore
                {
                    ClientId = playerId,
                    Score = points,
                    DishesCompleted = reason == ScoreReason.DishCompleted ? 1 : 0,
                    PerfectDishes = reason == ScoreReason.PerfectDish ? 1 : 0
                };

                _playerScores.Add(newScore);
            }

            ShowScorePopupClientRpc(playerId, points, reason);

            GameLogger.Log($"Added {points} points to player {playerId} for {reason}");
        }

        [ServerRpc(RequireOwnership = false)]
        public void AddTeamScoreServerRpc(int teamId, int points, ScoreReason reason, ServerRpcParams rpcParams = default)
        {
            if (teamId == 0)
            {
                _teamAScore.Value += points;
            }
            else if (teamId == 1)
            {
                _teamBScore.Value += points;
            }

            GameLogger.Log($"Added {points} points to team {teamId} for {reason}");
        }

        public int GetPlayerScore(ulong playerId)
        {
            int index = FindPlayerScoreIndex(playerId);
            return index >= 0 ? _playerScores[index].Score : 0;
        }

        public List<PlayerScore> GetAllPlayerScores()
        {
            List<PlayerScore> scores = new List<PlayerScore>();
            foreach (PlayerScore score in _playerScores)
            {
                scores.Add(score);
            }
            return scores;
        }

        public int GetTeamAScore() => _teamAScore.Value;
        public int GetTeamBScore() => _teamBScore.Value;

        [ServerRpc(RequireOwnership = false)]
        public void ResetScoresServerRpc()
        {
            _playerScores.Clear();
            _teamAScore.Value = 0;
            _teamBScore.Value = 0;

            GameLogger.Log("Reset all scores");
        }

        private int FindPlayerScoreIndex(ulong playerId)
        {
            for (int i = 0; i < _playerScores.Count; i++)
            {
                if (_playerScores[i].ClientId == playerId)
                {
                    return i;
                }
            }
            return -1;
        }

        private void OnPlayerScoresChanged(NetworkListEvent<PlayerScore> changeEvent)
        {
            if (changeEvent.Type == NetworkListEvent<PlayerScore>.EventType.Value ||
                changeEvent.Type == NetworkListEvent<PlayerScore>.EventType.Add)
            {
                PlayerScore score = _playerScores[changeEvent.Index];
                OnPlayerScoreUpdated?.Invoke(score.ClientId, score.Score);
            }

            OnScoreboardUpdated?.Invoke();
        }

        private void OnTeamAScoreChanged(int previousValue, int newValue)
        {
            OnTeamScoreUpdated?.Invoke(0, newValue);
            OnScoreboardUpdated?.Invoke();
        }

        private void OnTeamBScoreChanged(int previousValue, int newValue)
        {
            OnTeamScoreUpdated?.Invoke(1, newValue);
            OnScoreboardUpdated?.Invoke();
        }

        [ClientRpc]
        private void ShowScorePopupClientRpc(ulong playerId, int points, ScoreReason reason)
        {
            GameLogger.Log($"Player {playerId} earned {points} points for {reason}");
        }

        [ClientRpc]
        public void UpdateScoreboardClientRpc()
        {
            OnScoreboardUpdated?.Invoke();
        }
    }

    public struct PlayerScore : INetworkSerializable, IEquatable<PlayerScore>
    {
        public ulong ClientId;
        public int Score;
        public int DishesCompleted;
        public int PerfectDishes;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref Score);
            serializer.SerializeValue(ref DishesCompleted);
            serializer.SerializeValue(ref PerfectDishes);
        }

        public bool Equals(PlayerScore other)
        {
            return ClientId == other.ClientId;
        }
    }

    public enum ScoreReason
    {
        DishCompleted,
        PerfectDish,
        TimeBonus,
        ComboBonus,
        TeamworkBonus
    }
}
