using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RecipeRage.Core.Services;
using RecipeRage.Modules.Auth.Core;
using RecipeRage.Modules.Auth.Interfaces;
using UnityEngine;

namespace RecipeRage.Modules.Auth.Providers
{
    /// <summary>
    /// Auth provider for Facebook login.
    /// This provider uses the Facebook SDK to authenticate users.
    /// 
    /// Note: For this to work, you need to:
    /// 1. Import the Facebook SDK for Unity
    /// 2. Configure your Facebook App ID in the Facebook Settings
    /// 
    /// Complexity Rating: 4
    /// </summary>
    public class FacebookAuthProvider : BaseAuthProvider
    {
        /// <summary>
        /// The name of this authentication provider
        /// </summary>
        public override string ProviderName => "Facebook";
        
        /// <summary>
        /// Facebook login supports persistent login across app restarts
        /// </summary>
        public override bool SupportsPersistentLogin => true;
        
        /// <summary>
        /// PlayerPrefs keys
        /// </summary>
        private const string KEY_USER_ID = "UserID";
        private const string KEY_DISPLAY_NAME = "DisplayName";
        private const string KEY_ACCESS_TOKEN = "AccessToken";
        
        /// <summary>
        /// Facebook permissions required by the app
        /// </summary>
        private readonly List<string> _permissions = new List<string> { "public_profile", "email" };
        
        /// <summary>
        /// Flag to indicate if authentication is in progress
        /// </summary>
        private bool _authInProgress = false;
        
        /// <summary>
        /// Flag to indicate if Facebook SDK is initialized
        /// </summary>
        private bool _isInitialized = false;
        
        /// <summary>
        /// Initialize the Facebook SDK
        /// </summary>
        private void InitializeFacebookSDK()
        {
            if (_isInitialized)
            {
                return;
            }
            
            Logger.Info("FacebookAuthProvider", "Initializing Facebook SDK");
            
            // Check if Facebook SDK is available
            #if UNITY_FACEBOOK
            // Initialize Facebook SDK
            if (!FB.IsInitialized)
            {
                FB.Init(
                    () => {
                        if (FB.IsInitialized)
                        {
                            Logger.Info("FacebookAuthProvider", "Facebook SDK initialized");
                            FB.ActivateApp();
                            _isInitialized = true;
                        }
                        else
                        {
                            Logger.Error("FacebookAuthProvider", "Failed to initialize Facebook SDK");
                        }
                    },
                    isGameShown => {
                        if (!isGameShown)
                        {
                            // Pause game logic when game is hidden (e.g., Facebook login dialog is shown)
                            Time.timeScale = 0;
                        }
                        else
                        {
                            // Resume game logic when game becomes visible again
                            Time.timeScale = 1;
                        }
                    }
                );
            }
            else
            {
                Logger.Info("FacebookAuthProvider", "Facebook SDK already initialized");
                FB.ActivateApp();
                _isInitialized = true;
            }
            #else
            Logger.Warning("FacebookAuthProvider", "Facebook SDK is not available. Please import the Facebook SDK for Unity.");
            #endif
            
            _isInitialized = true;
        }
        
        /// <summary>
        /// Authenticate with Facebook
        /// </summary>
        /// <param name="onSuccess">Callback when authentication succeeds</param>
        /// <param name="onFailure">Callback when authentication fails</param>
        public override void Authenticate(Action<IAuthProviderUser> onSuccess, Action<string> onFailure)
        {
            if (_authInProgress)
            {
                onFailure?.Invoke("Authentication is already in progress");
                return;
            }
            
            _authInProgress = true;
            
            // Initialize Facebook SDK if needed
            InitializeFacebookSDK();
            
            // Check if Facebook SDK is available
            #if UNITY_FACEBOOK
            Logger.Info("FacebookAuthProvider", "Starting Facebook login");
            
            // Check if the user is already logged in
            if (FB.IsLoggedIn)
            {
                Logger.Info("FacebookAuthProvider", "User is already logged in, getting profile info");
                GetFacebookProfileInfo(onSuccess, onFailure);
                return;
            }
            
            // Login with Facebook
            FB.LogInWithReadPermissions(_permissions, (result) => {
                if (result.Cancelled)
                {
                    _authInProgress = false;
                    string error = "Facebook login was cancelled by the user";
                    Logger.Warning("FacebookAuthProvider", error);
                    onFailure?.Invoke(error);
                }
                else if (!string.IsNullOrEmpty(result.Error))
                {
                    _authInProgress = false;
                    string error = $"Facebook login error: {result.Error}";
                    Logger.Error("FacebookAuthProvider", error);
                    onFailure?.Invoke(error);
                }
                else
                {
                    Logger.Info("FacebookAuthProvider", "Login successful, getting profile info");
                    GetFacebookProfileInfo(onSuccess, onFailure);
                }
            });
            #else
            _authInProgress = false;
            string error = "Facebook SDK is not available. Please import the Facebook SDK for Unity.";
            Logger.Error("FacebookAuthProvider", error);
            onFailure?.Invoke(error);
            
            // For development/testing without Facebook SDK, you can create a mock user
            #if UNITY_EDITOR
            Logger.Warning("FacebookAuthProvider", "Creating mock Facebook user for testing");
            CreateMockFacebookUser(onSuccess);
            #endif
            #endif
        }
        
        /// <summary>
        /// Get the Facebook profile information
        /// </summary>
        /// <param name="onSuccess">Success callback</param>
        /// <param name="onFailure">Failure callback</param>
        private void GetFacebookProfileInfo(Action<IAuthProviderUser> onSuccess, Action<string> onFailure)
        {
            #if UNITY_FACEBOOK
            // Query Facebook API for user data
            FB.API("/me?fields=id,name,email", HttpMethod.GET, (result) => {
                if (result.Error != null)
                {
                    _authInProgress = false;
                    string error = $"Error retrieving Facebook profile: {result.Error}";
                    Logger.Error("FacebookAuthProvider", error);
                    onFailure?.Invoke(error);
                    return;
                }
                
                // Extract user information from the result
                string id = result.ResultDictionary["id"].ToString();
                string name = result.ResultDictionary.ContainsKey("name") ? 
                    result.ResultDictionary["name"].ToString() : 
                    $"FacebookUser{UnityEngine.Random.Range(1000, 9999)}";
                
                string accessToken = FB.IsLoggedIn ? AccessToken.CurrentAccessToken.TokenString : null;
                
                // Save user information
                SaveToPlayerPrefs(KEY_USER_ID, id);
                SaveToPlayerPrefs(KEY_DISPLAY_NAME, name);
                if (!string.IsNullOrEmpty(accessToken))
                {
                    SaveToPlayerPrefs(KEY_ACCESS_TOKEN, accessToken);
                }
                
                // Create user object
                IAuthProviderUser user = new AuthProviderUser(
                    userId: id,
                    provider: this,
                    displayName: name,
                    accessToken: accessToken,
                    isGuest: false
                );
                
                Logger.Info("FacebookAuthProvider", $"Successfully authenticated user {name} (ID: {id})");
                
                _authInProgress = false;
                onSuccess?.Invoke(user);
            });
            #else
            _authInProgress = false;
            onFailure?.Invoke("Facebook SDK is not available");
            #endif
        }
        
        /// <summary>
        /// Create a mock Facebook user for testing
        /// </summary>
        /// <param name="onSuccess">Success callback</param>
        private void CreateMockFacebookUser(Action<IAuthProviderUser> onSuccess)
        {
            string id = "mock_facebook_" + Guid.NewGuid().ToString().Substring(0, 8);
            string name = "Mock Facebook User";
            string accessToken = "mock_access_token_" + DateTime.UtcNow.Ticks;
            
            // Save mock user information
            SaveToPlayerPrefs(KEY_USER_ID, id);
            SaveToPlayerPrefs(KEY_DISPLAY_NAME, name);
            SaveToPlayerPrefs(KEY_ACCESS_TOKEN, accessToken);
            
            // Create user object
            IAuthProviderUser user = new AuthProviderUser(
                userId: id,
                provider: this,
                displayName: name,
                accessToken: accessToken,
                isGuest: false
            );
            
            Logger.Info("FacebookAuthProvider", $"Created mock Facebook user {name} (ID: {id})");
            
            _authInProgress = false;
            onSuccess?.Invoke(user);
        }
        
        /// <summary>
        /// Sign out from Facebook
        /// </summary>
        /// <param name="onComplete">Callback when sign out is complete</param>
        public override void SignOut(Action onComplete = null)
        {
            #if UNITY_FACEBOOK
            Logger.Info("FacebookAuthProvider", "Logging out from Facebook");
            
            if (FB.IsLoggedIn)
            {
                FB.LogOut();
                Logger.Info("FacebookAuthProvider", "Logged out from Facebook");
            }
            #endif
            
            // Clear cached credentials
            ClearCachedCredentials();
            
            onComplete?.Invoke();
        }
        
        /// <summary>
        /// Check if the provider has cached Facebook credentials
        /// </summary>
        /// <returns>True if cached credentials exist</returns>
        public override bool HasCachedCredentials()
        {
            #if UNITY_FACEBOOK
            // Check if the user is logged in with Facebook
            if (FB.IsInitialized && FB.IsLoggedIn)
            {
                return true;
            }
            #endif
            
            // Check if we have cached user ID and access token
            return HasPlayerPrefsKey(KEY_USER_ID) && HasPlayerPrefsKey(KEY_ACCESS_TOKEN);
        }
        
        /// <summary>
        /// Clear any cached Facebook credentials
        /// </summary>
        public override void ClearCachedCredentials()
        {
            DeleteFromPlayerPrefs(KEY_USER_ID);
            DeleteFromPlayerPrefs(KEY_DISPLAY_NAME);
            DeleteFromPlayerPrefs(KEY_ACCESS_TOKEN);
            Logger.Info("FacebookAuthProvider", "Cleared cached credentials");
        }
    }
} 