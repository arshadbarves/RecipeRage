using System;
using KitchenClash.Domain;
using Cysharp.Threading.Tasks;

namespace KitchenClash.Application.Services
{
    public interface IRemoteConfigService
    {
        UniTask<bool> Initialize();
        T GetConfig<T>() where T : class, IConfigModel;
        bool TryGetConfig<T>(out T config) where T : class, IConfigModel;
        UniTask<bool> RefreshConfig();
        UniTask<bool> RefreshConfig<T>() where T : class, IConfigModel;
        ConfigHealthStatus HealthStatus { get; }
        DateTime LastUpdateTime { get; }
        event Action<IConfigModel> OnConfigUpdated;
        event Action<Type, IConfigModel> OnSpecificConfigUpdated;
        event Action<ConfigHealthStatus> OnHealthStatusChanged;
    }
}
