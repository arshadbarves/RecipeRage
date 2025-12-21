using Core.Bootstrap;
using Cysharp.Threading.Tasks;

namespace Core.Maintenance
{
    public interface IMaintenanceService : IInitializable
    {
        UniTask<bool> CheckMaintenanceStatusAsync();

        void ShowServerDownMaintenance(string error);
    }
}
