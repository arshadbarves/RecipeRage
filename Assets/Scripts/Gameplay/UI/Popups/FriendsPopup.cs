using System;
using Modules.Logging;
using Modules.Networking;
using Modules.Networking.Interfaces;
using Modules.Session;
using Modules.UI; // Added
using Modules.UI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Gameplay.UI.Popups
{
    /// <summary>
    /// Friends popup - shows friends list and requests
    /// </summary>
    [UIScreen(UIScreenType.FriendsPopup, UIScreenCategory.Popup, "Popups/FriendsPopupTemplate")]
    public class FriendsPopup : BaseUIScreen
    {
        [Inject]
        private IUIService _uiService;

        [Inject]
        private SessionManager _sessionManager;

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

        private IFriendsService _friendsService;
        private ILobbyManager _lobbyManager;

        protected override void OnInitialize()
        {
            var sessionContainer = _sessionManager?.SessionContainer;
            if (sessionContainer != null)
            {
                var networking = sessionContainer.Resolve<INetworkingServices>();
                _friendsService = networking?.FriendsService;
                _lobbyManager = networking?.LobbyManager;
            }

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
            _copyCodeButton = GetElement<Button>("copy-friend-code-button");
            _friendsList = GetElement<ScrollView>("friends-list");
            _requestsList = GetElement<ScrollView>("requests-list");
            _titleLabel = GetElement<Label>("title-label");
            _myCodeLabel = GetElement<Label>("my-code-label");
            _friendsCountLabel = GetElement<Label>("friends-count-label");
            _requestsCountLabel = GetElement<Label>("requests-count-label");
            _requestsSection = GetElement<VisualElement>("requests-section");
            _myCodeSection = GetElement<VisualElement>("my-code-section");
        }

        private void SetupButtons()
        {
            if (_closeButton != null) _closeButton.clicked += OnCloseClicked;
            if (_refreshButton != null) _refreshButton.clicked += OnRefreshClicked;
            if (_addFriendButton != null) _addFriendButton.clicked += OnAddFriendClicked;
            if (_copyCodeButton != null) _copyCodeButton.clicked += OnCopyCodeClicked;
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
            if (_myCodeLabel == null || _friendsService == null) return;
            _myCodeLabel.text = _friendsService.IsInitialized ? _friendsService.MyFriendCode : "Loading...";
        }

        private async void LoadFriendRequests()
        {
            if (_requestsList == null || _friendsService == null) return;
            _requestsList.Clear();

            try
            {
                await _friendsService.RefreshFriendRequestsAsync();
                var requests = _friendsService.PendingRequests;

                if (requests.Count == 0)
                {
                    if (_requestsSection != null) _requestsSection.style.display = DisplayStyle.None;
                    return;
                }

                if (_requestsSection != null) _requestsSection.style.display = DisplayStyle.Flex;

                foreach (var request in requests)
                {
                    _requestsList.Add(CreateFriendRequestEntry(request));
                }

                if (_requestsCountLabel != null)
                    _requestsCountLabel.text = $"{requests.Count} Pending Request{(requests.Count != 1 ? "s" : "")}";
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Error loading requests: {ex.Message}");
            }
        }

        private async void LoadFriends()
        {
            if (_friendsList == null || _friendsService == null) return;
            _friendsList.Clear();

            try
            {
                await _friendsService.RefreshFriendsAsync();
                var friends = _friendsService.Friends;

                if (friends.Count == 0)
                {
                    ShowNoFriendsMessage();
                    return;
                }

                foreach (var friend in friends)
                {
                    _friendsList.Add(CreateFriendEntry(friend));
                }

                if (_friendsCountLabel != null)
                    _friendsCountLabel.text = $"{friends.Count} Friend{(friends.Count != 1 ? "s" : "")}";
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Error loading friends: {ex.Message}");
            }
        }

        private void ShowNoFriendsMessage()
        {
            var message = new Label("No friends yet.");
            _friendsList.Add(message);
        }

        private VisualElement CreateFriendRequestEntry(FriendRequest request)
        {
            var entry = new VisualElement();
            entry.Add(new Label(request.FromUserName));
            var acceptBtn = new Button(() => OnAcceptRequest(request)) { text = "ACCEPT" };
            entry.Add(acceptBtn);
            return entry;
        }

        private VisualElement CreateFriendEntry(FriendInfo friend)
        {
            var entry = new VisualElement();
            entry.Add(new Label(friend.DisplayName));
            return entry;
        }

        private async void OnAcceptRequest(FriendRequest request)
        {
            try
            {
                await _friendsService.AcceptFriendRequestAsync(request.Id);
                _uiService?.ShowNotification($"You are now friends with {request.FromUserName}!", NotificationType.Success);
                LoadFriendRequests();
                LoadFriends();
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Error: {ex.Message}");
            }
        }

        private void OnRefreshClicked()
        {
            LoadFriendRequests();
            LoadFriends();
        }

        private void OnAddFriendClicked() { }

        private void OnCopyCodeClicked()
        {
            if (_friendsService == null) return;
            GUIUtility.systemCopyBuffer = _friendsService.MyFriendCode;
            _uiService?.ShowNotification("Copied!", NotificationType.Success);
        }

        private void OnFriendsListUpdated() => LoadFriends();
        private void OnFriendRequestReceived(FriendRequest request) => LoadFriendRequests();

        private void OnCloseClicked() => Hide(true);

        protected override void OnDispose()
        {
            if (_friendsService != null)
            {
                _friendsService.OnFriendsListUpdated -= OnFriendsListUpdated;
                _friendsService.OnFriendRequestReceived -= OnFriendRequestReceived;
            }
        }
    }
}