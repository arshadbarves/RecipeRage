using System;
using System.Collections.Generic;
using RecipeRage.Modules.Friends.Data;

namespace RecipeRage.Modules.Friends.Interfaces
{
    /// <summary>
    /// Interface for the chat service
    /// 
    /// Complexity Rating: 3
    /// </summary>
    public interface IChatService
    {
        /// <summary>
        /// Event triggered when a new message is received
        /// </summary>
        event Action<ChatMessage> OnMessageReceived;
        
        /// <summary>
        /// Event triggered when a message is sent
        /// </summary>
        event Action<ChatMessage> OnMessageSent;
        
        /// <summary>
        /// Event triggered when messages are loaded from history
        /// </summary>
        event Action<string, List<ChatMessage>> OnChatHistoryLoaded;
        
        /// <summary>
        /// Initialize the chat service
        /// </summary>
        /// <param name="onComplete">Callback when initialization is complete</param>
        void Initialize(Action<bool> onComplete = null);
        
        /// <summary>
        /// Send a text message to a friend
        /// </summary>
        /// <param name="friendId">ID of the friend to send to</param>
        /// <param name="message">Message content</param>
        /// <param name="onComplete">Callback when the send operation is complete</param>
        void SendTextMessage(string friendId, string message, Action<bool> onComplete = null);
        
        /// <summary>
        /// Send a game invite to a friend
        /// </summary>
        /// <param name="friendId">ID of the friend to invite</param>
        /// <param name="gameData">Game data for the invite</param>
        /// <param name="message">Optional message to include</param>
        /// <param name="onComplete">Callback when the invite is sent</param>
        void SendGameInvite(string friendId, string gameData, string message = null, Action<bool> onComplete = null);
        
        /// <summary>
        /// Load chat history with a friend
        /// </summary>
        /// <param name="friendId">ID of the friend</param>
        /// <param name="count">Maximum number of messages to load</param>
        /// <param name="onComplete">Callback when history is loaded</param>
        void LoadChatHistory(string friendId, int count = 50, Action<List<ChatMessage>> onComplete = null);
        
        /// <summary>
        /// Get all recent conversations
        /// </summary>
        /// <returns>List of friend IDs with recent conversations</returns>
        List<string> GetRecentConversations();
        
        /// <summary>
        /// Mark all messages with a friend as read
        /// </summary>
        /// <param name="friendId">Friend ID</param>
        void MarkAsRead(string friendId);
        
        /// <summary>
        /// Get the number of unread messages from a friend
        /// </summary>
        /// <param name="friendId">Friend ID</param>
        /// <returns>Number of unread messages</returns>
        int GetUnreadCount(string friendId);
        
        /// <summary>
        /// Get the total number of unread messages
        /// </summary>
        /// <returns>Total number of unread messages</returns>
        int GetTotalUnreadCount();
        
        /// <summary>
        /// Clear chat history with a friend
        /// </summary>
        /// <param name="friendId">Friend ID</param>
        /// <param name="onComplete">Callback when history is cleared</param>
        void ClearChatHistory(string friendId, Action<bool> onComplete = null);
        
        /// <summary>
        /// Accept a game invite
        /// </summary>
        /// <param name="message">The game invite message</param>
        /// <param name="onComplete">Callback when the invite is accepted</param>
        void AcceptGameInvite(ChatMessage message, Action<bool> onComplete = null);
        
        /// <summary>
        /// Decline a game invite
        /// </summary>
        /// <param name="message">The game invite message</param>
        /// <param name="onComplete">Callback when the invite is declined</param>
        void DeclineGameInvite(ChatMessage message, Action<bool> onComplete = null);
        
        /// <summary>
        /// Shutdown the chat service
        /// </summary>
        void Shutdown();
    }
} 