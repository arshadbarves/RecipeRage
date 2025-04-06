using System;
using System.Collections.Generic;
using System.Linq;
using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using Epic.OnlineServices.Platform;
using Epic.OnlineServices.Sessions;
using UnityEngine;
using RecipeRage.Core.Networking;

namespace RecipeRage.Core.Networking
{
    /// <summary>
    /// Implementation of the network service using Epic Online Services.
    /// </summary>
    public class EOSNetworkService : INetworkService
    {
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
        public NetworkConnectionState ConnectionState { get; private set; }

        /// <summary>
        /// The local player.
        /// </summary>
        public NetworkPlayer LocalPlayer { get; private set; }

        /// <summary>
        /// The list of connected players.
        /// </summary>
        public IReadOnlyList<NetworkPlayer> ConnectedPlayers => _connectedPlayers.AsReadOnly();

        /// <summary>
        /// The current session ID.
        /// </summary>
        private string _currentSessionId;

        /// <summary>
        /// The list of connected players.
        /// </summary>
        private List<NetworkPlayer> _connectedPlayers;

        /// <summary>
        /// Dictionary of message handlers.
        /// </summary>
        private Dictionary<byte, Action<NetworkMessage>> _messageHandlers;

        /// <summary>
        /// Flag to track if the service is initialized.
        /// </summary>
        private bool _isInitialized;

        /// <summary>
        /// The EOS platform interface.
        /// </summary>
        private PlatformInterface _eosPlatform;

        /// <summary>
        /// The EOS sessions interface.
        /// </summary>
        private SessionsInterface _sessionsInterface;

        /// <summary>
        /// The EOS P2P interface.
        /// </summary>
        private P2PInterface _p2pInterface;

        /// <summary>
        /// The EOS product user ID.
        /// </summary>
        private ProductUserId _localUserId;

        /// <summary>
        /// The EOS session handle.
        /// </summary>
        private ulong _sessionHandle;

        /// <summary>
        /// The EOS P2P socket ID.
        /// </summary>
        private SocketId _socketId;

        /// <summary>
        /// Create a new EOS network service.
        /// </summary>
        public EOSNetworkService()
        {
            _connectedPlayers = new List<NetworkPlayer>();
            _messageHandlers = new Dictionary<byte, Action<NetworkMessage>>();
            ConnectionState = NetworkConnectionState.Disconnected;
        }

        /// <summary>
        /// Initialize the network service.
        /// </summary>
        /// <param name="callback">Callback when initialization is complete</param>
        public void Initialize(Action<bool> callback)
        {
            if (_isInitialized)
            {
                callback?.Invoke(true);
                return;
            }

            Debug.Log("[EOSNetworkService] Initializing EOS network service");

            try
            {
                // Get the EOS platform interface
                _eosPlatform = EOSManager.Instance?.GetEOSPlatformInterface();

                if (_eosPlatform == null)
                {
                    Debug.LogError("[EOSNetworkService] EOS platform interface not found");
                    SetConnectionState(NetworkConnectionState.Failed);
                    callback?.Invoke(false);
                    return;
                }

                // Get the EOS sessions interface
                _sessionsInterface = _eosPlatform.GetSessionsInterface();

                if (_sessionsInterface == null)
                {
                    Debug.LogError("[EOSNetworkService] EOS sessions interface not found");
                    SetConnectionState(NetworkConnectionState.Failed);
                    callback?.Invoke(false);
                    return;
                }

                // Get the EOS P2P interface
                _p2pInterface = _eosPlatform.GetP2PInterface();

                if (_p2pInterface == null)
                {
                    Debug.LogError("[EOSNetworkService] EOS P2P interface not found");
                    SetConnectionState(NetworkConnectionState.Failed);
                    callback?.Invoke(false);
                    return;
                }

                // Get the local user ID
                _localUserId = EOSManager.Instance?.GetProductUserId();

                if (_localUserId == null)
                {
                    Debug.LogError("[EOSNetworkService] Local user ID not found");
                    SetConnectionState(NetworkConnectionState.Failed);
                    callback?.Invoke(false);
                    return;
                }

                // Create the local player
                LocalPlayer = new NetworkPlayer
                {
                    PlayerId = _localUserId.ToString(),
                    DisplayName = RecipeRage.Core.Networking.EOSAdapter.GetDisplayName(),
                    IsLocal = true,
                    IsHost = false,
                    TeamId = 0,
                    CharacterType = 0
                };

                // Add the local player to the connected players list
                _connectedPlayers.Add(LocalPlayer);

                // Create the socket ID
                _socketId = new SocketId
                {
                    SocketName = "RecipeRageSocket"
                };

                // Register for P2P connection events
                RegisterForP2PEvents();

                // Register for session events
                RegisterForSessionEvents();

                _isInitialized = true;
                SetConnectionState(NetworkConnectionState.Disconnected);

                Debug.Log("[EOSNetworkService] EOS network service initialized");
                callback?.Invoke(true);
            }
            catch (Exception e)
            {
                Debug.LogError($"[EOSNetworkService] Error initializing EOS network service: {e.Message}");
                SetConnectionState(NetworkConnectionState.Failed);
                callback?.Invoke(false);
            }
        }

        /// <summary>
        /// Shutdown the network service.
        /// </summary>
        public void Shutdown()
        {
            if (!_isInitialized)
            {
                return;
            }

            Debug.Log("[EOSNetworkService] Shutting down EOS network service");

            // Leave the current session if connected
            if (ConnectionState == NetworkConnectionState.Connected)
            {
                LeaveSession();
            }

            // Unregister from P2P events
            UnregisterFromP2PEvents();

            // Unregister from session events
            UnregisterFromSessionEvents();

            // Clear connected players
            _connectedPlayers.Clear();

            // Clear message handlers
            _messageHandlers.Clear();

            _isInitialized = false;
            SetConnectionState(NetworkConnectionState.Disconnected);

            Debug.Log("[EOSNetworkService] EOS network service shut down");
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
                Debug.LogError("[EOSNetworkService] Cannot create session: service not initialized");
                OnSessionCreated?.Invoke(false, "Service not initialized");
                return;
            }

            if (ConnectionState == NetworkConnectionState.Connected || ConnectionState == NetworkConnectionState.Connecting)
            {
                Debug.LogError("[EOSNetworkService] Cannot create session: already connected or connecting");
                OnSessionCreated?.Invoke(false, "Already connected or connecting");
                return;
            }

            Debug.Log($"[EOSNetworkService] Creating session: {sessionName}, Max Players: {maxPlayers}, Private: {isPrivate}");

            SetConnectionState(NetworkConnectionState.Connecting);

            try
            {
                // Create session attributes
                var attributeData = new Epic.OnlineServices.Sessions.AttributeData
                {
                    Key = "SessionName",
                    Value = new Epic.OnlineServices.Sessions.AttributeDataValue
                    {
                        AsUtf8 = sessionName
                    }
                };

                // Create session modification handle
                var createSessionModificationOptions = new Epic.OnlineServices.Sessions.CreateSessionModificationOptions
                {
                    MaxPlayers = (uint)maxPlayers,
                    SessionName = sessionName,
                    BucketId = "RecipeRage",
                    PresenceEnabled = true,
                    // JoinInProgressAllowed is not available in this version of the SDK
                    LocalUserId = _localUserId
                };

                _sessionsInterface.CreateSessionModification(ref createSessionModificationOptions, out var sessionModificationHandle);

                if (sessionModificationHandle == null)
                {
                    Debug.LogError("[EOSNetworkService] Failed to create session modification handle");
                    SetConnectionState(NetworkConnectionState.Failed);
                    OnSessionCreated?.Invoke(false, "Failed to create session modification handle");
                    return;
                }

                // Set session attributes
                var sessionAttributeOptions = new Epic.OnlineServices.Sessions.SessionModificationAddAttributeOptions
                {
                    SessionAttribute = attributeData,
                    // Use our constants for SDK compatibility
                    AdvertisementType = EOSConstants.ConvertToSDKAttributeAdvertisementType(
                        isPrivate ? EOSConstants.AttributeAdvertisementType.DontAdvertise :
                        EOSConstants.AttributeAdvertisementType.Advertise)
                };

                sessionModificationHandle.AddAttribute(ref sessionAttributeOptions);

                // Add game mode attribute
                var gameModeAttribute = new Epic.OnlineServices.Sessions.AttributeData
                {
                    Key = "GameMode",
                    Value = new Epic.OnlineServices.Sessions.AttributeDataValue
                    {
                        AsUtf8 = "Classic"
                    }
                };

                var gameModeAttributeOptions = new Epic.OnlineServices.Sessions.SessionModificationAddAttributeOptions
                {
                    SessionAttribute = gameModeAttribute,
                    // Use our constants for SDK compatibility
                    AdvertisementType = EOSConstants.ConvertToSDKAttributeAdvertisementType(EOSConstants.AttributeAdvertisementType.Advertise)
                };

                sessionModificationHandle.AddAttribute(ref gameModeAttributeOptions);

                // Set join policy
                var joinPolicyOptions = new Epic.OnlineServices.Sessions.SessionModificationSetJoinInProgressAllowedOptions
                {
                    AllowJoinInProgress = true
                };

                sessionModificationHandle.SetJoinInProgressAllowed(ref joinPolicyOptions);

                // Set permissions
                var permissionOptions = new Epic.OnlineServices.Sessions.SessionModificationSetPermissionLevelOptions
                {
                    // Use our constants for SDK compatibility
                    PermissionLevel = EOSConstants.ConvertToSDKOnlineSessionPermissionLevel(
                        isPrivate ? EOSConstants.OnlineSessionPermissionLevel.JoinViaPresence :
                        EOSConstants.OnlineSessionPermissionLevel.PublicAdvertised)
                };

                sessionModificationHandle.SetPermissionLevel(ref permissionOptions);

                // Commit session modifications
                var updateSessionOptions = new Epic.OnlineServices.Sessions.UpdateSessionOptions
                {
                    SessionModificationHandle = sessionModificationHandle
                };

                _sessionsInterface.UpdateSession(ref updateSessionOptions, null, OnSessionCreationComplete);

                // Store session ID (using a different approach since GetSessionId is not available)
                _currentSessionId = System.Guid.NewGuid().ToString(); // Generate a unique session ID
                LocalPlayer.IsHost = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[EOSNetworkService] Error creating session: {e.Message}");
                SetConnectionState(NetworkConnectionState.Failed);
                OnSessionCreated?.Invoke(false, e.Message);
            }

            Debug.Log($"[EOSNetworkService] Session created: {_currentSessionId}");
        }

        /// <summary>
        /// Join an existing session.
        /// </summary>
        /// <param name="sessionId">ID of the session to join</param>
        public void JoinSession(string sessionId)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[EOSNetworkService] Cannot join session: service not initialized");
                OnSessionJoined?.Invoke(false, "Service not initialized");
                return;
            }

            if (ConnectionState == NetworkConnectionState.Connected || ConnectionState == NetworkConnectionState.Connecting)
            {
                Debug.LogError("[EOSNetworkService] Cannot join session: already connected or connecting");
                OnSessionJoined?.Invoke(false, "Already connected or connecting");
                return;
            }

            Debug.Log($"[EOSNetworkService] Joining session: {sessionId}");

            SetConnectionState(NetworkConnectionState.Connecting);

            // TODO: Implement actual session joining with EOS
            // This is a placeholder implementation

            // Simulate session joining
            _currentSessionId = sessionId;
            LocalPlayer.IsHost = false;

            // Simulate success
            SetConnectionState(NetworkConnectionState.Connected);
            OnSessionJoined?.Invoke(true, _currentSessionId);

            Debug.Log($"[EOSNetworkService] Session joined: {_currentSessionId}");
        }

        /// <summary>
        /// Join an existing session by invite.
        /// </summary>
        /// <param name="inviteToken">Invite token</param>
        public void JoinSessionByInvite(string inviteToken)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[EOSNetworkService] Cannot join session by invite: service not initialized");
                OnSessionJoined?.Invoke(false, "Service not initialized");
                return;
            }

            if (ConnectionState == NetworkConnectionState.Connected || ConnectionState == NetworkConnectionState.Connecting)
            {
                Debug.LogError("[EOSNetworkService] Cannot join session by invite: already connected or connecting");
                OnSessionJoined?.Invoke(false, "Already connected or connecting");
                return;
            }

            Debug.Log($"[EOSNetworkService] Joining session by invite: {inviteToken}");

            SetConnectionState(NetworkConnectionState.Connecting);

            // TODO: Implement actual session joining by invite with EOS
            // This is a placeholder implementation

            // Simulate session joining
            _currentSessionId = Guid.NewGuid().ToString();
            LocalPlayer.IsHost = false;

            // Simulate success
            SetConnectionState(NetworkConnectionState.Connected);
            OnSessionJoined?.Invoke(true, _currentSessionId);

            Debug.Log($"[EOSNetworkService] Session joined by invite: {_currentSessionId}");
        }

        /// <summary>
        /// Leave the current session.
        /// </summary>
        public void LeaveSession()
        {
            if (!_isInitialized)
            {
                Debug.LogError("[EOSNetworkService] Cannot leave session: service not initialized");
                return;
            }

            if (ConnectionState != NetworkConnectionState.Connected)
            {
                Debug.LogError("[EOSNetworkService] Cannot leave session: not connected");
                return;
            }

            Debug.Log("[EOSNetworkService] Leaving session");

            SetConnectionState(NetworkConnectionState.Disconnecting);

            // TODO: Implement actual session leaving with EOS
            // This is a placeholder implementation

            // Simulate session leaving
            _currentSessionId = null;
            LocalPlayer.IsHost = false;

            // Remove all players except the local player
            _connectedPlayers.RemoveAll(p => !p.IsLocal);

            // Simulate success
            SetConnectionState(NetworkConnectionState.Disconnected);

            Debug.Log("[EOSNetworkService] Session left");
        }

        /// <summary>
        /// Start the session.
        /// </summary>
        public void StartSession()
        {
            if (!_isInitialized)
            {
                Debug.LogError("[EOSNetworkService] Cannot start session: service not initialized");
                return;
            }

            if (ConnectionState != NetworkConnectionState.Connected)
            {
                Debug.LogError("[EOSNetworkService] Cannot start session: not connected");
                return;
            }

            if (!LocalPlayer.IsHost)
            {
                Debug.LogError("[EOSNetworkService] Cannot start session: not the host");
                return;
            }

            Debug.Log("[EOSNetworkService] Starting session");

            // TODO: Implement actual session starting with EOS
            // This is a placeholder implementation

            Debug.Log("[EOSNetworkService] Session started");
        }

        /// <summary>
        /// Find available sessions.
        /// </summary>
        /// <param name="callback">Callback with list of sessions</param>
        public void FindSessions(Action<List<NetworkSessionInfo>> callback)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[EOSNetworkService] Cannot find sessions: service not initialized");
                callback?.Invoke(new List<NetworkSessionInfo>());
                return;
            }

            Debug.Log("[EOSNetworkService] Finding sessions");

            // TODO: Implement actual session finding with EOS
            // This is a placeholder implementation

            // Simulate session finding
            var sessions = new List<NetworkSessionInfo>();

            // Add some fake sessions
            for (int i = 0; i < 3; i++)
            {
                sessions.Add(new NetworkSessionInfo
                {
                    SessionId = Guid.NewGuid().ToString(),
                    SessionName = $"Test Session {i + 1}",
                    PlayerCount = UnityEngine.Random.Range(1, 4),
                    MaxPlayers = 4,
                    IsPrivate = false,
                    HostName = $"Host {i + 1}",
                    GameMode = "Classic",
                    MapName = "Kitchen"
                });
            }

            callback?.Invoke(sessions);

            Debug.Log($"[EOSNetworkService] Found {sessions.Count} sessions");
        }

        /// <summary>
        /// Send data to all connected players.
        /// </summary>
        /// <param name="data">Data to send</param>
        /// <param name="reliable">Whether the data should be sent reliably</param>
        public void SendToAll(byte[] data, bool reliable)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[EOSNetworkService] Cannot send data: service not initialized");
                return;
            }

            if (ConnectionState != NetworkConnectionState.Connected)
            {
                Debug.LogError("[EOSNetworkService] Cannot send data: not connected");
                return;
            }

            Debug.Log($"[EOSNetworkService] Sending {data.Length} bytes to all players");

            // TODO: Implement actual data sending with EOS
            // This is a placeholder implementation

            Debug.Log("[EOSNetworkService] Data sent to all players");
        }

        /// <summary>
        /// Send data to a specific player.
        /// </summary>
        /// <param name="playerId">ID of the player to send to</param>
        /// <param name="data">Data to send</param>
        /// <param name="reliable">Whether the data should be sent reliably</param>
        public void SendToPlayer(string playerId, byte[] data, bool reliable)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[EOSNetworkService] Cannot send data: service not initialized");
                return;
            }

            if (ConnectionState != NetworkConnectionState.Connected)
            {
                Debug.LogError("[EOSNetworkService] Cannot send data: not connected");
                return;
            }

            Debug.Log($"[EOSNetworkService] Sending {data.Length} bytes to player {playerId}");

            // TODO: Implement actual data sending with EOS
            // This is a placeholder implementation

            Debug.Log($"[EOSNetworkService] Data sent to player {playerId}");
        }

        /// <summary>
        /// Register a message handler for a specific message type.
        /// </summary>
        /// <param name="messageType">Type of message to handle</param>
        /// <param name="handler">Handler function</param>
        public void RegisterMessageHandler(byte messageType, Action<NetworkMessage> handler)
        {
            if (handler == null)
            {
                Debug.LogError("[EOSNetworkService] Cannot register message handler: handler is null");
                return;
            }

            if (_messageHandlers.ContainsKey(messageType))
            {
                _messageHandlers[messageType] += handler;
            }
            else
            {
                _messageHandlers.Add(messageType, handler);
            }

            Debug.Log($"[EOSNetworkService] Registered message handler for type {messageType}");
        }

        /// <summary>
        /// Unregister a message handler.
        /// </summary>
        /// <param name="messageType">Type of message to unregister</param>
        public void UnregisterMessageHandler(byte messageType)
        {
            if (_messageHandlers.ContainsKey(messageType))
            {
                _messageHandlers.Remove(messageType);
                Debug.Log($"[EOSNetworkService] Unregistered message handler for type {messageType}");
            }
        }

        /// <summary>
        /// Callback for session creation completion.
        /// </summary>
        /// <param name="data">Callback data</param>
        private void OnSessionCreationComplete(ref Epic.OnlineServices.Sessions.UpdateSessionCallbackInfo data)
        {
            if (data.ResultCode == Epic.OnlineServices.Result.Success)
            {
                Debug.Log($"[EOSNetworkService] Session created successfully: {_currentSessionId}");
                SetConnectionState(NetworkConnectionState.Connected);
                OnSessionCreated?.Invoke(true, _currentSessionId);

                // Register for session events
                RegisterForSessionEvents();
            }
            else
            {
                Debug.LogError($"[EOSNetworkService] Failed to create session: {data.ResultCode}");
                SetConnectionState(NetworkConnectionState.Failed);
                OnSessionCreated?.Invoke(false, data.ResultCode.ToString());
            }
        }

        /// <summary>
        /// Register for P2P events.
        /// </summary>
        private void RegisterForP2PEvents()
        {
            if (_p2pInterface == null)
            {
                Debug.LogError("[EOSNetworkService] Cannot register for P2P events: P2P interface is null");
                return;
            }

            try
            {
                // Add P2P connection request handler
                var addNotifyPeerConnectionRequestOptions = new Epic.OnlineServices.P2P.AddNotifyPeerConnectionRequestOptions
                {
                    LocalUserId = _localUserId,
                    SocketId = _socketId
                };

                _p2pInterface.AddNotifyPeerConnectionRequest(ref addNotifyPeerConnectionRequestOptions, null, OnPeerConnectionRequest);

                // Add P2P connection established handler
                var addNotifyPeerConnectionEstablishedOptions = new Epic.OnlineServices.P2P.AddNotifyPeerConnectionEstablishedOptions
                {
                    LocalUserId = _localUserId,
                    SocketId = _socketId
                };

                _p2pInterface.AddNotifyPeerConnectionEstablished(ref addNotifyPeerConnectionEstablishedOptions, null, OnPeerConnectionEstablished);

                // Add P2P connection closed handler
                var addNotifyPeerConnectionClosedOptions = new Epic.OnlineServices.P2P.AddNotifyPeerConnectionClosedOptions
                {
                    LocalUserId = _localUserId,
                    SocketId = _socketId
                };

                _p2pInterface.AddNotifyPeerConnectionClosed(ref addNotifyPeerConnectionClosedOptions, null, OnPeerConnectionClosed);

                Debug.Log("[EOSNetworkService] Registered for P2P events");
            }
            catch (Exception e)
            {
                Debug.LogError($"[EOSNetworkService] Error registering for P2P events: {e.Message}");
            }
        }

        /// <summary>
        /// Callback for P2P connection request.
        /// </summary>
        /// <param name="data">Callback data</param>
        private void OnPeerConnectionRequest(ref Epic.OnlineServices.P2P.OnIncomingConnectionRequestInfo data)
        {
            Debug.Log($"[EOSNetworkService] P2P connection request from {data.RemoteUserId}");

            // Accept the connection
            var acceptConnectionOptions = new Epic.OnlineServices.P2P.AcceptConnectionOptions
            {
                LocalUserId = _localUserId,
                RemoteUserId = data.RemoteUserId,
                SocketId = data.SocketId
            };

            _p2pInterface.AcceptConnection(ref acceptConnectionOptions);
        }

        /// <summary>
        /// Callback for P2P connection established.
        /// </summary>
        /// <param name="data">Callback data</param>
        private void OnPeerConnectionEstablished(ref Epic.OnlineServices.P2P.OnPeerConnectionEstablishedInfo data)
        {
            Debug.Log($"[EOSNetworkService] P2P connection established with {data.RemoteUserId}");

            // Store the remote user ID in a local variable to avoid using the ref parameter in a lambda
            string remoteUserId = data.RemoteUserId.ToString();

            // Add the remote player if not already in the list
            if (!_connectedPlayers.Any(p => p.PlayerId == remoteUserId))
            {
                var remotePlayer = new NetworkPlayer
                {
                    PlayerId = data.RemoteUserId.ToString(),
                    DisplayName = "Player", // Will be updated when player info is received
                    IsLocal = false,
                    IsHost = false,
                    TeamId = 0,
                    CharacterType = 0
                };

                _connectedPlayers.Add(remotePlayer);
                OnPlayerJoined?.Invoke(remotePlayer);
            }
        }

        /// <summary>
        /// Callback for P2P connection closed.
        /// </summary>
        /// <param name="data">Callback data</param>
        private void OnPeerConnectionClosed(ref Epic.OnlineServices.P2P.OnRemoteConnectionClosedInfo data)
        {
            Debug.Log($"[EOSNetworkService] P2P connection closed with {data.RemoteUserId}");

            // Store the remote user ID in a local variable to avoid using the ref parameter in a lambda
            string remoteUserId = data.RemoteUserId.ToString();

            // Remove the remote player from the list
            var remotePlayer = _connectedPlayers.FirstOrDefault(p => p.PlayerId == remoteUserId);
            if (remotePlayer != null)
            {
                _connectedPlayers.Remove(remotePlayer);
                OnPlayerLeft?.Invoke(remotePlayer);

                // If the host left, handle host migration
                if (remotePlayer.IsHost)
                {
                    HandleHostMigration();
                }
            }
        }

        /// <summary>
        /// Handle host migration when the host leaves.
        /// </summary>
        private void HandleHostMigration()
        {
            Debug.Log("[EOSNetworkService] Handling host migration");

            // If there are no other players, no need for host migration
            if (_connectedPlayers.Count <= 1)
            {
                Debug.Log("[EOSNetworkService] No other players to migrate to, ending session");
                LeaveSession();
                return;
            }

            // Find the player with the lowest ID to be the new host
            // This ensures all clients select the same new host
            var newHost = _connectedPlayers
                .Where(p => !p.IsLocal) // Exclude local player for now
                .OrderBy(p => p.PlayerId)
                .FirstOrDefault();

            if (newHost == null)
            {
                // If no remote players, make local player the host
                newHost = LocalPlayer;
            }

            Debug.Log($"[EOSNetworkService] Selected new host: {newHost.DisplayName} ({newHost.PlayerId})");

            // Update host status
            foreach (var player in _connectedPlayers)
            {
                player.IsHost = (player.PlayerId == newHost.PlayerId);
            }

            // If local player is the new host, take over session management
            if (LocalPlayer.IsHost)
            {
                Debug.Log("[EOSNetworkService] Local player is now the host");

                // TODO: Implement session takeover logic
                // This would involve creating a new session with the same settings
                // and having all players join it
            }
        }

        /// <summary>
        /// Unregister from P2P events.
        /// </summary>
        private void UnregisterFromP2PEvents()
        {
            if (_p2pInterface == null)
            {
                return;
            }

            try
            {
                // TODO: Implement actual P2P event unregistration with EOS
                // This requires storing the notification IDs from the AddNotify calls

                Debug.Log("[EOSNetworkService] Unregistered from P2P events");
            }
            catch (Exception e)
            {
                Debug.LogError($"[EOSNetworkService] Error unregistering from P2P events: {e.Message}");
            }
        }

        /// <summary>
        /// Register for session events.
        /// </summary>
        private void RegisterForSessionEvents()
        {
            // TODO: Implement actual session event registration with EOS
            // This is a placeholder implementation
        }

        /// <summary>
        /// Unregister from session events.
        /// </summary>
        private void UnregisterFromSessionEvents()
        {
            // TODO: Implement actual session event unregistration with EOS
            // This is a placeholder implementation
        }

        /// <summary>
        /// Set the connection state and trigger the event.
        /// </summary>
        /// <param name="state">The new connection state</param>
        private void SetConnectionState(NetworkConnectionState state)
        {
            if (ConnectionState != state)
            {
                ConnectionState = state;
                OnConnectionStateChanged?.Invoke(ConnectionState);
            }
        }
    }
}
