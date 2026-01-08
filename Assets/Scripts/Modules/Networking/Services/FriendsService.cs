using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Modules.Logging;
using Modules.Networking.Interfaces;
using Core.SDK;
using Epic.OnlineServices;
using UGSFriends = Unity.Services.Friends;
using UGSModels = Unity.Services.Friends.Models;
using UGSNotifications = Unity.Services.Friends.Notifications;
using UGSExceptions = Unity.Services.Friends.Exceptions;

namespace Modules.Networking.Services
{
    /// <summary>
    /// Unity Gaming Services Friends implementation
    /// Features:
    /// - Friend requests (send/accept/reject/delete)
    /// - Real friends list with presence
    /// - Online status tracking
    /// - Integration with EOS lobbies
    /// </summary>
    public class FriendsService : IFriendsService
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
        private readonly UGSAuthenticationManager _authManager;
        private readonly List<FriendInfo> _friends = new List<FriendInfo>();
        private readonly List<FriendInfo> _recentPlayers = new List<FriendInfo>();
        private readonly List<FriendRequest> _pendingRequests = new List<FriendRequest>();

        private UGSFriends.IFriendsService _ugsInstance;

        #endregion

        #region Initialization

        public FriendsService(ILobbyManager lobbyManager, UGSAuthenticationManager authManager)
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
                // Ensure UGS is authenticated
                if (!_authManager.IsSignedIn)
                {
                    GameLogger.LogError("UGS not signed in - cannot initialize friends");
                    return;
                }

                // Get UGS Friends instance
                _ugsInstance = UGSFriends.FriendsService.Instance;

                // Initialize the service
                await _ugsInstance.InitializeAsync();

                // Subscribe to events
                SubscribeToEvents();

                // Initial data load
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
                throw new InvalidOperationException("Service not initialized");

            if (string.IsNullOrEmpty(friendCode))
                throw new ArgumentException("Friend code cannot be empty");

            if (friendCode == MyFriendCode)
                throw new ArgumentException("Cannot send request to yourself");

            GameLogger.Log($"Sending friend request to: {friendCode}");

            try
            {
                // Send request using UGS (by member ID, which is the PlayerId)
                var relationship = await _ugsInstance.AddFriendAsync(friendCode);

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
                throw new InvalidOperationException("Service not initialized");

            GameLogger.Log($"Accepting friend request: {requestId}");

            try
            {
                // Find the incoming request by ID
                var request = _ugsInstance.IncomingFriendRequests.FirstOrDefault(r => r.Id == requestId);

                if (request == null)
                    throw new Exception("Friend request not found");

                // Accept the request by adding them as a friend
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
                throw new InvalidOperationException("Service not initialized");

            GameLogger.Log($"Rejecting friend request: {requestId}");

            try
            {
                // Find the incoming request by ID
                var request = _ugsInstance.IncomingFriendRequests.FirstOrDefault(r => r.Id == requestId);

                if (request == null)
                    throw new Exception("Friend request not found");

                // Delete/reject the request
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
                return;

            GameLogger.Log("Refreshing friend requests...");

            try
            {
                // Force refresh relationships from server
                await _ugsInstance.ForceRelationshipsRefreshAsync();

                // Update local cache
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
                return;

            GameLogger.Log("Refreshing friends list...");

            try
            {
                // Force refresh relationships from server
                await _ugsInstance.ForceRelationshipsRefreshAsync();

                // Update local cache
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
                throw new InvalidOperationException("Service not initialized");

            var friend = _friends.FirstOrDefault(f => f.FriendCode == friendCode);
            if (friend == null)
                throw new Exception("Friend not found");

            GameLogger.Log($"Removing friend: {friend.DisplayName}");

            try
            {
                // Delete friendship in UGS (use member ID)
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

        public async void AddRecentPlayer(ProductUserId productUserId, string displayName)
        {
            if (!IsInitialized || productUserId == null || !productUserId.IsValid())
                return;

            try
            {
                var playerIdStr = productUserId.ToString();

                // Don't add yourself
                if (playerIdStr == _authManager.EosProductUserId)
                    return;

                GameLogger.Log($"Adding recent player: {displayName}");

                // Note: We can't reverse-lookup Unity PlayerId from EOS ProductUserId
                // without server-side tracking. For recent players from lobbies,
                // we only have their EOS ProductUserId, so use that as FriendCode.
                // When they become actual friends via Unity Friends API, we'll get their Unity PlayerId.

                // Update local list
                _recentPlayers.RemoveAll(p => p.ProductUserId == productUserId);

                _recentPlayers.Insert(0, new FriendInfo
                {
                    FriendCode = playerIdStr, // Use EOS ProductUserId as FriendCode for recent players
                    ProductUserId = productUserId,
                    DisplayName = displayName,
                    LastSeen = DateTime.UtcNow,
                    IsOnline = true,
                    IsRecent = true
                });

                // Keep only last 20
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

            var friend = _friends.FirstOrDefault(f => f.FriendCode == friendCode);
            if (friend == null)
            {
                GameLogger.LogError($"Friend not found: {friendCode}");
                return;
            }

            if (friend.ProductUserId == null || !friend.ProductUserId.IsValid())
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

            foreach (var relationship in _ugsInstance.Friends)
            {
                var member = relationship.Member;
                var profile = member.Profile;
                var presence = member.Presence;

                // Determine online status from presence
                bool isOnline = presence != null && presence.Availability == UGSModels.Availability.Online;
                DateTime lastSeen = presence?.LastSeen ?? DateTime.MinValue;

                // Note: We can't reverse-lookup EOS ProductUserId from Unity PlayerId
                // without server-side tracking. For Unity Friends, we only have Unity PlayerId.
                // The ProductUserId field will be null for friends from Unity Friends API.
                // They can still join lobbies by sending invites through Unity Friends system.

                _friends.Add(new FriendInfo
                {
                    FriendCode = member.Id, // Unity PlayerId for friends from Unity Friends API
                    ProductUserId = null, // Can't reverse-lookup without server-side tracking
                    DisplayName = profile?.Name ?? "Unknown",
                    AddedAt = DateTime.UtcNow, // UGS doesn't provide this
                    LastSeen = lastSeen,
                    IsOnline = isOnline,
                    IsRecent = false
                });
            }

            // Sort: online first, then by name
            _friends.Sort((a, b) =>
            {
                if (a.IsOnline != b.IsOnline)
                    return b.IsOnline.CompareTo(a.IsOnline);
                return string.Compare(a.DisplayName, b.DisplayName, StringComparison.Ordinal);
            });

            OnFriendsListUpdated?.Invoke();
        }

        private void UpdateLocalRequests()
        {
            _pendingRequests.Clear();

            foreach (var request in _ugsInstance.IncomingFriendRequests)
            {
                _pendingRequests.Add(new FriendRequest
                {
                    Id = request.Id,
                    FromUserId = request.Member.Id,
                    FromUserName = request.Member.Profile?.Name ?? "Unknown",
                    FromFriendCode = request.Member.Id,
                    ToUserId = MyFriendCode,
                    Status = "pending",
                    CreatedAt = DateTime.UtcNow // UGS doesn't provide this
                });
            }

            // Notify about new requests
            foreach (var request in _pendingRequests)
            {
                OnFriendRequestReceived?.Invoke(request);
            }
        }

        #endregion

        #region Event Subscription

        private void SubscribeToEvents()
        {
            // Subscribe to relationship changes
            _ugsInstance.RelationshipAdded += OnRelationshipAdded;
            _ugsInstance.RelationshipDeleted += OnRelationshipDeleted;

            // Subscribe to presence changes
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
            var relationship = eventData.Relationship;
            GameLogger.Log($"Relationship added: {relationship.Member.Profile?.Name ?? "Unknown"}");

            // Update local cache
            if (relationship.Type == UGSModels.RelationshipType.Friend)
            {
                UpdateLocalFriends();

                // Notify about new friend
                var friend = _friends.FirstOrDefault(f => f.FriendCode == relationship.Member.Id);
                if (friend != null)
                {
                    OnFriendAdded?.Invoke(friend);
                }
            }
            else if (relationship.Type == UGSModels.RelationshipType.FriendRequest)
            {
                UpdateLocalRequests();
            }
            else if (relationship.Type == UGSModels.RelationshipType.FriendRequest)
            {
                UpdateLocalRequests();
            }
        }

        private void OnRelationshipDeleted(UGSNotifications.IRelationshipDeletedEvent eventData)
        {
            var relationship = eventData.Relationship;
            GameLogger.Log($"Relationship deleted: {relationship.Member.Profile?.Name ?? "Unknown"}");

            // Remove from local list
            var friendCode = relationship.Member.Id;
            _friends.RemoveAll(f => f.FriendCode == friendCode);
            _pendingRequests.RemoveAll(r => r.FromFriendCode == friendCode);

            OnFriendRemoved?.Invoke(friendCode);
            OnFriendsListUpdated?.Invoke();
        }

        private void OnPresenceUpdated(UGSNotifications.IPresenceUpdatedEvent eventData)
        {
            var presence = eventData.Presence;
            GameLogger.Log($"Presence updated for: {eventData.ID} - {presence.Availability}");

            // Update friend's online status
            var friend = _friends.FirstOrDefault(f => f.FriendCode == eventData.ID);
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
