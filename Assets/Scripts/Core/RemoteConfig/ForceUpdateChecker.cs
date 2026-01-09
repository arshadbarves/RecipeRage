using System;
using Core.Core.Logging;
using Core.Core.RemoteConfig.Interfaces;
using Core.Core.RemoteConfig.Models;
using Core.Core.UI.Interfaces;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Core.RemoteConfig
{
    /// <summary>
    /// Checks for required app updates and handles force update flow
    /// </summary>
    public class ForceUpdateChecker
    {
        private readonly IRemoteConfigService _configService;
        private readonly IUIService _uiService;

        public ForceUpdateChecker(
            IRemoteConfigService configService,
            IUIService uiService)
        {
            _configService = configService;
            _uiService = uiService;
        }

        public async UniTask<bool> CheckForUpdateAsync()
        {
            try
            {
                await _configService.RefreshConfig<ForceUpdateConfig>();

                if (!_configService.TryGetConfig<ForceUpdateConfig>(out var updateConfig))
                {
                    GameLogger.LogWarning("ForceUpdateConfig not available");
                    return false;
                }

                string platform = GetCurrentPlatform();
                string currentVersion = Application.version;
                var requirement = updateConfig.GetRequirementForPlatform(platform);

                if (requirement == null) return false;

                bool isRequired = requirement.IsUpdateRequired(currentVersion);
                bool isRecommended = requirement.IsUpdateRecommended(currentVersion);

                if (isRequired)
                {
                    await ShowForceUpdatePopupAsync(requirement, true);
                    return true;
                }
                else if (isRecommended && requirement.UpdateUrgency == UpdateUrgency.Recommended)
                {
                    await ShowForceUpdatePopupAsync(requirement, false);
                    return false;
                }

                return false;
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Force update check failed: {ex.Message}");
                return false;
            }
        }

        private async UniTask ShowForceUpdatePopupAsync(PlatformVersionRequirement requirement, bool isRequired)
        {
            try
            {
                string message = $"{requirement.UpdateMessage}\n\n" +
                                $"Current Version: {Application.version}\n" +
                                $"Minimum Version: {requirement.MinimumVersion}";

                if (_uiService != null)
                {
                    _uiService.ShowScreen(UIScreenType.Notification);
                    var notificationScreen = _uiService.GetScreen<NotificationScreen>(UIScreenType.Notification);
                    if (notificationScreen != null)
                    {
                        var notificationType = isRequired ? global::Core.Core.UI.Interfaces.NotificationType.Error : global::Core.Core.UI.Interfaces.NotificationType.Warning;
                        await notificationScreen.Show(requirement.UpdateTitle, message, notificationType, isRequired ? 0f : 10f);
                    }
                }

                await UniTask.Yield();
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Failed to show update popup: {ex.Message}");
            }
        }

        private string GetCurrentPlatform()
        {
#if UNITY_IOS
            return "iOS";
#elif UNITY_ANDROID
            return "Android";
#elif UNITY_STANDALONE
            return "PC";
#else
            return "Unknown";
#endif
        }
    }
}
