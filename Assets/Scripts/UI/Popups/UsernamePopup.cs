using Core.Bootstrap;
using Core.Currency;
using Core.Events;
using Core.Logging;
using Core.SaveSystem;
using UI;
using UI.Core;
using UI.Screens;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using System;

namespace UI.Popups
{
    /// <summary>
    /// Mandatory popup for setting/changing username
    /// Used for both first-time setup and subsequent changes
    /// </summary>
    [UIScreen(UIScreenType.UsernamePopup, UIScreenCategory.Modal, "Popups/UsernamePopupTemplate")]
    public class UsernamePopup : BaseUIScreen
    {
        [Inject]
        private IEventBus _eventBus;

        [Inject]
        private ISaveService _saveService;

        [Inject]
        private IUIService _uiService;

        [Inject]
        private SessionManager _sessionManager;

        private ICurrencyService CurrencyService
        {
            get
            {
                var sessionContainer = _sessionManager?.SessionContainer;
                if (sessionContainer != null)
                {
                    return sessionContainer.Resolve<ICurrencyService>();
                }
                return null;
            }
        }

        private TextField _usernameField;
        private Label _statusLabel;
        private Label _costLabel;
        private Button _confirmButton;
        private Button _cancelButton;
        private VisualElement _costContainer;

        private Action<string> _onConfirm;
        private Action _onCancel;
        private bool _isFirstTime;
        private int _changeCost = 0; // Cost in gems to change name

        protected override void OnInitialize()
        {
            _usernameField = GetElement<TextField>("username-input");
            _statusLabel = GetElement<Label>("status-message");
            _costLabel = GetElement<Label>("cost-value");
            _confirmButton = GetElement<Button>("confirm-button");
            _cancelButton = GetElement<Button>("cancel-button");
            _costContainer = GetElement<VisualElement>("cost-container");

            if (_confirmButton != null)
                _confirmButton.clicked += OnConfirmClicked;

            if (_cancelButton != null)
                _cancelButton.clicked += OnCancelClicked;

            if (_usernameField != null)
            {
                _usernameField.RegisterValueChangedCallback(OnUsernameChanged);
                _usernameField.maxLength = 16;
            }
        }

        protected override void OnShow()
        {
            if (_usernameField != null)
            {
                _usernameField.value = _saveService?.GetPlayerStats().PlayerName ?? "";
                _usernameField.Focus();
            }

            UpdateStatus("", false);
            UpdateCostDisplay();
        }

        public void ShowForUsername(bool isFirstTime, Action<string> onConfirm, Action onCancel = null)
        {
            _isFirstTime = isFirstTime;
            _onConfirm = onConfirm;
            _onCancel = onCancel;

            // First time is free, subsequent changes might cost gems
            _changeCost = isFirstTime ? 0 : 50;

            UpdateCostDisplay();

            // Cancel button is hidden for first-time mandatory setup
            if (_cancelButton != null)
            {
                _cancelButton.style.display = isFirstTime ? DisplayStyle.None : DisplayStyle.Flex;
            }

            Show(true, false);
        }

        private void UpdateCostDisplay()
        {
            if (_costContainer == null) return;

            if (_changeCost > 0)
            {
                _costContainer.style.display = DisplayStyle.Flex;
                if (_costLabel != null) _costLabel.text = _changeCost.ToString();
            }
            else
            {
                _costContainer.style.display = DisplayStyle.None;
            }
        }

        private void OnUsernameChanged(ChangeEvent<string> evt)
        {
            string newName = evt.newValue?.Trim();
            bool isValid = ValidateUsername(newName, out string error);

            if (!string.IsNullOrEmpty(newName))
            {
                UpdateStatus(error, !isValid);
            }
            else
            {
                UpdateStatus("", false);
            }
        }

        private bool ValidateUsername(string name, out string error)
        {
            if (string.IsNullOrEmpty(name))
            {
                error = "Username cannot be empty";
                return false;
            }

            if (name.Length < 3)
            {
                error = "Minimum 3 characters required";
                return false;
            }

            if (name.Length > 16)
            {
                error = "Maximum 16 characters allowed";
                return false;
            }

            // Simple profanity or character check could go here
            foreach (char c in name)
            {
                if (!char.IsLetterOrDigit(c) && c != '_' && c != ' ')
                {
                    error = "Only letters, numbers, and underscores allowed";
                    return false;
                }
            }

            error = "Username is available";
            return true;
        }

        private void UpdateStatus(string message, bool isError)
        {
            if (_statusLabel == null) return;

            _statusLabel.text = message;
            _statusLabel.style.color = isError ? Color.red : Color.green;
            _statusLabel.style.visibility = string.IsNullOrEmpty(message) ? Visibility.Hidden : Visibility.Visible;
        }

        private async void OnConfirmClicked()
        {
            string newName = _usernameField?.value?.Trim();

            if (!ValidateUsername(newName, out string error))
            {
                UpdateStatus(error, true);
                return;
            }

            // Check cost
            if (_changeCost > 0)
            {
                if (CurrencyService == null || CurrencyService.Gems < _changeCost)
                {
                    UpdateStatus("Not enough gems!", true);
                    return;
                }
            }

            _confirmButton.SetEnabled(false);
            UpdateStatus("Saving...", false);

            try
            {
                // In a real game, we'd check availability with backend here
                // For now, just save locally via SaveService

                if (_changeCost > 0)
                {
                    CurrencyService.SpendGems(_changeCost);
                }

                _saveService.UpdatePlayerStats(stats =>
                {
                    stats.PlayerName = newName;
                });

                _onConfirm?.Invoke(newName);

                _uiService?.ShowNotification("Username updated successfully!", NotificationType.Success);

                Hide(true);
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Failed to save username: {ex.Message}");
                UpdateStatus("Failed to save. Try again.", true);
                _confirmButton.SetEnabled(true);
            }
        }

        private void OnCancelClicked()
        {
            _onCancel?.Invoke();
            Hide(true);
        }

        protected override void OnDispose()
        {
            if (_confirmButton != null)
                _confirmButton.clicked -= OnConfirmClicked;

            if (_cancelButton != null)
                _cancelButton.clicked -= OnCancelClicked;

            if (_usernameField != null)
                _usernameField.UnregisterValueChangedCallback(OnUsernameChanged);
        }
    }
}