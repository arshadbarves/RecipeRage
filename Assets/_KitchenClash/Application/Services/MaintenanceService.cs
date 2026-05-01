using System;
using System.Globalization;
using System.Threading.Tasks;
using KitchenClash.Domain;
using KitchenClash.Application.Models.RemoteConfigs;

namespace KitchenClash.Application.Services
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly IEventBus _eventBus;
        private readonly IRemoteConfigService _remoteConfigService;

        public bool IsInMaintenance { get; private set; }
        public string MaintenanceMessage { get; private set; } = string.Empty;
        public DateTime? EstimatedEndTime { get; private set; }

        public MaintenanceService(IEventBus eventBus, IRemoteConfigService remoteConfigService)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _remoteConfigService = remoteConfigService;
        }

        public async Task<bool> CheckMaintenanceStatusAsync()
        {
            GameLogger.LogInfo("Checking maintenance status...");

            try
            {
                if (_remoteConfigService == null)
                {
                    IsInMaintenance = false;
                    return false;
                }

                if (_remoteConfigService.TryGetConfig<MaintenanceConfig>(out var config) && config != null)
                {
                    IsInMaintenance = config.IsEnabled;
                    MaintenanceMessage = config.Message ?? "We are currently performing maintenance.";

                    if (!string.IsNullOrEmpty(config.EstimatedEndTimeUtc))
                    {
                        try
                        {
                            EstimatedEndTime = DateTime.Parse(config.EstimatedEndTimeUtc, null, DateTimeStyles.RoundtripKind);
                        }
                        catch
                        {
                            EstimatedEndTime = null;
                        }
                    }
                    else
                    {
                        EstimatedEndTime = null;
                    }

                    if (IsInMaintenance)
                    {
                        GameLogger.LogInfo($"[MaintenanceService] Maintenance active: {MaintenanceMessage}");
                        _eventBus.Publish(new MaintenanceModeEvent
                        {
                            IsMaintenanceMode = true,
                            Message = MaintenanceMessage,
                            EstimatedEndTime = config.EstimatedEndTimeUtc,
                            AllowRetry = config.AllowRetry
                        });
                    }
                    else
                    {
                        _eventBus.Publish(new MaintenanceModeEvent
                        {
                            IsMaintenanceMode = false,
                            Message = string.Empty
                        });
                    }

                    return IsInMaintenance;
                }

                // No config available — assume no maintenance
                IsInMaintenance = false;
                return false;
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"[MaintenanceService] Check failed: {ex.Message}");
                _eventBus.Publish(new MaintenanceCheckFailedEvent { Error = ex.Message });
                IsInMaintenance = false;
                return false;
            }
        }

        public void ShowServerDownMaintenance(string error)
        {
            GameLogger.LogInfo($"Showing server down maintenance: {error}");
            IsInMaintenance = true;
            MaintenanceMessage = error;
            _eventBus.Publish(new MaintenanceModeEvent
            {
                IsMaintenanceMode = true,
                Message = error,
                AllowRetry = true
            });
        }
    }
}
