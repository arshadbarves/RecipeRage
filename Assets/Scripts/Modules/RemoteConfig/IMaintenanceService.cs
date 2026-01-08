using Core.Bootstrap;
using Cysharp.Threading.Tasks;

namespace Modules.RemoteConfig
{
    public interface IMaintenanceService : IInitializable
    {
        UniTask<bool> CheckMaintenanceStatusAsync();

        void ShowServerDownMaintenance(string error);
    }
}
