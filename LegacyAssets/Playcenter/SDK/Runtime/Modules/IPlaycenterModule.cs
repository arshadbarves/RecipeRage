using System.Threading;
using System.Threading.Tasks;

namespace Playcenter.SDK
{
    public interface IPlaycenterModule
    {
        string Id { get; }
        float Weight { get; }
        Task<ModuleResult> InitializeAsync(ModuleContext context, CancellationToken ct);
    }
}
