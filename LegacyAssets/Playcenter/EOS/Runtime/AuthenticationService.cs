using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using PlayEveryWare.EpicOnlineServices;
using Playcenter.Services;
using Playcenter.Shell;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace Playcenter.EOS
{
    /// <summary>
    /// Shared EOS Connect + optional UGS bridge auth. Title side effects via <see cref="IAuthLifecycleHooks"/>.
    /// </summary>
    public class AuthenticationService : IAuthService, IDisposable
    {
        private const int TimeoutSeconds = 15;

        private readonly IEOSConfig _eosConfig;
        private readonly IAuthLifecycleHooks _lifecycleHooks;

        public string ProductUserId => EOSManager.Instance?.GetProductUserId()?.ToString();
        public bool IsGuest { get; private set; }

        public AuthenticationService(IEOSConfig eosConfig, IAuthLifecycleHooks lifecycleHooks = null)
        {
            _eosConfig = eosConfig ?? throw new ArgumentNullException(nameof(eosConfig));
            _lifecycleHooks = lifecycleHooks;
        }

        public async Task<AuthResult> LoginAsGuestAsync()
        {
            try
            {
                if (_eosConfig.EnableUgsBridge)
                {
                    await InitializeUgsAsync();
                }

                bool success = await LoginWithEosDeviceIdAsync();
                if (!success)
                {
                    return AuthResult.Failed("EOS Device ID login failed");
                }

                if (_eosConfig.EnableUgsBridge)
                {
                    await LoginToUgsWithEosAsync();
                }

                IsGuest = true;
                string puid = ProductUserId;
                _lifecycleHooks?.OnLoginSucceeded(puid, "User", isGuest: true, loginMethod: "DeviceID");
                return new AuthResult(true, puid, isGuest: true);
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

            string puid = ProductUserId ?? "unknown";
            IsGuest = false;

            if (UnityServices.State == ServicesInitializationState.Initialized &&
                Unity.Services.Authentication.AuthenticationService.Instance.IsSignedIn)
            {
                Unity.Services.Authentication.AuthenticationService.Instance.SignOut();
            }

            if (EOSManager.Instance != null)
            {
                Epic.OnlineServices.ProductUserId productUserId = EOSManager.Instance.GetProductUserId();
                if (productUserId != null && productUserId.IsValid())
                {
                    EOSManager.Instance.ClearConnectId(productUserId);
                }
            }

            _lifecycleHooks?.OnLogout(puid);
            await Task.CompletedTask;
        }

        private async Task InitializeUgsAsync()
        {
            if (UnityServices.State == ServicesInitializationState.Initialized)
            {
                return;
            }

            GameLogger.Log("Initializing Unity Services...");

            var options = new InitializationOptions();
            if (!string.IsNullOrEmpty(_eosConfig.AuthenticationProfile))
            {
                options.SetProfile(_eosConfig.AuthenticationProfile);
            }

            await UnityServices.InitializeAsync(options);

            Unity.Services.Authentication.AuthenticationService.Instance.SignedIn += () =>
                GameLogger.Log($"UGS signed in - PlayerId: {PlayerId}");
            Unity.Services.Authentication.AuthenticationService.Instance.SignedOut += () =>
                GameLogger.Log("UGS signed out");
            Unity.Services.Authentication.AuthenticationService.Instance.SignInFailed += ex =>
                GameLogger.LogError($"UGS sign-in failed: {ex.Message}");

            GameLogger.Log("Unity Services initialized");
        }

        private string PlayerId => UnityServices.State == ServicesInitializationState.Initialized
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

            return await tcs.Task.Timeout(TimeSpan.FromSeconds(TimeoutSeconds));
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

            const int maxRetries = 3;
            int attempt = 0;

            while (attempt < maxRetries)
            {
                var createOptions = new CreateDeviceIdOptions { DeviceModel = SystemInfo.deviceModel };
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

                bool success = await tcs.Task.Timeout(TimeSpan.FromSeconds(TimeoutSeconds));

                if (success)
                {
                    return true;
                }

                attempt++;
                if (attempt < maxRetries)
                {
                    int delayMs = (int)Math.Pow(2, attempt) * 500;
                    GameLogger.LogWarning($"[AuthenticationService] Device ID creation attempt {attempt} failed, retrying in {delayMs}ms...");
                    await UniTask.Delay(delayMs);
                }
            }

            GameLogger.LogError($"[AuthenticationService] Failed to create EOS Device ID after {maxRetries} attempts");
            return false;
        }

        public void Dispose()
        {
        }
    }
}
