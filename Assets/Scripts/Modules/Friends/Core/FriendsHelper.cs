using System;
using System.Collections.Generic;
using UnityEngine;
using RecipeRage.Modules.Friends.Data;
using RecipeRage.Modules.Friends.Interfaces;
using RecipeRage.Modules.Friends.Core;

namespace RecipeRage.Modules.Friends
{
    /// <summary>
    /// Static helper class for easy access to Friends functionality
    /// 
    /// Complexity Rating: 2
    /// </summary>
    public static class FriendsHelper
    {
        private static IFriendsService _friendsService;
        private static IPresenceService _presenceService;
        private static IIdentityService _identityService;
        private static IP2PNetworkService _p2pNetworkService;
        
        /// <summary>
        /// Check if the friends system is initialized
        /// </summary>
        public static bool IsInitialized { get; private set; }
        
        /// <summary>
        /// Get the current user's friend code
        /// </summary>
        public static string MyFriendCode => _identityService?.GetMyFriendCode();
        
        /// <summary>
        /// Get the current user's display name
        /// </summary>
        public static string MyDisplayName => _identityService?.GetCurrentDisplayName();
        
        /// <summary>
        /// Get the current user's status
        /// </summary>
        public static UserStatus MyStatus => _presenceService?.GetMyPresence()?.Status ?? UserStatus.Offline;
        
        /// <summary>
        /// Initialize the friends system
        /// </summary>
        /// <param name="enableDiscovery">Whether to enable friend discovery</param>
        /// <param name="onComplete">Callback when initialization is complete</param>
        public static void Initialize(bool enableDiscovery = true, Action<bool> onComplete = null)
        {
            if (IsInitialized)
            {
                Debug.LogWarning("FriendsHelper: System already initialized");
                onComplete?.Invoke(true);
                return;
            }
            
            Debug.Log("FriendsHelper: Initializing friends system...");
            
            // Create service instances
            _identityService = new IdentityService();
            _presenceService = new PresenceService();
            _p2pNetworkService = new EOSP2PNetworkService();
            _friendsService = new FriendsService(_identityService, _presenceService, _p2pNetworkService);
            
            // Chain initialization
            _identityService.Initialize(identitySuccess =>
            {
                if (!identitySuccess)
                {
                    Debug.LogError("FriendsHelper: Identity service initialization failed");
                    onComplete?.Invoke(false);
                    return;
                }
                
                _presenceService.Initialize(presenceSuccess =>
                {
                    if (!presenceSuccess)
                    {
                        Debug.LogError("FriendsHelper: Presence service initialization failed");
                        onComplete?.Invoke(false);
                        return;
                    }
                    
                    _p2pNetworkService.Initialize(p2pSuccess =>
                    {
                        if (!p2pSuccess)
                        {
                            Debug.LogError("FriendsHelper: P2P network service initialization failed");
                            onComplete?.Invoke(false);
                            return;
                        }
                        
                        _friendsService.Initialize(friendsSuccess =>
                        {
                            if (friendsSuccess)
                            {
                                IsInitialized = true;
                                Debug.Log("FriendsHelper: Friends system initialized successfully");
                            }
                            else
                            {
                                Debug.LogError("FriendsHelper: Friends service initialization failed");
                            }
                            
                            onComplete?.Invoke(friendsSuccess);
                        });
                    });
                });
            });
        }
        
        /// <summary>
        /// Shutdown the friends system
        /// </summary>
        public static void Shutdown()
        {
            if (!IsInitialized)
            {
                return;
            }
            
            Debug.Log("FriendsHelper: Shutting down friends system...");
            
            _p2pNetworkService?.Shutdown();
            
            _friendsService = null;
            _presenceService = null;
            _identityService = null;
            _p2pNetworkService = null;
            
            IsInitialized = false;
        }
        
        /// <summary>
        /// Set the current user's status
        /// </summary>
        /// <param name="status">New status</param>
        public static void SetStatus(UserStatus status)
        {
            if (!IsInitialized)
            {
                Debug.LogError("FriendsHelper: System not initialized");
                return;
            }
            
            _presenceService.SetStatus(status);
        }
        
        /// <summary>
        /// Set the current user's activity
        /// </summary>
        /// <param name="activity">Activity description</param>
        /// <param name="joinable">Whether the activity can be joined</param>
        /// <param name="joinData">Data needed to join the activity</param>
        public static void SetActivity(string activity, bool joinable = false, string joinData = null)
        {
            if (!IsInitialized)
            {
                Debug.LogError("FriendsHelper: System not initialized");
                return;
            }
            
            _presenceService.SetActivity(activity, joinable, joinData);
        }
        
        /// <summary>
        /// Get a list of friends
        /// </summary>
        /// <returns>List of friends</returns>
        public static List<FriendData> GetFriends()
        {
            if (!IsInitialized)
            {
                Debug.LogError("FriendsHelper: System not initialized");
                return new List<FriendData>();
            }
            
            return _friendsService.GetFriends();
        }
        
        /// <summary>
        /// Get a list of online friends
        /// </summary>
        /// <returns>List of online friends</returns>
        public static List<FriendData> GetOnlineFriends()
        {
            if (!IsInitialized)
            {
                Debug.LogError("FriendsHelper: System not initialized");
                return new List<FriendData>();
            }
            
            var allFriends = _friendsService.GetFriends();
            var onlineFriends = new List<FriendData>();
            
            foreach (var friend in allFriends)
            {
                if (_presenceService.IsFriendOnline(friend.UserId))
                {
                    onlineFriends.Add(friend);
                }
            }
            
            return onlineFriends;
        }
        
        /// <summary>
        /// Get information about a friend's presence
        /// </summary>
        /// <param name="friendId">Friend ID</param>
        /// <returns>Presence data</returns>
        public static PresenceData GetFriendPresence(string friendId)
        {
            if (!IsInitialized)
            {
                Debug.LogError("FriendsHelper: System not initialized");
                return null;
            }
            
            return _presenceService.GetFriendPresence(friendId);
        }
        
        /// <summary>
        /// Send a friend request using a friend code
        /// </summary>
        /// <param name="friendCode">Friend code</param>
        /// <param name="message">Optional message to include</param>
        /// <param name="onComplete">Callback when the request is sent</param>
        public static void SendFriendRequest(string friendCode, string message = null, Action<bool> onComplete = null)
        {
            if (!IsInitialized)
            {
                Debug.LogError("FriendsHelper: System not initialized");
                onComplete?.Invoke(false);
                return;
            }
            
            if (!_identityService.IsValidFriendCode(friendCode))
            {
                Debug.LogError($"FriendsHelper: Invalid friend code format: {friendCode}");
                onComplete?.Invoke(false);
                return;
            }
            
            _identityService.LookupUserByFriendCode(friendCode, (userId, displayName) =>
            {
                if (string.IsNullOrEmpty(userId))
                {
                    Debug.LogError($"FriendsHelper: User not found for friend code: {friendCode}");
                    onComplete?.Invoke(false);
                    return;
                }
                
                _friendsService.SendFriendRequest(userId, message, onComplete);
            });
        }
        
        /// <summary>
        /// Accept a friend request
        /// </summary>
        /// <param name="requestId">Request ID</param>
        /// <param name="onComplete">Callback when the request is accepted</param>
        public static void AcceptFriendRequest(string requestId, Action<bool> onComplete = null)
        {
            if (!IsInitialized)
            {
                Debug.LogError("FriendsHelper: System not initialized");
                onComplete?.Invoke(false);
                return;
            }
            
            _friendsService.AcceptFriendRequest(requestId, onComplete);
        }
        
        /// <summary>
        /// Reject a friend request
        /// </summary>
        /// <param name="requestId">Request ID</param>
        /// <param name="onComplete">Callback when the request is rejected</param>
        public static void RejectFriendRequest(string requestId, Action<bool> onComplete = null)
        {
            if (!IsInitialized)
            {
                Debug.LogError("FriendsHelper: System not initialized");
                onComplete?.Invoke(false);
                return;
            }
            
            _friendsService.RejectFriendRequest(requestId, onComplete);
        }
        
        /// <summary>
        /// Remove a friend
        /// </summary>
        /// <param name="friendId">Friend ID</param>
        /// <param name="onComplete">Callback when the friend is removed</param>
        public static void RemoveFriend(string friendId, Action<bool> onComplete = null)
        {
            if (!IsInitialized)
            {
                Debug.LogError("FriendsHelper: System not initialized");
                onComplete?.Invoke(false);
                return;
            }
            
            _friendsService.RemoveFriend(friendId, onComplete);
        }
        
        /// <summary>
        /// Get pending friend requests
        /// </summary>
        /// <returns>List of pending friend requests</returns>
        public static List<FriendRequest> GetPendingFriendRequests()
        {
            if (!IsInitialized)
            {
                Debug.LogError("FriendsHelper: System not initialized");
                return new List<FriendRequest>();
            }
            
            return _friendsService.GetPendingRequests();
        }
        
        /// <summary>
        /// Register for friends events
        /// </summary>
        /// <param name="onFriendAdded">Called when a friend is added</param>
        /// <param name="onFriendRemoved">Called when a friend is removed</param>
        /// <param name="onFriendRequestReceived">Called when a friend request is received</param>
        /// <param name="onFriendRequestAccepted">Called when a friend request is accepted</param>
        /// <param name="onFriendRequestRejected">Called when a friend request is rejected</param>
        /// <param name="onFriendPresenceChanged">Called when a friend's presence changes</param>
        public static void RegisterEvents(
            Action<FriendData> onFriendAdded = null,
            Action<string> onFriendRemoved = null,
            Action<FriendRequest> onFriendRequestReceived = null,
            Action<FriendRequest> onFriendRequestAccepted = null,
            Action<FriendRequest> onFriendRequestRejected = null,
            Action<string, PresenceData> onFriendPresenceChanged = null)
        {
            if (!IsInitialized)
            {
                Debug.LogError("FriendsHelper: System not initialized");
                return;
            }
            
            if (onFriendAdded != null)
            {
                _friendsService.OnFriendAdded += onFriendAdded;
            }
            
            if (onFriendRemoved != null)
            {
                _friendsService.OnFriendRemoved += onFriendRemoved;
            }
            
            if (onFriendRequestReceived != null)
            {
                _friendsService.OnFriendRequestReceived += onFriendRequestReceived;
            }
            
            if (onFriendRequestAccepted != null)
            {
                _friendsService.OnFriendRequestAccepted += onFriendRequestAccepted;
            }
            
            if (onFriendRequestRejected != null)
            {
                _friendsService.OnFriendRequestRejected += onFriendRequestRejected;
            }
            
            if (onFriendPresenceChanged != null)
            {
                _presenceService.OnFriendPresenceChanged += onFriendPresenceChanged;
            }
        }
        
        /// <summary>
        /// Unregister from friends events
        /// </summary>
        /// <param name="onFriendAdded">Friend added callback to remove</param>
        /// <param name="onFriendRemoved">Friend removed callback to remove</param>
        /// <param name="onFriendRequestReceived">Friend request received callback to remove</param>
        /// <param name="onFriendRequestAccepted">Friend request accepted callback to remove</param>
        /// <param name="onFriendRequestRejected">Friend request rejected callback to remove</param>
        /// <param name="onFriendPresenceChanged">Friend presence changed callback to remove</param>
        public static void UnregisterEvents(
            Action<FriendData> onFriendAdded = null,
            Action<string> onFriendRemoved = null,
            Action<FriendRequest> onFriendRequestReceived = null,
            Action<FriendRequest> onFriendRequestAccepted = null,
            Action<FriendRequest> onFriendRequestRejected = null,
            Action<string, PresenceData> onFriendPresenceChanged = null)
        {
            if (!IsInitialized)
            {
                return;
            }
            
            if (onFriendAdded != null)
            {
                _friendsService.OnFriendAdded -= onFriendAdded;
            }
            
            if (onFriendRemoved != null)
            {
                _friendsService.OnFriendRemoved -= onFriendRemoved;
            }
            
            if (onFriendRequestReceived != null)
            {
                _friendsService.OnFriendRequestReceived -= onFriendRequestReceived;
            }
            
            if (onFriendRequestAccepted != null)
            {
                _friendsService.OnFriendRequestAccepted -= onFriendRequestAccepted;
            }
            
            if (onFriendRequestRejected != null)
            {
                _friendsService.OnFriendRequestRejected -= onFriendRequestRejected;
            }
            
            if (onFriendPresenceChanged != null)
            {
                _presenceService.OnFriendPresenceChanged -= onFriendPresenceChanged;
            }
        }
    }
} 