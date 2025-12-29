using System;
using Cysharp.Threading.Tasks;
using RecipeRage.Modules.Auth.Core;
using PlayEveryWare.EpicOnlineServices;
using Core.Logging;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using UnityEngine;

namespace RecipeRage.Modules.Auth.Core
{
    public class EOSAuthService : IAuthService
    {
        private const int TIMEOUT_SECONDS = 15;

        public bool IsLoggedIn()
        {
            if (EOSManager.Instance == null) return false;
            var productUserId = EOSManager.Instance.GetProductUserId();
            return productUserId != null && productUserId.IsValid();
        }

        public string GetCurrentUserId()
        {
            return EOSManager.Instance?.GetProductUserId()?.ToString() ?? string.Empty;
        }

        public async UniTask<bool> LoginAsync(AuthType type)
        {
            GameLogger.Log($"[Auth] Attempting login with type: {type}");

            switch (type)
            {
                case AuthType.DeviceID:
                    return await LoginWithDeviceIdAsync();
                case AuthType.DevAuth:
                    return await LoginWithDevAuthAsync();
                case AuthType.AccountPortal:
                    return await LoginWithAccountPortalAsync();
                default:
                    GameLogger.LogError($"[Auth] Unsupported AuthType: {type}");
                    return false;
            }
        }

        public async UniTask LogoutAsync()
        {
            if (EOSManager.Instance == null) return;

            var productUserId = EOSManager.Instance.GetProductUserId();
            if (productUserId != null && productUserId.IsValid())
            {
                EOSManager.Instance.ClearConnectId(productUserId);
                GameLogger.Log("[Auth] Logged out from EOS");
            }
            
            await UniTask.Yield();
        }

        private async UniTask<bool> LoginWithDeviceIdAsync()
        {
            // 1. Ensure Device ID exists
            bool deviceIdReady = await EnsureDeviceIdCreated();
            if (!deviceIdReady) return false;

            // 2. Perform Connect Login
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
                        GameLogger.Log("[Auth] Device ID login successful");
                        tcs.TrySetResult(true);
                    }
                    else
                    {
                        GameLogger.LogError($"[Auth] Device ID login failed: {callbackInfo.ResultCode}");
                        tcs.TrySetResult(false);
                    }
                }
            );

            return await tcs.Task.Timeout(TimeSpan.FromSeconds(TIMEOUT_SECONDS));
        }

        private async UniTask<bool> EnsureDeviceIdCreated()
        {
            var connectInterface = EOSManager.Instance.GetEOSConnectInterface();
            if (connectInterface == null) return false;

            var createOptions = new CreateDeviceIdOptions()
            {
                DeviceModel = SystemInfo.deviceModel
            };

            var tcs = new UniTaskCompletionSource<bool>();
            connectInterface.CreateDeviceId(ref createOptions, null, (ref CreateDeviceIdCallbackInfo info) =>
            {
                if (info.ResultCode == Result.Success || info.ResultCode == Result.DuplicateNotAllowed)
                {
                    tcs.TrySetResult(true);
                }
                else
                {
                    GameLogger.LogError($"[Auth] Failed to create Device ID: {info.ResultCode}");
                    tcs.TrySetResult(false);
                }
            });

            return await tcs.Task.Timeout(TimeSpan.FromSeconds(TIMEOUT_SECONDS));
        }

        private async UniTask<bool> LoginWithDevAuthAsync()
        {
            // DevAuth implementation usually uses Account Portal or DevAuthTool
            // For now, implementing basic routing as per spec
            GameLogger.Log("[Auth] DevAuth login requested (Implementation pending DevAuthTool config)");
            return await UniTask.FromResult(false);
        }

        private async UniTask<bool> LoginWithAccountPortalAsync()
        {
            GameLogger.Log("[Auth] Account Portal login requested");
            return await UniTask.FromResult(false);
        }
        
        public void Initialize()
        {
            GameLogger.Log("[Auth] EOSAuthService Initialized");
        }
    }
}