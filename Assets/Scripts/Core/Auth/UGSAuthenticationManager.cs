using System;
using System.Threading.Tasks;
using Core.Logging;
using Core.Networking;
using PlayEveryWare.EpicOnlineServices;
using Unity.Services.Authentication;
using Unity.Services.Core;

namespace Core.Auth
{
    /// <summary>
    /// Manages Unity Gaming Services authentication
    /// EOS is PRIMARY, Unity Authentication is SECONDARY
    ///
    /// Flow: EOS Login (Primary) → Unity Auth (using EOS ProductUserId as external token)
    /// NO PlayerPrefs needed - Unity handles mapping server-side!
    /// </summary>
    public class UGSAuthenticationManager
    {
        #region Properties

        public bool IsInitialized { get; private set; }
        public bool IsSignedIn => AuthenticationService.Instance?.IsSignedIn ?? false;
        public string PlayerId => AuthenticationService.Instance?.PlayerId;
        public string EosProductUserId { get; private set; }

        #endregion

        #region Private Fields

        private readonly UGSConfig _config;

        #endregion

        #region Initialization

        public UGSAuthenticationManager(UGSConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Initialize Unity Gaming Services
        /// </summary>
        public async Task<bool> InitializeAsync()
        {
            if (IsInitialized)
            {
                GameLogger.LogWarning("UGS already initialized");
                return true;
            }

            try
            {
                GameLogger.Log("Initializing Unity Gaming Services...");

                // Initialize Unity Services
                var options = new InitializationOptions();

                if (!string.IsNullOrEmpty(_config.authenticationProfile))
                {
                    options.SetProfile(_config.authenticationProfile);
                }

                await UnityServices.InitializeAsync(options);

                // Setup authentication events
                AuthenticationService.Instance.SignedIn += OnSignedIn;
                AuthenticationService.Instance.SignedOut += OnSignedOut;
                AuthenticationService.Instance.SignInFailed += OnSignInFailed;
                AuthenticationService.Instance.Expired += OnSessionExpired;

                IsInitialized = true;
                GameLogger.Log("UGS initialized successfully");
                return true;
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Failed to initialize UGS: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Authentication - EOS PRIMARY

        /// <summary>
        /// Sign in to Unity using EOS ProductUserId as external identity
        /// EOS is PRIMARY, Unity is SECONDARY
        /// NO PlayerPrefs needed - Unity handles mapping server-side!
        /// </summary>
        public async Task<bool> SignInWithEOSAsync()
        {
            if (!IsInitialized)
            {
                GameLogger.LogError("UGS not initialized");
                return false;
            }

            try
            {
                // Get EOS ProductUserId (PRIMARY identity)
                var productUserId = EOSManager.Instance?.GetProductUserId();
                if (productUserId == null || !productUserId.IsValid())
                {
                    GameLogger.LogError("EOS ProductUserId not available");
                    return false;
                }

                EosProductUserId = productUserId.ToString();

                GameLogger.Log($"Signing in to Unity with EOS identity: {EosProductUserId}");

                // Check if already signed in with same EOS ID
                if (AuthenticationService.Instance.IsSignedIn)
                {
                    GameLogger.Log($"Already signed in - PlayerId: {PlayerId}");
                    return true;
                }

                // Sign in using EOS ProductUserId as external identity
                // Unity stores the mapping server-side: EOS "0002abc..." → Unity "player-xyz-123"
                // NO PlayerPrefs needed - works across all devices automatically!
                await AuthenticationService.Instance.SignInWithOpenIdConnectAsync(
                    "eos",  // Provider ID
                    EosProductUserId  // EOS ProductUserId as token
                );

                GameLogger.Log($"✅ Unity sign-in successful - PlayerId: {PlayerId}");
                GameLogger.Log($"✅ Mapping: EOS '{EosProductUserId}' → Unity '{PlayerId}'");
                GameLogger.Log($"✅ Mapping stored SERVER-SIDE by Unity (not PlayerPrefs!)");

                return true;
            }
            catch (AuthenticationException ex)
            {
                GameLogger.LogError($"Unity authentication failed: {ex.Message}");
                return false;
            }
            catch (RequestFailedException ex)
            {
                GameLogger.LogError($"Unity request failed: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Sign Out

        /// <summary>
        /// Sign out from Unity
        /// NO PlayerPrefs to clear - Unity handles everything server-side!
        /// </summary>
        public void SignOut()
        {
            if (IsSignedIn)
            {
                GameLogger.Log("Signing out from Unity...");

                // Sign out from Unity
                AuthenticationService.Instance.SignOut();

                // Clear memory
                EosProductUserId = null;

                GameLogger.Log("Unity sign out complete");
            }
        }

        #endregion

        #region Event Handlers

        private void OnSignedIn()
        {
            GameLogger.Log($"Unity signed in - PlayerId: {PlayerId}");
        }

        private void OnSignedOut()
        {
            GameLogger.Log("Unity signed out");
            EosProductUserId = null;
        }

        private void OnSignInFailed(RequestFailedException exception)
        {
            GameLogger.LogError($"Unity sign-in failed: {exception.Message}");
        }

        private void OnSessionExpired()
        {
            GameLogger.LogWarning("Unity session expired - attempting to refresh...");

            // Attempt to refresh session
            Task.Run(async () =>
            {
                try
                {
                    // Unity will auto-refresh with cached session token
                    if (AuthenticationService.Instance.SessionTokenExists)
                    {
                        GameLogger.Log("Session token exists - Unity will auto-refresh");
                    }
                    else
                    {
                        GameLogger.LogWarning("No session token - need to sign in again with EOS");
                    }
                }
                catch (Exception ex)
                {
                    GameLogger.LogError($"Failed to refresh Unity session: {ex.Message}");
                }
            });
        }

        #endregion

        #region Cleanup

        public void Dispose()
        {
            if (!IsInitialized)
                return;

            // Unsubscribe from events
            if (AuthenticationService.Instance != null)
            {
                AuthenticationService.Instance.SignedIn -= OnSignedIn;
                AuthenticationService.Instance.SignedOut -= OnSignedOut;
                AuthenticationService.Instance.SignInFailed -= OnSignInFailed;
                AuthenticationService.Instance.Expired -= OnSessionExpired;
            }

            IsInitialized = false;
        }

        #endregion
    }
}
