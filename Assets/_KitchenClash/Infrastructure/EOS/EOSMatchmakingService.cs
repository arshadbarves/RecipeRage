using System;
using System.Collections.Generic;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Network;
using KitchenClash.Infrastructure.Firebase;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using PlayEveryWare.EpicOnlineServices;
using PlayEveryWare.EpicOnlineServices.Samples;
using UnityEngine;

namespace KitchenClash.Infrastructure.EOS
{
    /// <summary>
    /// Service for matchmaking operations
    /// Handles finding matches for parties and solo players (PUBG-style)
    /// </summary>
    public class EOSMatchmakingService : IMatchmakingService
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

        public EOSMatchmakingService(ILobbyManager lobbyManager, EOSLobbyManager eosLobbyManager)
        {
            _lobbyManager = lobbyManager ?? throw new ArgumentNullException(nameof(lobbyManager));
            _eosLobbyManager = eosLobbyManager ?? throw new ArgumentNullException(nameof(eosLobbyManager));
            _botManager = new BotManager();
        }

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
            RequiredPlayers = teamSize * 2;
            PlayersFound = _partySize;
            _hasFilledWithBots = false;

            _botManager.ClearBots();

            GameLogger.Log($"Starting matchmaking: Mode={gameModeId}, TeamSize={teamSize}, PartySize={_partySize}");

            IsSearching = true;
            _searchStartTime = Time.time;

            OnMatchmakingStarted?.Invoke();

            SearchForMatchLobbies(gameModeId, teamSize, RequiredPlayers - _partySize);
        }

        public void CancelMatchmaking()
        {
            if (!IsSearching)
            {
                GameLogger.LogWarning("Not currently searching");
                return;
            }

            GameLogger.Log("Cancelling matchmaking");

            if (_currentSearch != null)
            {
                _currentSearch.Release();
                _currentSearch = null;
            }

            if (_lobbyManager.IsInMatchLobby)
            {
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

            ResetMatchmakingState();
        }

        public void SearchForMatchLobbies(string gameModeId, int teamSize, int neededPlayers)
        {
            GameLogger.Log($"Searching for match lobbies: Mode={gameModeId}, TeamSize={teamSize}, Needed={neededPlayers}");

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

            AddSearchFilter("Type", LobbyType.Match.ToString(), ComparisonOp.Equal);
            AddSearchFilter("GameMode", gameModeId, ComparisonOp.Equal);
            AddSearchFilter("TeamSize", teamSize.ToString(), ComparisonOp.Equal);
            AddSearchFilter("Status", "Filling", ComparisonOp.Equal);

            var findOptions = new LobbySearchFindOptions
            {
                LocalUserId = EOSManager.Instance.GetProductUserId()
            };

            _currentSearch.Find(ref findOptions, null, OnLobbySearchComplete);
        }

        public void CreateAndWaitForPlayers(string gameModeId, int teamSize)
        {
            GameLogger.Log("Creating match lobby and waiting for players");

            LobbyConfig config = BuildMatchLobbyConfig(gameModeId, teamSize);

            _lobbyManager.OnMatchLobbyCreated += OnMatchLobbyCreatedForMatchmaking;

            _lobbyManager.CreateMatchLobby(config);
        }

        private static LobbyConfig BuildMatchLobbyConfig(string gameModeId, int teamSize)
        {
            return new LobbyConfig
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
        }

        #endregion

        #region Private Methods - Search

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

        private void OnLobbySearchComplete(ref LobbySearchFindCallbackInfo data)
        {
            if (data.ResultCode != Result.Success)
            {
                GameLogger.LogError($"Lobby search failed: {data.ResultCode}");
                CreateAndWaitForPlayers(_currentGameModeId, _currentTeamSize);
                return;
            }

            var getCountOptions = new LobbySearchGetSearchResultCountOptions();
            uint resultCount = _currentSearch.GetSearchResultCount(ref getCountOptions);

            GameLogger.Log($"Found {resultCount} match lobbies");

            if (resultCount == 0)
            {
                CreateAndWaitForPlayers(_currentGameModeId, _currentTeamSize);
                return;
            }

            TryJoinSearchResult(0);
        }

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

            string lobbyStatus = GetLobbyAttribute(lobbyDetails, "Status");

            if (lobbyStatus != "Filling" && !string.IsNullOrEmpty(lobbyStatus))
            {
                GameLogger.LogWarning($"Skipping lobby {lobbyId} - Status is '{lobbyStatus}', not 'Filling'");

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

            if (availableSlots >= _partySize)
            {
                GameLogger.Log("Joining lobby with sufficient space");

                _lobbyManager.OnMatchLobbyJoined += OnMatchLobbyJoinedForMatchmaking;

                _lobbyManager.JoinMatchLobby(lobbyId);
            }
            else
            {
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
            }

            lobbyDetails.Release();
        }

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

        private void OnMatchLobbyCreatedForMatchmaking(Result result, LobbyInfo lobbyInfo)
        {
            _lobbyManager.OnMatchLobbyCreated -= OnMatchLobbyCreatedForMatchmaking;

            if (result != Result.Success)
            {
                GameLogger.LogError($"Failed to create match lobby: {result}");
                HandleMatchmakingFailure("Failed to create match lobby");
                return;
            }

            GameLogger.Log($"Match lobby created: {lobbyInfo.LobbyId}");

            _lobbyManager.OnMatchLobbyUpdated += OnMatchLobbyUpdatedDuringMatchmaking;

            PlayersFound = lobbyInfo.CurrentPlayers;
            OnPlayersFound?.Invoke(PlayersFound, RequiredPlayers);
        }

        private void OnMatchLobbyJoinedForMatchmaking(Result result, LobbyInfo lobbyInfo)
        {
            _lobbyManager.OnMatchLobbyJoined -= OnMatchLobbyJoinedForMatchmaking;

            if (result != Result.Success)
            {
                GameLogger.LogError($"Failed to join match lobby: {result}");
                SearchForMatchLobbies(_currentGameModeId, _currentTeamSize, RequiredPlayers - _partySize);
                return;
            }

            GameLogger.Log($"Joined match lobby: {lobbyInfo.LobbyId}");

            _lobbyManager.OnMatchLobbyUpdated += OnMatchLobbyUpdatedDuringMatchmaking;

            PlayersFound = lobbyInfo.CurrentPlayers;
            OnPlayersFound?.Invoke(PlayersFound, RequiredPlayers);

            if (lobbyInfo.IsFull)
            {
                HandleMatchReady(lobbyInfo);
            }
        }

        private void OnMatchLobbyUpdatedDuringMatchmaking()
        {
            var matchLobby = _lobbyManager.CurrentMatchLobby;
            if (matchLobby == null)
                return;

            PlayersFound = matchLobby.CurrentPlayers;
            OnPlayersFound?.Invoke(PlayersFound, RequiredPlayers);

            GameLogger.Log($"Match lobby updated: {PlayersFound}/{RequiredPlayers} players");

            if (matchLobby.IsFull)
            {
                HandleMatchReady(matchLobby);
            }
        }

        private void HandleMatchReady(LobbyInfo lobbyInfo, bool alreadyUpdatedStatus = false)
        {
            GameLogger.Log($"Match ready! Lobby: {lobbyInfo.LobbyId}");

            if (!alreadyUpdatedStatus && _lobbyManager.IsMatchLobbyOwner)
            {
                UpdateMatchLobbyStatus(lobbyInfo.LobbyId, "InProgress");
            }

            _lobbyManager.OnMatchLobbyUpdated -= OnMatchLobbyUpdatedDuringMatchmaking;

            OnMatchFound?.Invoke(lobbyInfo);

            ResetMatchmakingState();
        }

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

        private void HandleMatchmakingFailure(string reason)
        {
            GameLogger.LogError($"Matchmaking failed: {reason}");

            OnMatchmakingFailed?.Invoke(reason);
            ResetMatchmakingState();
        }

        #endregion

        #region Private Methods - Cleanup

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

            _lobbyManager.OnMatchLobbyCreated -= OnMatchLobbyCreatedForMatchmaking;
            _lobbyManager.OnMatchLobbyJoined -= OnMatchLobbyJoinedForMatchmaking;
            _lobbyManager.OnMatchLobbyUpdated -= OnMatchLobbyUpdatedDuringMatchmaking;
        }

        #endregion

        #region Bot Filling

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

            bool statusUpdated = false;
            if (_lobbyManager.IsMatchLobbyOwner)
            {
                GameLogger.Log("Updating lobby status to InProgress (bot-filled match)");
                UpdateMatchLobbyStatus(matchLobby.LobbyId, "InProgress");
                statusUpdated = true;
            }

            var bots = _botManager.CreateBots(neededBots, startingTeamId: 0);

            PlayersFound = RequiredPlayers;
            OnPlayersFound?.Invoke(PlayersFound, RequiredPlayers);

            GameLogger.Log($"Match filled with bots! Starting game with {currentPlayers} human players and {neededBots} bots");

            HandleMatchReady(matchLobby, alreadyUpdatedStatus: statusUpdated);
        }

        public List<BotPlayer> GetActiveBots()
        {
            return _botManager.GetActiveBots();
        }

        #endregion
    }
}
