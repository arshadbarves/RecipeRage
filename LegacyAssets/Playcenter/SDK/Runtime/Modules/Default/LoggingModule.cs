using System.Threading;
using System.Threading.Tasks;

namespace Playcenter.SDK
{
    public sealed class LoggingModule : IPlaycenterModule
    {
        public string Id => "logging";
        public float Weight => 0.05f;

        public Task<ModuleResult> InitializeAsync(ModuleContext context, CancellationToken ct)
        {
            context.Progress.Report(Id, 1f);
            return Task.FromResult(ModuleResult.Ok());
        }
    }
}
