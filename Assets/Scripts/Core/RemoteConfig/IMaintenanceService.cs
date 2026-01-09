using Core.Shared.Interfaces;
using Cysharp.Threading.Tasks;

namespace Core.RemoteConfig
{
    public interface IMaintenanceService : IInitializable
    {
        UniTask<bool> CheckMaintenanceStatusAsync();

        void ShowServerDownMaintenance(string error);
    }
}
