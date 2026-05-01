using System;
using System.Threading.Tasks;

namespace KitchenClash.Domain
{
    public interface IMaintenanceService
    {
        bool IsInMaintenance { get; }
        string MaintenanceMessage { get; }
        DateTime? EstimatedEndTime { get; }
        Task<bool> CheckMaintenanceStatusAsync();
    }
}
