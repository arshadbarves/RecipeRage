using KitchenClash.Application;
using System;
using System.Collections.Generic;
using KitchenClash.Domain;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using PlayEveryWare.EpicOnlineServices;
using PlayEveryWare.EpicOnlineServices.Samples;

namespace KitchenClash.Infrastructure.EOS
{
    /// <summary>
    /// Service for managing lobbies (Party and Match)
    /// Implements PUBG-style lobby system with persistent party lobbies
    /// </summary>
    public class EOSLobbyService : ILobbyManager
    {
        #region Events

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
        public bool IsPartyLeader => CurrentPartyLobby?.IsPartyLeader(EOSManager.Instance.GetProductUserId()?.ToString()) ?? false;
        public bool IsMatchLobbyOwner => CurrentMatchLobby?.IsOwner(EOSManager.Instance.GetProductUserId()?.ToString()) ?? false;

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
        public EOSLobbyService(EOSLobbyManager eosLobbyManager, ITeamManager teamManager)
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

        public void Dispose()
        {
            UnsubscribeFromLobbyNotifications();
            if (_teamManager is EOSTeamManager concreteTeamManager)
            {
                concreteTeamManager.Clear();
            }
            CurrentPartyLobby = null;
            CurrentMatchLobby = null;
            _isInitialized = false;
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
        public void InviteToParty(string friendProductUserId)
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

            GameLogger.Log($"Inviting friend to party: {friendProductUserId}");

            // Send invite via EOS
            var friendId = ProductUserId.FromString(friendProductUserId);
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
        public void SetGameMode(string gameModeId)
        {
            if (!IsPartyLeader)
            {
                OnError?.Invoke("Only party leader can change game mode");
                return;
            }

            if (CurrentPartyLobby != null)
            {
                CurrentPartyLobby.GameModeId = gameModeId;
                UpdateLobbyAttribute(CurrentPartyLobby.LobbyId, "GameMode", gameModeId);
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
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Check if all players are ready
        /// </summary>
        public bool AreAllPlayersReady()
        {
            LobbyInfo lobby = CurrentMatchLobby ?? CurrentPartyLobby;
            if (lobby == null || lobby.Players.Count == 0)
            {
                return false;
            }

            foreach (PlayerInfo player in lobby.Players)
            {
                if (!player.IsReady)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Get lobby info by ID
        /// </summary>
        public LobbyInfo GetLobbyInfo(string lobbyId)
        {
            if (CurrentPartyLobby?.LobbyId == lobbyId)
            {
                return CurrentPartyLobby;
            }

            if (CurrentMatchLobby?.LobbyId == lobbyId)
            {
                return CurrentMatchLobby;
            }

            return null;
        }

        #endregion

        #region Private Helper Methods - EOS Integration

        private void CreateLobbyInternal(LobbyConfig config, LobbyType type)
        {
            ProductUserId localUserId = EOSManager.Instance.GetProductUserId();
            if (!localUserId.IsValid())
            {
                OnError?.Invoke("Invalid user ID");
                return;
            }

            GameLogger.Log($"Creating {type} lobby: {config.LobbyName}");

            var createOptions = new CreateLobbyOptions
            {
                LocalUserId = localUserId,
                MaxLobbyMembers = (uint)config.MaxPlayers,
                PermissionLevel = config.IsPrivate ?
                    LobbyPermissionLevel.Inviteonly :
                    LobbyPermissionLevel.Publicadvertised,
                PresenceEnabled = config.PresenceEnabled,
                AllowInvites = config.AllowInvites,
                BucketId = config.GameModeId,
                EnableRTCRoom = config.RTCEnabled
            };

            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            lobbyInterface.CreateLobby(ref createOptions, null, (ref CreateLobbyCallbackInfo callbackData) =>
            {
                OnCreateLobbyCallback(callbackData, config, type);
            });
        }

        private void OnCreateLobbyCallback(CreateLobbyCallbackInfo data, LobbyConfig config, LobbyType type)
        {
            if (data.ResultCode != Result.Success)
            {
                GameLogger.LogError($"Failed to create lobby: {data.ResultCode}");

                if (type == LobbyType.Match)
                {
                    OnMatchLobbyCreated?.Invoke(data.ResultCode, null);
                }
                return;
            }

            GameLogger.Log($"Lobby created successfully: {data.LobbyId}");

            var lobbyInfo = new LobbyInfo
            {
                LobbyId = data.LobbyId,
                Type = type,
                LobbyName = config.LobbyName,
                MaxPlayers = config.MaxPlayers,
                CurrentPlayers = 1,
                IsPrivate = config.IsPrivate,
                GameModeId = config.GameModeId,
                MapName = config.MapName,
                TeamSize = config.TeamSize,
                OwnerId = EOSManager.Instance.GetProductUserId()?.ToString(),
                PartyLeaderId = EOSManager.Instance.GetProductUserId()?.ToString(),
                Status = "Active"
            };

            SetLobbyAttributes(data.LobbyId, config);

            if (type == LobbyType.Party)
            {
                CurrentPartyLobby = lobbyInfo;
                ChangeState(LobbyState.InParty);
            }
            else
            {
                CurrentMatchLobby = lobbyInfo;
                ChangeState(LobbyState.InMatchLobby);
                OnMatchLobbyCreated?.Invoke(Result.Success, lobbyInfo);
            }

            RefreshLobbyDetails(data.LobbyId);
        }

        private void JoinLobbyInternal(string lobbyId, LobbyType type)
        {
            ProductUserId localUserId = EOSManager.Instance.GetProductUserId();
            if (!localUserId.IsValid())
            {
                OnError?.Invoke("Invalid user ID");
                return;
            }

            GameLogger.Log($"Joining {type} lobby: {lobbyId}");

            var copyOptions = new CopyLobbyDetailsHandleOptions
            {
                LobbyId = lobbyId,
                LocalUserId = localUserId
            };

            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
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

        private void RefreshLobbyDetails(string lobbyId, Action<LobbyInfo> callback = null)
        {
            ProductUserId localUserId = EOSManager.Instance.GetProductUserId();
            var copyOptions = new CopyLobbyDetailsHandleOptions
            {
                LobbyId = lobbyId,
                LocalUserId = localUserId
            };

            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            Result result = lobbyInterface.CopyLobbyDetailsHandle(ref copyOptions, out LobbyDetails lobbyDetails);

            if (result != Result.Success || lobbyDetails == null)
            {
                GameLogger.LogError($"Failed to refresh lobby details: {result}");
                return;
            }

            var infoOptions = new LobbyDetailsCopyInfoOptions();
            LobbyDetailsInfo? lobbyDetailsInfo;
            result = lobbyDetails.CopyInfo(ref infoOptions, out lobbyDetailsInfo);

            if (result != Result.Success || !lobbyDetailsInfo.HasValue)
            {
                GameLogger.LogError($"Failed to copy lobby info: {result}");
                lobbyDetails.Release();
                return;
            }

            LobbyDetailsInfo info = lobbyDetailsInfo.Value;

            var lobbyInfo = new LobbyInfo
            {
                LobbyId = info.LobbyId,
                MaxPlayers = (int)info.MaxMembers,
                CurrentPlayers = (int)(info.MaxMembers - info.AvailableSlots),
                OwnerId = GetLobbyOwner(lobbyDetails)?.ToString()
            };

            var attrCountOptions = new LobbyDetailsGetAttributeCountOptions();
            uint attrCount = lobbyDetails.GetAttributeCount(ref attrCountOptions);

            for (uint i = 0; i < attrCount; i++)
            {
                var attrOptions = new LobbyDetailsCopyAttributeByIndexOptions { AttrIndex = i };
                result = lobbyDetails.CopyAttributeByIndex(ref attrOptions, out Epic.OnlineServices.Lobby.Attribute? attribute);

                if (result == Result.Success && attribute.HasValue)
                {
                    AttributeData? attrData = attribute.Value.Data;
                    string key = attrData.Value.Key;
                    string value = attrData.Value.Value.AsUtf8;

                    lobbyInfo.Attributes[key] = value;

                    switch (key)
                    {
                        case "Type":
                            lobbyInfo.Type = Enum.TryParse<LobbyType>(value, out LobbyType lobbyType) ? lobbyType : LobbyType.Party;
                            break;
                        case "GameMode":
                            lobbyInfo.GameModeId = value;
                            break;
                        case "MapName":
                            lobbyInfo.MapName = value;
                            break;
                        case "TeamSize":
                            lobbyInfo.TeamSize = int.TryParse(value, out int teamSize) ? teamSize : 2;
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

        private void SetLobbyAttributes(string lobbyId, LobbyConfig config)
        {
            ProductUserId localUserId = EOSManager.Instance.GetProductUserId();
            var modOptions = new UpdateLobbyModificationOptions
            {
                LobbyId = lobbyId,
                LocalUserId = localUserId
            };

            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            Result result = lobbyInterface.UpdateLobbyModification(ref modOptions, out LobbyModification modification);

            if (result != Result.Success || modification == null)
            {
                GameLogger.LogError($"Failed to create lobby modification: {result}");
                return;
            }

            foreach (KeyValuePair<string, string> kvp in config.CustomAttributes)
            {
                AddLobbyAttribute(modification, kvp.Key, kvp.Value);
            }

            AddLobbyAttribute(modification, "GameMode", config.GameModeId);
            AddLobbyAttribute(modification, "MapName", config.MapName ?? "");
            AddLobbyAttribute(modification, "TeamSize", config.TeamSize.ToString());

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

        private void UpdateLobbyAttributes(string lobbyId, LobbyConfig config)
        {
            SetLobbyAttributes(lobbyId, config);
        }

        private void UpdateLobbyAttribute(string lobbyId, string key, string value)
        {
            UpdateLobbyAttributeValue(lobbyId, key, value);
        }

        private void UpdateLobbyAttributeValue(string lobbyId, string key, string value)
        {
            ProductUserId localUserId = EOSManager.Instance.GetProductUserId();
            var modOptions = new UpdateLobbyModificationOptions
            {
                LobbyId = lobbyId,
                LocalUserId = localUserId
            };

            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            Result result = lobbyInterface.UpdateLobbyModification(ref modOptions, out LobbyModification modification);

            if (result != Result.Success || modification == null)
            {
                GameLogger.LogError($"Failed to create lobby modification for attribute update: {result}");
                return;
            }

            AddLobbyAttribute(modification, key, value);

            var updateOptions = new UpdateLobbyOptions
            {
                LobbyModificationHandle = modification
            };

            lobbyInterface.UpdateLobby(ref updateOptions, null, (ref UpdateLobbyCallbackInfo data) =>
            {
                if (data.ResultCode != Result.Success)
                {
                    GameLogger.LogError($"Failed to update lobby attribute {key}: {data.ResultCode}");
                }

                modification.Release();
            });
        }

        private void SubscribeToLobbyNotifications()
        {
            if (EOSManager.Instance == null)
            {
                return;
            }

            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            if (lobbyInterface == null)
            {
                return;
            }

            var updateOptions = new AddNotifyLobbyUpdateReceivedOptions();
            _lobbyUpdateNotification = lobbyInterface.AddNotifyLobbyUpdateReceived(ref updateOptions, null, OnLobbyUpdateReceived);

            var memberOptions = new AddNotifyLobbyMemberUpdateReceivedOptions();
            _lobbyMemberUpdateNotification = lobbyInterface.AddNotifyLobbyMemberUpdateReceived(ref memberOptions, null, OnLobbyMemberUpdateReceived);

            var inviteOptions = new AddNotifyLobbyInviteAcceptedOptions();
            _lobbyInviteAcceptedNotification = lobbyInterface.AddNotifyLobbyInviteAccepted(ref inviteOptions, null, OnLobbyInviteAccepted);

            GameLogger.Log("Subscribed to lobby notifications");
        }

        private void UnsubscribeFromLobbyNotifications()
        {
            if (EOSManager.Instance == null)
            {
                return;
            }

            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
            if (lobbyInterface == null)
            {
                return;
            }

            if (_lobbyUpdateNotification != 0)
            {
                lobbyInterface.RemoveNotifyLobbyUpdateReceived(_lobbyUpdateNotification);
                _lobbyUpdateNotification = 0;
            }

            if (_lobbyMemberUpdateNotification != 0)
            {
                lobbyInterface.RemoveNotifyLobbyMemberUpdateReceived(_lobbyMemberUpdateNotification);
                _lobbyMemberUpdateNotification = 0;
            }

            if (_lobbyInviteAcceptedNotification != 0)
            {
                lobbyInterface.RemoveNotifyLobbyInviteAccepted(_lobbyInviteAcceptedNotification);
                _lobbyInviteAcceptedNotification = 0;
            }
        }

        private void OnLobbyUpdateReceived(ref LobbyUpdateReceivedCallbackInfo data)
        {
            string lobbyId = data.LobbyId;
            GameLogger.Log($"Lobby update received: {lobbyId}");

            RefreshLobbyDetails(lobbyId, (lobbyInfo) =>
            {
                if (CurrentPartyLobby?.LobbyId == lobbyId)
                {
                    CurrentPartyLobby = lobbyInfo;
                }
                else if (CurrentMatchLobby?.LobbyId == lobbyId)
                {
                    CurrentMatchLobby = lobbyInfo;
                    OnMatchLobbyUpdated?.Invoke();
                }
            });
        }

        private void OnLobbyMemberUpdateReceived(ref LobbyMemberUpdateReceivedCallbackInfo data)
        {
            GameLogger.Log($"Lobby member update: {data.LobbyId}, Member: {data.TargetUserId}");
            RefreshLobbyDetails(data.LobbyId);
        }

        private void OnLobbyInviteAccepted(ref LobbyInviteAcceptedCallbackInfo data)
        {
            GameLogger.Log($"Lobby invite accepted: {data.LobbyId}");
            JoinLobbyInternal(data.LobbyId, LobbyType.Party);
        }

        private ProductUserId GetLobbyOwner(LobbyDetails lobbyDetails)
        {
            LobbyDetailsGetLobbyOwnerOptions options = new LobbyDetailsGetLobbyOwnerOptions();
            return lobbyDetails.GetLobbyOwner(ref options);
        }

        private void ChangeState(LobbyState newState)
        {
            if (CurrentState == newState)
            {
                return;
            }

            LobbyState oldState = CurrentState;
            CurrentState = newState;

            GameLogger.Log($"State changed: {oldState} -> {newState}");
            OnLobbyStateChanged?.Invoke(newState);
        }

        #endregion
    }
}
