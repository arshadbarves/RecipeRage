using System.Threading;
using System.Threading.Tasks;

namespace Playcenter.SDK
{
    /// <summary>Warms up the auth stack without triggering a login flow.</summary>
    public sealed class AuthWarmupModule : IPlaycenterModule
    {
        public string Id => "auth_warmup";
        public float Weight => 0.15f;

        public Task<ModuleResult> InitializeAsync(ModuleContext context, CancellationToken ct)
        {
            // Intentionally does NOT call LoginAsGuestAsync or any login method.
            context.Progress.Report(Id, 1f);
            return Task.FromResult(ModuleResult.Ok());
        }
    }
}
