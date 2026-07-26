using System.Threading;
using System.Threading.Tasks;

namespace Playcenter.SDK
{
    public sealed class AnalyticsModule : IPlaycenterModule
    {
        public string Id => "analytics";
        public float Weight => 0.10f;

        public Task<ModuleResult> InitializeAsync(ModuleContext context, CancellationToken ct)
        {
            // Analytics is optional; always succeeds regardless of service presence.
            context.Progress.Report(Id, 1f);
            return Task.FromResult(ModuleResult.Ok());
        }
    }
}
