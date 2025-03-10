using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RecipeRage.Modules.Friends.Data;
using RecipeRage.Modules.Friends.Interfaces;
using RecipeRage.Modules.Friends.Network;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

namespace RecipeRage.Modules.Friends.Core
{
    /// <summary>
    /// Implementation of the friends service
    /// 
    /// Complexity Rating: 4
    /// </summary>
    public class FriendsService : IFriendsService
    {
        private const string SAVE_PATH = "FriendsData";
        private const float CHECK_INTERVAL = 10.0f;
        
        private readonly IIdentityService _identityService;
        private readonly IPresenceService _presenceService;
        private readonly IP2PNetworkService _p2pNetworkService;
        
        private Dictionary<string, FriendData> _friends;
        private Dictionary<string, FriendRequest> _pendingRequests;
        private bool _isInitialized;
        private float _lastCheckTime;
        
        /// <summary>
        /// Event triggered when a friend is added
        /// </summary>
        public event Action<FriendData> OnFriendAdded;
        
        /// <summary>
        /// Event triggered when a friend is removed
        /// </summary>
        public event Action<string> OnFriendRemoved;
        
        /// <summary>
        /// Event triggered when a friend request is received
        /// </summary>
        public event Action<FriendRequest> OnFriendRequestReceived;
        
        /// <summary>
        /// Event triggered when a friend request is accepted
        /// </summary>
        public event Action<FriendRequest> OnFriendRequestAccepted;
        
        /// <summary>
        /// Event triggered when a friend request is rejected
        /// </summary>
        public event Action<FriendRequest> OnFriendRequestRejected;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="identityService">Identity service</param>
        /// <param name="presenceService">Presence service</param>
        /// <param name="p2pNetworkService">P2P network service</param>
        public FriendsService(IIdentityService identityService, IPresenceService presenceService, IP2PNetworkService p2pNetworkService)
        {
            _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            _presenceService = presenceService ?? throw new ArgumentNullException(nameof(presenceService));
            _p2pNetworkService = p2pNetworkService ?? throw new ArgumentNullException(nameof(p2pNetworkService));
            
            _friends = new Dictionary<string, FriendData>();
            _pendingRequests = new Dictionary<string, FriendRequest>();
            
            // Subscribe to P2P network events
            _p2pNetworkService.OnMessageReceived += HandleMessageReceived;
            _p2pNetworkService.OnConnectionStatusChanged += HandleConnectionStatusChanged;
        }
        
        /// <summary>
        /// Initialize the friends service
        /// </summary>
        /// <param name="onComplete">Callback when initialization is complete</param>
        public void Initialize(Action<bool> onComplete = null)
        {
            if (_isInitialized)
            {
                Debug.LogWarning("FriendsService: Already initialized");
                onComplete?.Invoke(true);
                return;
            }
            
            Debug.Log("FriendsService: Initializing...");
            
            // Load saved data
            LoadFriends();
            LoadPendingRequests();
            
            // Register for MonoBehaviour events
            FriendsServiceUpdater.Instance.OnUpdate += Update;
            
            _isInitialized = true;
            Debug.Log("FriendsService: Initialized successfully");
            onComplete?.Invoke(true);
        }
        
        /// <summary>
        /// Handle network message received
        /// </summary>
        /// <param name="senderId">Message sender ID</param>
        /// <param name="messageData">Message data</param>
        private void HandleMessageReceived(string senderId, byte[] messageData)
        {
            if (!_isInitialized || messageData == null || messageData.Length == 0)
            {
                return;
            }
            
            // Parse the message
            if (!FriendsNetworkProtocol.ParsePacket(messageData, out FriendsMessageType messageType, out byte[] payload))
            {
                Debug.LogError($"FriendsService: Failed to parse message from {senderId}");
                return;
            }
            
            // Handle different message types
            switch (messageType)
            {
                case FriendsMessageType.FriendRequest:
                    HandleFriendRequestMessage(senderId, payload);
                    break;
                    
                case FriendsMessageType.FriendAccept:
                    HandleFriendAcceptMessage(senderId, payload);
                    break;
                    
                case FriendsMessageType.FriendReject:
                    HandleFriendRejectMessage(senderId, payload);
                    break;
                    
                case FriendsMessageType.FriendRemove:
                    HandleFriendRemoveMessage(senderId, payload);
                    break;
                    
                case FriendsMessageType.PresenceUpdate:
                    HandlePresenceUpdateMessage(senderId, payload);
                    break;
            }
        }
        
        /// <summary>
        /// Handle connection status change
        /// </summary>
        /// <param name="peerId">Peer ID</param>
        /// <param name="isConnected">Whether the peer is now connected</param>
        private void HandleConnectionStatusChanged(string peerId, bool isConnected)
        {
            if (!_isInitialized)
            {
                return;
            }
            
            if (isConnected)
            {
                Debug.Log($"FriendsService: Connected to peer {peerId}");
                
                // If connected to a friend, send presence update
                if (IsFriend(peerId))
                {
                    SendPresenceUpdate(peerId);
                }
            }
            else
            {
                Debug.Log($"FriendsService: Disconnected from peer {peerId}");
            }
        }
        
        /// <summary>
        /// Update method called from MonoBehaviour
        /// </summary>
        private void Update()
        {
            if (!_isInitialized)
            {
                return;
            }
            
            // Process incoming messages
            if (_p2pNetworkService is EOSP2PNetworkService p2pService)
            {
                p2pService.ProcessIncomingMessages();
            }
            
            // Periodic check
            float currentTime = Time.time;
            if (currentTime - _lastCheckTime > CHECK_INTERVAL)
            {
                _lastCheckTime = currentTime;
                
                // Connect to any friends who are not connected
                foreach (var friend in _friends.Values)
                {
                    if (!_p2pNetworkService.IsConnectedTo(friend.UserId))
                    {
                        _p2pNetworkService.Connect(friend.UserId);
                    }
                }
                
                // Save data periodically
                SaveFriends();
                SavePendingRequests();
            }
        }
        
        /// <summary>
        /// Handle friend request message
        /// </summary>
        /// <param name="senderId">Sender ID</param>
        /// <param name="payload">Message payload</param>
        private void HandleFriendRequestMessage(string senderId, byte[] payload)
        {
            string jsonData = FriendsNetworkProtocol.GetStringPayload(payload);
            if (string.IsNullOrEmpty(jsonData))
            {
                Debug.LogError($"FriendsService: Invalid friend request data from {senderId}");
                return;
            }
            
            try
            {
                // Deserialize the request
                FriendRequest request = JsonConvert.DeserializeObject<FriendRequest>(jsonData);
                
                // Validate request
                if (request == null || request.SenderId != senderId || request.ReceiverId != _identityService.GetCurrentUserId())
                {
                    Debug.LogError($"FriendsService: Invalid friend request from {senderId}");
                    return;
                }
                
                // Check if already friends
                if (IsFriend(senderId))
                {
                    Debug.LogWarning($"FriendsService: Already friends with {senderId}");
                    return;
                }
                
                // Check for duplicate request
                if (_pendingRequests.ContainsKey(request.RequestId))
                {
                    Debug.LogWarning($"FriendsService: Duplicate friend request from {senderId}");
                    return;
                }
                
                // Mark as received request
                request.Type = FriendRequestType.Received;
                
                // Add to pending requests
                _pendingRequests.Add(request.RequestId, request);
                
                // Save requests
                SavePendingRequests();
                
                // Notify listeners
                OnFriendRequestReceived?.Invoke(request);
                
                Debug.Log($"FriendsService: Friend request received from {request.SenderName} ({senderId})");
            }
            catch (Exception ex)
            {
                Debug.LogError($"FriendsService: Error processing friend request: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Handle friend accept message
        /// </summary>
        /// <param name="senderId">Sender ID</param>
        /// <param name="payload">Message payload</param>
        private void HandleFriendAcceptMessage(string senderId, byte[] payload)
        {
            string requestId = FriendsNetworkProtocol.GetStringPayload(payload);
            if (string.IsNullOrEmpty(requestId))
            {
                Debug.LogError($"FriendsService: Invalid friend accept data from {senderId}");
                return;
            }
            
            // Find the request
            if (!_pendingRequests.TryGetValue(requestId, out FriendRequest request))
            {
                Debug.LogError($"FriendsService: Friend accept for unknown request {requestId} from {senderId}");
                return;
            }
            
            // Validate accept
            if (request.Type != FriendRequestType.Sent || request.ReceiverId != senderId)
            {
                Debug.LogError($"FriendsService: Invalid friend accept from {senderId}");
                return;
            }
            
            // Add as friend
            FriendData friendData = new FriendData
            {
                UserId = senderId,
                DisplayName = request.SenderName,
                FriendCode = "",
                FriendSince = DateTime.UtcNow,
                Notes = "",
                IsFavorite = false
            };
            
            _friends[senderId] = friendData;
            
            // Remove from pending requests
            _pendingRequests.Remove(requestId);
            
            // Save data
            SaveFriends();
            SavePendingRequests();
            
            // Connect to the new friend
            _p2pNetworkService.Connect(senderId);
            
            // Notify listeners
            OnFriendRequestAccepted?.Invoke(request);
            OnFriendAdded?.Invoke(friendData);
            
            Debug.Log($"FriendsService: Friend request accepted by {request.SenderName} ({senderId})");
        }
        
        /// <summary>
        /// Handle friend reject message
        /// </summary>
        /// <param name="senderId">Sender ID</param>
        /// <param name="payload">Message payload</param>
        private void HandleFriendRejectMessage(string senderId, byte[] payload)
        {
            string requestId = FriendsNetworkProtocol.GetStringPayload(payload);
            if (string.IsNullOrEmpty(requestId))
            {
                Debug.LogError($"FriendsService: Invalid friend reject data from {senderId}");
                return;
            }
            
            // Find the request
            if (!_pendingRequests.TryGetValue(requestId, out FriendRequest request))
            {
                Debug.LogError($"FriendsService: Friend reject for unknown request {requestId} from {senderId}");
                return;
            }
            
            // Validate reject
            if (request.Type != FriendRequestType.Sent || request.ReceiverId != senderId)
            {
                Debug.LogError($"FriendsService: Invalid friend reject from {senderId}");
                return;
            }
            
            // Remove from pending requests
            _pendingRequests.Remove(requestId);
            
            // Save requests
            SavePendingRequests();
            
            // Notify listeners
            OnFriendRequestRejected?.Invoke(request);
            
            Debug.Log($"FriendsService: Friend request rejected by {request.SenderName} ({senderId})");
        }
        
        /// <summary>
        /// Handle friend remove message
        /// </summary>
        /// <param name="senderId">Sender ID</param>
        /// <param name="payload">Message payload</param>
        private void HandleFriendRemoveMessage(string senderId, byte[] payload)
        {
            // Check if they are actually a friend
            if (!IsFriend(senderId))
            {
                Debug.LogWarning($"FriendsService: Remove friend message from non-friend {senderId}");
                return;
            }
            
            // Remove from friends
            string friendName = _friends[senderId].DisplayName;
            _friends.Remove(senderId);
            
            // Save friends
            SaveFriends();
            
            // Disconnect from this peer
            _p2pNetworkService.Disconnect(senderId);
            
            // Notify listeners
            OnFriendRemoved?.Invoke(senderId);
            
            Debug.Log($"FriendsService: Friend {friendName} ({senderId}) removed by remote user");
        }
        
        /// <summary>
        /// Handle presence update message
        /// </summary>
        /// <param name="senderId">Sender ID</param>
        /// <param name="payload">Message payload</param>
        private void HandlePresenceUpdateMessage(string senderId, byte[] payload)
        {
            if (!IsFriend(senderId))
            {
                Debug.LogWarning($"FriendsService: Presence update from non-friend {senderId}");
                return;
            }
            
            string jsonData = FriendsNetworkProtocol.GetStringPayload(payload);
            if (string.IsNullOrEmpty(jsonData))
            {
                Debug.LogError($"FriendsService: Invalid presence update data from {senderId}");
                return;
            }
            
            try
            {
                // Forward to presence service
                PresenceData presenceData = JsonConvert.DeserializeObject<PresenceData>(jsonData);
                if (presenceData != null && presenceData.UserId == senderId)
                {
                    _presenceService.UpdateFriendPresence(senderId, presenceData);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"FriendsService: Error processing presence update: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Send a presence update to a friend
        /// </summary>
        /// <param name="friendId">Friend ID</param>
        private void SendPresenceUpdate(string friendId)
        {
            if (!_isInitialized || !IsFriend(friendId))
            {
                return;
            }
            
            PresenceData myPresence = _presenceService.GetMyPresence();
            if (myPresence == null)
            {
                return;
            }
            
            try
            {
                string jsonData = JsonConvert.SerializeObject(myPresence);
                byte[] presencePacket = FriendsNetworkProtocol.CreatePresenceUpdate(jsonData);
                
                _p2pNetworkService.SendMessage(friendId, presencePacket);
            }
            catch (Exception ex)
            {
                Debug.LogError($"FriendsService: Error sending presence update: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Get a list of all friends
        /// </summary>
        /// <returns>List of friends</returns>
        public List<FriendData> GetFriends()
        {
            return _friends.Values.ToList();
        }
        
        /// <summary>
        /// Check if a user is a friend
        /// </summary>
        /// <param name="userId">User ID to check</param>
        /// <returns>True if the user is a friend</returns>
        public bool IsFriend(string userId)
        {
            return _friends.ContainsKey(userId);
        }
        
        /// <summary>
        /// Get the friendship state with a user
        /// </summary>
        /// <param name="userId">User ID to check</param>
        /// <returns>Friendship state</returns>
        public FriendshipState GetFriendshipState(string userId)
        {
            if (_friends.ContainsKey(userId))
            {
                return FriendshipState.Friends;
            }
            
            foreach (var request in _pendingRequests.Values)
            {
                if (request.Type == FriendRequestType.Sent && request.ReceiverId == userId)
                {
                    return FriendshipState.PendingOutgoing;
                }
                
                if (request.Type == FriendRequestType.Received && request.SenderId == userId)
                {
                    return FriendshipState.PendingIncoming;
                }
            }
            
            return FriendshipState.NotFriends;
        }
        
        /// <summary>
        /// Send a friend request
        /// </summary>
        /// <param name="userId">User ID to send request to</param>
        /// <param name="message">Optional message</param>
        /// <param name="onComplete">Callback when request is sent</param>
        public void SendFriendRequest(string userId, string message = null, Action<bool> onComplete = null)
        {
            if (!_isInitialized)
            {
                Debug.LogError("FriendsService: Not initialized");
                onComplete?.Invoke(false);
                return;
            }
            
            if (string.IsNullOrEmpty(userId))
            {
                Debug.LogError("FriendsService: Invalid user ID");
                onComplete?.Invoke(false);
                return;
            }
            
            if (userId == _identityService.GetCurrentUserId())
            {
                Debug.LogError("FriendsService: Cannot send friend request to yourself");
                onComplete?.Invoke(false);
                return;
            }
            
            // Check current friendship state
            FriendshipState state = GetFriendshipState(userId);
            if (state != FriendshipState.NotFriends)
            {
                Debug.LogError($"FriendsService: Cannot send friend request - current state is {state}");
                onComplete?.Invoke(false);
                return;
            }
            
            // Create the request
            FriendRequest request = new FriendRequest
            {
                RequestId = Guid.NewGuid().ToString(),
                SenderId = _identityService.GetCurrentUserId(),
                SenderName = _identityService.GetCurrentDisplayName(),
                ReceiverId = userId,
                SentTime = DateTime.UtcNow,
                Message = message ?? "",
                Type = FriendRequestType.Sent
            };
            
            // Add to pending requests
            _pendingRequests.Add(request.RequestId, request);
            
            // Save requests
            SavePendingRequests();
            
            // Try to establish connection
            _p2pNetworkService.Connect(userId, connected =>
            {
                if (!connected)
                {
                    Debug.LogWarning($"FriendsService: Failed to connect to {userId}, request will be sent when connection is established");
                    onComplete?.Invoke(true); // Still consider it a success
                    return;
                }
                
                // Send the request
                try
                {
                    string jsonData = JsonConvert.SerializeObject(request);
                    byte[] requestPacket = FriendsNetworkProtocol.CreateFriendRequest(jsonData);
                    
                    _p2pNetworkService.SendMessage(userId, requestPacket, true, success =>
                    {
                        if (success)
                        {
                            Debug.Log($"FriendsService: Friend request sent to {userId}");
                        }
                        else
                        {
                            Debug.LogError($"FriendsService: Failed to send friend request to {userId}");
                        }
                        
                        onComplete?.Invoke(success);
                    });
                }
                catch (Exception ex)
                {
                    Debug.LogError($"FriendsService: Error sending friend request: {ex.Message}");
                    onComplete?.Invoke(false);
                }
            });
        }
        
        /// <summary>
        /// Accept a friend request
        /// </summary>
        /// <param name="requestId">Request ID</param>
        /// <param name="onComplete">Callback when request is accepted</param>
        public void AcceptFriendRequest(string requestId, Action<bool> onComplete = null)
        {
            if (!_isInitialized)
            {
                Debug.LogError("FriendsService: Not initialized");
                onComplete?.Invoke(false);
                return;
            }
            
            // Find the request
            if (!_pendingRequests.TryGetValue(requestId, out FriendRequest request))
            {
                Debug.LogError($"FriendsService: Friend request {requestId} not found");
                onComplete?.Invoke(false);
                return;
            }
            
            // Validate request
            if (request.Type != FriendRequestType.Received || request.ReceiverId != _identityService.GetCurrentUserId())
            {
                Debug.LogError($"FriendsService: Invalid friend request {requestId}");
                onComplete?.Invoke(false);
                return;
            }
            
            // Add as friend
            FriendData friendData = new FriendData
            {
                UserId = request.SenderId,
                DisplayName = request.SenderName,
                FriendCode = "",
                FriendSince = DateTime.UtcNow,
                Notes = "",
                IsFavorite = false
            };
            
            _friends[request.SenderId] = friendData;
            
            // Remove from pending requests
            _pendingRequests.Remove(requestId);
            
            // Save data
            SaveFriends();
            SavePendingRequests();
            
            // Notify listeners
            OnFriendRequestAccepted?.Invoke(request);
            OnFriendAdded?.Invoke(friendData);
            
            // Send response to the sender
            _p2pNetworkService.Connect(request.SenderId, connected =>
            {
                if (!connected)
                {
                    Debug.LogWarning($"FriendsService: Failed to connect to {request.SenderId}, accept response will be sent when connection is established");
                    onComplete?.Invoke(true); // Still consider it a success
                    return;
                }
                
                // Send accept message
                byte[] acceptPacket = FriendsNetworkProtocol.CreateFriendAccept(requestId);
                
                _p2pNetworkService.SendMessage(request.SenderId, acceptPacket, true, success =>
                {
                    if (success)
                    {
                        Debug.Log($"FriendsService: Friend request acceptance sent to {request.SenderName} ({request.SenderId})");
                        
                        // Send presence update
                        SendPresenceUpdate(request.SenderId);
                    }
                    else
                    {
                        Debug.LogError($"FriendsService: Failed to send friend acceptance to {request.SenderId}");
                    }
                    
                    onComplete?.Invoke(success);
                });
            });
        }
        
        /// <summary>
        /// Reject a friend request
        /// </summary>
        /// <param name="requestId">Request ID</param>
        /// <param name="onComplete">Callback when request is rejected</param>
        public void RejectFriendRequest(string requestId, Action<bool> onComplete = null)
        {
            if (!_isInitialized)
            {
                Debug.LogError("FriendsService: Not initialized");
                onComplete?.Invoke(false);
                return;
            }
            
            // Find the request
            if (!_pendingRequests.TryGetValue(requestId, out FriendRequest request))
            {
                Debug.LogError($"FriendsService: Friend request {requestId} not found");
                onComplete?.Invoke(false);
                return;
            }
            
            // Validate request
            if (request.Type != FriendRequestType.Received || request.ReceiverId != _identityService.GetCurrentUserId())
            {
                Debug.LogError($"FriendsService: Invalid friend request {requestId}");
                onComplete?.Invoke(false);
                return;
            }
            
            // Remove from pending requests
            _pendingRequests.Remove(requestId);
            
            // Save requests
            SavePendingRequests();
            
            // Notify listeners
            OnFriendRequestRejected?.Invoke(request);
            
            // Send response to the sender
            _p2pNetworkService.Connect(request.SenderId, connected =>
            {
                if (!connected)
                {
                    Debug.LogWarning($"FriendsService: Failed to connect to {request.SenderId}, reject response will be sent when connection is established");
                    onComplete?.Invoke(true); // Still consider it a success
                    return;
                }
                
                // Send reject message
                byte[] rejectPacket = FriendsNetworkProtocol.CreateFriendReject(requestId);
                
                _p2pNetworkService.SendMessage(request.SenderId, rejectPacket, true, success =>
                {
                    if (success)
                    {
                        Debug.Log($"FriendsService: Friend request rejection sent to {request.SenderName} ({request.SenderId})");
                    }
                    else
                    {
                        Debug.LogError($"FriendsService: Failed to send friend rejection to {request.SenderId}");
                    }
                    
                    onComplete?.Invoke(success);
                });
            });
        }
        
        /// <summary>
        /// Remove a friend
        /// </summary>
        /// <param name="friendId">Friend ID</param>
        /// <param name="onComplete">Callback when friend is removed</param>
        public void RemoveFriend(string friendId, Action<bool> onComplete = null)
        {
            if (!_isInitialized)
            {
                Debug.LogError("FriendsService: Not initialized");
                onComplete?.Invoke(false);
                return;
            }
            
            // Check if they are actually a friend
            if (!IsFriend(friendId))
            {
                Debug.LogError($"FriendsService: User {friendId} is not a friend");
                onComplete?.Invoke(false);
                return;
            }
            
            // Get friend name before removing
            string friendName = _friends[friendId].DisplayName;
            
            // Remove from friends
            _friends.Remove(friendId);
            
            // Save friends
            SaveFriends();
            
            // Notify listeners
            OnFriendRemoved?.Invoke(friendId);
            
            // Send removal notification
            if (_p2pNetworkService.IsConnectedTo(friendId))
            {
                byte[] removePacket = FriendsNetworkProtocol.CreateFriendRemove(_identityService.GetCurrentUserId());
                
                _p2pNetworkService.SendMessage(friendId, removePacket, true, success =>
                {
                    // Disconnect after sending the message
                    _p2pNetworkService.Disconnect(friendId);
                    
                    if (success)
                    {
                        Debug.Log($"FriendsService: Friend removal notification sent to {friendName} ({friendId})");
                    }
                    else
                    {
                        Debug.LogError($"FriendsService: Failed to send friend removal to {friendId}");
                    }
                    
                    onComplete?.Invoke(true); // Consider it a success even if notification fails
                });
            }
            else
            {
                Debug.Log($"FriendsService: Friend {friendName} ({friendId}) removed (offline)");
                onComplete?.Invoke(true);
            }
        }
        
        /// <summary>
        /// Get a list of pending friend requests
        /// </summary>
        /// <returns>List of pending requests</returns>
        public List<FriendRequest> GetPendingRequests()
        {
            return _pendingRequests.Values.ToList();
        }
        
        /// <summary>
        /// Get the user's friend code
        /// </summary>
        /// <returns>Friend code</returns>
        public string GetMyFriendCode()
        {
            return _identityService.GetMyFriendCode();
        }
        
        /// <summary>
        /// Load friends from persistent storage
        /// </summary>
        private void LoadFriends()
        {
            try
            {
                string filePath = Path.Combine(Application.persistentDataPath, SAVE_PATH, "friends.json");
                if (File.Exists(filePath))
                {
                    string jsonData = File.ReadAllText(filePath);
                    List<FriendData> friendsList = JsonConvert.DeserializeObject<List<FriendData>>(jsonData);
                    
                    _friends.Clear();
                    foreach (var friend in friendsList)
                    {
                        _friends[friend.UserId] = friend;
                    }
                    
                    Debug.Log($"FriendsService: Loaded {_friends.Count} friends");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"FriendsService: Error loading friends: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Save friends to persistent storage
        /// </summary>
        private void SaveFriends()
        {
            try
            {
                string dirPath = Path.Combine(Application.persistentDataPath, SAVE_PATH);
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
                
                string filePath = Path.Combine(dirPath, "friends.json");
                string jsonData = JsonConvert.SerializeObject(_friends.Values.ToList());
                
                File.WriteAllText(filePath, jsonData);
                
                Debug.Log($"FriendsService: Saved {_friends.Count} friends");
            }
            catch (Exception ex)
            {
                Debug.LogError($"FriendsService: Error saving friends: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Load pending requests from persistent storage
        /// </summary>
        private void LoadPendingRequests()
        {
            try
            {
                string filePath = Path.Combine(Application.persistentDataPath, SAVE_PATH, "requests.json");
                if (File.Exists(filePath))
                {
                    string jsonData = File.ReadAllText(filePath);
                    List<FriendRequest> requestsList = JsonConvert.DeserializeObject<List<FriendRequest>>(jsonData);
                    
                    _pendingRequests.Clear();
                    foreach (var request in requestsList)
                    {
                        _pendingRequests[request.RequestId] = request;
                    }
                    
                    Debug.Log($"FriendsService: Loaded {_pendingRequests.Count} pending requests");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"FriendsService: Error loading pending requests: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Save pending requests to persistent storage
        /// </summary>
        private void SavePendingRequests()
        {
            try
            {
                string dirPath = Path.Combine(Application.persistentDataPath, SAVE_PATH);
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
                
                string filePath = Path.Combine(dirPath, "requests.json");
                string jsonData = JsonConvert.SerializeObject(_pendingRequests.Values.ToList());
                
                File.WriteAllText(filePath, jsonData);
                
                Debug.Log($"FriendsService: Saved {_pendingRequests.Count} pending requests");
            }
            catch (Exception ex)
            {
                Debug.LogError($"FriendsService: Error saving pending requests: {ex.Message}");
            }
        }
    }
} 