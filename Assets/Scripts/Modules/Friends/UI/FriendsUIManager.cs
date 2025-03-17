using RecipeRage.Modules.Friends.Data;
using RecipeRage.Modules.Friends.UI.Components;
using RecipeRage.Modules.Logging;
using RecipeRage.Modules.Logging.Interfaces;
using UnityEngine;
using UnityEngine.UIElements;

namespace RecipeRage.Modules.Friends.UI
{
    /// <summary>
    /// Manager for the friends system UI components
    /// Coordinates interactions between different UI components
    /// Complexity Rating: 3
    /// </summary>
    public class FriendsUIManager : MonoBehaviour
    {
        [SerializeField] private UIDocument _document;

        [Header("UI Components")] [SerializeField]
        private FriendsListComponent _friendsListComponent;

        [SerializeField] private FriendRequestsComponent _friendRequestsComponent;
        [SerializeField] private AddFriendComponent _addFriendComponent;
        [SerializeField] private FriendProfileComponent _friendProfileComponent;
        [SerializeField] private ChatComponent _chatComponent;
        private Button _closeButton;
        private string _currentScreen = "friends"; // friends, requests, add, profile, chat

        private bool _isInitialized;
        private VisualElement _mainContainer;
        private Label _requestsCountLabel;

        private VisualElement _root;
        private Button _toggleRequestsButton;

        private void Awake()
        {
            // Initialize systems
            InitializeModules();
        }

        private void OnEnable()
        {
            Initialize();
        }

        private void OnDisable()
        {
            // Unregister event handlers
            FriendsHelper.UnregisterEvents(
                onFriendRequestReceived: OnFriendRequestReceived
            );

            ChatHelper.UnregisterEventHandlers(
                OnMessageReceived
            );
        }

        private void OnDestroy()
        {
            // Shutdown systems
            ChatHelper.Shutdown();
        }

        /// <summary>
        /// Initialize modules
        /// </summary>
        private void InitializeModules()
        {
            // Initialize logging first
            LogHelper.SetConsoleOutput(true);
            LogHelper.SetFileOutput(true);
            LogHelper.SetLogLevel(LogLevel.Debug);

            // Make sure Friends system is initialized
            if (!FriendsHelper.IsInitialized)
                FriendsHelper.Initialize(OnFriendsInitialized);
            else
                OnFriendsInitialized(true);
        }

        /// <summary>
        /// Callback when friends system is initialized
        /// </summary>
        private void OnFriendsInitialized(bool success)
        {
            if (!success)
            {
                LogHelper.Error("FriendsUI", "Failed to initialize Friends system");
                return;
            }

            // Initialize chat system
            if (!FriendsHelper.IsInitialized)
            {
                LogHelper.Error("FriendsUI", "Cannot initialize Chat system: Friends system not initialized");
                return;
            }

            ChatHelper.Initialize(OnChatInitialized);
        }

        /// <summary>
        /// Callback when chat system is initialized
        /// </summary>
        private void OnChatInitialized(bool success)
        {
            if (!success)
            {
                LogHelper.Error("FriendsUI", "Failed to initialize Chat system");
                return;
            }

            // Now we can proceed with UI initialization
            if (!_isInitialized) Initialize();

            // Refresh data
            RefreshUI();
        }

        /// <summary>
        /// Initialize the UI
        /// </summary>
        private void Initialize()
        {
            if (_isInitialized)
                return;

            if (_document == null)
            {
                _document = GetComponent<UIDocument>();
                if (_document == null)
                {
                    LogHelper.Error("FriendsUI", "UIDocument not found");
                    return;
                }
            }

            _root = _document.rootVisualElement;
            if (_root == null)
            {
                LogHelper.Error("FriendsUI", "Root visual element not found");
                return;
            }

            // Find UI elements
            _closeButton = _root.Q<Button>("close-button");
            _toggleRequestsButton = _root.Q<Button>("toggle-requests-button");
            _requestsCountLabel = _root.Q<Label>("requests-count");
            _mainContainer = _root.Q<VisualElement>("main-container");

            // Initialize components
            InitializeComponents();

            // Register event handlers
            RegisterEventHandlers();

            // Show initial screen
            ShowScreen("friends");

            _isInitialized = true;
            LogHelper.Debug("FriendsUI", "Initialized FriendsUIManager");
        }

        /// <summary>
        /// Initialize UI components
        /// </summary>
        private void InitializeComponents()
        {
            // Initialize friends list component
            if (_friendsListComponent != null)
            {
                _friendsListComponent.Initialize();
                _friendsListComponent.OnAddFriendClicked += () => ShowScreen("add");
                _friendsListComponent.OnFriendClicked += OnFriendClicked;
                _friendsListComponent.OnRemoveFriendClicked += OnRemoveFriendClicked;
            }

            // Initialize friend requests component
            if (_friendRequestsComponent != null)
            {
                _friendRequestsComponent.Initialize();
                _friendRequestsComponent.OnAcceptRequest += OnAcceptFriendRequest;
                _friendRequestsComponent.OnRejectRequest += OnRejectFriendRequest;
            }

            // Initialize add friend component
            if (_addFriendComponent != null)
            {
                _addFriendComponent.Initialize();
                _addFriendComponent.OnCancelClicked += () => ShowScreen("friends");
            }

            // Initialize friend profile component
            if (_friendProfileComponent != null)
            {
                _friendProfileComponent.Initialize();
                _friendProfileComponent.OnCloseClicked += () => ShowScreen("friends");
                _friendProfileComponent.OnRemoveFriendClicked += OnRemoveFriendClicked;
                _friendProfileComponent.OnSendMessageClicked += OnOpenChat;
            }

            // Initialize chat component
            if (_chatComponent != null)
            {
                _chatComponent.Initialize();
                _chatComponent.OnCloseClicked += () => ShowScreen("friends");
            }
        }

        /// <summary>
        /// Register event handlers
        /// </summary>
        private void RegisterEventHandlers()
        {
            if (_closeButton != null) _closeButton.clicked += () => Hide();

            if (_toggleRequestsButton != null)
                _toggleRequestsButton.clicked +=
                    () => ShowScreen(_currentScreen == "requests" ? "friends" : "requests");

            // Register for friends system events
            FriendsHelper.RegisterEvents(
                onFriendRequestReceived: OnFriendRequestReceived
            );

            // Register for chat system events
            ChatHelper.RegisterEventHandlers(
                OnMessageReceived
            );
        }

        /// <summary>
        /// Show a specific screen
        /// </summary>
        private void ShowScreen(string screen)
        {
            _currentScreen = screen;

            // Hide all components
            if (_friendsListComponent != null) _friendsListComponent.Hide();
            if (_friendRequestsComponent != null) _friendRequestsComponent.Hide();
            if (_addFriendComponent != null) _addFriendComponent.Hide();
            if (_friendProfileComponent != null) _friendProfileComponent.Hide();
            if (_chatComponent != null) _chatComponent.Hide();

            // Show the appropriate component
            switch (screen)
            {
                case "friends":
                    if (_friendsListComponent != null) _friendsListComponent.Show();
                    break;

                case "requests":
                    if (_friendRequestsComponent != null) _friendRequestsComponent.Show();
                    break;

                case "add":
                    if (_addFriendComponent != null) _addFriendComponent.Show();
                    break;

                case "profile":
                    if (_friendProfileComponent != null) _friendProfileComponent.Show();
                    break;

                case "chat":
                    if (_chatComponent != null) _chatComponent.Show();
                    break;
            }

            // Update UI state
            UpdateUIState();
        }

        /// <summary>
        /// Update UI state based on current screen
        /// </summary>
        private void UpdateUIState()
        {
            if (_toggleRequestsButton != null)
                _toggleRequestsButton.text = _currentScreen == "requests" ? "Show Friends" : "Show Requests";

            // Update requests count
            UpdateRequestsCount();
        }

        /// <summary>
        /// Update the requests count label
        /// </summary>
        private void UpdateRequestsCount()
        {
            if (_requestsCountLabel == null)
                return;

            // Count pending friend requests
            int count = 0;
            List<FriendRequest> requests = FriendsHelper.GetPendingFriendRequests();

            foreach (var request in requests)
                if (request.Type == FriendRequestType.Received)
                    count++;

            // Update label
            if (count > 0)
            {
                _requestsCountLabel.text = count.ToString();
                _requestsCountLabel.style.display = DisplayStyle.Flex;
            }
            else
            {
                _requestsCountLabel.style.display = DisplayStyle.None;
            }
        }

        /// <summary>
        /// Refresh the UI
        /// </summary>
        private void RefreshUI()
        {
            // Refresh all components
            if (_friendsListComponent != null) _friendsListComponent.RefreshFriendsList();
            if (_friendRequestsComponent != null) _friendRequestsComponent.RefreshRequestsList();

            // Update UI state
            UpdateUIState();
        }

        /// <summary>
        /// Show the UI
        /// </summary>
        public void Show()
        {
            if (_mainContainer != null) _mainContainer.style.display = DisplayStyle.Flex;

            // Refresh data
            RefreshUI();
        }

        /// <summary>
        /// Hide the UI
        /// </summary>
        public void Hide()
        {
            if (_mainContainer != null) _mainContainer.style.display = DisplayStyle.None;
        }

        #region Event Handlers

        /// <summary>
        /// Handle friend click
        /// </summary>
        private void OnFriendClicked(string friendId)
        {
            List<FriendData> friends = FriendsHelper.GetFriends();
            FriendData friend = friends.Find(f => f.UserId == friendId);

            if (friend != null)
            {
                // Check if there are unread messages
                if (ChatHelper.GetUnreadCount(friendId) > 0)
                {
                    // Open chat directly
                    OnOpenChat(friendId);
                }
                else
                {
                    // Show profile
                    if (_friendProfileComponent != null)
                    {
                        _friendProfileComponent.LoadFriend(friendId);
                        ShowScreen("profile");
                    }
                }
            }
        }

        /// <summary>
        /// Handle removing a friend
        /// </summary>
        private void OnRemoveFriendClicked(string friendId)
        {
            FriendsHelper.RemoveFriend(friendId, success =>
            {
                if (success)
                    // Go back to friends list
                    ShowScreen("friends");
            });
        }

        /// <summary>
        /// Handle accepting a friend request
        /// </summary>
        private void OnAcceptFriendRequest(string requestId)
        {
            FriendsHelper.AcceptFriendRequest(requestId, success =>
            {
                if (success)
                {
                    // Refresh friends list
                    if (_friendsListComponent != null) _friendsListComponent.RefreshFriendsList();

                    // Refresh requests list
                    if (_friendRequestsComponent != null) _friendRequestsComponent.RefreshRequestsList();

                    // Update UI state
                    UpdateUIState();
                }
            });
        }

        /// <summary>
        /// Handle rejecting a friend request
        /// </summary>
        private void OnRejectFriendRequest(string requestId)
        {
            FriendsHelper.RejectFriendRequest(requestId, success =>
            {
                if (success)
                {
                    // Refresh requests list
                    if (_friendRequestsComponent != null) _friendRequestsComponent.RefreshRequestsList();

                    // Update UI state
                    UpdateUIState();
                }
            });
        }

        /// <summary>
        /// Handle opening a chat
        /// </summary>
        private void OnOpenChat(string friendId)
        {
            // Find friend data
            List<FriendData> friends = FriendsHelper.GetFriends();
            FriendData friend = friends.Find(f => f.UserId == friendId);

            if (friend != null && _chatComponent != null)
            {
                _chatComponent.OpenChat(friendId, friend.DisplayName);
                ShowScreen("chat");
            }
        }

        /// <summary>
        /// Handle friend request received
        /// </summary>
        private void OnFriendRequestReceived(FriendRequest request)
        {
            // Update requests count
            UpdateRequestsCount();
        }

        /// <summary>
        /// Handle message received
        /// </summary>
        private void OnMessageReceived(ChatMessage message)
        {
            // Update UI when a message is received
            // This could show a notification or highlight the friends list
            UpdateUIState();
        }

        #endregion
    }
}