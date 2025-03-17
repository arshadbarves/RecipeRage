using System;
using System.Collections.Generic;
using RecipeRage.Modules.Friends.Data;
using RecipeRage.Modules.Logging;
using UnityEngine;
using UnityEngine.UIElements;

namespace RecipeRage.Modules.Friends.UI.Components
{
    /// <summary>
    /// UI component for displaying friend requests
    /// Complexity Rating: 3
    /// </summary>
    public class FriendRequestsComponent : FriendsUIComponent
    {
        [SerializeField] private VisualTreeAsset _requestItemTemplate;
        private readonly Dictionary<string, VisualElement> _requestItems = new Dictionary<string, VisualElement>();
        private Label _emptyStateLabel;

        private ScrollView _requestsScrollView;

        /// <summary>
        /// Called when the component is disabled
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            // Unregister from events
            FriendsHelper.UnregisterEvents(
                onFriendRequestReceived: OnFriendRequestReceived,
                onFriendRequestAccepted: OnFriendRequestAccepted,
                onFriendRequestRejected: OnFriendRequestRejected
            );
        }

        public event Action<string> OnAcceptRequest;
        public event Action<string> OnRejectRequest;

        /// <summary>
        /// Initialize the component
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            if (!_isInitialized)
                return;

            // Find UI elements
            _requestsScrollView = FindElement<ScrollView>("requests-scroll-view");
            _emptyStateLabel = FindElement<Label>("empty-state-label");

            if (_requestsScrollView == null)
            {
                LogHelper.Error("FriendsUI", "Requests scroll view not found");
                return;
            }

            // Register for events from FriendsHelper
            FriendsHelper.RegisterEvents(
                onFriendRequestReceived: OnFriendRequestReceived,
                onFriendRequestAccepted: OnFriendRequestAccepted,
                onFriendRequestRejected: OnFriendRequestRejected
            );

            // Initialize UI
            RefreshRequestsList();
        }

        /// <summary>
        /// Called when the component is shown
        /// </summary>
        protected override void OnShow()
        {
            base.OnShow();

            // Refresh the list when shown
            RefreshRequestsList();
        }

        /// <summary>
        /// Refresh the requests list
        /// </summary>
        public void RefreshRequestsList()
        {
            if (!_isInitialized || _requestsScrollView == null)
                return;

            // Clear existing items
            _requestsScrollView.Clear();
            _requestItems.Clear();

            // Add requests to the list
            List<FriendRequest> requests = FriendsHelper.GetPendingFriendRequests();

            // Filter to only show received requests
            var receivedRequests = new List<FriendRequest>();
            foreach (var request in requests)
            {
                if (request.Type == FriendRequestType.Received)
                {
                    receivedRequests.Add(request);
                }
            }

            if (receivedRequests.Count == 0)
            {
                if (_emptyStateLabel != null)
                {
                    _emptyStateLabel.style.display = DisplayStyle.Flex;
                }
                return;
            }

            if (_emptyStateLabel != null)
            {
                _emptyStateLabel.style.display = DisplayStyle.None;
            }

            foreach (var request in receivedRequests)
            {
                AddRequestToList(request);
            }
        }

        /// <summary>
        /// Add a request to the list
        /// </summary>
        private void AddRequestToList(FriendRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.RequestId) || _requestItemTemplate == null)
                return;

            // Create request item from template
            var requestItem = _requestItemTemplate.Instantiate().Q<VisualElement>("request-item");
            if (requestItem == null)
            {
                LogHelper.Error("FriendsUI", "Could not create request item from template");
                return;
            }

            // Set request data
            var nameLabel = requestItem.Q<Label>("sender-name");
            var messageLabel = requestItem.Q<Label>("request-message");
            var timeLabel = requestItem.Q<Label>("request-time");
            var acceptButton = requestItem.Q<Button>("accept-button");
            var rejectButton = requestItem.Q<Button>("reject-button");

            if (nameLabel != null)
            {
                nameLabel.text = request.SenderName;
            }

            if (messageLabel != null)
            {
                if (!string.IsNullOrEmpty(request.Message))
                {
                    messageLabel.text = request.Message;
                    messageLabel.style.display = DisplayStyle.Flex;
                }
                else
                {
                    messageLabel.style.display = DisplayStyle.None;
                }
            }

            if (timeLabel != null)
            {
                // Format time based on how long ago it was
                var timeSince = DateTime.UtcNow - request.SentTime;

                if (timeSince.TotalDays >= 1)
                {
                    timeLabel.text = $"{(int)timeSince.TotalDays}d ago";
                }
                else if (timeSince.TotalHours >= 1)
                {
                    timeLabel.text = $"{(int)timeSince.TotalHours}h ago";
                }
                else if (timeSince.TotalMinutes >= 1)
                {
                    timeLabel.text = $"{(int)timeSince.TotalMinutes}m ago";
                }
                else
                {
                    timeLabel.text = "Just now";
                }
            }

            // Register for button events
            if (acceptButton != null)
            {
                acceptButton.clicked += () => OnAcceptRequest?.Invoke(request.RequestId);
            }

            if (rejectButton != null)
            {
                rejectButton.clicked += () => OnRejectRequest?.Invoke(request.RequestId);
            }

            // Store reference and add to scroll view
            _requestItems[request.RequestId] = requestItem;
            _requestsScrollView.Add(requestItem);
        }

        #region Event Handlers

        /// <summary>
        /// Called when a friend request is received
        /// </summary>
        private void OnFriendRequestReceived(FriendRequest request)
        {
            if (_emptyStateLabel != null)
            {
                _emptyStateLabel.style.display = DisplayStyle.None;
            }

            AddRequestToList(request);
        }

        /// <summary>
        /// Called when a friend request is accepted
        /// </summary>
        private void OnFriendRequestAccepted(FriendRequest request)
        {
            RemoveRequestFromList(request.RequestId);
        }

        /// <summary>
        /// Called when a friend request is rejected
        /// </summary>
        private void OnFriendRequestRejected(FriendRequest request)
        {
            RemoveRequestFromList(request.RequestId);
        }

        /// <summary>
        /// Remove a request from the list
        /// </summary>
        private void RemoveRequestFromList(string requestId)
        {
            if (_requestItems.TryGetValue(requestId, out var requestItem))
            {
                _requestsScrollView.Remove(requestItem);
                _requestItems.Remove(requestId);
            }

            if (_requestItems.Count == 0 && _emptyStateLabel != null)
            {
                _emptyStateLabel.style.display = DisplayStyle.Flex;
            }
        }

        #endregion
    }
}