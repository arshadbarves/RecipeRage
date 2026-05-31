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

        private readonly IEventBus _eventBus;
        private readonly ISaveService _saveService;
        private readonly UGSConfig _ugsConfig;

        public string ProductUserId => EOSManager.Instance?.GetProductUserId()?.ToString();
        public bool IsGuest { get; private set; }

        public AuthenticationService(IEventBus eventBus, ISaveService saveService, UGSConfig ugsConfig)
        {
            _eventBus = eventBus;
            _saveService = saveService;
            _ugsConfig = ugsConfig;
        }

        // ══════════════════════════════════════════════
        // GDD v3 contract
        // ══════════════════════════════════════════════

        public async Task<AuthResult> LoginAsGuestAsync()
        {
            try
            {
                await InitializeUgsAsync();
                bool success = await LoginWithEosDeviceIdAsync();
                if (!success)
                {
                    return AuthResult.Failed("EOS Device ID login failed");
                }

                await LoginToUgsWithEosAsync();

                IsGuest = true;
                _saveService.UpdateSettings(s => s.LastLoginMethod = "DeviceID");

                var result = new AuthResult(true, ProductUserId, isGuest: true);
                _eventBus?.Publish(new LoginSuccessEvent { UserId = ProductUserId, DisplayName = "User" });
                return result;
            }
            catch (Exception ex)
            {
                return AuthResult.Failed(ex.Message);
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

        public async Task LogoutAsync()
        {
            GameLogger.LogInfo("Logging out from all services...");

            _saveService.UpdateSettings(s => s.LastLoginMethod = "");
            IsGuest = false;

            if (UnityServices.State == ServicesInitializationState.Initialized &&
                Unity.Services.Authentication.AuthenticationService.Instance.IsSignedIn)
            {
                Unity.Services.Authentication.AuthenticationService.Instance.SignOut();
            }

            if (EOSManager.Instance != null)
            {
                ProductUserId puid = EOSManager.Instance.GetProductUserId();
                if (puid != null && puid.IsValid())
                {
                    EOSManager.Instance.ClearConnectId(puid);
                }
            }

            _eventBus?.Publish(new LogoutEvent { UserId = ProductUserId ?? "unknown" });
            await Task.CompletedTask;
        }

        // ══════════════════════════════════════════════
        // Internal helpers
        // ══════════════════════════════════════════════

        private async Task InitializeUgsAsync()
        {
            if (UnityServices.State == ServicesInitializationState.Initialized)
            {
                return;
            }

            GameLogger.Log("Initializing Unity Services...");

            var options = new InitializationOptions();
            if (_ugsConfig != null && !string.IsNullOrEmpty(_ugsConfig.authenticationProfile))
            {
                options.SetProfile(_ugsConfig.authenticationProfile);
            }
            await UnityServices.InitializeAsync(options);

            Unity.Services.Authentication.AuthenticationService.Instance.SignedIn += () =>
                GameLogger.Log($"UGS signed in - PlayerId: {PlayerId}");
            Unity.Services.Authentication.AuthenticationService.Instance.SignedOut += () =>
                GameLogger.Log("UGS signed out");
            Unity.Services.Authentication.AuthenticationService.Instance.SignInFailed += (ex) =>
                GameLogger.LogError($"UGS sign-in failed: {ex.Message}");

            GameLogger.Log("Unity Services initialized");
        }

        private string PlayerId => (UnityServices.State == ServicesInitializationState.Initialized)
            ? Unity.Services.Authentication.AuthenticationService.Instance?.PlayerId
            : "NOT_INITIALIZED";

        private async UniTask<bool> LoginWithEosDeviceIdAsync()
        {
            bool deviceIdReady = await EnsureEosDeviceIdCreated();
            if (!deviceIdReady)
            {
                return false;
            }

            var tcs = new UniTaskCompletionSource<bool>();

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
                string eosId = ProductUserId;
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
                    int delayMs = (int)Math.Pow(2, attempt) * 500;
                    GameLogger.LogWarning($"[AuthenticationService] Device ID creation attempt {attempt} failed, retrying in {delayMs}ms...");
                    await UniTask.Delay(delayMs);
                }
            }

            GameLogger.LogError($"[AuthenticationService] Failed to create EOS Device ID after {MAX_RETRIES} attempts");
            return false;
        }

        public void Dispose()
        {
            if (UnityServices.State == ServicesInitializationState.Initialized)
            {
                IAuthenticationService authService = Unity.Services.Authentication.AuthenticationService.Instance;
                if (authService != null)
                {
                }
            }
        }
    }
}
