using System;
using System.Collections.Generic;
using System.Linq;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using PlayEveryWare.EpicOnlineServices.Samples;
using RecipeRage.Core.GameModes;
using RecipeRage.Core.Networking.Common;
using UnityEngine;

namespace RecipeRage.Core.Networking.EOS
{
    /// <summary>
    /// Manages lobbies using the EOS Lobby interface.
    /// Extends the functionality of the EOSLobbyManager from the EOS samples.
    /// </summary>
    public class RecipeRageLobbyManager : MonoBehaviour
    {
        // Reference to the EOS Lobby Manager
        private EOSLobbyManager _eosLobbyManager;
        
        // Game-specific lobby properties
        private List<NetworkPlayer> _players = new List<NetworkPlayer>();
        private List<NetworkPlayer> _teamA = new List<NetworkPlayer>();
        private List<NetworkPlayer> _teamB = new List<NetworkPlayer>();
        private GameMode _selectedGameMode;
        private string _selectedMapName;
        private bool _isHost;
        
        // Events
        public event Action<Result> OnLobbyCreated;
        public event Action<Result> OnLobbyJoined;
        public event Action<Result> OnLobbyLeft;
        public event Action OnLobbyUpdated;
        public event Action<NetworkPlayer> OnPlayerJoined;
        public event Action<NetworkPlayer> OnPlayerLeft;
        public event Action<NetworkPlayer> OnPlayerReadyChanged;
        public event Action<NetworkPlayer, int> OnPlayerTeamChanged;
        public event Action<NetworkPlayer, int> OnPlayerCharacterChanged;
        public event Action<GameMode> OnGameModeChanged;
        public event Action<string> OnMapChanged;
        
        /// <summary>
        /// Initialize the lobby manager.
        /// </summary>
        public void Initialize()
        {
            // Get the EOS Lobby Manager
            _eosLobbyManager = EOSManager.Instance.GetComponent<EOSLobbyManager>();
            if (_eosLobbyManager == null)
            {
                Debug.LogError("[RecipeRageLobbyManager] EOSLobbyManager not found!");
                return;
            }
            
            // Subscribe to EOS lobby events
            _eosLobbyManager.LobbyChanged += OnEOSLobbyChanged;
            
            Debug.Log("[RecipeRageLobbyManager] Initialized");
        }
        
        /// <summary>
        /// Create a lobby.
        /// </summary>
        /// <param name="lobbyName">The lobby name</param>
        /// <param name="maxPlayers">The maximum number of players</param>
        /// <param name="isPrivate">Whether the lobby is private</param>
        /// <param name="gameMode">The game mode</param>
        /// <param name="mapName">The map name</param>
        public void CreateLobby(string lobbyName, int maxPlayers, bool isPrivate, GameMode gameMode, string mapName)
        {
            if (_eosLobbyManager == null)
            {
                Debug.LogError("[RecipeRageLobbyManager] EOSLobbyManager not initialized!");
                return;
            }
            
            _selectedGameMode = gameMode;
            _selectedMapName = mapName;
            _isHost = true;
            
            // Create a lobby using the EOS sample code
            Lobby lobby = new Lobby();
            lobby.MaxNumLobbyMembers = (uint)maxPlayers;
            lobby.LobbyPermissionLevel = isPrivate ? LobbyPermissionLevel.Inviteonly : LobbyPermissionLevel.Publicadvertised;
            lobby.AllowInvites = true;
            lobby.DisableHostMigration = false;
            lobby.PresenceEnabled = true;
            lobby.RTCRoomEnabled = true;
            
            // Add game-specific attributes
            AddLobbyAttributes(lobby, gameMode, mapName);
            
            // Use the EOS sample code to create the lobby
            _eosLobbyManager.CreateLobby(lobby, OnLobbyCreationComplete);
            
            Debug.Log($"[RecipeRageLobbyManager] Creating lobby: {lobbyName}, Max Players: {maxPlayers}, Private: {isPrivate}, Game Mode: {gameMode.DisplayName}, Map: {mapName}");
        }
        
        /// <summary>
        /// Join a lobby by ID.
        /// </summary>
        /// <param name="lobbyId">The lobby ID</param>
        public void JoinLobby(string lobbyId)
        {
            if (_eosLobbyManager == null)
            {
                Debug.LogError("[RecipeRageLobbyManager] EOSLobbyManager not initialized!");
                return;
            }
            
            _isHost = false;
            
            // Use the EOS sample code to join the lobby
            _eosLobbyManager.JoinLobby(lobbyId, OnLobbyJoinComplete);
            
            Debug.Log($"[RecipeRageLobbyManager] Joining lobby: {lobbyId}");
        }
        
        /// <summary>
        /// Leave the current lobby.
        /// </summary>
        public void LeaveLobby()
        {
            if (_eosLobbyManager == null)
            {
                Debug.LogError("[RecipeRageLobbyManager] EOSLobbyManager not initialized!");
                return;
            }
            
            // Use the EOS sample code to leave the lobby
            _eosLobbyManager.LeaveLobby(OnLobbyLeftComplete);
            
            Debug.Log("[RecipeRageLobbyManager] Leaving lobby");
        }
        
        /// <summary>
        /// Search for lobbies.
        /// </summary>
        /// <param name="callback">Callback to invoke with the results</param>
        public void FindLobbies(Action<List<NetworkSessionInfo>> callback)
        {
            if (_eosLobbyManager == null)
            {
                Debug.LogError("[RecipeRageLobbyManager] EOSLobbyManager not initialized!");
                callback?.Invoke(new List<NetworkSessionInfo>());
                return;
            }
            
            // Use the EOS sample code to search for lobbies
            _eosLobbyManager.FindLobbies(results => 
            {
                // Convert EOS lobbies to our NetworkSessionInfo format
                List<NetworkSessionInfo> sessionInfos = new List<NetworkSessionInfo>();
                foreach (var kvp in results)
                {
                    Lobby lobby = kvp.Key;
                    
                    NetworkSessionInfo sessionInfo = new NetworkSessionInfo
                    {
                        SessionId = lobby.Id,
                        SessionName = GetLobbyName(lobby),
                        PlayerCount = lobby.Members.Count,
                        MaxPlayers = (int)lobby.MaxNumLobbyMembers,
                        IsPrivate = lobby.LobbyPermissionLevel == LobbyPermissionLevel.Inviteonly,
                        HostName = lobby.LobbyOwnerDisplayName,
                        GameMode = GetGameModeId(lobby),
                        MapName = GetMapName(lobby)
                    };
                    
                    sessionInfos.Add(sessionInfo);
                }
                
                callback?.Invoke(sessionInfos);
            });
            
            Debug.Log("[RecipeRageLobbyManager] Searching for lobbies");
        }
        
        /// <summary>
        /// Set the player's ready state.
        /// </summary>
        /// <param name="isReady">Whether the player is ready</param>
        public void SetReady(bool isReady)
        {
            if (_eosLobbyManager == null)
            {
                Debug.LogError("[RecipeRageLobbyManager] EOSLobbyManager not initialized!");
                return;
            }
            
            // Use the EOS sample code to set the player's ready state
            LobbyAttribute readyAttr = new LobbyAttribute();
            readyAttr.Key = "ISREADY";
            readyAttr.ValueType = AttributeType.Boolean;
            readyAttr.AsBool = isReady;
            readyAttr.Visibility = LobbyAttributeVisibility.Public;
            
            _eosLobbyManager.UpdateMemberAttribute(readyAttr);
            
            Debug.Log($"[RecipeRageLobbyManager] Setting ready state: {isReady}");
        }
        
        /// <summary>
        /// Set the player's team.
        /// </summary>
        /// <param name="teamId">The team ID</param>
        public void SetTeam(int teamId)
        {
            if (_eosLobbyManager == null)
            {
                Debug.LogError("[RecipeRageLobbyManager] EOSLobbyManager not initialized!");
                return;
            }
            
            // Use the EOS sample code to set the player's team
            LobbyAttribute teamAttr = new LobbyAttribute();
            teamAttr.Key = "TEAMID";
            teamAttr.ValueType = AttributeType.Int64;
            teamAttr.AsInt64 = teamId;
            teamAttr.Visibility = LobbyAttributeVisibility.Public;
            
            _eosLobbyManager.UpdateMemberAttribute(teamAttr);
            
            Debug.Log($"[RecipeRageLobbyManager] Setting team: {teamId}");
        }
        
        /// <summary>
        /// Set the player's character.
        /// </summary>
        /// <param name="characterType">The character type</param>
        public void SetCharacter(int characterType)
        {
            if (_eosLobbyManager == null)
            {
                Debug.LogError("[RecipeRageLobbyManager] EOSLobbyManager not initialized!");
                return;
            }
            
            // Use the EOS sample code to set the player's character
            LobbyAttribute characterAttr = new LobbyAttribute();
            characterAttr.Key = "CHARACTERTYPE";
            characterAttr.ValueType = AttributeType.Int64;
            characterAttr.AsInt64 = characterType;
            characterAttr.Visibility = LobbyAttributeVisibility.Public;
            
            _eosLobbyManager.UpdateMemberAttribute(characterAttr);
            
            Debug.Log($"[RecipeRageLobbyManager] Setting character: {characterType}");
        }
        
        /// <summary>
        /// Set the game mode.
        /// </summary>
        /// <param name="gameModeId">The game mode ID</param>
        public void SetGameMode(string gameModeId)
        {
            if (_eosLobbyManager == null || !_isHost)
            {
                Debug.LogError("[RecipeRageLobbyManager] EOSLobbyManager not initialized or not host!");
                return;
            }
            
            GameMode gameMode = GameModeManager.Instance.GetGameMode(gameModeId);
            if (gameMode == null)
            {
                Debug.LogError($"[RecipeRageLobbyManager] Game mode not found: {gameModeId}");
                return;
            }
            
            _selectedGameMode = gameMode;
            
            // Use the EOS sample code to set the game mode
            LobbyAttribute gameModeIdAttr = new LobbyAttribute();
            gameModeIdAttr.Key = "GAMEMODEID";
            gameModeIdAttr.ValueType = AttributeType.String;
            gameModeIdAttr.AsString = gameMode.Id;
            gameModeIdAttr.Visibility = LobbyAttributeVisibility.Public;
            
            LobbyAttribute gameModeNameAttr = new LobbyAttribute();
            gameModeNameAttr.Key = "GAMEMODENAME";
            gameModeNameAttr.ValueType = AttributeType.String;
            gameModeNameAttr.AsString = gameMode.DisplayName;
            gameModeNameAttr.Visibility = LobbyAttributeVisibility.Public;
            
            _eosLobbyManager.UpdateLobbyAttribute(gameModeIdAttr);
            _eosLobbyManager.UpdateLobbyAttribute(gameModeNameAttr);
            
            Debug.Log($"[RecipeRageLobbyManager] Setting game mode: {gameMode.DisplayName} ({gameMode.Id})");
            
            OnGameModeChanged?.Invoke(gameMode);
        }
        
        /// <summary>
        /// Set the map.
        /// </summary>
        /// <param name="mapName">The map name</param>
        public void SetMap(string mapName)
        {
            if (_eosLobbyManager == null || !_isHost)
            {
                Debug.LogError("[RecipeRageLobbyManager] EOSLobbyManager not initialized or not host!");
                return;
            }
            
            _selectedMapName = mapName;
            
            // Use the EOS sample code to set the map
            LobbyAttribute mapNameAttr = new LobbyAttribute();
            mapNameAttr.Key = "MAPNAME";
            mapNameAttr.ValueType = AttributeType.String;
            mapNameAttr.AsString = mapName;
            mapNameAttr.Visibility = LobbyAttributeVisibility.Public;
            
            _eosLobbyManager.UpdateLobbyAttribute(mapNameAttr);
            
            Debug.Log($"[RecipeRageLobbyManager] Setting map: {mapName}");
            
            OnMapChanged?.Invoke(mapName);
        }
        
        /// <summary>
        /// Get the list of all players in the lobby.
        /// </summary>
        /// <returns>The list of players</returns>
        public List<NetworkPlayer> GetPlayers()
        {
            return _players;
        }
        
        /// <summary>
        /// Get the list of players in team A.
        /// </summary>
        /// <returns>The list of players in team A</returns>
        public List<NetworkPlayer> GetTeamA()
        {
            return _teamA;
        }
        
        /// <summary>
        /// Get the list of players in team B.
        /// </summary>
        /// <returns>The list of players in team B</returns>
        public List<NetworkPlayer> GetTeamB()
        {
            return _teamB;
        }
        
        /// <summary>
        /// Get the selected game mode.
        /// </summary>
        /// <returns>The selected game mode</returns>
        public GameMode GetSelectedGameMode()
        {
            return _selectedGameMode;
        }
        
        /// <summary>
        /// Get the selected map name.
        /// </summary>
        /// <returns>The selected map name</returns>
        public string GetSelectedMapName()
        {
            return _selectedMapName;
        }
        
        /// <summary>
        /// Check if the local player is the host.
        /// </summary>
        /// <returns>True if the local player is the host, false otherwise</returns>
        public bool IsHost()
        {
            return _isHost;
        }
        
        /// <summary>
        /// Check if all players are ready.
        /// </summary>
        /// <returns>True if all players are ready, false otherwise</returns>
        public bool AreAllPlayersReady()
        {
            return _players.Count > 0 && _players.All(p => p.IsReady);
        }
        
        #region Private Methods
        
        /// <summary>
        /// Add lobby attributes.
        /// </summary>
        /// <param name="lobby">The lobby</param>
        /// <param name="gameMode">The game mode</param>
        /// <param name="mapName">The map name</param>
        private void AddLobbyAttributes(Lobby lobby, GameMode gameMode, string mapName)
        {
            // Add lobby name
            LobbyAttribute lobbyNameAttr = new LobbyAttribute();
            lobbyNameAttr.Key = "LOBBYNAME";
            lobbyNameAttr.ValueType = AttributeType.String;
            lobbyNameAttr.AsString = $"{gameMode.DisplayName} - {mapName}";
            lobbyNameAttr.Visibility = LobbyAttributeVisibility.Public;
            lobby.Attributes.Add(lobbyNameAttr);
            
            // Add game mode ID
            LobbyAttribute gameModeIdAttr = new LobbyAttribute();
            gameModeIdAttr.Key = "GAMEMODEID";
            gameModeIdAttr.ValueType = AttributeType.String;
            gameModeIdAttr.AsString = gameMode.Id;
            gameModeIdAttr.Visibility = LobbyAttributeVisibility.Public;
            lobby.Attributes.Add(gameModeIdAttr);
            
            // Add game mode name
            LobbyAttribute gameModeNameAttr = new LobbyAttribute();
            gameModeNameAttr.Key = "GAMEMODENAME";
            gameModeNameAttr.ValueType = AttributeType.String;
            gameModeNameAttr.AsString = gameMode.DisplayName;
            gameModeNameAttr.Visibility = LobbyAttributeVisibility.Public;
            lobby.Attributes.Add(gameModeNameAttr);
            
            // Add map name
            LobbyAttribute mapNameAttr = new LobbyAttribute();
            mapNameAttr.Key = "MAPNAME";
            mapNameAttr.ValueType = AttributeType.String;
            mapNameAttr.AsString = mapName;
            mapNameAttr.Visibility = LobbyAttributeVisibility.Public;
            lobby.Attributes.Add(mapNameAttr);
        }
        
        /// <summary>
        /// Get the lobby name from a lobby.
        /// </summary>
        /// <param name="lobby">The lobby</param>
        /// <returns>The lobby name, or a default name if not found</returns>
        private string GetLobbyName(Lobby lobby)
        {
            foreach (LobbyAttribute attr in lobby.Attributes)
            {
                if (attr.Key == "LOBBYNAME" && attr.ValueType == AttributeType.String)
                {
                    return attr.AsString;
                }
            }
            
            return $"Lobby {lobby.Id}";
        }
        
        /// <summary>
        /// Get the game mode ID from a lobby.
        /// </summary>
        /// <param name="lobby">The lobby</param>
        /// <returns>The game mode ID, or an empty string if not found</returns>
        private string GetGameModeId(Lobby lobby)
        {
            foreach (LobbyAttribute attr in lobby.Attributes)
            {
                if (attr.Key == "GAMEMODEID" && attr.ValueType == AttributeType.String)
                {
                    return attr.AsString;
                }
            }
            
            return string.Empty;
        }
        
        /// <summary>
        /// Get the map name from a lobby.
        /// </summary>
        /// <param name="lobby">The lobby</param>
        /// <returns>The map name, or an empty string if not found</returns>
        private string GetMapName(Lobby lobby)
        {
            foreach (LobbyAttribute attr in lobby.Attributes)
            {
                if (attr.Key == "MAPNAME" && attr.ValueType == AttributeType.String)
                {
                    return attr.AsString;
                }
            }
            
            return string.Empty;
        }
        
        /// <summary>
        /// Update the player lists.
        /// </summary>
        private void UpdatePlayerLists()
        {
            _players.Clear();
            _teamA.Clear();
            _teamB.Clear();
            
            Lobby currentLobby = _eosLobbyManager.GetCurrentLobby();
            if (currentLobby == null)
            {
                return;
            }
            
            foreach (LobbyMember member in currentLobby.Members)
            {
                NetworkPlayer player = new NetworkPlayer
                {
                    PlayerId = member.ProductId.ToString(),
                    DisplayName = member.DisplayName,
                    IsLocal = member.ProductId.ToString() == EOSManager.Instance.GetProductUserId().ToString(),
                    IsHost = currentLobby.LobbyOwner.ToString() == member.ProductId.ToString(),
                    TeamId = GetTeamId(member),
                    CharacterType = GetCharacterType(member),
                    IsReady = GetIsReady(member)
                };
                
                _players.Add(player);
                
                if (player.TeamId == 0)
                {
                    _teamA.Add(player);
                }
                else
                {
                    _teamB.Add(player);
                }
            }
            
            // Update game mode and map
            if (currentLobby.Attributes.Count > 0)
            {
                string gameModeId = GetGameModeId(currentLobby);
                if (!string.IsNullOrEmpty(gameModeId))
                {
                    _selectedGameMode = GameModeManager.Instance.GetGameMode(gameModeId);
                }
                
                _selectedMapName = GetMapName(currentLobby);
            }
            
            // Update host status
            _isHost = _players.Any(p => p.IsLocal && p.IsHost);
        }
        
        /// <summary>
        /// Get the team ID from a lobby member.
        /// </summary>
        /// <param name="member">The lobby member</param>
        /// <returns>The team ID, or 0 if not found</returns>
        private int GetTeamId(LobbyMember member)
        {
            if (member.MemberAttributes.TryGetValue("TEAMID", out LobbyAttribute teamAttr))
            {
                return (int)(teamAttr.AsInt64 ?? 0);
            }
            
            return 0;
        }
        
        /// <summary>
        /// Get the character type from a lobby member.
        /// </summary>
        /// <param name="member">The lobby member</param>
        /// <returns>The character type, or 0 if not found</returns>
        private int GetCharacterType(LobbyMember member)
        {
            if (member.MemberAttributes.TryGetValue("CHARACTERTYPE", out LobbyAttribute characterAttr))
            {
                return (int)(characterAttr.AsInt64 ?? 0);
            }
            
            return 0;
        }
        
        /// <summary>
        /// Get the ready state from a lobby member.
        /// </summary>
        /// <param name="member">The lobby member</param>
        /// <returns>The ready state, or false if not found</returns>
        private bool GetIsReady(LobbyMember member)
        {
            if (member.MemberAttributes.TryGetValue("ISREADY", out LobbyAttribute readyAttr))
            {
                return readyAttr.AsBool ?? false;
            }
            
            return false;
        }
        
        #endregion
        
        #region Event Handlers
        
        /// <summary>
        /// Handle lobby changes from the EOS lobby manager.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The event args</param>
        private void OnEOSLobbyChanged(object sender, EOSLobbyManager.LobbyChangeEventArgs e)
        {
            Debug.Log($"[RecipeRageLobbyManager] Lobby changed: {e.LobbyId}, Type: {e.LobbyChangeType}");
            
            switch (e.LobbyChangeType)
            {
                case LobbyChangeType.Create:
                    // Lobby created, update our state
                    UpdatePlayerLists();
                    OnLobbyUpdated?.Invoke();
                    break;
                    
                case LobbyChangeType.Join:
                    // Lobby joined, update our state
                    UpdatePlayerLists();
                    OnLobbyUpdated?.Invoke();
                    break;
                    
                case LobbyChangeType.Leave:
                case LobbyChangeType.Kicked:
                    // Lobby left, clear our state
                    _players.Clear();
                    _teamA.Clear();
                    _teamB.Clear();
                    _selectedGameMode = null;
                    _selectedMapName = null;
                    _isHost = false;
                    OnLobbyUpdated?.Invoke();
                    break;
            }
        }
        
        #endregion
        
        #region Callbacks
        
        /// <summary>
        /// Callback for lobby creation completion.
        /// </summary>
        /// <param name="result">Result code</param>
        private void OnLobbyCreationComplete(Result result)
        {
            if (result == Result.Success)
            {
                Debug.Log("[RecipeRageLobbyManager] Lobby created successfully");
                UpdatePlayerLists();
            }
            else
            {
                Debug.LogError($"[RecipeRageLobbyManager] Failed to create lobby: {result}");
                _isHost = false;
            }
            
            OnLobbyCreated?.Invoke(result);
        }
        
        /// <summary>
        /// Callback for lobby join completion.
        /// </summary>
        /// <param name="result">Result code</param>
        private void OnLobbyJoinComplete(Result result)
        {
            if (result == Result.Success)
            {
                Debug.Log("[RecipeRageLobbyManager] Lobby joined successfully");
                UpdatePlayerLists();
            }
            else
            {
                Debug.LogError($"[RecipeRageLobbyManager] Failed to join lobby: {result}");
            }
            
            OnLobbyJoined?.Invoke(result);
        }
        
        /// <summary>
        /// Callback for lobby leave completion.
        /// </summary>
        /// <param name="result">Result code</param>
        private void OnLobbyLeftComplete(Result result)
        {
            if (result == Result.Success)
            {
                Debug.Log("[RecipeRageLobbyManager] Lobby left successfully");
                _players.Clear();
                _teamA.Clear();
                _teamB.Clear();
                _selectedGameMode = null;
                _selectedMapName = null;
                _isHost = false;
            }
            else
            {
                Debug.LogError($"[RecipeRageLobbyManager] Failed to leave lobby: {result}");
            }
            
            OnLobbyLeft?.Invoke(result);
        }
        
        #endregion
    }
}
