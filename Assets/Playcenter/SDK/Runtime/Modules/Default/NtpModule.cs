using System.Threading;
using System.Threading.Tasks;
using Playcenter.Services;

namespace Playcenter.SDK
{
    public sealed class NtpModule : IPlaycenterModule
    {
        public string Id => "ntp";
        public float Weight => 0.10f;

        public async Task<ModuleResult> InitializeAsync(ModuleContext context, CancellationToken ct)
        {
            if (context.Services.TryGet<INTPTimeService>(out var ntp))
            {
                // Best-effort sync with 5-second timeout; never fail boot.
                var syncTask = ntp.SyncTime();
                var timeoutTask = Task.Delay(5000, ct);
                await Task.WhenAny(syncTask, timeoutTask);
            }
            context.Progress.Report(Id, 1f);
            return ModuleResult.Ok();
        }
    }
}
