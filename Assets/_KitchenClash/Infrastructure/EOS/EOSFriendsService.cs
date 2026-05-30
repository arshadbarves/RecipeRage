using KitchenClash.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KitchenClash.Domain;
using UGSFriends = Unity.Services.Friends;
using UGSModels = Unity.Services.Friends.Models;
using UGSNotifications = Unity.Services.Friends.Notifications;
using UGSExceptions = Unity.Services.Friends.Exceptions;

namespace KitchenClash.Infrastructure.EOS
{
    /// <summary>
    /// Unity Gaming Services Friends implementation
    /// Features:
    /// - Friend requests (send/accept/reject/delete)
    /// - Real friends list with presence
    /// - Online status tracking
    /// - Integration with EOS lobbies
    /// </summary>
    public class EOSFriendsService : IFriendsService
    {
        #region Events

        public event Action OnFriendsListUpdated;
        public event Action<FriendInfo> OnFriendAdded;
        public event Action<string> OnFriendRemoved;
        public event Action<FriendRequest> OnFriendRequestReceived;

        #endregion

        #region Properties

        public bool IsInitialized { get; private set; }
        public string MyFriendCode => _authManager?.PlayerId ?? "UNKNOWN";
        public IReadOnlyList<FriendInfo> Friends => _friends.AsReadOnly();
        public IReadOnlyList<FriendInfo> RecentPlayers => _recentPlayers.AsReadOnly();
        public IReadOnlyList<FriendRequest> PendingRequests => _pendingRequests.AsReadOnly();

        #endregion

        #region Private Fields

        private readonly ILobbyManager _lobbyManager;
        private readonly IAuthService _authManager;
        private readonly List<FriendInfo> _friends = new List<FriendInfo>();
        private readonly List<FriendInfo> _recentPlayers = new List<FriendInfo>();
        private readonly List<FriendRequest> _pendingRequests = new List<FriendRequest>();

        private UGSFriends.IFriendsService _ugsInstance;

        #endregion

        #region Initialization

        public EOSFriendsService(ILobbyManager lobbyManager, IAuthService authManager)
        {
            _lobbyManager = lobbyManager ?? throw new ArgumentNullException(nameof(lobbyManager));
            _authManager = authManager ?? throw new ArgumentNullException(nameof(authManager));
        }

        public async void Initialize()
        {
            if (IsInitialized)
            {
                GameLogger.LogWarning("Already initialized");
                return;
            }

            GameLogger.Log("Initializing UGS Friends Service...");

            try
            {
                if (!_authManager.IsUgsSignedIn)
                {
                    GameLogger.LogError("UGS not signed in - cannot initialize friends");
                    return;
                }

                _ugsInstance = UGSFriends.FriendsService.Instance;

                await _ugsInstance.InitializeAsync();

                SubscribeToEvents();

                UpdateLocalFriends();
                UpdateLocalRequests();

                IsInitialized = true;
                GameLogger.Log($"Initialized - Player ID: {MyFriendCode}");
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Initialization failed: {ex.Message}");
            }
        }

        #endregion

        #region Public Methods - Friend Requests

        public async Task<FriendRequest> SendFriendRequestAsync(string friendCode)
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("Service not initialized");
            }

            if (string.IsNullOrEmpty(friendCode))
            {
                throw new ArgumentException("Friend code cannot be empty");
            }

            if (friendCode == MyFriendCode)
            {
                throw new ArgumentException("Cannot send request to yourself");
            }

            GameLogger.Log($"Sending friend request to: {friendCode}");

            try
            {
                UGSModels.Relationship relationship = await _ugsInstance.AddFriendAsync(friendCode);

                GameLogger.Log($"Friend request sent to {friendCode}");

                return new FriendRequest
                {
                    Id = relationship.Id,
                    FromUserId = MyFriendCode,
                    FromUserName = "You",
                    FromFriendCode = MyFriendCode,
                    ToUserId = friendCode,
                    Status = "pending",
                    CreatedAt = DateTime.UtcNow
                };
            }
            catch (UGSExceptions.FriendsServiceException ex)
            {
                GameLogger.LogError($"Failed to send friend request: {ex.Message}");
                throw new Exception($"Failed to send friend request: {ex.Message}");
            }
        }

        public async Task AcceptFriendRequestAsync(string requestId)
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("Service not initialized");
            }

            GameLogger.Log($"Accepting friend request: {requestId}");

            try
            {
                UGSModels.Relationship request = _ugsInstance.IncomingFriendRequests.FirstOrDefault(r => r.Id == requestId);

                if (request == null)
                {
                    throw new Exception("Friend request not found");
                }

                await _ugsInstance.AddFriendAsync(request.Member.Id);

                GameLogger.Log("Friend request accepted");
            }
            catch (UGSExceptions.FriendsServiceException ex)
            {
                GameLogger.LogError($"Failed to accept friend request: {ex.Message}");
                throw;
            }
        }

        public async Task RejectFriendRequestAsync(string requestId)
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("Service not initialized");
            }

            GameLogger.Log($"Rejecting friend request: {requestId}");

            try
            {
                UGSModels.Relationship request = _ugsInstance.IncomingFriendRequests.FirstOrDefault(r => r.Id == requestId);

                if (request == null)
                {
                    throw new Exception("Friend request not found");
                }

                await _ugsInstance.DeleteIncomingFriendRequestAsync(request.Member.Id);

                GameLogger.Log("Friend request rejected");
            }
            catch (UGSExceptions.FriendsServiceException ex)
            {
                GameLogger.LogError($"Failed to reject friend request: {ex.Message}");
                throw;
            }
        }

        public async Task RefreshFriendRequestsAsync()
        {
            if (!IsInitialized)
            {
                return;
            }

            GameLogger.Log("Refreshing friend requests...");

            try
            {
                await _ugsInstance.ForceRelationshipsRefreshAsync();

                UpdateLocalRequests();

                GameLogger.Log($"Found {_pendingRequests.Count} pending requests");
            }
            catch (UGSExceptions.FriendsServiceException ex)
            {
                GameLogger.LogError($"Failed to refresh friend requests: {ex.Message}");
            }
        }

        #endregion

        #region Public Methods - Friends

        public async Task RefreshFriendsAsync()
        {
            if (!IsInitialized)
            {
                return;
            }

            GameLogger.Log("Refreshing friends list...");

            try
            {
                await _ugsInstance.ForceRelationshipsRefreshAsync();

                UpdateLocalFriends();

                GameLogger.Log($"Loaded {_friends.Count} friends");
            }
            catch (UGSExceptions.FriendsServiceException ex)
            {
                GameLogger.LogError($"Failed to refresh friends: {ex.Message}");
            }
        }

        public async Task RemoveFriendAsync(string friendCode)
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("Service not initialized");
            }

            FriendInfo friend = _friends.FirstOrDefault(f => f.FriendCode == friendCode);
            if (friend == null)
            {
                throw new Exception("Friend not found");
            }

            GameLogger.Log($"Removing friend: {friend.DisplayName}");

            try
            {
                await _ugsInstance.DeleteFriendAsync(friendCode);

                GameLogger.Log("Friend removed");
            }
            catch (UGSExceptions.FriendsServiceException ex)
            {
                GameLogger.LogError($"Failed to remove friend: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Public Methods - Recent Players

        public async void AddRecentPlayer(string productUserId, string displayName)
        {
            if (!IsInitialized || string.IsNullOrEmpty(productUserId))
            {
                return;
            }

            try
            {
                if (productUserId == _authManager.EosProductUserId)
                {
                    return;
                }

                GameLogger.Log($"Adding recent player: {displayName}");

                _recentPlayers.RemoveAll(p => p.ProductUserId == productUserId);

                _recentPlayers.Insert(0, new FriendInfo
                {
                    FriendCode = productUserId,
                    ProductUserId = productUserId,
                    DisplayName = displayName,
                    LastSeen = DateTime.UtcNow,
                    IsOnline = true,
                    IsRecent = true
                });

                if (_recentPlayers.Count > 20)
                {
                    _recentPlayers.RemoveRange(20, _recentPlayers.Count - 20);
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Error adding recent player: {ex.Message}");
            }
        }

        #endregion

        #region Public Methods - Party Invites

        public void InviteToParty(string friendCode)
        {
            if (!_lobbyManager.IsInParty)
            {
                GameLogger.LogWarning("Not in a party");
                return;
            }

            FriendInfo friend = _friends.FirstOrDefault(f => f.FriendCode == friendCode);
            if (friend == null)
            {
                GameLogger.LogError($"Friend not found: {friendCode}");
                return;
            }

            if (string.IsNullOrEmpty(friend.ProductUserId))
            {
                GameLogger.LogError($"Invalid ProductUserId for friend: {friendCode}");
                return;
            }

            GameLogger.Log($"Inviting {friend.DisplayName} to party");
            _lobbyManager.InviteToParty(friend.ProductUserId);
        }

        public FriendInfo GetFriend(string friendCode)
        {
            return _friends.FirstOrDefault(f => f.FriendCode == friendCode);
        }

        #endregion

        #region Private Methods - Local Cache Updates

        private void UpdateLocalFriends()
        {
            _friends.Clear();

            foreach (UGSModels.Relationship relationship in _ugsInstance.Friends)
            {
                UGSModels.Member member = relationship.Member;
                UGSModels.Profile profile = member.Profile;
                UGSModels.Presence presence = member.Presence;

                bool isOnline = presence != null && presence.Availability == UGSModels.Availability.Online;
                DateTime lastSeen = presence?.LastSeen ?? DateTime.MinValue;

                _friends.Add(new FriendInfo
                {
                    FriendCode = member.Id,
                    ProductUserId = null,
                    DisplayName = profile?.Name ?? "Unknown",
                    AddedAt = DateTime.UtcNow,
                    LastSeen = lastSeen,
                    IsOnline = isOnline,
                    IsRecent = false
                });
            }

            _friends.Sort((a, b) =>
            {
                if (a.IsOnline != b.IsOnline)
                {
                    return b.IsOnline.CompareTo(a.IsOnline);
                }

                return string.Compare(a.DisplayName, b.DisplayName, StringComparison.Ordinal);
            });

            OnFriendsListUpdated?.Invoke();
        }

        private void UpdateLocalRequests()
        {
            _pendingRequests.Clear();

            foreach (UGSModels.Relationship request in _ugsInstance.IncomingFriendRequests)
            {
                _pendingRequests.Add(new FriendRequest
                {
                    Id = request.Id,
                    FromUserId = request.Member.Id,
                    FromUserName = request.Member.Profile?.Name ?? "Unknown",
                    FromFriendCode = request.Member.Id,
                    ToUserId = MyFriendCode,
                    Status = "pending",
                    CreatedAt = DateTime.UtcNow
                });
            }

            foreach (FriendRequest request in _pendingRequests)
            {
                OnFriendRequestReceived?.Invoke(request);
            }
        }

        #endregion

        #region Event Subscription

        private void SubscribeToEvents()
        {
            _ugsInstance.RelationshipAdded += OnRelationshipAdded;
            _ugsInstance.RelationshipDeleted += OnRelationshipDeleted;

            _ugsInstance.PresenceUpdated += OnPresenceUpdated;

            GameLogger.Log("Subscribed to UGS Friends events");
        }

        private void UnsubscribeFromEvents()
        {
            if (_ugsInstance != null)
            {
                _ugsInstance.RelationshipAdded -= OnRelationshipAdded;
                _ugsInstance.RelationshipDeleted -= OnRelationshipDeleted;
                _ugsInstance.PresenceUpdated -= OnPresenceUpdated;
            }
        }

        #endregion

        #region Event Handlers

        private void OnRelationshipAdded(UGSNotifications.IRelationshipAddedEvent eventData)
        {
            UGSModels.Relationship relationship = eventData.Relationship;
            GameLogger.Log($"Relationship added: {relationship.Member.Profile?.Name ?? "Unknown"}");

            if (relationship.Type == UGSModels.RelationshipType.Friend)
            {
                UpdateLocalFriends();

                FriendInfo friend = _friends.FirstOrDefault(f => f.FriendCode == relationship.Member.Id);
                if (friend != null)
                {
                    OnFriendAdded?.Invoke(friend);
                }
            }
            else if (relationship.Type == UGSModels.RelationshipType.FriendRequest)
            {
                UpdateLocalRequests();
            }
        }

        private void OnRelationshipDeleted(UGSNotifications.IRelationshipDeletedEvent eventData)
        {
            UGSModels.Relationship relationship = eventData.Relationship;
            GameLogger.Log($"Relationship deleted: {relationship.Member.Profile?.Name ?? "Unknown"}");

            string friendCode = relationship.Member.Id;
            _friends.RemoveAll(f => f.FriendCode == friendCode);
            _pendingRequests.RemoveAll(r => r.FromFriendCode == friendCode);

            OnFriendRemoved?.Invoke(friendCode);
            OnFriendsListUpdated?.Invoke();
        }

        private void OnPresenceUpdated(UGSNotifications.IPresenceUpdatedEvent eventData)
        {
            UGSModels.Presence presence = eventData.Presence;
            GameLogger.Log($"Presence updated for: {eventData.ID} - {presence.Availability}");

            FriendInfo friend = _friends.FirstOrDefault(f => f.FriendCode == eventData.ID);
            if (friend != null)
            {
                friend.IsOnline = presence.Availability == UGSModels.Availability.Online;
                friend.LastSeen = presence.LastSeen;

                OnFriendsListUpdated?.Invoke();
            }
        }

        #endregion

        #region Cleanup

        public void Dispose()
        {
            UnsubscribeFromEvents();
            _friends.Clear();
            _recentPlayers.Clear();
            _pendingRequests.Clear();
            IsInitialized = false;
        }

        #endregion
    }
}
