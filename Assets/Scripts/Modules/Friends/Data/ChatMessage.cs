using System;

namespace RecipeRage.Modules.Friends.Data
{
    /// <summary>
    /// Represents a chat message between friends
    /// 
    /// Complexity Rating: 2
    /// </summary>
    [Serializable]
    public class ChatMessage
    {
        /// <summary>
        /// Unique ID of the message
        /// </summary>
        public string MessageId { get; set; }
        
        /// <summary>
        /// ID of the sender
        /// </summary>
        public string SenderId { get; set; }
        
        /// <summary>
        /// Display name of the sender
        /// </summary>
        public string SenderName { get; set; }
        
        /// <summary>
        /// ID of the receiver
        /// </summary>
        public string ReceiverId { get; set; }
        
        /// <summary>
        /// Content of the message
        /// </summary>
        public string Content { get; set; }
        
        /// <summary>
        /// When the message was sent
        /// </summary>
        public DateTime SentTime { get; set; }
        
        /// <summary>
        /// Whether the message has been read
        /// </summary>
        public bool IsRead { get; set; }
        
        /// <summary>
        /// Whether this is sent by the local user
        /// </summary>
        public bool IsFromLocalUser { get; set; }
        
        /// <summary>
        /// Message type
        /// </summary>
        public ChatMessageType MessageType { get; set; }
        
        /// <summary>
        /// Additional data for special message types (like game invites)
        /// </summary>
        public string AdditionalData { get; set; }
        
        /// <summary>
        /// Format the time as a string
        /// </summary>
        /// <returns>Formatted time string</returns>
        public string GetFormattedTime()
        {
            // If today, show time only
            if (SentTime.Date == DateTime.Today)
            {
                return SentTime.ToString("HH:mm");
            }
            
            // If within the last week, show day and time
            if ((DateTime.Now - SentTime).TotalDays < 7)
            {
                return SentTime.ToString("ddd HH:mm");
            }
            
            // Otherwise show date and time
            return SentTime.ToString("MMM d, HH:mm");
        }
    }
    
    /// <summary>
    /// Types of chat messages
    /// 
    /// Complexity Rating: 1
    /// </summary>
    public enum ChatMessageType
    {
        /// <summary>
        /// Normal text message
        /// </summary>
        Text,
        
        /// <summary>
        /// System message (e.g. "Friend is now online")
        /// </summary>
        System,
        
        /// <summary>
        /// Game invite
        /// </summary>
        GameInvite
    }
} 