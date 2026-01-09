using System;
using System.Collections.Generic;
using Core.Core.Logging;
using Core.Core.RemoteConfig.Enums;
using Core.Core.RemoteConfig.Interfaces;
using Core.Core.Shared.Interfaces;
using Cysharp.Threading.Tasks;
using VContainer;

namespace Core.Core.RemoteConfig.Services
{
    public class RemoteConfigService : IRemoteConfigService
    {
        private readonly Dictionary<Type, IConfigModel> _configCache;
        private readonly IConfigProvider _firebaseProvider;

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
            _firebaseProvider = configProvider;
            _configCache = new Dictionary<Type, IConfigModel>();
            _healthStatus = ConfigHealthStatus.Failed;
            _lastUpdateTime = DateTime.MinValue;
            _isInitialized = false;
        }

        /// <summary>
        /// Called after all services are constructed (IInitializable).
        /// </summary>
        void IInitializable.Initialize()
        {
            // RemoteConfigService uses async initialization via Initialize() method
        }

        public async UniTask<bool> Initialize()
        {
            try
            {
                GameLogger.Log($"Initializing RemoteConfigService with provider: {_firebaseProvider.ProviderName}...");

                // Initialize provider
                bool providerSuccess = await _firebaseProvider.Initialize();

                if (!providerSuccess || !_firebaseProvider.IsAvailable())
                {
                    GameLogger.LogError("Config provider failed to initialize");
                    UpdateHealthStatus(ConfigHealthStatus.Failed);
                    return false;
                }

                GameLogger.Log("Config provider initialized successfully");

                // Fetch initial configuration
                bool fetchSuccess = await FetchAllConfigsAsync();

                if (fetchSuccess)
                {
                    UpdateHealthStatus(ConfigHealthStatus.Healthy);
                    _isInitialized = true;
                    GameLogger.Log("RemoteConfigService initialized successfully");
                    return true;
                }
                else
                {
                    UpdateHealthStatus(ConfigHealthStatus.Degraded);
                    GameLogger.LogWarning("RemoteConfigService initialized with degraded status");
                    return true; // Still return true as service is functional
                }
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
            if (_configCache.TryGetValue(typeof(T), out var config))
            {
                return config as T;
            }

            GameLogger.LogWarning($"Config of type {typeof(T).Name} not found in cache");
            return null;
        }

        public bool TryGetConfig<T>(out T config) where T : class, IConfigModel
        {
            config = GetConfig<T>();
            return config != null;
        }

        public async UniTask<bool> RefreshConfig()
        {
            if (!_isInitialized || _firebaseProvider == null)
            {
                GameLogger.LogError("Cannot refresh config: Service not initialized");
                return false;
            }

            try
            {
                GameLogger.Log("Refreshing all configurations...");
                return await FetchAllConfigsAsync();
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Failed to refresh configurations: {ex.Message}");
                UpdateHealthStatus(ConfigHealthStatus.Degraded);
                return false;
            }
        }

        public async UniTask<bool> RefreshConfig<T>() where T : class, IConfigModel
        {
            if (!_isInitialized || _firebaseProvider == null)
            {
                GameLogger.LogError("Cannot refresh config: Service not initialized");
                return false;
            }

            try
            {
                GameLogger.Log($"Refreshing configuration: {typeof(T).Name}");

                string configKey = GetConfigKey<T>();
                var config = await _firebaseProvider.FetchConfig<T>(configKey);

                if (config != null && config.Validate())
                {
                    UpdateConfig(config);
                    _lastUpdateTime = NTPTime.UtcNow;
                    UpdateHealthStatus(ConfigHealthStatus.Healthy);
                    GameLogger.Log($"Successfully refreshed config: {typeof(T).Name}");
                    return true;
                }
                else
                {
                    GameLogger.LogWarning($"Failed to refresh config: {typeof(T).Name}");
                    UpdateHealthStatus(ConfigHealthStatus.Degraded);
                    return false;
                }
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Failed to refresh config {typeof(T).Name}: {ex.Message}");
                UpdateHealthStatus(ConfigHealthStatus.Degraded);
                return false;
            }
        }

        private async UniTask<bool> FetchAllConfigsAsync()
        {
            try
            {
                Dictionary<string, IConfigModel> configs = await _firebaseProvider.FetchAllConfigs();

                if (configs == null || configs.Count == 0)
                {
                    GameLogger.LogWarning("No configurations fetched from provider");
                    return false;
                }

                int validCount = 0;
                foreach (var kvp in configs)
                {
                    if (kvp.Value != null && kvp.Value.Validate())
                    {
                        UpdateConfig(kvp.Value);
                        validCount++;
                    }
                    else
                    {
                        GameLogger.LogWarning($"Config validation failed: {kvp.Key}");
                    }
                }

                _lastUpdateTime = NTPTime.UtcNow;
                GameLogger.Log($"Fetched and validated {validCount}/{configs.Count} configurations");

                return validCount > 0;
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Failed to fetch all configs: {ex.Message}");
                return false;
            }
        }

        private void UpdateConfig(IConfigModel config)
        {
            if (config == null)
            {
                return;
            }

            var configType = config.GetType();

            // Check if config has changed
            bool hasChanged = true;
            if (_configCache.TryGetValue(configType, out var existingConfig))
            {
                // Simple reference equality check - Firebase handles versioning
                hasChanged = !ReferenceEquals(existingConfig, config);
            }

            _configCache[configType] = config;

            if (hasChanged)
            {
                GameLogger.Log($"Config updated: {configType}");
                OnConfigUpdated?.Invoke(config);
                OnSpecificConfigUpdated?.Invoke(configType, config);
            }
        }

        private void UpdateHealthStatus(ConfigHealthStatus newStatus)
        {
            if (_healthStatus != newStatus)
            {
                var previousStatus = _healthStatus;
                _healthStatus = newStatus;
                GameLogger.Log($"Health status changed: {previousStatus} -> {newStatus}");
                OnHealthStatusChanged?.Invoke(newStatus);
            }
        }

        private string GetConfigKey<T>() where T : IConfigModel
        {
            var typeName = typeof(T).Name;

            // Remove "Config" suffix if present
            if (typeName.EndsWith("Config"))
            {
                return typeName;
            }

            return typeName;
        }
    }
}
