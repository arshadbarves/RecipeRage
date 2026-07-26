using System;
using System.Threading.Tasks;

namespace Playcenter.Services
{
    public interface INTPTimeService
    {
        Task<bool> SyncTime();
        DateTime GetServerTime();
        TimeSpan GetTimeOffset();
        bool IsSynced { get; }
        DateTime LastSyncTime { get; }
    }
}
