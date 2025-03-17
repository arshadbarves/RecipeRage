using System;
using System.Collections.Generic;
using RecipeRage.Modules.Friends.Data;

namespace RecipeRage.Modules.Friends.Interfaces
{
    /// <summary>
    /// Interface for the presence service.
    /// Manages online status and activity of friends.
    /// Complexity Rating: 2
    /// </summary>
    public interface IPresenceService
    {
        /// <summary>
        /// Event triggered when a friend's presence changes
        /// </summary>
        event Action<string, PresenceData> OnFriendPresenceChanged;

        /// <summary>
        /// Set the current user's status
        /// </summary>
        /// <param name="status"> New status </param>
        void SetStatus(UserStatus status);

        /// <summary>
        /// Set the current user's activity
        /// </summary>
        /// <param name="activity"> Activity description </param>
        /// <param name="joinable"> Whether friends can join this activity </param>
        /// <param name="joinData"> Data needed to join (if joinable) </param>
        void SetActivity(string activity, bool joinable = false, string joinData = null);

        /// <summary>
        /// Get a friend's presence information
        /// </summary>
        /// <param name="friendId"> Friend's ID </param>
        /// <returns> Presence data or null if not available </returns>
        PresenceData GetFriendPresence(string friendId);

        /// <summary>
        /// Get presence information for all friends
        /// </summary>
        /// <returns> Dictionary mapping friend IDs to presence data </returns>
        IReadOnlyDictionary<string, PresenceData> GetAllFriendsPresence();

        /// <summary>
        /// Check if a friend is online
        /// </summary>
        /// <param name="friendId"> Friend's ID </param>
        /// <returns> True if the friend is online </returns>
        bool IsFriendOnline(string friendId);

        /// <summary>
        /// Initialize the presence service
        /// </summary>
        /// <param name="onComplete"> Callback when initialization is complete </param>
        void Initialize(Action<bool> onComplete = null);

        /// <summary>
        /// Get the current user's presence data
        /// </summary>
        /// <returns> Current user's presence </returns>
        PresenceData GetMyPresence();
    }
}