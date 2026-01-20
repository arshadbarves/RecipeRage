using System;
using System.Collections.Generic;
using Core.Logging;
using Core.Networking.Common;
using Core.Networking.Interfaces;
using Core.Networking.Models;
using Core.RemoteConfig.Services;
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
        private BotManager _botManager;
        private bool _isInitialized;

        // Matchmaking state
        private string _currentGameModeId;
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
            _botManager = new BotManager();
        }

        /// <summary>
        /// Initialize the matchmaking service
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
            {
                GameLogger.LogWarning("Already initialized");
                return;
            }

            _isInitialized = true;
            GameLogger.Log("Initialized");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Start matchmaking for the current party
        /// </summary>
        public void FindMatch(string gameModeId, int teamSize)
        {
            if (!_isInitialized)
            {
                OnMatchmakingFailed?.Invoke("MatchmakingService not initialized");
                return;
            }

            if (IsSearching)
            {
                GameLogger.LogWarning("Already searching for a match");
                return;
            }

            _currentGameModeId = gameModeId;
            _currentTeamSize = teamSize;
            _partySize = _lobbyManager.CurrentPartyLobby?.CurrentPlayers ?? 1;
            RequiredPlayers = teamSize * 2; // e.g., 4v4 = 8 total players
            PlayersFound = _partySize;
            _hasFilledWithBots = false;

            // Clear any previous bots
            _botManager.ClearBots();

            GameLogger.Log($"Starting matchmaking: Mode={gameModeId}, TeamSize={teamSize}, PartySize={_partySize}");

            IsSearching = true;
            _searchStartTime = Time.time;

            OnMatchmakingStarted?.Invoke();

            // Start searching for existing match lobbies
            SearchForMatchLobbies(gameModeId, teamSize, RequiredPlayers - _partySize);
        }

        /// <summary>
        /// Cancel active matchmaking
        /// </summary>
        public void CancelMatchmaking()
        {
            if (!IsSearching)
            {
                GameLogger.LogWarning("Not currently searching");
                return;
            }

            GameLogger.Log("Cancelling matchmaking");

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
                    GameLogger.Log("Destroying match lobby (owner) due to cancellation");
                    _lobbyManager.DestroyMatchLobby();
                }
                else
                {
                    GameLogger.Log("Leaving match lobby due to cancellation");
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
        public void SearchForMatchLobbies(string gameModeId, int teamSize, int neededPlayers)
        {
            GameLogger.Log($"Searching for match lobbies: Mode={gameModeId}, TeamSize={teamSize}, Needed={neededPlayers}");

            // Create search parameters
            var searchOptions = new CreateLobbySearchOptions
            {
                MaxResults = 10
            };

            Result result = EOSManager.Instance.GetEOSLobbyInterface().CreateLobbySearch(ref searchOptions, out _currentSearch);

            if (result != Result.Success || _currentSearch == null)
            {
                GameLogger.LogError($"Failed to create lobby search: {result}");
                HandleMatchmakingFailure("Failed to create lobby search");
                return;
            }

            // Add search filters
            AddSearchFilter("Type", LobbyType.Match.ToString(), ComparisonOp.Equal);
            AddSearchFilter("GameMode", gameModeId, ComparisonOp.Equal);
            AddSearchFilter("TeamSize", teamSize.ToString(), ComparisonOp.Equal);
            AddSearchFilter("Status", "Filling", ComparisonOp.Equal);

            // Additional safety filters
            // Note: EOS will automatically filter out full lobbies via AvailableSlots

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
        public void CreateAndWaitForPlayers(string gameModeId, int teamSize)
        {
            GameLogger.Log("Creating match lobby and waiting for players");

            // Create match lobby configuration
            var config = new LobbyConfig
            {
                Type = LobbyType.Match,
                LobbyName = $"Match_{gameModeId}_{NTPTime.UtcNow.Ticks}",
                MaxPlayers = teamSize * 2,
                IsPrivate = false,
                GameModeId = gameModeId,
                TeamSize = teamSize,
                AllowInvites = false,
                PresenceEnabled = false,
                CustomAttributes = new Dictionary<string, string>
                {
                    { "Type", "Match" },
                    { "GameMode", gameModeId },
                    { "TeamSize", teamSize.ToString() },
                    { "Status", "Filling" },
                    { "CreatedAt", NTPTime.UtcNow.ToString("o") }
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
                GameLogger.LogWarning($"Failed to set search parameter {key}: {result}");
            }
        }

        /// <summary>
        /// Callback when lobby search completes
        /// </summary>
        private void OnLobbySearchComplete(ref LobbySearchFindCallbackInfo data)
        {
            if (data.ResultCode != Result.Success)
            {
                GameLogger.LogError($"Lobby search failed: {data.ResultCode}");

                // No lobbies found, create our own
                CreateAndWaitForPlayers(_currentGameModeId, _currentTeamSize);
                return;
            }

            // Get search results
            var getCountOptions = new LobbySearchGetSearchResultCountOptions();
            uint resultCount = _currentSearch.GetSearchResultCount(ref getCountOptions);

            GameLogger.Log($"Found {resultCount} match lobbies");

            if (resultCount == 0)
            {
                // No lobbies found, create our own
                CreateAndWaitForPlayers(_currentGameModeId, _currentTeamSize);
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
                GameLogger.LogError($"Failed to get lobby details: {result}");

                // Try next result or create lobby
                var getCountOptions = new LobbySearchGetSearchResultCountOptions();
                uint resultCount = _currentSearch.GetSearchResultCount(ref getCountOptions);

                if (index + 1 < resultCount)
                {
                    TryJoinSearchResult(index + 1);
                }
                else
                {
                    CreateAndWaitForPlayers(_currentGameModeId, _currentTeamSize);
                }
                return;
            }

            // Get lobby info
            var lobbyInfoOptions = new LobbyDetailsCopyInfoOptions();
            result = lobbyDetails.CopyInfo(ref lobbyInfoOptions, out LobbyDetailsInfo? lobbyInfo);

            if (result != Result.Success || !lobbyInfo.HasValue)
            {
                GameLogger.LogError($"Failed to get lobby info: {result}");
                lobbyDetails.Release();
                return;
            }

            string lobbyId = lobbyInfo.Value.LobbyId;
            uint availableSlots = lobbyInfo.Value.AvailableSlots;

            GameLogger.Log($"Found lobby {lobbyId} with {availableSlots} available slots");

            // Get lobby attributes to verify status
            string lobbyStatus = GetLobbyAttribute(lobbyDetails, "Status");

            // Safety check: Don't join if status is not "Filling"
            if (lobbyStatus != "Filling" && !string.IsNullOrEmpty(lobbyStatus))
            {
                GameLogger.LogWarning($"Skipping lobby {lobbyId} - Status is '{lobbyStatus}', not 'Filling'");

                // Try next lobby
                var getCountOptions = new LobbySearchGetSearchResultCountOptions();
                uint resultCount = _currentSearch.GetSearchResultCount(ref getCountOptions);

                if (index + 1 < resultCount)
                {
                    TryJoinSearchResult(index + 1);
                }
                else
                {
                    CreateAndWaitForPlayers(_currentGameModeId, _currentTeamSize);
                }
                return;
            }

            // Check if lobby has enough space for our party
            if (availableSlots >= _partySize)
            {
                // Join this lobby
                GameLogger.Log("Joining lobby with sufficient space");

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
                    CreateAndWaitForPlayers(_currentGameModeId, _currentTeamSize);
                }
            }

            lobbyDetails.Release();
        }

        /// <summary>
        /// Get a lobby attribute value
        /// </summary>
        private string GetLobbyAttribute(LobbyDetails lobbyDetails, string key)
        {
            var attrCountOptions = new LobbyDetailsGetAttributeCountOptions();
            uint attrCount = lobbyDetails.GetAttributeCount(ref attrCountOptions);

            for (uint i = 0; i < attrCount; i++)
            {
                var attrOptions = new LobbyDetailsCopyAttributeByIndexOptions { AttrIndex = i };
                Result result = lobbyDetails.CopyAttributeByIndex(ref attrOptions, out Epic.OnlineServices.Lobby.Attribute? attribute);

                if (result == Result.Success && attribute.HasValue)
                {
                    var attrData = attribute.Value.Data;
                    if (attrData.Value.Key == key)
                    {
                        return attrData.Value.Value.AsUtf8;
                    }
                }
            }

            return null;
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
                GameLogger.LogError($"Failed to create match lobby: {result}");
                HandleMatchmakingFailure("Failed to create match lobby");
                return;
            }

            GameLogger.Log($"Match lobby created: {lobbyInfo.LobbyId}");

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
                GameLogger.LogError($"Failed to join match lobby: {result}");

                // Try to find another lobby or create one
                SearchForMatchLobbies(_currentGameModeId, _currentTeamSize, RequiredPlayers - _partySize);
                return;
            }

            GameLogger.Log($"Joined match lobby: {lobbyInfo.LobbyId}");

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

            GameLogger.Log($"Match lobby updated: {PlayersFound}/{RequiredPlayers} players");

            // Check if lobby is full
            if (matchLobby.IsFull)
            {
                HandleMatchReady(matchLobby);
            }
        }

        /// <summary>
        /// Handle when match is ready (lobby full or filled with bots)
        /// </summary>
        private void HandleMatchReady(LobbyInfo lobbyInfo, bool alreadyUpdatedStatus = false)
        {
            GameLogger.Log($"Match ready! Lobby: {lobbyInfo.LobbyId}");

            // Update lobby status to prevent new players from joining (if not already done)
            if (!alreadyUpdatedStatus && _lobbyManager.IsMatchLobbyOwner)
            {
                UpdateMatchLobbyStatus(lobbyInfo.LobbyId, "InProgress");
            }

            // Unsubscribe from updates
            _lobbyManager.OnMatchLobbyUpdated -= OnMatchLobbyUpdatedDuringMatchmaking;

            // Notify that match is found
            OnMatchFound?.Invoke(lobbyInfo);

            // Reset matchmaking state
            ResetMatchmakingState();
        }

        /// <summary>
        /// Update match lobby status attribute
        /// </summary>
        private void UpdateMatchLobbyStatus(string lobbyId, string status)
        {
            var localUserId = EOSManager.Instance.GetProductUserId();
            var modOptions = new UpdateLobbyModificationOptions
            {
                LobbyId = lobbyId,
                LocalUserId = localUserId
            };

            var lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            Result result = lobbyInterface.UpdateLobbyModification(ref modOptions, out LobbyModification modification);

            if (result != Result.Success || modification == null)
            {
                GameLogger.LogError($"Failed to create lobby modification for status update: {result}");
                return;
            }

            // Update Status attribute
            var attributeData = new AttributeData
            {
                Key = "Status",
                Value = new AttributeDataValue { AsUtf8 = status }
            };

            var addOptions = new LobbyModificationAddAttributeOptions
            {
                Attribute = attributeData,
                Visibility = LobbyAttributeVisibility.Public
            };

            result = modification.AddAttribute(ref addOptions);

            if (result != Result.Success)
            {
                GameLogger.LogWarning($"Failed to add Status attribute: {result}");
                modification.Release();
                return;
            }

            // Apply the update
            var updateOptions = new UpdateLobbyOptions
            {
                LobbyModificationHandle = modification
            };

            lobbyInterface.UpdateLobby(ref updateOptions, null, (ref UpdateLobbyCallbackInfo data) =>
            {
                if (data.ResultCode == Result.Success)
                {
                    GameLogger.Log($"Match lobby status updated to: {status}");
                }
                else
                {
                    GameLogger.LogError($"Failed to update lobby status: {data.ResultCode}");
                }

                modification.Release();
            });
        }

        /// <summary>
        /// Handle matchmaking failure
        /// </summary>
        private void HandleMatchmakingFailure(string reason)
        {
            GameLogger.LogError($"Matchmaking failed: {reason}");

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
                GameLogger.LogError("Cannot fill with bots - no match lobby");
                HandleMatchmakingFailure("No match lobby to fill");
                return;
            }

            int currentPlayers = matchLobby.CurrentPlayers;
            int neededBots = RequiredPlayers - currentPlayers;

            if (neededBots <= 0)
            {
                GameLogger.Log("Match already full, no bots needed");
                HandleMatchReady(matchLobby);
                return;
            }

            GameLogger.Log($"Creating {neededBots} bots to fill match ({currentPlayers}/{RequiredPlayers} players)");

            // CRITICAL: Update lobby status IMMEDIATELY to prevent new players from joining
            // Bots don't take lobby slots in EOS, so we must mark the lobby as InProgress manually
            bool statusUpdated = false;
            if (_lobbyManager.IsMatchLobbyOwner)
            {
                GameLogger.Log("Updating lobby status to InProgress (bot-filled match)");
                UpdateMatchLobbyStatus(matchLobby.LobbyId, "InProgress");
                statusUpdated = true;
            }

            // Create bots
            var bots = _botManager.CreateBots(neededBots, startingTeamId: 0);

            // Update player count
            PlayersFound = RequiredPlayers;
            OnPlayersFound?.Invoke(PlayersFound, RequiredPlayers);

            GameLogger.Log($"Match filled with bots! Starting game with {currentPlayers} human players and {neededBots} bots");

            // Start the match (pass flag to avoid double status update)
            HandleMatchReady(matchLobby, alreadyUpdatedStatus: statusUpdated);
        }

        /// <summary>
        /// Get active bots in the current match
        /// </summary>
        public List<BotPlayer> GetActiveBots()
        {
            return _botManager.GetActiveBots();
        }

        #endregion
    }
}
