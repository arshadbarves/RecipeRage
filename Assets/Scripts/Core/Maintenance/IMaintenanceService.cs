using Cysharp.Threading.Tasks;

namespace Core.Maintenance
{
    /// <summary>
    /// Interface for maintenance service
    /// </summary>
    public interface IMaintenanceService
    {
        /// <summary>
        /// Check maintenance status from Title Storage (requires login)
        /// </summary>
        UniTask<bool> CheckMaintenanceStatusAsync();

        /// <summary>
        /// Show maintenance screen for server down scenario (no estimation)
        /// </summary>
        void ShowServerDownMaintenance(string error);

        /// <summary>
        /// Get cached maintenance data
        /// </summary>
        MaintenanceData GetCachedMaintenanceData();
    }
}
