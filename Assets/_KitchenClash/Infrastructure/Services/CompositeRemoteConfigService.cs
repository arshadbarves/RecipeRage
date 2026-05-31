using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using KitchenClash.Domain.Events;
using Cysharp.Threading.Tasks;

namespace KitchenClash.Infrastructure.Services
{
    public sealed class CompositeRemoteConfigService : IConfigService, IRemoteConfigService
    {
        private readonly Dictionary<Type, IConfigModel> _cache = new();
        private readonly FallbackRemoteConfigService _fallback = new();
        private readonly Dictionary<string, object> _rawCache = new();
        private readonly IEventBus _eventBus;

#if FIREBASE_REMOTE_CONFIG
        private readonly IConfigProvider _firebaseProvider;
        private bool _firebaseAvailable;
#endif

        private ConfigHealthStatus _healthStatus = ConfigHealthStatus.Failed;
        private DateTime _lastUpdateTime = DateTime.MinValue;
        private bool _isInitialized;

        public ConfigHealthStatus HealthStatus => _healthStatus;
        public DateTime LastUpdateTime => _lastUpdateTime;

        public event Action<IConfigModel> OnConfigUpdated;
        public event Action<Type, IConfigModel> OnSpecificConfigUpdated;
        public event Action<ConfigHealthStatus> OnHealthStatusChanged;

#if FIREBASE_REMOTE_CONFIG
        public CompositeRemoteConfigService(IConfigProvider configProvider, IEventBus eventBus)
        {
            _firebaseProvider = configProvider;
            _eventBus = eventBus;
        }
#else
        public CompositeRemoteConfigService(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }
#endif

        public async UniTask<bool> Initialize()
        {
            if (_isInitialized)
            {
                return true;
            }

#if FIREBASE_REMOTE_CONFIG
            try
            {
                GameLogger.Log("[CompositeRemoteConfig] Attempting Firebase initialization...");
                bool success = await _firebaseProvider.Initialize();
                if (success && _firebaseProvider.IsAvailable())
                {
                    _firebaseAvailable = true;
                    _isInitialized = true;
                    UpdateHealthStatus(ConfigHealthStatus.Healthy);
                    GameLogger.Log("[CompositeRemoteConfig] Firebase initialized successfully");
                    return true;
                }
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"[CompositeRemoteConfig] Firebase init failed: {ex.Message}");
            }
#endif
            GameLogger.Log("[CompositeRemoteConfig] Using fallback defaults");
            await _fallback.Initialize();
            _isInitialized = true;
            UpdateHealthStatus(ConfigHealthStatus.Degraded);
            return true;
        }

        public T GetConfig<T>() where T : class, IConfigModel
        {
            if (_cache.TryGetValue(typeof(T), out IConfigModel cached))
            {
                return cached as T;
            }

            return _fallback.GetConfig<T>();
        }

        public bool TryGetConfig<T>(out T config) where T : class, IConfigModel
        {
            config = GetConfig<T>();
            return config != null;
        }

        public async UniTask<bool> RefreshConfig()
        {
            if (!_isInitialized)
            {
                return false;
            }

#if FIREBASE_REMOTE_CONFIG
            if (_firebaseAvailable)
            {
                try
                {
                    Dictionary<string, IConfigModel> configs = await _firebaseProvider.FetchAllConfigs();
                    if (configs != null && configs.Count > 0)
                    {
                        foreach (KeyValuePair<string, IConfigModel> kvp in configs)
                        {
                            if (kvp.Value != null && kvp.Value.Validate())
                            {
                                _cache[kvp.Value.GetType()] = kvp.Value;
                                OnConfigUpdated?.Invoke(kvp.Value);
                                OnSpecificConfigUpdated?.Invoke(kvp.Value.GetType(), kvp.Value);
                                _eventBus.Publish(new ConfigUpdatedEvent(kvp.Value));
                            }
                        }
                        _lastUpdateTime = DateTime.UtcNow;
                        UpdateHealthStatus(ConfigHealthStatus.Healthy);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    GameLogger.LogError($"[CompositeRemoteConfig] Firebase refresh failed: {ex.Message}");
                }
                UpdateHealthStatus(ConfigHealthStatus.Degraded);
            }
#endif
            return await _fallback.RefreshConfig();
        }

        public async UniTask<bool> RefreshConfig<T>() where T : class, IConfigModel
        {
            if (!_isInitialized)
            {
                return false;
            }

#if FIREBASE_REMOTE_CONFIG
            if (_firebaseAvailable)
            {
                try
                {
                    T config = await _firebaseProvider.FetchConfig<T>(typeof(T).Name);
                    if (config != null && config.Validate())
                    {
                        _cache[typeof(T)] = config;
                        OnConfigUpdated?.Invoke(config);
                        OnSpecificConfigUpdated?.Invoke(typeof(T), config);
                        _eventBus.Publish(new ConfigUpdatedEvent(config));
                        _lastUpdateTime = DateTime.UtcNow;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    GameLogger.LogError($"[CompositeRemoteConfig] Firebase refresh failed: {ex.Message}");
                }
            }
#endif
            return await _fallback.RefreshConfig<T>();
        }

        public T Get<T>(string key, T fallback)
        {
            if (_rawCache.TryGetValue(key, out object cached))
            {
                try { return (T)Convert.ChangeType(cached, typeof(T)); }
                catch { }
            }
            return fallback;
        }

        public Task FetchAsync() => RefreshConfig().AsTask();

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
