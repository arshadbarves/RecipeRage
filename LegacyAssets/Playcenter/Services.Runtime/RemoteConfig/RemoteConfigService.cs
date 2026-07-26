using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Playcenter.Services
{
    /// <summary>
    /// Shared remote config: typed-model cache + fallback + health, engine-free.
    /// Change notification via plain C# events; games bridge to their own event bus.
    /// Implements both <see cref="IRemoteConfigService"/> and <see cref="IConfigService"/>.
    /// </summary>
    public sealed class RemoteConfigService : IRemoteConfigService, IConfigService
    {
        private readonly IConfigProvider _provider;
        private readonly Dictionary<Type, IConfigModel> _cache = new();
        private readonly Dictionary<string, object> _rawCache = new();
        private ConfigHealthStatus _healthStatus = ConfigHealthStatus.Failed;
        private bool _isInitialized;

        public event Action<IConfigModel> OnConfigUpdated;
        public event Action<ConfigHealthStatus> OnHealthChanged;

        public ConfigHealthStatus HealthStatus => _healthStatus;
        public DateTime LastUpdateTime { get; private set; } = DateTime.MinValue;

        public RemoteConfigService(IConfigProvider provider = null)
        {
            _provider = provider ?? new FallbackConfigProvider();
        }

        public async Task<bool> Initialize()
        {
            if (_isInitialized)
            {
                return true;
            }

            bool ok = false;
            try
            {
                ok = await _provider.Initialize() && _provider.IsAvailable();
            }
            catch (Exception)
            {
                ok = false;
            }

            _isInitialized = true;
            SetHealth(ok ? ConfigHealthStatus.Healthy : ConfigHealthStatus.Degraded);
            return true; // never block boot on config
        }

        public T GetConfig<T>() where T : class, IConfigModel
        {
            return _cache.TryGetValue(typeof(T), out IConfigModel cached) ? cached as T : default;
        }

        public bool TryGetConfig<T>(out T config) where T : class, IConfigModel
        {
            config = GetConfig<T>();
            return config != null;
        }

        public async Task<bool> RefreshConfig()
        {
            if (!_isInitialized)
            {
                return false;
            }

            try
            {
                Dictionary<string, IConfigModel> configs = await _provider.FetchAllConfigs();
                if (configs != null && configs.Count > 0)
                {
                    foreach (KeyValuePair<string, IConfigModel> kvp in configs)
                    {
                        if (kvp.Value != null && kvp.Value.Validate())
                        {
                            _cache[kvp.Value.GetType()] = kvp.Value;
                            OnConfigUpdated?.Invoke(kvp.Value);
                        }
                    }
                    LastUpdateTime = DateTime.UtcNow;
                    SetHealth(ConfigHealthStatus.Healthy);
                    return true;
                }
            }
            catch (Exception)
            {
                // fall through to degraded
            }

            SetHealth(ConfigHealthStatus.Degraded);
            return false;
        }

        public async Task<bool> RefreshConfig<T>() where T : class, IConfigModel
        {
            if (!_isInitialized)
            {
                return false;
            }

            try
            {
                T config = await _provider.FetchConfig<T>(typeof(T).Name);
                if (config != null && config.Validate())
                {
                    _cache[typeof(T)] = config;
                    OnConfigUpdated?.Invoke(config);
                    LastUpdateTime = DateTime.UtcNow;
                    return true;
                }
            }
            catch (Exception)
            {
                SetHealth(ConfigHealthStatus.Degraded);
            }
            return false;
        }

        public T Get<T>(string key, T fallback)
        {
            if (_rawCache.TryGetValue(key, out object cached))
            {
                try { return (T)Convert.ChangeType(cached, typeof(T)); }
                catch { /* fall through */ }
            }
            return fallback;
        }

        public Task FetchAsync() => RefreshConfig();

        private void SetHealth(ConfigHealthStatus status)
        {
            if (_healthStatus != status)
            {
                _healthStatus = status;
                OnHealthChanged?.Invoke(status);
            }
        }
    }
}
