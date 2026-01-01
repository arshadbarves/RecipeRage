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
        private readonly ILoggingService _logger;
        private bool _isChecking;

        public MaintenanceService(IEventBus eventBus, IRemoteConfigService remoteConfigService, ILoggingService logger)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _remoteConfigService = remoteConfigService;
            _logger = logger;
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
                _logger.LogWarning("Already checking maintenance status", "Maintenance");
                return false;
            }

            _isChecking = true;

            try
            {
                _logger.LogInfo("Checking maintenance status from Firebase...", "Maintenance");

                if (!_remoteConfigService.TryGetConfig<MaintenanceConfig>(out var maintenanceConfig))
                {
                    await _remoteConfigService.RefreshConfig<MaintenanceConfig>();
                    _remoteConfigService.TryGetConfig(out maintenanceConfig);
                }

                if (maintenanceConfig == null)
                {
                    _logger.LogInfo("No maintenance config found - service is operational", "Maintenance");
                    _isChecking = false;
                    return false;
                }

                if (!maintenanceConfig.IsMaintenanceActive)
                {
                    _logger.LogInfo("Maintenance is not active - service is operational", "Maintenance");
                    _isChecking = false;
                    return false;
                }

                DateTime serverTime = NTPTime.UtcNow;
                bool isInWindow = maintenanceConfig.IsInMaintenanceWindow(serverTime);

                if (isInWindow)
                {
                    _logger.LogInfo("Maintenance mode is active", "Maintenance");
                    PublishMaintenanceEvent(maintenanceConfig);
                    _isChecking = false;
                    return true;
                }

                _logger.LogInfo("Maintenance scheduled but not in window yet", "Maintenance");
                _isChecking = false;
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception during maintenance check: {ex.Message}", "Maintenance");
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
            _logger.LogInfo($"Showing server down maintenance: {error}", "Maintenance");

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
            _logger.LogInfo($"Publishing maintenance mode event: {config.MaintenanceMessage}", "Maintenance");

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
