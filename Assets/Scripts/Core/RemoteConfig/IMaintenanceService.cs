using Core.Core.Shared.Interfaces;
using Cysharp.Threading.Tasks;

namespace Core.Core.RemoteConfig
{
    public interface IMaintenanceService : IInitializable
    {
        UniTask<bool> CheckMaintenanceStatusAsync();

        void ShowServerDownMaintenance(string error);
    }
}
