using System;
using Core.RemoteConfig;
using Core.RemoteConfig.Models;
using Cysharp.Threading.Tasks;

namespace Tests.Editor.Mocks
{
    public class MockRemoteConfigService : IRemoteConfigService
    {
        public ConfigHealthStatus HealthStatus => ConfigHealthStatus.Healthy;
        public DateTime LastUpdateTime => DateTime.UtcNow;

        public event Action<IConfigModel> OnConfigUpdated;
        public event Action<Type, IConfigModel> OnSpecificConfigUpdated;
        public event Action<ConfigHealthStatus> OnHealthStatusChanged;

        public void Initialize() { }
        UniTask<bool> IRemoteConfigService.Initialize() => UniTask.FromResult(true);
        public T GetConfig<T>() where T : class, IConfigModel => null;
        public bool TryGetConfig<T>(out T config) where T : class, IConfigModel { config = null; return false; }
        public UniTask<bool> RefreshConfig() => UniTask.FromResult(true);
        public UniTask<bool> RefreshConfig<T>() where T : class, IConfigModel => UniTask.FromResult(true);
    }
}
