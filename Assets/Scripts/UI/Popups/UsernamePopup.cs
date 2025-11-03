using System;
using System.Text.RegularExpressions;
using Core.Animation;
using Core.Bootstrap;
using Core.Currency;
using Core.Events;
using Core.Logging;
using Core.SaveSystem;
using UI.Core;
using UI.Screens;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Popups
{
    /// <summary>
    /// Username popup for setting/changing player name
    /// First time: Free
    /// Subsequent changes: Costs gems
    /// Now properly extends BaseUIScreen for UIService integration
    /// </summary>
    [UIScreen(UIScreenType.UsernamePopup, UIScreenCategory.Popup, "Popups/UsernamePopupTemplate")]
    public class UsernamePopup : BaseUIScreen
    {
        private const int USERNAME_CHANGE_COST = 100; // Gems
        private const int MIN_USERNAME_LENGTH = 3;
        private const int MAX_USERNAME_LENGTH = 15;
        private static readonly Regex USERNAME_REGEX = new Regex(@"^[a-zA-Z0-9_]+$");

        private ISaveService _saveService;
        private ICurrencyService _currencyService;

        private TextField _usernameField;
        private Button _confirmButton;
        private Button _cancelButton;
        private Button _closeButton;
        private Label _titleLabel;
        private Label _errorLabel;
        private VisualElement _overlay;

        private bool _isFirstTime;
        private Action<string> _onConfirm;
        private Action _onCancel;

        protected override void OnInitialize()
        {
            // Get services
            _saveService = GameBootstrap.Services?.SaveService;
            _currencyService = GameBootstrap.Services?.CurrencyService;

            // Query elements
            QueryElements();

            // Setup callbacks
            SetupCallbacks();

            GameLogger.Log("Initialized");
        }

        private void QueryElements()
        {
            _overlay = GetElement<VisualElement>("popup-overlay");
            _usernameField = GetElement<TextField>("username-field");
            _confirmButton = GetElement<Button>("confirm-button");
            _cancelButton = GetElement<Button>("cancel-button");
            _closeButton = GetElement<Button>("close-button");
            _titleLabel = GetElement<Label>("popup-title");
            _errorLabel = GetElement<Label>("error-label");
        }

        private void SetupCallbacks()
        {
            if (_confirmButton != null)
            {
                _confirmButton.clicked += OnConfirmClicked;
            }

            if (_cancelButton != null)
            {
                _cancelButton.clicked += OnCancelClicked;
            }

            if (_closeButton != null)
            {
                _closeButton.clicked += OnCloseClicked;
            }

            if (_usernameField != null)
            {
                _usernameField.RegisterValueChangedCallback(OnUsernameChanged);
            }

            // Allow clicking overlay to close if closable
            if (_overlay != null)
            {
                _overlay.RegisterCallback<ClickEvent>(OnOverlayClicked);
            }
        }

        /// <summary>
        /// Configure and show popup for username entry
        /// </summary>
        public void ShowForUsername(bool isFirstTime, Action<string> onConfirm, Action onCancel = null)
        {
            _isFirstTime = isFirstTime;
            _onConfirm = onConfirm;
            _onCancel = onCancel;

            // Update UI based on first time or change
            UpdateUIForMode();

            // Clear previous input
            if (_usernameField != null && _saveService != null)
            {
                var stats = _saveService.GetPlayerStats();
                _usernameField.value = isFirstTime ? "" : stats.PlayerName;
            }

            if (_errorLabel != null)
            {
                _errorLabel.text = "";
                _errorLabel.style.display = DisplayStyle.None;
            }

            // Show the screen
            Show(animate: true, addToHistory: false);
        }

        protected override void OnShow()
        {
            // Focus username field when shown
            _usernameField?.Focus();
        }

        private void UpdateUIForMode()
        {
            if (_isFirstTime)
            {
                // First time - free and mandatory
                if (_titleLabel != null)
                {
                    _titleLabel.text = "Choose Your Username";
                }

                if (_confirmButton != null)
                {
                    _confirmButton.text = "Confirm";
                }

                if (_cancelButton != null)
                {
                    _cancelButton.style.display = DisplayStyle.None;
                }

                if (_closeButton != null)
                {
                    _closeButton.style.display = DisplayStyle.None;
                }

                // Disable overlay click for mandatory popup
                if (_overlay != null)
                {
                    _overlay.pickingMode = PickingMode.Ignore;
                }
            }
            else
            {
                // Changing username - costs gems and closable
                if (_titleLabel != null)
                {
                    _titleLabel.text = "Change Username";
                }

                // Show cost in confirm button
                if (_confirmButton != null)
                {
                    _confirmButton.text = $"Confirm ({USERNAME_CHANGE_COST} 💎)";
                }

                if (_cancelButton != null)
                {
                    _cancelButton.style.display = DisplayStyle.Flex;
                }

                if (_closeButton != null)
                {
                    _closeButton.style.display = DisplayStyle.Flex;
                }

                // Enable overlay click for closable popup
                if (_overlay != null)
                {
                    _overlay.pickingMode = PickingMode.Position;
                }
            }
        }

        private void OnUsernameChanged(ChangeEvent<string> evt)
        {
            // Clear error when user types
            if (_errorLabel != null)
            {
                _errorLabel.text = "";
                _errorLabel.style.display = DisplayStyle.None;
            }
        }

        private void OnConfirmClicked()
        {
            string username = _usernameField?.value?.Trim() ?? "";

            // Validate username
            if (!ValidateUsername(username, out string errorMessage))
            {
                ShowError(errorMessage);
                return;
            }

            // Check if changing username (not first time)
            if (!_isFirstTime)
            {
                // Check if user has enough gems
                if (_currencyService.Gems < USERNAME_CHANGE_COST)
                {
                    ShowError($"Not enough gems! Need {USERNAME_CHANGE_COST} gems.");
                    return;
                }

                // Deduct gems
                if (!_currencyService.SpendGems(USERNAME_CHANGE_COST))
                {
                    ShowError("Failed to deduct gems. Please try again.");
                    return;
                }

                GameLogger.Log($"Deducted {USERNAME_CHANGE_COST} gems for username change");
            }

            // Update player stats
            _saveService.UpdatePlayerStats(stats =>
            {
                stats.PlayerName = username;
                stats.UsernameChangeCount++;
            });

            GameLogger.Log($"Username set to: {username} (Change count: {_saveService.GetPlayerStats().UsernameChangeCount})");

            // Publish event to notify other UI components
            GameBootstrap.Services?.EventBus?.Publish(new PlayerStatsChangedEvent
            {
                PlayerName = username
            });

            // Invoke callback
            _onConfirm?.Invoke(username);

            // Hide popup
            Hide(animate: true);
        }

        private void OnCancelClicked()
        {
            if (_isFirstTime) return; // Cannot cancel if first time

            _onCancel?.Invoke();
            Hide(animate: true);
        }

        private void OnCloseClicked()
        {
            if (_isFirstTime) return; // Cannot close if first time

            _onCancel?.Invoke();
            Hide(animate: true);
        }

        private void OnOverlayClicked(ClickEvent evt)
        {
            if (_isFirstTime) return; // Cannot close if first time

            _onCancel?.Invoke();
            Hide(animate: true);
        }

        private bool ValidateUsername(string username, out string errorMessage)
        {
            errorMessage = "";

            if (string.IsNullOrWhiteSpace(username))
            {
                errorMessage = "Username cannot be empty";
                return false;
            }

            if (username.Length < MIN_USERNAME_LENGTH)
            {
                errorMessage = $"Username must be at least {MIN_USERNAME_LENGTH} characters";
                return false;
            }

            if (username.Length > MAX_USERNAME_LENGTH)
            {
                errorMessage = $"Username must be at most {MAX_USERNAME_LENGTH} characters";
                return false;
            }

            if (!USERNAME_REGEX.IsMatch(username))
            {
                errorMessage = "Username can only contain letters, numbers, and underscores";
                return false;
            }

            return true;
        }

        private void ShowError(string message)
        {
            if (_errorLabel != null)
            {
                _errorLabel.text = message;
                _errorLabel.style.display = DisplayStyle.Flex;
            }

            // Show error toast
            _ = GameBootstrap.Services?.UIService?.ShowToast(message, ToastType.Error, 3f);

            GameLogger.LogWarning($"Validation error: {message}");
        }

        protected override void OnDispose()
        {
            if (_confirmButton != null)
            {
                _confirmButton.clicked -= OnConfirmClicked;
            }

            if (_cancelButton != null)
            {
                _cancelButton.clicked -= OnCancelClicked;
            }

            if (_closeButton != null)
            {
                _closeButton.clicked -= OnCloseClicked;
            }

            if (_usernameField != null)
            {
                _usernameField.UnregisterValueChangedCallback(OnUsernameChanged);
            }

            if (_overlay != null)
            {
                _overlay.UnregisterCallback<ClickEvent>(OnOverlayClicked);
            }
        }

        // Override animations for popup behavior
        public override void AnimateShow(IUIAnimator animator, VisualElement element, float duration, Action onComplete)
        {
            // Scale in popup
            animator.ScaleIn(element, duration, onComplete);
        }

        public override void AnimateHide(IUIAnimator animator, VisualElement element, float duration, Action onComplete)
        {
            // Scale out popup
            animator.ScaleOut(element, duration, onComplete);
        }
    }
}
