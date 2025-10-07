using System;
using System.Collections;
using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Connect;
using PlayEveryWare.EpicOnlineServices;
using UnityEngine;

namespace Core.Authentication
{
    /// <summary>
    /// Manages all authentication logic for EOS (Epic Online Services).
    /// Handles Device ID guest login, Facebook login, and persistent sessions.
    /// </summary>
    public class AuthenticationManager : MonoBehaviour
    {
        #region Singleton

        public static AuthenticationManager Instance { get; private set; }

        #endregion

        #region Constants

        private const string PREF_LAST_LOGIN_METHOD = "EOS_LastLoginMethod";
        private const string LOGIN_METHOD_DEVICE_ID = "DeviceID";
        private const string LOGIN_METHOD_FACEBOOK = "Facebook";

        #endregion

        #region Events

        public event Action OnLoginSuccess;
        public event Action<string> OnLoginFailed;
        public event Action<string> OnLoginStatusChanged;

        #endregion

        #region Properties

        public bool IsLoggedIn => IsUserLoggedIn();
        public string LastLoginMethod => PlayerPrefs.GetString(PREF_LAST_LOGIN_METHOD, "");

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Attempt to login automatically using saved login method.
        /// </summary>
        public IEnumerator AttemptAutoLogin()
        {
            UpdateStatus("Checking for existing session...");

            // Wait for EOSManager to initialize
            while (EOSManager.Instance == null)
            {
                yield return new WaitForSeconds(0.1f);
            }

            // Check if user is already logged in
            if (IsUserLoggedIn())
            {
                UpdateStatus("Session found!");
                yield return new WaitForSeconds(0.5f);
                OnLoginSuccess?.Invoke();
                yield break;
            }

            // Try to auto-login using the last login method
            string lastLoginMethod = PlayerPrefs.GetString(PREF_LAST_LOGIN_METHOD, "");

            if (string.IsNullOrEmpty(lastLoginMethod))
            {
                UpdateStatus("No previous session found");
                yield break;
            }

            Debug.Log($"[AuthenticationManager] Attempting auto-login with method: {lastLoginMethod}");

            switch (lastLoginMethod)
            {
                case LOGIN_METHOD_DEVICE_ID:
                    yield return LoginWithDeviceIdInternal(true);
                    break;

                case LOGIN_METHOD_FACEBOOK:
                    UpdateStatus("Facebook auto-login requires Facebook SDK");
                    // Facebook auto-login would go here when SDK is integrated
                    break;

                default:
                    UpdateStatus("Unknown login method");
                    break;
            }
        }

        /// <summary>
        /// Login as guest using Device ID.
        /// </summary>
        public IEnumerator LoginAsGuest()
        {
            Debug.Log("[AuthenticationManager] Guest login requested");
            UpdateStatus("Logging in as guest...");

            yield return LoginWithDeviceIdInternal(false);
        }

        /// <summary>
        /// Login with Facebook (requires Facebook SDK integration).
        /// </summary>
        public IEnumerator LoginWithFacebook()
        {
            Debug.Log("[AuthenticationManager] Facebook login requested");
            UpdateStatus("Facebook login requires Facebook SDK integration");

            // TODO: Implement Facebook login when SDK is integrated
            yield return new WaitForSeconds(1f);

            OnLoginFailed?.Invoke("Facebook SDK not integrated");
        }

        /// <summary>
        /// Logout the current user.
        /// </summary>
        public void Logout()
        {
            Debug.Log("[AuthenticationManager] Logging out user");

            // Clear saved login method
            PlayerPrefs.DeleteKey(PREF_LAST_LOGIN_METHOD);
            PlayerPrefs.Save();

            // Clear EOS session
            ProductUserId productUserId = EOSManager.Instance?.GetProductUserId();
            if (productUserId != null && productUserId.IsValid())
            {
                EOSManager.Instance.ClearConnectId(productUserId);
            }

            UpdateStatus("Logged out");
        }

        #endregion

        #region Private Methods - Device ID Login

        private IEnumerator LoginWithDeviceIdInternal(bool isAutoLogin)
        {
            bool createCompleted = false;
            bool createSuccess = false;
            bool loginCompleted = false;
            bool loginSuccess = false;
            string errorMessage = "";

            // First, try to create Device ID if it doesn't exist
            if (!isAutoLogin)
            {
                UpdateStatus("Setting up guest account...");

                 ConnectInterface connectInterface = EOSManager.Instance.GetEOSConnectInterface();
                var createOptions = new Epic.OnlineServices.Connect.CreateDeviceIdOptions()
                {
                    DeviceModel = SystemInfo.deviceModel
                };

                connectInterface.CreateDeviceId(ref createOptions, null, (ref Epic.OnlineServices.Connect.CreateDeviceIdCallbackInfo createInfo) =>
                {
                    createCompleted = true;

                    if (createInfo.ResultCode == Result.Success)
                    {
                        createSuccess = true;
                        Debug.Log("[AuthenticationManager] Device ID created successfully");
                    }
                    else if (createInfo.ResultCode == Result.DuplicateNotAllowed)
                    {
                        // Device ID already exists, this is fine
                        createSuccess = true;
                        Debug.Log("[AuthenticationManager] Device ID already exists");
                    }
                    else
                    {
                        errorMessage = $"Failed to create Device ID: {createInfo.ResultCode}";
                        Debug.LogError($"[AuthenticationManager] {errorMessage}");
                    }
                });

                // Wait for Device ID creation
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

            // Now login with Device ID
            UpdateStatus(isAutoLogin ? "Restoring guest session..." : "Logging in as guest...");

            string displayName = $"Guest_{SystemInfo.deviceUniqueIdentifier.Substring(0, 8)}";

            EOSManager.Instance.StartConnectLoginWithOptions(
                ExternalCredentialType.DeviceidAccessToken,
                null,
                displayName,
                (Epic.OnlineServices.Connect.LoginCallbackInfo callbackInfo) =>
                {
                    loginCompleted = true;

                    if (callbackInfo.ResultCode == Result.Success)
                    {
                        loginSuccess = true;
                        Debug.Log("[AuthenticationManager] Device ID login successful");
                    }
                    else if (callbackInfo.ResultCode == Result.InvalidUser && isAutoLogin)
                    {
                        // Device ID doesn't exist yet during auto-login
                        Debug.Log("[AuthenticationManager] Device ID not found during auto-login");
                        errorMessage = "Device ID not found";
                    }
                    else
                    {
                        errorMessage = $"Login failed: {callbackInfo.ResultCode}";
                        Debug.LogError($"[AuthenticationManager] {errorMessage}");
                    }
                }
            );

            // Wait for login to complete
            float loginTimeout = 10f;
            float loginElapsed = 0f;

            while (!loginCompleted && loginElapsed < loginTimeout)
            {
                loginElapsed += Time.deltaTime;
                yield return null;
            }

            if (loginSuccess)
            {
                // Save login method for auto-login next time
                PlayerPrefs.SetString(PREF_LAST_LOGIN_METHOD, LOGIN_METHOD_DEVICE_ID);
                PlayerPrefs.Save();
                Debug.Log("[AuthenticationManager] Saved Device ID as last login method");

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

        #endregion

        #region Helper Methods

        private bool IsUserLoggedIn()
        {
            if (EOSManager.Instance == null) return false;

            // Check if user has a valid ProductUserId (Connect login)
            ProductUserId productUserId = EOSManager.Instance.GetProductUserId();
            bool isLoggedIn = productUserId != null && productUserId.IsValid();

            if (isLoggedIn)
            {
                Debug.Log($"[AuthenticationManager] User is logged in with ProductUserId: {productUserId}");
            }

            return isLoggedIn;
        }

        private void UpdateStatus(string message)
        {
            Debug.Log($"[AuthenticationManager] {message}");
            OnLoginStatusChanged?.Invoke(message);
        }

        #endregion
    }
}
