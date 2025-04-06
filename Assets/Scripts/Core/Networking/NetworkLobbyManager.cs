using System;
using System.Collections.Generic;
using RecipeRage.Core.Patterns;
using UnityEngine;
using Unity.Netcode;

namespace RecipeRage.Core.Networking
{
    /// <summary>
    /// Manages the lobby and matchmaking system for RecipeRage.
    /// </summary>
    public class NetworkLobbyManager : MonoBehaviourSingleton<NetworkLobbyManager>
    {
        [Header("Lobby Settings")]
        [SerializeField] private int _maxPlayersPerTeam = 2;
        [SerializeField] private int _maxTeams = 2;
        [SerializeField] private float _lobbyCountdownTime = 10f;
        [SerializeField] private float _minPlayersToStart = 2;
        
        [Header("Game Mode Settings")]
        [SerializeField] private string _defaultGameMode = "Classic";
        [SerializeField] private string _defaultMapName = "Kitchen";
        
        [Header("References")]
        [SerializeField] private NetworkManager _networkManager;
        [SerializeField] private NetworkObject _playerPrefab;
        
        /// <summary>
        /// Event triggered when the lobby state changes.
        /// </summary>
        public event Action<LobbyState> OnLobbyStateChanged;
        
        /// <summary>
        /// Event triggered when a player joins the lobby.
        /// </summary>
        public event Action<NetworkPlayer> OnPlayerJoinedLobby;
        
        /// <summary>
        /// Event triggered when a player leaves the lobby.
        /// </summary>
        public event Action<NetworkPlayer> OnPlayerLeftLobby;
        
        /// <summary>
        /// Event triggered when a player changes team.
        /// </summary>
        public event Action<NetworkPlayer, int> OnPlayerChangedTeam;
        
        /// <summary>
        /// Event triggered when a player changes character.
        /// </summary>
        public event Action<NetworkPlayer, int> OnPlayerChangedCharacter;
        
        /// <summary>
        /// Event triggered when the lobby countdown changes.
        /// </summary>
        public event Action<float> OnLobbyCountdownChanged;
        
        /// <summary>
        /// Event triggered when the game mode changes.
        /// </summary>
        public event Action<string> OnGameModeChanged;
        
        /// <summary>
        /// Event triggered when the map changes.
        /// </summary>
        public event Action<string> OnMapChanged;
        
        /// <summary>
        /// The current lobby state.
        /// </summary>
        public LobbyState LobbyState { get; private set; }
        
        /// <summary>
        /// The current game mode.
        /// </summary>
        public string GameMode { get; private set; }
        
        /// <summary>
        /// The current map name.
        /// </summary>
        public string MapName { get; private set; }
        
        /// <summary>
        /// The current lobby countdown time.
        /// </summary>
        public float LobbyCountdown { get; private set; }
        
        /// <summary>
        /// The list of players in the lobby.
        /// </summary>
        public IReadOnlyList<NetworkPlayer> LobbyPlayers => _lobbyPlayers.AsReadOnly();
        
        /// <summary>
        /// The list of players in the lobby.
        /// </summary>
        private List<NetworkPlayer> _lobbyPlayers = new List<NetworkPlayer>();
        
        /// <summary>
        /// Flag to track if the lobby is counting down.
        /// </summary>
        private bool _isCountingDown;
        
        /// <summary>
        /// Flag to track if the lobby manager is initialized.
        /// </summary>
        private bool _isInitialized;
        
        /// <summary>
        /// Initialize the lobby manager.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            
            // Find the network manager if not set
            if (_networkManager == null)
            {
                _networkManager = FindObjectOfType<NetworkManager>();
            }
            
            // Initialize values
            LobbyState = LobbyState.Inactive;
            GameMode = _defaultGameMode;
            MapName = _defaultMapName;
            LobbyCountdown = _lobbyCountdownTime;
            _isCountingDown = false;
            
            // Register the lobby manager with the service locator
            ServiceLocator.Instance.Register<NetworkLobbyManager>(this);
            
            Debug.Log("[NetworkLobbyManager] Lobby manager initialized");
        }
        
        /// <summary>
        /// Subscribe to network manager events.
        /// </summary>
        private void Start()
        {
            if (_networkManager != null)
            {
                // Subscribe to network manager events
                _networkManager.OnConnectionStateChanged += HandleConnectionStateChanged;
                _networkManager.OnPlayerJoined += HandlePlayerJoined;
                _networkManager.OnPlayerLeft += HandlePlayerLeft;
                _networkManager.OnSessionCreated += HandleSessionCreated;
                _networkManager.OnSessionJoined += HandleSessionJoined;
                
                _isInitialized = true;
            }
            else
            {
                Debug.LogError("[NetworkLobbyManager] Network manager not found");
            }
        }
        
        /// <summary>
        /// Update the lobby manager.
        /// </summary>
        private void Update()
        {
            // Update lobby countdown
            if (_isCountingDown && LobbyState == LobbyState.Countdown)
            {
                LobbyCountdown -= Time.deltaTime;
                OnLobbyCountdownChanged?.Invoke(LobbyCountdown);
                
                if (LobbyCountdown <= 0f)
                {
                    StartGame();
                }
            }
        }
        
        /// <summary>
        /// Clean up when the object is destroyed.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (_networkManager != null)
            {
                // Unsubscribe from network manager events
                _networkManager.OnConnectionStateChanged -= HandleConnectionStateChanged;
                _networkManager.OnPlayerJoined -= HandlePlayerJoined;
                _networkManager.OnPlayerLeft -= HandlePlayerLeft;
                _networkManager.OnSessionCreated -= HandleSessionCreated;
                _networkManager.OnSessionJoined -= HandleSessionJoined;
            }
            
            Debug.Log("[NetworkLobbyManager] Lobby manager destroyed");
        }
        
        /// <summary>
        /// Create a new lobby.
        /// </summary>
        /// <param name="lobbyName">Name of the lobby</param>
        /// <param name="isPrivate">Whether the lobby is private</param>
        public void CreateLobby(string lobbyName, bool isPrivate)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[NetworkLobbyManager] Cannot create lobby: manager not initialized");
                return;
            }
            
            if (LobbyState != LobbyState.Inactive)
            {
                Debug.LogError("[NetworkLobbyManager] Cannot create lobby: already in a lobby");
                return;
            }
            
            Debug.Log($"[NetworkLobbyManager] Creating lobby: {lobbyName}, Private: {isPrivate}");
            
            // Set lobby state to creating
            SetLobbyState(LobbyState.Creating);
            
            // Create a session with the network manager
            _networkManager.CreateSession(lobbyName, _maxPlayersPerTeam * _maxTeams, isPrivate);
        }
        
        /// <summary>
        /// Join an existing lobby.
        /// </summary>
        /// <param name="lobbyId">ID of the lobby to join</param>
        public void JoinLobby(string lobbyId)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[NetworkLobbyManager] Cannot join lobby: manager not initialized");
                return;
            }
            
            if (LobbyState != LobbyState.Inactive)
            {
                Debug.LogError("[NetworkLobbyManager] Cannot join lobby: already in a lobby");
                return;
            }
            
            Debug.Log($"[NetworkLobbyManager] Joining lobby: {lobbyId}");
            
            // Set lobby state to joining
            SetLobbyState(LobbyState.Joining);
            
            // Join the session with the network manager
            _networkManager.JoinSession(lobbyId);
        }
        
        /// <summary>
        /// Join a lobby by invite.
        /// </summary>
        /// <param name="inviteToken">Invite token</param>
        public void JoinLobbyByInvite(string inviteToken)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[NetworkLobbyManager] Cannot join lobby by invite: manager not initialized");
                return;
            }
            
            if (LobbyState != LobbyState.Inactive)
            {
                Debug.LogError("[NetworkLobbyManager] Cannot join lobby by invite: already in a lobby");
                return;
            }
            
            Debug.Log($"[NetworkLobbyManager] Joining lobby by invite: {inviteToken}");
            
            // Set lobby state to joining
            SetLobbyState(LobbyState.Joining);
            
            // Join the session by invite with the network manager
            _networkManager.JoinSessionByInvite(inviteToken);
        }
        
        /// <summary>
        /// Leave the current lobby.
        /// </summary>
        public void LeaveLobby()
        {
            if (!_isInitialized)
            {
                Debug.LogError("[NetworkLobbyManager] Cannot leave lobby: manager not initialized");
                return;
            }
            
            if (LobbyState == LobbyState.Inactive)
            {
                Debug.LogError("[NetworkLobbyManager] Cannot leave lobby: not in a lobby");
                return;
            }
            
            Debug.Log("[NetworkLobbyManager] Leaving lobby");
            
            // Set lobby state to leaving
            SetLobbyState(LobbyState.Leaving);
            
            // Leave the session with the network manager
            _networkManager.LeaveSession();
            
            // Reset lobby state
            ResetLobby();
        }
        
        /// <summary>
        /// Start the lobby countdown.
        /// </summary>
        public void StartCountdown()
        {
            if (!_isInitialized)
            {
                Debug.LogError("[NetworkLobbyManager] Cannot start countdown: manager not initialized");
                return;
            }
            
            if (LobbyState != LobbyState.Ready)
            {
                Debug.LogError("[NetworkLobbyManager] Cannot start countdown: lobby not ready");
                return;
            }
            
            // Check if we have enough players
            if (_lobbyPlayers.Count < _minPlayersToStart)
            {
                Debug.LogError($"[NetworkLobbyManager] Cannot start countdown: not enough players ({_lobbyPlayers.Count}/{_minPlayersToStart})");
                return;
            }
            
            Debug.Log("[NetworkLobbyManager] Starting countdown");
            
            // Set lobby state to countdown
            SetLobbyState(LobbyState.Countdown);
            
            // Start the countdown
            LobbyCountdown = _lobbyCountdownTime;
            _isCountingDown = true;
            OnLobbyCountdownChanged?.Invoke(LobbyCountdown);
        }
        
        /// <summary>
        /// Cancel the lobby countdown.
        /// </summary>
        public void CancelCountdown()
        {
            if (!_isInitialized)
            {
                Debug.LogError("[NetworkLobbyManager] Cannot cancel countdown: manager not initialized");
                return;
            }
            
            if (LobbyState != LobbyState.Countdown)
            {
                Debug.LogError("[NetworkLobbyManager] Cannot cancel countdown: not counting down");
                return;
            }
            
            Debug.Log("[NetworkLobbyManager] Canceling countdown");
            
            // Set lobby state to ready
            SetLobbyState(LobbyState.Ready);
            
            // Stop the countdown
            _isCountingDown = false;
            LobbyCountdown = _lobbyCountdownTime;
            OnLobbyCountdownChanged?.Invoke(LobbyCountdown);
        }
        
        /// <summary>
        /// Change the player's team.
        /// </summary>
        /// <param name="teamId">The team ID to change to</param>
        public void ChangeTeam(int teamId)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[NetworkLobbyManager] Cannot change team: manager not initialized");
                return;
            }
            
            if (LobbyState != LobbyState.Ready && LobbyState != LobbyState.Waiting)
            {
                Debug.LogError("[NetworkLobbyManager] Cannot change team: lobby not ready or waiting");
                return;
            }
            
            if (teamId < 0 || teamId >= _maxTeams)
            {
                Debug.LogError($"[NetworkLobbyManager] Cannot change team: invalid team ID ({teamId})");
                return;
            }
            
            Debug.Log($"[NetworkLobbyManager] Changing team to {teamId}");
            
            // Get the local player
            var localPlayer = _networkManager.LocalPlayer;
            if (localPlayer == null)
            {
                Debug.LogError("[NetworkLobbyManager] Cannot change team: local player not found");
                return;
            }
            
            // Check if the team is full
            int teamCount = 0;
            foreach (var player in _lobbyPlayers)
            {
                if (player.TeamId == teamId)
                {
                    teamCount++;
                }
            }
            
            if (teamCount >= _maxPlayersPerTeam)
            {
                Debug.LogError($"[NetworkLobbyManager] Cannot change team: team {teamId} is full");
                return;
            }
            
            // Change the team
            localPlayer.TeamId = teamId;
            
            // Notify other players
            // TODO: Implement network message to notify other players
            
            // Trigger event
            OnPlayerChangedTeam?.Invoke(localPlayer, teamId);
        }
        
        /// <summary>
        /// Change the player's character.
        /// </summary>
        /// <param name="characterType">The character type to change to</param>
        public void ChangeCharacter(int characterType)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[NetworkLobbyManager] Cannot change character: manager not initialized");
                return;
            }
            
            if (LobbyState != LobbyState.Ready && LobbyState != LobbyState.Waiting)
            {
                Debug.LogError("[NetworkLobbyManager] Cannot change character: lobby not ready or waiting");
                return;
            }
            
            Debug.Log($"[NetworkLobbyManager] Changing character to {characterType}");
            
            // Get the local player
            var localPlayer = _networkManager.LocalPlayer;
            if (localPlayer == null)
            {
                Debug.LogError("[NetworkLobbyManager] Cannot change character: local player not found");
                return;
            }
            
            // Change the character
            localPlayer.CharacterType = characterType;
            
            // Notify other players
            // TODO: Implement network message to notify other players
            
            // Trigger event
            OnPlayerChangedCharacter?.Invoke(localPlayer, characterType);
        }
        
        /// <summary>
        /// Change the game mode.
        /// </summary>
        /// <param name="gameMode">The game mode to change to</param>
        public void ChangeGameMode(string gameMode)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[NetworkLobbyManager] Cannot change game mode: manager not initialized");
                return;
            }
            
            if (LobbyState != LobbyState.Ready && LobbyState != LobbyState.Waiting)
            {
                Debug.LogError("[NetworkLobbyManager] Cannot change game mode: lobby not ready or waiting");
                return;
            }
            
            // Only the host can change the game mode
            if (!_networkManager.LocalPlayer.IsHost)
            {
                Debug.LogError("[NetworkLobbyManager] Cannot change game mode: not the host");
                return;
            }
            
            Debug.Log($"[NetworkLobbyManager] Changing game mode to {gameMode}");
            
            // Change the game mode
            GameMode = gameMode;
            
            // Notify other players
            // TODO: Implement network message to notify other players
            
            // Trigger event
            OnGameModeChanged?.Invoke(GameMode);
        }
        
        /// <summary>
        /// Change the map.
        /// </summary>
        /// <param name="mapName">The map name to change to</param>
        public void ChangeMap(string mapName)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[NetworkLobbyManager] Cannot change map: manager not initialized");
                return;
            }
            
            if (LobbyState != LobbyState.Ready && LobbyState != LobbyState.Waiting)
            {
                Debug.LogError("[NetworkLobbyManager] Cannot change map: lobby not ready or waiting");
                return;
            }
            
            // Only the host can change the map
            if (!_networkManager.LocalPlayer.IsHost)
            {
                Debug.LogError("[NetworkLobbyManager] Cannot change map: not the host");
                return;
            }
            
            Debug.Log($"[NetworkLobbyManager] Changing map to {mapName}");
            
            // Change the map
            MapName = mapName;
            
            // Notify other players
            // TODO: Implement network message to notify other players
            
            // Trigger event
            OnMapChanged?.Invoke(MapName);
        }
        
        /// <summary>
        /// Start the game.
        /// </summary>
        private void StartGame()
        {
            if (!_isInitialized)
            {
                Debug.LogError("[NetworkLobbyManager] Cannot start game: manager not initialized");
                return;
            }
            
            if (LobbyState != LobbyState.Countdown)
            {
                Debug.LogError("[NetworkLobbyManager] Cannot start game: not in countdown state");
                return;
            }
            
            Debug.Log("[NetworkLobbyManager] Starting game");
            
            // Set lobby state to starting
            SetLobbyState(LobbyState.Starting);
            
            // Start the session with the network manager
            _networkManager.StartSession();
            
            // Load the game scene
            // TODO: Implement scene loading
            
            // Set lobby state to in game
            SetLobbyState(LobbyState.InGame);
        }
        
        /// <summary>
        /// Handle connection state changed event.
        /// </summary>
        /// <param name="state">The new connection state</param>
        private void HandleConnectionStateChanged(NetworkConnectionState state)
        {
            Debug.Log($"[NetworkLobbyManager] Connection state changed to {state}");
            
            switch (state)
            {
                case NetworkConnectionState.Connected:
                    // If we were joining or creating, set lobby state to waiting
                    if (LobbyState == LobbyState.Joining || LobbyState == LobbyState.Creating)
                    {
                        SetLobbyState(LobbyState.Waiting);
                    }
                    break;
                
                case NetworkConnectionState.Disconnected:
                case NetworkConnectionState.Failed:
                    // Reset lobby state
                    ResetLobby();
                    break;
            }
        }
        
        /// <summary>
        /// Handle player joined event.
        /// </summary>
        /// <param name="player">The player that joined</param>
        private void HandlePlayerJoined(NetworkPlayer player)
        {
            Debug.Log($"[NetworkLobbyManager] Player joined: {player.DisplayName} ({player.PlayerId})");
            
            // Add the player to the lobby
            if (!_lobbyPlayers.Contains(player))
            {
                _lobbyPlayers.Add(player);
                OnPlayerJoinedLobby?.Invoke(player);
            }
            
            // Check if we have enough players to start
            CheckLobbyReadiness();
        }
        
        /// <summary>
        /// Handle player left event.
        /// </summary>
        /// <param name="player">The player that left</param>
        private void HandlePlayerLeft(NetworkPlayer player)
        {
            Debug.Log($"[NetworkLobbyManager] Player left: {player.DisplayName} ({player.PlayerId})");
            
            // Remove the player from the lobby
            if (_lobbyPlayers.Contains(player))
            {
                _lobbyPlayers.Remove(player);
                OnPlayerLeftLobby?.Invoke(player);
            }
            
            // Check if we still have enough players to start
            CheckLobbyReadiness();
            
            // If the host left, handle host migration
            if (player.IsHost)
            {
                // This is handled by the network manager
            }
        }
        
        /// <summary>
        /// Handle session created event.
        /// </summary>
        /// <param name="success">Whether the session was created successfully</param>
        /// <param name="sessionId">The session ID</param>
        private void HandleSessionCreated(bool success, string sessionId)
        {
            if (success)
            {
                Debug.Log($"[NetworkLobbyManager] Session created: {sessionId}");
                
                // Set lobby state to waiting
                SetLobbyState(LobbyState.Waiting);
            }
            else
            {
                Debug.LogError($"[NetworkLobbyManager] Failed to create session: {sessionId}");
                
                // Reset lobby state
                ResetLobby();
            }
        }
        
        /// <summary>
        /// Handle session joined event.
        /// </summary>
        /// <param name="success">Whether the session was joined successfully</param>
        /// <param name="sessionId">The session ID</param>
        private void HandleSessionJoined(bool success, string sessionId)
        {
            if (success)
            {
                Debug.Log($"[NetworkLobbyManager] Session joined: {sessionId}");
                
                // Set lobby state to waiting
                SetLobbyState(LobbyState.Waiting);
            }
            else
            {
                Debug.LogError($"[NetworkLobbyManager] Failed to join session: {sessionId}");
                
                // Reset lobby state
                ResetLobby();
            }
        }
        
        /// <summary>
        /// Check if the lobby is ready to start.
        /// </summary>
        private void CheckLobbyReadiness()
        {
            // If we're not in the waiting state, don't check
            if (LobbyState != LobbyState.Waiting)
            {
                return;
            }
            
            // Check if we have enough players
            if (_lobbyPlayers.Count >= _minPlayersToStart)
            {
                // Set lobby state to ready
                SetLobbyState(LobbyState.Ready);
            }
            else
            {
                // Set lobby state to waiting
                SetLobbyState(LobbyState.Waiting);
            }
        }
        
        /// <summary>
        /// Reset the lobby state.
        /// </summary>
        private void ResetLobby()
        {
            // Clear the lobby players
            _lobbyPlayers.Clear();
            
            // Reset the countdown
            _isCountingDown = false;
            LobbyCountdown = _lobbyCountdownTime;
            
            // Reset the game mode and map
            GameMode = _defaultGameMode;
            MapName = _defaultMapName;
            
            // Set lobby state to inactive
            SetLobbyState(LobbyState.Inactive);
        }
        
        /// <summary>
        /// Set the lobby state and trigger the event.
        /// </summary>
        /// <param name="state">The new lobby state</param>
        private void SetLobbyState(LobbyState state)
        {
            if (LobbyState != state)
            {
                LobbyState = state;
                OnLobbyStateChanged?.Invoke(LobbyState);
            }
        }
    }
    
    /// <summary>
    /// Enum for lobby states.
    /// </summary>
    public enum LobbyState
    {
        Inactive,
        Creating,
        Joining,
        Waiting,
        Ready,
        Countdown,
        Starting,
        InGame,
        Leaving
    }
}
