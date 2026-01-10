using Cysharp.Threading.Tasks;

namespace Core.RemoteConfig
{
    public interface IMaintenanceService
    {
        UniTask<bool> CheckMaintenanceStatusAsync();

        void ShowServerDownMaintenance(string error);
    }
}
