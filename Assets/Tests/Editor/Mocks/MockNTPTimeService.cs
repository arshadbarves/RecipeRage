using System;
using Core.RemoteConfig;
using Cysharp.Threading.Tasks;

namespace Tests.Editor.Mocks
{
    public class MockNTPTimeService : INTPTimeService
    {
        public bool IsSynced => true;
        public DateTime LastSyncTime => DateTime.UtcNow;

        public void Initialize() { }
        public UniTask<bool> SyncTime() => UniTask.FromResult(true);
        public DateTime GetServerTime() => DateTime.UtcNow;
        public TimeSpan GetTimeOffset() => TimeSpan.Zero;
    }
}
