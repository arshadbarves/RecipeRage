using System;
using System.Collections.Generic;
using Core.Networking.Common;
using Core.Networking.Interfaces;
using Core.Networking.Services;
using Core.Networking.Strategies;
using Epic.OnlineServices;
using PlayEveryWare.EpicOnlineServices;
using PlayEveryWare.EpicOnlineServices.Samples;
using UnityEngine;

namespace Core.Networking.EOS
{
    /// <summary>
    /// Facade for lobby functionality - delegates to specialized services (SOLID compliant)
    /// Implements all interfaces to maintain backward compatibility
    /// </summary>
    public class RecipeRageLobbyManager : MonoBehaviour, ILobbyManager, IPlayerManager, IMatchmakingService, ITeamManager
    {
        // Injected dependencies
        private ILobbyManager _lobbyManager;
        private IPlayerManager _playerManager;
        private IMatchmakingService _matchmakingService;
        private ITeamManager _teamManager;

        // Events - delegated from services
        public event Action<Result> OnLobbyCreated;
        public event Action<Result> OnLobbyJoined;
        public event Action<Result> OnLobbyLeft;
        public event Action OnLobbyUpdated;
        public event Action<PlayerInfo> OnPlayerJoined;
        public event Action<PlayerInfo> OnPlayerLeft;
        public event Action OnMatchmakingStarted;
        public event Action OnMatchmakingCancelled;
        public event Action<int> OnPlayersFoundUpdated;
        public event Action OnMatchFound;

        // Properties - delegated from services
        public List<PlayerInfo> TeamA => _teamManager.TeamA;
        public List<PlayerInfo> TeamB => _teamManager.TeamB;
        public GameMode CurrentGameMode => _lobbyManager.CurrentGameMode;
        public string CurrentMapName => _lobbyManager.CurrentMapName;
        public bool IsPrivate => _lobbyManager.IsPrivate;
        public bool IsLobbyOwner => _lobbyManager.IsLobbyOwner;
        public bool IsSearchingForMatch => _matchmakingService.IsSearchingForMatch;
        public float SearchTime => _matchmakingService.SearchTime;
        public int CurrentPlayerCount => _lobbyManager.CurrentPlayerCount;
        public Lobby CurrentLobby => (_lobbyManager as LobbyStateManager)?.CurrentLobby;

        /// <summary>
        /// Initialize with dependency injection
        /// </summary>
        public void Initialize()
        {
            EOSLobbyManager eosLobbyManager = EOSManager.Instance.GetOrCreateManager<EOSLobbyManager>();

            if (eosLobbyManager == null)
            {
                Debug.LogError("[RecipeRageLobbyManager] EOSLobbyManager not found");
                return;
            }

            // Create services with dependency injection
            _teamManager = new TeamManager();
            _lobbyManager = new LobbyStateManager(eosLobbyManager, _teamManager);
            _playerManager = new PlayerManager(eosLobbyManager);

            IMatchmakingStrategy strategy = new QuickMatchStrategy(_lobbyManager);
            _matchmakingService = new MatchmakingService(_lobbyManager, strategy);

            // Wire up events
            WireUpEvents();

            // Initialize services
            _lobbyManager.Initialize();

            Debug.Log("[RecipeRageLobbyManager] Initialized with SOLID architecture");
        }

        /// <summary>
        /// Wire up events from services to facade
        /// </summary>
        private void WireUpEvents()
        {
            _lobbyManager.OnLobbyCreated += (result) =>
            {
                // Update player manager with current lobby
                if (_playerManager is PlayerManager pm && _lobbyManager is LobbyStateManager lsm)
                {
                    pm.SetCurrentLobby(lsm.CurrentLobby);
                }
                OnLobbyCreated?.Invoke(result);
            };

            _lobbyManager.OnLobbyJoined += (result) =>
            {
                // Update player manager with current lobby
                if (_playerManager is PlayerManager pm && _lobbyManager is LobbyStateManager lsm)
                {
                    pm.SetCurrentLobby(lsm.CurrentLobby);
                }
                OnLobbyJoined?.Invoke(result);
            };

            _lobbyManager.OnLobbyLeft += (result) => OnLobbyLeft?.Invoke(result);
            _lobbyManager.OnLobbyUpdated += () =>
            {
                // Update player manager with current lobby
                if (_playerManager is PlayerManager pm && _lobbyManager is LobbyStateManager lsm)
                {
                    pm.SetCurrentLobby(lsm.CurrentLobby);
                }
                OnLobbyUpdated?.Invoke();
            };

            _playerManager.OnPlayerJoined += (player) => OnPlayerJoined?.Invoke(player);
            _playerManager.OnPlayerLeft += (player) => OnPlayerLeft?.Invoke(player);

            _matchmakingService.OnMatchmakingStarted += () => OnMatchmakingStarted?.Invoke();
            _matchmakingService.OnMatchmakingCancelled += () => OnMatchmakingCancelled?.Invoke();
            _matchmakingService.OnPlayersFoundUpdated += (count) => OnPlayersFoundUpdated?.Invoke(count);
            _matchmakingService.OnMatchFound += () => OnMatchFound?.Invoke();
        }

        /// <summary>
        /// Create a new lobby - delegates to LobbyStateManager
        /// </summary>
        public void CreateLobby(string lobbyName, int maxPlayers = 4, bool isPrivate = false)
        {
            _lobbyManager.CreateLobby(lobbyName, maxPlayers, isPrivate);
        }

        /// <summary>
        /// Join an existing lobby - delegates to LobbyStateManager
        /// </summary>
        public void JoinLobby(string lobbyId, bool presenceEnabled = true)
        {
            _lobbyManager.JoinLobby(lobbyId, presenceEnabled);
        }

        /// <summary>
        /// Leave the current lobby - delegates to LobbyStateManager
        /// </summary>
        public void LeaveLobby()
        {
            _lobbyManager.LeaveLobby();
        }

        /// <summary>
        /// Set the player's ready state - delegates to PlayerManager
        /// </summary>
        public void SetPlayerReady(bool isReady)
        {
            _playerManager.SetPlayerReady(isReady);
        }

        /// <summary>
        /// Set the player's team - delegates to PlayerManager
        /// </summary>
        public void SetPlayerTeam(TeamId teamId)
        {
            _playerManager.SetPlayerTeam(teamId);
        }

        /// <summary>
        /// Set the player's character class - delegates to PlayerManager
        /// </summary>
        public void SetPlayerCharacterClass(CharacterClass characterClass)
        {
            _playerManager.SetPlayerCharacterClass(characterClass);
        }

        /// <summary>
        /// Set the game mode - delegates to LobbyStateManager
        /// </summary>
        public void SetGameMode(GameMode gameMode)
        {
            _lobbyManager.SetGameMode(gameMode);
        }

        /// <summary>
        /// Set the map name - delegates to LobbyStateManager
        /// </summary>
        public void SetMapName(string mapName)
        {
            _lobbyManager.SetMapName(mapName);
        }

        /// <summary>
        /// Check if all players are ready - delegates to LobbyStateManager
        /// </summary>
        public bool AreAllPlayersReady()
        {
            return _lobbyManager.AreAllPlayersReady();
        }

        #region Matchmaking Methods - Delegated to MatchmakingService

        /// <summary>
        /// Start matchmaking - delegates to MatchmakingService
        /// </summary>
        public void StartMatchmaking(GameMode gameMode, int minPlayers = 2, int maxPlayers = 4)
        {
            _matchmakingService.StartMatchmaking(gameMode, minPlayers, maxPlayers);
        }

        /// <summary>
        /// Cancel matchmaking - delegates to MatchmakingService
        /// </summary>
        public void CancelMatchmaking()
        {
            _matchmakingService.CancelMatchmaking();
        }

        /// <summary>
        /// Search for available lobbies - delegates to MatchmakingService
        /// </summary>
        public void SearchForLobbies(GameMode gameMode)
        {
            _matchmakingService.SearchForLobbies(gameMode);
        }
        public void CheckForMatchReady(int currentPlayerCount, bool allPlayersReady)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Invite a friend - delegates to PlayerManager
        /// </summary>
        public void InviteFriend(ProductUserId friendId)
        {
            _playerManager.InviteFriend(friendId);
        }

        /// <summary>
        /// Kick a player - delegates to PlayerManager
        /// </summary>
        public void KickPlayer(ProductUserId playerId)
        {
            _playerManager.KickPlayer(playerId);
        }

        #endregion

        #region ITeamManager Implementation

        /// <summary>
        /// Update teams - delegates to TeamManager
        /// </summary>
        public void UpdateTeams()
        {
            _teamManager.UpdateTeams();
        }

        /// <summary>
        /// Get player info - delegates to TeamManager
        /// </summary>
        public PlayerInfo GetPlayerInfo(string playerId)
        {
            return _teamManager.GetPlayerInfo(playerId);
        }

        #endregion

        private void Update()
        {
            // Check for match ready during matchmaking
            if (_matchmakingService != null && _matchmakingService.IsSearchingForMatch)
            {
                _matchmakingService.CheckForMatchReady(CurrentPlayerCount, AreAllPlayersReady());
            }
        }
    }
}
