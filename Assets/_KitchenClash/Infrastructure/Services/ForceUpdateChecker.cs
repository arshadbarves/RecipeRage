using System;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Configuration;
using KitchenClash.Application.Models.RemoteConfigs;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace KitchenClash.Infrastructure.Services
{
    public class ForceUpdateChecker
    {
        private readonly IRemoteConfigService _remoteConfigService;
        private readonly IEventBus _eventBus;

        public bool ForceUpdateRequired { get; private set; }
        public string UpdateMessage { get; private set; } = string.Empty;
        public string UpdateUrl { get; private set; } = string.Empty;

        public ForceUpdateChecker(IRemoteConfigService configService, IEventBus eventBus)
        {
            _remoteConfigService = configService;
            _eventBus = eventBus;
        }

        public async UniTask<bool> CheckForUpdateAsync()
        {
            try
            {
                GameLogger.Log("[ForceUpdateChecker] Checking for force update...");

                // Try ForceUpdateConfig first
                if (_remoteConfigService.TryGetConfig<ForceUpdateConfig>(out var forceUpdateConfig) && forceUpdateConfig != null)
                {
                    return EvaluateVersion(forceUpdateConfig.MinimumVersion, forceUpdateConfig.UpdateMessage, forceUpdateConfig.UpdateUrl);
                }

                // Fallback: check GameSettingsConfig.MinimumAppVersion
                if (_remoteConfigService.TryGetConfig<GameSettingsConfig>(out var gameSettings) && gameSettings != null)
                {
                    return EvaluateVersion(gameSettings.MinimumAppVersion, "A new version is available. Please update to continue.", "");
                }

                GameLogger.Log("[ForceUpdateChecker] No version config found, skipping check");
                ForceUpdateRequired = false;
                return false;
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"[ForceUpdateChecker] Check failed: {ex.Message}");
                ForceUpdateRequired = false;
                return false;
            }
        }

        private bool EvaluateVersion(string minimumVersion, string message, string url)
        {
            if (string.IsNullOrEmpty(minimumVersion))
            {
                ForceUpdateRequired = false;
                return false;
            }

            string currentVersion = UnityEngine.Application.version;
            bool updateRequired = CompareVersions(currentVersion, minimumVersion) < 0;

            ForceUpdateRequired = updateRequired;
            UpdateMessage = message;
            UpdateUrl = url;

            if (updateRequired)
            {
                GameLogger.LogInfo($"[ForceUpdateChecker] Update required: current={currentVersion}, minimum={minimumVersion}");
                _eventBus?.Publish(new ForceUpdateEvent
                {
                    Title = "Update Required",
                    Message = message,
                    IsRequired = true
                });
            }
            else
            {
                GameLogger.Log($"[ForceUpdateChecker] Version OK: current={currentVersion}, minimum={minimumVersion}");
            }

            return updateRequired;
        }

        /// <summary>
        /// Compares two semantic version strings. Returns negative if a &lt; b, 0 if equal, positive if a &gt; b.
        /// </summary>
        internal static int CompareVersions(string a, string b)
        {
            var partsA = ParseVersion(a);
            var partsB = ParseVersion(b);

            for (int i = 0; i < 3; i++)
            {
                int diff = partsA[i] - partsB[i];
                if (diff != 0) return diff;
            }
            return 0;
        }

        private static int[] ParseVersion(string version)
        {
            var result = new int[3];
            if (string.IsNullOrEmpty(version)) return result;

            var parts = version.Split('.');
            for (int i = 0; i < Math.Min(parts.Length, 3); i++)
            {
                int.TryParse(parts[i], out result[i]);
            }
            return result;
        }
    }
}
