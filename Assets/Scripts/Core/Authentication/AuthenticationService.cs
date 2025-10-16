using System;
using System.Collections;
using Core.SaveSystem;
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

        public IEnumerator AttemptAutoLogin()
        {
            UpdateStatus("Checking for existing session...");

            if (IsUserLoggedIn())
            {
                UpdateStatus("Session found!");
                yield return new WaitForSeconds(0.5f);
                OnLoginSuccess?.Invoke();
                yield break;
            }

            string lastLoginMethod = _saveService?.GetSettings().LastLoginMethod ?? "";

            if (string.IsNullOrEmpty(lastLoginMethod))
            {
                UpdateStatus("No previous session found");
                yield break;
            }

            Debug.Log($"[AuthenticationService] Attempting auto-login with method: {lastLoginMethod}");

            switch (lastLoginMethod)
            {
                case LOGIN_METHOD_DEVICE_ID:
                    yield return LoginWithDeviceIdInternal(true);
                    break;
                case LOGIN_METHOD_FACEBOOK:
                    UpdateStatus("Facebook auto-login requires Facebook SDK");
                    break;
                default:
                    UpdateStatus("Unknown login method");
                    break;
            }
        }

        public IEnumerator LoginAsGuest()
        {
            Debug.Log("[AuthenticationService] Guest login requested");
            UpdateStatus("Logging in as guest...");
            yield return LoginWithDeviceIdInternal(false);
        }

        public IEnumerator LoginWithFacebook()
        {
            Debug.Log("[AuthenticationService] Facebook login requested");
            UpdateStatus("Facebook login requires Facebook SDK integration");
            yield return new WaitForSeconds(1f);
            OnLoginFailed?.Invoke("Facebook SDK not integrated");
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

        private IEnumerator LoginWithDeviceIdInternal(bool isAutoLogin)
        {
            bool createCompleted = false;
            bool createSuccess = false;
            string errorMessage = "";

            if (!isAutoLogin)
            {
                UpdateStatus("Setting up guest account...");

                ConnectInterface connectInterface = EOSManager.Instance.GetEOSConnectInterface();
                var createOptions = new CreateDeviceIdOptions()
                {
                    DeviceModel = SystemInfo.deviceModel
                };

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

                float timeout = 10f;
                float elapsed = 0f;

                while (!createCompleted && elapsed < timeout)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                if (!createSuccess)
                {
                    UpdateStatus(string.IsNullOrEmpty(errorMessage) ? "Failed to create guest account" : errorMessage);
                    OnLoginFailed?.Invoke(errorMessage);
                    yield break;
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

            float loginTimeout = 10f;
            float loginElapsed = 0f;

            while (!loginCompleted && loginElapsed < loginTimeout)
            {
                loginElapsed += Time.deltaTime;
                yield return null;
            }

            if (loginSuccess)
            {
                // Save login method using SaveService (secure and consistent)
                _saveService?.UpdateSettings(s => s.LastLoginMethod = LOGIN_METHOD_DEVICE_ID);

                UpdateStatus("Login successful!");
                OnLoginSuccess?.Invoke();
            }
            else
            {
                if (string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = "Login timed out";
                }

                UpdateStatus(errorMessage);
                OnLoginFailed?.Invoke(errorMessage);
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
