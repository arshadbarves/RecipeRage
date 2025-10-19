using System;
using Core.SaveSystem;
using Cysharp.Threading.Tasks;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using PlayEveryWare.EpicOnlineServices;
using UnityEngine;

namespace Core.Authentication
{
    /// <summary>
    /// Authentication service - uses SaveService for persistence (SOLID compliant)
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private const string LOGIN_METHOD_DEVICE_ID = "DeviceID";
        private const string LOGIN_METHOD_FACEBOOK = "Facebook";
        
        private readonly ISaveService _saveService;

        public event Action OnLoginSuccess;
        public event Action<string> OnLoginFailed;
        public event Action<string> OnLoginStatusChanged;

        public bool IsLoggedIn => IsUserLoggedIn();
        public string LastLoginMethod => _saveService?.GetSettings().LastLoginMethod ?? "";

        public AuthenticationService(ISaveService saveService)
        {
            _saveService = saveService;
        }

        public async UniTask<bool> AttemptAutoLoginAsync()
        {
            UpdateStatus("Checking for existing session...");

            if (IsUserLoggedIn())
            {
                UpdateStatus("Session found!");
                await UniTask.Delay(500);
                OnLoginSuccess?.Invoke();
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
            OnLoginFailed?.Invoke("Facebook SDK not integrated");
            return false;
        }

        public void Logout()
        {
            Debug.Log("[AuthenticationService] Logging out user");
            
            // Clear login method from save service
            _saveService?.UpdateSettings(s => s.LastLoginMethod = "");

            ProductUserId productUserId = EOSManager.Instance?.GetProductUserId();
            if (productUserId != null && productUserId.IsValid())
            {
                EOSManager.Instance.ClearConnectId(productUserId);
            }

            UpdateStatus("Logged out");
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
                    OnLoginFailed?.Invoke(errorMessage);
                    return false;
                }

                if (!createSuccess)
                {
                    UpdateStatus(string.IsNullOrEmpty(errorMessage) ? "Failed to create guest account" : errorMessage);
                    OnLoginFailed?.Invoke(errorMessage);
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
                errorMessage = "Login timed out";
                UpdateStatus(errorMessage);
                OnLoginFailed?.Invoke(errorMessage);
                return false;
            }

            if (loginSuccess)
            {
                // Save login method using SaveService (secure and consistent)
                _saveService?.UpdateSettings(s => s.LastLoginMethod = LOGIN_METHOD_DEVICE_ID);

                UpdateStatus("Login successful!");
                OnLoginSuccess?.Invoke();
                return true;
            }
            else
            {
                if (string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = "Login failed";
                }

                UpdateStatus(errorMessage);
                OnLoginFailed?.Invoke(errorMessage);
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
            OnLoginStatusChanged?.Invoke(message);
        }
    }
}
