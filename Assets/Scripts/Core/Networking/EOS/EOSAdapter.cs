using System;
using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Platform;
using UnityEngine;

namespace RecipeRage.Core.Networking.EOS
{
    /// <summary>
    /// Adapter class to bridge the gap between our code's expected API and the actual EOS SDK API.
    /// </summary>
    public static class EOSAdapter
    {
        private static bool _isInitialized = false;
        private static bool _isLoggedIn = false;
        private static EpicAccountId _currentUserId = null;
        private static string _displayName = "Player";
        private static Action<bool, EpicAccountId, string> _loggedInCallback = null;

        /// <summary>
        /// Initialize the EOS SDK.
        /// </summary>
        /// <returns>True if initialization was successful.</returns>
        public static bool Init()
        {
            try
            {
                // The PlayEveryWare SDK initializes itself, so we just need to check if it's available
                if (EOSManager.Instance == null)
                {
                    Debug.LogError("[EOSAdapter] EOSManager instance not found");
                    return false;
                }

                _isInitialized = true;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[EOSAdapter] Error initializing EOS SDK: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get the EOS Platform Interface.
        /// </summary>
        /// <returns>The EOS Platform Interface.</returns>
        public static PlatformInterface GetEOSPlatformInterface()
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[EOSAdapter] EOS SDK not initialized");
                return null;
            }

            return EOSManager.Instance.GetEOSPlatformInterface();
        }

        /// <summary>
        /// Check if the user is logged in.
        /// </summary>
        /// <returns>True if the user is logged in.</returns>
        public static bool IsLoggedIn()
        {
            return _isLoggedIn;
        }

        /// <summary>
        /// Set the callback for when the user logs in.
        /// </summary>
        /// <param name="callback">The callback to invoke when the user logs in.</param>
        public static void SetLoggedInCallback(Action<bool, EpicAccountId, string> callback)
        {
            _loggedInCallback = callback;
        }

        /// <summary>
        /// Start login with device ID.
        /// </summary>
        /// <param name="displayName">The display name to use.</param>
        /// <param name="callback">The callback to invoke when login completes.</param>
        public static void StartLoginWithDeviceID(string displayName, Action<bool, string> callback)
        {
            if (!_isInitialized)
            {
                callback?.Invoke(false, "EOS SDK not initialized");
                return;
            }

            _displayName = displayName;

            // Get the auth interface
            var authInterface = EOSManager.Instance.GetEOSPlatformInterface().GetAuthInterface();
            if (authInterface == null)
            {
                callback?.Invoke(false, "Failed to get auth interface");
                return;
            }

            // Create login credentials
            var credentials = new Epic.OnlineServices.Auth.Credentials
            {
                Type = LoginCredentialType.DeviceCode,
                Id = displayName
            };

            var options = new LoginOptions
            {
                Credentials = credentials,
                ScopeFlags = AuthScopeFlags.BasicProfile | AuthScopeFlags.FriendsList | AuthScopeFlags.Presence
            };

            // Create a callback delegate that matches the expected signature
            Epic.OnlineServices.Auth.OnLoginCallback loginCallback = (ref LoginCallbackInfo result) =>
            {
                if (result.ResultCode == Result.Success)
                {
                    _isLoggedIn = true;
                    _currentUserId = result.LocalUserId;

                    // Invoke the logged in callback
                    _loggedInCallback?.Invoke(true, _currentUserId, null);

                    // Invoke the completion callback
                    callback?.Invoke(true, null);
                }
                else
                {
                    string errorMessage = result.ResultCode.ToString();
                    Debug.LogError($"[EOSAdapter] Login failed: {errorMessage}");

                    // Invoke the completion callback
                    callback?.Invoke(false, errorMessage);
                }
            };

            // Login with the proper callback
            authInterface.Login(ref options, null, loginCallback);
        }

        /// <summary>
        /// Get the display name of the current user.
        /// </summary>
        /// <returns>The display name of the current user.</returns>
        public static string GetDisplayName()
        {
            return _displayName;
        }

        /// <summary>
        /// Get the current user ID.
        /// </summary>
        /// <returns>The current user ID.</returns>
        public static EpicAccountId GetCurrentUserId()
        {
            return _currentUserId;
        }
    }
}
