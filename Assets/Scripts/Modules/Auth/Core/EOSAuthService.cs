using System;
using Cysharp.Threading.Tasks;
using PlayEveryWare.EpicOnlineServices;
using Core.Logging;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using UnityEngine;
using Core.Events;
using Core.SaveSystem;

namespace RecipeRage.Modules.Auth.Core
{
    public class EOSAuthService : IAuthService
    {
        private const int TIMEOUT_SECONDS = 15;
        private readonly IEventBus _eventBus;
        private readonly ISaveService _saveService;
        private readonly Action _onLoginSuccess;

        public EOSAuthService(IEventBus eventBus, ISaveService saveService, Action onLoginSuccess = null)
        {
            _eventBus = eventBus;
            _saveService = saveService;
            _onLoginSuccess = onLoginSuccess;
        }

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

            bool success = false;
            switch (type)
            {
                case AuthType.DeviceID:
                    success = await LoginWithDeviceIdAsync();
                    break;
                default:
                    GameLogger.LogError($"[Auth] Unsupported AuthType: {type}");
                    break;
            }

                        if (success)
                        {
                            // Save persistence
                            if (type == AuthType.DeviceID)
                            {
                                _saveService.UpdateSettings(s => s.LastLoginMethod = "DeviceID");
                            }
            
                            _onLoginSuccess?.Invoke();
                            
                            GameLogger.Log("[Auth] Publishing LoginSuccessEvent...");
                            _eventBus?.Publish(new LoginSuccessEvent
                            {
                                UserId = GetCurrentUserId(),
                                DisplayName = "User"
                            });
                            GameLogger.Log("[Auth] LoginSuccessEvent Published");
                        }            else
            {
                _eventBus?.Publish(new LoginFailedEvent
                {
                    Error = "Login failed"
                });
            }

            return success;
        }

        public async UniTask LogoutAsync()
        {
            // Clear persistence
            _saveService.UpdateSettings(s => s.LastLoginMethod = "");

            if (EOSManager.Instance == null) return;

            var productUserId = EOSManager.Instance.GetProductUserId();
            string userIdStr = productUserId?.ToString() ?? "unknown";

            if (productUserId != null && productUserId.IsValid())
            {
                EOSManager.Instance.ClearConnectId(productUserId);
                GameLogger.Log("[Auth] Logged out from EOS");
            }

            _eventBus?.Publish(new LogoutEvent
            {
                UserId = userIdStr
            });

            await UniTask.Yield();
        }

        private async UniTask<bool> LoginWithDeviceIdAsync()
        {
            bool deviceIdReady = await EnsureDeviceIdCreated();
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
    }
}
