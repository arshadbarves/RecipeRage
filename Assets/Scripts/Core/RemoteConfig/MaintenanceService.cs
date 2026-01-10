using System;
using Core.Logging;
using Core.RemoteConfig.Interfaces;
using Core.RemoteConfig.Models;
using Core.RemoteConfig.Services;
using Core.Shared.Events;
using Cysharp.Threading.Tasks;

namespace Core.RemoteConfig
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
                GameLogger.LogInfo("Checking maintenance status from Firebase...");

                if (!_remoteConfigService.TryGetConfig<MaintenanceConfig>(out var maintenanceConfig))
                {
                    await _remoteConfigService.RefreshConfig<MaintenanceConfig>();
                    _remoteConfigService.TryGetConfig(out maintenanceConfig);
                }

                if (maintenanceConfig == null)
                {
                    GameLogger.LogInfo("No maintenance config found - service is operational");
                    _isChecking = false;
                    return false;
                }

                if (!maintenanceConfig.IsMaintenanceActive)
                {
                    GameLogger.LogInfo("Maintenance is not active - service is operational");
                    _isChecking = false;
                    return false;
                }

                DateTime serverTime = NTPTime.UtcNow;
                bool isInWindow = maintenanceConfig.IsInMaintenanceWindow(serverTime);

                if (isInWindow)
                {
                    GameLogger.LogInfo("Maintenance mode is active");
                    PublishMaintenanceEvent(maintenanceConfig);
                    _isChecking = false;
                    return true;
                }

                GameLogger.LogInfo("Maintenance scheduled but not in window yet");
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
            GameLogger.LogInfo($"Showing server down maintenance: {error}");

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
            GameLogger.LogInfo($"Publishing maintenance mode event: {config.MaintenanceMessage}");

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