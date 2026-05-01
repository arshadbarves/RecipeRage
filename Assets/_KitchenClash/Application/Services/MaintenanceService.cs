using System;
using KitchenClash.Infrastructure.Logging;
using KitchenClash.Domain.Interfaces;
using Cysharp.Threading.Tasks;

namespace KitchenClash.Application.Services
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly IEventBus _eventBus;
        private readonly IRemoteConfigService _remoteConfigService;

        public MaintenanceService(IEventBus eventBus, IRemoteConfigService remoteConfigService)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _remoteConfigService = remoteConfigService;
        }

        public async UniTask<bool> CheckMaintenanceStatusAsync()
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

    public interface IMaintenanceService
    {
        UniTask<bool> CheckMaintenanceStatusAsync();
        void ShowServerDownMaintenance(string error);
    }
}
