using Core.Maintenance;
using Cysharp.Threading.Tasks;

namespace Tests.Editor.Mocks
{
    public class MockMaintenanceService : IMaintenanceService
    {
        public void Initialize() { }
        public void ShowServerDownMaintenance(string message) { }
        public UniTask<bool> CheckMaintenanceStatusAsync() => UniTask.FromResult(false);
    }
}
