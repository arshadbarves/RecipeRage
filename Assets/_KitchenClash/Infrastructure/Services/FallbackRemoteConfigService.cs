using System;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using Cysharp.Threading.Tasks;

namespace KitchenClash.Infrastructure.Services
{
    /// <summary>
    /// Fallback remote config that returns defaults when no cloud provider is available.
    /// </summary>
    public sealed class FallbackRemoteConfigService : IRemoteConfigService
    {
        public ConfigHealthStatus HealthStatus => ConfigHealthStatus.Healthy;
        public DateTime LastUpdateTime => DateTime.MinValue;

        public event Action<IConfigModel> OnConfigUpdated;
        public event Action<Type, IConfigModel> OnSpecificConfigUpdated;
        public event Action<ConfigHealthStatus> OnHealthStatusChanged;

        public UniTask<bool> Initialize()
        {
            GameLogger.Log("[FallbackRemoteConfigService] Initialized with defaults");
            return UniTask.FromResult(true);
        }

        public T GetConfig<T>() where T : class, IConfigModel => default;

        public bool TryGetConfig<T>(out T config) where T : class, IConfigModel
        {
            config = default;
            return false;
        }

        public UniTask<bool> RefreshConfig() => UniTask.FromResult(true);

        public UniTask<bool> RefreshConfig<T>() where T : class, IConfigModel => UniTask.FromResult(true);
    }
}
