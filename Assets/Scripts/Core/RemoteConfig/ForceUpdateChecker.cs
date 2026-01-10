using System;
using Core.Logging;
using Core.RemoteConfig.Interfaces;
using Core.RemoteConfig.Models;
using Core.Shared.Events;
using Core.Shared.Utilities;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.RemoteConfig
{
    /// <summary>
    /// Checks for required app updates and handles force update flow.
    /// Uses events to notify UI layer - decoupled from Gameplay assembly.
    /// </summary>
    public class ForceUpdateChecker
    {
        private readonly IRemoteConfigService _configService;
        private readonly IEventBus _eventBus;

        public ForceUpdateChecker(
            IRemoteConfigService configService,
            IEventBus eventBus)
        {
            _configService = configService;
            _eventBus = eventBus;
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

                string platform = PlatformUtils.GetPlatform();
                string currentVersion = Application.version;
                var requirement = updateConfig.GetRequirementForPlatform(platform);

                if (requirement == null) return false;

                bool isRequired = requirement.IsUpdateRequired(currentVersion);
                bool isRecommended = requirement.IsUpdateRecommended(currentVersion);

                if (isRequired)
                {
                    PublishForceUpdateEvent(requirement, true);
                    return true;
                }
                else if (isRecommended && requirement.UpdateUrgency == UpdateUrgency.Recommended)
                {
                    PublishForceUpdateEvent(requirement, false);
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

        private void PublishForceUpdateEvent(PlatformVersionRequirement requirement, bool isRequired)
        {
            string message = $"{requirement.UpdateMessage}\n\n" +
                            $"Current Version: {Application.version}\n" +
                            $"Minimum Version: {requirement.MinimumVersion}";

            _eventBus?.Publish(new ForceUpdateEvent
            {
                Title = requirement.UpdateTitle,
                Message = message,
                IsRequired = isRequired,
                Duration = isRequired ? 0f : 10f
            });

            GameLogger.LogInfo($"Force update event published: IsRequired={isRequired}");
        }
    }
}
