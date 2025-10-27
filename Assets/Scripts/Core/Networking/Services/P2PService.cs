using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Core.Networking.Common;
using Core.Networking.Interfaces;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using PlayEveryWare.EpicOnlineServices;
using PlayEveryWare.EpicOnlineServices.Samples;
using UnityEngine;

namespace Core.Networking.Services
{
    /// <summary>
    /// Service for P2P networking operations
    /// Handles in-game communication between players
    /// </summary>
    public class P2PService : IP2PService
    {
        #region Events
        
        public event Action<PlayerInfo, PlayerAction> OnPlayerActionReceived;
        public event Action<PlayerInfo, string> OnChatMessageReceived;
        public event Action<PlayerInfo, int> OnEmoteReceived;
        public event Action<byte[]> OnGameStateReceived;
        public event Action<ProductUserId> OnPlayerConnected;
        public event Action<ProductUserId> OnPlayerDisconnected;
        
        #endregion
        
        #region Properties
        
        public bool IsInitialized { get; private set; }
        public bool IsHost { get; private set; }
        
        #endregion
        
        #region Private Fields
        
        private EOSPeer2PeerManager _eosP2PManager;
        private ILobbyManager _lobbyManager;
        
        // Socket configuration
        private const string SOCKET_NAME = "RECIPERAGEP2P";
        private SocketId _socketId;
        
        // Message processing
        private Queue<P2PMessage> _messageQueue = new Queue<P2PMessage>();
        private bool _isProcessingMessages;
        
        // Connected players
        private HashSet<ProductUserId> _connectedPlayers = new HashSet<ProductUserId>();
        
        #endregion
        
        #region Nested Types
        
        /// <summary>
        /// Internal P2P message structure
        /// </summary>
        private class P2PMessage
        {
            public ProductUserId SenderId { get; set; }
            public byte MessageType { get; set; }
            public byte[] Data { get; set; }
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Constructor
        /// </summary>
        public P2PService(EOSPeer2PeerManager eosP2PManager, ILobbyManager lobbyManager)
        {
            _eosP2PManager = eosP2PManager ?? throw new ArgumentNullException(nameof(eosP2PManager));
            _lobbyManager = lobbyManager ?? throw new ArgumentNullException(nameof(lobbyManager));
        }
        
        /// <summary>
        /// Initialize the P2P service
        /// </summary>
        public void Initialize()
        {
            if (IsInitialized)
            {
                Debug.LogWarning("[P2PService] Already initialized");
                return;
            }
            
            // Create socket ID
            _socketId = new SocketId { SocketName = SOCKET_NAME };
            
            IsInitialized = true;
            Debug.Log("[P2PService] Initialized");
        }
        
        #endregion
        
        #region Public Methods - Connection
        
        /// <summary>
        /// Start hosting a P2P session
        /// </summary>
        public void StartHosting()
        {
            if (!IsInitialized)
            {
                Debug.LogError("[P2PService] Not initialized");
                return;
            }
            
            IsHost = true;
            _isProcessingMessages = true;
            
            Debug.Log("[P2PService] Started hosting P2P session");
            
            // Start processing messages
            // Note: This should be called from a MonoBehaviour's Update loop
        }
        
        /// <summary>
        /// Connect to a host
        /// </summary>
        public void ConnectToHost(ProductUserId hostId)
        {
            if (!IsInitialized)
            {
                Debug.LogError("[P2PService] Not initialized");
                return;
            }
            
            IsHost = false;
            _isProcessingMessages = true;
            
            Debug.Log($"[P2PService] Connecting to host: {hostId}");
            
            // Send connection request
            SendConnectionRequest(hostId);
            
            // Add to connected players
            _connectedPlayers.Add(hostId);
            OnPlayerConnected?.Invoke(hostId);
        }
        
        /// <summary>
        /// Disconnect from P2P session
        /// </summary>
        public void Disconnect()
        {
            if (!IsInitialized)
                return;
            
            Debug.Log("[P2PService] Disconnecting from P2P session");
            
            _isProcessingMessages = false;
            IsHost = false;
            
            // Close all connections
            foreach (var playerId in _connectedPlayers)
            {
                CloseConnection(playerId);
            }
            
            _connectedPlayers.Clear();
            _messageQueue.Clear();
        }
        
        #endregion
        
        #region Public Methods - Sending
        
        /// <summary>
        /// Send a player action to all players
        /// </summary>
        public void SendPlayerAction(PlayerAction action)
        {
            string actionJson = JsonUtility.ToJson(action);
            byte[] actionData = Encoding.UTF8.GetBytes(actionJson);
            SendToAll(NetworkMessageType.PlayerAction, actionData);
        }
        
        /// <summary>
        /// Send a chat message to all players
        /// </summary>
        public void SendChatMessage(string message)
        {
            byte[] messageData = Encoding.UTF8.GetBytes(message);
            SendToAll(NetworkMessageType.ChatMessage, messageData);
        }
        
        /// <summary>
        /// Send an emote to all players
        /// </summary>
        public void SendEmote(int emoteId)
        {
            byte[] emoteData = BitConverter.GetBytes(emoteId);
            SendToAll(NetworkMessageType.Emote, emoteData);
        }
        
        /// <summary>
        /// Send game state to all players (host only)
        /// </summary>
        public void SendGameState(byte[] data)
        {
            if (!IsHost)
            {
                Debug.LogWarning("[P2PService] Only host can send game state");
                return;
            }
            
            SendToAll(NetworkMessageType.GameState, data);
        }
        
        /// <summary>
        /// Send data to a specific player
        /// </summary>
        public void SendToPlayer(PlayerInfo targetPlayer, byte messageType, byte[] data)
        {
            if (targetPlayer == null || targetPlayer.ProductUserId == null)
            {
                Debug.LogError("[P2PService] Invalid target player");
                return;
            }
            
            SendP2PPacket(targetPlayer.ProductUserId, messageType, data);
        }
        
        /// <summary>
        /// Send data to all players
        /// </summary>
        public void SendToAll(byte messageType, byte[] data)
        {
            var matchLobby = _lobbyManager.CurrentMatchLobby;
            if (matchLobby == null)
            {
                Debug.LogWarning("[P2PService] Not in a match lobby");
                return;
            }
            
            foreach (var player in matchLobby.Players)
            {
                if (!player.IsLocal)
                {
                    SendP2PPacket(player.ProductUserId, messageType, data);
                }
            }
        }
        
        #endregion
        
        #region Private Methods - Sending
        
        /// <summary>
        /// Send a P2P packet
        /// </summary>
        private void SendP2PPacket(ProductUserId targetUserId, byte messageType, byte[] data)
        {
            if (!IsInitialized)
                return;
            
            // Create packet with message type header
            byte[] packet = new byte[data.Length + 1];
            packet[0] = messageType;
            Array.Copy(data, 0, packet, 1, data.Length);
            
            // Send via EOS P2P
            var sendOptions = new SendPacketOptions
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RemoteUserId = targetUserId,
                SocketId = _socketId,
                Channel = 0,
                Data = new ArraySegment<byte>(packet),
                AllowDelayedDelivery = true,
                Reliability = PacketReliability.ReliableOrdered
            };
            
            Result result = EOSManager.Instance.GetEOSPlatformInterface().GetP2PInterface().SendPacket(ref sendOptions);
            
            if (result != Result.Success)
            {
                Debug.LogWarning($"[P2PService] Failed to send packet to {targetUserId}: {result}");
            }
        }
        
        /// <summary>
        /// Send connection request
        /// </summary>
        private void SendConnectionRequest(ProductUserId targetUserId)
        {
            byte[] emptyData = new byte[0];
            SendP2PPacket(targetUserId, NetworkMessageType.ConnectionRequest, emptyData);
        }
        
        /// <summary>
        /// Close connection to a player
        /// </summary>
        private void CloseConnection(ProductUserId playerId)
        {
            var closeOptions = new CloseConnectionOptions
            {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RemoteUserId = playerId,
                SocketId = _socketId
            };
            
            EOSManager.Instance.GetEOSPlatformInterface().GetP2PInterface().CloseConnection(ref closeOptions);
        }
        
        #endregion
        
        #region Private Methods - Receiving
        
        /// <summary>
        /// Process incoming messages (should be called from Update)
        /// </summary>
        public void Update()
        {
            if (!IsInitialized || !_isProcessingMessages)
                return;
            
            // Receive packets
            ReceivePackets();
            
            // Process queued messages
            while (_messageQueue.Count > 0)
            {
                var message = _messageQueue.Dequeue();
                HandleMessage(message);
            }
        }
        
        /// <summary>
        /// Receive P2P packets
        /// </summary>
        private void ReceivePackets()
        {
            var p2pInterface = EOSManager.Instance.GetEOSPlatformInterface().GetP2PInterface();
            var localUserId = EOSManager.Instance.GetProductUserId();
            
            // Get next packet size
            var getNextReceivedPacketSizeOptions = new GetNextReceivedPacketSizeOptions
            {
                LocalUserId = localUserId,
                RequestedChannel = null
            };
            
            Result result = p2pInterface.GetNextReceivedPacketSize(ref getNextReceivedPacketSizeOptions, out uint packetSize);
            
            while (result == Result.Success && packetSize > 0)
            {
                // Receive packet
                byte[] data = new byte[packetSize];
                var receivePacketOptions = new ReceivePacketOptions
                {
                    LocalUserId = localUserId,
                    MaxDataSizeBytes = packetSize,
                    RequestedChannel = null
                };
                
                var dataSegment = new ArraySegment<byte>(data);
                ProductUserId senderId = null;
                SocketId socketId = default;
                byte channel;
                uint bytesWritten;
                result = p2pInterface.ReceivePacket(ref receivePacketOptions, ref senderId, ref socketId, out channel, dataSegment, out bytesWritten);
                
                if (result == Result.Success && bytesWritten > 0)
                {
                    // Extract message type
                    byte messageType = data[0];
                    
                    // Extract message data
                    byte[] messageData = new byte[bytesWritten - 1];
                    Array.Copy(data, 1, messageData, 0, messageData.Length);
                    
                    // Queue message for processing
                    _messageQueue.Enqueue(new P2PMessage
                    {
                        SenderId = senderId,
                        MessageType = messageType,
                        Data = messageData
                    });
                    
                    // Track connection
                    if (!_connectedPlayers.Contains(senderId))
                    {
                        _connectedPlayers.Add(senderId);
                        OnPlayerConnected?.Invoke(senderId);
                    }
                }
                
                // Check for next packet
                result = p2pInterface.GetNextReceivedPacketSize(ref getNextReceivedPacketSizeOptions, out packetSize);
            }
        }
        
        /// <summary>
        /// Handle a received message
        /// </summary>
        private void HandleMessage(P2PMessage message)
        {
            // Find sender player info
            PlayerInfo sender = FindPlayerById(message.SenderId);
            
            if (sender == null && message.MessageType != NetworkMessageType.ConnectionRequest)
            {
                Debug.LogWarning($"[P2PService] Received message from unknown player: {message.SenderId}");
                return;
            }
            
            // Handle based on message type
            switch (message.MessageType)
            {
                case NetworkMessageType.ConnectionRequest:
                    HandleConnectionRequest(message.SenderId);
                    break;
                    
                case NetworkMessageType.PlayerAction:
                    HandlePlayerAction(sender, message.Data);
                    break;
                    
                case NetworkMessageType.ChatMessage:
                    HandleChatMessage(sender, message.Data);
                    break;
                    
                case NetworkMessageType.Emote:
                    HandleEmote(sender, message.Data);
                    break;
                    
                case NetworkMessageType.GameState:
                    HandleGameState(message.Data);
                    break;
                    
                default:
                    Debug.LogWarning($"[P2PService] Unknown message type: {message.MessageType}");
                    break;
            }
        }
        
        #endregion
        
        #region Private Methods - Message Handlers
        
        /// <summary>
        /// Handle connection request
        /// </summary>
        private void HandleConnectionRequest(ProductUserId senderId)
        {
            Debug.Log($"[P2PService] Connection request from: {senderId}");
            
            if (!_connectedPlayers.Contains(senderId))
            {
                _connectedPlayers.Add(senderId);
                OnPlayerConnected?.Invoke(senderId);
            }
        }
        
        /// <summary>
        /// Handle player action
        /// </summary>
        private void HandlePlayerAction(PlayerInfo sender, byte[] data)
        {
            try
            {
                string actionJson = Encoding.UTF8.GetString(data);
                PlayerAction action = JsonUtility.FromJson<PlayerAction>(actionJson);
                OnPlayerActionReceived?.Invoke(sender, action);
            }
            catch (Exception e)
            {
                Debug.LogError($"[P2PService] Error handling player action: {e.Message}");
            }
        }
        
        /// <summary>
        /// Handle chat message
        /// </summary>
        private void HandleChatMessage(PlayerInfo sender, byte[] data)
        {
            try
            {
                string message = Encoding.UTF8.GetString(data);
                OnChatMessageReceived?.Invoke(sender, message);
            }
            catch (Exception e)
            {
                Debug.LogError($"[P2PService] Error handling chat message: {e.Message}");
            }
        }
        
        /// <summary>
        /// Handle emote
        /// </summary>
        private void HandleEmote(PlayerInfo sender, byte[] data)
        {
            try
            {
                int emoteId = BitConverter.ToInt32(data, 0);
                OnEmoteReceived?.Invoke(sender, emoteId);
            }
            catch (Exception e)
            {
                Debug.LogError($"[P2PService] Error handling emote: {e.Message}");
            }
        }
        
        /// <summary>
        /// Handle game state
        /// </summary>
        private void HandleGameState(byte[] data)
        {
            OnGameStateReceived?.Invoke(data);
        }
        
        #endregion
        
        #region Private Methods - Utility
        
        /// <summary>
        /// Find player by product user ID
        /// </summary>
        private PlayerInfo FindPlayerById(ProductUserId userId)
        {
            var matchLobby = _lobbyManager.CurrentMatchLobby;
            if (matchLobby == null)
                return null;
            
            foreach (var player in matchLobby.Players)
            {
                if (player.ProductUserId == userId)
                    return player;
            }
            
            return null;
        }
        
        #endregion
    }
}
