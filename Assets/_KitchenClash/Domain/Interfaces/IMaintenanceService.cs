using System.Threading.Tasks;

namespace KitchenClash.Domain
{
    public interface IMaintenanceService
    {
        bool IsInMaintenance { get; }
        string MaintenanceMessage { get; }
        Task<bool> CheckMaintenanceStatusAsync();
    }
}
