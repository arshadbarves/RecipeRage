using System;
using Core.Bootstrap;
using Core.SaveSystem;
using Cysharp.Threading.Tasks;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using PlayEveryWare.EpicOnlineServices;
using UI;
using UI.Screens;
using UnityEngine;

namespace Core.Authentication
{
    /// <summary>
    /// Authentication service - handles all auth logic and UI flow
    /// Uses SaveService for persistence (SOLID compliant)
    /// Publishes events via EventBus for decoupled communication
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private const string LOGIN_METHOD_DEVICE_ID = "DeviceID";
        private const string LOGIN_METHOD_FACEBOOK = "Facebook";

        private readonly ISaveService _saveService;
        private readonly Core.Events.IEventBus _eventBus;
        private readonly UI.IUIService _uiService;

        public bool IsLoggedIn => IsUserLoggedIn();
        public string LastLoginMethod => _saveService?.GetSettings().LastLoginMethod ?? "";

        /// <summary>
        /// Constructor with proper dependency injection
        /// </summary>
        public AuthenticationService(
            ISaveService saveService,
            Core.Events.IEventBus eventBus,
            UI.IUIService uiService)
        {
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _uiService = uiService ?? throw new ArgumentNullException(nameof(uiService));
        }

        /// <summary>
        /// Initialize authentication - checks for existing session and shows appropriate UI
        /// Returns true if user is authenticated, false if login screen is shown
        /// </summary>
        public async UniTask<bool> InitializeAsync()
        {
            Debug.Log("[AuthenticationService] Initializing authentication...");

            // Try auto-login
            bool loginSuccess = await AttemptAutoLoginAsync();

            if (loginSuccess)
            {
                Debug.Log("[AuthenticationService] Auto-login successful");

                // Notify save service that user logged in
                _saveService.OnUserLoggedIn();

                return true;
            }
            else
            {
                Debug.Log("[AuthenticationService] Auto-login failed - showing login screen");
                ShowLoginScreen();
                return false;
            }
        }

        /// <summary>
        /// Show login screen and subscribe to login events
        /// </summary>
        private void ShowLoginScreen()
        {
            if (_uiService != null)
            {
                _uiService.ShowScreen(UIScreenType.Login);

                // Subscribe to login success via EventBus
                _eventBus?.Subscribe<Core.Events.LoginSuccessEvent>(OnLoginSuccessInternal);
            }
            else
            {
                Debug.LogError("[AuthenticationService] UIService not available to show login screen");
            }
        }

        /// <summary>
        /// Internal handler for login success event
        /// </summary>
        private void OnLoginSuccessInternal(Core.Events.LoginSuccessEvent evt)
        {
            // Unsubscribe
            _eventBus?.Unsubscribe<Core.Events.LoginSuccessEvent>(OnLoginSuccessInternal);

            Debug.Log($"[AuthenticationService] Login successful for user: {evt.UserId}");

            // Notify save service that user logged in
            _saveService.OnUserLoggedIn();
        }

        public async UniTask<bool> AttemptAutoLoginAsync()
        {
            UpdateStatus("Checking for existing session...");

            if (IsUserLoggedIn())
            {
                UpdateStatus("Session found!");
                await UniTask.Delay(500);

                // Publish event via EventBus
                _eventBus?.Publish(new Core.Events.LoginSuccessEvent
                {
                    UserId = EOSManager.Instance?.GetProductUserId()?.ToString() ?? "unknown",
                    DisplayName = "User"
                });

                return true;
            }

            string lastLoginMethod = _saveService?.GetSettings().LastLoginMethod ?? "";

            if (string.IsNullOrEmpty(lastLoginMethod))
            {
                UpdateStatus("No previous session found");
                return false;
            }

            Debug.Log($"[AuthenticationService] Attempting auto-login with method: {lastLoginMethod}");

            switch (lastLoginMethod)
            {
                case LOGIN_METHOD_DEVICE_ID:
                    return await LoginWithDeviceIdInternalAsync(true);
                case LOGIN_METHOD_FACEBOOK:
                    UpdateStatus("Facebook auto-login requires Facebook SDK");
                    return false;
                default:
                    UpdateStatus("Unknown login method");
                    return false;
            }
        }

        public async UniTask<bool> LoginAsGuestAsync()
        {
            Debug.Log("[AuthenticationService] Guest login requested");
            UpdateStatus("Logging in as guest...");
            return await LoginWithDeviceIdInternalAsync(false);
        }

        public async UniTask<bool> LoginWithFacebookAsync()
        {
            Debug.Log("[AuthenticationService] Facebook login requested");
            UpdateStatus("Facebook login requires Facebook SDK integration");
            await UniTask.Delay(1000);

            _eventBus?.Publish(new Core.Events.LoginFailedEvent
            {
                Error = "Facebook SDK not integrated"
            });

            return false;
        }

        public void Logout()
        {
            Debug.Log("[AuthenticationService] Logging out user (sync)");

            // Clear login method from save service
            _saveService?.UpdateSettings(s => s.LastLoginMethod = "");

            // Notify save service to clear user-specific cache
            _saveService?.OnUserLoggedOut();

            // Clear all UI screens and history
            if (_uiService != null)
            {
                Debug.Log("[AuthenticationService] Clearing all UI screens and history");
                _uiService.HideAllScreens(animate: false);
                _uiService.ClearHistory();
            }

            ProductUserId productUserId = EOSManager.Instance?.GetProductUserId();
            if (productUserId != null && productUserId.IsValid())
            {
                EOSManager.Instance.ClearConnectId(productUserId);
            }

            UpdateStatus("Logged out");

            // Trigger logout complete event - GameBootstrap will handle full reboot
            OnLogoutComplete?.Invoke();
        }

        private async UniTask<bool> LoginWithDeviceIdInternalAsync(bool isAutoLogin)
        {
            string errorMessage = "";

            if (!isAutoLogin)
            {
                UpdateStatus("Setting up guest account...");

                ConnectInterface connectInterface = EOSManager.Instance.GetEOSConnectInterface();
                var createOptions = new CreateDeviceIdOptions()
                {
                    DeviceModel = SystemInfo.deviceModel
                };

                bool createCompleted = false;
                bool createSuccess = false;

                connectInterface.CreateDeviceId(ref createOptions, null, (ref CreateDeviceIdCallbackInfo createInfo) =>
                {
                    createCompleted = true;

                    if (createInfo.ResultCode == Result.Success || createInfo.ResultCode == Result.DuplicateNotAllowed)
                    {
                        createSuccess = true;
                        Debug.Log("[AuthenticationService] Device ID ready");
                    }
                    else
                    {
                        errorMessage = $"Failed to create Device ID: {createInfo.ResultCode}";
                        Debug.LogError($"[AuthenticationService] {errorMessage}");
                    }
                });

                // Wait for completion with timeout
                var timeoutTask = UniTask.Delay(TimeSpan.FromSeconds(10));
                var waitTask = UniTask.WaitUntil(() => createCompleted);

                var completedTask = await UniTask.WhenAny(waitTask, timeoutTask);

                if (completedTask == 1) // Timeout
                {
                    errorMessage = "Device ID creation timed out";
                    UpdateStatus(errorMessage);

                    _eventBus?.Publish(new Core.Events.LoginFailedEvent
                    {
                        Error = errorMessage
                    });

                    // Show maintenance screen for server down scenario
                    GameBootstrap.Services?.MaintenanceService?.ShowServerDownMaintenance(errorMessage);

                    return false;
                }

                if (!createSuccess)
                {
                    UpdateStatus(string.IsNullOrEmpty(errorMessage) ? "Failed to create guest account" : errorMessage);

                    _eventBus?.Publish(new Core.Events.LoginFailedEvent
                    {
                        Error = errorMessage
                    });

                    return false;
                }
            }

            UpdateStatus(isAutoLogin ? "Restoring guest session..." : "Logging in as guest...");

            bool loginCompleted = false;
            bool loginSuccess = false;
            string displayName = $"Guest_{SystemInfo.deviceUniqueIdentifier.Substring(0, 8)}";

            EOSManager.Instance.StartConnectLoginWithOptions(
                ExternalCredentialType.DeviceidAccessToken,
                null,
                displayName,
                (LoginCallbackInfo callbackInfo) =>
                {
                    loginCompleted = true;

                    if (callbackInfo.ResultCode == Result.Success)
                    {
                        loginSuccess = true;
                        Debug.Log("[AuthenticationService] Device ID login successful");
                    }
                    else
                    {
                        errorMessage = $"Login failed: {callbackInfo.ResultCode}";
                        Debug.LogError($"[AuthenticationService] {errorMessage}");
                    }
                }
            );

            // Wait for login completion with timeout
            var loginTimeoutTask = UniTask.Delay(TimeSpan.FromSeconds(10));
            var loginWaitTask = UniTask.WaitUntil(() => loginCompleted);

            var loginCompletedTask = await UniTask.WhenAny(loginWaitTask, loginTimeoutTask);

            if (loginCompletedTask == 1) // Timeout
            {
                errorMessage = "Login timeout - server not responding";
                UpdateStatus(errorMessage);

                // Show error toast
                _ = _uiService?.ShowToast(errorMessage, ToastType.Error, 4f);

                _eventBus?.Publish(new Core.Events.LoginFailedEvent
                {
                    Error = errorMessage
                });

                // Show maintenance screen for server down scenario
                GameBootstrap.Services?.MaintenanceService?.ShowServerDownMaintenance(errorMessage);

                return false;
            }

            if (loginSuccess)
            {
                // Save login method using SaveService (secure and consistent)
                _saveService?.UpdateSettings(s => s.LastLoginMethod = LOGIN_METHOD_DEVICE_ID);

                UpdateStatus("Login successful!");

                // Publish event via EventBus
                _eventBus?.Publish(new Core.Events.LoginSuccessEvent
                {
                    UserId = EOSManager.Instance?.GetProductUserId()?.ToString() ?? "unknown",
                    DisplayName = "Guest"
                });

                return true;
            }
            else
            {
                if (string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = "Login failed";
                }

                UpdateStatus(errorMessage);

                // Publish event via EventBus
                _eventBus?.Publish(new Core.Events.LoginFailedEvent
                {
                    Error = errorMessage
                });

                return false;
            }
        }

        private bool IsUserLoggedIn()
        {
            if (EOSManager.Instance == null) return false;

            ProductUserId productUserId = EOSManager.Instance.GetProductUserId();
            return productUserId != null && productUserId.IsValid();
        }

        private void UpdateStatus(string message)
        {
            Debug.Log($"[AuthenticationService] {message}");

            // Publish event via EventBus
            _eventBus?.Publish(new Core.Events.LoginStatusChangedEvent
            {
                Status = message
            });
        }

        // ============================================
        // LOGOUT SUPPORT
        // ============================================

        public event Action OnLogoutComplete;

        public async UniTask LogoutAsync()
        {
            Debug.Log("[AuthenticationService] Logging out...");
            UpdateStatus("Logging out...");

            try
            {
                // Clear saved login method
                var settings = _saveService.GetSettings();
                settings.LastLoginMethod = "";
                _saveService.SaveSettings(settings);

                // Notify save service to clear user-specific cache
                _saveService.OnUserLoggedOut();

                // Clear all UI screens and history
                if (_uiService != null)
                {
                    Debug.Log("[AuthenticationService] Clearing all UI screens and history");
                    _uiService.HideAllScreens(animate: false);
                    _uiService.ClearHistory();
                }

                // EOS logout (if available)
                if (EOSManager.Instance != null)
                {
                    ProductUserId productUserId = EOSManager.Instance.GetProductUserId();
                    if (productUserId != null && productUserId.IsValid())
                    {
                        EOSManager.Instance.ClearConnectId(productUserId);
                    }
                    Debug.Log("[AuthenticationService] EOS session cleared");
                }

                await UniTask.Delay(500);

                UpdateStatus("Logged out successfully");

                // Show success toast
                _ = _uiService?.ShowToast("Logged out successfully", ToastType.Success, 2f);

                // Trigger logout complete event - GameBootstrap will handle full reboot
                OnLogoutComplete?.Invoke();

                Debug.Log("[AuthenticationService] Logout complete");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AuthenticationService] Logout failed: {ex.Message}");
                UpdateStatus($"Logout failed: {ex.Message}");

                // Show error toast
                _ = _uiService?.ShowToast("Logout failed", ToastType.Error, 3f);
            }
        }
    }
}
