using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using RecipeRage.Modules.Friends.Data;
using RecipeRage.Modules.Logging;

namespace RecipeRage.Modules.Friends.UI.Components
{
    /// <summary>
    /// UI component for displaying the friends list
    /// 
    /// Complexity Rating: 3
    /// </summary>
    public class FriendsListComponent : FriendsUIComponent
    {
        [SerializeField] private VisualTreeAsset _friendItemTemplate;
        
        private ScrollView _friendsScrollView;
        private Button _addFriendButton;
        private Button _refreshButton;
        private Label _emptyStateLabel;
        private Dictionary<string, VisualElement> _friendItems = new Dictionary<string, VisualElement>();
        
        public event Action OnAddFriendClicked;
        public event Action<string> OnFriendClicked;
        public event Action<string> OnRemoveFriendClicked;
        
        /// <summary>
        /// Initialize the component
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            
            if (!_isInitialized)
                return;
                
            // Find UI elements
            _friendsScrollView = FindElement<ScrollView>("friends-scroll-view");
            _addFriendButton = FindElement<Button>("add-friend-button");
            _refreshButton = FindElement<Button>("refresh-button");
            _emptyStateLabel = FindElement<Label>("empty-state-label");
            
            if (_friendsScrollView == null)
            {
                LogHelper.Error("FriendsUI", "Friends scroll view not found");
                return;
            }
            
            // Register for events from FriendsHelper
            FriendsHelper.RegisterEvents(
                onFriendAdded: OnFriendAdded,
                onFriendRemoved: OnFriendRemoved,
                onFriendPresenceChanged: OnFriendPresenceChanged
            );
            
            // Initialize UI
            RefreshFriendsList();
        }
        
        /// <summary>
        /// Register UI callbacks
        /// </summary>
        protected override void RegisterCallbacks()
        {
            base.RegisterCallbacks();
            
            if (_addFriendButton != null)
            {
                _addFriendButton.clicked += () => OnAddFriendClicked?.Invoke();
            }
            
            if (_refreshButton != null)
            {
                _refreshButton.clicked += RefreshFriendsList;
            }
        }
        
        /// <summary>
        /// Called when the component is shown
        /// </summary>
        protected override void OnShow()
        {
            base.OnShow();
            
            // Refresh the list when shown
            RefreshFriendsList();
        }
        
        /// <summary>
        /// Refresh the friends list
        /// </summary>
        public void RefreshFriendsList()
        {
            if (!_isInitialized || _friendsScrollView == null)
                return;
                
            // Clear existing items
            _friendsScrollView.Clear();
            _friendItems.Clear();
            
            // Add friends to the list
            List<FriendData> friends = FriendsHelper.GetFriends();
            
            if (friends.Count == 0)
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
            
            foreach (var friend in friends)
            {
                AddFriendToList(friend);
            }
        }
        
        /// <summary>
        /// Add a friend to the list
        /// </summary>
        private void AddFriendToList(FriendData friend)
        {
            if (friend == null || string.IsNullOrEmpty(friend.UserId) || _friendItemTemplate == null)
                return;
                
            // Create friend item from template
            VisualElement friendItem = _friendItemTemplate.Instantiate().Q<VisualElement>("friend-item");
            if (friendItem == null)
            {
                LogHelper.Error("FriendsUI", "Could not create friend item from template");
                return;
            }
            
            // Set friend data
            Label nameLabel = friendItem.Q<Label>("friend-name");
            VisualElement statusIndicator = friendItem.Q<VisualElement>("status-indicator");
            Label activityLabel = friendItem.Q<Label>("friend-activity");
            Button removeButton = friendItem.Q<Button>("remove-button");
            
            if (nameLabel != null)
            {
                nameLabel.text = friend.DisplayName;
            }
            
            // Set status indicator
            UpdateFriendPresence(friendItem, friend.UserId);
            
            // Register for click events
            friendItem.RegisterCallback<ClickEvent>(evt => 
            {
                if (evt.target != removeButton)
                {
                    OnFriendClicked?.Invoke(friend.UserId);
                }
            });
            
            if (removeButton != null)
            {
                removeButton.clicked += () => OnRemoveFriendClicked?.Invoke(friend.UserId);
            }
            
            // Store reference and add to scroll view
            _friendItems[friend.UserId] = friendItem;
            _friendsScrollView.Add(friendItem);
        }
        
        /// <summary>
        /// Update a friend's presence in the UI
        /// </summary>
        private void UpdateFriendPresence(VisualElement friendItem, string friendId)
        {
            if (friendItem == null)
                return;
                
            VisualElement statusIndicator = friendItem.Q<VisualElement>("status-indicator");
            Label activityLabel = friendItem.Q<Label>("friend-activity");
            
            PresenceData presenceData = FriendsHelper.GetFriendPresence(friendId);
            
            if (statusIndicator != null)
            {
                if (presenceData != null && presenceData.IsOnline)
                {
                    // Set color based on status
                    Color statusColor;
                    switch (presenceData.Status)
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
                    
                    statusIndicator.style.backgroundColor = statusColor;
                }
                else
                {
                    // Offline
                    statusIndicator.style.backgroundColor = new Color(0.6f, 0.6f, 0.6f); // Gray
                }
            }
            
            if (activityLabel != null)
            {
                if (presenceData != null && presenceData.IsOnline && !string.IsNullOrEmpty(presenceData.Activity))
                {
                    activityLabel.text = presenceData.Activity;
                    activityLabel.style.display = DisplayStyle.Flex;
                    
                    // Add joinable indicator if applicable
                    if (presenceData.IsJoinable)
                    {
                        activityLabel.text += " (Joinable)";
                        activityLabel.AddToClassList("joinable");
                    }
                    else
                    {
                        activityLabel.RemoveFromClassList("joinable");
                    }
                }
                else
                {
                    activityLabel.style.display = DisplayStyle.None;
                }
            }
        }
        
        #region Event Handlers
        
        /// <summary>
        /// Called when a friend is added
        /// </summary>
        private void OnFriendAdded(FriendData friend)
        {
            if (_emptyStateLabel != null)
            {
                _emptyStateLabel.style.display = DisplayStyle.None;
            }
            
            AddFriendToList(friend);
        }
        
        /// <summary>
        /// Called when a friend is removed
        /// </summary>
        private void OnFriendRemoved(string friendId)
        {
            if (_friendItems.TryGetValue(friendId, out VisualElement friendItem))
            {
                _friendsScrollView.Remove(friendItem);
                _friendItems.Remove(friendId);
            }
            
            if (_friendItems.Count == 0 && _emptyStateLabel != null)
            {
                _emptyStateLabel.style.display = DisplayStyle.Flex;
            }
        }
        
        /// <summary>
        /// Called when a friend's presence changes
        /// </summary>
        private void OnFriendPresenceChanged(string friendId, PresenceData presenceData)
        {
            if (_friendItems.TryGetValue(friendId, out VisualElement friendItem))
            {
                UpdateFriendPresence(friendItem, friendId);
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
                onFriendAdded: OnFriendAdded,
                onFriendRemoved: OnFriendRemoved,
                onFriendPresenceChanged: OnFriendPresenceChanged
            );
        }
    }
} 