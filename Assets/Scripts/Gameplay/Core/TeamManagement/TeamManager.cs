using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections.Generic;
using System.Linq;
using RecipeRage.Core.Networking;

namespace RecipeRage.Gameplay.Core.TeamManagement
{
    public class TeamManager : NetworkBehaviour
    {
        [Header("Team Settings")]
        [SerializeField] private int maxTeams = 2;
        [SerializeField] private int maxPlayersPerTeam = 3;

        // Network state
        private readonly NetworkVariable<GameMode> _currentGameMode = new();
        private readonly NetworkList<TeamData> _teams;
        private readonly NetworkDictionary<ulong, int> _playerTeams;

        // Events
        public event Action<GameMode> OnGameModeChanged;
        public event Action<TeamData> OnTeamScoreChanged;
        public event Action<ulong, int> OnPlayerTeamChanged;

        public GameMode CurrentGameMode => _currentGameMode.Value;

        public TeamManager()
        {
            _teams = new NetworkList<TeamData>();
            _playerTeams = new NetworkDictionary<ulong, int>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                InitializeTeams();
            }

            _currentGameMode.OnValueChanged += (_, newMode) => OnGameModeChanged?.Invoke(newMode);
            _teams.OnListChanged += HandleTeamDataChanged;
            _playerTeams.OnDictionaryChanged += HandlePlayerTeamChanged;
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetGameModeServerRpc(GameMode mode)
        {
            if (!IsServer) return;

            _currentGameMode.Value = mode;
            maxPlayersPerTeam = GetMaxPlayersForMode(mode);
            InitializeTeams();
        }

        private int GetMaxPlayersForMode(GameMode mode)
        {
            return mode switch
            {
                GameMode.OneVsOne => 1,
                GameMode.TwoVsTwo => 2,
                GameMode.ThreeVsThree => 3,
                _ => 1
            };
        }

        private void InitializeTeams()
        {
            _teams.Clear();
            for (int i = 0; i < maxTeams; i++)
            {
                _teams.Add(new TeamData
                {
                    TeamId = i,
                    Score = 0,
                    PlayerCount = 0
                });
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void AssignPlayerToTeamServerRpc(ulong playerId, int teamId)
        {
            if (!IsServer) return;
            if (teamId < 0 || teamId >= maxTeams) return;

            // Check if team is full
            var team = _teams[teamId];
            if (team.PlayerCount >= maxPlayersPerTeam)
            {
                // Try to find another team
                teamId = FindAvailableTeam();
                if (teamId == -1) return; // No available teams
                team = _teams[teamId];
            }

            // Remove from old team if exists
            if (_playerTeams.ContainsKey(playerId))
            {
                int oldTeamId = _playerTeams[playerId];
                var oldTeam = _teams[oldTeamId];
                oldTeam.PlayerCount--;
                _teams[oldTeamId] = oldTeam;
            }

            // Add to new team
            _playerTeams[playerId] = teamId;
            team.PlayerCount++;
            _teams[teamId] = team;
        }

        private int FindAvailableTeam()
        {
            for (int i = 0; i < _teams.Count; i++)
            {
                if (_teams[i].PlayerCount < maxPlayersPerTeam)
                {
                    return i;
                }
            }
            return -1;
        }

        [ServerRpc(RequireOwnership = false)]
        public void AddTeamScoreServerRpc(int teamId, int points)
        {
            if (!IsServer) return;
            if (teamId < 0 || teamId >= _teams.Count) return;

            var team = _teams[teamId];
            team.Score += points;
            _teams[teamId] = team;
        }

        public int GetPlayerTeam(ulong playerId)
        {
            return _playerTeams.ContainsKey(playerId) ? _playerTeams[playerId] : -1;
        }

        public bool ArePlayersInSameTeam(ulong player1Id, ulong player2Id)
        {
            if (!_playerTeams.ContainsKey(player1Id) || !_playerTeams.ContainsKey(player2Id))
                return false;

            return _playerTeams[player1Id] == _playerTeams[player2Id];
        }

        public IEnumerable<ulong> GetTeamPlayers(int teamId)
        {
            return _playerTeams.Where(kvp => kvp.Value == teamId).Select(kvp => kvp.Key);
        }

        private void HandleTeamDataChanged()
        {
            foreach (var team in _teams)
            {
                OnTeamScoreChanged?.Invoke(team);
            }
        }

        private void HandlePlayerTeamChanged(NetworkDictionaryEvent<ulong, int> changeEvent)
        {
            OnPlayerTeamChanged?.Invoke(changeEvent.Key, changeEvent.Value);
        }

        public void Reset()
        {
            if (!IsServer) return;

            foreach (var team in _teams)
            {
                var resetTeam = team;
                resetTeam.Score = 0;
                _teams[team.TeamId] = resetTeam;
            }
        }
    }

    public struct TeamData : INetworkSerializable, IEquatable<TeamData>
    {
        public int TeamId;
        public int Score;
        public int PlayerCount;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref TeamId);
            serializer.SerializeValue(ref Score);
            serializer.SerializeValue(ref PlayerCount);
        }

        public bool Equals(TeamData other)
        {
            return TeamId == other.TeamId &&
                   Score == other.Score &&
                   PlayerCount == other.PlayerCount;
        }
    }

    public enum GameMode
    {
        OneVsOne,
        TwoVsTwo,
        ThreeVsThree
    }
}
