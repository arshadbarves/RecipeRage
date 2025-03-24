using System;
using System.Collections.Generic;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using PlayEveryWare.EpicOnlineServices;
using RecipeRage.Modules.Friends.Interfaces;
using UnityEngine;

namespace RecipeRage.Modules.Friends.Network
{
    /// <summary>
    /// Represents a received message to be processed
    /// </summary>
    public struct ReceivedMessageData
    {
        /// <summary>
        /// ID of the sender
        /// </summary>
        public string SenderId;

        /// <summary>
        /// Type of message
        /// </summary>
        public FriendsMessageType MessageType;

        /// <summary>
        /// Message payload
        /// </summary>
        public byte[] Payload;
    }

    /// <summary>
    /// Implementation of the P2P network service using EOS P2P
    /// Complexity Rating: 4
    /// </summary>
    public class EOSP2PNetworkService : IP2PNetworkService
    {
        private const string SOCKET_NAME = "FRIENDS";
        private const byte CHANNEL_RELIABLE = 0;
        private const byte CHANNEL_UNRELIABLE = 1;
        private readonly Dictionary<string, bool> _connectionStatus;
        private readonly Queue<ReceivedMessageData> _messageQueue;
        private ulong _connectionNotificationId;

        private bool _isInitialized;

        private P2PInterface _p2pInterface;

        /// <summary>
        /// Constructor
        /// </summary>
        public EOSP2PNetworkService()
        {
            _connectionStatus = new Dictionary<string, bool>();
            _messageQueue = new Queue<ReceivedMessageData>();
        }

        /// <summary>
        /// Event triggered when a message is received from a peer
        /// </summary>
        public event Action<string, byte[]> OnMessageReceived;

        /// <summary>
        /// Event triggered when a connection status changes
        /// </summary>
        public event Action<string, bool> OnConnectionStatusChanged;

        /// <summary>
        /// Initialize the P2P network service
        /// </summary>
        /// <param name="onComplete"> Callback when initialization is complete </param>
        public void Initialize(Action<bool> onComplete = null)
        {
            if (_isInitialized)
            {
                Debug.LogWarning("EOSP2PNetworkService: Already initialized");
                onComplete?.Invoke(true);
                return;
            }

            Debug.Log("EOSP2PNetworkService: Initializing...");

            if (EOSManager.Instance == null)
            {
                Debug.LogError("EOSP2PNetworkService: EOSManager not found");
                onComplete?.Invoke(false);
                return;
            }

            // Get the P2P interface from EOS
            _p2pInterface = EOSManager.Instance.GetEOSPlatformInterface().GetP2PInterface();

            if (_p2pInterface == null)
            {
                Debug.LogError("EOSP2PNetworkService: Failed to get P2P interface");
                onComplete?.Invoke(false);
                return;
            }

            // Subscribe to connection requests
            SubscribeToConnectionRequests();

            // Query NAT type
            RefreshNATType();

            _isInitialized = true;
            Debug.Log("EOSP2PNetworkService: Initialized successfully");
            onComplete?.Invoke(true);
        }

        /// <summary>
        /// Connect to a peer
        /// </summary>
        /// <param name="peerId"> ID of the peer to connect to </param>
        /// <param name="onComplete"> Callback when the connection attempt is complete </param>
        public void Connect(string peerId, Action<bool> onComplete = null)
        {
            if (!_isInitialized)
            {
                Debug.LogError("EOSP2PNetworkService: Not initialized");
                onComplete?.Invoke(false);
                return;
            }

            Debug.Log($"EOSP2PNetworkService: Connecting to peer {peerId}...");

            var remoteUserId = ProductUserId.FromString(peerId);
            if (!remoteUserId.IsValid())
            {
                Debug.LogError($"EOSP2PNetworkService: Invalid remote user ID: {peerId}");
                onComplete?.Invoke(false);
                return;
            }

            // Send an initial connection packet
            var socketId = new SocketId
            {
                SocketName = SOCKET_NAME
            };

            var options = new SendPacketOptions
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RemoteUserId = remoteUserId,
                SocketId = socketId,
                Channel = CHANNEL_RELIABLE,
                Reliability = PacketReliability.ReliableOrdered,
                AllowDelayedDelivery = true,
                Data = new ArraySegment<byte>(FriendsNetworkProtocol.CreatePing())
            };

            var result = _p2pInterface.SendPacket(ref options);

            if (result != Result.Success)
            {
                Debug.LogError($"EOSP2PNetworkService: Failed to send connection packet: {result}");
                onComplete?.Invoke(false);
                return;
            }

            // Mark as connected
            _connectionStatus[peerId] = true;
            OnConnectionStatusChanged?.Invoke(peerId, true);

            Debug.Log($"EOSP2PNetworkService: Connected to peer {peerId}");
            onComplete?.Invoke(true);
        }

        /// <summary>
        /// Disconnect from a peer
        /// </summary>
        /// <param name="peerId"> ID of the peer to disconnect from </param>
        public void Disconnect(string peerId)
        {
            if (!_isInitialized)
            {
                Debug.LogError("EOSP2PNetworkService: Not initialized");
                return;
            }

            Debug.Log($"EOSP2PNetworkService: Disconnecting from peer {peerId}...");

            var remoteUserId = ProductUserId.FromString(peerId);
            if (!remoteUserId.IsValid())
            {
                Debug.LogError($"EOSP2PNetworkService: Invalid remote user ID: {peerId}");
                return;
            }

            // Close the connection with EOS
            var options = new CloseConnectionsOptions
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                SocketId = new SocketId { SocketName = SOCKET_NAME }
            };

            // Set remote user ID if the class has that field
            // This might need to be modified based on your EOS SDK version
            try
            {
                // Try to set the property using reflection if available
                var type = options.GetType();
                var property = type.GetProperty("RemoteUserId");
                if (property != null)
                {
                    property.SetValue(options, remoteUserId);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"EOSP2PNetworkService: Could not set RemoteUserId: {ex.Message}");
            }

            var result = _p2pInterface.CloseConnections(ref options);

            if (result != Result.Success && result != Result.NotFound)
            {
                Debug.LogError($"EOSP2PNetworkService: Failed to close connection: {result}");
                return;
            }

            // Update status
            _connectionStatus[peerId] = false;
            OnConnectionStatusChanged?.Invoke(peerId, false);

            Debug.Log($"EOSP2PNetworkService: Disconnected from peer {peerId}");
        }

        /// <summary>
        /// Send a message to a peer
        /// </summary>
        /// <param name="peerId"> ID of the peer to send to </param>
        /// <param name="data"> Message data </param>
        /// <param name="reliable"> Whether the message should be sent reliably </param>
        /// <param name="onComplete"> Callback when the send operation is complete </param>
        public void SendMessage(string peerId, byte[] data, bool reliable = true, Action<bool> onComplete = null)
        {
            if (!_isInitialized)
            {
                Debug.LogError("EOSP2PNetworkService: Not initialized");
                onComplete?.Invoke(false);
                return;
            }

            var remoteUserId = ProductUserId.FromString(peerId);
            if (!remoteUserId.IsValid())
            {
                Debug.LogError($"EOSP2PNetworkService: Invalid remote user ID: {peerId}");
                onComplete?.Invoke(false);
                return;
            }

            if (data == null || data.Length == 0)
            {
                Debug.LogError("EOSP2PNetworkService: Empty message data");
                onComplete?.Invoke(false);
                return;
            }

            // Send the packet
            var socketId = new SocketId
            {
                SocketName = SOCKET_NAME
            };

            var options = new SendPacketOptions
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RemoteUserId = remoteUserId,
                SocketId = socketId,
                Channel = reliable ? CHANNEL_RELIABLE : CHANNEL_UNRELIABLE,
                Reliability = reliable ? PacketReliability.ReliableOrdered : PacketReliability.UnreliableUnordered,
                AllowDelayedDelivery = reliable,
                Data = new ArraySegment<byte>(data)
            };

            var result = _p2pInterface.SendPacket(ref options);

            if (result != Result.Success)
            {
                Debug.LogError($"EOSP2PNetworkService: Failed to send message: {result}");
                onComplete?.Invoke(false);
                return;
            }

            onComplete?.Invoke(true);
        }

        /// <summary>
        /// Check if connected to a peer
        /// </summary>
        /// <param name="peerId"> ID of the peer </param>
        /// <returns> True if connected to the peer </returns>
        public bool IsConnectedTo(string peerId)
        {
            if (!_isInitialized)
            {
                Debug.LogError("EOSP2PNetworkService: Not initialized");
                return false;
            }

            return _connectionStatus.TryGetValue(peerId, out bool connected) && connected;
        }

        /// <summary>
        /// Get a list of connected peers
        /// </summary>
        /// <returns> List of connected peer IDs </returns>
        public IReadOnlyList<string> GetConnectedPeers()
        {
            if (!_isInitialized)
            {
                Debug.LogError("EOSP2PNetworkService: Not initialized");
                return new List<string>();
            }

            var connectedPeers = new List<string>();
            foreach (KeyValuePair<string, bool> pair in _connectionStatus)
                if (pair.Value)
                    connectedPeers.Add(pair.Key);

            return connectedPeers;
        }

        /// <summary>
        /// Get the NAT type for the current user
        /// </summary>
        /// <returns> NAT type string (Open, Moderate, Strict) </returns>
        public string GetNATType()
        {
            if (!_isInitialized)
            {
                Debug.LogError("EOSP2PNetworkService: Not initialized");
                return "Unknown";
            }

            var options = new GetNATTypeOptions();
            var result = _p2pInterface.GetNATType(ref options, out var natType);

            if (result == Result.NotFound) return "Unknown";

            if (result != Result.Success)
            {
                Debug.LogError($"EOSP2PNetworkService: Failed to get NAT type: {result}");
                return "Unknown";
            }

            switch (natType)
            {
                case NATType.Open:
                    return "Open";
                case NATType.Moderate:
                    return "Moderate";
                case NATType.Strict:
                    return "Strict";
                default:
                    return "Unknown";
            }
        }

        /// <summary>
        /// Close all connections and clean up
        /// </summary>
        public void Shutdown()
        {
            if (!_isInitialized) return;

            Debug.Log("EOSP2PNetworkService: Shutting down...");

            // Disconnect from all peers
            foreach (string peerId in new List<string>(_connectionStatus.Keys))
            {
                Disconnect(peerId);
            }

            // Unsubscribe from connection requests
            UnsubscribeFromConnectionRequests();

            // Close all remaining connections
            try
            {
                var options = new CloseConnectionsOptions
                {
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    SocketId = new SocketId { SocketName = SOCKET_NAME }
                };

                _p2pInterface.CloseConnections(ref options);
            }
            catch (Exception ex)
            {
                Debug.LogError($"EOSP2PNetworkService: Error during shutdown: {ex.Message}");
            }

            _connectionStatus.Clear();
            _messageQueue.Clear();

            _isInitialized = false;

            Debug.Log("EOSP2PNetworkService: Shutdown complete");
        }

        /// <summary>
        /// Process any incoming messages
        /// </summary>
        public void ProcessIncomingMessages()
        {
            if (!_isInitialized || _p2pInterface == null)
                return;

            try
            {
                // Process up to 100 packets per frame to avoid freezing
                for (int i = 0; i < 100; i++)
                {
                    var socketId = new SocketId { SocketName = SOCKET_NAME };

                    var options = new ReceivePacketOptions
                    {
                        LocalUserId = EOSManager.Instance.GetProductUserId(),
                        MaxDataSizeBytes = 4096
                    };

                    // Try to set channel and socket properties if available in this SDK version
                    try
                    {
                        var type = options.GetType();
                        var socketIdProp = type.GetProperty("SocketId");
                        if (socketIdProp != null)
                        {
                            socketIdProp.SetValue(options, socketId);
                        }

                        var channelProp = type.GetProperty("RequiredChannel");
                        if (channelProp != null)
                        {
                            channelProp.SetValue(options, byte.MaxValue); // all channels
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"EOSP2PNetworkService: Error setting properties: {ex.Message}");
                    }

                    ProductUserId peerId = null;
                    byte channel = 0;
                    var dataArray = new byte[4096];
                    var dataSegment = new ArraySegment<byte>(dataArray);
                    uint bytesWritten = 0;

                    // The parameter order and ref/out modifiers might need to be adjusted based on your EOS SDK version
                    var result = _p2pInterface.ReceivePacket(
                        ref options,
                        ref peerId,
                        ref socketId,
                        out channel,
                        dataSegment,
                        out bytesWritten);

                    if (result == Result.Success)
                    {
                        if (peerId != null && peerId.IsValid())
                        {
                            string peerIdString = peerId.ToString();
                            byte[] messageData = new byte[bytesWritten];
                            Array.Copy(dataArray, messageData, bytesWritten);

                            // First byte is message type
                            if (messageData.Length > 0)
                            {
                                // Extract message type and payload
                                FriendsMessageType messageType = (FriendsMessageType)messageData[0];
                                byte[] payload = new byte[messageData.Length - 1];
                                Array.Copy(messageData, 1, payload, 0, payload.Length);

                                // Handle the message
                                HandleMessage(peerIdString, messageType, payload);
                            }
                            else
                            {
                                Debug.LogWarning($"EOSP2PNetworkService: Received empty message from {peerIdString}");
                            }
                        }
                    }
                    else if (result != Result.NotFound)
                    {
                        // No more packets to receive
                        break;
                    }
                    else if (result != Result.Success && result != Result.NotFound)
                    {
                        Debug.LogError($"EOSP2PNetworkService: Error receiving packet: {result}");
                        break;
                    }
                }

                // Process queued messages
                while (_messageQueue.Count > 0)
                {
                    var message = _messageQueue.Dequeue();
                    OnMessageReceived?.Invoke(message.SenderId, message.Payload);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"EOSP2PNetworkService: Error processing messages: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Handle a received message
        /// </summary>
        /// <param name="peerId"> Sender ID </param>
        /// <param name="messageType"> Message type </param>
        /// <param name="payload"> Message payload </param>
        private void HandleMessage(string peerId, FriendsMessageType messageType, byte[] payload)
        {
            // Handle special message types
            switch (messageType)
            {
                case FriendsMessageType.Ping:
                    // Respond with pong
                    SendMessage(peerId, FriendsNetworkProtocol.CreatePong());

                    // Update connection status if needed
                    if (!IsConnectedTo(peerId))
                    {
                        _connectionStatus[peerId] = true;
                        OnConnectionStatusChanged?.Invoke(peerId, true);
                    }

                    return;

                case FriendsMessageType.Pong:
                    // Update connection status if needed
                    if (!IsConnectedTo(peerId))
                    {
                        _connectionStatus[peerId] = true;
                        OnConnectionStatusChanged?.Invoke(peerId, true);
                    }

                    return;
            }

            // Forward the message to listeners
            OnMessageReceived?.Invoke(peerId, payload);
        }

        /// <summary>
        /// Subscribe to connection requests
        /// </summary>
        private void SubscribeToConnectionRequests()
        {
            if (_p2pInterface == null)
            {
                Debug.LogError("EOSP2PNetworkService: P2P interface is null");
                return;
            }

            var options = new AddNotifyPeerConnectionRequestOptions
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                SocketId = new SocketId { SocketName = SOCKET_NAME }
            };

            _connectionNotificationId =
                _p2pInterface.AddNotifyPeerConnectionRequest(ref options, null, OnIncomingConnectionRequest);

            if (_connectionNotificationId == 0)
                Debug.LogError("EOSP2PNetworkService: Failed to subscribe to connection requests");
            else
                Debug.Log("EOSP2PNetworkService: Subscribed to connection requests");
        }

        /// <summary>
        /// Unsubscribe from connection requests
        /// </summary>
        private void UnsubscribeFromConnectionRequests()
        {
            if (_p2pInterface == null || _connectionNotificationId == 0) return;

            _p2pInterface.RemoveNotifyPeerConnectionRequest(_connectionNotificationId);
            _connectionNotificationId = 0;

            Debug.Log("EOSP2PNetworkService: Unsubscribed from connection requests");
        }

        /// <summary>
        /// Handle an incoming connection request
        /// </summary>
        /// <param name="data"> Connection request data </param>
        private void OnIncomingConnectionRequest(ref OnIncomingConnectionRequestInfo data)
        {
            if (data.LocalUserId == null || !data.LocalUserId.IsValid() ||
                data.RemoteUserId == null || !data.RemoteUserId.IsValid())
            {
                Debug.LogError("EOSP2PNetworkService: Invalid user ID in connection request");
                return;
            }

            string remoteId = data.RemoteUserId.ToString();
            Debug.Log($"EOSP2PNetworkService: Incoming connection request from {remoteId}");

            // Auto-accept all connection requests
            var options = new AcceptConnectionOptions
            {
                LocalUserId = data.LocalUserId,
                RemoteUserId = data.RemoteUserId,
                SocketId = data.SocketId
            };

            var result = _p2pInterface.AcceptConnection(ref options);

            if (result != Result.Success)
            {
                Debug.LogError($"EOSP2PNetworkService: Failed to accept connection: {result}");
                return;
            }

            // Update connection status
            _connectionStatus[remoteId] = true;
            OnConnectionStatusChanged?.Invoke(remoteId, true);

            Debug.Log($"EOSP2PNetworkService: Accepted connection from {remoteId}");
        }

        /// <summary>
        /// Refresh NAT type
        /// </summary>
        private void RefreshNATType()
        {
            var options = new QueryNATTypeOptions();
            _p2pInterface.QueryNATType(ref options, null, OnRefreshNATTypeFinished);
        }

        /// <summary>
        /// Callback for NAT type refresh
        /// </summary>
        /// <param name="data"> NAT type result </param>
        private void OnRefreshNATTypeFinished(ref OnQueryNATTypeCompleteInfo data)
        {
            if (data.ResultCode != Result.Success)
            {
                Debug.LogError($"EOSP2PNetworkService: Failed to refresh NAT type: {data.ResultCode}");
                return;
            }

            Debug.Log($"EOSP2PNetworkService: NAT type refreshed: {data.NATType}");
        }
    }
}