using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Core.Networking.Common;
using Epic.OnlineServices;
using PlayEveryWare.EpicOnlineServices;
using PlayEveryWare.EpicOnlineServices.Samples;
using UnityEngine;

namespace Core.Networking.EOS
{
    /// <summary>
    /// Wrapper for EOSPeer2PeerManager that provides game-specific P2P functionality.
    /// </summary>
    public class RecipeRageP2PManager : MonoBehaviour
    {
        // Reference to the EOS P2P Manager
        private EOSPeer2PeerManager _eosP2PManager;

        // Socket name for game communication
        private const string SOCKET_NAME = "RECIPERAGEP2P";

        // Message processing
        private Queue<P2PMessage> _messageQueue = new Queue<P2PMessage>();
        private bool _isProcessingMessages = false;

        // Events
        public event Action<PlayerInfo, PlayerAction> OnPlayerActionReceived;
        public event Action<PlayerInfo, string> OnChatMessageReceived;
        public event Action<PlayerInfo, int> OnEmoteReceived;

        /// <summary>
        /// Class representing a P2P message.
        /// </summary>
        private class P2PMessage
        {
            public ProductUserId SenderId { get; set; }
            public byte MessageType { get; set; }
            public byte[] Data { get; set; }
        }

        /// <summary>
        /// Initialize the P2P manager.
        /// </summary>
        public void Initialize()
        {
            // Get the EOS P2P Manager
            _eosP2PManager = EOSManager.Instance.GetOrCreateManager<EOSPeer2PeerManager>();

            if (_eosP2PManager == null)
            {
                Debug.LogError("[RecipeRageP2PManager] EOSPeer2PeerManager not found on EOSManager");
                return;
            }

            // Start processing messages
            StartCoroutine(ProcessMessagesCoroutine());

            Debug.Log("[RecipeRageP2PManager] Initialized");
        }

        /// <summary>
        /// Send a player action to all players.
        /// </summary>
        /// <param name="action">The player action</param>
        public void SendPlayerAction(PlayerAction action)
        {
            // Serialize the action to JSON
            string actionJson = JsonUtility.ToJson(action);
            byte[] actionData = Encoding.UTF8.GetBytes(actionJson);

            // Send to all players
            SendToAll(NetworkMessageType.PlayerAction, actionData);
        }

        /// <summary>
        /// Send a chat message to all players.
        /// </summary>
        /// <param name="message">The chat message</param>
        public void SendChatMessage(string message)
        {
            // Encode the message
            byte[] messageData = Encoding.UTF8.GetBytes(message);

            // Send to all players
            SendToAll(NetworkMessageType.ChatMessage, messageData);
        }

        /// <summary>
        /// Send an emote to all players.
        /// </summary>
        /// <param name="emoteId">The emote ID</param>
        public void SendEmote(int emoteId)
        {
            // Encode the emote ID
            byte[] emoteData = BitConverter.GetBytes(emoteId);

            // Send to all players
            SendToAll(NetworkMessageType.Emote, emoteData);
        }

        /// <summary>
        /// Send a message to a specific player.
        /// </summary>
        /// <param name="targetPlayer">The target player</param>
        /// <param name="messageType">The message type</param>
        /// <param name="data">The message data</param>
        public void SendToPlayer(PlayerInfo targetPlayer, byte messageType, byte[] data)
        {
            if (targetPlayer == null || targetPlayer.ProductUserId == null)
            {
                Debug.LogError("[RecipeRageP2PManager] Invalid target player");
                return;
            }

            // Create the message packet
            byte[] packet = new byte[data.Length + 1];
            packet[0] = messageType;
            Array.Copy(data, 0, packet, 1, data.Length);

            // Create the message data
            PlayEveryWare.EpicOnlineServices.Samples.messageData message;
            message.type = PlayEveryWare.EpicOnlineServices.Samples.messageType.textMessage;
            message.textData = Convert.ToBase64String(packet);
            message.xPos = 0;
            message.yPos = 0;

            // Send the message
            _eosP2PManager.SendMessage(targetPlayer.ProductUserId, message);

            Debug.Log($"[RecipeRageP2PManager] Sent message to player {targetPlayer.DisplayName}, Type: {messageType}, Size: {data.Length}");
        }

        /// <summary>
        /// Send a message to all players.
        /// </summary>
        /// <param name="messageType">The message type</param>
        /// <param name="data">The message data</param>
        public void SendToAll(byte messageType, byte[] data)
        {
            // Get all players
            List<PlayerInfo> allPlayers = GetAllPlayers();

            foreach (PlayerInfo player in allPlayers)
            {
                if (!player.IsLocal)
                {
                    SendToPlayer(player, messageType, data);
                }
            }

            Debug.Log($"[RecipeRageP2PManager] Sent message to all players, Type: {messageType}, Size: {data.Length}");
        }

        /// <summary>
        /// Process received messages.
        /// </summary>
        private IEnumerator ProcessMessagesCoroutine()
        {
            _isProcessingMessages = true;

            while (_isProcessingMessages)
            {
                // Check for new messages
                ProductUserId senderId = _eosP2PManager.HandleReceivedMessages();

                if (senderId != null)
                {
                    // Get the message from the chat cache
                    if (_eosP2PManager.GetChatDataCache(out Dictionary<ProductUserId, ChatWithFriendData> chatCache))
                    {
                        if (chatCache.TryGetValue(senderId, out ChatWithFriendData chatData))
                        {
                            // Process all messages
                            foreach (ChatEntry chatEntry in chatData.ChatLines)
                            {
                                if (!chatEntry.isOwnEntry)
                                {
                                    ProcessMessage(senderId, chatEntry.Message);
                                }
                            }
                        }
                    }
                }

                // Process queued messages
                while (_messageQueue.Count > 0)
                {
                    P2PMessage message = _messageQueue.Dequeue();
                    HandleMessage(message);
                }

                yield return null;
            }
        }

        /// <summary>
        /// Process a received message.
        /// </summary>
        /// <param name="senderId">The sender ID</param>
        /// <param name="message">The message</param>
        private void ProcessMessage(ProductUserId senderId, string message)
        {
            try
            {
                // Decode the message
                byte[] data = Convert.FromBase64String(message);

                if (data.Length > 0)
                {
                    // Extract the message type
                    byte messageType = data[0];

                    // Extract the message data
                    byte[] messageData = new byte[data.Length - 1];
                    Array.Copy(data, 1, messageData, 0, messageData.Length);

                    // Queue the message for processing
                    _messageQueue.Enqueue(new P2PMessage
                    {
                        SenderId = senderId,
                        MessageType = messageType,
                        Data = messageData
                    });
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[RecipeRageP2PManager] Error processing message: {e.Message}");
            }
        }

        /// <summary>
        /// Handle a processed message.
        /// </summary>
        /// <param name="message">The message</param>
        private void HandleMessage(P2PMessage message)
        {
            // Find the sender
            PlayerInfo sender = FindPlayerById(message.SenderId.ToString());

            if (sender == null)
            {
                Debug.LogWarning($"[RecipeRageP2PManager] Received message from unknown player: {message.SenderId}");
                return;
            }

            // Handle based on message type
            switch (message.MessageType)
            {
                case NetworkMessageType.PlayerAction:
                    HandlePlayerAction(sender, message.Data);
                    break;
                case NetworkMessageType.ChatMessage:
                    HandleChatMessage(sender, message.Data);
                    break;
                case NetworkMessageType.Emote:
                    HandleEmote(sender, message.Data);
                    break;
                default:
                    Debug.LogWarning($"[RecipeRageP2PManager] Unknown message type: {message.MessageType}");
                    break;
            }
        }

        /// <summary>
        /// Handle a player action message.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="data">The message data</param>
        private void HandlePlayerAction(PlayerInfo sender, byte[] data)
        {
            try
            {
                // Deserialize the action
                string actionJson = Encoding.UTF8.GetString(data);
                PlayerAction action = JsonUtility.FromJson<PlayerAction>(actionJson);

                // Notify listeners
                OnPlayerActionReceived?.Invoke(sender, action);

                Debug.Log($"[RecipeRageP2PManager] Received player action from {sender.DisplayName}, Type: {action.ActionType}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[RecipeRageP2PManager] Error handling player action: {e.Message}");
            }
        }

        /// <summary>
        /// Handle a chat message.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="data">The message data</param>
        private void HandleChatMessage(PlayerInfo sender, byte[] data)
        {
            try
            {
                // Decode the message
                string message = Encoding.UTF8.GetString(data);

                // Notify listeners
                OnChatMessageReceived?.Invoke(sender, message);

                Debug.Log($"[RecipeRageP2PManager] Received chat message from {sender.DisplayName}: {message}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[RecipeRageP2PManager] Error handling chat message: {e.Message}");
            }
        }

        /// <summary>
        /// Handle an emote message.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="data">The message data</param>
        private void HandleEmote(PlayerInfo sender, byte[] data)
        {
            try
            {
                // Decode the emote ID
                int emoteId = BitConverter.ToInt32(data, 0);

                // Notify listeners
                OnEmoteReceived?.Invoke(sender, emoteId);

                Debug.Log($"[RecipeRageP2PManager] Received emote from {sender.DisplayName}: {emoteId}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[RecipeRageP2PManager] Error handling emote: {e.Message}");
            }
        }

        /// <summary>
        /// Find a player by ID.
        /// </summary>
        /// <param name="playerId">The player ID</param>
        /// <returns>The player info</returns>
        private PlayerInfo FindPlayerById(string playerId)
        {
            // Get all players
            List<PlayerInfo> allPlayers = GetAllPlayers();

            foreach (PlayerInfo player in allPlayers)
            {
                if (player.PlayerId == playerId)
                {
                    return player;
                }
            }

            return null;
        }

        /// <summary>
        /// Get all players.
        /// </summary>
        /// <returns>The list of all players</returns>
        private List<PlayerInfo> GetAllPlayers()
        {
            // Get the lobby manager
            RecipeRageLobbyManager lobbyManager = GetComponent<RecipeRageLobbyManager>();

            if (lobbyManager != null)
            {
                // Combine both teams
                List<PlayerInfo> allPlayers = new List<PlayerInfo>();
                allPlayers.AddRange(lobbyManager.TeamA);
                allPlayers.AddRange(lobbyManager.TeamB);
                return allPlayers;
            }

            return new List<PlayerInfo>();
        }

        /// <summary>
        /// Clean up when destroyed.
        /// </summary>
        private void OnDestroy()
        {
            _isProcessingMessages = false;
        }
    }
}
