using System;
using System.Collections.Generic;
using System.Linq;
using Epic.OnlineServices;
using PlayEveryWare.EpicOnlineServices.Samples;
using RecipeRage.Core.GameModes;
using RecipeRage.Core.Networking.Common;
using RecipeRage.Core.Networking.EOS;
using RecipeRage.Core.Patterns;
using UnityEngine;

namespace RecipeRage.Core.Networking
{
    /// <summary>
    /// Central manager for all networking functionality in RecipeRage.
    /// </summary>
    public class RecipeRageNetworkManager : MonoBehaviourSingleton<RecipeRageNetworkManager>
    {
        // References to our wrapper components
        public RecipeRageSessionManager SessionManager { get; private set; }
        public RecipeRageLobbyManager LobbyManager { get; private set; }
        public RecipeRageP2PManager P2PManager { get; private set; }
        
        // Network state
        public bool IsConnected { get; private set; }
        public bool IsHost { get; private set; }
        public NetworkConnectionState ConnectionState { get; private set; }
        
        // Events
        public event Action OnNetworkInitialized;
        public event Action OnNetworkStateChanged;
        public event Action<NetworkSessionInfo> OnSessionCreated;
        public event Action<NetworkSessionInfo> OnSessionJoined;
        public event Action OnSessionLeft;
        public event Action<NetworkPlayer> OnPlayerJoined;
        public event Action<NetworkPlayer> OnPlayerLeft;
        public event Action<NetworkPlayer> OnPlayerReadyChanged;
        public event Action<GameMode> OnGameModeChanged;
        public event Action<string> OnMapChanged;
        
        /// <summary>
        /// Network connection states.
        /// </summary>
        public enum NetworkConnectionState
        {
            Disconnected,
            Connecting,
            Connected,
            Disconnecting
        }
        
        /// <summary>
        /// Initialize the network manager.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            
            // Register with the service locator
            ServiceLocator.Instance.Register<RecipeRageNetworkManager>(this);
            
            // Create and initialize our wrapper components
            SessionManager = gameObject.AddComponent<RecipeRageSessionManager>();
            LobbyManager = gameObject.AddComponent<RecipeRageLobbyManager>();
            P2PManager = gameObject.AddComponent<RecipeRageP2PManager>();
            
            // Initialize the components
            Initialize();
        }
        
        /// <summary>
        /// Initialize the network manager.
        /// </summary>
        public void Initialize()
        {
            // Initialize the components
            SessionManager.Initialize();
            LobbyManager.Initialize();
            P2PManager.Initialize();
            
            // Subscribe to events
            SessionManager.OnSessionCreated += OnSessionCreatedHandler;
            SessionManager.OnSessionJoined += OnSessionJoinedHandler;
            SessionManager.OnSessionLeft += OnSessionLeftHandler;
            
            LobbyManager.OnLobbyCreated += OnLobbyCreatedHandler;
            LobbyManager.OnLobbyJoined += OnLobbyJoinedHandler;
            LobbyManager.OnLobbyLeft += OnLobbyLeftHandler;
            LobbyManager.OnLobbyUpdated += OnLobbyUpdatedHandler;
            LobbyManager.OnPlayerJoined += OnPlayerJoinedHandler;
            LobbyManager.OnPlayerLeft += OnPlayerLeftHandler;
            LobbyManager.OnPlayerReadyChanged += OnPlayerReadyChangedHandler;
            LobbyManager.OnGameModeChanged += OnGameModeChangedHandler;
            LobbyManager.OnMapChanged += OnMapChangedHandler;
            
            // Set initial state
            ConnectionState = NetworkConnectionState.Disconnected;
            IsConnected = false;
            IsHost = false;
            
            Debug.Log("[RecipeRageNetworkManager] Initialized");
            
            OnNetworkInitialized?.Invoke();
        }
        
        /// <summary>
        /// Create a game session.
        /// </summary>
        /// <param name="sessionName">The session name</param>
        /// <param name="gameMode">The game mode</param>
        /// <param name="mapName">The map name</param>
        public void CreateGame(string sessionName, GameMode gameMode, string mapName)
        {
            if (ConnectionState != NetworkConnectionState.Disconnected)
            {
                Debug.LogError("[RecipeRageNetworkManager] Cannot create game: already connected");
                return;
            }
            
            ConnectionState = NetworkConnectionState.Connecting;
            OnNetworkStateChanged?.Invoke();
            
            // Create a lobby first
            LobbyManager.CreateLobby(sessionName, gameMode.MaxPlayers, false, gameMode, mapName);
            
            Debug.Log($"[RecipeRageNetworkManager] Creating game: {sessionName}, Game Mode: {gameMode.DisplayName}, Map: {mapName}");
        }
        
        /// <summary>
        /// Join a game by session ID.
        /// </summary>
        /// <param name="sessionId">The session ID</param>
        public void JoinGame(string sessionId)
        {
            if (ConnectionState != NetworkConnectionState.Disconnected)
            {
                Debug.LogError("[RecipeRageNetworkManager] Cannot join game: already connected");
                return;
            }
            
            ConnectionState = NetworkConnectionState.Connecting;
            OnNetworkStateChanged?.Invoke();
            
            // Join the lobby
            LobbyManager.JoinLobby(sessionId);
            
            Debug.Log($"[RecipeRageNetworkManager] Joining game: {sessionId}");
        }
        
        /// <summary>
        /// Leave the current game.
        /// </summary>
        public void LeaveGame()
        {
            if (ConnectionState == NetworkConnectionState.Disconnected)
            {
                Debug.LogError("[RecipeRageNetworkManager] Cannot leave game: not connected");
                return;
            }
            
            ConnectionState = NetworkConnectionState.Disconnecting;
            OnNetworkStateChanged?.Invoke();
            
            // Leave the lobby
            LobbyManager.LeaveLobby();
            
            Debug.Log("[RecipeRageNetworkManager] Leaving game");
        }
        
        /// <summary>
        /// Search for games.
        /// </summary>
        /// <param name="callback">Callback to invoke with the results</param>
        public void FindGames(Action<List<NetworkSessionInfo>> callback)
        {
            // Search for lobbies
            LobbyManager.FindLobbies(callback);
            
            Debug.Log("[RecipeRageNetworkManager] Searching for games");
        }
        
        /// <summary>
        /// Set the player's ready state.
        /// </summary>
        /// <param name="isReady">Whether the player is ready</param>
        public void SetReady(bool isReady)
        {
            if (ConnectionState != NetworkConnectionState.Connected)
            {
                Debug.LogError("[RecipeRageNetworkManager] Cannot set ready state: not connected");
                return;
            }
            
            // Set the ready state
            LobbyManager.SetReady(isReady);
            
            Debug.Log($"[RecipeRageNetworkManager] Setting ready state: {isReady}");
        }
        
        /// <summary>
        /// Set the player's team.
        /// </summary>
        /// <param name="teamId">The team ID</param>
        public void SetTeam(int teamId)
        {
            if (ConnectionState != NetworkConnectionState.Connected)
            {
                Debug.LogError("[RecipeRageNetworkManager] Cannot set team: not connected");
                return;
            }
            
            // Set the team
            LobbyManager.SetTeam(teamId);
            
            Debug.Log($"[RecipeRageNetworkManager] Setting team: {teamId}");
        }
        
        /// <summary>
        /// Set the player's character.
        /// </summary>
        /// <param name="characterType">The character type</param>
        public void SetCharacter(int characterType)
        {
            if (ConnectionState != NetworkConnectionState.Connected)
            {
                Debug.LogError("[RecipeRageNetworkManager] Cannot set character: not connected");
                return;
            }
            
            // Set the character
            LobbyManager.SetCharacter(characterType);
            
            Debug.Log($"[RecipeRageNetworkManager] Setting character: {characterType}");
        }
        
        /// <summary>
        /// Set the game mode.
        /// </summary>
        /// <param name="gameModeId">The game mode ID</param>
        public void SetGameMode(string gameModeId)
        {
            if (ConnectionState != NetworkConnectionState.Connected || !IsHost)
            {
                Debug.LogError("[RecipeRageNetworkManager] Cannot set game mode: not connected or not host");
                return;
            }
            
            // Set the game mode
            LobbyManager.SetGameMode(gameModeId);
            
            Debug.Log($"[RecipeRageNetworkManager] Setting game mode: {gameModeId}");
        }
        
        /// <summary>
        /// Set the map.
        /// </summary>
        /// <param name="mapName">The map name</param>
        public void SetMap(string mapName)
        {
            if (ConnectionState != NetworkConnectionState.Connected || !IsHost)
            {
                Debug.LogError("[RecipeRageNetworkManager] Cannot set map: not connected or not host");
                return;
            }
            
            // Set the map
            LobbyManager.SetMap(mapName);
            
            Debug.Log($"[RecipeRageNetworkManager] Setting map: {mapName}");
        }
        
        /// <summary>
        /// Start the game.
        /// </summary>
        public void StartGame()
        {
            if (ConnectionState != NetworkConnectionState.Connected || !IsHost)
            {
                Debug.LogError("[RecipeRageNetworkManager] Cannot start game: not connected or not host");
                return;
            }
            
            if (!AreAllPlayersReady())
            {
                Debug.LogError("[RecipeRageNetworkManager] Cannot start game: not all players are ready");
                return;
            }
            
            // Create a session
            GameMode gameMode = LobbyManager.GetSelectedGameMode();
            string mapName = LobbyManager.GetSelectedMapName();
            string sessionName = $"RecipeRage_{DateTime.Now.Ticks}";
            
            SessionManager.CreateGameSession(sessionName, gameMode, mapName);
            
            Debug.Log("[RecipeRageNetworkManager] Starting game");
        }
        
        /// <summary>
        /// Get the list of all players.
        /// </summary>
        /// <returns>The list of players</returns>
        public List<NetworkPlayer> GetPlayers()
        {
            return LobbyManager.GetPlayers();
        }
        
        /// <summary>
        /// Get the list of players in team A.
        /// </summary>
        /// <returns>The list of players in team A</returns>
        public List<NetworkPlayer> GetTeamA()
        {
            return LobbyManager.GetTeamA();
        }
        
        /// <summary>
        /// Get the list of players in team B.
        /// </summary>
        /// <returns>The list of players in team B</returns>
        public List<NetworkPlayer> GetTeamB()
        {
            return LobbyManager.GetTeamB();
        }
        
        /// <summary>
        /// Get the selected game mode.
        /// </summary>
        /// <returns>The selected game mode</returns>
        public GameMode GetSelectedGameMode()
        {
            return LobbyManager.GetSelectedGameMode();
        }
        
        /// <summary>
        /// Get the selected map name.
        /// </summary>
        /// <returns>The selected map name</returns>
        public string GetSelectedMapName()
        {
            return LobbyManager.GetSelectedMapName();
        }
        
        /// <summary>
        /// Check if all players are ready.
        /// </summary>
        /// <returns>True if all players are ready, false otherwise</returns>
        public bool AreAllPlayersReady()
        {
            return LobbyManager.AreAllPlayersReady();
        }
        
        /// <summary>
        /// Send a player action.
        /// </summary>
        /// <param name="action">The player action</param>
        public void SendPlayerAction(PlayerAction action)
        {
            if (ConnectionState != NetworkConnectionState.Connected)
            {
                Debug.LogError("[RecipeRageNetworkManager] Cannot send player action: not connected");
                return;
            }
            
            // Get all players
            List<NetworkPlayer> players = GetPlayers();
            
            // Send to all players
            foreach (NetworkPlayer player in players)
            {
                if (!player.IsLocal)
                {
                    ProductUserId targetPlayer = ProductUserId.FromString(player.PlayerId);
                    if (targetPlayer != null && targetPlayer.IsValid())
                    {
                        P2PManager.SendPlayerAction(targetPlayer, action);
                    }
                }
            }
            
            Debug.Log("[RecipeRageNetworkManager] Sending player action to all players");
        }
        
        /// <summary>
        /// Send a game state update.
        /// </summary>
        /// <param name="gameState">The game state</param>
        public void SendGameState(GameStateMessage gameState)
        {
            if (ConnectionState != NetworkConnectionState.Connected || !IsHost)
            {
                Debug.LogError("[RecipeRageNetworkManager] Cannot send game state: not connected or not host");
                return;
            }
            
            // Get all players
            List<NetworkPlayer> players = GetPlayers();
            
            // Send to all players
            foreach (NetworkPlayer player in players)
            {
                if (!player.IsLocal)
                {
                    ProductUserId targetPlayer = ProductUserId.FromString(player.PlayerId);
                    if (targetPlayer != null && targetPlayer.IsValid())
                    {
                        P2PManager.SendGameState(targetPlayer, gameState);
                    }
                }
            }
            
            Debug.Log("[RecipeRageNetworkManager] Sending game state to all players");
        }
        
        #region Event Handlers
        
        /// <summary>
        /// Handle session creation completion.
        /// </summary>
        /// <param name="result">Result code</param>
        private void OnSessionCreatedHandler(Result result)
        {
            if (result == Result.Success)
            {
                Debug.Log("[RecipeRageNetworkManager] Session created successfully");
                
                // Update state
                IsConnected = true;
                IsHost = true;
                ConnectionState = NetworkConnectionState.Connected;
                
                // Get the session info
                NetworkSessionInfo sessionInfo = SessionManager.GetCurrentSessionInfo();
                
                // Notify listeners
                OnNetworkStateChanged?.Invoke();
                OnSessionCreated?.Invoke(sessionInfo);
            }
            else
            {
                Debug.LogError($"[RecipeRageNetworkManager] Failed to create session: {result}");
                
                // Update state
                ConnectionState = NetworkConnectionState.Disconnected;
                
                // Notify listeners
                OnNetworkStateChanged?.Invoke();
            }
        }
        
        /// <summary>
        /// Handle session join completion.
        /// </summary>
        /// <param name="result">Result code</param>
        private void OnSessionJoinedHandler(Result result)
        {
            if (result == Result.Success)
            {
                Debug.Log("[RecipeRageNetworkManager] Session joined successfully");
                
                // Update state
                IsConnected = true;
                IsHost = false;
                ConnectionState = NetworkConnectionState.Connected;
                
                // Get the session info
                NetworkSessionInfo sessionInfo = SessionManager.GetCurrentSessionInfo();
                
                // Notify listeners
                OnNetworkStateChanged?.Invoke();
                OnSessionJoined?.Invoke(sessionInfo);
            }
            else
            {
                Debug.LogError($"[RecipeRageNetworkManager] Failed to join session: {result}");
                
                // Update state
                ConnectionState = NetworkConnectionState.Disconnected;
                
                // Notify listeners
                OnNetworkStateChanged?.Invoke();
            }
        }
        
        /// <summary>
        /// Handle session leave completion.
        /// </summary>
        /// <param name="result">Result code</param>
        private void OnSessionLeftHandler(Result result)
        {
            if (result == Result.Success)
            {
                Debug.Log("[RecipeRageNetworkManager] Session left successfully");
            }
            else
            {
                Debug.LogError($"[RecipeRageNetworkManager] Failed to leave session: {result}");
            }
            
            // Update state
            IsConnected = false;
            IsHost = false;
            ConnectionState = NetworkConnectionState.Disconnected;
            
            // Notify listeners
            OnNetworkStateChanged?.Invoke();
            OnSessionLeft?.Invoke();
        }
        
        /// <summary>
        /// Handle lobby creation completion.
        /// </summary>
        /// <param name="result">Result code</param>
        private void OnLobbyCreatedHandler(Result result)
        {
            if (result == Result.Success)
            {
                Debug.Log("[RecipeRageNetworkManager] Lobby created successfully");
                
                // Update state
                IsConnected = true;
                IsHost = true;
                ConnectionState = NetworkConnectionState.Connected;
                
                // Notify listeners
                OnNetworkStateChanged?.Invoke();
            }
            else
            {
                Debug.LogError($"[RecipeRageNetworkManager] Failed to create lobby: {result}");
                
                // Update state
                ConnectionState = NetworkConnectionState.Disconnected;
                
                // Notify listeners
                OnNetworkStateChanged?.Invoke();
            }
        }
        
        /// <summary>
        /// Handle lobby join completion.
        /// </summary>
        /// <param name="result">Result code</param>
        private void OnLobbyJoinedHandler(Result result)
        {
            if (result == Result.Success)
            {
                Debug.Log("[RecipeRageNetworkManager] Lobby joined successfully");
                
                // Update state
                IsConnected = true;
                IsHost = LobbyManager.IsHost();
                ConnectionState = NetworkConnectionState.Connected;
                
                // Notify listeners
                OnNetworkStateChanged?.Invoke();
            }
            else
            {
                Debug.LogError($"[RecipeRageNetworkManager] Failed to join lobby: {result}");
                
                // Update state
                ConnectionState = NetworkConnectionState.Disconnected;
                
                // Notify listeners
                OnNetworkStateChanged?.Invoke();
            }
        }
        
        /// <summary>
        /// Handle lobby leave completion.
        /// </summary>
        /// <param name="result">Result code</param>
        private void OnLobbyLeftHandler(Result result)
        {
            if (result == Result.Success)
            {
                Debug.Log("[RecipeRageNetworkManager] Lobby left successfully");
            }
            else
            {
                Debug.LogError($"[RecipeRageNetworkManager] Failed to leave lobby: {result}");
            }
            
            // Update state
            IsConnected = false;
            IsHost = false;
            ConnectionState = NetworkConnectionState.Disconnected;
            
            // Notify listeners
            OnNetworkStateChanged?.Invoke();
        }
        
        /// <summary>
        /// Handle lobby updates.
        /// </summary>
        private void OnLobbyUpdatedHandler()
        {
            Debug.Log("[RecipeRageNetworkManager] Lobby updated");
            
            // Update host status
            IsHost = LobbyManager.IsHost();
            
            // Notify listeners
            OnNetworkStateChanged?.Invoke();
        }
        
        /// <summary>
        /// Handle player join.
        /// </summary>
        /// <param name="player">The player</param>
        private void OnPlayerJoinedHandler(NetworkPlayer player)
        {
            Debug.Log($"[RecipeRageNetworkManager] Player joined: {player.DisplayName}");
            
            // Notify listeners
            OnPlayerJoined?.Invoke(player);
        }
        
        /// <summary>
        /// Handle player leave.
        /// </summary>
        /// <param name="player">The player</param>
        private void OnPlayerLeftHandler(NetworkPlayer player)
        {
            Debug.Log($"[RecipeRageNetworkManager] Player left: {player.DisplayName}");
            
            // Notify listeners
            OnPlayerLeft?.Invoke(player);
        }
        
        /// <summary>
        /// Handle player ready state change.
        /// </summary>
        /// <param name="player">The player</param>
        private void OnPlayerReadyChangedHandler(NetworkPlayer player)
        {
            Debug.Log($"[RecipeRageNetworkManager] Player ready state changed: {player.DisplayName}, Ready: {player.IsReady}");
            
            // Notify listeners
            OnPlayerReadyChanged?.Invoke(player);
            
            // If all players are ready and we're the host, we can start the game
            if (IsHost && AreAllPlayersReady())
            {
                Debug.Log("[RecipeRageNetworkManager] All players are ready, can start the game");
            }
        }
        
        /// <summary>
        /// Handle game mode change.
        /// </summary>
        /// <param name="gameMode">The game mode</param>
        private void OnGameModeChangedHandler(GameMode gameMode)
        {
            Debug.Log($"[RecipeRageNetworkManager] Game mode changed: {gameMode.DisplayName}");
            
            // Notify listeners
            OnGameModeChanged?.Invoke(gameMode);
        }
        
        /// <summary>
        /// Handle map change.
        /// </summary>
        /// <param name="mapName">The map name</param>
        private void OnMapChangedHandler(string mapName)
        {
            Debug.Log($"[RecipeRageNetworkManager] Map changed: {mapName}");
            
            // Notify listeners
            OnMapChanged?.Invoke(mapName);
        }
        
        #endregion
    }
}
