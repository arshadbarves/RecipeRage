using System.Collections.Generic;
using RecipeRage.Modules.Auth;
using RecipeRage.Modules.Friends;
using RecipeRage.Modules.Friends.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RecipeRage.Examples
{
    /// <summary>
    /// Example script demonstrating how to use the Friends system
    /// Complexity Rating: 2
    /// </summary>
    public class FriendsExample : MonoBehaviour
    {
        [Header("Friend Code UI")] public TMP_Text friendCodeText;

        public TMP_InputField addFriendInput;
        public Button addFriendButton;

        [Header("Friends List UI")] public Transform friendsListContainer;

        public GameObject friendEntryPrefab;

        [Header("Friend Requests UI")] public Transform requestsListContainer;

        public GameObject requestEntryPrefab;

        private readonly Dictionary<string, GameObject> _friendEntries = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, GameObject> _requestEntries = new Dictionary<string, GameObject>();

        private bool _isInitialized;

        private void Start()
        {
            // Wait for authentication to complete
            if (AuthHelper.IsSignedIn())
                InitializeFriends();
            else
                Debug.LogWarning("FriendsExample: Please sign in first to use the Friends system");

            // Setup UI
            if (addFriendButton != null) addFriendButton.onClick.AddListener(OnAddFriendClicked);
        }

        private void OnDestroy()
        {
            // Unregister events when destroyed
            if (_isInitialized)
                FriendsHelper.UnregisterEvents(
                    OnFriendAdded,
                    OnFriendRemoved,
                    OnFriendRequestReceived,
                    OnFriendRequestAccepted,
                    OnFriendRequestRejected,
                    OnFriendPresenceChanged
                );
        }

        /// <summary>
        /// Initialize the Friends system
        /// </summary>
        private void InitializeFriends()
        {
            Debug.Log("FriendsExample: Initializing Friends system...");

            // Initialize the Friends system
            FriendsHelper.Initialize(true, success =>
            {
                if (success)
                {
                    Debug.Log("FriendsExample: Friends system initialized successfully");

                    // Register for events
                    FriendsHelper.RegisterEvents(
                        OnFriendAdded,
                        OnFriendRemoved,
                        OnFriendRequestReceived,
                        OnFriendRequestAccepted,
                        OnFriendRequestRejected,
                        OnFriendPresenceChanged
                    );

                    // Display friend code
                    if (friendCodeText != null) friendCodeText.text = FriendsHelper.MyFriendCode;

                    // Load existing friends and requests
                    RefreshFriendsList();
                    RefreshRequestsList();

                    // Set initial status
                    FriendsHelper.SetStatus(UserStatus.Online);
                    FriendsHelper.SetActivity("Playing RecipeRage", true, "main_menu");

                    _isInitialized = true;
                }
                else
                {
                    Debug.LogError("FriendsExample: Failed to initialize Friends system");
                }
            });
        }

        /// <summary>
        /// Refresh the friends list UI
        /// </summary>
        private void RefreshFriendsList()
        {
            if (friendsListContainer == null) return;

            // Clear previous entries
            foreach (var entry in _friendEntries.Values) Destroy(entry);
            _friendEntries.Clear();

            // Add friends to the list
            List<FriendData> friends = FriendsHelper.GetFriends();
            foreach (var friend in friends) AddFriendToUI(friend);
        }

        /// <summary>
        /// Refresh the friend requests list UI
        /// </summary>
        private void RefreshRequestsList()
        {
            if (requestsListContainer == null) return;

            // Clear previous entries
            foreach (var entry in _requestEntries.Values) Destroy(entry);
            _requestEntries.Clear();

            // Add pending requests to the list
            List<FriendRequest> requests = FriendsHelper.GetPendingFriendRequests();
            foreach (var request in requests)
                if (request.Type == FriendRequestType.Received)
                    AddRequestToUI(request);
        }

        /// <summary>
        /// Add a friend to the UI
        /// </summary>
        /// <param name="friend"> Friend data </param>
        private void AddFriendToUI(FriendData friend)
        {
            if (friendsListContainer == null || friendEntryPrefab == null) return;

            // Create friend entry
            var entryObj = Instantiate(friendEntryPrefab, friendsListContainer);

            // Set friend name
            var nameText = entryObj.GetComponentInChildren<TMP_Text>();
            if (nameText != null) nameText.text = friend.DisplayName;

            // Get presence info
            var presence = FriendsHelper.GetFriendPresence(friend.UserId);

            // Update status indicator
            var statusIndicator = entryObj.transform.Find("StatusIndicator")?.GetComponent<Image>();
            if (statusIndicator != null)
            {
                if (presence != null && presence.IsOnline)
                    statusIndicator.color = Color.green;
                else
                    statusIndicator.color = Color.gray;
            }

            // Setup remove button
            var removeButton = entryObj.transform.Find("RemoveButton")?.GetComponent<Button>();
            if (removeButton != null) removeButton.onClick.AddListener(() => OnRemoveFriendClicked(friend.UserId));

            // Store reference
            _friendEntries[friend.UserId] = entryObj;
        }

        /// <summary>
        /// Update a friend's UI with new presence data
        /// </summary>
        /// <param name="friendId"> Friend ID </param>
        /// <param name="presence"> New presence data </param>
        private void UpdateFriendPresenceUI(string friendId, PresenceData presence)
        {
            if (!_friendEntries.TryGetValue(friendId, out var entryObj)) return;

            // Update status indicator
            var statusIndicator = entryObj.transform.Find("StatusIndicator")?.GetComponent<Image>();
            if (statusIndicator != null)
            {
                if (presence.IsOnline)
                    statusIndicator.color = Color.green;
                else
                    statusIndicator.color = Color.gray;
            }

            // Update activity text if present
            var activityText = entryObj.transform.Find("ActivityText")?.GetComponent<TMP_Text>();
            if (activityText != null)
            {
                if (presence.IsOnline && !string.IsNullOrEmpty(presence.Activity))
                {
                    activityText.text = presence.Activity;
                    activityText.gameObject.SetActive(true);
                }
                else
                {
                    activityText.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Add a friend request to the UI
        /// </summary>
        /// <param name="request"> Friend request </param>
        private void AddRequestToUI(FriendRequest request)
        {
            if (requestsListContainer == null || requestEntryPrefab == null) return;

            // Create request entry
            var entryObj = Instantiate(requestEntryPrefab, requestsListContainer);

            // Set sender name
            var nameText = entryObj.GetComponentInChildren<TMP_Text>();
            if (nameText != null) nameText.text = request.SenderName;

            // Set message if any
            var messageText = entryObj.transform.Find("MessageText")?.GetComponent<TMP_Text>();
            if (messageText != null)
            {
                if (!string.IsNullOrEmpty(request.Message))
                {
                    messageText.text = request.Message;
                    messageText.gameObject.SetActive(true);
                }
                else
                {
                    messageText.gameObject.SetActive(false);
                }
            }

            // Setup accept button
            var acceptButton = entryObj.transform.Find("AcceptButton")?.GetComponent<Button>();
            if (acceptButton != null) acceptButton.onClick.AddListener(() => OnAcceptRequestClicked(request.RequestId));

            // Setup reject button
            var rejectButton = entryObj.transform.Find("RejectButton")?.GetComponent<Button>();
            if (rejectButton != null) rejectButton.onClick.AddListener(() => OnRejectRequestClicked(request.RequestId));

            // Store reference
            _requestEntries[request.RequestId] = entryObj;
        }

        /// <summary>
        /// Handle Add Friend button click
        /// </summary>
        private void OnAddFriendClicked()
        {
            if (!_isInitialized || addFriendInput == null) return;

            string friendCode = addFriendInput.text.Trim();
            if (string.IsNullOrEmpty(friendCode))
            {
                Debug.LogWarning("FriendsExample: Please enter a friend code");
                return;
            }

            // Send friend request
            FriendsHelper.SendFriendRequest(friendCode, null, success =>
            {
                if (success)
                {
                    Debug.Log($"FriendsExample: Friend request sent to {friendCode}");

                    // Clear input field
                    addFriendInput.text = "";
                }
                else
                {
                    Debug.LogError($"FriendsExample: Failed to send friend request to {friendCode}");
                }
            });
        }

        /// <summary>
        /// Handle Remove Friend button click
        /// </summary>
        /// <param name="friendId"> Friend ID to remove </param>
        private void OnRemoveFriendClicked(string friendId)
        {
            if (!_isInitialized) return;

            // Remove friend
            FriendsHelper.RemoveFriend(friendId);
        }

        /// <summary>
        /// Handle Accept Request button click
        /// </summary>
        /// <param name="requestId"> Request ID to accept </param>
        private void OnAcceptRequestClicked(string requestId)
        {
            if (!_isInitialized) return;

            // Accept friend request
            FriendsHelper.AcceptFriendRequest(requestId);
        }

        /// <summary>
        /// Handle Reject Request button click
        /// </summary>
        /// <param name="requestId"> Request ID to reject </param>
        private void OnRejectRequestClicked(string requestId)
        {
            if (!_isInitialized) return;

            // Reject friend request
            FriendsHelper.RejectFriendRequest(requestId);
        }

        #region Event Handlers

        /// <summary>
        /// Called when a friend is added
        /// </summary>
        /// <param name="friend"> New friend data </param>
        private void OnFriendAdded(FriendData friend)
        {
            Debug.Log($"FriendsExample: Friend added: {friend.DisplayName}");
            AddFriendToUI(friend);
        }

        /// <summary>
        /// Called when a friend is removed
        /// </summary>
        /// <param name="friendId"> Removed friend ID </param>
        private void OnFriendRemoved(string friendId)
        {
            Debug.Log($"FriendsExample: Friend removed: {friendId}");

            if (_friendEntries.TryGetValue(friendId, out var entryObj))
            {
                Destroy(entryObj);
                _friendEntries.Remove(friendId);
            }
        }

        /// <summary>
        /// Called when a friend request is received
        /// </summary>
        /// <param name="request"> Received friend request </param>
        private void OnFriendRequestReceived(FriendRequest request)
        {
            Debug.Log($"FriendsExample: Friend request received from {request.SenderName}");
            AddRequestToUI(request);
        }

        /// <summary>
        /// Called when a friend request is accepted
        /// </summary>
        /// <param name="request"> Accepted friend request </param>
        private void OnFriendRequestAccepted(FriendRequest request)
        {
            Debug.Log($"FriendsExample: Friend request accepted: {request.RequestId}");

            if (_requestEntries.TryGetValue(request.RequestId, out var entryObj))
            {
                Destroy(entryObj);
                _requestEntries.Remove(request.RequestId);
            }
        }

        /// <summary>
        /// Called when a friend request is rejected
        /// </summary>
        /// <param name="request"> Rejected friend request </param>
        private void OnFriendRequestRejected(FriendRequest request)
        {
            Debug.Log($"FriendsExample: Friend request rejected: {request.RequestId}");

            if (_requestEntries.TryGetValue(request.RequestId, out var entryObj))
            {
                Destroy(entryObj);
                _requestEntries.Remove(request.RequestId);
            }
        }

        /// <summary>
        /// Called when a friend's presence changes
        /// </summary>
        /// <param name="friendId"> Friend ID </param>
        /// <param name="presence"> New presence data </param>
        private void OnFriendPresenceChanged(string friendId, PresenceData presence)
        {
            Debug.Log($"FriendsExample: Friend presence changed: {friendId} - {presence.Status}");
            UpdateFriendPresenceUI(friendId, presence);
        }

        #endregion
    }
}