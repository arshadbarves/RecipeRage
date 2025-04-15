using System;
using System.Collections.Generic;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
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
        private readonly Dictionary<string, PlayerInfo> _playerInfoCache = new Dictionary<string, PlayerInfo>();

        // Game settings

        // Current lobby information
        private Lobby _currentLobby;
        // Reference to the EOS Lobby Manager
        private EOSLobbyManager _eosLobbyManager;

        // Cached player information

        // Properties
        public List<PlayerInfo> TeamA { get; } = new List<PlayerInfo>();
        public List<PlayerInfo> TeamB { get; } = new List<PlayerInfo>();
        public GameMode CurrentGameMode { get; private set; } = GameMode.Classic;
        public string CurrentMapName { get; private set; } = "Kitchen";
        public bool IsPrivate { get; private set; }

        public bool IsLobbyOwner => _currentLobby?.IsOwner(EOSManager.Instance.GetProductUserId()) ?? false;

        // Events
        public event Action<Result> OnLobbyCreated;
        public event Action<Result> OnLobbyJoined;
        public event Action<Result> OnLobbyLeft;
        public event Action OnLobbyUpdated;
        public event Action<PlayerInfo> OnPlayerJoined;
        public event Action<PlayerInfo> OnPlayerLeft;

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
        /// <param name="lobbyName"> The name of the lobby </param>
        /// <param name="maxPlayers"> The maximum number of players </param>
        /// <param name="isPrivate"> Whether the lobby is private </param>
        public void CreateLobby(string lobbyName, int maxPlayers = 4, bool isPrivate = false)
        {
            // Create a new lobby
            var lobby = new Lobby();
            lobby.MaxNumLobbyMembers = (uint)maxPlayers;
            lobby.LobbyPermissionLevel = isPrivate ?
                LobbyPermissionLevel.Inviteonly :
                LobbyPermissionLevel.Publicadvertised;
            lobby.AllowInvites = true;
            lobby.PresenceEnabled = true;
            lobby.RTCRoomEnabled = true;

            // Add game-specific attributes
            AddLobbyAttribute(lobby, "LobbyName", lobbyName, LobbyAttributeVisibility.Public);
            AddLobbyAttribute(lobby, "GameMode", CurrentGameMode.ToString(), LobbyAttributeVisibility.Public);
            AddLobbyAttribute(lobby, "MapName", CurrentMapName, LobbyAttributeVisibility.Public);

            // Create the lobby
            _eosLobbyManager.CreateLobby(lobby, OnLobbyCreationComplete);

            // Update local state
            IsPrivate = isPrivate;

            Debug.Log($"[RecipeRageLobbyManager] Creating lobby: {lobbyName}, MaxPlayers: {maxPlayers}, IsPrivate: {isPrivate}");
        }

        /// <summary>
        /// Join an existing lobby.
        /// </summary>
        /// <param name="lobbyId"> The ID of the lobby to join </param>
        public void JoinLobby(string lobbyId)
        {
            // For now, we'll just simulate joining a lobby
            // In a real implementation, we would use the EOSLobbyManager.JoinLobby method
            Debug.Log($"[RecipeRageLobbyManager] Joining lobby: {lobbyId}");

            // Simulate a successful join
            OnLobbyJoinComplete(Result.Success);
        }

        /// <summary>
        /// Leave the current lobby.
        /// </summary>
        public void LeaveLobby()
        {
            if (_currentLobby != null)
            {
                // For now, we'll just simulate leaving a lobby
                // In a real implementation, we would use the EOSLobbyManager.LeaveLobby method
                Debug.Log($"[RecipeRageLobbyManager] Leaving lobby: {_currentLobby.Id}");

                // Simulate a successful leave
                OnLobbyLeftComplete(Result.Success);
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
        /// <param name="isReady"> Whether the player is ready </param>
        public void SetPlayerReady(bool isReady)
        {
            if (_currentLobby != null)
            {
                // Find the local player
                var localUserId = EOSManager.Instance.GetProductUserId();

                foreach (var member in _currentLobby.Members)
                {
                    if (member.ProductId == localUserId)
                    {
                        // Update the ready state
                        AddMemberAttribute(_currentLobby, "IsReady", isReady.ToString(), LobbyAttributeVisibility.Public);

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
        /// <param name="teamId"> The team ID </param>
        public void SetPlayerTeam(TeamId teamId)
        {
            if (_currentLobby != null)
            {
                // Find the local player
                var localUserId = EOSManager.Instance.GetProductUserId();

                foreach (var member in _currentLobby.Members)
                {
                    if (member.ProductId == localUserId)
                    {
                        // Update the team
                        AddMemberAttribute(_currentLobby, "TeamId", ((int)teamId).ToString(), LobbyAttributeVisibility.Public);

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
        /// <param name="characterClass"> The character class </param>
        public void SetPlayerCharacterClass(CharacterClass characterClass)
        {
            if (_currentLobby != null)
            {
                // Find the local player
                var localUserId = EOSManager.Instance.GetProductUserId();

                foreach (var member in _currentLobby.Members)
                {
                    if (member.ProductId == localUserId)
                    {
                        // Update the character class
                        AddMemberAttribute(_currentLobby, "CharacterClass", ((int)characterClass).ToString(), LobbyAttributeVisibility.Public);

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
        /// <param name="gameMode"> The game mode </param>
        public void SetGameMode(GameMode gameMode)
        {
            if (_currentLobby != null && IsLobbyOwner)
            {
                // Update the game mode
                AddLobbyAttribute(_currentLobby, "GameMode", gameMode.ToString(), LobbyAttributeVisibility.Public);

                // Update local state
                CurrentGameMode = gameMode;

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
        /// <param name="mapName"> The map name </param>
        public void SetMapName(string mapName)
        {
            if (_currentLobby != null && IsLobbyOwner)
            {
                // Update the map name
                AddLobbyAttribute(_currentLobby, "MapName", mapName, LobbyAttributeVisibility.Public);

                // Update local state
                CurrentMapName = mapName;

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
        /// <returns> Whether all players are ready </returns>
        public bool AreAllPlayersReady()
        {
            foreach (var player in TeamA)
            {
                if (!player.IsReady)
                {
                    return false;
                }
            }

            foreach (var player in TeamB)
            {
                if (!player.IsReady)
                {
                    return false;
                }
            }

            return TeamA.Count + TeamB.Count > 0;
        }

        /// <summary>
        /// Add a lobby attribute to a lobby.
        /// </summary>
        /// <param name="lobby"> The lobby </param>
        /// <param name="key"> The attribute key </param>
        /// <param name="value"> The attribute value </param>
        /// <param name="visibility"> The attribute visibility </param>
        private void AddLobbyAttribute(Lobby lobby, string key, string value, LobbyAttributeVisibility visibility)
        {
            // For now, we'll just simulate adding a lobby attribute
            // In a real implementation, we would use the appropriate EOSLobbyManager method
            Debug.Log($"[RecipeRageLobbyManager] Adding lobby attribute: {key}={value}");
        }

        /// <summary>
        /// Add a member attribute to a lobby.
        /// </summary>
        /// <param name="lobby"> The lobby </param>
        /// <param name="key"> The attribute key </param>
        /// <param name="value"> The attribute value </param>
        /// <param name="visibility"> The attribute visibility </param>
        private void AddMemberAttribute(Lobby lobby, string key, string value, LobbyAttributeVisibility visibility)
        {
            // For now, we'll just simulate adding a member attribute
            // In a real implementation, we would use the appropriate EOSLobbyManager method
            Debug.Log($"[RecipeRageLobbyManager] Adding member attribute: {key}={value}");
        }

        /// <summary>
        /// Update the teams based on the current lobby.
        /// </summary>
        private void UpdateTeams()
        {
            // Clear the teams
            TeamA.Clear();
            TeamB.Clear();

            if (_currentLobby == null)
            {
                return;
            }

            // Process each member
            foreach (var member in _currentLobby.Members)
            {
                // Create or get the player info
                string playerId = member.ProductId.ToString();

                if (_playerInfoCache.TryGetValue(playerId, out var playerInfo))
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
                foreach (KeyValuePair<string, LobbyAttribute> kvp in member.MemberAttributes)
                {
                    var attribute = kvp.Value;

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
                    TeamA.Add(playerInfo);
                }
                else
                {
                    TeamB.Add(playerInfo);
                }
            }

            // Extract lobby attributes
            foreach (var attribute in _currentLobby.Attributes)
            {
                switch (attribute.Key)
                {
                    case "GameMode":
                        if (Enum.TryParse(attribute.AsString, out GameMode gameMode))
                        {
                            CurrentGameMode = gameMode;
                        }
                        break;
                    case "MapName":
                        CurrentMapName = attribute.AsString;
                        break;
                }
            }

            // Update private state
            IsPrivate = _currentLobby.LobbyPermissionLevel == LobbyPermissionLevel.Inviteonly;
        }

        /// <summary>
        /// Handle lobby changes.
        /// </summary>
        /// <param name="sender"> The sender </param>
        /// <param name="e"> The event args </param>
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
        /// <param name="result"> The result </param>
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
        /// <param name="result"> The result </param>
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
        /// <param name="result"> The result </param>
        private void OnLobbyLeftComplete(Result result)
        {
            if (result == Result.Success)
            {
                Debug.Log("[RecipeRageLobbyManager] Lobby left");

                // Clear the current lobby
                _currentLobby = null;

                // Clear teams
                TeamA.Clear();
                TeamB.Clear();
            }
            else
            {
                Debug.LogError($"[RecipeRageLobbyManager] Failed to leave lobby: {result}");
            }

            OnLobbyLeft?.Invoke(result);
        }
    }
}