using System;
using Epic.OnlineServices;
using Connect = Epic.OnlineServices.Connect;
using PlayEveryWare.EpicOnlineServices;
using RecipeRage.Modules.Auth.Core;
using RecipeRage.Modules.Auth.Interfaces;
using RecipeRage.Modules.Logging;
using UnityEngine;
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
            LogHelper.Info("EOSDeviceAuthProvider", "Starting device authentication");

            // Get the Connect interface from EOS Manager
            var connectInterface = EOSManager.Instance.GetEOSConnectInterface();
            if (connectInterface == null)
            {
                _authInProgress = false;
                string error = "EOS Connect interface is not available";
                LogHelper.Error("EOSDeviceAuthProvider", error);
                onFailure?.Invoke(error);
                return;
            }

            // Create options for device ID creation
            var createDeviceOptions = new Connect.CreateDeviceIdOptions
            {
                DeviceModel = SystemInfo.deviceModel
            };

            // Start the device creation/authentication flow
            connectInterface.CreateDeviceId(ref createDeviceOptions, null,
                (ref Connect.CreateDeviceIdCallbackInfo callbackInfo) =>
                {
                    if (callbackInfo.ResultCode == Result.Success ||
                        callbackInfo.ResultCode == Result.DuplicateNotAllowed)
                    {
                        // Device ID created or already exists, now login
                        string displayName = Environment.UserName;
                        if (string.IsNullOrEmpty(displayName) || displayName == "Unknown")
                            displayName = $"Player{Random.Range(1000, 9999)}";

                        LogHelper.Info("EOSDeviceAuthProvider", $"Device ID created, logging in as {displayName}");

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
                        LogHelper.Error("EOSDeviceAuthProvider", error);
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
        private void HandleLoginCallback(Connect.LoginCallbackInfo loginCallbackInfo, Action<IAuthProviderUser> onSuccess,
            Action<string> onFailure)
        {
            if (loginCallbackInfo.ResultCode == Result.Success)
            {
                // Login successful
                LogHelper.Info("EOSDeviceAuthProvider",
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
                LogHelper.Info("EOSDeviceAuthProvider", "New user needs to be created");

                // Create a new connect user with the continuance token
                EOSManager.Instance.CreateConnectUserWithContinuanceToken(
                    loginCallbackInfo.ContinuanceToken,
                    createUserCallbackInfo =>
                    {
                        if (createUserCallbackInfo.ResultCode == Result.Success)
                        {
                            LogHelper.Info("EOSDeviceAuthProvider", "New user created, trying to login again");

                            // Try to login again after creating the user
                            Authenticate(onSuccess, onFailure);
                        }
                        else
                        {
                            _authInProgress = false;
                            string error = $"Failed to create user: {createUserCallbackInfo.ResultCode}";
                            LogHelper.Error("EOSDeviceAuthProvider", error);
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
                LogHelper.Error("EOSDeviceAuthProvider", error);
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
            try
            {
                // Try to get display name from user info manager
                // Use Type.GetType to locate the EOSUserInfoManager class at runtime from the sample plugin
                Type userInfoManagerType = Type.GetType("EOSUserInfoManager, Assembly-CSharp");
                if (userInfoManagerType != null)
                {
                    var getOrCreateMethod = typeof(EOSManager).GetMethod("GetOrCreateManager")?.MakeGenericMethod(userInfoManagerType);
                    if (getOrCreateMethod != null)
                    {
                        var userInfoManager = getOrCreateMethod.Invoke(EOSManager.Instance, null);
                        if (userInfoManager != null)
                        {
                            // Try to get user info using reflection
                            var getUserInfoMethod = userInfoManagerType.GetMethod("GetUserInfo", new[] { typeof(ProductUserId) });
                            if (getUserInfoMethod != null)
                            {
                                var userInfo = getUserInfoMethod.Invoke(userInfoManager, new object[] { productUserId });
                                if (userInfo != null)
                                {
                                    var displayNameProperty = userInfo.GetType().GetProperty("DisplayName");
                                    if (displayNameProperty != null)
                                    {
                                        string displayName = displayNameProperty.GetValue(userInfo) as string;
                                        if (!string.IsNullOrEmpty(displayName))
                                        {
                                            callback(displayName);
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warning("EOSDeviceAuthProvider", $"Error getting display name: {ex.Message}");
                // Continue to fallback
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
            LogHelper.Info("EOSDeviceAuthProvider", "Cleared cached credentials");
        }

        /// <summary>
        /// Sign out from EOS
        /// </summary>
        /// <param name="onComplete"> Callback when sign out is complete </param>
        public override void SignOut(Action onComplete = null)
        {
            LogHelper.Info("EOSDeviceAuthProvider", "Signing out");

            // Clear cached credentials
            ClearCachedCredentials();

            var connectInterface = EOSManager.Instance.GetEOSConnectInterface();
            if (connectInterface != null)
            {
                var productUserId = EOSManager.Instance.GetProductUserId();
                if (productUserId != null && productUserId.IsValid())
                {
                    // Log out of EOS Connect
                    var connectLogoutOptions = new Connect.LogoutOptions
                    {
                        LocalUserId = productUserId
                    };

                    connectInterface.Logout(ref connectLogoutOptions, null,
                        (ref Connect.LogoutCallbackInfo logoutCallbackInfo) =>
                        {
                            if (logoutCallbackInfo.ResultCode == Result.Success)
                            {
                                LogHelper.Info("EOSDeviceAuthProvider", "Successfully logged out of EOS Connect");
                            }
                            else
                            {
                                LogHelper.Warning("EOSDeviceAuthProvider",
                                    $"Failed to log out of EOS Connect: {logoutCallbackInfo.ResultCode}");
                            }

                            onComplete?.Invoke();
                        });
                }
                else
                {
                    LogHelper.Warning("EOSDeviceAuthProvider", "No valid product user ID to log out");
                    onComplete?.Invoke();
                }
            }
            else
            {
                LogHelper.Warning("EOSDeviceAuthProvider", "EOS Connect interface is not available");
                onComplete?.Invoke();
            }
        }
    }
}