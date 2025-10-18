using System;
using System.Collections;
using Core.Authentication;
using Core.Bootstrap;
using Core.Utilities;
using UI.UISystem.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.UISystem.Screens
{
    /// <summary>
    /// Login screen UI - handles button clicks and status display.
    /// All authentication logic is handled by AuthenticationManager.
    /// </summary>
    [UIScreen(UIScreenType.Login, UIScreenPriority.Login, "LoginScreenTemplate")]
    public class LoginScreen : BaseUIScreen
    {
        #region UI Elements

        private Button _facebookLoginButton;
        private Button _guestLoginButton;
        private Label _statusText;
        private VisualElement _loadingIndicator;

        #endregion

        #region State

        private bool _isLoggingIn;

        #endregion

        #region Events

        public event Action OnLoginSuccess;
        public event Action<string> OnLoginFailed;

        #endregion

        #region Lifecycle

        protected override void OnInitialize()
        {
            CacheUIElements();
            SetupButtonHandlers();

            Debug.Log("[LoginScreen] Initialized");
        }

        protected override void OnShow()
        {
            _isLoggingIn = false;

            // Setup AuthenticationManager connection when screen is shown
            // This handles timing issues where AuthenticationManager might not exist during OnInitialize
            SetupAuthenticationManager();

            UpdateUI();
            UpdateStatus("Please select a login method");
        }

        protected override void OnHide()
        {
            _isLoggingIn = false;
        }

        public override void Update(float deltaTime)
        {
            // Update UI based on login state
        }

        protected override void OnDispose()
        {
            // Clean up button handlers
            if (_facebookLoginButton != null)
            {
                _facebookLoginButton.clicked -= OnFacebookLoginClicked;
            }

            if (_guestLoginButton != null)
            {
                _guestLoginButton.clicked -= OnGuestLoginClicked;
            }

            // Unsubscribe from authentication events
            if (GameBootstrap.Services.AuthenticationService != null)
            {
                GameBootstrap.Services.AuthenticationService.OnLoginSuccess -= HandleLoginSuccess;
                GameBootstrap.Services.AuthenticationService.OnLoginFailed -= HandleLoginFailed;
                GameBootstrap.Services.AuthenticationService.OnLoginStatusChanged -= UpdateStatus;
            }
        }

        #endregion

        #region UI Setup

        private void CacheUIElements()
        {
            _facebookLoginButton = GetElement<Button>("facebook-login-button");
            _guestLoginButton = GetElement<Button>("guest-login-button");
            _statusText = GetElement<Label>("status-text");
            _loadingIndicator = GetElement<VisualElement>("loading-indicator");

            // Log missing elements for debugging
            if (_facebookLoginButton == null)
            {
                Debug.LogWarning("[LoginScreen] facebook-login-button not found");
            }
            if (_guestLoginButton == null)
            {
                Debug.LogWarning("[LoginScreen] guest-login-button not found");
            }
            if (_statusText == null)
            {
                Debug.LogWarning("[LoginScreen] status-text not found");
            }
            if (_loadingIndicator == null)
            {
                Debug.LogWarning("[LoginScreen] loading-indicator not found");
            }
        }

        private void SetupButtonHandlers()
        {
            if (_facebookLoginButton != null)
            {
                _facebookLoginButton.clicked += OnFacebookLoginClicked;
            }

            if (_guestLoginButton != null)
            {
                _guestLoginButton.clicked += OnGuestLoginClicked;
            }
        }

        private void SetupAuthenticationManager()
        {
            if (GameBootstrap.Services.AuthenticationService == null)
            {
                Debug.LogWarning("[LoginScreen] AuthenticationManager not available yet, will retry when needed");
                return;
            }

            // Unsubscribe first to avoid duplicate subscriptions
            GameBootstrap.Services.AuthenticationService.OnLoginSuccess -= HandleLoginSuccess;
            GameBootstrap.Services.AuthenticationService.OnLoginFailed -= HandleLoginFailed;
            GameBootstrap.Services.AuthenticationService.OnLoginStatusChanged -= UpdateStatus;

            // Subscribe to authentication events
            GameBootstrap.Services.AuthenticationService.OnLoginSuccess += HandleLoginSuccess;
            GameBootstrap.Services.AuthenticationService.OnLoginFailed += HandleLoginFailed;
            GameBootstrap.Services.AuthenticationService.OnLoginStatusChanged += UpdateStatus;

            Debug.Log("[LoginScreen] Connected to AuthenticationManager");
        }

        private void UpdateUI()
        {
            bool buttonsEnabled = !_isLoggingIn;

            if (_facebookLoginButton != null)
            {
                _facebookLoginButton.SetEnabled(buttonsEnabled);
            }

            if (_guestLoginButton != null)
            {
                _guestLoginButton.SetEnabled(buttonsEnabled);
            }

            if (_loadingIndicator != null)
            {
                _loadingIndicator.style.display = _isLoggingIn ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void UpdateStatus(string message)
        {
            if (_statusText != null)
            {
                _statusText.text = message;
            }

            Debug.Log($"[LoginScreen] {message}");
        }

        #endregion



        #region Button Handlers

        private void OnFacebookLoginClicked()
        {
            if (_isLoggingIn)
            {
                return;
            }

            Debug.Log("[LoginScreen] Facebook login clicked");

            _isLoggingIn = true;
            UpdateUI();

            // Start coroutine to handle async AuthenticationManager check
            CoroutineRunner.Run(HandleFacebookLogin());
        }

        private void OnGuestLoginClicked()
        {
            if (_isLoggingIn)
            {
                return;
            }

            Debug.Log("[LoginScreen] Guest login clicked");

            _isLoggingIn = true;
            UpdateUI();

            // Start coroutine to handle async AuthenticationManager check
            CoroutineRunner.Run(HandleGuestLogin());
        }

        private IEnumerator HandleFacebookLogin()
        {
            // Wait for AuthenticationManager if not available
            if (GameBootstrap.Services.AuthenticationService == null)
            {
                UpdateStatus("Initializing authentication...");

                float timeout = 5f;
                float elapsed = 0f;

                while (GameBootstrap.Services.AuthenticationService == null && elapsed < timeout)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                if (GameBootstrap.Services.AuthenticationService == null)
                {
                    Debug.LogError("[LoginScreen] AuthenticationManager not available!");
                    UpdateStatus("Authentication system not available");
                    _isLoggingIn = false;
                    UpdateUI();
                    yield break;
                }

                // Ensure we're connected
                SetupAuthenticationManager();
            }

            // Delegate to AuthenticationManager
            yield return GameBootstrap.Services.AuthenticationService.LoginWithFacebook();
        }

        private IEnumerator HandleGuestLogin()
        {
            // Wait for AuthenticationManager if not available
            if (GameBootstrap.Services.AuthenticationService == null)
            {
                UpdateStatus("Initializing authentication...");

                float timeout = 5f;
                float elapsed = 0f;

                while (GameBootstrap.Services.AuthenticationService == null && elapsed < timeout)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                if (GameBootstrap.Services.AuthenticationService == null)
                {
                    Debug.LogError("[LoginScreen] AuthenticationManager not available!");
                    UpdateStatus("Authentication system not available");
                    _isLoggingIn = false;
                    UpdateUI();
                    yield break;
                }

                // Ensure we're connected
                SetupAuthenticationManager();
            }

            // Delegate to AuthenticationManager
            yield return GameBootstrap.Services.AuthenticationService.LoginAsGuest();
        }

        #endregion

        #region Event Handlers

        private void HandleLoginSuccess()
        {
            _isLoggingIn = false;
            UpdateUI();

            Debug.Log("[LoginScreen] Login successful from AuthenticationManager");

            // Notify listeners
            OnLoginSuccess?.Invoke();

            // Hide login screen after a short delay
            CoroutineRunner.Run(HideAfterDelay(1f));
        }

        private void HandleLoginFailed(string error)
        {
            _isLoggingIn = false;
            UpdateUI();

            Debug.LogWarning($"[LoginScreen] Login failed from AuthenticationManager: {error}");

            // Notify listeners
            OnLoginFailed?.Invoke(error);
        }

        private IEnumerator HideAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            Hide(true);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Check if currently logging in
        /// </summary>
        public bool IsLoggingIn() => _isLoggingIn;

        /// <summary>
        /// Manually trigger guest login
        /// </summary>
        public void TriggerGuestLogin()
        {
            OnGuestLoginClicked();
        }

        #endregion
    }
}
