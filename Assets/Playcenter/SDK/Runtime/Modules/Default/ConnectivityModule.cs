using System.Threading;
using System.Threading.Tasks;
using Playcenter.Shell;

namespace Playcenter.SDK
{
    public sealed class ConnectivityModule : IPlaycenterModule
    {
        public string Id => "connectivity";
        public float Weight => 0.15f;

        public Task<ModuleResult> InitializeAsync(ModuleContext context, CancellationToken ct)
        {
            if (!context.Services.TryGet<IConnectivityService>(out var connectivity) || !connectivity.IsOnline)
            {
                context.Progress.Report(Id, 1f);
                return Task.FromResult(ModuleResult.Fail(BootFailureCode.Offline, "Device is offline."));
            }
            context.Progress.Report(Id, 1f);
            return Task.FromResult(ModuleResult.Ok());
        }
    }
}
