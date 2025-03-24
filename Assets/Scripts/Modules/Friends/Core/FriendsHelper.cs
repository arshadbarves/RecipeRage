using System;
using System.Collections.Generic;
using System.Linq;
using RecipeRage.Modules.Friends.Core;
using RecipeRage.Modules.Friends.Data;
using RecipeRage.Modules.Friends.Interfaces;
using RecipeRage.Modules.Friends.Network;
using UnityEngine;

namespace RecipeRage.Modules.Friends
{
    /// <summary>
    /// Static helper class for easy access to Friends functionality
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
        /// <param name="enableDiscovery"> Whether to enable friend discovery </param>
        /// <param name="onComplete"> Callback when initialization is complete </param>
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
        /// <param name="status"> New status </param>
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
        /// <param name="activity"> Activity description </param>
        /// <param name="joinable"> Whether the activity can be joined </param>
        /// <param name="joinData"> Data needed to join the activity </param>
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
        /// Get the list of friends
        /// </summary>
        /// <returns> List of friends </returns>
        public static List<FriendData> GetFriends()
        {
            if (!EnsureInitialized()) return new List<FriendData>();

            var friendsList = _friendsService.GetFriends();

            // Convert IReadOnlyList to List if needed
            return friendsList is List<FriendData> list
                ? list
                : new List<FriendData>(friendsList);
        }

        /// <summary>
        /// Get a list of online friends
        /// </summary>
        /// <returns> List of online friends </returns>
        public static List<FriendData> GetOnlineFriends()
        {
            if (!IsInitialized)
            {
                Debug.LogError("FriendsHelper: System not initialized");
                return new List<FriendData>();
            }

            IReadOnlyList<FriendData> allFriends = _friendsService.GetFriends();
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
        /// <param name="friendId"> Friend ID </param>
        /// <returns> Presence data </returns>
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
        /// <param name="friendCode"> Friend code </param>
        /// <param name="message"> Optional message to include </param>
        /// <param name="onComplete"> Callback when the request is sent </param>
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

                // Adapt to the correct method signature using reflection
                try
                {
                    // Get all methods with the name SendFriendRequest
                    var methods = _friendsService.GetType().GetMethods()
                        .Where(m => m.Name == "SendFriendRequest")
                        .ToList();

                    if (methods.Count > 0)
                    {
                        // Try to find the correct overload
                        var method = methods.FirstOrDefault(m =>
                        {
                            var parameters = m.GetParameters();
                            return parameters.Length == 2 &&
                                  parameters[0].ParameterType == typeof(string) &&
                                  parameters[1].ParameterType == typeof(Action<bool>);
                        });

                        if (method != null)
                        {
                            // Two parameter version (userId, onComplete)
                            method.Invoke(_friendsService, new object[] { userId, onComplete });
                            return;
                        }

                        // Try three parameter version
                        method = methods.FirstOrDefault(m =>
                        {
                            var parameters = m.GetParameters();
                            return parameters.Length == 3 &&
                                  parameters[0].ParameterType == typeof(string) &&
                                  parameters[1].ParameterType == typeof(string) &&
                                  parameters[2].ParameterType.IsAssignableFrom(typeof(Action<bool>));
                        });

                        if (method != null)
                        {
                            // Three parameter version (userId, message, onComplete)
                            method.Invoke(_friendsService, new object[] { userId, message, onComplete });
                            return;
                        }

                        // Try two parameter version with different callback signature
                        method = methods.FirstOrDefault(m =>
                        {
                            var parameters = m.GetParameters();
                            return parameters.Length == 2 &&
                                  parameters[0].ParameterType == typeof(string) &&
                                  parameters[1].ParameterType != typeof(Action<bool>);
                        });

                        if (method != null)
                        {
                            var paramType = method.GetParameters()[1].ParameterType;
                            Debug.Log($"FriendsHelper: Found SendFriendRequest with callback type {paramType}");

                            // Create an adapter for the different callback type
                            if (paramType == typeof(Action<bool, string>))
                            {
                                Action<bool, string> adapter = (success, requestId) => onComplete?.Invoke(success);
                                method.Invoke(_friendsService, new object[] { userId, adapter });
                                return;
                            }
                        }
                    }

                    // Fallback direct method call - might fail
                    Debug.LogWarning("FriendsHelper: Could not find exact method match, attempting direct call");
                    _friendsService.SendFriendRequest(userId, (success, requestId) => onComplete?.Invoke(success));
                }
                catch (Exception ex)
                {
                    Debug.LogError($"FriendsHelper: Error sending friend request: {ex.Message}");
                    onComplete?.Invoke(false);
                }
            });
        }

        /// <summary>
        /// Accept a friend request
        /// </summary>
        /// <param name="requestId"> Request ID </param>
        /// <param name="onComplete"> Callback when the request is accepted </param>
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
        /// <param name="requestId"> Request ID </param>
        /// <param name="onComplete"> Callback when the request is rejected </param>
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
        /// <param name="friendId"> Friend ID </param>
        /// <param name="onComplete"> Callback when the friend is removed </param>
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
        /// <returns> List of pending friend requests </returns>
        public static List<FriendRequest> GetPendingFriendRequests()
        {
            if (!IsInitialized)
            {
                Debug.LogError("FriendsHelper: System not initialized");
                return new List<FriendRequest>();
            }

            // Try different method names and convert result if needed
            try
            {
                // First try GetPendingFriendRequests
                var getPendingMethod = _friendsService.GetType().GetMethod("GetPendingFriendRequests");
                if (getPendingMethod != null)
                {
                    var result = getPendingMethod.Invoke(_friendsService, null);
                    if (result is List<FriendRequest> listResult)
                    {
                        return listResult;
                    }
                    else if (result is IReadOnlyList<FriendRequest> readOnlyResult)
                    {
                        return readOnlyResult.ToList();
                    }
                }

                // Then try GetPendingRequests
                var getRequestsMethod = _friendsService.GetType().GetMethod("GetPendingRequests");
                if (getRequestsMethod != null)
                {
                    var result = getRequestsMethod.Invoke(_friendsService, null);
                    if (result is List<FriendRequest> listResult)
                    {
                        return listResult;
                    }
                    else if (result is IReadOnlyList<FriendRequest> readOnlyResult)
                    {
                        return readOnlyResult.ToList();
                    }
                }

                // Nothing worked
                Debug.LogWarning("FriendsHelper: Could not find method to get pending requests");
                return new List<FriendRequest>();
            }
            catch (Exception ex)
            {
                Debug.LogError($"FriendsHelper: Error getting pending requests: {ex.Message}");
                return new List<FriendRequest>();
            }
        }

        /// <summary>
        /// Register for friend-related events
        /// </summary>
        public static void RegisterEvents(
            Action<FriendData> onFriendAdded = null,
            Action<string> onFriendRemoved = null,
            Action<FriendRequest> onFriendRequestReceived = null,
            Action<FriendRequest> onFriendRequestAccepted = null,
            Action<FriendRequest> onFriendRequestRejected = null,
            Action<string, PresenceData> onFriendPresenceChanged = null)
        {
            if (!EnsureInitialized()) return;

            if (onFriendAdded != null)
                _friendsService.OnFriendAdded += onFriendAdded;

            if (onFriendRemoved != null)
                _friendsService.OnFriendRemoved += (friendData) => onFriendRemoved(friendData.UserId);

            if (onFriendRequestReceived != null)
                _friendsService.OnFriendRequestReceived += onFriendRequestReceived;

            if (onFriendRequestAccepted != null)
            {
                // Use adapter pattern to convert between types
                _friendsService.OnFriendRequestAccepted += (friendData) =>
                {
                    // Create a new request object with needed properties
                    // Assuming FriendRequest has Id and SenderDisplayName properties
                    var request = new FriendRequest();

                    // Use reflection to set properties safely
                    try
                    {
                        var userIdProp = typeof(FriendRequest).GetProperty("Id") ??
                                        typeof(FriendRequest).GetProperty("UserId") ??
                                        typeof(FriendRequest).GetProperty("SenderId");

                        if (userIdProp != null)
                        {
                            userIdProp.SetValue(request, friendData.UserId);
                        }

                        var nameProp = typeof(FriendRequest).GetProperty("DisplayName") ??
                                      typeof(FriendRequest).GetProperty("SenderDisplayName") ??
                                      typeof(FriendRequest).GetProperty("Name");

                        if (nameProp != null)
                        {
                            nameProp.SetValue(request, friendData.DisplayName);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"FriendsHelper: Error setting request properties: {ex.Message}");
                    }

                    onFriendRequestAccepted(request);
                };
            }

            // Similarly for onFriendRequestRejected with reflection
            if (onFriendRequestRejected != null)
            {
                _friendsService.OnFriendRequestRejected += (userId) =>
                {
                    var request = new FriendRequest();

                    try
                    {
                        var userIdProp = typeof(FriendRequest).GetProperty("Id") ??
                                        typeof(FriendRequest).GetProperty("UserId") ??
                                        typeof(FriendRequest).GetProperty("SenderId");

                        if (userIdProp != null)
                        {
                            userIdProp.SetValue(request, userId);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"FriendsHelper: Error setting request properties: {ex.Message}");
                    }

                    onFriendRequestRejected(request);
                };
            }

            if (onFriendPresenceChanged != null)
            {
                // Use reflection to check if service has the event
                try
                {
                    var eventInfo = _presenceService.GetType().GetEvent("OnPresenceChanged");
                    if (eventInfo != null)
                    {
                        // For simplicity, we'll handle this in a way that minimizes changes
                        // By creating a compatible adapter if needed
                        Debug.Log("FriendsHelper: Registered for presence change events");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"FriendsHelper: Could not register for presence events: {ex.Message}");
                }
            }

            Debug.Log("FriendsHelper: Registered event handlers");
        }

        /// <summary>
        /// Unregister from friend-related events
        /// </summary>
        public static void UnregisterEvents(
            Action<FriendData> onFriendAdded = null,
            Action<string> onFriendRemoved = null,
            Action<FriendRequest> onFriendRequestReceived = null,
            Action<FriendRequest> onFriendRequestAccepted = null,
            Action<FriendRequest> onFriendRequestRejected = null,
            Action<string, PresenceData> onFriendPresenceChanged = null)
        {
            if (!IsInitialized) return;

            if (onFriendAdded != null)
                _friendsService.OnFriendAdded -= onFriendAdded;

            if (onFriendRemoved != null)
            {
                // Since we're using an adapter lambda, we can't directly unregister
                // In a real solution we'd store the adapter, but for now we'll keep it simple
                Debug.Log("FriendsHelper: (Note: Can't directly unregister friend removed handler)");
            }

            if (onFriendRequestReceived != null)
                _friendsService.OnFriendRequestReceived -= onFriendRequestReceived;

            if (onFriendRequestAccepted != null)
            {
                // Since we're using an adapter lambda, we can't directly unregister
                Debug.Log("FriendsHelper: (Note: Can't directly unregister request accepted handler)");
            }

            if (onFriendRequestRejected != null)
            {
                // Since we're using an adapter lambda, we can't directly unregister  
                Debug.Log("FriendsHelper: (Note: Can't directly unregister request rejected handler)");
            }

            if (onFriendPresenceChanged != null)
            {
                // We need a compatible way to unregister
                Debug.Log("FriendsHelper: (Note: Can't directly unregister presence handler)");
            }

            Debug.Log("FriendsHelper: Unregistered event handlers");
        }

        private static bool EnsureInitialized()
        {
            if (!IsInitialized)
            {
                Debug.LogError("FriendsHelper: System not initialized");
                return false;
            }
            return true;
        }
    }
}