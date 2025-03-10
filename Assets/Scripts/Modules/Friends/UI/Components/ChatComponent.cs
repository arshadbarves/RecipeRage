using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using RecipeRage.Modules.Friends.Data;
using RecipeRage.Modules.Logging;

namespace RecipeRage.Modules.Friends.UI.Components
{
    /// <summary>
    /// UI component for chat functionality
    /// 
    /// Complexity Rating: 4
    /// </summary>
    public class ChatComponent : FriendsUIComponent
    {
        [SerializeField] private VisualTreeAsset _textMessageTemplate;
        [SerializeField] private VisualTreeAsset _systemMessageTemplate;
        [SerializeField] private VisualTreeAsset _gameInviteTemplate;
        
        private string _currentFriendId;
        private string _currentFriendName;
        
        private ScrollView _messagesScrollView;
        private TextField _messageInput;
        private Button _sendButton;
        private Button _sendGameInviteButton;
        private Button _closeButton;
        private Label _friendNameLabel;
        private Label _statusLabel;
        private VisualElement _statusIndicator;
        private VisualElement _loadingContainer;
        private VisualElement _chatContainer;
        private VisualElement _messageComposer;
        
        private List<ChatMessage> _messages = new List<ChatMessage>();
        private Dictionary<string, VisualElement> _messageElements = new Dictionary<string, VisualElement>();
        
        public event Action<string, string> OnGameInviteSent;
        public event Action<string, string> OnGameInviteAccepted;
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
            _messagesScrollView = FindElement<ScrollView>("messages-scroll-view");
            _messageInput = FindElement<TextField>("message-input");
            _sendButton = FindElement<Button>("send-button");
            _sendGameInviteButton = FindElement<Button>("send-game-invite-button");
            _closeButton = FindElement<Button>("close-button");
            _friendNameLabel = FindElement<Label>("friend-name");
            _statusLabel = FindElement<Label>("status-text");
            _statusIndicator = FindElement<VisualElement>("status-indicator");
            _loadingContainer = FindElement<VisualElement>("loading-container");
            _chatContainer = FindElement<VisualElement>("chat-container");
            _messageComposer = FindElement<VisualElement>("message-composer");
            
            if (_messagesScrollView == null || _messageInput == null || _sendButton == null)
            {
                LogHelper.Error("FriendsUI", "Required UI elements not found for ChatComponent");
                return;
            }
            
            // Initially show loading
            if (_loadingContainer != null)
            {
                _loadingContainer.style.display = DisplayStyle.Flex;
            }
            
            if (_chatContainer != null)
            {
                _chatContainer.style.display = DisplayStyle.None;
            }
            
            // Register for chat events
            ChatHelper.RegisterEventHandlers(
                onMessageReceived: OnMessageReceived,
                onMessageSent: OnMessageSent,
                onChatHistoryLoaded: OnChatHistoryLoaded
            );
            
            // Register for presence changes
            FriendsHelper.RegisterEvents(
                onFriendPresenceChanged: OnFriendPresenceChanged
            );
        }
        
        /// <summary>
        /// Register UI callbacks
        /// </summary>
        protected override void RegisterCallbacks()
        {
            base.RegisterCallbacks();
            
            if (_messageInput != null)
            {
                _messageInput.RegisterCallback<KeyDownEvent>(evt =>
                {
                    if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                    {
                        if (!evt.shiftKey)
                        {
                            evt.PreventDefault();
                            evt.StopPropagation();
                            SendMessage();
                        }
                    }
                });
            }
            
            if (_sendButton != null)
            {
                _sendButton.clicked += SendMessage;
            }
            
            if (_sendGameInviteButton != null)
            {
                _sendGameInviteButton.clicked += OnSendGameInviteClicked;
            }
            
            if (_closeButton != null)
            {
                _closeButton.clicked += () => OnCloseClicked?.Invoke();
            }
        }
        
        /// <summary>
        /// Open a chat with a friend
        /// </summary>
        public void OpenChat(string friendId, string friendName)
        {
            if (string.IsNullOrEmpty(friendId))
            {
                LogHelper.Error("FriendsUI", "Cannot open chat: friend ID is empty");
                return;
            }
            
            _currentFriendId = friendId;
            _currentFriendName = friendName;
            
            // Update header
            if (_friendNameLabel != null)
            {
                _friendNameLabel.text = friendName;
            }
            
            // Show loading
            if (_loadingContainer != null)
            {
                _loadingContainer.style.display = DisplayStyle.Flex;
            }
            
            if (_chatContainer != null)
            {
                _chatContainer.style.display = DisplayStyle.None;
            }
            
            // Clear messages
            _messages.Clear();
            _messagesScrollView.Clear();
            _messageElements.Clear();
            
            // Load chat history
            ChatHelper.LoadChatHistory(friendId);
            
            // Mark messages as read
            ChatHelper.MarkAsRead(friendId);
            
            // Update presence info
            UpdatePresenceUI();
            
            // Focus message input
            if (_messageInput != null)
            {
                _messageInput.value = "";
                _messageInput.Focus();
            }
        }
        
        /// <summary>
        /// Send a message
        /// </summary>
        private void SendMessage()
        {
            if (string.IsNullOrEmpty(_currentFriendId) || _messageInput == null)
                return;
                
            string message = _messageInput.value?.Trim();
            
            if (string.IsNullOrEmpty(message))
                return;
                
            // Disable the input until the message is sent
            _messageInput.SetEnabled(false);
            _sendButton.SetEnabled(false);
            
            // Send the message
            ChatHelper.SendTextMessage(_currentFriendId, message, success =>
            {
                // Re-enable the input
                _messageInput.SetEnabled(true);
                _sendButton.SetEnabled(true);
                
                if (success)
                {
                    // Clear input
                    _messageInput.value = "";
                    _messageInput.Focus();
                }
            });
        }
        
        /// <summary>
        /// Send a game invite
        /// </summary>
        private void OnSendGameInviteClicked()
        {
            if (string.IsNullOrEmpty(_currentFriendId))
                return;
                
            // In a real implementation, this would open a dialog to configure the game invite
            // For now, we'll send a simple invite with hardcoded data
            
            string gameData = "{\"gameMode\":\"Standard\",\"mapId\":\"1\",\"difficulty\":\"Normal\"}";
            string message = "Join my game!";
            
            ChatHelper.SendGameInvite(_currentFriendId, gameData, message, success =>
            {
                if (success)
                {
                    LogHelper.Info("FriendsUI", $"Game invite sent to {_currentFriendName}");
                    OnGameInviteSent?.Invoke(_currentFriendId, gameData);
                }
            });
        }
        
        /// <summary>
        /// Update presence information
        /// </summary>
        private void UpdatePresenceUI()
        {
            if (string.IsNullOrEmpty(_currentFriendId))
                return;
                
            PresenceData presence = FriendsHelper.GetFriendPresence(_currentFriendId);
            
            if (_statusLabel != null)
            {
                if (presence != null && presence.IsOnline)
                {
                    _statusLabel.text = presence.Status.ToString();
                    
                    if (!string.IsNullOrEmpty(presence.Activity))
                    {
                        _statusLabel.text += $" - {presence.Activity}";
                    }
                }
                else
                {
                    _statusLabel.text = "Offline";
                }
            }
            
            if (_statusIndicator != null)
            {
                if (presence != null && presence.IsOnline)
                {
                    // Set color based on status
                    Color statusColor;
                    switch (presence.Status)
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
            
            // Enable/disable message composer based on online status
            if (_messageComposer != null)
            {
                if (presence != null && presence.IsOnline)
                {
                    _messageComposer.SetEnabled(true);
                }
                else
                {
                    _messageComposer.SetEnabled(false);
                }
            }
        }
        
        /// <summary>
        /// Add a message to the chat
        /// </summary>
        private void AddMessageToUI(ChatMessage message)
        {
            if (message == null || _messagesScrollView == null)
                return;
                
            // Skip if the message is already in the UI
            if (_messageElements.ContainsKey(message.MessageId))
                return;
                
            // Create message element based on type
            VisualElement messageElement = null;
            
            switch (message.MessageType)
            {
                case ChatMessageType.Text:
                    messageElement = CreateTextMessageElement(message);
                    break;
                    
                case ChatMessageType.System:
                    messageElement = CreateSystemMessageElement(message);
                    break;
                    
                case ChatMessageType.GameInvite:
                    messageElement = CreateGameInviteElement(message);
                    break;
            }
            
            if (messageElement == null)
                return;
                
            // Add to scroll view
            _messagesScrollView.Add(messageElement);
            _messageElements[message.MessageId] = messageElement;
            
            // Scroll to bottom
            _messagesScrollView.scrollOffset = new Vector2(0, float.MaxValue);
        }
        
        /// <summary>
        /// Create a text message element
        /// </summary>
        private VisualElement CreateTextMessageElement(ChatMessage message)
        {
            if (_textMessageTemplate == null)
                return null;
                
            VisualElement element = _textMessageTemplate.Instantiate();
            VisualElement container = element.Q<VisualElement>("message-container");
            Label contentLabel = element.Q<Label>("message-content");
            Label timeLabel = element.Q<Label>("message-time");
            Label nameLabel = element.Q<Label>("sender-name");
            
            if (container != null)
            {
                if (message.IsFromLocalUser)
                {
                    container.AddToClassList("outgoing");
                }
                else
                {
                    container.AddToClassList("incoming");
                }
            }
            
            if (contentLabel != null)
            {
                contentLabel.text = message.Content;
            }
            
            if (timeLabel != null)
            {
                timeLabel.text = message.GetFormattedTime();
            }
            
            if (nameLabel != null)
            {
                nameLabel.text = message.IsFromLocalUser ? "You" : message.SenderName;
            }
            
            return element;
        }
        
        /// <summary>
        /// Create a system message element
        /// </summary>
        private VisualElement CreateSystemMessageElement(ChatMessage message)
        {
            if (_systemMessageTemplate == null)
                return null;
                
            VisualElement element = _systemMessageTemplate.Instantiate();
            Label contentLabel = element.Q<Label>("message-content");
            Label timeLabel = element.Q<Label>("message-time");
            
            if (contentLabel != null)
            {
                contentLabel.text = message.Content;
            }
            
            if (timeLabel != null)
            {
                timeLabel.text = message.GetFormattedTime();
            }
            
            return element;
        }
        
        /// <summary>
        /// Create a game invite element
        /// </summary>
        private VisualElement CreateGameInviteElement(ChatMessage message)
        {
            if (_gameInviteTemplate == null)
                return null;
                
            VisualElement element = _gameInviteTemplate.Instantiate();
            VisualElement container = element.Q<VisualElement>("invite-container");
            Label contentLabel = element.Q<Label>("invite-message");
            Label timeLabel = element.Q<Label>("invite-time");
            Label nameLabel = element.Q<Label>("sender-name");
            Button acceptButton = element.Q<Button>("accept-button");
            Button declineButton = element.Q<Button>("decline-button");
            
            if (container != null)
            {
                if (message.IsFromLocalUser)
                {
                    container.AddToClassList("outgoing");
                    
                    // Hide accept/decline buttons for outgoing invites
                    if (acceptButton != null) acceptButton.style.display = DisplayStyle.None;
                    if (declineButton != null) declineButton.style.display = DisplayStyle.None;
                }
                else
                {
                    container.AddToClassList("incoming");
                    
                    // Add event handlers for incoming invites
                    if (acceptButton != null)
                    {
                        acceptButton.clicked += () => OnAcceptGameInvite(message);
                    }
                    
                    if (declineButton != null)
                    {
                        declineButton.clicked += () => OnDeclineGameInvite(message);
                    }
                }
            }
            
            if (contentLabel != null)
            {
                contentLabel.text = message.Content;
            }
            
            if (timeLabel != null)
            {
                timeLabel.text = message.GetFormattedTime();
            }
            
            if (nameLabel != null)
            {
                nameLabel.text = message.IsFromLocalUser ? "You" : message.SenderName;
            }
            
            return element;
        }
        
        /// <summary>
        /// Handle accepting a game invite
        /// </summary>
        private void OnAcceptGameInvite(ChatMessage message)
        {
            if (message == null || message.MessageType != ChatMessageType.GameInvite)
                return;
                
            ChatHelper.AcceptGameInvite(message, success =>
            {
                if (success)
                {
                    LogHelper.Info("FriendsUI", $"Accepted game invite from {message.SenderName}");
                    OnGameInviteAccepted?.Invoke(message.SenderId, message.AdditionalData);
                }
            });
        }
        
        /// <summary>
        /// Handle declining a game invite
        /// </summary>
        private void OnDeclineGameInvite(ChatMessage message)
        {
            if (message == null || message.MessageType != ChatMessageType.GameInvite)
                return;
                
            ChatHelper.DeclineGameInvite(message);
        }
        
        #region Event Handlers
        
        /// <summary>
        /// Called when a message is received
        /// </summary>
        private void OnMessageReceived(ChatMessage message)
        {
            if (message == null || message.SenderId != _currentFriendId && message.ReceiverId != _currentFriendId)
                return;
                
            // Add to messages
            _messages.Add(message);
            
            // Add to UI
            AddMessageToUI(message);
            
            // Mark as read
            ChatHelper.MarkAsRead(_currentFriendId);
        }
        
        /// <summary>
        /// Called when a message is sent
        /// </summary>
        private void OnMessageSent(ChatMessage message)
        {
            if (message == null || message.ReceiverId != _currentFriendId)
                return;
                
            // Add to messages
            _messages.Add(message);
            
            // Add to UI
            AddMessageToUI(message);
        }
        
        /// <summary>
        /// Called when chat history is loaded
        /// </summary>
        private void OnChatHistoryLoaded(string friendId, List<ChatMessage> messages)
        {
            if (friendId != _currentFriendId || messages == null)
                return;
                
            // Update messages
            _messages = new List<ChatMessage>(messages);
            
            // Clear UI
            _messagesScrollView.Clear();
            _messageElements.Clear();
            
            // Add all messages to UI
            foreach (var message in _messages)
            {
                AddMessageToUI(message);
            }
            
            // Show chat container
            if (_loadingContainer != null)
            {
                _loadingContainer.style.display = DisplayStyle.None;
            }
            
            if (_chatContainer != null)
            {
                _chatContainer.style.display = DisplayStyle.Flex;
            }
            
            // Scroll to bottom
            _messagesScrollView.scrollOffset = new Vector2(0, float.MaxValue);
        }
        
        /// <summary>
        /// Called when a friend's presence changes
        /// </summary>
        private void OnFriendPresenceChanged(string friendId, PresenceData presenceData)
        {
            if (friendId != _currentFriendId)
                return;
                
            UpdatePresenceUI();
        }
        
        #endregion
        
        /// <summary>
        /// Called when the component is disabled
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();
            
            // Unregister from events
            ChatHelper.UnregisterEventHandlers(
                onMessageReceived: OnMessageReceived,
                onMessageSent: OnMessageSent,
                onChatHistoryLoaded: OnChatHistoryLoaded
            );
            
            FriendsHelper.UnregisterEvents(
                onFriendPresenceChanged: OnFriendPresenceChanged
            );
        }
    }
} 