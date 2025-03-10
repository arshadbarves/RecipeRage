using System;
using UnityEngine;
using UnityEngine.UIElements;
using RecipeRage.Modules.Friends.Data;
using RecipeRage.Modules.Logging;

namespace RecipeRage.Modules.Friends.UI.Components
{
    /// <summary>
    /// UI component for displaying and managing friend profiles
    /// 
    /// Complexity Rating: 3
    /// </summary>
    public class FriendProfileComponent : FriendsUIComponent
    {
        private string _currentFriendId;
        private FriendData _currentFriend;
        private PresenceData _currentPresence;
        
        private Label _nameLabel;
        private Label _friendCodeLabel;
        private Label _statusLabel;
        private Label _activityLabel;
        private TextField _notesInput;
        private Button _saveNotesButton;
        private Button _removeFriendButton;
        private Button _sendMessageButton;
        private Button _joinActivityButton;
        private Button _closeButton;
        private VisualElement _statusIndicator;
        private VisualElement _contentContainer;
        private VisualElement _loadingContainer;
        
        public event Action<string> OnRemoveFriendClicked;
        public event Action<string> OnSendMessageClicked;
        public event Action<string, string> OnJoinActivityClicked;
        public event Action OnCloseClicked;
        
        /// <summary>
        /// Initialize the component
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            
            if (!_isInitialized)
                return;
                
            // Find UI elements
            _nameLabel = FindElement<Label>("friend-name");
            _friendCodeLabel = FindElement<Label>("friend-code");
            _statusLabel = FindElement<Label>("status-text");
            _activityLabel = FindElement<Label>("activity-text");
            _notesInput = FindElement<TextField>("notes-input");
            _saveNotesButton = FindElement<Button>("save-notes-button");
            _removeFriendButton = FindElement<Button>("remove-friend-button");
            _sendMessageButton = FindElement<Button>("send-message-button");
            _joinActivityButton = FindElement<Button>("join-activity-button");
            _closeButton = FindElement<Button>("close-button");
            _statusIndicator = FindElement<VisualElement>("status-indicator");
            _contentContainer = FindElement<VisualElement>("content-container");
            _loadingContainer = FindElement<VisualElement>("loading-container");
            
            if (_contentContainer == null || _closeButton == null)
            {
                LogHelper.Error("FriendsUI", "Required UI elements not found for FriendProfileComponent");
                return;
            }
            
            // Initially show loading
            if (_loadingContainer != null)
            {
                _loadingContainer.style.display = DisplayStyle.Flex;
            }
            
            if (_contentContainer != null)
            {
                _contentContainer.style.display = DisplayStyle.None;
            }
            
            // Register for presence changes
            FriendsHelper.RegisterEvents(
                onFriendRemoved: OnFriendRemoved,
                onFriendPresenceChanged: OnFriendPresenceChanged
            );
        }
        
        /// <summary>
        /// Register UI callbacks
        /// </summary>
        protected override void RegisterCallbacks()
        {
            base.RegisterCallbacks();
            
            if (_saveNotesButton != null)
            {
                _saveNotesButton.clicked += OnSaveNotesClicked;
            }
            
            if (_removeFriendButton != null)
            {
                _removeFriendButton.clicked += () => OnRemoveFriendClicked?.Invoke(_currentFriendId);
            }
            
            if (_sendMessageButton != null)
            {
                _sendMessageButton.clicked += () => OnSendMessageClicked?.Invoke(_currentFriendId);
            }
            
            if (_joinActivityButton != null)
            {
                _joinActivityButton.clicked += () => 
                {
                    if (_currentPresence != null && _currentPresence.IsJoinable)
                    {
                        OnJoinActivityClicked?.Invoke(_currentFriendId, _currentPresence.JoinData);
                    }
                };
            }
            
            if (_closeButton != null)
            {
                _closeButton.clicked += () => OnCloseClicked?.Invoke();
            }
        }
        
        /// <summary>
        /// Load a friend's profile
        /// </summary>
        public void LoadFriend(string friendId)
        {
            if (string.IsNullOrEmpty(friendId))
            {
                LogHelper.Error("FriendsUI", "Cannot load friend profile: friend ID is empty");
                return;
            }
            
            _currentFriendId = friendId;
            
            // Show loading state
            if (_loadingContainer != null)
            {
                _loadingContainer.style.display = DisplayStyle.Flex;
            }
            
            if (_contentContainer != null)
            {
                _contentContainer.style.display = DisplayStyle.None;
            }
            
            // Get friend data
            List<FriendData> friends = FriendsHelper.GetFriends();
            
            foreach (var friend in friends)
            {
                if (friend.UserId == friendId)
                {
                    _currentFriend = friend;
                    break;
                }
            }
            
            if (_currentFriend == null)
            {
                LogHelper.Error("FriendsUI", $"Friend not found with ID: {friendId}");
                return;
            }
            
            // Get presence data
            _currentPresence = FriendsHelper.GetFriendPresence(friendId);
            
            // Update UI
            UpdateUI();
            
            // Show content
            if (_loadingContainer != null)
            {
                _loadingContainer.style.display = DisplayStyle.None;
            }
            
            if (_contentContainer != null)
            {
                _contentContainer.style.display = DisplayStyle.Flex;
            }
        }
        
        /// <summary>
        /// Update the UI with current friend data
        /// </summary>
        private void UpdateUI()
        {
            if (_currentFriend == null)
                return;
                
            // Set name
            if (_nameLabel != null)
            {
                _nameLabel.text = _currentFriend.DisplayName;
            }
            
            // Set friend code
            if (_friendCodeLabel != null)
            {
                _friendCodeLabel.text = _currentFriend.FriendCode;
            }
            
            // Set notes
            if (_notesInput != null)
            {
                _notesInput.value = _currentFriend.Notes ?? "";
            }
            
            // Update presence information
            UpdatePresenceUI();
        }
        
        /// <summary>
        /// Update the presence UI elements
        /// </summary>
        private void UpdatePresenceUI()
        {
            if (_currentPresence == null)
            {
                if (_statusLabel != null)
                {
                    _statusLabel.text = "Offline";
                }
                
                if (_statusIndicator != null)
                {
                    _statusIndicator.style.backgroundColor = new Color(0.6f, 0.6f, 0.6f); // Gray
                }
                
                if (_activityLabel != null)
                {
                    _activityLabel.style.display = DisplayStyle.None;
                }
                
                if (_joinActivityButton != null)
                {
                    _joinActivityButton.style.display = DisplayStyle.None;
                }
                
                return;
            }
            
            // Set status
            if (_statusLabel != null)
            {
                _statusLabel.text = _currentPresence.Status.ToString();
            }
            
            // Set status color
            if (_statusIndicator != null)
            {
                if (_currentPresence.IsOnline)
                {
                    // Set color based on status
                    Color statusColor;
                    switch (_currentPresence.Status)
                    {
                        case UserStatus.Online:
                            statusColor = new Color(0.2f, 0.8f, 0.2f); // Green
                            break;
                        case UserStatus.Away:
                            statusColor = new Color(0.9f, 0.7f, 0.1f); // Yellow
                            break;
                        case UserStatus.DoNotDisturb:
                            statusColor = new Color(0.9f, 0.3f, 0.1f); // Red
                            break;
                        case UserStatus.Playing:
                            statusColor = new Color(0.2f, 0.6f, 0.9f); // Blue
                            break;
                        default:
                            statusColor = new Color(0.6f, 0.6f, 0.6f); // Gray
                            break;
                    }
                    
                    _statusIndicator.style.backgroundColor = statusColor;
                }
                else
                {
                    // Offline
                    _statusIndicator.style.backgroundColor = new Color(0.6f, 0.6f, 0.6f); // Gray
                }
            }
            
            // Set activity
            if (_activityLabel != null)
            {
                if (_currentPresence.IsOnline && !string.IsNullOrEmpty(_currentPresence.Activity))
                {
                    _activityLabel.text = _currentPresence.Activity;
                    _activityLabel.style.display = DisplayStyle.Flex;
                }
                else
                {
                    _activityLabel.style.display = DisplayStyle.None;
                }
            }
            
            // Set join button
            if (_joinActivityButton != null)
            {
                if (_currentPresence.IsOnline && _currentPresence.IsJoinable)
                {
                    _joinActivityButton.style.display = DisplayStyle.Flex;
                }
                else
                {
                    _joinActivityButton.style.display = DisplayStyle.None;
                }
            }
        }
        
        /// <summary>
        /// Handle save notes button click
        /// </summary>
        private void OnSaveNotesClicked()
        {
            if (_currentFriend == null || _notesInput == null)
                return;
                
            // Update notes
            _currentFriend.Notes = _notesInput.value;
            
            // TODO: Save notes persistently
            
            LogHelper.Info("FriendsUI", $"Saved notes for {_currentFriend.DisplayName}");
        }
        
        #region Event Handlers
        
        /// <summary>
        /// Called when a friend is removed
        /// </summary>
        private void OnFriendRemoved(string friendId)
        {
            if (friendId == _currentFriendId)
            {
                // The current friend was removed, close the profile
                OnCloseClicked?.Invoke();
            }
        }
        
        /// <summary>
        /// Called when a friend's presence changes
        /// </summary>
        private void OnFriendPresenceChanged(string friendId, PresenceData presenceData)
        {
            if (friendId == _currentFriendId)
            {
                _currentPresence = presenceData;
                UpdatePresenceUI();
            }
        }
        
        #endregion
        
        /// <summary>
        /// Called when the component is disabled
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();
            
            // Unregister from events
            FriendsHelper.UnregisterEvents(
                onFriendRemoved: OnFriendRemoved,
                onFriendPresenceChanged: OnFriendPresenceChanged
            );
        }
    }
} 