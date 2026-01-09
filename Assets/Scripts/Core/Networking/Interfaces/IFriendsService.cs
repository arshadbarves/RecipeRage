using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Epic.OnlineServices;

namespace Core.Core.Networking.Interfaces
{
    /// <summary>
    /// Interface for custom Friends service
    /// Uses Friend Codes (like PUBG/Brawl Stars) instead of Epic accounts
    /// </summary>
    public interface IFriendsService
    {
        #region Events

        /// <summary>
        /// Fired when friends list is updated
        /// </summary>
        event Action OnFriendsListUpdated;

        /// <summary>
        /// Fired when a friend is added
        /// </summary>
        event Action<FriendInfo> OnFriendAdded;

        /// <summary>
        /// Fired when a friend is removed
        /// </summary>
        event Action<string> OnFriendRemoved;

        /// <summary>
        /// Fired when a friend request is received
        /// </summary>
        event Action<FriendRequest> OnFriendRequestReceived;

        #endregion

        #region Properties

        /// <summary>
        /// Is the service initialized
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// My friend code
        /// </summary>
        string MyFriendCode { get; }

        /// <summary>
        /// Current friends list
        /// </summary>
        IReadOnlyList<FriendInfo> Friends { get; }

        /// <summary>
        /// Recent players list
        /// </summary>
        IReadOnlyList<FriendInfo> RecentPlayers { get; }

        /// <summary>
        /// Pending friend requests (received)
        /// </summary>
        IReadOnlyList<FriendRequest> PendingRequests { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Initialize the friends service
        /// </summary>
        void Initialize();

        /// <summary>
        /// Send friend request by friend code
        /// </summary>
        /// <param name="friendCode">Friend's code (e.g., "ABC12345")</param>
        Task<FriendRequest> SendFriendRequestAsync(string friendCode);

        /// <summary>
        /// Accept friend request
        /// </summary>
        /// <param name="requestId">Request ID</param>
        Task AcceptFriendRequestAsync(string requestId);

        /// <summary>
        /// Reject friend request
        /// </summary>
        /// <param name="requestId">Request ID</param>
        Task RejectFriendRequestAsync(string requestId);

        /// <summary>
        /// Get pending friend requests
        /// </summary>
        Task RefreshFriendRequestsAsync();

        /// <summary>
        /// Remove friend
        /// </summary>
        /// <param name="friendCode">Friend's code</param>
        Task RemoveFriendAsync(string friendCode);

        /// <summary>
        /// Add recent player (after match)
        /// </summary>
        /// <param name="productUserId">Player's ProductUserId</param>
        /// <param name="displayName">Player's display name</param>
        void AddRecentPlayer(ProductUserId productUserId, string displayName);

        /// <summary>
        /// Send party invite to friend
        /// </summary>
        /// <param name="friendCode">Friend's code</param>
        void InviteToParty(string friendCode);

        /// <summary>
        /// Get friend by code
        /// </summary>
        FriendInfo GetFriend(string friendCode);

        /// <summary>
        /// Refresh friends list from backend
        /// </summary>
        Task RefreshFriendsAsync();

        #endregion
    }

    /// <summary>
    /// Friend information
    /// </summary>
    public class FriendInfo
    {
        public string FriendCode { get; set; }
        public ProductUserId ProductUserId { get; set; }
        public string DisplayName { get; set; }
        public DateTime AddedAt { get; set; }
        public DateTime LastSeen { get; set; }
        public bool IsOnline { get; set; }
        public bool IsRecent { get; set; }
    }

    /// <summary>
    /// Friend request information
    /// </summary>
    public class FriendRequest
    {
        public string Id { get; set; }
        public string FromUserId { get; set; }
        public string FromUserName { get; set; }
        public string FromFriendCode { get; set; }
        public string ToUserId { get; set; }
        public string Status { get; set; } // pending, accepted, rejected
        public DateTime CreatedAt { get; set; }
    }
}
