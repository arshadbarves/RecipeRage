using System;
using Cysharp.Threading.Tasks;

namespace KitchenClash.Application.Services
{
    public interface INTPTimeService
    {
        UniTask<bool> SyncTime();
        DateTime GetServerTime();
        TimeSpan GetTimeOffset();
        bool IsSynced { get; }
        DateTime LastSyncTime { get; }
    }
}
