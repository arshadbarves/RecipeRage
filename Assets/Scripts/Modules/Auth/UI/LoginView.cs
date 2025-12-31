using System;
using Core.Logging;
using RecipeRage.Modules.Auth.Core;
using UI;
using UI.Core;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;
using VContainer;

namespace RecipeRage.Modules.Auth.UI
{
    /// <summary>
    /// New Login View implementing the Skewed Shop aesthetic.
    /// Handles production-ready EOS DeviceID authentication.
    /// </summary>
    [UIScreen(UIScreenType.Login, UIScreenCategory.Overlay, "Screens/LoginView")]
    public class LoginView : BaseUIScreen
    {
        [Inject]
        private IAuthService _authService;

        private Button _guestLoginButton;
        private Label _statusText;
        private VisualElement _loadingIndicator;

        protected override void OnInitialize()
        {
            _guestLoginButton = GetElement<Button>("guest-login-button");
            _statusText = GetElement<Label>("status-text");
            _loadingIndicator = GetElement<VisualElement>("loading-indicator");

            if (_guestLoginButton != null)
            {
                _guestLoginButton.clicked += OnGuestLoginClicked;
            }
        }

        protected override void OnShow()
        {
            UpdateStatus("Ready to connect");
            SetLoading(false);
        }

        private void OnGuestLoginClicked()
        {
            if (_authService == null)
            {
                GameLogger.LogError("[AuthUI] AuthService not found!");
                return;
            }

            HandleLoginAsync(AuthType.DeviceID).Forget();
        }

        private async UniTaskVoid HandleLoginAsync(AuthType type)
        {
            SetLoading(true);
            UpdateStatus("Connecting to EOS...");

            try
            {
                bool success = await _authService.LoginAsync(type);
                if (success)
                {
                    UpdateStatus("Connected!");
                    // LoginSuccessEvent will be caught by LoginState to transition screens
                }
                else
                {
                    UpdateStatus("Connection failed. Try again.");
                    SetLoading(false);
                }
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"[AuthUI] Login Error: {ex.Message}");
                UpdateStatus("Error during connection");
                SetLoading(false);
            }
        }

        private void SetLoading(bool isLoading)
        {
            if (_loadingIndicator != null)
            {
                _loadingIndicator.style.display = isLoading ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (_guestLoginButton != null)
            {
                _guestLoginButton.SetEnabled(!isLoading);
            }
        }

        private void UpdateStatus(string message)
        {
            if (_statusText != null)
            {
                _statusText.text = message.ToUpper();
            }
        }

        protected override void OnDispose()
        {
            if (_guestLoginButton != null)
            {
                _guestLoginButton.clicked -= OnGuestLoginClicked;
            }
        }
    }
}
