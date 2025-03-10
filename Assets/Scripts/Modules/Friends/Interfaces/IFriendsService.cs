using System;
using System.Collections.Generic;
using RecipeRage.Modules.Friends.Data;

namespace RecipeRage.Modules.Friends.Interfaces
{
    /// <summary>
    /// Interface for the friends service.
    /// Manages friend relationships, requests, and interactions.
    /// 
    /// Complexity Rating: 2
    /// </summary>
    public interface IFriendsService
    {
        /// <summary>
        /// Event triggered when a friend is added
        /// </summary>
        event Action<FriendData> OnFriendAdded;
        
        /// <summary>
        /// Event triggered when a friend is removed
        /// </summary>
        event Action<FriendData> OnFriendRemoved;
        
        /// <summary>
        /// Event triggered when a friend request is received
        /// </summary>
        event Action<FriendRequest> OnFriendRequestReceived;
        
        /// <summary>
        /// Event triggered when a friend request is accepted
        /// </summary>
        event Action<FriendData> OnFriendRequestAccepted;
        
        /// <summary>
        /// Event triggered when a friend request is rejected
        /// </summary>
        event Action<string> OnFriendRequestRejected;
        
        /// <summary>
        /// Get a list of all friends
        /// </summary>
        /// <returns>List of friends</returns>
        IReadOnlyList<FriendData> GetFriends();
        
        /// <summary>
        /// Get a friend by their ID
        /// </summary>
        /// <param name="friendId">Friend's unique identifier</param>
        /// <returns>Friend data or null if not found</returns>
        FriendData GetFriend(string friendId);
        
        /// <summary>
        /// Check if a user is a friend
        /// </summary>
        /// <param name="userId">User ID to check</param>
        /// <returns>True if the user is a friend</returns>
        bool IsFriend(string userId);
        
        /// <summary>
        /// Add a friend using their friend code
        /// </summary>
        /// <param name="friendCode">Friend code to add</param>
        /// <param name="onComplete">Callback when operation completes</param>
        void AddFriendByCode(string friendCode, Action<bool, string> onComplete = null);
        
        /// <summary>
        /// Remove a friend by their ID
        /// </summary>
        /// <param name="friendId">Friend's ID to remove</param>
        /// <param name="onComplete">Callback when operation completes</param>
        void RemoveFriend(string friendId, Action<bool> onComplete = null);
        
        /// <summary>
        /// Send a friend request to a user
        /// </summary>
        /// <param name="friendCode">Friend code of the recipient</param>
        /// <param name="onComplete">Callback when operation completes</param>
        void SendFriendRequest(string friendCode, Action<bool, string> onComplete = null);
        
        /// <summary>
        /// Accept a friend request
        /// </summary>
        /// <param name="requestId">ID of the friend request</param>
        /// <param name="onComplete">Callback when operation completes</param>
        void AcceptFriendRequest(string requestId, Action<bool> onComplete = null);
        
        /// <summary>
        /// Reject a friend request
        /// </summary>
        /// <param name="requestId">ID of the friend request</param>
        /// <param name="onComplete">Callback when operation completes</param>
        void RejectFriendRequest(string requestId, Action<bool> onComplete = null);
        
        /// <summary>
        /// Get a list of pending friend requests
        /// </summary>
        /// <returns>List of pending friend requests</returns>
        IReadOnlyList<FriendRequest> GetPendingFriendRequests();
        
        /// <summary>
        /// Get the current user's friend code
        /// </summary>
        /// <returns>Friend code for the current user</returns>
        string GetMyFriendCode();
        
        /// <summary>
        /// Initialize the friends service
        /// </summary>
        /// <param name="onComplete">Callback when initialization is complete</param>
        void Initialize(Action<bool> onComplete = null);
    }
} 