using System;
using System.Collections.Generic;
using Core.Logging;
using Core.Persistence.Factory;
using Core.Persistence.Interfaces;
using Core.Persistence.Models;
using Core.Persistence.Providers;
using Cysharp.Threading.Tasks;

namespace Core.Persistence
{
    /// <summary>
    /// Generic multi-provider save service with cloud and local storage support.
    /// Handles storage strategies and sync. Does NOT hold game-specific data.
    /// </summary>
    public class SaveService : ISaveService
    {
        private readonly StorageProviderFactory _providerFactory;
        private readonly IEncryptionService _encryption;
        private readonly IStorageProvider _localProvider;
        private readonly IStorageProvider _cloudProvider;

        // Storage configurations per data type
        private readonly Dictionary<string, StorageConfig> _storageConfigs;

        // Sync status tracking
        private readonly Dictionary<string, SyncStatus> _syncStatus;

        // Settings Cache (device-wide, Core responsibility)
        private GameSettingsData _cachedSettings;

        public event Action<GameSettingsData> OnSettingsChanged;

        public SaveService(StorageProviderFactory providerFactory, IEncryptionService encryption)
        {
            _providerFactory = providerFactory ?? throw new ArgumentNullException(nameof(providerFactory));
            _encryption = encryption;

            _localProvider = _providerFactory.GetLocalProvider();
            _cloudProvider = _providerFactory.GetCloudProvider();

            // Default Storage Configs (used if key not explicitly registered)
            _storageConfigs = new Dictionary<string, StorageConfig>
            {
                { "settings.json", new StorageConfig("settings.json", StorageStrategy.LocalOnly, false) }
            };

            _syncStatus = new Dictionary<string, SyncStatus>();
            foreach (StorageConfig config in _storageConfigs.Values)
            {
                _syncStatus[config.Key] = new SyncStatus();
            }

            _cachedSettings = LoadData<GameSettingsData>("settings.json");

            GameLogger.Log("SaveService initialized");
        }

        /// <summary>
        /// Register a storage configuration for a specific key.
        /// Use this for gameplay data types that need cloud sync.
        /// </summary>
        public void RegisterStorageConfig(string key, StorageStrategy strategy, bool encrypt)
        {
            _storageConfigs[key] = new StorageConfig(key, strategy, encrypt);
            _syncStatus[key] = new SyncStatus();
        }

        public void OnUserLoggedIn()
        {
            GameLogger.Log("[SaveService] User logged in - enabling cloud storage");
            if (_cloudProvider is EOSCloudStorageProvider eosProvider)
            {
                eosProvider.OnUserLoggedIn();
            }
        }

        public void OnUserLoggedOut()
        {
            GameLogger.Log("[SaveService] User logged out");
            if (_cloudProvider is EOSCloudStorageProvider eosProvider)
            {
                eosProvider.OnUserLoggedOut();
            }
        }

        public GameSettingsData GetSettings()
        {
            if (_cachedSettings == null)
            {
                _cachedSettings = LoadData<GameSettingsData>("settings.json");
            }
            return _cachedSettings;
        }

        public void SaveSettings(GameSettingsData settings)
        {
            _cachedSettings = settings;
            SaveData("settings.json", settings);
            OnSettingsChanged?.Invoke(settings);
        }

        public void UpdateSettings(Action<GameSettingsData> updateAction)
        {
            var settings = GetSettings();
            updateAction?.Invoke(settings);
            SaveSettings(settings);
        }

        public SyncStatus GetSyncStatus(string key)
        {
            return _syncStatus.TryGetValue(key, out var status) ? status : null;
        }

        public async UniTask SyncAllCloudDataAsync()
        {
            if (!_cloudProvider.IsAvailable)
            {
                GameLogger.Log("[SaveService] Cloud provider not available for sync");
                return;
            }

            var syncTasks = new List<UniTask>();
            foreach (var config in _storageConfigs.Values)
            {
                if (config.Strategy is StorageStrategy.CloudWithCache or StorageStrategy.CloudOnly)
                {
                    syncTasks.Add(SyncToCloudAsync(config.Key));
                }
            }
            await UniTask.WhenAll(syncTasks);
        }

        public T LoadData<T>(string key) where T : class, new()
        {
            var config = GetStorageConfig(key);
            var provider = GetProviderForStrategy(config.Strategy);

            if (provider.Exists(key))
            {
                string content = provider.Read(key);
                if (!string.IsNullOrEmpty(content))
                {
                    return DeserializeData<T>(key, content, config.EncryptData);
                }
            }

            if (config.Strategy == StorageStrategy.CloudWithCache && _localProvider.Exists(key))
            {
                string content = _localProvider.Read(key);
                if (!string.IsNullOrEmpty(content))
                {
                    GameLogger.Log($"[SaveService] Using local cache for {key}");
                    return DeserializeData<T>(key, content, config.EncryptData);
                }
            }

            return new T();
        }

        public void SaveData<T>(string key, T data) where T : class, new()
        {
            var config = GetStorageConfig(key);
            string content = SerializeData(data, config.EncryptData);

            switch (config.Strategy)
            {
                case StorageStrategy.LocalOnly:
                    _localProvider.Write(key, content);
                    break;
                case StorageStrategy.CloudOnly:
                    if (_cloudProvider.IsAvailable)
                        _cloudProvider.Write(key, content);
                    else
                        GameLogger.LogWarning($"[SaveService] Cloud not available for {key}");
                    break;
                case StorageStrategy.CloudWithCache:
                    _localProvider.Write(key, content);
                    if (_cloudProvider.IsAvailable)
                    {
                        _syncStatus[key]?.MarkPendingChanges();
                        SyncToCloudAsync(key).Forget();
                    }
                    else
                    {
                        GameLogger.Log($"[SaveService] Cloud not available, saved to local: {key}");
                    }
                    break;
            }
        }

        private async UniTask SyncToCloudAsync(string key)
        {
            var config = GetStorageConfig(key);
            var status = _syncStatus.TryGetValue(key, out var s) ? s : null;

            if (status == null || status.IsSyncing) return;

            status.MarkSyncStarted();

            try
            {
                string content = _localProvider.Read(key);
                if (string.IsNullOrEmpty(content))
                {
                    status.MarkSyncCompleted();
                    return;
                }
                await _cloudProvider.WriteAsync(key, content);
                status.MarkSyncCompleted();
                GameLogger.Log($"[SaveService] Synced {key} to cloud");
            }
            catch (Exception e)
            {
                status.MarkSyncFailed(e.Message);
                GameLogger.LogError($"[SaveService] Sync failed {key}: {e.Message}");
            }
        }

        private string SerializeData<T>(T data, bool encrypt)
        {
            string content = UnityEngine.JsonUtility.ToJson(data, true);
            if (encrypt && _encryption != null)
                content = _encryption.Encrypt(content);
            return content;
        }

        private T DeserializeData<T>(string key, string content, bool encrypted) where T : new()
        {
            try
            {
                if (encrypted && _encryption != null)
                    content = _encryption.Decrypt(content);
                return UnityEngine.JsonUtility.FromJson<T>(content);
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"[SaveService] Deserialize failed {key}: {ex.Message}");
                return new T();
            }
        }

        private StorageConfig GetStorageConfig(string key)
        {
            return _storageConfigs.TryGetValue(key, out var config)
                ? config
                : new StorageConfig(key, StorageStrategy.LocalOnly, false);
        }

        private IStorageProvider GetProviderForStrategy(StorageStrategy strategy)
        {
            return strategy switch
            {
                StorageStrategy.LocalOnly => _localProvider,
                StorageStrategy.CloudOnly => _cloudProvider,
                StorageStrategy.CloudWithCache => _cloudProvider.IsAvailable ? _cloudProvider : _localProvider,
                _ => _localProvider
            };
        }
    }
}
