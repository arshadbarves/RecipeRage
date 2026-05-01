using System;
using System.Collections.Generic;
using KitchenClash.Infrastructure.Logging;
using KitchenClash.Domain.Enums;
using KitchenClash.Domain.Interfaces;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;

namespace KitchenClash.Application.Services
{
    public class RemoteConfigService : IRemoteConfigService, IStartable
    {
        private readonly Dictionary<Type, IConfigModel> _configCache;
        private readonly IConfigProvider _provider;

        private ConfigHealthStatus _healthStatus;
        private DateTime _lastUpdateTime;
        private bool _isInitialized;

        public ConfigHealthStatus HealthStatus => _healthStatus;
        public DateTime LastUpdateTime => _lastUpdateTime;

        public event Action<IConfigModel> OnConfigUpdated;
        public event Action<Type, IConfigModel> OnSpecificConfigUpdated;
        public event Action<ConfigHealthStatus> OnHealthStatusChanged;

        [Inject]
        public RemoteConfigService(IConfigProvider configProvider)
        {
            _provider = configProvider;
            _configCache = new Dictionary<Type, IConfigModel>();
            _healthStatus = ConfigHealthStatus.Failed;
        }

        public void Start() => Initialize().Forget();

        public async UniTask<bool> Initialize()
        {
            try
            {
                bool success = await _provider.Initialize();
                if (!success || !_provider.IsAvailable())
                {
                    UpdateHealthStatus(ConfigHealthStatus.Failed);
                    return false;
                }

                bool fetchSuccess = await FetchAllConfigsAsync();
                UpdateHealthStatus(fetchSuccess ? ConfigHealthStatus.Healthy : ConfigHealthStatus.Degraded);
                _isInitialized = true;
                return true;
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Failed to initialize RemoteConfigService: {ex.Message}");
                UpdateHealthStatus(ConfigHealthStatus.Failed);
                return false;
            }
        }

        public T GetConfig<T>() where T : class, IConfigModel
        {
            return _configCache.TryGetValue(typeof(T), out var config) ? config as T : null;
        }

        public bool TryGetConfig<T>(out T config) where T : class, IConfigModel
        {
            config = GetConfig<T>();
            return config != null;
        }

        public async UniTask<bool> RefreshConfig()
        {
            if (!_isInitialized) return false;
            return await FetchAllConfigsAsync();
        }

        public async UniTask<bool> RefreshConfig<T>() where T : class, IConfigModel
        {
            if (!_isInitialized) return false;
            var config = await _provider.FetchConfig<T>(typeof(T).Name);
            if (config != null && config.Validate())
            {
                _configCache[typeof(T)] = config;
                OnConfigUpdated?.Invoke(config);
                OnSpecificConfigUpdated?.Invoke(typeof(T), config);
                return true;
            }
            return false;
        }

        private async UniTask<bool> FetchAllConfigsAsync()
        {
            var configs = await _provider.FetchAllConfigs();
            if (configs == null || configs.Count == 0) return false;

            int validCount = 0;
            foreach (var kvp in configs)
            {
                if (kvp.Value != null && kvp.Value.Validate())
                {
                    _configCache[kvp.Value.GetType()] = kvp.Value;
                    OnConfigUpdated?.Invoke(kvp.Value);
                    OnSpecificConfigUpdated?.Invoke(kvp.Value.GetType(), kvp.Value);
                    validCount++;
                }
            }
            _lastUpdateTime = DateTime.UtcNow;
            return validCount > 0;
        }

        private void UpdateHealthStatus(ConfigHealthStatus newStatus)
        {
            if (_healthStatus != newStatus)
            {
                _healthStatus = newStatus;
                OnHealthStatusChanged?.Invoke(newStatus);
            }
        }
    }
}
