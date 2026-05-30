using KitchenClash.Application;
using System;
using System.Threading.Tasks;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Network;
using Cysharp.Threading.Tasks;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using PlayEveryWare.EpicOnlineServices;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace KitchenClash.Infrastructure.EOS
{
    public class AuthenticationService : IAuthService, IDisposable
    {
        private const int TIMEOUT_SECONDS = 15;

        /// <summary>
        /// Constant for DeviceID login method. Use this instead of nameof() to avoid breaking changes.
        /// </summary>
        public const string LOGIN_METHOD_DEVICE_ID = "DeviceID";
        
        private readonly IEventBus _eventBus;
        private readonly ISaveService _saveService;
        private readonly UGSConfig _ugsConfig;

        // ── GDD v3 properties ──
        public string ProductUserId => EosProductUserId;
        public bool IsGuest { get; private set; }
        public event Action<AuthResult> OnAuthChanged;

        // ── Legacy properties ──
        public bool IsInitialized { get; private set; }
        public bool IsSignedIn => IsLoggedIn(); // EOS State
        public bool IsUgsSignedIn => (UnityServices.State == ServicesInitializationState.Initialized) && 
                                     Unity.Services.Authentication.AuthenticationService.Instance.IsSignedIn;

        public string PlayerId => (UnityServices.State == ServicesInitializationState.Initialized) 
            ? Unity.Services.Authentication.AuthenticationService.Instance?.PlayerId 
            : "NOT_INITIALIZED";
        public string EosProductUserId => EOSManager.Instance?.GetProductUserId()?.ToString();

        public AuthenticationService(IEventBus eventBus, ISaveService saveService, UGSConfig ugsConfig)
        {
            _eventBus = eventBus;
            _saveService = saveService;
            _ugsConfig = ugsConfig;
        }

        // ══════════════════════════════════════════════
        // GDD v3 methods
        // ══════════════════════════════════════════════

        public async Task<AuthResult> LoginAsGuestAsync()
        {
            try
            {
                await InitializeAsync();
                bool success = await LoginWithEosDeviceIdAsync();
                if (!success)
                {
                    var fail = AuthResult.Failed("EOS Device ID login failed");
                    OnAuthChanged?.Invoke(fail);
                    return fail;
                }

                // Best-effort UGS
                await LoginToUgsWithEosAsync();

                IsGuest = true;
                _saveService.UpdateSettings(s => s.LastLoginMethod = LOGIN_METHOD_DEVICE_ID);

                var result = new AuthResult(true, ProductUserId, isGuest: true);
                OnAuthChanged?.Invoke(result);
                _eventBus?.Publish(new LoginSuccessEvent { UserId = EosProductUserId, DisplayName = "User" });
                return result;
            }
            catch (Exception ex)
            {
                var fail = AuthResult.Failed(ex.Message);
                OnAuthChanged?.Invoke(fail);
                return fail;
            }
        }

        public async Task<AuthResult> LoginWithGoogleAsync()
        {
#if UNITY_ANDROID
            // REQUIRES: Google Sign-In Unity SDK (com.google.signin).
            //           GoogleSignIn.DefaultInstance.SignIn() → idToken
            //           → EOS Connect.Login(ExternalCredentialType.GoogleIdToken, token)
#endif
            return await Task.FromResult(AuthResult.Failed("Google login not yet implemented"));
        }

        public async Task<AuthResult> LoginWithFacebookAsync()
        {
#if UNITY_ANDROID || UNITY_IOS
            // REQUIRES: Facebook SDK for Unity (com.facebook.sdk).
            //           FB.LogInWithReadPermissions() → accessToken
            //           → EOS Connect.Login(ExternalCredentialType.FacebookAccessToken, token)
#endif
            return await Task.FromResult(AuthResult.Failed("Facebook login not yet implemented"));
        }

        public async Task<AuthResult> LoginWithAppleAsync()
        {
#if UNITY_IOS
            // REQUIRES: Apple Sign-In Unity plugin (e.g. com.lupidan.apple-signin-unity).
            //           AppleAuthManager.LoginWithAppleId() → idToken
            //           → EOS Connect.Login(ExternalCredentialType.AppleIdToken, token)
#endif
            return await Task.FromResult(AuthResult.Failed("Apple login not yet implemented"));
        }

        public async Task LinkToGoogleAsync()
        {
#if UNITY_ANDROID
            // REQUIRES: Google Sign-In SDK.
            //           EOS Connect.LinkAccount(DeviceId PUID → Google PUID)
#endif
            await Task.CompletedTask;
        }

        // ══════════════════════════════════════════════
        // Legacy methods (kept for backward compat)
        // ══════════════════════════════════════════════

        public async UniTask<bool> InitializeAsync()
        {
            if (IsInitialized)
            {
                return true;
            }

            try
            {
                GameLogger.Log("Initializing Authentication Services (EOS & UGS)...");

                // 1. Initialize Unity Services
                var options = new InitializationOptions();
                if (_ugsConfig != null && !string.IsNullOrEmpty(_ugsConfig.authenticationProfile))
                {
                    options.SetProfile(_ugsConfig.authenticationProfile);
                }
                await UnityServices.InitializeAsync(options);

                // Setup UGS events
                Unity.Services.Authentication.AuthenticationService.Instance.SignedIn += OnUgsSignedIn;
                Unity.Services.Authentication.AuthenticationService.Instance.SignedOut += OnUgsSignedOut;
                Unity.Services.Authentication.AuthenticationService.Instance.SignInFailed += OnUgsSignInFailed;

                IsInitialized = true;
                GameLogger.Log("Authentication Services initialized successfully");
                return true;
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Failed to initialize Authentication Services: {ex.Message}");
                return false;
            }
        }

        private bool IsLoggedIn()
        {
            if (EOSManager.Instance == null)
            {
                return false;
            }

            ProductUserId productUserId = EOSManager.Instance.GetProductUserId();
            bool eosLoggedIn = productUserId != null && productUserId.IsValid();
            
            // UGS is now optional for the general "IsSignedIn" state
            return eosLoggedIn;
        }

        public async UniTask<bool> LoginAsync(AuthType type)
        {
            GameLogger.LogInfo($"Attempting unified login with type: {type}");

            // 1. EOS Login (Primary)
            bool eosSuccess = false;
            switch (type)
            {
                case AuthType.DeviceID:
                case AuthType.Guest:
                    eosSuccess = await LoginWithEosDeviceIdAsync();
                    IsGuest = true;
                    break;
                default:
                    GameLogger.LogError($"Unsupported AuthType: {type}");
                    break;
            }

            if (!eosSuccess)
            {
                _eventBus?.Publish(new LoginFailedEvent { Error = "EOS Login failed" });
                return false;
            }

            // 2. UGS Login (Secondary, linked to EOS)
            // USER NOTE: UGS is optional. Even if it fails, we proceed with EOS.
            bool ugsSuccess = await LoginToUgsWithEosAsync();
            
            if (!ugsSuccess)
            {
                GameLogger.LogWarning("UGS Login failed - Friends system and other UGS features will be disabled.");
                // We don't return false here anymore
            }

            // Save persistence
            if (type == AuthType.DeviceID || type == AuthType.Guest)
            {
                _saveService.UpdateSettings(s => s.LastLoginMethod = LOGIN_METHOD_DEVICE_ID);
            }

            GameLogger.LogInfo("Unified login successful (EOS primary ready).");
            var authResult = new AuthResult(true, EosProductUserId, isGuest: IsGuest);
            OnAuthChanged?.Invoke(authResult);
            _eventBus?.Publish(new LoginSuccessEvent { UserId = EosProductUserId, DisplayName = "User" });
            return true;
        }

        public async UniTask LogoutAsync()
        {
            GameLogger.LogInfo("Logging out from all services...");
            
            // 1. Clear persistence
            _saveService.UpdateSettings(s => s.LastLoginMethod = "");
            IsGuest = false;

            // 2. UGS Logout
            if (UnityServices.State == ServicesInitializationState.Initialized && 
                Unity.Services.Authentication.AuthenticationService.Instance.IsSignedIn)
            {
                Unity.Services.Authentication.AuthenticationService.Instance.SignOut();
            }

            // 3. EOS Logout
            if (EOSManager.Instance != null)
            {
                ProductUserId productUserId = EOSManager.Instance.GetProductUserId();
                if (productUserId != null && productUserId.IsValid())
                {
                    EOSManager.Instance.ClearConnectId(productUserId);
                }
            }

            var result = AuthResult.Failed("Logged out");
            OnAuthChanged?.Invoke(result);
            _eventBus?.Publish(new LogoutEvent { UserId = EosProductUserId ?? "unknown" });
            await UniTask.Yield();
        }

        // ══════════════════════════════════════════════
        // Internal EOS helpers
        // ══════════════════════════════════════════════

        private async UniTask<bool> LoginWithEosDeviceIdAsync()
        {
            bool deviceIdReady = await EnsureEosDeviceIdCreated();
            if (!deviceIdReady)
            {
                return false;
            }

            var tcs = new UniTaskCompletionSource<bool>();

            // Safe device ID extraction with null/empty check
            string deviceId = SystemInfo.deviceUniqueIdentifier ?? "unknown";
            string displayName = $"Guest_{(deviceId.Length >= 8 ? deviceId.Substring(0, 8) : deviceId)}";

            EOSManager.Instance.StartConnectLoginWithOptions(
                ExternalCredentialType.DeviceidAccessToken,
                null,
                displayName,
                (LoginCallbackInfo callbackInfo) =>
                {
                    if (callbackInfo.ResultCode == Result.Success)
                    {
                        GameLogger.LogInfo("EOS Device ID login successful");
                        tcs.TrySetResult(true);
                    }
                    else
                    {
                        GameLogger.LogError($"EOS Device ID login failed: {callbackInfo.ResultCode}");
                        tcs.TrySetResult(false);
                    }
                }
            );

            return await tcs.Task.Timeout(TimeSpan.FromSeconds(TIMEOUT_SECONDS));
        }

        private async UniTask<bool> LoginToUgsWithEosAsync()
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                GameLogger.LogWarning("UGS Authentication skipped: Unity Services not initialized.");
                return false;
            }

            try
            {
                string eosId = EosProductUserId;
                if (string.IsNullOrEmpty(eosId))
                {
                    return false;
                }

                GameLogger.Log($"Signing in to UGS with EOS identity: {eosId}");

                await Unity.Services.Authentication.AuthenticationService.Instance.SignInWithOpenIdConnectAsync(
                    "eos", 
                    eosId
                );

                return true;
            }
            catch (Exception ex)
            {
                GameLogger.LogWarning($"UGS authentication failed: {ex.Message}");
                return false;
            }
        }

        private async UniTask<bool> EnsureEosDeviceIdCreated()
        {
            ConnectInterface connectInterface = EOSManager.Instance.GetEOSConnectInterface();
            if (connectInterface == null)
            {
                return false;
            }

            // Retry logic with exponential backoff
            const int MAX_RETRIES = 3;
            int attempt = 0;

            while (attempt < MAX_RETRIES)
            {
                var createOptions = new CreateDeviceIdOptions() { DeviceModel = SystemInfo.deviceModel };
                var tcs = new UniTaskCompletionSource<bool>();

                connectInterface.CreateDeviceId(ref createOptions, null, (ref CreateDeviceIdCallbackInfo info) =>
                {
                    if (info.ResultCode == Result.Success || info.ResultCode == Result.DuplicateNotAllowed)
                    {
                        tcs.TrySetResult(true);
                    }
                    else
                    {
                        tcs.TrySetResult(false);
                    }
                });

                bool success = await tcs.Task.Timeout(TimeSpan.FromSeconds(TIMEOUT_SECONDS));

                if (success)
                {
                    return true;
                }

                attempt++;
                if (attempt < MAX_RETRIES)
                {
                    int delayMs = (int)Math.Pow(2, attempt) * 500; // 1s, 2s, 4s
                    GameLogger.LogWarning($"[AuthenticationService] Device ID creation attempt {attempt} failed, retrying in {delayMs}ms...");
                    await UniTask.Delay(delayMs);
                }
            }

            GameLogger.LogError($"[AuthenticationService] Failed to create EOS Device ID after {MAX_RETRIES} attempts");
            return false;
        }

        private void OnUgsSignedIn() => GameLogger.Log($"UGS signed in - PlayerId: {PlayerId}");
        private void OnUgsSignedOut() => GameLogger.Log("UGS signed out");
        private void OnUgsSignInFailed(RequestFailedException ex) => GameLogger.LogError($"UGS sign-in failed: {ex.Message}");

        public void Dispose()
        {
            if (UnityServices.State == ServicesInitializationState.Initialized)
            {
                IAuthenticationService authService = Unity.Services.Authentication.AuthenticationService.Instance;
                if (authService != null)
                {
                    authService.SignedIn -= OnUgsSignedIn;
                    authService.SignedOut -= OnUgsSignedOut;
                    authService.SignInFailed -= OnUgsSignInFailed;
                }
            }
        }
    }
}
