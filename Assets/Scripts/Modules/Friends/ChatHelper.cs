using System;
using System.Collections.Generic;
using RecipeRage.Modules.Friends.Data;
using RecipeRage.Modules.Friends.Interfaces;
using RecipeRage.Modules.Friends.Core;
using RecipeRage.Modules.Friends.Network;
using RecipeRage.Modules.Logging;
using UnityEngine;

namespace RecipeRage.Modules.Friends
{
    /// <summary>
    /// Static helper class for easy access to chat functionality
    /// 
    /// Complexity Rating: 2
    /// </summary>
    public static class ChatHelper
    {
        private static IChatService _chatService;
        private static bool _isInitialized;

        /// <summary>
        /// Initialize the chat system
        /// </summary>
        /// <param name="onComplete">Callback when initialization is complete</param>
        public static void Initialize(Action<bool> onComplete = null)
        {
            if (_isInitialized)
            {
                LogHelper.Warning("ChatHelper", "Already initialized");
                onComplete?.Invoke(true);
                return;
            }

            if (!FriendsHelper.IsInitialized)
            {
                LogHelper.Error("ChatHelper", "Friends system not initialized");
                onComplete?.Invoke(false);
                return;
            }

            LogHelper.Info("ChatHelper", "Initializing chat system...");

            // Create the chat service
            _chatService = new ChatService(
                new IdentityService(),
                new EOSP2PNetworkService()
            );

            // Initialize the service
            _chatService.Initialize(success =>
            {
                if (success)
                {
                    _isInitialized = true;
                    LogHelper.Info("ChatHelper", "Chat system initialized successfully");
                }
                else
                {
                    LogHelper.Error("ChatHelper", "Failed to initialize chat system");
                }

                onComplete?.Invoke(success);
            });
        }

        /// <summary>
        /// Shutdown the chat system
        /// </summary>
        public static void Shutdown()
        {
            if (!_isInitialized)
                return;

            LogHelper.Info("ChatHelper", "Shutting down chat system...");

            _chatService.Shutdown();
            _chatService = null;
            _isInitialized = false;

            LogHelper.Info("ChatHelper", "Chat system shutdown complete");
        }

        /// <summary>
        /// Send a text message to a friend
        /// </summary>
        /// <param name="friendId">Friend ID</param>
        /// <param name="message">Message content</param>
        /// <param name="onComplete">Callback when the message is sent</param>
        public static void SendTextMessage(string friendId, string message, Action<bool> onComplete = null)
        {
            if (!EnsureInitialized())
            {
                onComplete?.Invoke(false);
                return;
            }

            _chatService.SendTextMessage(friendId, message, onComplete);
        }

        /// <summary>
        /// Send a game invite to a friend
        /// </summary>
        /// <param name="friendId">Friend ID</param>
        /// <param name="gameData">Game data</param>
        /// <param name="message">Optional message</param>
        /// <param name="onComplete">Callback when the invite is sent</param>
        public static void SendGameInvite(string friendId, string gameData, string message = null, Action<bool> onComplete = null)
        {
            if (!EnsureInitialized())
            {
                onComplete?.Invoke(false);
                return;
            }

            _chatService.SendGameInvite(friendId, gameData, message, onComplete);
        }

        /// <summary>
        /// Load chat history with a friend
        /// </summary>
        /// <param name="friendId">Friend ID</param>
        /// <param name="count">Maximum number of messages to load</param>
        /// <param name="onComplete">Callback when history is loaded</param>
        public static void LoadChatHistory(string friendId, int count = 50, Action<List<ChatMessage>> onComplete = null)
        {
            if (!EnsureInitialized())
            {
                onComplete?.Invoke(null);
                return;
            }

            _chatService.LoadChatHistory(friendId, count, onComplete);
        }

        /// <summary>
        /// Get all recent conversations
        /// </summary>
        /// <returns>List of friend IDs with recent conversations</returns>
        public static List<string> GetRecentConversations()
        {
            if (!EnsureInitialized())
            {
                return new List<string>();
            }

            return _chatService.GetRecentConversations();
        }

        /// <summary>
        /// Mark messages with a friend as read
        /// </summary>
        /// <param name="friendId">Friend ID</param>
        public static void MarkAsRead(string friendId)
        {
            if (!EnsureInitialized())
            {
                return;
            }

            _chatService.MarkAsRead(friendId);
        }

        /// <summary>
        /// Get the number of unread messages from a friend
        /// </summary>
        /// <param name="friendId">Friend ID</param>
        /// <returns>Number of unread messages</returns>
        public static int GetUnreadCount(string friendId)
        {
            if (!EnsureInitialized())
            {
                return 0;
            }

            return _chatService.GetUnreadCount(friendId);
        }

        /// <summary>
        /// Get the total number of unread messages
        /// </summary>
        /// <returns>Total number of unread messages</returns>
        public static int GetTotalUnreadCount()
        {
            if (!EnsureInitialized())
            {
                return 0;
            }

            return _chatService.GetTotalUnreadCount();
        }

        /// <summary>
        /// Clear chat history with a friend
        /// </summary>
        /// <param name="friendId">Friend ID</param>
        /// <param name="onComplete">Callback when history is cleared</param>
        public static void ClearChatHistory(string friendId, Action<bool> onComplete = null)
        {
            if (!EnsureInitialized())
            {
                onComplete?.Invoke(false);
                return;
            }

            _chatService.ClearChatHistory(friendId, onComplete);
        }

        /// <summary>
        /// Accept a game invite
        /// </summary>
        /// <param name="message">The game invite message</param>
        /// <param name="onComplete">Callback when the invite is accepted</param>
        public static void AcceptGameInvite(ChatMessage message, Action<bool> onComplete = null)
        {
            if (!EnsureInitialized())
            {
                onComplete?.Invoke(false);
                return;
            }

            _chatService.AcceptGameInvite(message, onComplete);
        }

        /// <summary>
        /// Decline a game invite
        /// </summary>
        /// <param name="message">The game invite message</param>
        /// <param name="onComplete">Callback when the invite is declined</param>
        public static void DeclineGameInvite(ChatMessage message, Action<bool> onComplete = null)
        {
            if (!EnsureInitialized())
            {
                onComplete?.Invoke(false);
                return;
            }

            _chatService.DeclineGameInvite(message, onComplete);
        }

        /// <summary>
        /// Register event handlers for chat events
        /// </summary>
        /// <param name="onMessageReceived">Handler for message received events</param>
        /// <param name="onMessageSent">Handler for message sent events</param>
        /// <param name="onChatHistoryLoaded">Handler for chat history loaded events</param>
        public static void RegisterEventHandlers(
            Action<ChatMessage> onMessageReceived = null,
            Action<ChatMessage> onMessageSent = null,
            Action<string, List<ChatMessage>> onChatHistoryLoaded = null)
        {
            if (!EnsureInitialized())
            {
                return;
            }

            if (onMessageReceived != null)
            {
                _chatService.OnMessageReceived += onMessageReceived;
            }

            if (onMessageSent != null)
            {
                _chatService.OnMessageSent += onMessageSent;
            }

            if (onChatHistoryLoaded != null)
            {
                _chatService.OnChatHistoryLoaded += onChatHistoryLoaded;
            }
        }

        /// <summary>
        /// Unregister event handlers for chat events
        /// </summary>
        /// <param name="onMessageReceived">Handler for message received events</param>
        /// <param name="onMessageSent">Handler for message sent events</param>
        /// <param name="onChatHistoryLoaded">Handler for chat history loaded events</param>
        public static void UnregisterEventHandlers(
            Action<ChatMessage> onMessageReceived = null,
            Action<ChatMessage> onMessageSent = null,
            Action<string, List<ChatMessage>> onChatHistoryLoaded = null)
        {
            if (!_isInitialized)
            {
                return;
            }

            if (onMessageReceived != null)
            {
                _chatService.OnMessageReceived -= onMessageReceived;
            }

            if (onMessageSent != null)
            {
                _chatService.OnMessageSent -= onMessageSent;
            }

            if (onChatHistoryLoaded != null)
            {
                _chatService.OnChatHistoryLoaded -= onChatHistoryLoaded;
            }
        }

        /// <summary>
        /// Ensure the chat system is initialized
        /// </summary>
        /// <returns>True if initialized</returns>
        private static bool EnsureInitialized()
        {
            if (!_isInitialized)
            {
                LogHelper.Error("ChatHelper", "Chat system not initialized");
                return false;
            }

            return true;
        }
    }
}