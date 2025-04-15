using System;
using System.Collections.Generic;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using Epic.OnlineServices.Sessions;
using PlayEveryWare.EpicOnlineServices;
using PlayEveryWare.EpicOnlineServices.Samples;
using RecipeRage.Core.Networking.Common;
using UnityEngine;

namespace RecipeRage.Core.Networking.EOS
{
    /// <summary>
    /// Wrapper for EOSLobbyManager that provides game-specific lobby functionality.
    /// </summary>
    public class RecipeRageLobbyManager : MonoBehaviour
    {
        // Reference to the EOS Lobby Manager
        private EOSLobbyManager _eosLobbyManager;

        // Current lobby information
        private Lobby _currentLobby;

        // Cached player information
        private List<PlayerInfo> _teamA = new List<PlayerInfo>();
        private List<PlayerInfo> _teamB = new List<PlayerInfo>();
        private Dictionary<string, PlayerInfo> _playerInfoCache = new Dictionary<string, PlayerInfo>();

        // Game settings
        private GameMode _currentGameMode = GameMode.Classic;
        private string _currentMapName = "Kitchen";
        private bool _isPrivate = false;

        // Events
        public event Action<Result> OnLobbyCreated;
        public event Action<Result> OnLobbyJoined;
        public event Action<Result> OnLobbyLeft;
        public event Action OnLobbyUpdated;
        public event Action<PlayerInfo> OnPlayerJoined;
        public event Action<PlayerInfo> OnPlayerLeft;

        // Properties
        public List<PlayerInfo> TeamA => _teamA;
        public List<PlayerInfo> TeamB => _teamB;
        public GameMode CurrentGameMode => _currentGameMode;
        public string CurrentMapName => _currentMapName;
        public bool IsPrivate => _isPrivate;
        public bool IsLobbyOwner => _currentLobby?.IsOwner(EOSManager.Instance.GetProductUserId()) ?? false;

        /// <summary>
        /// Initialize the lobby manager.
        /// </summary>
        public void Initialize()
        {
            // Get the EOS Lobby Manager from the EOSManager
            _eosLobbyManager = EOSManager.Instance.GetOrCreateManager<EOSLobbyManager>();

            if (_eosLobbyManager == null)
            {
                Debug.LogError("[RecipeRageLobbyManager] EOSLobbyManager not found on EOSManager");
                return;
            }

            // Subscribe to lobby events
            _eosLobbyManager.LobbyChanged += OnLobbyChanged;

            Debug.Log("[RecipeRageLobbyManager] Initialized");
        }

        /// <summary>
        /// Create a new lobby.
        /// </summary>
        /// <param name="lobbyName">The name of the lobby</param>
        /// <param name="maxPlayers">The maximum number of players</param>
        /// <param name="isPrivate">Whether the lobby is private</param>
        public void CreateLobby(string lobbyName, int maxPlayers = 4, bool isPrivate = false)
        {
            // Create a new lobby
            Lobby lobby = new Lobby();
            lobby.MaxNumLobbyMembers = (uint)maxPlayers;
            lobby.LobbyPermissionLevel = isPrivate ?
                Epic.OnlineServices.Lobby.LobbyPermissionLevel.Inviteonly :
                Epic.OnlineServices.Lobby.LobbyPermissionLevel.Publicadvertised;
            lobby.AllowInvites = true;
            lobby.PresenceEnabled = true;
            lobby.RTCRoomEnabled = true;

            // Add game-specific attributes
            AddLobbyAttribute(lobby, "LobbyName", lobbyName, SessionAttributeAdvertisementType.Advertise);
            AddLobbyAttribute(lobby, "GameMode", _currentGameMode.ToString(), SessionAttributeAdvertisementType.Advertise);
            AddLobbyAttribute(lobby, "MapName", _currentMapName, SessionAttributeAdvertisementType.Advertise);

            // Create the lobby
            _eosLobbyManager.CreateLobby(lobby, OnLobbyCreationComplete);

            // Update local state
            _isPrivate = isPrivate;

            Debug.Log($"[RecipeRageLobbyManager] Creating lobby: {lobbyName}, MaxPlayers: {maxPlayers}, IsPrivate: {isPrivate}");
        }

        /// <summary>
        /// Join an existing lobby.
        /// </summary>
        /// <param name="lobbyId">The ID of the lobby to join</param>
        /// <param name="presenceEnabled">Whether to enable presence for this lobby (default: true)</param>
        public void JoinLobby(string lobbyId, bool presenceEnabled = true)
        {
            // Search for the lobby by ID
            _eosLobbyManager.SearchByLobbyId(lobbyId, (Result searchResult) =>
            {
                if (searchResult != Result.Success)
                {
                    Debug.LogError($"[RecipeRageLobbyManager] Failed to find lobby {lobbyId}: {searchResult}");
                    OnLobbyJoined?.Invoke(searchResult);
                    return;
                }

                // Get the search results
                var searchResults = _eosLobbyManager.GetSearchResults();

                foreach (var kvp in searchResults)
                {
                    if (kvp.Key.Id == lobbyId)
                    {
                        // Join the lobby
                        _eosLobbyManager.JoinLobby(lobbyId, kvp.Value, presenceEnabled, OnLobbyJoinComplete);

                        Debug.Log($"[RecipeRageLobbyManager] Joining lobby: {lobbyId}");
                        return;
                    }
                }

                Debug.LogError($"[RecipeRageLobbyManager] Lobby {lobbyId} found in search but not in results");
                OnLobbyJoined?.Invoke(Result.NotFound);
            });

            Debug.Log($"[RecipeRageLobbyManager] Searching for lobby: {lobbyId}");
        }

        /// <summary>
        /// Leave the current lobby.
        /// </summary>
        public void LeaveLobby()
        {
            if (_currentLobby != null)
            {
                // Leave the lobby
                _eosLobbyManager.LeaveLobby(OnLobbyLeftComplete);

                Debug.Log($"[RecipeRageLobbyManager] Leaving lobby: {_currentLobby.Id}");
            }
            else
            {
                Debug.LogError("[RecipeRageLobbyManager] No current lobby to leave");
                OnLobbyLeft?.Invoke(Result.NotFound);
            }
        }

        /// <summary>
        /// Set the player's ready state.
        /// </summary>
        /// <param name="isReady">Whether the player is ready</param>
        public void SetPlayerReady(bool isReady)
        {
            if (_currentLobby != null)
            {
                // Find the local player
                ProductUserId localUserId = EOSManager.Instance.GetProductUserId();

                foreach (LobbyMember member in _currentLobby.Members)
                {
                    if (member.ProductId == localUserId)
                    {
                        // Update the ready state
                        AddMemberAttribute(_currentLobby, "IsReady", isReady.ToString(), SessionAttributeAdvertisementType.Advertise);

                        Debug.Log($"[RecipeRageLobbyManager] Setting player ready: {isReady}");
                        return;
                    }
                }

                Debug.LogError("[RecipeRageLobbyManager] Local player not found in lobby");
            }
            else
            {
                Debug.LogError("[RecipeRageLobbyManager] No current lobby");
            }
        }

        /// <summary>
        /// Set the player's team.
        /// </summary>
        /// <param name="teamId">The team ID</param>
        public void SetPlayerTeam(TeamId teamId)
        {
            if (_currentLobby != null)
            {
                // Find the local player
                ProductUserId localUserId = EOSManager.Instance.GetProductUserId();

                foreach (LobbyMember member in _currentLobby.Members)
                {
                    if (member.ProductId == localUserId)
                    {
                        // Update the team
                        AddMemberAttribute(_currentLobby, "TeamId", ((int)teamId).ToString(), SessionAttributeAdvertisementType.Advertise);

                        Debug.Log($"[RecipeRageLobbyManager] Setting player team: {teamId}");
                        return;
                    }
                }

                Debug.LogError("[RecipeRageLobbyManager] Local player not found in lobby");
            }
            else
            {
                Debug.LogError("[RecipeRageLobbyManager] No current lobby");
            }
        }

        /// <summary>
        /// Set the player's character class.
        /// </summary>
        /// <param name="characterClass">The character class</param>
        public void SetPlayerCharacterClass(CharacterClass characterClass)
        {
            if (_currentLobby != null)
            {
                // Find the local player
                ProductUserId localUserId = EOSManager.Instance.GetProductUserId();

                foreach (LobbyMember member in _currentLobby.Members)
                {
                    if (member.ProductId == localUserId)
                    {
                        // Update the character class
                        AddMemberAttribute(_currentLobby, "CharacterClass", ((int)characterClass).ToString(), SessionAttributeAdvertisementType.Advertise);

                        Debug.Log($"[RecipeRageLobbyManager] Setting player character class: {characterClass}");
                        return;
                    }
                }

                Debug.LogError("[RecipeRageLobbyManager] Local player not found in lobby");
            }
            else
            {
                Debug.LogError("[RecipeRageLobbyManager] No current lobby");
            }
        }

        /// <summary>
        /// Set the game mode.
        /// </summary>
        /// <param name="gameMode">The game mode</param>
        public void SetGameMode(GameMode gameMode)
        {
            if (_currentLobby != null && IsLobbyOwner)
            {
                // Update the game mode
                AddLobbyAttribute(_currentLobby, "GameMode", gameMode.ToString(), SessionAttributeAdvertisementType.Advertise);

                // Update local state
                _currentGameMode = gameMode;

                Debug.Log($"[RecipeRageLobbyManager] Setting game mode: {gameMode}");
            }
            else
            {
                Debug.LogError("[RecipeRageLobbyManager] No current lobby or not lobby owner");
            }
        }

        /// <summary>
        /// Set the map name.
        /// </summary>
        /// <param name="mapName">The map name</param>
        public void SetMapName(string mapName)
        {
            if (_currentLobby != null && IsLobbyOwner)
            {
                // Update the map name
                AddLobbyAttribute(_currentLobby, "MapName", mapName, SessionAttributeAdvertisementType.Advertise);

                // Update local state
                _currentMapName = mapName;

                Debug.Log($"[RecipeRageLobbyManager] Setting map name: {mapName}");
            }
            else
            {
                Debug.LogError("[RecipeRageLobbyManager] No current lobby or not lobby owner");
            }
        }

        /// <summary>
        /// Check if all players are ready.
        /// </summary>
        /// <returns>Whether all players are ready</returns>
        public bool AreAllPlayersReady()
        {
            foreach (PlayerInfo player in _teamA)
            {
                if (!player.IsReady)
                {
                    return false;
                }
            }

            foreach (PlayerInfo player in _teamB)
            {
                if (!player.IsReady)
                {
                    return false;
                }
            }

            return _teamA.Count + _teamB.Count > 0;
        }

        /// <summary>
        /// Add a lobby attribute to a lobby.
        /// </summary>
        /// <param name="lobby">The lobby</param>
        /// <param name="key">The attribute key</param>
        /// <param name="value">The attribute value</param>
        /// <param name="visibility">The attribute visibility</param>
        private void AddLobbyAttribute(Lobby lobby, string key, string value, SessionAttributeAdvertisementType visibility)
        {
            // Create the attribute
            LobbyAttribute attribute = new LobbyAttribute
            {
                Key = key,
                ValueType = AttributeType.String,
                AsString = value,
                Visibility = LobbyAttributeVisibility.Public
            };

            // Add the attribute to the lobby
            if (_eosLobbyManager.GetCurrentLobby() != null)
            {
                // Create a new lobby with the updated attribute
                Lobby updatedLobby = new Lobby();
                // Add the attribute to the lobby
                updatedLobby.Attributes.Add(attribute);

                // Modify the lobby
                _eosLobbyManager.ModifyLobby(updatedLobby, null);
            }
        }

        /// <summary>
        /// Add a member attribute to a lobby.
        /// </summary>
        /// <param name="lobby">The lobby</param>
        /// <param name="key">The attribute key</param>
        /// <param name="value">The attribute value</param>
        /// <param name="visibility">The attribute visibility</param>
        private void AddMemberAttribute(Lobby lobby, string key, string value, SessionAttributeAdvertisementType visibility)
        {
            // Create the attribute
            LobbyAttribute attribute = new LobbyAttribute
            {
                Key = key,
                ValueType = AttributeType.String,
                AsString = value,
                Visibility = LobbyAttributeVisibility.Public
            };

            // Add the attribute to the member
            _eosLobbyManager.SetMemberAttribute(attribute);
        }

        /// <summary>
        /// Update the teams based on the current lobby.
        /// </summary>
        private void UpdateTeams()
        {
            // Clear the teams
            _teamA.Clear();
            _teamB.Clear();

            if (_currentLobby == null)
            {
                return;
            }

            // Process each member
            foreach (LobbyMember member in _currentLobby.Members)
            {
                // Create or get the player info
                PlayerInfo playerInfo;
                string playerId = member.ProductId.ToString();

                if (_playerInfoCache.TryGetValue(playerId, out playerInfo))
                {
                    // Update existing player info
                    playerInfo.DisplayName = member.DisplayName;
                    playerInfo.IsHost = _currentLobby.IsOwner(member.ProductId);
                    playerInfo.IsLocal = member.ProductId == EOSManager.Instance.GetProductUserId();
                    playerInfo.ProductUserId = member.ProductId;
                }
                else
                {
                    // Create new player info
                    playerInfo = new PlayerInfo
                    {
                        PlayerId = playerId,
                        DisplayName = member.DisplayName,
                        IsHost = _currentLobby.IsOwner(member.ProductId),
                        IsLocal = member.ProductId == EOSManager.Instance.GetProductUserId(),
                        ProductUserId = member.ProductId
                    };

                    _playerInfoCache[playerId] = playerInfo;
                }

                // Extract member attributes
                foreach (var kvp in member.MemberAttributes)
                {
                    LobbyAttribute attribute = kvp.Value;

                    switch (attribute.Key)
                    {
                        case "IsReady":
                            bool.TryParse(attribute.AsString, out bool isReady);
                            playerInfo.IsReady = isReady;
                            break;
                        case "TeamId":
                            int.TryParse(attribute.AsString, out int teamId);
                            playerInfo.Team = (TeamId)teamId;
                            break;
                        case "CharacterClass":
                            int.TryParse(attribute.AsString, out int characterClass);
                            playerInfo.CharacterClass = (CharacterClass)characterClass;
                            break;
                    }
                }

                // Add to the appropriate team
                if (playerInfo.Team == TeamId.TeamA)
                {
                    _teamA.Add(playerInfo);
                }
                else
                {
                    _teamB.Add(playerInfo);
                }
            }

            // Extract lobby attributes
            foreach (LobbyAttribute attribute in _currentLobby.Attributes)
            {
                switch (attribute.Key)
                {
                    case "GameMode":
                        if (Enum.TryParse<GameMode>(attribute.AsString, out GameMode gameMode))
                        {
                            _currentGameMode = gameMode;
                        }
                        break;
                    case "MapName":
                        _currentMapName = attribute.AsString;
                        break;
                }
            }

            // Update private state
            _isPrivate = _currentLobby.LobbyPermissionLevel == Epic.OnlineServices.Lobby.LobbyPermissionLevel.Inviteonly;
        }

        /// <summary>
        /// Handle lobby changes.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The event args</param>
        private void OnLobbyChanged(object sender, EOSLobbyManager.LobbyChangeEventArgs e)
        {
            // Get the current lobby
            _currentLobby = _eosLobbyManager.GetCurrentLobby();

            // Update teams
            UpdateTeams();

            // Notify listeners
            OnLobbyUpdated?.Invoke();

            Debug.Log($"[RecipeRageLobbyManager] Lobby changed: {e.LobbyId}, Type: {e.LobbyChangeType}");
        }

        /// <summary>
        /// Callback for lobby creation.
        /// </summary>
        /// <param name="result">The result</param>
        private void OnLobbyCreationComplete(Result result)
        {
            if (result == Result.Success)
            {
                Debug.Log("[RecipeRageLobbyManager] Lobby created");

                // Get the current lobby
                _currentLobby = _eosLobbyManager.GetCurrentLobby();

                // Update teams
                UpdateTeams();
            }
            else
            {
                Debug.LogError($"[RecipeRageLobbyManager] Failed to create lobby: {result}");
            }

            OnLobbyCreated?.Invoke(result);
        }

        /// <summary>
        /// Callback for lobby join.
        /// </summary>
        /// <param name="result">The result</param>
        private void OnLobbyJoinComplete(Result result)
        {
            if (result == Result.Success)
            {
                Debug.Log("[RecipeRageLobbyManager] Lobby joined");

                // Get the current lobby
                _currentLobby = _eosLobbyManager.GetCurrentLobby();

                // Update teams
                UpdateTeams();
            }
            else
            {
                Debug.LogError($"[RecipeRageLobbyManager] Failed to join lobby: {result}");
            }

            OnLobbyJoined?.Invoke(result);
        }

        /// <summary>
        /// Callback for lobby leave.
        /// </summary>
        /// <param name="result">The result</param>
        private void OnLobbyLeftComplete(Result result)
        {
            if (result == Result.Success)
            {
                Debug.Log("[RecipeRageLobbyManager] Lobby left");

                // Clear the current lobby
                _currentLobby = null;

                // Clear teams
                _teamA.Clear();
                _teamB.Clear();
            }
            else
            {
                Debug.LogError($"[RecipeRageLobbyManager] Failed to leave lobby: {result}");
            }

            OnLobbyLeft?.Invoke(result);
        }
    }
}
