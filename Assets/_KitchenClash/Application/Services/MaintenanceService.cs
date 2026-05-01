using System;
using System.Threading.Tasks;
using KitchenClash.Domain;

namespace KitchenClash.Application.Services
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly IEventBus _eventBus;
        private readonly IRemoteConfigService _remoteConfigService;

        public bool IsInMaintenance { get; private set; }
        public string MaintenanceMessage { get; private set; } = string.Empty;

        public MaintenanceService(IEventBus eventBus, IRemoteConfigService remoteConfigService)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _remoteConfigService = remoteConfigService;
        }

        public async Task<bool> CheckMaintenanceStatusAsync()
        {
            // Simplified - check remote config for maintenance flag
            GameLogger.LogInfo("Checking maintenance status...");
            return false;
        }

        public void ShowServerDownMaintenance(string error)
        {
            GameLogger.LogInfo($"Showing server down maintenance: {error}");
        }
    }
}
