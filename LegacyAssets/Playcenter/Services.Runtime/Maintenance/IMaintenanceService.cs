using System;
using System.Threading.Tasks;

namespace Playcenter.Services
{
    public interface IMaintenanceService
    {
        bool IsInMaintenance { get; }
        string MaintenanceMessage { get; }
        DateTime? EstimatedEndTime { get; }
        Task<bool> CheckMaintenanceStatusAsync();
    }
}
