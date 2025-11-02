using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Logging;
using Core.Networking.Interfaces;
using Cysharp.Threading.Tasks;
using Epic.OnlineServices;
using PlayEveryWare.EpicOnlineServices;
using UnityEngine;

namespace Core.Networking.Services
{
    /// <summary>
    /// Production-ready Friends Service with Supabase backend
    /// Features:
    /// - Friend requests (send/accept/reject)
    /// - Real friends list
    /// - Recent players tracking
    /// - Online status
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
        public string MyFriendCode { get; private set; }
        public IReadOnlyList<FriendInfo> Friends => _friends.AsReadOnly();
        public IReadOnlyList<FriendInfo> RecentPlayers => _recentPlayers.AsReadOnly();
        public IReadOnlyList<FriendRequest> PendingRequests => _pendingRequests.AsReadOnly();

        #endregion

        #region Private Fields

        private readonly ILobbyManager _lobbyManager;
        private readonly SupabaseClient _supabase;
        private readonly List<FriendInfo> _friends = new List<FriendInfo>();
        private readonly List<FriendInfo> _recentPlayers = new List<FriendInfo>();
        private readonly List<FriendRequest> _pendingRequests = new List<FriendRequest>();

        private string _myProductUserId;
        private string _myDisplayName;

        #endregion

        #region Initialization

        public FriendsService(ILobbyManager lobbyManager, SupabaseConfig config)
        {
            _lobbyManager = lobbyManager ?? throw new ArgumentNullException(nameof(lobbyManager));
            _supabase = new SupabaseClient(config);
        }

        public async void Initialize()
        {
            if (IsInitialized)
            {
                GameLogger.LogWarning("Already initialized");
                return;
            }

            GameLogger.Log("Initializing with Supabase...");

            try
            {
                // Get EOS user info
                var productUserId = EOSManager.Instance.GetProductUserId();
                if (productUserId == null || !productUserId.IsValid())
                {
                    GameLogger.LogError("Invalid ProductUserId");
                    return;
                }

                _myProductUserId = productUserId.ToString();
                _myDisplayName = GetDisplayName();

                // Register user and get friend code
                await RegisterUserAsync();

                // Load friends and requests
                await RefreshFriendsAsync();
                await RefreshFriendRequestsAsync();

                IsInitialized = true;
                GameLogger.Log($"Initialized - Friend Code: {MyFriendCode}");
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

            // Validate friend code
            if (string.IsNullOrEmpty(friendCode) || friendCode.Length != 8)
                throw new ArgumentException("Invalid friend code format");

            if (friendCode == MyFriendCode)
                throw new ArgumentException("Cannot send request to yourself");

            GameLogger.Log($"Sending friend request to: {friendCode}");

            // Find user by friend code
            var query = $"friend_code=eq.{friendCode}&select=product_user_id,display_name,friend_code";
            var response = await _supabase.GetAsync("users", query);

            var users = JsonHelper.FromJson<UserData>(response);
            if (users == null || users.Length == 0)
                throw new Exception("Friend code not found");

            var targetUser = users[0];

            // Check if already friends
            if (_friends.Any(f => f.FriendCode == friendCode))
                throw new Exception("Already friends with this user");

            // Send friend request
            var requestData = new
            {
                from_user_id = _myProductUserId,
                to_user_id = targetUser.product_user_id,
                status = "pending"
            };

            var requestJson = JsonUtility.ToJson(requestData);
            var requestResponse = await _supabase.PostAsync("friend_requests", requestJson);

            var createdRequest = JsonUtility.FromJson<FriendRequestData>(requestResponse);

            GameLogger.Log($"Friend request sent to {targetUser.display_name}");

            return new FriendRequest
            {
                Id = createdRequest.id,
                FromUserId = _myProductUserId,
                FromUserName = _myDisplayName,
                FromFriendCode = MyFriendCode,
                ToUserId = targetUser.product_user_id,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };
        }

        public async Task AcceptFriendRequestAsync(string requestId)
        {
            if (!IsInitialized)
                throw new InvalidOperationException("Service not initialized");

            GameLogger.Log($"Accepting friend request: {requestId}");

            // Call RPC function to accept request
            var rpcData = new { request_id = requestId };
            var rpcJson = JsonUtility.ToJson(rpcData);

            await _supabase.RpcAsync("accept_friend_request", rpcJson);

            // Remove from pending
            _pendingRequests.RemoveAll(r => r.Id == requestId);

            // Refresh friends list
            await RefreshFriendsAsync();

            GameLogger.Log("Friend request accepted");
        }

        public async Task RejectFriendRequestAsync(string requestId)
        {
            if (!IsInitialized)
                throw new InvalidOperationException("Service not initialized");

            GameLogger.Log($"Rejecting friend request: {requestId}");

            // Call RPC function to reject request
            var rpcData = new { request_id = requestId };
            var rpcJson = JsonUtility.ToJson(rpcData);

            await _supabase.RpcAsync("reject_friend_request", rpcJson);

            // Remove from pending
            _pendingRequests.RemoveAll(r => r.Id == requestId);

            GameLogger.Log("Friend request rejected");
        }

        public async Task RefreshFriendRequestsAsync()
        {
            if (!IsInitialized)
                return;

            GameLogger.Log("Refreshing friend requests...");

            // Get pending requests where I'm the recipient
            var query = $"to_user_id=eq.{_myProductUserId}&status=eq.pending&select=*,from_user:users!from_user_id(product_user_id,display_name,friend_code)";
            var response = await _supabase.GetAsync("friend_requests", query);

            var requests = JsonHelper.FromJson<FriendRequestWithUserData>(response);

            _pendingRequests.Clear();

            if (requests != null)
            {
                foreach (var req in requests)
                {
                    _pendingRequests.Add(new FriendRequest
                    {
                        Id = req.id,
                        FromUserId = req.from_user_id,
                        FromUserName = req.from_user?.display_name ?? "Unknown",
                        FromFriendCode = req.from_user?.friend_code ?? "",
                        ToUserId = req.to_user_id,
                        Status = req.status,
                        CreatedAt = DateTime.Parse(req.created_at)
                    });
                }
            }

            GameLogger.Log($"Found {_pendingRequests.Count} pending requests");

            // Notify if new requests
            foreach (var request in _pendingRequests)
            {
                OnFriendRequestReceived?.Invoke(request);
            }
        }

        #endregion

        #region Public Methods - Friends

        public async Task RefreshFriendsAsync()
        {
            if (!IsInitialized)
                return;

            GameLogger.Log("Refreshing friends list...");

            // Get friends where I'm either user_id or friend_id
            var query = $"or=(user_id.eq.{_myProductUserId},friend_id.eq.{_myProductUserId})&select=*,user:users!user_id(*),friend:users!friend_id(*)";
            var response = await _supabase.GetAsync("friends", query);

            var friendships = JsonHelper.FromJson<FriendshipData>(response);

            _friends.Clear();

            if (friendships != null)
            {
                foreach (var friendship in friendships)
                {
                    // Determine which user is the friend (not me)
                    var friendData = friendship.user_id == _myProductUserId
                        ? friendship.friend
                        : friendship.user;

                    if (friendData != null)
                    {
                        var isOnline = IsUserOnline(friendData.last_seen);

                        _friends.Add(new FriendInfo
                        {
                            FriendCode = friendData.friend_code,
                            ProductUserId = ProductUserId.FromString(friendData.product_user_id),
                            DisplayName = friendData.display_name,
                            AddedAt = DateTime.Parse(friendship.added_at),
                            LastSeen = DateTime.Parse(friendData.last_seen),
                            IsOnline = isOnline,
                            IsRecent = false
                        });
                    }
                }
            }

            // Sort: online first, then by name
            _friends.Sort((a, b) =>
            {
                if (a.IsOnline != b.IsOnline)
                    return b.IsOnline.CompareTo(a.IsOnline);
                return string.Compare(a.DisplayName, b.DisplayName, StringComparison.Ordinal);
            });

            GameLogger.Log($"Loaded {_friends.Count} friends");
            OnFriendsListUpdated?.Invoke();
        }

        public async Task RemoveFriendAsync(string friendCode)
        {
            if (!IsInitialized)
                throw new InvalidOperationException("Service not initialized");

            var friend = _friends.FirstOrDefault(f => f.FriendCode == friendCode);
            if (friend == null)
                throw new Exception("Friend not found");

            GameLogger.Log($"Removing friend: {friend.DisplayName}");

            // Delete friendship (both directions handled by database)
            var query = $"or=(and(user_id.eq.{_myProductUserId},friend_id.eq.{friend.ProductUserId}),and(user_id.eq.{friend.ProductUserId},friend_id.eq.{_myProductUserId}))";
            await _supabase.DeleteAsync("friends", query);

            _friends.Remove(friend);
            OnFriendRemoved?.Invoke(friendCode);
            OnFriendsListUpdated?.Invoke();

            GameLogger.Log("Friend removed");
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
                if (playerIdStr == _myProductUserId)
                    return;

                GameLogger.Log($"Adding recent player: {displayName}");

                // Upsert to recent_players table
                var recentData = new
                {
                    user_id = _myProductUserId,
                    recent_player_id = playerIdStr,
                    last_played_at = DateTime.UtcNow.ToString("o")
                };

                var json = JsonUtility.ToJson(recentData);
                await _supabase.PostAsync("recent_players", json);

                // Update local list
                _recentPlayers.RemoveAll(p => p.ProductUserId == productUserId);

                // Get friend code for this player
                var query = $"product_user_id=eq.{playerIdStr}&select=friend_code";
                var response = await _supabase.GetAsync("users", query);
                var users = JsonHelper.FromJson<UserData>(response);
                var friendCode = users?.Length > 0 ? users[0].friend_code : "UNKNOWN";

                _recentPlayers.Insert(0, new FriendInfo
                {
                    FriendCode = friendCode,
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

        #region Private Methods

        private async Task RegisterUserAsync()
        {
            GameLogger.Log("Registering user...");

            // Check if user exists
            var query = $"product_user_id=eq.{_myProductUserId}&select=friend_code";
            var response = await _supabase.GetAsync("users", query);

            var users = JsonHelper.FromJson<UserData>(response);

            if (users != null && users.Length > 0)
            {
                // User exists, use existing friend code
                MyFriendCode = users[0].friend_code;
                GameLogger.Log($"User exists with code: {MyFriendCode}");

                // Update last_seen
                await UpdateLastSeenAsync();
            }
            else
            {
                // New user, generate friend code
                MyFriendCode = await GenerateUniqueFriendCodeAsync();

                var userData = new
                {
                    product_user_id = _myProductUserId,
                    display_name = _myDisplayName,
                    friend_code = MyFriendCode,
                    device_id = SystemInfo.deviceUniqueIdentifier
                };

                var json = JsonUtility.ToJson(userData);
                await _supabase.PostAsync("users", json);

                GameLogger.Log($"New user registered with code: {MyFriendCode}");
            }
        }

        private async Task<string> GenerateUniqueFriendCodeAsync()
        {
            const int maxAttempts = 10;

            for (int i = 0; i < maxAttempts; i++)
            {
                var code = GenerateFriendCode();

                // Check if code exists
                var query = $"friend_code=eq.{code}&select=friend_code";
                var response = await _supabase.GetAsync("users", query);
                var users = JsonHelper.FromJson<UserData>(response);

                if (users == null || users.Length == 0)
                {
                    return code; // Unique code found
                }
            }

            throw new Exception("Failed to generate unique friend code");
        }

        private string GenerateFriendCode()
        {
            const string letters = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string numbers = "0123456789";

            var random = new System.Random();
            var code = new char[8];

            for (int i = 0; i < 3; i++)
                code[i] = letters[random.Next(letters.Length)];

            for (int i = 3; i < 8; i++)
                code[i] = numbers[random.Next(numbers.Length)];

            return new string(code);
        }

        private async Task UpdateLastSeenAsync()
        {
            try
            {
                var updateData = new { last_seen = DateTime.UtcNow.ToString("o") };
                var json = JsonUtility.ToJson(updateData);
                var query = $"product_user_id=eq.{_myProductUserId}";

                await _supabase.PatchAsync("users", query, json);
            }
            catch (Exception ex)
            {
                GameLogger.LogWarning($"Failed to update last_seen: {ex.Message}");
            }
        }

        private bool IsUserOnline(string lastSeenStr)
        {
            if (string.IsNullOrEmpty(lastSeenStr))
                return false;

            try
            {
                var lastSeen = DateTime.Parse(lastSeenStr);
                var timeSince = DateTime.UtcNow - lastSeen;
                return timeSince.TotalMinutes < 5; // Online if seen in last 5 minutes
            }
            catch
            {
                return false;
            }
        }

        private string GetDisplayName()
        {
            // Try to get display name from EOS or use default
            return $"Player_{UnityEngine.Random.Range(1000, 9999)}";
        }

        #endregion

        #region Serialization Classes

        [Serializable]
        private class UserData
        {
            public string product_user_id;
            public string display_name;
            public string friend_code;
            public string last_seen;
        }

        [Serializable]
        private class FriendRequestData
        {
            public string id;
            public string from_user_id;
            public string to_user_id;
            public string status;
            public string created_at;
        }

        [Serializable]
        private class FriendRequestWithUserData
        {
            public string id;
            public string from_user_id;
            public string to_user_id;
            public string status;
            public string created_at;
            public UserData from_user;
        }

        [Serializable]
        private class FriendshipData
        {
            public string user_id;
            public string friend_id;
            public string added_at;
            public UserData user;
            public UserData friend;
        }

        #endregion
    }

    /// <summary>
    /// Helper for JSON array deserialization
    /// </summary>
    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            if (string.IsNullOrEmpty(json) || json == "[]")
                return new T[0];

            var wrapper = JsonUtility.FromJson<Wrapper<T>>("{\"items\":" + json + "}");
            return wrapper.items;
        }

        [Serializable]
        private class Wrapper<T>
        {
            public T[] items;
        }
    }
}
