using System;
using System.Threading.Tasks;
using Playcenter.Services;
using Playcenter.Shell;

namespace KitchenClash.Infrastructure.Services
{
    /// <summary>
    /// Fallback remote config that returns defaults when no cloud provider is available.
    /// </summary>
    public sealed class FallbackRemoteConfigService : IRemoteConfigService
    {
        public ConfigHealthStatus HealthStatus => ConfigHealthStatus.Healthy;
        public DateTime LastUpdateTime => DateTime.MinValue;

        public Task<bool> Initialize()
        {
            GameLogger.Log("[FallbackRemoteConfigService] Initialized with defaults");
            return Task.FromResult(true);
        }

        public T GetConfig<T>() where T : class, IConfigModel => default;

        public bool TryGetConfig<T>(out T config) where T : class, IConfigModel
        {
            config = default;
            return false;
        }

        public Task<bool> RefreshConfig() => Task.FromResult(true);

        public Task<bool> RefreshConfig<T>() where T : class, IConfigModel => Task.FromResult(true);
    }
}
