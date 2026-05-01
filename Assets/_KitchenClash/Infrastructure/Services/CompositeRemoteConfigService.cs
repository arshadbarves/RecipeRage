using System;
using System.Collections.Generic;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using Cysharp.Threading.Tasks;

namespace KitchenClash.Infrastructure.Services
{
    /// <summary>
    /// Composite remote config service that tries Firebase first (when available),
    /// then falls back to local defaults. Caches fetched values.
    /// </summary>
    public sealed class CompositeRemoteConfigService : IRemoteConfigService
    {
        private readonly Dictionary<Type, IConfigModel> _cache = new();
        private readonly FallbackRemoteConfigService _fallback = new();

#if FIREBASE_REMOTE_CONFIG
        private readonly Firebase.RemoteConfigService _firebaseService;
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
        public CompositeRemoteConfigService(IConfigProvider configProvider)
        {
            _firebaseService = new Firebase.RemoteConfigService(configProvider);
            _firebaseService.OnConfigUpdated += cfg =>
            {
                _cache[cfg.GetType()] = cfg;
                OnConfigUpdated?.Invoke(cfg);
            };
            _firebaseService.OnSpecificConfigUpdated += (t, cfg) =>
            {
                _cache[t] = cfg;
                OnSpecificConfigUpdated?.Invoke(t, cfg);
            };
        }
#else
        public CompositeRemoteConfigService()
        {
        }
#endif

        public async UniTask<bool> Initialize()
        {
            if (_isInitialized) return true;

#if FIREBASE_REMOTE_CONFIG
            try
            {
                GameLogger.Log("[CompositeRemoteConfig] Attempting Firebase initialization...");
                bool success = await _firebaseService.Initialize();
                if (success)
                {
                    _firebaseAvailable = true;
                    _isInitialized = true;
                    UpdateHealthStatus(_firebaseService.HealthStatus);
                    _lastUpdateTime = _firebaseService.LastUpdateTime;
                    GameLogger.Log("[CompositeRemoteConfig] Firebase initialized successfully");
                    return true;
                }
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"[CompositeRemoteConfig] Firebase init failed: {ex.Message}");
            }
#endif
            // Fall back to local defaults
            GameLogger.Log("[CompositeRemoteConfig] Using fallback defaults");
            await _fallback.Initialize();
            _isInitialized = true;
            UpdateHealthStatus(ConfigHealthStatus.Degraded);
            return true;
        }

        public T GetConfig<T>() where T : class, IConfigModel
        {
            if (_cache.TryGetValue(typeof(T), out var cached))
                return cached as T;

#if FIREBASE_REMOTE_CONFIG
            if (_firebaseAvailable)
            {
                var config = _firebaseService.GetConfig<T>();
                if (config != null)
                {
                    _cache[typeof(T)] = config;
                    return config;
                }
            }
#endif
            return _fallback.GetConfig<T>();
        }

        public bool TryGetConfig<T>(out T config) where T : class, IConfigModel
        {
            config = GetConfig<T>();
            return config != null;
        }

        public async UniTask<bool> RefreshConfig()
        {
            if (!_isInitialized) return false;

#if FIREBASE_REMOTE_CONFIG
            if (_firebaseAvailable)
            {
                bool result = await _firebaseService.RefreshConfig();
                if (result)
                {
                    _lastUpdateTime = DateTime.UtcNow;
                    UpdateHealthStatus(ConfigHealthStatus.Healthy);
                    return true;
                }
                UpdateHealthStatus(ConfigHealthStatus.Degraded);
            }
#endif
            return await _fallback.RefreshConfig();
        }

        public async UniTask<bool> RefreshConfig<T>() where T : class, IConfigModel
        {
            if (!_isInitialized) return false;

#if FIREBASE_REMOTE_CONFIG
            if (_firebaseAvailable)
            {
                bool result = await _firebaseService.RefreshConfig<T>();
                if (result)
                {
                    _lastUpdateTime = DateTime.UtcNow;
                    return true;
                }
            }
#endif
            return await _fallback.RefreshConfig<T>();
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
