using System;
using System.Collections.Generic;
using System.Linq;
using RecipeRage.Core.Player;
using Unity.Netcode;
using UnityEngine;

namespace RecipeRage.Core.GameMode
{
    /// <summary>
    /// Manages game modes and team functionality
    /// </summary>
    public class GameModeManager : NetworkBehaviour
    {
        #region Events

        public event Action<GameModeType> OnGameModeChanged;
        public event Action<Team> OnTeamScoreUpdated;
        public event Action<PlayerController> OnPlayerJoined;
        public event Action<PlayerController> OnPlayerLeft;

        #endregion

        #region Properties

        public GameModeType CurrentGameMode { get; private set; }

        public bool IsTeamMode => CurrentGameMode == GameModeType.TeamBattle;
        public IReadOnlyList<Team> Teams => _teams.AsReadOnly();
        public IReadOnlyDictionary<ulong, Team> PlayerTeams => _playerTeams;

        #endregion

        #region Serialized Fields

        [Header("Game Mode Settings")] [SerializeField]
        private GameModeType _defaultGameMode = GameModeType.Classic;
        [SerializeField] private int _maxTeams = 2;
        [SerializeField] private int _playersPerTeam = 2;

        #endregion

        #region Private Fields

        private readonly List<Team> _teams = new List<Team>();
        private readonly Dictionary<ulong, Team> _playerTeams = new Dictionary<ulong, Team>();
        private GameModeBase _activeGameMode;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeTeams();
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Changes the current game mode
        /// </summary>
        /// <param name="newMode"> New game mode to switch to </param>
        [ServerRpc(RequireOwnership = false)]
        public void ChangeGameModeServerRpc(GameModeType newMode)
        {
            if (CurrentGameMode == newMode)
                return;

            // Clean up current game mode
            if (_activeGameMode != null)
            {
                _activeGameMode.EndGame();
                Destroy(_activeGameMode.gameObject);
            }

            // Create new game mode
            CurrentGameMode = newMode;
            CreateGameMode();
            OnGameModeChanged?.Invoke(newMode);

            // Notify clients
            ChangeGameModeClientRpc(newMode);
        }

        /// <summary>
        /// Assigns a player to a team
        /// </summary>
        /// <param name="playerId"> Player's network ID </param>
        /// <param name="teamId"> Team ID to assign to </param>
        [ServerRpc(RequireOwnership = false)]
        public void AssignPlayerToTeamServerRpc(ulong playerId, int teamId)
        {
            if (teamId < 0 || teamId >= _teams.Count)
                return;

            var team = _teams[teamId];
            if (team.Players.Count >= _playersPerTeam)
                return;

            // Remove from current team if any
            if (_playerTeams.TryGetValue(playerId, out var currentTeam))
            {
                currentTeam.RemovePlayer(playerId);
            }

            // Add to new team
            team.AddPlayer(playerId);
            _playerTeams[playerId] = team;

            // Notify clients
            AssignPlayerToTeamClientRpc(playerId, teamId);
        }

        /// <summary>
        /// Updates a team's score
        /// </summary>
        /// <param name="teamId"> Team ID </param>
        /// <param name="scoreToAdd"> Score to add </param>
        [ServerRpc(RequireOwnership = false)]
        public void UpdateTeamScoreServerRpc(int teamId, int scoreToAdd)
        {
            if (teamId < 0 || teamId >= _teams.Count)
                return;

            var team = _teams[teamId];
            team.AddScore(scoreToAdd);
            OnTeamScoreUpdated?.Invoke(team);

            // Notify clients
            UpdateTeamScoreClientRpc(teamId, team.Score);
        }

        #endregion

        #region Private Methods

        private void InitializeTeams()
        {
            _teams.Clear();
            for (int i = 0; i < _maxTeams; i++)
            {
                _teams.Add(new Team(i));
            }
        }

        private void CreateGameMode()
        {
            var gameModeObj = new GameObject($"{CurrentGameMode}GameMode");
            gameModeObj.transform.SetParent(transform);

            switch (CurrentGameMode)
            {
                case GameModeType.Classic:
                    _activeGameMode = gameModeObj.AddComponent<ClassicGameMode>();
                    break;
                case GameModeType.TimeAttack:
                    // TODO: Implement TimeAttackGameMode
                    break;
                case GameModeType.TeamBattle:
                    // TODO: Implement TeamBattleGameMode
                    break;
            }
        }

        private void HandleClientConnected(ulong clientId)
        {
            // Auto-assign to team in team mode
            if (IsTeamMode)
            {
                var teamWithSpace = _teams.FirstOrDefault(t => t.Players.Count < _playersPerTeam);
                if (teamWithSpace != null)
                {
                    AssignPlayerToTeamServerRpc(clientId, teamWithSpace.TeamId);
                }
            }
        }

        // TODO: We need to handle player disconnections and also allow reconnection, so we can't remove the player from the team.
        private void HandleClientDisconnected(ulong clientId)
        {
            if (_playerTeams.TryGetValue(clientId, out var team))
            {
                team.RemovePlayer(clientId);
                _playerTeams.Remove(clientId);
            }
        }

        #endregion

        #region ClientRpc Methods

        [ClientRpc]
        private void ChangeGameModeClientRpc(GameModeType newMode)
        {
            CurrentGameMode = newMode;
            OnGameModeChanged?.Invoke(newMode);
        }

        [ClientRpc]
        private void AssignPlayerToTeamClientRpc(ulong playerId, int teamId)
        {
            if (teamId >= 0 && teamId < _teams.Count)
            {
                _playerTeams[playerId] = _teams[teamId];
            }
        }

        [ClientRpc]
        private void UpdateTeamScoreClientRpc(int teamId, int newScore)
        {
            if (teamId >= 0 && teamId < _teams.Count)
            {
                _teams[teamId].Score = newScore;
                OnTeamScoreUpdated?.Invoke(_teams[teamId]);
            }
        }

        #endregion
    }

    /// <summary>
    /// Available game modes
    /// </summary>
    public enum GameModeType
    {
        Classic,
        TimeAttack,
        TeamBattle
    }

    /// <summary>
    /// Represents a team in team-based game modes
    /// </summary>
    public class Team
    {

        public Team(int teamId)
        {
            TeamId = teamId;
        }
        public int TeamId { get; }
        public int Score { get; set; }
        public HashSet<ulong> Players { get; } = new HashSet<ulong>();

        public void AddPlayer(ulong playerId)
        {
            Players.Add(playerId);
        }

        public void RemovePlayer(ulong playerId)
        {
            Players.Remove(playerId);
        }

        public void AddScore(int points)
        {
            Score += points;
        }
    }
}