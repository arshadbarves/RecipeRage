using KitchenClash.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KitchenClash.Application
{
    public interface IFriendsService : IDisposable
    {
        bool IsInitialized { get; }
        string MyFriendCode { get; }
        IReadOnlyList<FriendInfo> Friends { get; }
        IReadOnlyList<FriendInfo> RecentPlayers { get; }
        IReadOnlyList<FriendRequest> PendingRequests { get; }

        event Action OnFriendsListUpdated;
        event Action<FriendInfo> OnFriendAdded;
        event Action<string> OnFriendRemoved;
        event Action<FriendRequest> OnFriendRequestReceived;

        void Initialize();
        Task<FriendRequest> SendFriendRequestAsync(string friendCode);
        Task AcceptFriendRequestAsync(string requestId);
        Task RejectFriendRequestAsync(string requestId);
        Task RefreshFriendRequestsAsync();
        Task RefreshFriendsAsync();
        Task RemoveFriendAsync(string friendCode);
        void AddRecentPlayer(string productUserId, string displayName);
        void InviteToParty(string friendCode);
        FriendInfo GetFriend(string friendCode);
    }
}
