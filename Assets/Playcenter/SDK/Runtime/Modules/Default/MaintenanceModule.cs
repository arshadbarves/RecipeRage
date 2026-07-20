using System.Threading;
using System.Threading.Tasks;
using Playcenter.Services;

namespace Playcenter.SDK
{
    public sealed class MaintenanceModule : IPlaycenterModule
    {
        public string Id => "maintenance";
        public float Weight => 0.10f;

        public async Task<ModuleResult> InitializeAsync(ModuleContext context, CancellationToken ct)
        {
            if (!context.Services.TryGet<IMaintenanceService>(out var maintenance))
            {
                context.Progress.Report(Id, 1f);
                return ModuleResult.Ok();
            }

            bool statusFromCheck = false;
            try
            {
                context.Progress.Report(Id, 0.5f);
                statusFromCheck = await maintenance.CheckMaintenanceStatusAsync();
            }
            catch
            {
                // Fall through and check the cached IsInMaintenance property.
            }

            if (statusFromCheck || maintenance.IsInMaintenance)
            {
                context.Progress.Report(Id, 1f);
                return ModuleResult.Fail(BootFailureCode.Maintenance,
                    maintenance.MaintenanceMessage ?? "The game is currently under maintenance.");
            }

            context.Progress.Report(Id, 1f);
            return ModuleResult.Ok();
        }
    }
}
