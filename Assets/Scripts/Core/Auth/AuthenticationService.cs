using System;
using System.Threading.Tasks;
using Core.Logging;
using Core.Persistence;
using Core.Shared.Events;
using Core.Networking;
using Cysharp.Threading.Tasks;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using PlayEveryWare.EpicOnlineServices;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace Core.Auth
{
    public class AuthenticationService : IAuthService, IDisposable
    {
        private const int TIMEOUT_SECONDS = 15;
        
        private readonly IEventBus _eventBus;
        private readonly ISaveService _saveService;
        private readonly UGSConfig _ugsConfig;

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

        public async UniTask<bool> InitializeAsync()
        {
            if (IsInitialized) return true;

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
            if (EOSManager.Instance == null) return false;
            var productUserId = EOSManager.Instance.GetProductUserId();
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
                    eosSuccess = await LoginWithEosDeviceIdAsync();
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
            if (type == AuthType.DeviceID)
            {
                _saveService.UpdateSettings(s => s.LastLoginMethod = "DeviceID");
            }

            GameLogger.LogInfo("Unified login successful (EOS primary ready).");
            _eventBus?.Publish(new LoginSuccessEvent { UserId = EosProductUserId, DisplayName = "User" });
            return true;
        }

        public async UniTask LogoutAsync()
        {
            GameLogger.LogInfo("Logging out from all services...");
            
            // 1. Clear persistence
            _saveService.UpdateSettings(s => s.LastLoginMethod = "");

            // 2. UGS Logout
            if (UnityServices.State == ServicesInitializationState.Initialized && 
                Unity.Services.Authentication.AuthenticationService.Instance.IsSignedIn)
            {
                Unity.Services.Authentication.AuthenticationService.Instance.SignOut();
            }

            // 3. EOS Logout
            if (EOSManager.Instance != null)
            {
                var productUserId = EOSManager.Instance.GetProductUserId();
                if (productUserId != null && productUserId.IsValid())
                {
                    EOSManager.Instance.ClearConnectId(productUserId);
                }
            }

            _eventBus?.Publish(new LogoutEvent { UserId = EosProductUserId ?? "unknown" });
            await UniTask.Yield();
        }

        private async UniTask<bool> LoginWithEosDeviceIdAsync()
        {
            bool deviceIdReady = await EnsureEosDeviceIdCreated();
            if (!deviceIdReady) return false;

            var tcs = new UniTaskCompletionSource<bool>();
            string displayName = $"Guest_{SystemInfo.deviceUniqueIdentifier.Substring(0, 8)}";

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
                if (string.IsNullOrEmpty(eosId)) return false;

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
            var connectInterface = EOSManager.Instance.GetEOSConnectInterface();
            if (connectInterface == null) return false;

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
                    GameLogger.LogError($"Failed to create EOS Device ID: {info.ResultCode}");
                    tcs.TrySetResult(false);
                }
            });

            return await tcs.Task.Timeout(TimeSpan.FromSeconds(TIMEOUT_SECONDS));
        }

        private void OnUgsSignedIn() => GameLogger.Log($"UGS signed in - PlayerId: {PlayerId}");
        private void OnUgsSignedOut() => GameLogger.Log("UGS signed out");
        private void OnUgsSignInFailed(RequestFailedException ex) => GameLogger.LogError($"UGS sign-in failed: {ex.Message}");

        public void Dispose()
        {
            if (UnityServices.State == ServicesInitializationState.Initialized)
            {
                var authService = Unity.Services.Authentication.AuthenticationService.Instance;
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
