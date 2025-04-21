using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using PlayEveryWare.EpicOnlineServices.Samples;
using RecipeRage.Core.Networking.Common;
using UnityEngine;

namespace RecipeRage.Core.Networking.EOS
{
    /// <summary>
    /// Manages peer-to-peer communication using the EOS P2P interface.
    /// Extends the functionality of the EOSPeer2PeerManager from the EOS samples.
    /// </summary>
    public class RecipeRageP2PManager : MonoBehaviour
    {
        // Reference to the EOS P2P Manager
        private EOSPeer2PeerManager _eosPeerManager;
        
        // Socket name for game messages
        private const string SOCKET_NAME = "RECIPERAGEGAME";
        
        // Message types
        public enum MessageType
        {
            PlayerAction = 0,
            IngredientPickup = 1,
            IngredientDrop = 2,
            RecipeComplete = 3,
            GameState = 4,
            PlayerSync = 5,
            HostMigration = 6
        }
        
        // Events
        public event Action<ProductUserId, PlayerAction> OnPlayerActionReceived;
        public event Action<ProductUserId, IngredientPickupMessage> OnIngredientPickupReceived;
        public event Action<ProductUserId, IngredientDropMessage> OnIngredientDropReceived;
        public event Action<ProductUserId, RecipeCompleteMessage> OnRecipeCompleteReceived;
        public event Action<ProductUserId, GameStateMessage> OnGameStateReceived;
        public event Action<ProductUserId, PlayerSyncMessage> OnPlayerSyncReceived;
        public event Action<ProductUserId, HostMigrationMessage> OnHostMigrationReceived;
        
        // Coroutine for receiving messages
        private Coroutine _receiveMessagesCoroutine;
        
        /// <summary>
        /// Initialize the P2P manager.
        /// </summary>
        public void Initialize()
        {
            // Get the EOS P2P Manager
            _eosPeerManager = EOSManager.Instance.GetComponent<EOSPeer2PeerManager>();
            if (_eosPeerManager == null)
            {
                Debug.LogError("[RecipeRageP2PManager] EOSPeer2PeerManager not found!");
                return;
            }
            
            // Start receiving messages
            _receiveMessagesCoroutine = StartCoroutine(ReceiveMessagesCoroutine());
            
            Debug.Log("[RecipeRageP2PManager] Initialized");
        }
        
        /// <summary>
        /// Send a player action message.
        /// </summary>
        /// <param name="targetPlayer">The target player</param>
        /// <param name="action">The player action</param>
        public void SendPlayerAction(ProductUserId targetPlayer, PlayerAction action)
        {
            if (_eosPeerManager == null)
            {
                Debug.LogError("[RecipeRageP2PManager] EOSPeer2PeerManager not initialized!");
                return;
            }
            
            // Serialize the player action
            string actionData = JsonUtility.ToJson(action);
            
            // Create a message
            string message = $"{(int)MessageType.PlayerAction}|{actionData}";
            
            // Send using the EOS P2P manager
            SendMessage(targetPlayer, message);
            
            Debug.Log($"[RecipeRageP2PManager] Sending player action to {targetPlayer}");
        }
        
        /// <summary>
        /// Send an ingredient pickup message.
        /// </summary>
        /// <param name="targetPlayer">The target player</param>
        /// <param name="message">The ingredient pickup message</param>
        public void SendIngredientPickup(ProductUserId targetPlayer, IngredientPickupMessage message)
        {
            if (_eosPeerManager == null)
            {
                Debug.LogError("[RecipeRageP2PManager] EOSPeer2PeerManager not initialized!");
                return;
            }
            
            // Serialize the message
            string messageData = JsonUtility.ToJson(message);
            
            // Create a message
            string formattedMessage = $"{(int)MessageType.IngredientPickup}|{messageData}";
            
            // Send using the EOS P2P manager
            SendMessage(targetPlayer, formattedMessage);
            
            Debug.Log($"[RecipeRageP2PManager] Sending ingredient pickup to {targetPlayer}");
        }
        
        /// <summary>
        /// Send an ingredient drop message.
        /// </summary>
        /// <param name="targetPlayer">The target player</param>
        /// <param name="message">The ingredient drop message</param>
        public void SendIngredientDrop(ProductUserId targetPlayer, IngredientDropMessage message)
        {
            if (_eosPeerManager == null)
            {
                Debug.LogError("[RecipeRageP2PManager] EOSPeer2PeerManager not initialized!");
                return;
            }
            
            // Serialize the message
            string messageData = JsonUtility.ToJson(message);
            
            // Create a message
            string formattedMessage = $"{(int)MessageType.IngredientDrop}|{messageData}";
            
            // Send using the EOS P2P manager
            SendMessage(targetPlayer, formattedMessage);
            
            Debug.Log($"[RecipeRageP2PManager] Sending ingredient drop to {targetPlayer}");
        }
        
        /// <summary>
        /// Send a recipe complete message.
        /// </summary>
        /// <param name="targetPlayer">The target player</param>
        /// <param name="message">The recipe complete message</param>
        public void SendRecipeComplete(ProductUserId targetPlayer, RecipeCompleteMessage message)
        {
            if (_eosPeerManager == null)
            {
                Debug.LogError("[RecipeRageP2PManager] EOSPeer2PeerManager not initialized!");
                return;
            }
            
            // Serialize the message
            string messageData = JsonUtility.ToJson(message);
            
            // Create a message
            string formattedMessage = $"{(int)MessageType.RecipeComplete}|{messageData}";
            
            // Send using the EOS P2P manager
            SendMessage(targetPlayer, formattedMessage);
            
            Debug.Log($"[RecipeRageP2PManager] Sending recipe complete to {targetPlayer}");
        }
        
        /// <summary>
        /// Send a game state message.
        /// </summary>
        /// <param name="targetPlayer">The target player</param>
        /// <param name="message">The game state message</param>
        public void SendGameState(ProductUserId targetPlayer, GameStateMessage message)
        {
            if (_eosPeerManager == null)
            {
                Debug.LogError("[RecipeRageP2PManager] EOSPeer2PeerManager not initialized!");
                return;
            }
            
            // Serialize the message
            string messageData = JsonUtility.ToJson(message);
            
            // Create a message
            string formattedMessage = $"{(int)MessageType.GameState}|{messageData}";
            
            // Send using the EOS P2P manager
            SendMessage(targetPlayer, formattedMessage);
            
            Debug.Log($"[RecipeRageP2PManager] Sending game state to {targetPlayer}");
        }
        
        /// <summary>
        /// Send a player sync message.
        /// </summary>
        /// <param name="targetPlayer">The target player</param>
        /// <param name="message">The player sync message</param>
        public void SendPlayerSync(ProductUserId targetPlayer, PlayerSyncMessage message)
        {
            if (_eosPeerManager == null)
            {
                Debug.LogError("[RecipeRageP2PManager] EOSPeer2PeerManager not initialized!");
                return;
            }
            
            // Serialize the message
            string messageData = JsonUtility.ToJson(message);
            
            // Create a message
            string formattedMessage = $"{(int)MessageType.PlayerSync}|{messageData}";
            
            // Send using the EOS P2P manager
            SendMessage(targetPlayer, formattedMessage);
            
            Debug.Log($"[RecipeRageP2PManager] Sending player sync to {targetPlayer}");
        }
        
        /// <summary>
        /// Send a host migration message.
        /// </summary>
        /// <param name="targetPlayer">The target player</param>
        /// <param name="message">The host migration message</param>
        public void SendHostMigration(ProductUserId targetPlayer, HostMigrationMessage message)
        {
            if (_eosPeerManager == null)
            {
                Debug.LogError("[RecipeRageP2PManager] EOSPeer2PeerManager not initialized!");
                return;
            }
            
            // Serialize the message
            string messageData = JsonUtility.ToJson(message);
            
            // Create a message
            string formattedMessage = $"{(int)MessageType.HostMigration}|{messageData}";
            
            // Send using the EOS P2P manager
            SendMessage(targetPlayer, formattedMessage);
            
            Debug.Log($"[RecipeRageP2PManager] Sending host migration to {targetPlayer}");
        }
        
        /// <summary>
        /// Broadcast a message to all players.
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="players">The list of players</param>
        public void BroadcastMessage(string message, List<NetworkPlayer> players)
        {
            if (_eosPeerManager == null)
            {
                Debug.LogError("[RecipeRageP2PManager] EOSPeer2PeerManager not initialized!");
                return;
            }
            
            foreach (NetworkPlayer player in players)
            {
                if (!player.IsLocal)
                {
                    ProductUserId targetPlayer = ProductUserId.FromString(player.PlayerId);
                    if (targetPlayer != null && targetPlayer.IsValid())
                    {
                        SendMessage(targetPlayer, message);
                    }
                }
            }
            
            Debug.Log($"[RecipeRageP2PManager] Broadcasting message to {players.Count} players");
        }
        
        /// <summary>
        /// Send a message to a player.
        /// </summary>
        /// <param name="targetPlayer">The target player</param>
        /// <param name="message">The message</param>
        private void SendMessage(ProductUserId targetPlayer, string message)
        {
            if (_eosPeerManager == null)
            {
                Debug.LogError("[RecipeRageP2PManager] EOSPeer2PeerManager not initialized!");
                return;
            }
            
            // Create a message data
            messageData messageData = new messageData
            {
                type = messageType.textMessage,
                textData = message
            };
            
            // Send using the EOS P2P manager
            _eosPeerManager.SendMessage(targetPlayer, messageData);
        }
        
        /// <summary>
        /// Coroutine to receive messages.
        /// </summary>
        private IEnumerator ReceiveMessagesCoroutine()
        {
            while (true)
            {
                if (_eosPeerManager != null)
                {
                    // Use the EOS P2P manager to receive messages
                    ProductUserId senderId = _eosPeerManager.HandleReceivedMessages();
                    if (senderId != null)
                    {
                        // Get the message from the chat cache
                        if (_eosPeerManager.GetChatDataCache(out Dictionary<ProductUserId, ChatWithFriendData> chatCache))
                        {
                            if (chatCache.TryGetValue(senderId, out ChatWithFriendData chatData))
                            {
                                // Process the latest message
                                if (chatData.ChatLines.Count > 0)
                                {
                                    ChatEntry latestMessage = chatData.ChatLines.Peek();
                                    ProcessGameMessage(senderId, latestMessage.Message);
                                }
                            }
                        }
                    }
                }
                
                yield return null;
            }
        }
        
        /// <summary>
        /// Process a game message.
        /// </summary>
        /// <param name="senderId">The sender ID</param>
        /// <param name="message">The message</param>
        private void ProcessGameMessage(ProductUserId senderId, string message)
        {
            // Parse the message type and data
            string[] parts = message.Split('|');
            if (parts.Length < 2)
                return;
                
            if (int.TryParse(parts[0], out int messageTypeInt))
            {
                MessageType messageType = (MessageType)messageTypeInt;
                string data = parts[1];
                
                switch (messageType)
                {
                    case MessageType.PlayerAction:
                        PlayerAction action = JsonUtility.FromJson<PlayerAction>(data);
                        OnPlayerActionReceived?.Invoke(senderId, action);
                        break;
                        
                    case MessageType.IngredientPickup:
                        IngredientPickupMessage pickupMessage = JsonUtility.FromJson<IngredientPickupMessage>(data);
                        OnIngredientPickupReceived?.Invoke(senderId, pickupMessage);
                        break;
                        
                    case MessageType.IngredientDrop:
                        IngredientDropMessage dropMessage = JsonUtility.FromJson<IngredientDropMessage>(data);
                        OnIngredientDropReceived?.Invoke(senderId, dropMessage);
                        break;
                        
                    case MessageType.RecipeComplete:
                        RecipeCompleteMessage recipeMessage = JsonUtility.FromJson<RecipeCompleteMessage>(data);
                        OnRecipeCompleteReceived?.Invoke(senderId, recipeMessage);
                        break;
                        
                    case MessageType.GameState:
                        GameStateMessage stateMessage = JsonUtility.FromJson<GameStateMessage>(data);
                        OnGameStateReceived?.Invoke(senderId, stateMessage);
                        break;
                        
                    case MessageType.PlayerSync:
                        PlayerSyncMessage syncMessage = JsonUtility.FromJson<PlayerSyncMessage>(data);
                        OnPlayerSyncReceived?.Invoke(senderId, syncMessage);
                        break;
                        
                    case MessageType.HostMigration:
                        HostMigrationMessage migrationMessage = JsonUtility.FromJson<HostMigrationMessage>(data);
                        OnHostMigrationReceived?.Invoke(senderId, migrationMessage);
                        break;
                }
            }
        }
        
        /// <summary>
        /// Clean up resources.
        /// </summary>
        private void OnDestroy()
        {
            if (_receiveMessagesCoroutine != null)
            {
                StopCoroutine(_receiveMessagesCoroutine);
                _receiveMessagesCoroutine = null;
            }
        }
    }
    
    #region Message Classes
    
    /// <summary>
    /// Player action message.
    /// </summary>
    [Serializable]
    public class PlayerAction
    {
        public string PlayerId;
        public int ActionType;
        public float PositionX;
        public float PositionY;
        public float PositionZ;
        public float RotationY;
        public int TargetId;
        public string CustomData;
    }
    
    /// <summary>
    /// Ingredient pickup message.
    /// </summary>
    [Serializable]
    public class IngredientPickupMessage
    {
        public string PlayerId;
        public int IngredientId;
        public int StationId;
    }
    
    /// <summary>
    /// Ingredient drop message.
    /// </summary>
    [Serializable]
    public class IngredientDropMessage
    {
        public string PlayerId;
        public int IngredientId;
        public int StationId;
    }
    
    /// <summary>
    /// Recipe complete message.
    /// </summary>
    [Serializable]
    public class RecipeCompleteMessage
    {
        public string PlayerId;
        public int RecipeId;
        public int Score;
        public int TeamId;
    }
    
    /// <summary>
    /// Game state message.
    /// </summary>
    [Serializable]
    public class GameStateMessage
    {
        public int GameState;
        public float TimeRemaining;
        public int TeamAScore;
        public int TeamBScore;
        public List<int> ActiveOrderIds;
    }
    
    /// <summary>
    /// Player sync message.
    /// </summary>
    [Serializable]
    public class PlayerSyncMessage
    {
        public string PlayerId;
        public float PositionX;
        public float PositionY;
        public float PositionZ;
        public float RotationY;
        public int CurrentIngredientId;
        public int CurrentStationId;
        public int PlayerState;
    }
    
    /// <summary>
    /// Host migration message.
    /// </summary>
    [Serializable]
    public class HostMigrationMessage
    {
        public string NewHostId;
        public GameStateMessage GameState;
    }
    
    #endregion
}
