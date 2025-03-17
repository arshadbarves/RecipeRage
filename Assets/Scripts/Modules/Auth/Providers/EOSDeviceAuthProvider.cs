using System;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using PlayEveryWare.EpicOnlineServices;
using RecipeRage.Modules.Auth.Core;
using RecipeRage.Modules.Auth.Interfaces;
using UnityEngine;
using Logger = RecipeRage.Core.Services.Logger;
using Random = UnityEngine.Random;

namespace RecipeRage.Modules.Auth.Providers
{
    /// <summary>
    /// Auth provider for EOS Device Authentication.
    /// This provider uses Epic Online Services' device authentication for EOS integration.
    /// Complexity Rating: 4
    /// </summary>
    public class EOSDeviceAuthProvider : BaseAuthProvider
    {
        /// <summary>
        /// PlayerPrefs keys
        /// </summary>
        private const string KEY_PRODUCT_USER_ID = "ProductUserID";

        private const string KEY_DISPLAY_NAME = "DisplayName";

        /// <summary>
        /// Flag to indicate if authentication is in progress
        /// </summary>
        private bool _authInProgress;

        /// <summary>
        /// The name of this authentication provider
        /// </summary>
        public override string ProviderName => "EOSDevice";

        /// <summary>
        /// EOS supports persistent login across app restarts
        /// </summary>
        public override bool SupportsPersistentLogin => true;

        /// <summary>
        /// Authenticate with EOS using device authentication
        /// </summary>
        /// <param name="onSuccess"> Callback when authentication succeeds </param>
        /// <param name="onFailure"> Callback when authentication fails </param>
        public override void Authenticate(Action<IAuthProviderUser> onSuccess, Action<string> onFailure)
        {
            if (_authInProgress)
            {
                onFailure?.Invoke("Authentication is already in progress");
                return;
            }

            _authInProgress = true;
            Logger.Info("EOSDeviceAuthProvider", "Starting device authentication");

            // Get the Connect interface from EOS Manager
            var connectInterface = EOSManager.Instance.GetEOSConnectInterface();
            if (connectInterface == null)
            {
                _authInProgress = false;
                string error = "EOS Connect interface is not available";
                Logger.Error("EOSDeviceAuthProvider", error);
                onFailure?.Invoke(error);
                return;
            }

            // Create options for device ID creation
            var createDeviceOptions = new CreateDeviceIdOptions
            {
                DeviceModel = SystemInfo.deviceModel
            };

            // Start the device creation/authentication flow
            connectInterface.CreateDeviceId(ref createDeviceOptions, null,
                (ref CreateDeviceIdCallbackInfo callbackInfo) =>
                {
                    if (callbackInfo.ResultCode == Result.Success ||
                        callbackInfo.ResultCode == Result.DuplicateNotAllowed)
                    {
                        // Device ID created or already exists, now login
                        string displayName = Environment.UserName;
                        if (string.IsNullOrEmpty(displayName) || displayName == "Unknown")
                            displayName = $"Player{Random.Range(1000, 9999)}";

                        Logger.Info("EOSDeviceAuthProvider", $"Device ID created, logging in as {displayName}");

                        // Login with the device credentials
                        EOSManager.Instance.StartConnectLoginWithOptions(
                            ExternalCredentialType.DeviceidAccessToken,
                            null,
                            displayName,
                            loginCallbackInfo => HandleLoginCallback(loginCallbackInfo, onSuccess, onFailure)
                        );
                    }
                    else
                    {
                        _authInProgress = false;
                        string error = $"Failed to create Device ID: {callbackInfo.ResultCode}";
                        Logger.Error("EOSDeviceAuthProvider", error);
                        onFailure?.Invoke(error);
                    }
                });
        }

        /// <summary>
        /// Handle the login callback from EOS
        /// </summary>
        /// <param name="loginCallbackInfo"> Login callback info </param>
        /// <param name="onSuccess"> Success callback </param>
        /// <param name="onFailure"> Failure callback </param>
        private void HandleLoginCallback(LoginCallbackInfo loginCallbackInfo, Action<IAuthProviderUser> onSuccess,
            Action<string> onFailure)
        {
            if (loginCallbackInfo.ResultCode == Result.Success)
            {
                // Login successful
                Logger.Info("EOSDeviceAuthProvider",
                    $"Login successful, ProductUserId: {loginCallbackInfo.LocalUserId}");

                // Save the product user ID
                SaveToPlayerPrefs(KEY_PRODUCT_USER_ID, loginCallbackInfo.LocalUserId.ToString());

                // Get display name from EOS
                GetUserDisplayName(loginCallbackInfo.LocalUserId, displayName =>
                {
                    // Save display name
                    SaveToPlayerPrefs(KEY_DISPLAY_NAME, displayName);

                    // Create user object
                    IAuthProviderUser user = new AuthProviderUser(
                        loginCallbackInfo.LocalUserId.ToString(),
                        this,
                        displayName
                    );

                    _authInProgress = false;
                    onSuccess?.Invoke(user);
                });
            }
            else if (loginCallbackInfo.ResultCode == Result.InvalidUser)
            {
                // New user needs to be created
                Logger.Info("EOSDeviceAuthProvider", "New user needs to be created");

                // Create a new connect user with the continuance token
                EOSManager.Instance.CreateConnectUserWithContinuanceToken(
                    loginCallbackInfo.ContinuanceToken,
                    createUserCallbackInfo =>
                    {
                        if (createUserCallbackInfo.ResultCode == Result.Success)
                        {
                            Logger.Info("EOSDeviceAuthProvider", "New user created, trying to login again");

                            // Try to login again after creating the user
                            Authenticate(onSuccess, onFailure);
                        }
                        else
                        {
                            _authInProgress = false;
                            string error = $"Failed to create user: {createUserCallbackInfo.ResultCode}";
                            Logger.Error("EOSDeviceAuthProvider", error);
                            onFailure?.Invoke(error);
                        }
                    }
                );
            }
            else
            {
                // Login failed
                _authInProgress = false;
                string error = $"Login failed: {loginCallbackInfo.ResultCode}";
                Logger.Error("EOSDeviceAuthProvider", error);
                onFailure?.Invoke(error);
            }
        }

        /// <summary>
        /// Get the display name for a user
        /// </summary>
        /// <param name="productUserId"> Product user ID </param>
        /// <param name="callback"> Callback with display name </param>
        private void GetUserDisplayName(ProductUserId productUserId, Action<string> callback)
        {
            // Try to get display name from user info manager
            var userInfoManager = EOSManager.Instance.GetOrCreateManager<EOSUserInfoManager>();
            if (userInfoManager != null)
            {
                // Request user info
                var userInfo = userInfoManager.GetUserInfo(productUserId);
                if (userInfo != null && !string.IsNullOrEmpty(userInfo.DisplayName))
                {
                    callback(userInfo.DisplayName);
                    return;
                }
            }

            // Fall back to a generic name if no display name is available
            string fallbackName = LoadFromPlayerPrefs(KEY_DISPLAY_NAME);
            if (string.IsNullOrEmpty(fallbackName)) fallbackName = $"Player{Random.Range(1000, 9999)}";

            callback(fallbackName);
        }

        /// <summary>
        /// Check if the provider has cached EOS credentials
        /// </summary>
        /// <returns> True if cached credentials exist </returns>
        public override bool HasCachedCredentials()
        {
            return HasPlayerPrefsKey(KEY_PRODUCT_USER_ID);
        }

        /// <summary>
        /// Clear any cached EOS credentials
        /// </summary>
        public override void ClearCachedCredentials()
        {
            DeleteFromPlayerPrefs(KEY_PRODUCT_USER_ID);
            DeleteFromPlayerPrefs(KEY_DISPLAY_NAME);
            Logger.Info("EOSDeviceAuthProvider", "Cleared cached credentials");
        }

        /// <summary>
        /// Sign out from EOS
        /// </summary>
        /// <param name="onComplete"> Callback when sign out is complete </param>
        public override void SignOut(Action onComplete = null)
        {
            // Check if we have cached credentials
            if (HasCachedCredentials())
            {
                string productUserIdStr = LoadFromPlayerPrefs(KEY_PRODUCT_USER_ID);

                // Try to sign out from EOS if we have a valid product user ID
                if (!string.IsNullOrEmpty(productUserIdStr))
                {
                    Logger.Info("EOSDeviceAuthProvider", $"Signing out user {productUserIdStr}");

                    // Attempt to logout from EOS Connect
                    var connectInterface = EOSManager.Instance.GetEOSConnectInterface();
                    if (connectInterface != null)
                    {
                        // Parse the product user ID
                        var productUserId = ProductUserId.FromString(productUserIdStr);
                        if (productUserId != null && productUserId.IsValid())
                        {
                            var logoutOptions = new LogoutOptions
                            {
                                LocalUserId = productUserId
                            };

                            connectInterface.Logout(ref logoutOptions, null,
                                (ref LogoutCallbackInfo logoutCallbackInfo) =>
                                {
                                    if (logoutCallbackInfo.ResultCode == Result.Success)
                                        Logger.Info("EOSDeviceAuthProvider", "Logout successful");
                                    else
                                        Logger.Warning("EOSDeviceAuthProvider",
                                            $"Logout returned {logoutCallbackInfo.ResultCode}");

                                    // Clear credentials even if logout fails
                                    ClearCachedCredentials();
                                    onComplete?.Invoke();
                                });

                            return;
                        }
                    }
                }
            }

            // If we can't sign out properly, just clear credentials
            ClearCachedCredentials();
            onComplete?.Invoke();
        }
    }
}