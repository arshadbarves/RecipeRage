using System;
using System.Threading.Tasks;

namespace KitchenClash.Domain
{
    public interface IMatchmakingService
    {
        Task<bool> StartMatchmakingAsync(string queueId);
        Task CancelMatchmakingAsync();
        bool IsSearching { get; }
        event Action OnMatchFound;
        event Action<string> OnMatchmakingFailed;
    }
}
