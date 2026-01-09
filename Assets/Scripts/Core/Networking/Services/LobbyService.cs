using System;
using Core.Logging;
using Core.Networking.Common;
using Core.Networking.Interfaces;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using PlayEveryWare.EpicOnlineServices;
using PlayEveryWare.EpicOnlineServices.Samples;

namespace Core.Networking.Services
{
    /// <summary>
    /// Service for managing lobbies (Party and Match)
    /// Implements PUBG-style lobby system with persistent party lobbies
    /// </summary>
    public class LobbyService : ILobbyManager
    {
        #region Events

        public event Action<Result, LobbyInfo> OnPartyCreated;
        public event Action<PlayerInfo> OnPartyMemberJoined;
        public event Action<PlayerInfo> OnPartyMemberLeft;
        public event Action OnPartyUpdated;

        public event Action<Result, LobbyInfo> OnMatchLobbyCreated;
        public event Action<Result, LobbyInfo> OnMatchLobbyJoined;
        public event Action OnMatchLobbyLeft;
        public event Action OnMatchLobbyUpdated;

        public event Action<LobbyState> OnLobbyStateChanged;
        public event Action<string> OnError;

        #endregion

        #region Properties

        public LobbyInfo CurrentPartyLobby { get; private set; }
        public LobbyInfo CurrentMatchLobby { get; private set; }
        public LobbyState CurrentState { get; private set; } = LobbyState.Idle;
        public bool IsInParty => CurrentPartyLobby != null;
        public bool IsInMatchLobby => CurrentMatchLobby != null;
        public bool IsPartyLeader => CurrentPartyLobby?.IsPartyLeader(EOSManager.Instance.GetProductUserId()) ?? false;
        public bool IsMatchLobbyOwner => CurrentMatchLobby?.IsOwner(EOSManager.Instance.GetProductUserId()) ?? false;

        #endregion

        #region Private Fields

        private EOSLobbyManager _eosLobbyManager;
        private ITeamManager _teamManager;
        private bool _isInitialized;

        // Notification handles
        private ulong _lobbyUpdateNotification;
        private ulong _lobbyMemberUpdateNotification;
        private ulong _lobbyInviteAcceptedNotification;

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor
        /// </summary>
        public LobbyService(EOSLobbyManager eosLobbyManager, ITeamManager teamManager)
        {
            _eosLobbyManager = eosLobbyManager ?? throw new ArgumentNullException(nameof(eosLobbyManager));
            _teamManager = teamManager ?? throw new ArgumentNullException(nameof(teamManager));
        }

        /// <summary>
        /// Initialize the lobby service
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
            {
                GameLogger.LogWarning("Already initialized");
                return;
            }

            // Subscribe to EOS lobby notifications
            SubscribeToLobbyNotifications();

            _isInitialized = true;
            GameLogger.Log("Initialized");
        }

        #endregion

        #region Party Lobby Methods

        /// <summary>
        /// Create a new party lobby
        /// </summary>
        public void CreatePartyLobby(LobbyConfig config)
        {
            if (!_isInitialized)
            {
                OnError?.Invoke("LobbyService not initialized");
                return;
            }

            if (IsInParty)
            {
                OnError?.Invoke("Already in a party. Leave current party first.");
                return;
            }

            // Set defaults for party lobby
            config.Type = LobbyType.Party;
            config.IsPrivate = true; // Party lobbies are always private
            config.AllowInvites = true;

            // Add party-specific attributes
            config.CustomAttributes["Type"] = "Party";
            config.CustomAttributes["PartyLeader"] = EOSManager.Instance.GetProductUserId().ToString();
            config.CustomAttributes["IsSearching"] = "false";

            GameLogger.Log($"Creating party lobby: {config.LobbyName}");

            // Create lobby via EOS
            CreateLobbyInternal(config, LobbyType.Party);
        }

        /// <summary>
        /// Invite a friend to the party
        /// </summary>
        public void InviteToParty(ProductUserId friendId)
        {
            if (!IsInParty)
            {
                OnError?.Invoke("Not in a party");
                return;
            }

            if (!IsPartyLeader)
            {
                OnError?.Invoke("Only party leader can invite");
                return;
            }

            GameLogger.Log($"Inviting friend to party: {friendId}");

            // Send invite via EOS
            _eosLobbyManager.SendInvite(friendId);
        }

        /// <summary>
        /// Leave the current party
        /// </summary>
        public void LeaveParty()
        {
            if (!IsInParty)
            {
                GameLogger.LogWarning("Not in a party");
                return;
            }

            GameLogger.Log("Leaving party");

            string lobbyId = CurrentPartyLobby.LobbyId;
            CurrentPartyLobby = null;

            // Leave via EOS
            _eosLobbyManager.LeaveLobby(null);

            ChangeState(LobbyState.Idle);
        }

        /// <summary>
        /// Update party lobby settings
        /// </summary>
        public void UpdatePartySettings(LobbyConfig config)
        {
            if (!IsInParty)
            {
                OnError?.Invoke("Not in a party");
                return;
            }

            if (!IsPartyLeader)
            {
                OnError?.Invoke("Only party leader can update settings");
                return;
            }

            GameLogger.Log("Updating party settings");

            // Update lobby attributes via EOS
            UpdateLobbyAttributes(CurrentPartyLobby.LobbyId, config);
        }

        #endregion

        #region Match Lobby Methods

        /// <summary>
        /// Create a new match lobby
        /// </summary>
        public void CreateMatchLobby(LobbyConfig config)
        {
            if (!_isInitialized)
            {
                OnError?.Invoke("LobbyService not initialized");
                return;
            }

            // Set defaults for match lobby
            config.Type = LobbyType.Match;
            config.IsPrivate = false; // Match lobbies are public for matchmaking

            // Add match-specific attributes
            config.CustomAttributes["Type"] = "Match";
            config.CustomAttributes["Status"] = "Filling";
            config.CustomAttributes["TeamA_Count"] = "0";
            config.CustomAttributes["TeamB_Count"] = "0";

            GameLogger.Log($"Creating match lobby: {config.LobbyName}");

            // Create lobby via EOS
            CreateLobbyInternal(config, LobbyType.Match);
        }

        /// <summary>
        /// Join an existing match lobby
        /// </summary>
        public void JoinMatchLobby(string lobbyId)
        {
            if (!_isInitialized)
            {
                OnError?.Invoke("LobbyService not initialized");
                return;
            }

            if (IsInMatchLobby)
            {
                OnError?.Invoke("Already in a match lobby");
                return;
            }

            GameLogger.Log($"Joining match lobby: {lobbyId}");

            // Join via EOS
            JoinLobbyInternal(lobbyId, LobbyType.Match);
        }

        /// <summary>
        /// Leave the current match lobby
        /// </summary>
        public void LeaveMatchLobby()
        {
            if (!IsInMatchLobby)
            {
                GameLogger.LogWarning("Not in a match lobby");
                return;
            }

            GameLogger.Log("Leaving match lobby");

            string lobbyId = CurrentMatchLobby.LobbyId;
            bool wasOwner = IsMatchLobbyOwner;
            bool wasLastPlayer = CurrentMatchLobby.CurrentPlayers <= 1;

            CurrentMatchLobby = null;

            // Leave via EOS
            // Note: EOS automatically destroys the lobby if the last player leaves
            // or if the owner leaves (depending on lobby settings)
            _eosLobbyManager.LeaveLobby(null);

            if (wasOwner || wasLastPlayer)
            {
                GameLogger.Log($"Left match lobby as {(wasOwner ? "owner" : "last player")} - lobby will be destroyed by EOS");
            }

            OnMatchLobbyLeft?.Invoke();

            // Return to party state if in party, otherwise idle
            ChangeState(IsInParty ? LobbyState.InParty : LobbyState.Idle);
        }

        /// <summary>
        /// Destroy the current match lobby (owner only)
        /// Explicitly destroys the lobby instead of just leaving
        /// </summary>
        public void DestroyMatchLobby()
        {
            if (!IsInMatchLobby)
            {
                GameLogger.LogWarning("Not in a match lobby");
                return;
            }

            if (!IsMatchLobbyOwner)
            {
                GameLogger.LogWarning("Only the lobby owner can destroy the lobby. Use LeaveMatchLobby() instead.");
                LeaveMatchLobby();
                return;
            }

            GameLogger.Log("Destroying match lobby (owner)");

            string lobbyId = CurrentMatchLobby.LobbyId;

            // Destroy the lobby via EOS
            var destroyOptions = new DestroyLobbyOptions
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                LobbyId = lobbyId
            };

            EOSManager.Instance.GetEOSLobbyInterface().DestroyLobby(ref destroyOptions, null, (ref DestroyLobbyCallbackInfo data) =>
            {
                if (data.ResultCode == Result.Success)
                {
                    GameLogger.Log($"Match lobby destroyed successfully: {lobbyId}");
                }
                else
                {
                    GameLogger.LogError($"Failed to destroy match lobby: {data.ResultCode}");
                }
            });

            CurrentMatchLobby = null;
            OnMatchLobbyLeft?.Invoke();

            // Return to party state if in party, otherwise idle
            ChangeState(IsInParty ? LobbyState.InParty : LobbyState.Idle);
        }

        #endregion

        #region Game Settings

        /// <summary>
        /// Set the game mode
        /// </summary>
        public void SetGameMode(GameMode gameMode)
        {
            if (!IsPartyLeader)
            {
                OnError?.Invoke("Only party leader can change game mode");
                return;
            }

            if (CurrentPartyLobby != null)
            {
                CurrentPartyLobby.GameMode = gameMode;
                UpdateLobbyAttribute(CurrentPartyLobby.LobbyId, "GameMode", gameMode.ToString());
                OnPartyUpdated?.Invoke();
            }
        }

        /// <summary>
        /// Set the map name
        /// </summary>
        public void SetMapName(string mapName)
        {
            if (!IsPartyLeader)
            {
                OnError?.Invoke("Only party leader can change map");
                return;
            }

            if (CurrentPartyLobby != null)
            {
                CurrentPartyLobby.MapName = mapName;
                UpdateLobbyAttribute(CurrentPartyLobby.LobbyId, "MapName", mapName);
                OnPartyUpdated?.Invoke();
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Check if all players are ready
        /// </summary>
        public bool AreAllPlayersReady()
        {
            var lobby = CurrentMatchLobby ?? CurrentPartyLobby;
            if (lobby == null || lobby.Players.Count == 0)
                return false;

            foreach (var player in lobby.Players)
            {
                if (!player.IsReady)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Get lobby info by ID
        /// </summary>
        public LobbyInfo GetLobbyInfo(string lobbyId)
        {
            if (CurrentPartyLobby?.LobbyId == lobbyId)
                return CurrentPartyLobby;

            if (CurrentMatchLobby?.LobbyId == lobbyId)
                return CurrentMatchLobby;

            return null;
        }

        #endregion

        #region Private Helper Methods - EOS Integration

        /// <summary>
        /// Create a lobby internally using EOS
        /// </summary>
        private void CreateLobbyInternal(LobbyConfig config, LobbyType type)
        {
            var localUserId = EOSManager.Instance.GetProductUserId();
            if (!localUserId.IsValid())
            {
                OnError?.Invoke("Invalid user ID");
                return;
            }

            GameLogger.Log($"Creating {type} lobby: {config.LobbyName}");

            // Create EOS lobby options
            var createOptions = new CreateLobbyOptions
            {
                LocalUserId = localUserId,
                MaxLobbyMembers = (uint)config.MaxPlayers,
                PermissionLevel = config.IsPrivate ?
                    LobbyPermissionLevel.Inviteonly :
                    LobbyPermissionLevel.Publicadvertised,
                PresenceEnabled = config.PresenceEnabled,
                AllowInvites = config.AllowInvites,
                BucketId = config.GameMode.ToString(),
                EnableRTCRoom = config.RTCEnabled
            };

            // Create lobby via EOS
            var lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            lobbyInterface.CreateLobby(ref createOptions, null, (ref CreateLobbyCallbackInfo callbackData) =>
            {
                OnCreateLobbyCallback(callbackData, config, type);
            });
        }

        /// <summary>
        /// Callback when lobby is created
        /// </summary>
        private void OnCreateLobbyCallback(CreateLobbyCallbackInfo data, LobbyConfig config, LobbyType type)
        {
            if (data.ResultCode != Result.Success)
            {
                GameLogger.LogError($"Failed to create lobby: {data.ResultCode}");

                if (type == LobbyType.Party)
                {
                    OnPartyCreated?.Invoke(data.ResultCode, null);
                }
                else
                {
                    OnMatchLobbyCreated?.Invoke(data.ResultCode, null);
                }
                return;
            }

            GameLogger.Log($"Lobby created successfully: {data.LobbyId}");

            // Create lobby info
            var lobbyInfo = new LobbyInfo
            {
                LobbyId = data.LobbyId,
                Type = type,
                LobbyName = config.LobbyName,
                MaxPlayers = config.MaxPlayers,
                CurrentPlayers = 1,
                IsPrivate = config.IsPrivate,
                GameMode = config.GameMode,
                MapName = config.MapName,
                TeamSize = config.TeamSize,
                OwnerId = EOSManager.Instance.GetProductUserId(),
                PartyLeaderId = EOSManager.Instance.GetProductUserId(),
                Status = "Active"
            };

            // Set lobby attributes
            SetLobbyAttributes(data.LobbyId, config);

            // Store lobby
            if (type == LobbyType.Party)
            {
                CurrentPartyLobby = lobbyInfo;
                ChangeState(LobbyState.InParty);
                OnPartyCreated?.Invoke(Result.Success, lobbyInfo);
            }
            else
            {
                CurrentMatchLobby = lobbyInfo;
                ChangeState(LobbyState.InMatchLobby);
                OnMatchLobbyCreated?.Invoke(Result.Success, lobbyInfo);
            }

            // Refresh lobby details
            RefreshLobbyDetails(data.LobbyId);
        }

        /// <summary>
        /// Join a lobby internally using EOS
        /// </summary>
        private void JoinLobbyInternal(string lobbyId, LobbyType type)
        {
            var localUserId = EOSManager.Instance.GetProductUserId();
            if (!localUserId.IsValid())
            {
                OnError?.Invoke("Invalid user ID");
                return;
            }

            GameLogger.Log($"Joining {type} lobby: {lobbyId}");

            // Get lobby details first
            var copyOptions = new CopyLobbyDetailsHandleOptions
            {
                LobbyId = lobbyId,
                LocalUserId = localUserId
            };

            var lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            Result result = lobbyInterface.CopyLobbyDetailsHandle(ref copyOptions, out LobbyDetails lobbyDetails);

            if (result != Result.Success || lobbyDetails == null)
            {
                GameLogger.LogError($"Failed to get lobby details: {result}");

                if (type == LobbyType.Match)
                {
                    OnMatchLobbyJoined?.Invoke(result, null);
                }
                return;
            }

            // Join lobby
            var joinOptions = new JoinLobbyOptions
            {
                LobbyDetailsHandle = lobbyDetails,
                LocalUserId = localUserId,
                PresenceEnabled = true
            };

            lobbyInterface.JoinLobby(ref joinOptions, null, (ref JoinLobbyCallbackInfo callbackData) =>
            {
                OnJoinLobbyCallback(callbackData, type);
                lobbyDetails.Release();
            });
        }

        /// <summary>
        /// Callback when lobby is joined
        /// </summary>
        private void OnJoinLobbyCallback(JoinLobbyCallbackInfo data, LobbyType type)
        {
            if (data.ResultCode != Result.Success)
            {
                GameLogger.LogError($"Failed to join lobby: {data.ResultCode}");

                if (type == LobbyType.Match)
                {
                    OnMatchLobbyJoined?.Invoke(data.ResultCode, null);
                }
                return;
            }

            GameLogger.Log($"Joined lobby successfully: {data.LobbyId}");

            // Get lobby details
            RefreshLobbyDetails(data.LobbyId, (lobbyInfo) =>
            {
                if (type == LobbyType.Match)
                {
                    CurrentMatchLobby = lobbyInfo;
                    ChangeState(LobbyState.InMatchLobby);
                    OnMatchLobbyJoined?.Invoke(Result.Success, lobbyInfo);
                }
            });
        }

        /// <summary>
        /// Refresh lobby details from EOS
        /// </summary>
        private void RefreshLobbyDetails(string lobbyId, Action<LobbyInfo> callback = null)
        {
            var localUserId = EOSManager.Instance.GetProductUserId();
            var copyOptions = new CopyLobbyDetailsHandleOptions
            {
                LobbyId = lobbyId,
                LocalUserId = localUserId
            };

            var lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            Result result = lobbyInterface.CopyLobbyDetailsHandle(ref copyOptions, out LobbyDetails lobbyDetails);

            if (result != Result.Success || lobbyDetails == null)
            {
                GameLogger.LogError($"Failed to refresh lobby details: {result}");
                return;
            }

            // Copy lobby info
            var infoOptions = new LobbyDetailsCopyInfoOptions();
            LobbyDetailsInfo? lobbyDetailsInfo;
            result = lobbyDetails.CopyInfo(ref infoOptions, out lobbyDetailsInfo);

            if (result != Result.Success || !lobbyDetailsInfo.HasValue)
            {
                GameLogger.LogError($"Failed to copy lobby info: {result}");
                lobbyDetails.Release();
                return;
            }

            var info = lobbyDetailsInfo.Value;

            // Create lobby info
            var lobbyInfo = new LobbyInfo
            {
                LobbyId = info.LobbyId,
                MaxPlayers = (int)info.MaxMembers,
                CurrentPlayers = (int)(info.MaxMembers - info.AvailableSlots),
                OwnerId = GetLobbyOwner(lobbyDetails)
            };

            // Get attributes
            var attrCountOptions = new LobbyDetailsGetAttributeCountOptions();
            uint attrCount = lobbyDetails.GetAttributeCount(ref attrCountOptions);

            for (uint i = 0; i < attrCount; i++)
            {
                var attrOptions = new LobbyDetailsCopyAttributeByIndexOptions { AttrIndex = i };
                result = lobbyDetails.CopyAttributeByIndex(ref attrOptions, out Epic.OnlineServices.Lobby.Attribute? attribute);

                if (result == Result.Success && attribute.HasValue)
                {
                    var attrData = attribute.Value.Data;
                    string key = attrData.Value.Key;
                    string value = attrData.Value.Value.AsUtf8;

                    lobbyInfo.Attributes[key] = value;

                    // Parse known attributes
                    switch (key)
                    {
                        case "Type":
                            lobbyInfo.Type = Enum.TryParse<LobbyType>(value, out var lobbyType) ? lobbyType : LobbyType.Party;
                            break;
                        case "GameMode":
                            lobbyInfo.GameMode = Enum.TryParse<GameMode>(value, out var gameMode) ? gameMode : GameMode.Classic;
                            break;
                        case "MapName":
                            lobbyInfo.MapName = value;
                            break;
                        case "TeamSize":
                            lobbyInfo.TeamSize = int.TryParse(value, out var teamSize) ? teamSize : 4;
                            break;
                        case "Status":
                            lobbyInfo.Status = value;
                            break;
                    }
                }
            }

            lobbyDetails.Release();

            callback?.Invoke(lobbyInfo);
        }

        /// <summary>
        /// Set lobby attributes
        /// </summary>
        private void SetLobbyAttributes(string lobbyId, LobbyConfig config)
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
                GameLogger.LogError($"Failed to create lobby modification: {result}");
                return;
            }

            // Add custom attributes
            foreach (var kvp in config.CustomAttributes)
            {
                AddLobbyAttribute(modification, kvp.Key, kvp.Value);
            }

            // Add standard attributes
            AddLobbyAttribute(modification, "GameMode", config.GameMode.ToString());
            AddLobbyAttribute(modification, "MapName", config.MapName ?? "");
            AddLobbyAttribute(modification, "TeamSize", config.TeamSize.ToString());

            // Update lobby
            var updateOptions = new UpdateLobbyOptions
            {
                LobbyModificationHandle = modification
            };

            lobbyInterface.UpdateLobby(ref updateOptions, null, (ref UpdateLobbyCallbackInfo data) =>
            {
                if (data.ResultCode != Result.Success)
                {
                    GameLogger.LogError($"Failed to update lobby attributes: {data.ResultCode}");
                }

                modification.Release();
            });
        }

        /// <summary>
        /// Add a lobby attribute
        /// </summary>
        private void AddLobbyAttribute(LobbyModification modification, string key, string value)
        {
            var attributeData = new AttributeData
            {
                Key = key,
                Value = new AttributeDataValue { AsUtf8 = value }
            };

            var addOptions = new LobbyModificationAddAttributeOptions
            {
                Attribute = attributeData,
                Visibility = LobbyAttributeVisibility.Public
            };

            Result result = modification.AddAttribute(ref addOptions);

            if (result != Result.Success)
            {
                GameLogger.LogWarning($"Failed to add attribute {key}: {result}");
            }
        }

        /// <summary>
        /// Update lobby attributes
        /// </summary>
        private void UpdateLobbyAttributes(string lobbyId, LobbyConfig config)
        {
            SetLobbyAttributes(lobbyId, config);
        }

        /// <summary>
        /// Update a single lobby attribute
        /// </summary>
        private void UpdateLobbyAttribute(string lobbyId, string key, string value)
        {
            var config = new LobbyConfig();
            config.CustomAttributes[key] = value;
            SetLobbyAttributes(lobbyId, config);
        }

        /// <summary>
        /// Subscribe to EOS lobby notifications
        /// </summary>
        private void SubscribeToLobbyNotifications()
        {
            if (EOSManager.Instance == null) return;
            var lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            if (lobbyInterface == null) return;

            // Subscribe to lobby updates
            var updateOptions = new AddNotifyLobbyUpdateReceivedOptions();
            _lobbyUpdateNotification = lobbyInterface.AddNotifyLobbyUpdateReceived(ref updateOptions, null, OnLobbyUpdateReceived);

            // Subscribe to member updates
            var memberOptions = new AddNotifyLobbyMemberUpdateReceivedOptions();
            _lobbyMemberUpdateNotification = lobbyInterface.AddNotifyLobbyMemberUpdateReceived(ref memberOptions, null, OnLobbyMemberUpdateReceived);

            // Subscribe to invite accepted
            var inviteOptions = new AddNotifyLobbyInviteAcceptedOptions();
            _lobbyInviteAcceptedNotification = lobbyInterface.AddNotifyLobbyInviteAccepted(ref inviteOptions, null, OnLobbyInviteAccepted);

            GameLogger.Log("Subscribed to lobby notifications");
        }

        /// <summary>
        /// Callback when lobby is updated
        /// </summary>
        private void OnLobbyUpdateReceived(ref LobbyUpdateReceivedCallbackInfo data)
        {
            string lobbyId = data.LobbyId; // Copy to local variable to use in lambda
            GameLogger.Log($"Lobby update received: {lobbyId}");

            // Refresh lobby details
            RefreshLobbyDetails(lobbyId, (lobbyInfo) =>
            {
                if (CurrentPartyLobby?.LobbyId == lobbyId)
                {
                    CurrentPartyLobby = lobbyInfo;
                    OnPartyUpdated?.Invoke();
                }
                else if (CurrentMatchLobby?.LobbyId == lobbyId)
                {
                    CurrentMatchLobby = lobbyInfo;
                    OnMatchLobbyUpdated?.Invoke();
                }
            });
        }

        /// <summary>
        /// Callback when lobby member is updated
        /// </summary>
        private void OnLobbyMemberUpdateReceived(ref LobbyMemberUpdateReceivedCallbackInfo data)
        {
            GameLogger.Log($"Lobby member update: {data.LobbyId}, Member: {data.TargetUserId}");

            // Refresh lobby details
            RefreshLobbyDetails(data.LobbyId);
        }

        /// <summary>
        /// Callback when lobby invite is accepted
        /// </summary>
        private void OnLobbyInviteAccepted(ref LobbyInviteAcceptedCallbackInfo data)
        {
            GameLogger.Log($"Lobby invite accepted: {data.LobbyId}");

            // Join the lobby
            JoinLobbyInternal(data.LobbyId, LobbyType.Party);
        }

        /// <summary>
        /// Get lobby owner (helper to avoid ref in expression)
        /// </summary>
        private ProductUserId GetLobbyOwner(LobbyDetails lobbyDetails)
        {
            LobbyDetailsGetLobbyOwnerOptions options = new LobbyDetailsGetLobbyOwnerOptions();
            return lobbyDetails.GetLobbyOwner(ref options);
        }

        /// <summary>
        /// Change lobby state
        /// </summary>
        private void ChangeState(LobbyState newState)
        {
            if (CurrentState == newState)
                return;

            var oldState = CurrentState;
            CurrentState = newState;

            GameLogger.Log($"State changed: {oldState} -> {newState}");
            OnLobbyStateChanged?.Invoke(newState);
        }

        #endregion
    }
}
