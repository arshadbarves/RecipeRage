using System;
using UnityEngine;
using UnityEngine.UIElements;
using RecipeRage.Modules.Logging;

namespace RecipeRage.Modules.Friends.UI.Components
{
    /// <summary>
    /// UI component for adding new friends
    /// 
    /// Complexity Rating: 2
    /// </summary>
    public class AddFriendComponent : FriendsUIComponent
    {
        private TextField _friendCodeInput;
        private TextField _messageInput;
        private Button _sendButton;
        private Button _cancelButton;
        private Label _myFriendCodeLabel;
        private Label _errorLabel;
        private Label _successLabel;
        
        public event Action OnCancelClicked;
        
        /// <summary>
        /// Initialize the component
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            
            if (!_isInitialized)
                return;
                
            // Find UI elements
            _friendCodeInput = FindElement<TextField>("friend-code-input");
            _messageInput = FindElement<TextField>("message-input");
            _sendButton = FindElement<Button>("send-button");
            _cancelButton = FindElement<Button>("cancel-button");
            _myFriendCodeLabel = FindElement<Label>("my-friend-code");
            _errorLabel = FindElement<Label>("error-message");
            _successLabel = FindElement<Label>("success-message");
            
            if (_friendCodeInput == null || _sendButton == null || _cancelButton == null)
            {
                LogHelper.Error("FriendsUI", "Required UI elements not found for AddFriendComponent");
                return;
            }
            
            // Hide status messages initially
            if (_errorLabel != null)
            {
                _errorLabel.style.display = DisplayStyle.None;
            }
            
            if (_successLabel != null)
            {
                _successLabel.style.display = DisplayStyle.None;
            }
        }
        
        /// <summary>
        /// Register UI callbacks
        /// </summary>
        protected override void RegisterCallbacks()
        {
            base.RegisterCallbacks();
            
            if (_friendCodeInput != null)
            {
                _friendCodeInput.RegisterValueChangedCallback(OnFriendCodeChanged);
            }
            
            if (_sendButton != null)
            {
                _sendButton.clicked += OnSendRequestClicked;
            }
            
            if (_cancelButton != null)
            {
                _cancelButton.clicked += () => OnCancelClicked?.Invoke();
            }
        }
        
        /// <summary>
        /// Called when the component is shown
        /// </summary>
        protected override void OnShow()
        {
            base.OnShow();
            
            // Reset UI
            if (_friendCodeInput != null)
            {
                _friendCodeInput.value = "";
            }
            
            if (_messageInput != null)
            {
                _messageInput.value = "";
            }
            
            if (_errorLabel != null)
            {
                _errorLabel.style.display = DisplayStyle.None;
            }
            
            if (_successLabel != null)
            {
                _successLabel.style.display = DisplayStyle.None;
            }
            
            // Display user's own friend code
            if (_myFriendCodeLabel != null)
            {
                _myFriendCodeLabel.text = FriendsHelper.MyFriendCode ?? "Loading...";
            }
            
            // Focus the friend code input
            _friendCodeInput?.Focus();
        }
        
        /// <summary>
        /// Handle friend code input changes
        /// </summary>
        private void OnFriendCodeChanged(ChangeEvent<string> evt)
        {
            if (_errorLabel != null)
            {
                _errorLabel.style.display = DisplayStyle.None;
            }
            
            if (_successLabel != null)
            {
                _successLabel.style.display = DisplayStyle.None;
            }
            
            // Format the friend code as it's typed (add dashes)
            string input = evt.newValue;
            
            // Remove non-alphanumeric characters
            string cleanInput = System.Text.RegularExpressions.Regex.Replace(input, @"[^A-Z0-9a-z]", "").ToUpper();
            
            // Apply format with dashes
            if (cleanInput.Length > 0)
            {
                string formattedCode = "";
                for (int i = 0; i < cleanInput.Length; i++)
                {
                    if (i > 0 && i % 4 == 0 && i < 12)
                    {
                        formattedCode += "-";
                    }
                    
                    if (i < 12)
                    {
                        formattedCode += cleanInput[i];
                    }
                }
                
                if (formattedCode != input)
                {
                    _friendCodeInput.SetValueWithoutNotify(formattedCode);
                }
            }
        }
        
        /// <summary>
        /// Handle send request button click
        /// </summary>
        private void OnSendRequestClicked()
        {
            if (_friendCodeInput == null)
                return;
                
            string friendCode = _friendCodeInput.value;
            string message = _messageInput?.value ?? "";
            
            if (string.IsNullOrEmpty(friendCode))
            {
                ShowError("Please enter a friend code.");
                return;
            }
            
            if (friendCode == FriendsHelper.MyFriendCode)
            {
                ShowError("You cannot add yourself as a friend.");
                return;
            }
            
            _sendButton.SetEnabled(false);
            
            // Send the friend request
            FriendsHelper.SendFriendRequest(friendCode, message, success =>
            {
                _sendButton.SetEnabled(true);
                
                if (success)
                {
                    ShowSuccess("Friend request sent successfully!");
                    
                    // Clear inputs
                    _friendCodeInput.value = "";
                    if (_messageInput != null)
                    {
                        _messageInput.value = "";
                    }
                }
                else
                {
                    ShowError("Failed to send friend request. Please check the friend code and try again.");
                }
            });
        }
        
        /// <summary>
        /// Show an error message
        /// </summary>
        private void ShowError(string message)
        {
            if (_errorLabel == null)
                return;
                
            _errorLabel.text = message;
            _errorLabel.style.display = DisplayStyle.Flex;
            
            if (_successLabel != null)
            {
                _successLabel.style.display = DisplayStyle.None;
            }
        }
        
        /// <summary>
        /// Show a success message
        /// </summary>
        private void ShowSuccess(string message)
        {
            if (_successLabel == null)
                return;
                
            _successLabel.text = message;
            _successLabel.style.display = DisplayStyle.Flex;
            
            if (_errorLabel != null)
            {
                _errorLabel.style.display = DisplayStyle.None;
            }
        }
    }
} 