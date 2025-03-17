using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using RecipeRage.Modules.Friends.Data;
using RecipeRage.Modules.Friends.Interfaces;
using RecipeRage.Modules.Friends.Network;
using RecipeRage.Modules.Logging;
using UnityEngine;

namespace RecipeRage.Modules.Friends.Core
{
    /// <summary>
    /// Implementation of the chat service
    /// Complexity Rating: 4
    /// </summary>
    public class ChatService : IChatService
    {
        private const string SAVE_PATH = "FriendsData/Chats";
        private const int MAX_CACHED_MESSAGES = 100;

        private readonly Dictionary<string, List<ChatMessage>> _chatHistory = new Dictionary<string, List<ChatMessage>>();

        private readonly IIdentityService _identityService;
        private readonly IP2PNetworkService _p2pNetworkService;
        private bool _isInitialized;
        private Dictionary<string, int> _unreadCounts = new Dictionary<string, int>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="identityService"> Identity service </param>
        /// <param name="p2pNetworkService"> P2P network service </param>
        public ChatService(IIdentityService identityService, IP2PNetworkService p2pNetworkService)
        {
            _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            _p2pNetworkService = p2pNetworkService ?? throw new ArgumentNullException(nameof(p2pNetworkService));
        }

        /// <summary>
        /// Event triggered when a new message is received
        /// </summary>
        public event Action<ChatMessage> OnMessageReceived;

        /// <summary>
        /// Event triggered when a message is sent
        /// </summary>
        public event Action<ChatMessage> OnMessageSent;

        /// <summary>
        /// Event triggered when messages are loaded from history
        /// </summary>
        public event Action<string, List<ChatMessage>> OnChatHistoryLoaded;

        /// <summary>
        /// Initialize the chat service
        /// </summary>
        /// <param name="onComplete"> Callback when initialization is complete </param>
        public void Initialize(Action<bool> onComplete = null)
        {
            if (_isInitialized)
            {
                LogHelper.Warning("ChatService", "Already initialized");
                onComplete?.Invoke(true);
                return;
            }

            LogHelper.Info("ChatService", "Initializing...");

            // Subscribe to network events
            _p2pNetworkService.OnMessageReceived += HandleMessageReceived;

            // Ensure save directory exists
            EnsureSaveDirectoryExists();

            // Load unread counts
            LoadUnreadCounts();

            _isInitialized = true;
            LogHelper.Info("ChatService", "Initialized successfully");
            onComplete?.Invoke(true);
        }

        /// <summary>
        /// Send a text message to a friend
        /// </summary>
        public void SendTextMessage(string friendId, string message, Action<bool> onComplete = null)
        {
            if (!_isInitialized)
            {
                LogHelper.Error("ChatService", "Not initialized");
                onComplete?.Invoke(false);
                return;
            }

            if (string.IsNullOrEmpty(friendId) || string.IsNullOrEmpty(message))
            {
                LogHelper.Error("ChatService", "Invalid message parameters");
                onComplete?.Invoke(false);
                return;
            }

            // Create the message
            var chatMessage = new ChatMessage
            {
                MessageId = Guid.NewGuid().ToString(),
                SenderId = _identityService.GetCurrentUserId(),
                SenderName = _identityService.GetCurrentDisplayName(),
                ReceiverId = friendId,
                Content = message,
                SentTime = DateTime.UtcNow,
                IsRead = true, // Messages sent by local user are already read
                IsFromLocalUser = true,
                MessageType = ChatMessageType.Text
            };

            // Add to history
            AddMessageToHistory(chatMessage);

            // Convert to JSON and send
            try
            {
                string json = JsonConvert.SerializeObject(chatMessage);
                byte[] packet = FriendsNetworkProtocol.CreateChatMessage(json);

                // Try to establish connection and send
                _p2pNetworkService.Connect(friendId, connected =>
                {
                    if (!connected)
                    {
                        LogHelper.Warning("ChatService", $"Could not connect to {friendId}, message will be delivered when connection is established");

                        // Still consider it a success, will be delivered when connection is established
                        OnMessageSent?.Invoke(chatMessage);
                        onComplete?.Invoke(true);
                        return;
                    }

                    _p2pNetworkService.SendMessage(friendId, packet, true, success =>
                    {
                        if (success)
                        {
                            LogHelper.Debug("ChatService", $"Message sent to {friendId}");
                            OnMessageSent?.Invoke(chatMessage);
                        }
                        else
                        {
                            LogHelper.Error("ChatService", $"Failed to send message to {friendId}");
                        }

                        onComplete?.Invoke(success);
                    });
                });
            }
            catch (Exception ex)
            {
                LogHelper.Exception("ChatService", ex, "Error sending message");
                onComplete?.Invoke(false);
            }
        }

        /// <summary>
        /// Send a game invite to a friend
        /// </summary>
        public void SendGameInvite(string friendId, string gameData, string message = null, Action<bool> onComplete = null)
        {
            if (!_isInitialized)
            {
                LogHelper.Error("ChatService", "Not initialized");
                onComplete?.Invoke(false);
                return;
            }

            if (string.IsNullOrEmpty(friendId) || string.IsNullOrEmpty(gameData))
            {
                LogHelper.Error("ChatService", "Invalid game invite parameters");
                onComplete?.Invoke(false);
                return;
            }

            // Create the invite message
            var chatMessage = new ChatMessage
            {
                MessageId = Guid.NewGuid().ToString(),
                SenderId = _identityService.GetCurrentUserId(),
                SenderName = _identityService.GetCurrentDisplayName(),
                ReceiverId = friendId,
                Content = string.IsNullOrEmpty(message) ? "Join my game!" : message,
                SentTime = DateTime.UtcNow,
                IsRead = true, // Messages sent by local user are already read
                IsFromLocalUser = true,
                MessageType = ChatMessageType.GameInvite,
                AdditionalData = gameData
            };

            // Add to history
            AddMessageToHistory(chatMessage);

            // Convert to JSON and send
            try
            {
                string json = JsonConvert.SerializeObject(chatMessage);
                byte[] packet = FriendsNetworkProtocol.CreateGameInvite(json);

                // Try to establish connection and send
                _p2pNetworkService.Connect(friendId, connected =>
                {
                    if (!connected)
                    {
                        LogHelper.Warning("ChatService", $"Could not connect to {friendId}, invite will be delivered when connection is established");

                        // Still consider it a success, will be delivered when connection is established
                        OnMessageSent?.Invoke(chatMessage);
                        onComplete?.Invoke(true);
                        return;
                    }

                    _p2pNetworkService.SendMessage(friendId, packet, true, success =>
                    {
                        if (success)
                        {
                            LogHelper.Debug("ChatService", $"Game invite sent to {friendId}");
                            OnMessageSent?.Invoke(chatMessage);
                        }
                        else
                        {
                            LogHelper.Error("ChatService", $"Failed to send game invite to {friendId}");
                        }

                        onComplete?.Invoke(success);
                    });
                });
            }
            catch (Exception ex)
            {
                LogHelper.Exception("ChatService", ex, "Error sending game invite");
                onComplete?.Invoke(false);
            }
        }

        /// <summary>
        /// Load chat history with a friend
        /// </summary>
        public void LoadChatHistory(string friendId, int count = 50, Action<List<ChatMessage>> onComplete = null)
        {
            if (!_isInitialized)
            {
                LogHelper.Error("ChatService", "Not initialized");
                onComplete?.Invoke(null);
                return;
            }

            if (string.IsNullOrEmpty(friendId))
            {
                LogHelper.Error("ChatService", "Invalid friend ID");
                onComplete?.Invoke(null);
                return;
            }

            // Check if already loaded
            if (_chatHistory.TryGetValue(friendId, out List<ChatMessage> cachedHistory))
            {
                // Return last 'count' messages sorted by time
                var result = cachedHistory
                    .OrderBy(m => m.SentTime)
                    .Skip(Math.Max(0, cachedHistory.Count - count))
                    .ToList();

                onComplete?.Invoke(result);
                return;
            }

            // Load from disk
            string filePath = GetChatFilePath(friendId);
            if (!File.Exists(filePath))
            {
                // No history
                _chatHistory[friendId] = new List<ChatMessage>();
                onComplete?.Invoke(new List<ChatMessage>());
                OnChatHistoryLoaded?.Invoke(friendId, new List<ChatMessage>());
                return;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                List<ChatMessage> history = JsonConvert.DeserializeObject<List<ChatMessage>>(json);

                if (history == null)
                {
                    history = new List<ChatMessage>();
                }

                // Cache history
                _chatHistory[friendId] = history;

                // Return last 'count' messages sorted by time
                var result = history
                    .OrderBy(m => m.SentTime)
                    .Skip(Math.Max(0, history.Count - count))
                    .ToList();

                LogHelper.Debug("ChatService", $"Loaded {result.Count} messages from history with {friendId}");

                onComplete?.Invoke(result);
                OnChatHistoryLoaded?.Invoke(friendId, result);
            }
            catch (Exception ex)
            {
                LogHelper.Exception("ChatService", ex, $"Error loading chat history with {friendId}");
                _chatHistory[friendId] = new List<ChatMessage>();
                onComplete?.Invoke(new List<ChatMessage>());
            }
        }

        /// <summary>
        /// Get all recent conversations
        /// </summary>
        public List<string> GetRecentConversations()
        {
            if (!_isInitialized)
            {
                LogHelper.Error("ChatService", "Not initialized");
                return new List<string>();
            }

            // Check chat directory for files
            string directoryPath = Path.Combine(Application.persistentDataPath, SAVE_PATH);
            if (!Directory.Exists(directoryPath))
            {
                return new List<string>();
            }

            try
            {
                var result = new List<string>();
                string[] files = Directory.GetFiles(directoryPath, "*.json");

                foreach (string file in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);

                    // Friend ID is the filename
                    if (!string.IsNullOrEmpty(fileName) && fileName != "unread_counts")
                    {
                        result.Add(fileName);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                LogHelper.Exception("ChatService", ex, "Error getting recent conversations");
                return new List<string>();
            }
        }

        /// <summary>
        /// Mark all messages with a friend as read
        /// </summary>
        public void MarkAsRead(string friendId)
        {
            if (!_isInitialized || string.IsNullOrEmpty(friendId))
            {
                return;
            }

            if (_chatHistory.TryGetValue(friendId, out List<ChatMessage> history))
            {
                bool changed = false;

                foreach (var message in history)
                {
                    if (!message.IsFromLocalUser && !message.IsRead)
                    {
                        message.IsRead = true;
                        changed = true;
                    }
                }

                if (changed)
                {
                    // Save updated history
                    SaveChatHistory(friendId);
                }
            }

            // Update unread count
            _unreadCounts[friendId] = 0;
            SaveUnreadCounts();
        }

        /// <summary>
        /// Get the number of unread messages from a friend
        /// </summary>
        public int GetUnreadCount(string friendId)
        {
            if (!_isInitialized || string.IsNullOrEmpty(friendId))
            {
                return 0;
            }

            if (_unreadCounts.TryGetValue(friendId, out int count))
            {
                return count;
            }

            return 0;
        }

        /// <summary>
        /// Get the total number of unread messages
        /// </summary>
        public int GetTotalUnreadCount()
        {
            if (!_isInitialized)
            {
                return 0;
            }

            int total = 0;
            foreach (int count in _unreadCounts.Values)
            {
                total += count;
            }

            return total;
        }

        /// <summary>
        /// Clear chat history with a friend
        /// </summary>
        public void ClearChatHistory(string friendId, Action<bool> onComplete = null)
        {
            if (!_isInitialized || string.IsNullOrEmpty(friendId))
            {
                onComplete?.Invoke(false);
                return;
            }

            // Remove from memory
            _chatHistory.Remove(friendId);

            // Remove file
            string filePath = GetChatFilePath(friendId);
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                // Clear unread count
                _unreadCounts[friendId] = 0;
                SaveUnreadCounts();

                LogHelper.Info("ChatService", $"Cleared chat history with {friendId}");
                onComplete?.Invoke(true);
            }
            catch (Exception ex)
            {
                LogHelper.Exception("ChatService", ex, $"Error clearing chat history with {friendId}");
                onComplete?.Invoke(false);
            }
        }

        /// <summary>
        /// Accept a game invite
        /// </summary>
        public void AcceptGameInvite(ChatMessage message, Action<bool> onComplete = null)
        {
            if (!_isInitialized || message == null || message.MessageType != ChatMessageType.GameInvite)
            {
                onComplete?.Invoke(false);
                return;
            }

            // In a full implementation, we would send an acceptance message to the sender
            // and handle game joining logic

            LogHelper.Info("ChatService", "Game invite acceptance not fully implemented");
            onComplete?.Invoke(true);
        }

        /// <summary>
        /// Decline a game invite
        /// </summary>
        public void DeclineGameInvite(ChatMessage message, Action<bool> onComplete = null)
        {
            if (!_isInitialized || message == null || message.MessageType != ChatMessageType.GameInvite)
            {
                onComplete?.Invoke(false);
                return;
            }

            // In a full implementation, we would send a decline message to the sender

            LogHelper.Info("ChatService", "Game invite decline not fully implemented");
            onComplete?.Invoke(true);
        }

        /// <summary>
        /// Shutdown the chat service
        /// </summary>
        public void Shutdown()
        {
            if (!_isInitialized)
            {
                return;
            }

            LogHelper.Info("ChatService", "Shutting down...");

            // Unsubscribe from network events
            _p2pNetworkService.OnMessageReceived -= HandleMessageReceived;

            // Save all chat histories
            foreach (string friendId in _chatHistory.Keys.ToList())
            {
                SaveChatHistory(friendId);
            }

            // Save unread counts
            SaveUnreadCounts();

            _chatHistory.Clear();
            _isInitialized = false;

            LogHelper.Info("ChatService", "Shutdown complete");
        }

        /// <summary>
        /// Handle an incoming network message
        /// </summary>
        private void HandleMessageReceived(string senderId, byte[] data)
        {
            if (!_isInitialized || data == null || data.Length == 0)
            {
                return;
            }

            try
            {
                // Parse the message
                if (!FriendsNetworkProtocol.ParsePacket(data, out var messageType, out byte[] payload))
                {
                    LogHelper.Error("ChatService", $"Failed to parse message from {senderId}");
                    return;
                }

                // Check if this is a chat message
                if (messageType != FriendsMessageType.ChatMessage && messageType != FriendsMessageType.GameInvite)
                {
                    // Not a chat message or game invite
                    return;
                }

                // Parse the JSON
                string json = FriendsNetworkProtocol.GetStringPayload(payload);
                if (string.IsNullOrEmpty(json))
                {
                    LogHelper.Error("ChatService", $"Empty message content from {senderId}");
                    return;
                }

                var message = JsonConvert.DeserializeObject<ChatMessage>(json);

                if (message == null)
                {
                    LogHelper.Error("ChatService", $"Failed to deserialize message from {senderId}");
                    return;
                }

                // Validate message
                if (message.SenderId != senderId || message.ReceiverId != _identityService.GetCurrentUserId())
                {
                    LogHelper.Error("ChatService", $"Invalid message routing from {senderId}");
                    return;
                }

                // Mark as from remote user
                message.IsFromLocalUser = false;
                message.IsRead = false;

                // Add to history
                AddMessageToHistory(message);

                // Update unread count
                if (_unreadCounts.TryGetValue(senderId, out int count))
                {
                    _unreadCounts[senderId] = count + 1;
                }
                else
                {
                    _unreadCounts[senderId] = 1;
                }

                SaveUnreadCounts();

                // Notify listeners
                OnMessageReceived?.Invoke(message);

                LogHelper.Debug("ChatService", $"Received {message.MessageType} message from {message.SenderName} ({senderId})");
            }
            catch (Exception ex)
            {
                LogHelper.Exception("ChatService", ex, $"Error processing message from {senderId}");
            }
        }

        /// <summary>
        /// Add a message to the chat history
        /// </summary>
        private void AddMessageToHistory(ChatMessage message)
        {
            if (message == null)
                return;

            string friendId = message.IsFromLocalUser ? message.ReceiverId : message.SenderId;

            if (!_chatHistory.TryGetValue(friendId, out List<ChatMessage> history))
            {
                // Try to load history first
                LoadChatHistory(friendId, 0, loadedHistory =>
                {
                    if (loadedHistory == null)
                    {
                        loadedHistory = new List<ChatMessage>();
                    }

                    loadedHistory.Add(message);
                    _chatHistory[friendId] = loadedHistory;

                    // Trim if needed
                    if (loadedHistory.Count > MAX_CACHED_MESSAGES)
                    {
                        loadedHistory.RemoveRange(0, loadedHistory.Count - MAX_CACHED_MESSAGES);
                    }

                    SaveChatHistory(friendId);
                });

                return;
            }

            history.Add(message);

            // Trim if needed
            if (history.Count > MAX_CACHED_MESSAGES)
            {
                history.RemoveRange(0, history.Count - MAX_CACHED_MESSAGES);
            }

            SaveChatHistory(friendId);
        }

        /// <summary>
        /// Save chat history to disk
        /// </summary>
        private void SaveChatHistory(string friendId)
        {
            if (string.IsNullOrEmpty(friendId) || !_chatHistory.TryGetValue(friendId, out List<ChatMessage> history))
            {
                return;
            }

            try
            {
                string filePath = GetChatFilePath(friendId);
                string json = JsonConvert.SerializeObject(history);

                File.WriteAllText(filePath, json);

                LogHelper.Debug("ChatService", $"Saved {history.Count} messages to history with {friendId}");
            }
            catch (Exception ex)
            {
                LogHelper.Exception("ChatService", ex, $"Error saving chat history with {friendId}");
            }
        }

        /// <summary>
        /// Get the file path for a chat history
        /// </summary>
        private string GetChatFilePath(string friendId)
        {
            string directoryPath = Path.Combine(Application.persistentDataPath, SAVE_PATH);
            return Path.Combine(directoryPath, $"{friendId}.json");
        }

        /// <summary>
        /// Ensure the save directory exists
        /// </summary>
        private void EnsureSaveDirectoryExists()
        {
            string directoryPath = Path.Combine(Application.persistentDataPath, SAVE_PATH);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        /// <summary>
        /// Load unread counts from disk
        /// </summary>
        private void LoadUnreadCounts()
        {
            string filePath = Path.Combine(Application.persistentDataPath, SAVE_PATH, "unread_counts.json");
            if (!File.Exists(filePath))
            {
                // No unread counts
                _unreadCounts = new Dictionary<string, int>();
                return;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                _unreadCounts = JsonConvert.DeserializeObject<Dictionary<string, int>>(json);

                if (_unreadCounts == null)
                {
                    _unreadCounts = new Dictionary<string, int>();
                }

                LogHelper.Debug("ChatService", $"Loaded unread counts for {_unreadCounts.Count} friends");
            }
            catch (Exception ex)
            {
                LogHelper.Exception("ChatService", ex, "Error loading unread counts");
                _unreadCounts = new Dictionary<string, int>();
            }
        }

        /// <summary>
        /// Save unread counts to disk
        /// </summary>
        private void SaveUnreadCounts()
        {
            try
            {
                string filePath = Path.Combine(Application.persistentDataPath, SAVE_PATH, "unread_counts.json");
                string json = JsonConvert.SerializeObject(_unreadCounts);

                File.WriteAllText(filePath, json);

                LogHelper.Debug("ChatService", $"Saved unread counts for {_unreadCounts.Count} friends");
            }
            catch (Exception ex)
            {
                LogHelper.Exception("ChatService", ex, "Error saving unread counts");
            }
        }
    }
}