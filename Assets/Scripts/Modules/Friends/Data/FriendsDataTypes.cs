using System;

namespace RecipeRage.Modules.Friends.Data
{
    /// <summary>
    /// Represents a user's online status
    /// 
    /// Complexity Rating: 1
    /// </summary>
    public enum UserStatus
    {
        Offline,
        Online,
        Away,
        DoNotDisturb,
        Playing
    }

    /// <summary>
    /// Represents the type of a friend request
    /// 
    /// Complexity Rating: 1
    /// </summary>
    public enum FriendRequestType
    {
        Sent,
        Received
    }

    /// <summary>
    /// Represents the state of a friendship
    /// 
    /// Complexity Rating: 1
    /// </summary>
    public enum FriendshipState
    {
        NotFriends,
        Friends,
        PendingOutgoing,
        PendingIncoming
    }

    /// <summary>
    /// Contains presence information for a user
    /// 
    /// Complexity Rating: 2
    /// </summary>
    [Serializable]
    public class PresenceData
    {
        /// <summary>
        /// User's unique identifier
        /// </summary>
        public string UserId { get; set; }
        
        /// <summary>
        /// User's display name
        /// </summary>
        public string DisplayName { get; set; }
        
        /// <summary>
        /// User's current status
        /// </summary>
        public UserStatus Status { get; set; }
        
        /// <summary>
        /// Time when the user was last seen online (UTC)
        /// </summary>
        public DateTime LastOnline { get; set; }
        
        /// <summary>
        /// Description of the user's current activity
        /// </summary>
        public string Activity { get; set; }
        
        /// <summary>
        /// Whether the activity can be joined by friends
        /// </summary>
        public bool IsJoinable { get; set; }
        
        /// <summary>
        /// Data needed to join the user's activity (if joinable)
        /// </summary>
        public string JoinData { get; set; }
        
        /// <summary>
        /// Check if the user is currently online
        /// </summary>
        public bool IsOnline => Status != UserStatus.Offline;
    }

    /// <summary>
    /// Represents a friend request
    /// 
    /// Complexity Rating: 2
    /// </summary>
    [Serializable]
    public class FriendRequest
    {
        /// <summary>
        /// Unique identifier for the request
        /// </summary>
        public string RequestId { get; set; }
        
        /// <summary>
        /// User ID of the sender
        /// </summary>
        public string SenderId { get; set; }
        
        /// <summary>
        /// Display name of the sender
        /// </summary>
        public string SenderName { get; set; }
        
        /// <summary>
        /// User ID of the receiver
        /// </summary>
        public string ReceiverId { get; set; }
        
        /// <summary>
        /// When the request was sent
        /// </summary>
        public DateTime SentTime { get; set; }
        
        /// <summary>
        /// Optional message sent with the request
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Type of request (sent or received) from the local user's perspective
        /// </summary>
        public FriendRequestType Type { get; set; }
    }

    /// <summary>
    /// Represents a friend
    /// 
    /// Complexity Rating: 2
    /// </summary>
    [Serializable]
    public class FriendData
    {
        /// <summary>
        /// User ID of the friend
        /// </summary>
        public string UserId { get; set; }
        
        /// <summary>
        /// Display name of the friend
        /// </summary>
        public string DisplayName { get; set; }
        
        /// <summary>
        /// Friend code that can be used to add this friend
        /// </summary>
        public string FriendCode { get; set; }
        
        /// <summary>
        /// Time when this friendship was established
        /// </summary>
        public DateTime FriendSince { get; set; }
        
        /// <summary>
        /// Notes about this friend (can be set by the user)
        /// </summary>
        public string Notes { get; set; }
        
        /// <summary>
        /// Whether this friend is marked as favorite
        /// </summary>
        public bool IsFavorite { get; set; }
    }
} 