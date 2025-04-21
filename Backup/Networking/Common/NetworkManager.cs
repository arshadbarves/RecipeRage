using System;
using System.Collections.Generic;
using RecipeRage.Core.Networking.Interfaces;
using RecipeRage.Core.Networking.EOS;
using RecipeRage.Core.Patterns;
using UnityEngine;

namespace RecipeRage.Core.Networking.Common
{
    /// <summary>
    /// Manages network functionality for the game.
    /// </summary>
    public class NetworkManager : MonoBehaviourSingleton<NetworkManager>
    {
        [Header("Network Settings")]
        [SerializeField] private bool _autoInitialize = true;
        [SerializeField] private float _reconnectDelay = 5f;
        [SerializeField] private int _maxReconnectAttempts = 3;

        /// <summary>
        /// Event triggered when the connection state changes.
        /// </summary>
        public event Action<NetworkConnectionState> OnConnectionStateChanged;

        /// <summary>
        /// Event triggered when a player joins the session.
        /// </summary>
        public event Action<NetworkPlayer> OnPlayerJoined;

        /// <summary>
        /// Event triggered when a player leaves the session.
        /// </summary>
        public event Action<NetworkPlayer> OnPlayerLeft;

        /// <summary>
        /// Event triggered when the session is created.
        /// </summary>
        public event Action<bool, string> OnSessionCreated;

        /// <summary>
        /// Event triggered when the session is joined.
        /// </summary>
        public event Action<bool, string> OnSessionJoined;

        /// <summary>
        /// The current connection state.
        /// </summary>
        public NetworkConnectionState ConnectionState => _networkService?.ConnectionState ?? NetworkConnectionState.Disconnected;

        /// <summary>
        /// The local player.
        /// </summary>
        public NetworkPlayer LocalPlayer => _networkService?.LocalPlayer;

        /// <summary>
        /// The list of connected players.
        /// </summary>
        public IReadOnlyList<NetworkPlayer> ConnectedPlayers => _networkService?.ConnectedPlayers;

        /// <summary>
        /// The network service.
        /// </summary>
        private INetworkService _networkService;

        /// <summary>
        /// Flag to track if the manager is initialized.
        /// </summary>
        private bool _isInitialized;

        /// <summary>
        /// The current reconnect attempt.
        /// </summary>
        private int _reconnectAttempt;

        /// <summary>
        /// Timer for reconnection.
        /// </summary>
        private float _reconnectTimer;

        /// <summary>
        /// Flag to track if reconnection is in progress.
        /// </summary>
        private bool _isReconnecting;

        /// <summary>
        /// Initialize the network manager.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // Create the network service
            _networkService = new EOSNetworkService();

            // Subscribe to network service events
            SubscribeToNetworkServiceEvents();

            // Register the network manager with the service locator
            ServiceLocator.Instance.Register<NetworkManager>(this);

            Debug.Log("[NetworkManager] Network manager initialized");
        }

        /// <summary>
        /// Initialize the network service if auto-initialize is enabled.
        /// </summary>
        private void Start()
        {
            if (_autoInitialize)
            {
                InitializeNetworkService();
            }
        }

        /// <summary>
        /// Update the network manager.
        /// </summary>
        private void Update()
        {
            // Handle reconnection
            if (_isReconnecting)
            {
                _reconnectTimer -= Time.deltaTime;

                if (_reconnectTimer <= 0f)
                {
                    AttemptReconnect();
                }
            }
        }

        /// <summary>
        /// Clean up when the object is destroyed.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Unsubscribe from network service events
            UnsubscribeFromNetworkServiceEvents();

            // Shutdown the network service
            _networkService?.Shutdown();

            Debug.Log("[NetworkManager] Network manager destroyed");
        }

        /// <summary>
        /// Initialize the network service.
        /// </summary>
        /// <param name="callback">Callback when initialization is complete</param>
        public void InitializeNetworkService(Action<bool> callback = null)
        {
            if (_isInitialized)
            {
                callback?.Invoke(true);
                return;
            }

            Debug.Log("[NetworkManager] Initializing network service");

            _networkService.Initialize(success =>
            {
                _isInitialized = success;

                if (success)
                {
                    Debug.Log("[NetworkManager] Network service initialized successfully");
                }
                else
                {
                    Debug.LogError("[NetworkManager] Failed to initialize network service");
                }

                callback?.Invoke(success);
            });
        }

        /// <summary>
        /// Create a new session.
        /// </summary>
        /// <param name="sessionName">Name of the session</param>
        /// <param name="maxPlayers">Maximum number of players</param>
        /// <param name="isPrivate">Whether the session is private</param>
        public void CreateSession(string sessionName, int maxPlayers, bool isPrivate)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[NetworkManager] Cannot create session: network service not initialized");
                OnSessionCreated?.Invoke(false, "Network service not initialized");
                return;
            }

            Debug.Log($"[NetworkManager] Creating session: {sessionName}, Max Players: {maxPlayers}, Private: {isPrivate}");

            _networkService.CreateSession(sessionName, maxPlayers, isPrivate);
        }

        /// <summary>
        /// Join an existing session.
        /// </summary>
        /// <param name="sessionId">ID of the session to join</param>
        public void JoinSession(string sessionId)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[NetworkManager] Cannot join session: network service not initialized");
                OnSessionJoined?.Invoke(false, "Network service not initialized");
                return;
            }

            Debug.Log($"[NetworkManager] Joining session: {sessionId}");

            _networkService.JoinSession(sessionId);
        }

        /// <summary>
        /// Join an existing session by invite.
        /// </summary>
        /// <param name="inviteToken">Invite token</param>
        public void JoinSessionByInvite(string inviteToken)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[NetworkManager] Cannot join session by invite: network service not initialized");
                OnSessionJoined?.Invoke(false, "Network service not initialized");
                return;
            }

            Debug.Log($"[NetworkManager] Joining session by invite: {inviteToken}");

            _networkService.JoinSessionByInvite(inviteToken);
        }

        /// <summary>
        /// Leave the current session.
        /// </summary>
        public void LeaveSession()
        {
            if (!_isInitialized)
            {
                Debug.LogError("[NetworkManager] Cannot leave session: network service not initialized");
                return;
            }

            Debug.Log("[NetworkManager] Leaving session");

            _networkService.LeaveSession();
        }

        /// <summary>
        /// Start the session.
        /// </summary>
        public void StartSession()
        {
            if (!_isInitialized)
            {
                Debug.LogError("[NetworkManager] Cannot start session: network service not initialized");
                return;
            }

            Debug.Log("[NetworkManager] Starting session");

            _networkService.StartSession();
        }

        /// <summary>
        /// Find available sessions.
        /// </summary>
        /// <param name="callback">Callback with list of sessions</param>
        public void FindSessions(Action<List<NetworkSessionInfo>> callback)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[NetworkManager] Cannot find sessions: network service not initialized");
                callback?.Invoke(new List<NetworkSessionInfo>());
                return;
            }

            Debug.Log("[NetworkManager] Finding sessions");

            _networkService.FindSessions(callback);
        }

        /// <summary>
        /// Send data to all connected players.
        /// </summary>
        /// <param name="messageType">Type of message to send</param>
        /// <param name="data">Data to send</param>
        /// <param name="reliable">Whether the data should be sent reliably</param>
        public void SendToAll(byte messageType, byte[] data, bool reliable = true)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[NetworkManager] Cannot send data: network service not initialized");
                return;
            }

            if (ConnectionState != NetworkConnectionState.Connected)
            {
                Debug.LogError("[NetworkManager] Cannot send data: not connected");
                return;
            }

            // Prepend the message type to the data
            byte[] messageData = new byte[data.Length + 1];
            messageData[0] = messageType;
            Array.Copy(data, 0, messageData, 1, data.Length);

            Debug.Log($"[NetworkManager] Sending message type {messageType} to all players");

            _networkService.SendToAll(messageData, reliable);
        }

        /// <summary>
        /// Send data to a specific player.
        /// </summary>
        /// <param name="playerId">ID of the player to send to</param>
        /// <param name="messageType">Type of message to send</param>
        /// <param name="data">Data to send</param>
        /// <param name="reliable">Whether the data should be sent reliably</param>
        public void SendToPlayer(string playerId, byte messageType, byte[] data, bool reliable = true)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[NetworkManager] Cannot send data: network service not initialized");
                return;
            }

            if (ConnectionState != NetworkConnectionState.Connected)
            {
                Debug.LogError("[NetworkManager] Cannot send data: not connected");
                return;
            }

            // Prepend the message type to the data
            byte[] messageData = new byte[data.Length + 1];
            messageData[0] = messageType;
            Array.Copy(data, 0, messageData, 1, data.Length);

            Debug.Log($"[NetworkManager] Sending message type {messageType} to player {playerId}");

            _networkService.SendToPlayer(playerId, messageData, reliable);
        }

        /// <summary>
        /// Register a message handler for a specific message type.
        /// </summary>
        /// <param name="messageType">Type of message to handle</param>
        /// <param name="handler">Handler function</param>
        public void RegisterMessageHandler(byte messageType, Action<NetworkMessage> handler)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[NetworkManager] Cannot register message handler: network service not initialized");
                return;
            }

            Debug.Log($"[NetworkManager] Registering message handler for type {messageType}");

            _networkService.RegisterMessageHandler(messageType, handler);
        }

        /// <summary>
        /// Unregister a message handler.
        /// </summary>
        /// <param name="messageType">Type of message to unregister</param>
        public void UnregisterMessageHandler(byte messageType)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[NetworkManager] Cannot unregister message handler: network service not initialized");
                return;
            }

            Debug.Log($"[NetworkManager] Unregistering message handler for type {messageType}");

            _networkService.UnregisterMessageHandler(messageType);
        }

        /// <summary>
        /// Subscribe to network service events.
        /// </summary>
        private void SubscribeToNetworkServiceEvents()
        {
            if (_networkService == null)
            {
                return;
            }

            _networkService.OnConnectionStateChanged += HandleConnectionStateChanged;
            _networkService.OnPlayerJoined += HandlePlayerJoined;
            _networkService.OnPlayerLeft += HandlePlayerLeft;
            _networkService.OnSessionCreated += HandleSessionCreated;
            _networkService.OnSessionJoined += HandleSessionJoined;
        }

        /// <summary>
        /// Unsubscribe from network service events.
        /// </summary>
        private void UnsubscribeFromNetworkServiceEvents()
        {
            if (_networkService == null)
            {
                return;
            }

            _networkService.OnConnectionStateChanged -= HandleConnectionStateChanged;
            _networkService.OnPlayerJoined -= HandlePlayerJoined;
            _networkService.OnPlayerLeft -= HandlePlayerLeft;
            _networkService.OnSessionCreated -= HandleSessionCreated;
            _networkService.OnSessionJoined -= HandleSessionJoined;
        }

        /// <summary>
        /// Handle connection state changed event.
        /// </summary>
        /// <param name="state">The new connection state</param>
        private void HandleConnectionStateChanged(NetworkConnectionState state)
        {
            Debug.Log($"[NetworkManager] Connection state changed to {state}");

            // Handle reconnection
            if (state == NetworkConnectionState.Disconnected || state == NetworkConnectionState.Failed)
            {
                // Start reconnection if we were previously connected
                if (ConnectionState == NetworkConnectionState.Connected)
                {
                    StartReconnection();
                }
            }
            else if (state == NetworkConnectionState.Connected)
            {
                // Reset reconnection if we're connected
                ResetReconnection();
            }

            // Forward the event
            OnConnectionStateChanged?.Invoke(state);
        }

        /// <summary>
        /// Handle player joined event.
        /// </summary>
        /// <param name="player">The player that joined</param>
        private void HandlePlayerJoined(NetworkPlayer player)
        {
            Debug.Log($"[NetworkManager] Player joined: {player.DisplayName} ({player.PlayerId})");

            // Forward the event
            OnPlayerJoined?.Invoke(player);
        }

        /// <summary>
        /// Handle player left event.
        /// </summary>
        /// <param name="player">The player that left</param>
        private void HandlePlayerLeft(NetworkPlayer player)
        {
            Debug.Log($"[NetworkManager] Player left: {player.DisplayName} ({player.PlayerId})");

            // Forward the event
            OnPlayerLeft?.Invoke(player);
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
                Debug.Log($"[NetworkManager] Session created: {sessionId}");
            }
            else
            {
                Debug.LogError($"[NetworkManager] Failed to create session: {sessionId}");
            }

            // Forward the event
            OnSessionCreated?.Invoke(success, sessionId);
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
                Debug.Log($"[NetworkManager] Session joined: {sessionId}");
            }
            else
            {
                Debug.LogError($"[NetworkManager] Failed to join session: {sessionId}");
            }

            // Forward the event
            OnSessionJoined?.Invoke(success, sessionId);
        }

        /// <summary>
        /// Start the reconnection process.
        /// </summary>
        private void StartReconnection()
        {
            if (_isReconnecting)
            {
                return;
            }

            _isReconnecting = true;
            _reconnectAttempt = 0;
            _reconnectTimer = _reconnectDelay;

            Debug.Log("[NetworkManager] Starting reconnection process");
        }

        /// <summary>
        /// Reset the reconnection process.
        /// </summary>
        private void ResetReconnection()
        {
            _isReconnecting = false;
            _reconnectAttempt = 0;
            _reconnectTimer = 0f;
        }

        /// <summary>
        /// Attempt to reconnect to the session.
        /// </summary>
        private void AttemptReconnect()
        {
            _reconnectAttempt++;

            Debug.Log($"[NetworkManager] Reconnection attempt {_reconnectAttempt}/{_maxReconnectAttempts}");

            if (_reconnectAttempt > _maxReconnectAttempts)
            {
                Debug.LogError("[NetworkManager] Max reconnection attempts reached");
                ResetReconnection();
                return;
            }

            // TODO: Implement actual reconnection logic
            // This is a placeholder implementation

            // Reset the timer for the next attempt
            _reconnectTimer = _reconnectDelay;
        }
    }
}
