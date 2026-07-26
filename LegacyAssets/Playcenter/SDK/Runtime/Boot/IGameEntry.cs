using System.Threading;
using System.Threading.Tasks;

namespace Playcenter.SDK
{
    public interface IGameEntry
    {
        Task OnPlaycenterReadyAsync(PlaycenterClient client, CancellationToken ct);
        Task OnPlaycenterFailedAsync(BootFailure failure, CancellationToken ct);
    }
}
