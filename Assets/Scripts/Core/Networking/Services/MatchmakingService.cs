using System;
using System.Collections.Generic;
using Core.Networking.Common;
using Core.Networking.Interfaces;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using PlayEveryWare.EpicOnlineServices;
using PlayEveryWare.EpicOnlineServices.Samples;
using UnityEngine;

namespace Core.Networking.Services
{
    /// <summary>
    /// Service for matchmaking operations
    /// Handles finding matches for parties and solo players (PUBG-style)
    /// </summary>
    public class MatchmakingService : IMatchmakingService
    {
        #region Events
        
        public event Action OnMatchmakingStarted;
        public event Action OnMatchmakingCancelled;
        public event Action<string> OnMatchmakingFailed;
        public event Action<int, int> OnPlayersFound;
        public event Action<LobbyInfo> OnMatchFound;
        
        #endregion
        
        #region Properties
        
        public bool IsSearching { get; private set; }
        public int PlayersFound { get; private set; }
        public int RequiredPlayers { get; private set; }
        
        #endregion
        
        #region Private Fields
        
        private ILobbyManager _lobbyManager;
        private EOSLobbyManager _eosLobbyManager;
        private Bot.BotManager _botManager;
        private bool _isInitialized;
        
        // Matchmaking state
        private GameMode _currentGameMode;
        private int _currentTeamSize;
        private int _partySize;
        private float _searchStartTime;
        private bool _hasFilledWithBots;
        
        // Search handle
        private LobbySearch _currentSearch;
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Constructor
        /// </summary>
        public MatchmakingService(ILobbyManager lobbyManager, EOSLobbyManager eosLobbyManager)
        {
            _lobbyManager = lobbyManager ?? throw new ArgumentNullException(nameof(lobbyManager));
            _eosLobbyManager = eosLobbyManager ?? throw new ArgumentNullException(nameof(eosLobbyManager));
            _botManager = new Bot.BotManager();
        }
        
        /// <summary>
        /// Initialize the matchmaking service
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("[MatchmakingService] Already initialized");
                return;
            }
            
            _isInitialized = true;
            Debug.Log("[MatchmakingService] Initialized");
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Start matchmaking for the current party
        /// </summary>
        public void FindMatch(GameMode gameMode, int teamSize)
        {
            if (!_isInitialized)
            {
                OnMatchmakingFailed?.Invoke("MatchmakingService not initialized");
                return;
            }
            
            if (IsSearching)
            {
                Debug.LogWarning("[MatchmakingService] Already searching for a match");
                return;
            }
            
            _currentGameMode = gameMode;
            _currentTeamSize = teamSize;
            _partySize = _lobbyManager.CurrentPartyLobby?.CurrentPlayers ?? 1;
            RequiredPlayers = teamSize * 2; // e.g., 4v4 = 8 total players
            PlayersFound = _partySize;
            _hasFilledWithBots = false;
            
            // Clear any previous bots
            _botManager.ClearBots();
            
            Debug.Log($"[MatchmakingService] Starting matchmaking: Mode={gameMode}, TeamSize={teamSize}, PartySize={_partySize}");
            
            IsSearching = true;
            _searchStartTime = Time.time;
            
            OnMatchmakingStarted?.Invoke();
            
            // Start searching for existing match lobbies
            SearchForMatchLobbies(gameMode, teamSize, RequiredPlayers - _partySize);
        }
        
        /// <summary>
        /// Cancel active matchmaking
        /// </summary>
        public void CancelMatchmaking()
        {
            if (!IsSearching)
            {
                Debug.LogWarning("[MatchmakingService] Not currently searching");
                return;
            }
            
            Debug.Log("[MatchmakingService] Cancelling matchmaking");
            
            // Clean up search
            if (_currentSearch != null)
            {
                _currentSearch.Release();
                _currentSearch = null;
            }
            
            // If we're in a match lobby (created or joined during matchmaking), leave/destroy it
            if (_lobbyManager.IsInMatchLobby)
            {
                // If we're the owner and created this lobby for matchmaking, destroy it
                // Otherwise, just leave it
                if (_lobbyManager.IsMatchLobbyOwner)
                {
                    Debug.Log("[MatchmakingService] Destroying match lobby (owner) due to cancellation");
                    _lobbyManager.DestroyMatchLobby();
                }
                else
                {
                    Debug.Log("[MatchmakingService] Leaving match lobby due to cancellation");
                    _lobbyManager.LeaveMatchLobby();
                }
            }
            
            IsSearching = false;
            OnMatchmakingCancelled?.Invoke();
            
            // Reset state
            ResetMatchmakingState();
        }
        
        /// <summary>
        /// Search for available match lobbies
        /// </summary>
        public void SearchForMatchLobbies(GameMode gameMode, int teamSize, int neededPlayers)
        {
            Debug.Log($"[MatchmakingService] Searching for match lobbies: Mode={gameMode}, TeamSize={teamSize}, Needed={neededPlayers}");
            
            // Create search parameters
            var searchOptions = new CreateLobbySearchOptions
            {
                MaxResults = 10
            };
            
            Result result = EOSManager.Instance.GetEOSLobbyInterface().CreateLobbySearch(ref searchOptions, out _currentSearch);
            
            if (result != Result.Success || _currentSearch == null)
            {
                Debug.LogError($"[MatchmakingService] Failed to create lobby search: {result}");
                HandleMatchmakingFailure("Failed to create lobby search");
                return;
            }
            
            // Add search filters
            AddSearchFilter("Type", LobbyType.Match.ToString(), ComparisonOp.Equal);
            AddSearchFilter("GameMode", gameMode.ToString(), ComparisonOp.Equal);
            AddSearchFilter("TeamSize", teamSize.ToString(), ComparisonOp.Equal);
            AddSearchFilter("Status", "Filling", ComparisonOp.Equal);
            
            // Search for lobbies with available slots
            var findOptions = new LobbySearchFindOptions
            {
                LocalUserId = EOSManager.Instance.GetProductUserId()
            };
            
            _currentSearch.Find(ref findOptions, null, OnLobbySearchComplete);
        }
        
        /// <summary>
        /// Create a new match lobby and wait for players
        /// </summary>
        public void CreateAndWaitForPlayers(GameMode gameMode, int teamSize)
        {
            Debug.Log($"[MatchmakingService] Creating match lobby and waiting for players");
            
            // Create match lobby configuration
            var config = new LobbyConfig
            {
                Type = LobbyType.Match,
                LobbyName = $"Match_{gameMode}_{DateTime.UtcNow.Ticks}",
                MaxPlayers = teamSize * 2,
                IsPrivate = false,
                GameMode = gameMode,
                TeamSize = teamSize,
                AllowInvites = false,
                PresenceEnabled = false,
                CustomAttributes = new Dictionary<string, string>
                {
                    { "Type", "Match" },
                    { "GameMode", gameMode.ToString() },
                    { "TeamSize", teamSize.ToString() },
                    { "Status", "Filling" },
                    { "CreatedAt", DateTime.UtcNow.ToString("o") }
                }
            };
            
            // Subscribe to lobby created event
            _lobbyManager.OnMatchLobbyCreated += OnMatchLobbyCreatedForMatchmaking;
            
            // Create the match lobby
            _lobbyManager.CreateMatchLobby(config);
        }
        
        #endregion
        
        #region Private Methods - Search
        
        /// <summary>
        /// Add a search filter
        /// </summary>
        private void AddSearchFilter(string key, string value, ComparisonOp comparison)
        {
            var attributeData = new AttributeData
            {
                Key = key,
                Value = new AttributeDataValue { AsUtf8 = value }
            };
            
            var setParameterOptions = new LobbySearchSetParameterOptions
            {
                Parameter = attributeData,
                ComparisonOp = comparison
            };
            
            Result result = _currentSearch.SetParameter(ref setParameterOptions);
            
            if (result != Result.Success)
            {
                Debug.LogWarning($"[MatchmakingService] Failed to set search parameter {key}: {result}");
            }
        }
        
        /// <summary>
        /// Callback when lobby search completes
        /// </summary>
        private void OnLobbySearchComplete(ref LobbySearchFindCallbackInfo data)
        {
            if (data.ResultCode != Result.Success)
            {
                Debug.LogError($"[MatchmakingService] Lobby search failed: {data.ResultCode}");
                
                // No lobbies found, create our own
                CreateAndWaitForPlayers(_currentGameMode, _currentTeamSize);
                return;
            }
            
            // Get search results
            var getCountOptions = new LobbySearchGetSearchResultCountOptions();
            uint resultCount = _currentSearch.GetSearchResultCount(ref getCountOptions);
            
            Debug.Log($"[MatchmakingService] Found {resultCount} match lobbies");
            
            if (resultCount == 0)
            {
                // No lobbies found, create our own
                CreateAndWaitForPlayers(_currentGameMode, _currentTeamSize);
                return;
            }
            
            // Try to join the first suitable lobby
            TryJoinSearchResult(0);
        }
        
        /// <summary>
        /// Try to join a search result
        /// </summary>
        private void TryJoinSearchResult(uint index)
        {
            var copyOptions = new LobbySearchCopySearchResultByIndexOptions
            {
                LobbyIndex = index
            };
            
            Result result = _currentSearch.CopySearchResultByIndex(ref copyOptions, out LobbyDetails lobbyDetails);
            
            if (result != Result.Success || lobbyDetails == null)
            {
                Debug.LogError($"[MatchmakingService] Failed to get lobby details: {result}");
                
                // Try next result or create lobby
                var getCountOptions = new LobbySearchGetSearchResultCountOptions();
                uint resultCount = _currentSearch.GetSearchResultCount(ref getCountOptions);
                
                if (index + 1 < resultCount)
                {
                    TryJoinSearchResult(index + 1);
                }
                else
                {
                    CreateAndWaitForPlayers(_currentGameMode, _currentTeamSize);
                }
                return;
            }
            
            // Get lobby info
            var lobbyInfoOptions = new LobbyDetailsCopyInfoOptions();
            result = lobbyDetails.CopyInfo(ref lobbyInfoOptions, out LobbyDetailsInfo? lobbyInfo);
            
            if (result != Result.Success || !lobbyInfo.HasValue)
            {
                Debug.LogError($"[MatchmakingService] Failed to get lobby info: {result}");
                lobbyDetails.Release();
                return;
            }
            
            string lobbyId = lobbyInfo.Value.LobbyId;
            uint availableSlots = lobbyInfo.Value.AvailableSlots;
            
            Debug.Log($"[MatchmakingService] Found lobby {lobbyId} with {availableSlots} available slots");
            
            // Check if lobby has enough space for our party
            if (availableSlots >= _partySize)
            {
                // Join this lobby
                Debug.Log($"[MatchmakingService] Joining lobby with sufficient space");
                
                // Subscribe to join event
                _lobbyManager.OnMatchLobbyJoined += OnMatchLobbyJoinedForMatchmaking;
                
                // Join the lobby
                _lobbyManager.JoinMatchLobby(lobbyId);
            }
            else
            {
                // Not enough space, try next lobby
                var getCountOptions = new LobbySearchGetSearchResultCountOptions();
                uint resultCount = _currentSearch.GetSearchResultCount(ref getCountOptions);
                
                if (index + 1 < resultCount)
                {
                    TryJoinSearchResult(index + 1);
                }
                else
                {
                    // No suitable lobbies, create our own
                    CreateAndWaitForPlayers(_currentGameMode, _currentTeamSize);
                }
            }
            
            lobbyDetails.Release();
        }
        
        #endregion
        
        #region Private Methods - Callbacks
        
        /// <summary>
        /// Callback when match lobby is created during matchmaking
        /// </summary>
        private void OnMatchLobbyCreatedForMatchmaking(Result result, LobbyInfo lobbyInfo)
        {
            // Unsubscribe
            _lobbyManager.OnMatchLobbyCreated -= OnMatchLobbyCreatedForMatchmaking;
            
            if (result != Result.Success)
            {
                Debug.LogError($"[MatchmakingService] Failed to create match lobby: {result}");
                HandleMatchmakingFailure("Failed to create match lobby");
                return;
            }
            
            Debug.Log($"[MatchmakingService] Match lobby created: {lobbyInfo.LobbyId}");
            
            // Subscribe to lobby updates to track when it fills
            _lobbyManager.OnMatchLobbyUpdated += OnMatchLobbyUpdatedDuringMatchmaking;
            
            // Update players found
            PlayersFound = lobbyInfo.CurrentPlayers;
            OnPlayersFound?.Invoke(PlayersFound, RequiredPlayers);
        }
        
        /// <summary>
        /// Callback when match lobby is joined during matchmaking
        /// </summary>
        private void OnMatchLobbyJoinedForMatchmaking(Result result, LobbyInfo lobbyInfo)
        {
            // Unsubscribe
            _lobbyManager.OnMatchLobbyJoined -= OnMatchLobbyJoinedForMatchmaking;
            
            if (result != Result.Success)
            {
                Debug.LogError($"[MatchmakingService] Failed to join match lobby: {result}");
                
                // Try to find another lobby or create one
                SearchForMatchLobbies(_currentGameMode, _currentTeamSize, RequiredPlayers - _partySize);
                return;
            }
            
            Debug.Log($"[MatchmakingService] Joined match lobby: {lobbyInfo.LobbyId}");
            
            // Subscribe to lobby updates
            _lobbyManager.OnMatchLobbyUpdated += OnMatchLobbyUpdatedDuringMatchmaking;
            
            // Update players found
            PlayersFound = lobbyInfo.CurrentPlayers;
            OnPlayersFound?.Invoke(PlayersFound, RequiredPlayers);
            
            // Check if lobby is already full
            if (lobbyInfo.IsFull)
            {
                HandleMatchReady(lobbyInfo);
            }
        }
        
        /// <summary>
        /// Callback when match lobby is updated during matchmaking
        /// </summary>
        private void OnMatchLobbyUpdatedDuringMatchmaking()
        {
            var matchLobby = _lobbyManager.CurrentMatchLobby;
            if (matchLobby == null)
                return;
            
            // Update players found
            PlayersFound = matchLobby.CurrentPlayers;
            OnPlayersFound?.Invoke(PlayersFound, RequiredPlayers);
            
            Debug.Log($"[MatchmakingService] Match lobby updated: {PlayersFound}/{RequiredPlayers} players");
            
            // Check if lobby is full
            if (matchLobby.IsFull)
            {
                HandleMatchReady(matchLobby);
            }
        }
        
        /// <summary>
        /// Handle when match is ready (lobby full)
        /// </summary>
        private void HandleMatchReady(LobbyInfo lobbyInfo)
        {
            Debug.Log($"[MatchmakingService] Match ready! Lobby full: {lobbyInfo.LobbyId}");
            
            // Unsubscribe from updates
            _lobbyManager.OnMatchLobbyUpdated -= OnMatchLobbyUpdatedDuringMatchmaking;
            
            // Notify that match is found
            OnMatchFound?.Invoke(lobbyInfo);
            
            // Reset matchmaking state
            ResetMatchmakingState();
        }
        
        /// <summary>
        /// Handle matchmaking failure
        /// </summary>
        private void HandleMatchmakingFailure(string reason)
        {
            Debug.LogError($"[MatchmakingService] Matchmaking failed: {reason}");
            
            OnMatchmakingFailed?.Invoke(reason);
            ResetMatchmakingState();
        }
        
        #endregion
        
        #region Private Methods - Cleanup
        
        /// <summary>
        /// Reset matchmaking state
        /// </summary>
        private void ResetMatchmakingState()
        {
            PlayersFound = 0;
            RequiredPlayers = 0;
            _searchStartTime = 0f;
            _hasFilledWithBots = false;
            IsSearching = false;
            
            if (_currentSearch != null)
            {
                _currentSearch.Release();
                _currentSearch = null;
            }
            
            // Unsubscribe from any events
            _lobbyManager.OnMatchLobbyCreated -= OnMatchLobbyCreatedForMatchmaking;
            _lobbyManager.OnMatchLobbyJoined -= OnMatchLobbyJoinedForMatchmaking;
            _lobbyManager.OnMatchLobbyUpdated -= OnMatchLobbyUpdatedDuringMatchmaking;
        }
        
        #endregion
        
        #region Bot Filling
        
        /// <summary>
        /// Fill remaining slots with bots and start the match
        /// Called by MatchmakingState when timeout occurs
        /// </summary>
        public void FillMatchWithBots()
        {
            if (_hasFilledWithBots)
                return;
            
            _hasFilledWithBots = true;
            
            var matchLobby = _lobbyManager.CurrentMatchLobby;
            if (matchLobby == null)
            {
                Debug.LogError("[MatchmakingService] Cannot fill with bots - no match lobby");
                HandleMatchmakingFailure("No match lobby to fill");
                return;
            }
            
            int currentPlayers = matchLobby.CurrentPlayers;
            int neededBots = RequiredPlayers - currentPlayers;
            
            if (neededBots <= 0)
            {
                Debug.Log("[MatchmakingService] Match already full, no bots needed");
                HandleMatchReady(matchLobby);
                return;
            }
            
            Debug.Log($"[MatchmakingService] Creating {neededBots} bots to fill match ({currentPlayers}/{RequiredPlayers} players)");
            
            // Create bots
            var bots = _botManager.CreateBots(neededBots, startingTeamId: 0);
            
            // Update player count
            PlayersFound = RequiredPlayers;
            OnPlayersFound?.Invoke(PlayersFound, RequiredPlayers);
            
            Debug.Log($"[MatchmakingService] Match filled with bots! Starting game with {currentPlayers} human players and {neededBots} bots");
            
            // Start the match
            HandleMatchReady(matchLobby);
        }
        
        /// <summary>
        /// Get active bots in the current match
        /// </summary>
        public List<Bot.BotPlayer> GetActiveBots()
        {
            return _botManager.GetActiveBots();
        }
        
        #endregion
    }
}
