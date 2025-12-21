using System;
using Core.Logging;
using Core.RemoteConfig;
using Core.RemoteConfig.Models;
using Cysharp.Threading.Tasks;
using UI;
using UnityEngine;

namespace Core.Update
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
                GameLogger.Log("Checking for force update...");

                // Refresh force update config to get latest data
                await _configService.RefreshConfig<ForceUpdateConfig>();

                // Get force update config
                if (!_configService.TryGetConfig<ForceUpdateConfig>(out var updateConfig))
                {
                    GameLogger.LogWarning("ForceUpdateConfig not available");
                    return false;
                }

                // Get current platform
                string platform = GetCurrentPlatform();
                string currentVersion = Application.version;

                GameLogger.Log($"Current platform: {platform}, version: {currentVersion}");

                // Get platform-specific requirements
                var requirement = updateConfig.GetRequirementForPlatform(platform);

                if (requirement == null)
                {
                    GameLogger.Log($"No update requirements for platform: {platform}");
                    return false;
                }

                // Check if update is required
                bool isRequired = requirement.IsUpdateRequired(currentVersion);
                bool isRecommended = requirement.IsUpdateRecommended(currentVersion);

                if (isRequired)
                {
                    GameLogger.Log($"Force update required: {currentVersion} < {requirement.MinimumVersion}");
                    await ShowForceUpdatePopupAsync(requirement, true);
                    return true;
                }
                else if (isRecommended && requirement.UpdateUrgency == UpdateUrgency.Recommended)
                {
                    GameLogger.Log($"Update recommended: {currentVersion} < {requirement.RecommendedVersion}");
                    await ShowForceUpdatePopupAsync(requirement, false);
                    return false;
                }

                GameLogger.Log("App version is up to date");
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
                GameLogger.Log($"Showing update popup - Required: {isRequired}");

                string urgencyText = isRequired ? "REQUIRED" : "RECOMMENDED";
                string message = $"{requirement.UpdateMessage}\n\n" +
                                $"Current Version: {Application.version}\n" +
                                $"Minimum Version: {requirement.MinimumVersion}";

                GameLogger.Log($"Update popup: {message}");

                // Show notification with update prompt
                if (_uiService != null)
                {
                    _uiService.ShowScreen(UIScreenType.Notification);
                    var notificationScreen = _uiService.GetScreen<UI.Screens.NotificationScreen>(UIScreenType.Notification);
                    if (notificationScreen != null)
                    {
                        var notificationType = isRequired ? UI.Screens.NotificationType.Error : UI.Screens.NotificationType.Warning;
                        await notificationScreen.Show(requirement.UpdateTitle, message, notificationType, isRequired ? 0f : 10f);

                        // Open store URL
                        if (!string.IsNullOrEmpty(requirement.StoreUrl))
                        {
                            // Should show a popup here
                            // OpenStoreUrl(requirement.StoreUrl);
                        }
                    }
                }

                if (isRequired)
                {
                    // Block app usage until update
                    GameLogger.Log("Blocking app usage - update required");
                    // Could show a blocking screen here
                }

                await UniTask.Yield();
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Failed to show update popup: {ex.Message}");
            }
        }

        private void OpenStoreUrl(string storeUrl)
        {
            if (string.IsNullOrEmpty(storeUrl))
            {
                GameLogger.LogError("Store URL is empty");
                return;
            }

            try
            {
                GameLogger.Log($"Opening store URL: {storeUrl}");
                Application.OpenURL(storeUrl);
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Failed to open store URL: {ex.Message}");
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
