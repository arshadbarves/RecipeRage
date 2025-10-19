using System;
using Core.Animation;
using Core.Authentication;
using Core.Bootstrap;
using Core.Events;
using Core.Utilities;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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
        private VisualElement _loginCard;

        #endregion

        #region State

        private bool _isLoggingIn;

        #endregion

        #region Events
        // Events removed - using EventBus instead
        #endregion

        #region Lifecycle

        protected override void OnInitialize()
        {
            CacheUIElements();
            SetupButtonHandlers();

            _loginCard.style.translate = new Translate(new Length(100, LengthUnit.Percent), 0);

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

            // Slide in from right using translate
            if (_loginCard != null)
            {
                SlideInFromRight(_loginCard, 0.5f);
            }
        }

        protected override void OnHide()
        {
            _isLoggingIn = false;

            // Slide out to right using translate
            if (_loginCard != null)
            {
                SlideOutToRight(_loginCard, 0.4f);
            }
        }

        private void SlideInFromRight(VisualElement element, float duration)
        {
            // Start off-screen to the right (100% translate)
            float startX = 100f;
            float endX = 0f;

            element.style.translate = new Translate(new Length(startX, LengthUnit.Percent), 0);

            // Animate translate from 100% to 0%
            DOTween.To(
                () => startX,
                x => element.style.translate = new Translate(new Length(x, LengthUnit.Percent), 0),
                endX,
                duration
            ).SetEase(Ease.OutCubic);
        }

        private void SlideOutToRight(VisualElement element, float duration)
        {
            // Animate translate from 0% to 100%
            float startX = 0f;
            float endX = 100f;

            DOTween.To(
                () => startX,
                x => element.style.translate = new Translate(new Length(x, LengthUnit.Percent), 0),
                endX,
                duration
            ).SetEase(Ease.InCubic);
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

            // Unsubscribe from EventBus
            var eventBus = GameBootstrap.Services?.EventBus;
            if (eventBus != null)
            {
                eventBus.Unsubscribe<LoginSuccessEvent>(HandleLoginSuccess);
                eventBus.Unsubscribe<LoginFailedEvent>(HandleLoginFailed);
                eventBus.Unsubscribe<LoginStatusChangedEvent>(HandleStatusChanged);
            }
        }

        #endregion

        #region UI Setup

        private void CacheUIElements()
        {
            _loginCard = GetElement<VisualElement>("login-card");
            _facebookLoginButton = GetElement<Button>("facebook-login-button");
            _guestLoginButton = GetElement<Button>("guest-login-button");
            _statusText = GetElement<Label>("status-text");
            _loadingIndicator = GetElement<VisualElement>("loading-indicator");

            // Log missing elements for debugging
            if (_loginCard == null)
            {
                Debug.LogWarning("[LoginScreen] login-card not found");
            }
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

            // Subscribe to EventBus events
            var eventBus = GameBootstrap.Services.EventBus;
            if (eventBus != null)
            {
                eventBus.Subscribe<LoginSuccessEvent>(HandleLoginSuccess);
                eventBus.Subscribe<LoginFailedEvent>(HandleLoginFailed);
                eventBus.Subscribe<LoginStatusChangedEvent>(HandleStatusChanged);
            }

            Debug.Log("[LoginScreen] Connected to EventBus");
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

            // Start async login
            HandleFacebookLoginAsync().Forget();
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

            // Start async login
            HandleGuestLoginAsync().Forget();
        }

        private async UniTaskVoid HandleFacebookLoginAsync()
        {
            try
            {
                // Wait for AuthenticationService if not available
                if (GameBootstrap.Services.AuthenticationService == null)
                {
                    UpdateStatus("Initializing authentication...");

                    var timeout = TimeSpan.FromSeconds(5);
                    var startTime = Time.time;

                    while (GameBootstrap.Services.AuthenticationService == null)
                    {
                        if (Time.time - startTime > timeout.TotalSeconds)
                        {
                            Debug.LogError("[LoginScreen] AuthenticationService not available!");
                            UpdateStatus("Authentication system not available");
                            _isLoggingIn = false;
                            UpdateUI();
                            return;
                        }

                        await UniTask.Yield();
                    }

                    // Ensure we're connected
                    SetupAuthenticationManager();
                }

                // Delegate to AuthenticationService
                await GameBootstrap.Services.AuthenticationService.LoginWithFacebookAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LoginScreen] Facebook login error: {ex.Message}");
                UpdateStatus("Login failed");
                _isLoggingIn = false;
                UpdateUI();
            }
        }

        private async UniTaskVoid HandleGuestLoginAsync()
        {
            try
            {
                // Wait for AuthenticationService if not available
                if (GameBootstrap.Services.AuthenticationService == null)
                {
                    UpdateStatus("Initializing authentication...");

                    var timeout = TimeSpan.FromSeconds(5);
                    var startTime = Time.time;

                    while (GameBootstrap.Services.AuthenticationService == null)
                    {
                        if (Time.time - startTime > timeout.TotalSeconds)
                        {
                            Debug.LogError("[LoginScreen] AuthenticationService not available!");
                            UpdateStatus("Authentication system not available");
                            _isLoggingIn = false;
                            UpdateUI();
                            return;
                        }

                        await UniTask.Yield();
                    }

                    // Ensure we're connected
                    SetupAuthenticationManager();
                }

                // Delegate to AuthenticationService
                await GameBootstrap.Services.AuthenticationService.LoginAsGuestAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LoginScreen] Guest login error: {ex.Message}");
                UpdateStatus("Login failed");
                _isLoggingIn = false;
                UpdateUI();
            }
        }

        #endregion

        #region Event Handlers

        private void HandleLoginSuccess(LoginSuccessEvent evt)
        {
            _isLoggingIn = false;
            UpdateUI();

            Debug.Log($"[LoginScreen] Login successful for user: {evt.UserId}");

            // Hide login screen after a short delay
            HideAfterDelayAsync(1f).Forget();
        }

        private void HandleLoginFailed(LoginFailedEvent evt)
        {
            _isLoggingIn = false;
            UpdateUI();

            Debug.LogWarning($"[LoginScreen] Login failed: {evt.Error}");
        }

        private void HandleStatusChanged(LoginStatusChangedEvent evt)
        {
            UpdateStatus(evt.Status);
        }

        private async UniTaskVoid HideAfterDelayAsync(float delay)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delay));
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
