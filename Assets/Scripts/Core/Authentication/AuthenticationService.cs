using System;
using Core.Bootstrap;
using Core.Logging;
using Core.SaveSystem;
using Cysharp.Threading.Tasks;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using PlayEveryWare.EpicOnlineServices;
using UnityEngine;

namespace Core.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private const string LOGIN_METHOD_DEVICE_ID = "DeviceID";
        private const string LOGIN_METHOD_FACEBOOK = "Facebook";

        private readonly ISaveService _saveService;
        private readonly Core.Events.IEventBus _eventBus;

        public bool IsLoggedIn => IsUserLoggedIn();
        public string LastLoginMethod => _saveService?.GetSettings().LastLoginMethod ?? "";

        public AuthenticationService(
            ISaveService saveService,
            Core.Events.IEventBus eventBus)
        {
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public async UniTask<bool> InitializeAsync()
        {
            GameLogger.Log("Initializing authentication...");

            // Try auto-login
            bool loginSuccess = await AttemptAutoLoginAsync();

            if (loginSuccess)
            {
                GameLogger.Log("Auto-login successful");

                _saveService.OnUserLoggedIn();

                return true;
            }
            else
            {
                GameLogger.Log("Auto-login failed or no session");
                return false;
            }
        }

        public async UniTask<bool> AttemptAutoLoginAsync()
        {
            UpdateStatus("Checking for existing session...");

            if (IsUserLoggedIn())
            {
                UpdateStatus("Session found!");
                await UniTask.Delay(500);

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

            GameLogger.Log($"Attempting auto-login with method: {lastLoginMethod}");

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
            GameLogger.Log("Guest login requested");
            UpdateStatus("Logging in as guest...");
            return await LoginWithDeviceIdInternalAsync(false);
        }

        public async UniTask<bool> LoginWithFacebookAsync()
        {
            GameLogger.Log("Facebook login requested");
            UpdateStatus("Facebook login requires Facebook SDK integration");
            await UniTask.Delay(1000);

            _eventBus?.Publish(new Core.Events.LoginFailedEvent
            {
                Error = "Facebook SDK not integrated"
            });

            return false;
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
                        GameLogger.Log("Device ID ready");
                    }
                    else
                    {
                        errorMessage = $"Failed to create Device ID: {createInfo.ResultCode}";
                        GameLogger.LogError($"{errorMessage}");
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
                        GameLogger.Log("Device ID login successful");
                    }
                    else
                    {
                        errorMessage = $"Login failed: {callbackInfo.ResultCode}";
                        GameLogger.LogError($"{errorMessage}");
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

                _eventBus?.Publish(new Core.Events.LoginFailedEvent
                {
                    Error = errorMessage
                });

                GameBootstrap.Services?.MaintenanceService?.ShowServerDownMaintenance(errorMessage);

                return false;
            }

            if (loginSuccess)
            {
                _saveService?.UpdateSettings(s => s.LastLoginMethod = LOGIN_METHOD_DEVICE_ID);

                UpdateStatus("Login successful!");

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
            GameLogger.Log($"{message}");

            _eventBus?.Publish(new Core.Events.LoginStatusChangedEvent
            {
                Status = message
            });
        }

        // ============================================
        // LOGOUT SUPPORT
        // ============================================
        public async UniTask LogoutAsync()
        {
            GameLogger.Log("Logging out...");
            UpdateStatus("Logging out...");

            try
            {
                // Clear saved login method
                var settings = _saveService.GetSettings();
                settings.LastLoginMethod = "";
                _saveService.SaveSettings(settings);

                _saveService.OnUserLoggedOut();

                // EOS logout (if available)
                if (EOSManager.Instance != null)
                {
                    ProductUserId productUserId = EOSManager.Instance.GetProductUserId();
                    if (productUserId != null && productUserId.IsValid())
                    {
                        EOSManager.Instance.ClearConnectId(productUserId);
                    }
                    GameLogger.Log("EOS session cleared");
                }

                await UniTask.Delay(500);

                UpdateStatus("Logged out successfully");

                _eventBus?.Publish(new Core.Events.LogoutEvent
                {
                    UserId = EOSManager.Instance?.GetProductUserId()?.ToString() ?? "unknown"
                });

                GameLogger.Log("Logout complete");
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Logout failed: {ex.Message}");
                UpdateStatus($"Logout failed: {ex.Message}");
            }
        }
    }
}
