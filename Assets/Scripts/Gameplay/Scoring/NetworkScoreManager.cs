using System;
using System.Collections.Generic;
using Unity.Netcode;
using Core.Logging;

namespace Gameplay.Scoring
{
    /// <summary>
    /// Manages score synchronization across the network.
    /// Follows Single Responsibility Principle - handles only score network state.
    /// </summary>
    public class NetworkScoreManager : NetworkBehaviour
    {
        /// <summary>
        /// The list of player scores.
        /// </summary>
        private NetworkList<PlayerScore> _playerScores;

        /// <summary>
        /// Team A score (for team modes).
        /// </summary>
        private NetworkVariable<int> _teamAScore = new NetworkVariable<int>(0);

        /// <summary>
        /// Team B score (for team modes).
        /// </summary>
        private NetworkVariable<int> _teamBScore = new NetworkVariable<int>(0);

        /// <summary>
        /// Event triggered when a player's score is updated.
        /// </summary>
        public event Action<ulong, int> OnPlayerScoreUpdated;

        /// <summary>
        /// Event triggered when a team's score is updated.
        /// </summary>
        public event Action<int, int> OnTeamScoreUpdated;

        /// <summary>
        /// Event triggered when the scoreboard should be updated.
        /// </summary>
        public event Action OnScoreboardUpdated;

        /// <summary>
        /// Initialize the network score manager.
        /// </summary>
        private void Awake()
        {
            _playerScores = new NetworkList<PlayerScore>();
        }

        /// <summary>
        /// Set up network variable callbacks.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _playerScores.OnListChanged += OnPlayerScoresChanged;
            _teamAScore.OnValueChanged += OnTeamAScoreChanged;
            _teamBScore.OnValueChanged += OnTeamBScoreChanged;
        }

        /// <summary>
        /// Clean up network variable callbacks.
        /// </summary>
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            _playerScores.OnListChanged -= OnPlayerScoresChanged;
            _teamAScore.OnValueChanged -= OnTeamAScoreChanged;
            _teamBScore.OnValueChanged -= OnTeamBScoreChanged;
        }

        /// <summary>
        /// Add score to a player.
        /// </summary>
        /// <param name="playerId">The player ID</param>
        /// <param name="points">The points to add</param>
        /// <param name="reason">The reason for the score</param>
        /// <param name="rpcParams">RPC parameters</param>
        [ServerRpc(RequireOwnership = false)]
        public void AddScoreServerRpc(ulong playerId, int points, ScoreReason reason, ServerRpcParams rpcParams = default)
        {
            // Find or create player score
            int index = FindPlayerScoreIndex(playerId);

            if (index >= 0)
            {
                // Update existing score
                PlayerScore score = _playerScores[index];
                score.Score += points;

                // Update stats based on reason
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
                // Create new score entry
                PlayerScore newScore = new PlayerScore
                {
                    ClientId = playerId,
                    Score = points,
                    DishesCompleted = reason == ScoreReason.DishCompleted ? 1 : 0,
                    PerfectDishes = reason == ScoreReason.PerfectDish ? 1 : 0
                };

                _playerScores.Add(newScore);
            }

            // Show score popup on all clients
            ShowScorePopupClientRpc(playerId, points, reason);

            GameLogger.Log($"Added {points} points to player {playerId} for {reason}");
        }

        /// <summary>
        /// Add score to a team.
        /// </summary>
        /// <param name="teamId">The team ID (0 = Team A, 1 = Team B)</param>
        /// <param name="points">The points to add</param>
        /// <param name="reason">The reason for the score</param>
        /// <param name="rpcParams">RPC parameters</param>
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

        /// <summary>
        /// Get a player's score.
        /// </summary>
        /// <param name="playerId">The player ID</param>
        /// <returns>The player's score, or 0 if not found</returns>
        public int GetPlayerScore(ulong playerId)
        {
            int index = FindPlayerScoreIndex(playerId);
            return index >= 0 ? _playerScores[index].Score : 0;
        }

        /// <summary>
        /// Get all player scores.
        /// </summary>
        /// <returns>A list of all player scores</returns>
        public List<PlayerScore> GetAllPlayerScores()
        {
            List<PlayerScore> scores = new List<PlayerScore>();
            foreach (PlayerScore score in _playerScores)
            {
                scores.Add(score);
            }
            return scores;
        }

        /// <summary>
        /// Get team A score.
        /// </summary>
        public int GetTeamAScore() => _teamAScore.Value;

        /// <summary>
        /// Get team B score.
        /// </summary>
        public int GetTeamBScore() => _teamBScore.Value;

        /// <summary>
        /// Reset all scores.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void ResetScoresServerRpc()
        {
            _playerScores.Clear();
            _teamAScore.Value = 0;
            _teamBScore.Value = 0;

            GameLogger.Log("Reset all scores");
        }

        /// <summary>
        /// Find the index of a player's score.
        /// </summary>
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

        /// <summary>
        /// Handle player scores list changes.
        /// </summary>
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

        /// <summary>
        /// Handle team A score changes.
        /// </summary>
        private void OnTeamAScoreChanged(int previousValue, int newValue)
        {
            OnTeamScoreUpdated?.Invoke(0, newValue);
            OnScoreboardUpdated?.Invoke();
        }

        /// <summary>
        /// Handle team B score changes.
        /// </summary>
        private void OnTeamBScoreChanged(int previousValue, int newValue)
        {
            OnTeamScoreUpdated?.Invoke(1, newValue);
            OnScoreboardUpdated?.Invoke();
        }

        /// <summary>
        /// Show a score popup on all clients.
        /// </summary>
        [ClientRpc]
        private void ShowScorePopupClientRpc(ulong playerId, int points, ScoreReason reason)
        {
            // UI can subscribe to this to show score popups
            GameLogger.Log($"Player {playerId} earned {points} points for {reason}");
        }

        /// <summary>
        /// Update the scoreboard on all clients.
        /// </summary>
        [ClientRpc]
        public void UpdateScoreboardClientRpc()
        {
            OnScoreboardUpdated?.Invoke();
        }
    }

    /// <summary>
    /// Player score data structure.
    /// </summary>
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

    /// <summary>
    /// Reasons for scoring.
    /// </summary>
    public enum ScoreReason
    {
        DishCompleted,
        PerfectDish,
        TimeBonus,
        ComboBonus,
        TeamworkBonus
    }
}
