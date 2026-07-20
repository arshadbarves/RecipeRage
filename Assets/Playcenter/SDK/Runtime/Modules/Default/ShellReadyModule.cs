using System.Threading;
using System.Threading.Tasks;

namespace Playcenter.SDK
{
    /// <summary>Signals that the shell UI is ready. Theme is applied before boot starts.</summary>
    public sealed class ShellReadyModule : IPlaycenterModule
    {
        public string Id => "shell_ready";
        public float Weight => 0.10f;

        public Task<ModuleResult> InitializeAsync(ModuleContext context, CancellationToken ct)
        {
            context.Progress.Report(Id, 1f);
            return Task.FromResult(ModuleResult.Ok());
        }
    }
}
