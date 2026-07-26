using System.Threading;
using System.Threading.Tasks;

namespace Playcenter.SDK
{
    public sealed class ForceUpdateModule : IPlaycenterModule
    {
        public string Id => "force_update";
        public float Weight => 0.10f;

        public async Task<ModuleResult> InitializeAsync(ModuleContext context, CancellationToken ct)
        {
            if (!context.Services.TryGet<IForceUpdatePolicy>(out var policy))
            {
                context.Progress.Report(Id, 1f);
                return ModuleResult.Ok();
            }

            var decision = await policy.EvaluateAsync(ct);
            context.Progress.Report(Id, 1f);

            if (decision.Required)
                return ModuleResult.Fail(BootFailureCode.ForceUpdate,
                    decision.Message ?? "A newer version of the app is required.");

            return ModuleResult.Ok();
        }
    }
}
