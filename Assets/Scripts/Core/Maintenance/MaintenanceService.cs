using System;
using Core.Bootstrap;
using Core.Events;
using Core.Logging;
using Core.RemoteConfig;
using Core.RemoteConfig.Models;
using Cysharp.Threading.Tasks;

namespace Core.Maintenance
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly IEventBus _eventBus;
        private readonly IRemoteConfigService _remoteConfigService;
        private bool _isChecking;

        public MaintenanceService(IEventBus eventBus, IRemoteConfigService remoteConfigService)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _remoteConfigService = remoteConfigService;
        }

        /// <summary>
        /// Called after all services are constructed.
        /// </summary>
        public void Initialize()
        {
            // MaintenanceService doesn't need cross-service setup
        }

        public async UniTask<bool> CheckMaintenanceStatusAsync()
        {
            if (_isChecking)
            {
                GameLogger.LogWarning("Already checking maintenance status");
                return false;
            }

            _isChecking = true;

            try
            {
                GameLogger.Log("Checking maintenance status from Firebase...");

                if (!_remoteConfigService.TryGetConfig<MaintenanceConfig>(out var maintenanceConfig))
                {
                    await _remoteConfigService.RefreshConfig<MaintenanceConfig>();
                    _remoteConfigService.TryGetConfig(out maintenanceConfig);
                }

                if (maintenanceConfig == null)
                {
                    GameLogger.Log("No maintenance config found - service is operational");
                    _isChecking = false;
                    return false;
                }

                if (!maintenanceConfig.IsMaintenanceActive)
                {
                    GameLogger.Log("Maintenance is not active - service is operational");
                    _isChecking = false;
                    return false;
                }

                DateTime serverTime = NTPTime.UtcNow;
                bool isInWindow = maintenanceConfig.IsInMaintenanceWindow(serverTime);

                if (isInWindow)
                {
                    GameLogger.Log("Maintenance mode is active");
                    PublishMaintenanceEvent(maintenanceConfig);
                    _isChecking = false;
                    return true;
                }

                GameLogger.Log("Maintenance scheduled but not in window yet");
                _isChecking = false;
                return false;
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Exception during maintenance check: {ex.Message}");
                _eventBus?.Publish(new MaintenanceCheckFailedEvent
                {
                    Error = $"Maintenance check error: {ex.Message}"
                });
                _isChecking = false;
                return false;
            }
        }

        public void ShowServerDownMaintenance(string error)
        {
            GameLogger.Log($"Showing server down maintenance: {error}");

            _eventBus?.Publish(new MaintenanceModeEvent
            {
                IsMaintenanceMode = true,
                EstimatedEndTime = null,
                Message = "We're experiencing technical difficulties. Please try again later.",
                AllowRetry = true
            });
        }

        private void PublishMaintenanceEvent(MaintenanceConfig config)
        {
            GameLogger.Log($"Publishing maintenance mode event: {config.MaintenanceMessage}");

            _eventBus?.Publish(new MaintenanceModeEvent
            {
                IsMaintenanceMode = true,
                EstimatedEndTime = config.GetEndTime().ToString("O"),
                Message = config.MaintenanceMessage,
                AllowRetry = true
            });
        }
    }
}
