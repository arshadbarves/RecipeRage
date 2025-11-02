using System;
using System.Collections.Generic;
using Core.Bootstrap;
using Core.Logging;
using Core.Networking.Interfaces;
using UI.Core;
using UI.Screens;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Popups
{
    /// <summary>
    /// Friends popup - Production version with friend requests
    /// Shows: My code, pending requests, friends list, add friend
    /// </summary>
    [UIScreen(UIScreenType.FriendsPopup, UIScreenPriority.Popup, "Popups/FriendsPopupTemplate")]
    public class FriendsPopup : BaseUIScreen
    {
        // UI Elements
        private Button _closeButton;
        private Button _refreshButton;
        private Button _addFriendButton;
        private Button _copyCodeButton;
        private ScrollView _friendsList;
        private ScrollView _requestsList;
        private Label _titleLabel;
        private Label _myCodeLabel;
        private Label _friendsCountLabel;
        private Label _requestsCountLabel;
        private VisualElement _requestsSection;
        private VisualElement _myCodeSection;

        // Services
        private IFriendsService _friendsService;
        private ILobbyManager _lobbyManager;

        protected override void OnInitialize()
        {
            // Get services
            var networking = GameBootstrap.Services?.NetworkingServices;
            _friendsService = networking?.FriendsService;
            _lobbyManager = networking?.LobbyManager;

            CacheUIElements();
            SetupButtons();
            SubscribeToEvents();

            GameLogger.Log("Initialized");
        }

        private void CacheUIElements()
        {
            _closeButton = GetElement<Button>("close-button");
            _refreshButton = GetElement<Button>("refresh-button");
            _addFriendButton = GetElement<Button>("add-friend-button");
            _copyCodeButton = GetElement<Button>("copy-code-button");
            _friendsList = GetElement<ScrollView>("friends-list");
            _requestsList = GetElement<ScrollView>("requests-list");
            _titleLabel = GetElement<Label>("title-label");
            _myCodeLabel = GetElement<Label>("my-code-label");
            _friendsCountLabel = GetElement<Label>("friends-count-label");
            _requestsCountLabel = GetElement<Label>("requests-count-label");
            _requestsSection = GetElement<VisualElement>("requests-section");
            _myCodeSection = GetElement<VisualElement>("my-code-section");

            if (_titleLabel != null)
            {
                _titleLabel.text = "FRIENDS";
            }
        }

        private void SetupButtons()
        {
            if (_closeButton != null)
                _closeButton.clicked += OnCloseClicked;

            if (_refreshButton != null)
                _refreshButton.clicked += OnRefreshClicked;

            if (_addFriendButton != null)
                _addFriendButton.clicked += OnAddFriendClicked;

            if (_copyCodeButton != null)
                _copyCodeButton.clicked += OnCopyCodeClicked;
        }

        private void SubscribeToEvents()
        {
            if (_friendsService != null)
            {
                _friendsService.OnFriendsListUpdated += OnFriendsListUpdated;
                _friendsService.OnFriendRequestReceived += OnFriendRequestReceived;
            }
        }

        protected override void OnShow()
        {
            UpdateMyCode();
            LoadFriendRequests();
            LoadFriends();
        }

        private void UpdateMyCode()
        {
            if (_myCodeLabel == null || _friendsService == null)
                return;

            if (_friendsService.IsInitialized)
            {
                _myCodeLabel.text = _friendsService.MyFriendCode;
            }
            else
            {
                _myCodeLabel.text = "Loading...";
            }
        }

        private async void LoadFriendRequests()
        {
            if (_requestsList == null || _friendsService == null)
                return;

            _requestsList.Clear();

            if (!_friendsService.IsInitialized)
            {
                GameLogger.LogWarning("Friends service not initialized");
                return;
            }

            try
            {
                // Refresh requests from backend
                await _friendsService.RefreshFriendRequestsAsync();

                var requests = _friendsService.PendingRequests;

                if (requests.Count == 0)
                {
                    if (_requestsSection != null)
                        _requestsSection.style.display = DisplayStyle.None;
                    return;
                }

                if (_requestsSection != null)
                    _requestsSection.style.display = DisplayStyle.Flex;

                foreach (var request in requests)
                {
                    var requestEntry = CreateFriendRequestEntry(request);
                    _requestsList.Add(requestEntry);
                }

                if (_requestsCountLabel != null)
                {
                    _requestsCountLabel.text = $"{requests.Count} Pending Request{(requests.Count != 1 ? "s" : "")}";
                }

                GameLogger.Log($"Loaded {requests.Count} friend requests");
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Error loading requests: {ex.Message}");
            }
        }

        private async void LoadFriends()
        {
            if (_friendsList == null || _friendsService == null)
                return;

            _friendsList.Clear();

            if (!_friendsService.IsInitialized)
            {
                ShowNoFriendsMessage();
                return;
            }

            try
            {
                // Refresh friends from backend
                await _friendsService.RefreshFriendsAsync();

                var friends = _friendsService.Friends;

                if (friends.Count == 0)
                {
                    ShowNoFriendsMessage();
                    return;
                }

                // Group by online status
                var onlineFriends = new List<FriendInfo>();
                var offlineFriends = new List<FriendInfo>();

                foreach (var friend in friends)
                {
                    if (friend.IsOnline)
                        onlineFriends.Add(friend);
                    else
                        offlineFriends.Add(friend);
                }

                // Add online friends
                if (onlineFriends.Count > 0)
                {
                    var onlineHeader = new Label($"Online ({onlineFriends.Count})");
                    onlineHeader.AddToClassList("friends-section-header");
                    _friendsList.Add(onlineHeader);

                    foreach (var friend in onlineFriends)
                    {
                        _friendsList.Add(CreateFriendEntry(friend));
                    }
                }

                // Add offline friends
                if (offlineFriends.Count > 0)
                {
                    var offlineHeader = new Label($"Offline ({offlineFriends.Count})");
                    offlineHeader.AddToClassList("friends-section-header");
                    _friendsList.Add(offlineHeader);

                    foreach (var friend in offlineFriends)
                    {
                        _friendsList.Add(CreateFriendEntry(friend));
                    }
                }

                // Update count
                if (_friendsCountLabel != null)
                {
                    _friendsCountLabel.text = $"{friends.Count} Friend{(friends.Count != 1 ? "s" : "")}";
                }

                GameLogger.Log($"Loaded {friends.Count} friends");
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Error loading friends: {ex.Message}");
            }
        }

        private void ShowNoFriendsMessage()
        {
            var message = new Label("No friends yet. Add friends using their Friend Code!");
            message.AddToClassList("no-friends-message");
            _friendsList.Add(message);

            if (_friendsCountLabel != null)
            {
                _friendsCountLabel.text = "0 Friends";
            }
        }

        private VisualElement CreateFriendRequestEntry(FriendRequest request)
        {
            var entry = new VisualElement();
            entry.AddToClassList("friend-request-entry");

            // Info
            var info = new VisualElement();
            info.AddToClassList("request-info");

            var nameLabel = new Label(request.FromUserName);
            nameLabel.AddToClassList("request-name");
            info.Add(nameLabel);

            var codeLabel = new Label($"Code: {request.FromFriendCode}");
            codeLabel.AddToClassList("request-code");
            info.Add(codeLabel);

            entry.Add(info);

            // Buttons
            var buttons = new VisualElement();
            buttons.AddToClassList("request-buttons");

            var acceptButton = new Button(() => OnAcceptRequest(request));
            acceptButton.text = "ACCEPT";
            acceptButton.AddToClassList("accept-button");
            buttons.Add(acceptButton);

            var rejectButton = new Button(() => OnRejectRequest(request));
            rejectButton.text = "REJECT";
            rejectButton.AddToClassList("reject-button");
            buttons.Add(rejectButton);

            entry.Add(buttons);

            return entry;
        }

        private VisualElement CreateFriendEntry(FriendInfo friend)
        {
            var entry = new VisualElement();
            entry.AddToClassList("friend-entry");

            // Status indicator
            var statusDot = new VisualElement();
            statusDot.AddToClassList("status-dot");
            statusDot.AddToClassList(friend.IsOnline ? "online" : "offline");
            entry.Add(statusDot);

            // Info
            var info = new VisualElement();
            info.AddToClassList("friend-info");

            var nameLabel = new Label(friend.DisplayName);
            nameLabel.AddToClassList("friend-name");
            info.Add(nameLabel);

            var codeLabel = new Label($"Code: {friend.FriendCode}");
            codeLabel.AddToClassList("friend-code");
            info.Add(codeLabel);

            entry.Add(info);

            // Invite button (only if online and in party)
            if (friend.IsOnline && _lobbyManager != null && _lobbyManager.IsInParty)
            {
                var inviteButton = new Button(() => OnInviteFriend(friend));
                inviteButton.text = "INVITE";
                inviteButton.AddToClassList("invite-button");
                entry.Add(inviteButton);
            }

            return entry;
        }

        private async void OnAcceptRequest(FriendRequest request)
        {
            GameLogger.Log($"Accepting request from: {request.FromUserName}");

            try
            {
                await _friendsService.AcceptFriendRequestAsync(request.Id);

                var uiService = GameBootstrap.Services?.UIService;
                uiService?.ShowToast($"You are now friends with {request.FromUserName}!", ToastType.Success, 2f);

                // Reload
                LoadFriendRequests();
                LoadFriends();
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Error accepting request: {ex.Message}");

                var uiService = GameBootstrap.Services?.UIService;
                uiService?.ShowToast("Failed to accept friend request", ToastType.Error, 2f);
            }
        }

        private async void OnRejectRequest(FriendRequest request)
        {
            GameLogger.Log($"Rejecting request from: {request.FromUserName}");

            try
            {
                await _friendsService.RejectFriendRequestAsync(request.Id);

                var uiService = GameBootstrap.Services?.UIService;
                uiService?.ShowToast("Friend request rejected", ToastType.Info, 2f);

                // Reload
                LoadFriendRequests();
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Error rejecting request: {ex.Message}");

                var uiService = GameBootstrap.Services?.UIService;
                uiService?.ShowToast("Failed to reject friend request", ToastType.Error, 2f);
            }
        }

        private void OnInviteFriend(FriendInfo friend)
        {
            GameLogger.Log($"Inviting friend: {friend.DisplayName}");

            if (_friendsService == null)
            {
                GameLogger.LogError("Friends service not available");
                return;
            }

            _friendsService.InviteToParty(friend.FriendCode);

            var uiService = GameBootstrap.Services?.UIService;
            uiService?.ShowToast($"Invited {friend.DisplayName} to party", ToastType.Info, 2f);
        }

        private async void OnRefreshClicked()
        {
            GameLogger.Log("Refreshing...");

            var uiService = GameBootstrap.Services?.UIService;
            uiService?.ShowToast("Refreshing...", ToastType.Info, 1f);

            LoadFriendRequests();
            LoadFriends();
        }

        private void OnAddFriendClicked()
        {
            GameLogger.Log("Opening add friend dialog");
            ShowAddFriendDialog();
        }

        private void OnCopyCodeClicked()
        {
            if (_friendsService == null || !_friendsService.IsInitialized)
                return;

            var code = _friendsService.MyFriendCode;
            GUIUtility.systemCopyBuffer = code;

            var uiService = GameBootstrap.Services?.UIService;
            uiService?.ShowToast($"Friend Code copied: {code}", ToastType.Success, 2f);

            GameLogger.Log($"Copied friend code: {code}");
        }

        private void ShowAddFriendDialog()
        {
            // Create dialog overlay
            var overlay = new VisualElement();
            overlay.AddToClassList("dialog-overlay");

            var dialog = new VisualElement();
            dialog.AddToClassList("dialog");

            var title = new Label("ADD FRIEND");
            title.AddToClassList("dialog-title");
            dialog.Add(title);

            var instruction = new Label("Enter your friend's 8-character code:");
            instruction.AddToClassList("dialog-instruction");
            dialog.Add(instruction);

            var codeField = new TextField();
            codeField.maxLength = 8;
            codeField.AddToClassList("dialog-input");
            dialog.Add(codeField);

            var buttons = new VisualElement();
            buttons.AddToClassList("dialog-buttons");

            var cancelButton = new Button(() =>
            {
                Container.Remove(overlay);
            });
            cancelButton.text = "CANCEL";
            cancelButton.AddToClassList("dialog-button");
            cancelButton.AddToClassList("cancel-button");
            buttons.Add(cancelButton);

            var addButton = new Button(async () =>
            {
                var code = codeField.value?.ToUpper().Trim();

                if (string.IsNullOrEmpty(code) || code.Length != 8)
                {
                    var uiService = GameBootstrap.Services?.UIService;
                    uiService?.ShowToast("Please enter a valid 8-character code", ToastType.Error, 2f);
                    return;
                }

                try
                {
                    await _friendsService.SendFriendRequestAsync(code);

                    var uiService = GameBootstrap.Services?.UIService;
                    uiService?.ShowToast("Friend request sent!", ToastType.Success, 2f);

                    Container.Remove(overlay);
                }
                catch (Exception ex)
                {
                    GameLogger.LogError($"Error sending request: {ex.Message}");

                    var uiService = GameBootstrap.Services?.UIService;
                    uiService?.ShowToast(ex.Message, ToastType.Error, 3f);
                }
            });
            addButton.text = "SEND REQUEST";
            addButton.AddToClassList("dialog-button");
            addButton.AddToClassList("add-button");
            buttons.Add(addButton);

            dialog.Add(buttons);
            overlay.Add(dialog);
            Container.Add(overlay);

            // Focus input
            codeField.Focus();
        }

        private void OnFriendsListUpdated()
        {
            GameLogger.Log("Friends list updated");
            LoadFriends();
        }

        private void OnFriendRequestReceived(FriendRequest request)
        {
            GameLogger.Log($"New friend request from: {request.FromUserName}");
            LoadFriendRequests();

            var uiService = GameBootstrap.Services?.UIService;
            uiService?.ShowToast($"Friend request from {request.FromUserName}", ToastType.Info, 3f);
        }

        private void OnCloseClicked()
        {
            Hide(true);
        }

        protected override void OnDispose()
        {
            // Unsubscribe from events
            if (_friendsService != null)
            {
                _friendsService.OnFriendsListUpdated -= OnFriendsListUpdated;
                _friendsService.OnFriendRequestReceived -= OnFriendRequestReceived;
            }

            // Unsubscribe from buttons
            if (_closeButton != null)
                _closeButton.clicked -= OnCloseClicked;

            if (_refreshButton != null)
                _refreshButton.clicked -= OnRefreshClicked;

            if (_addFriendButton != null)
                _addFriendButton.clicked -= OnAddFriendClicked;

            if (_copyCodeButton != null)
                _copyCodeButton.clicked -= OnCopyCodeClicked;
        }
    }
}
