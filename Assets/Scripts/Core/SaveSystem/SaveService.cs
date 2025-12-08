using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Core.Logging;

namespace Core.SaveSystem
{
    /// <summary>
    /// Multi-provider save service with cloud and local storage support.
    /// Orchestrates storage strategies per data type.
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

        // Cached data
        private GameSettingsData _cachedSettings;
        private PlayerProgressData _cachedProgress;
        private PlayerStatsData _cachedStats;

        // Events
        public event Action<GameSettingsData> OnSettingsChanged;
        public event Action<PlayerProgressData> OnPlayerProgressChanged;
        public event Action<PlayerStatsData> OnPlayerStatsChanged;

        public SaveService(StorageProviderFactory providerFactory, IEncryptionService encryption)
        {
            _providerFactory = providerFactory ?? throw new ArgumentNullException(nameof(providerFactory));
            _encryption = encryption;

            // Get providers
            _localProvider = _providerFactory.GetLocalProvider();
            _cloudProvider = _providerFactory.GetCloudProvider();

            // Configure storage strategies
            _storageConfigs = new Dictionary<string, StorageConfig>
            {
                // Settings: Local only (fast, offline)
                { "settings.json", new StorageConfig("settings.json", StorageStrategy.LocalOnly, false) },

                // Progress: Cloud with local cache (persistent, cross-device)
                { "progress.json", new StorageConfig("progress.json", StorageStrategy.CloudWithCache, true) },

                // Stats: Cloud with local cache (persistent, cross-device)
                { "stats.json", new StorageConfig("stats.json", StorageStrategy.CloudWithCache, true) }
            };

            // Initialize sync status
            _syncStatus = new Dictionary<string, SyncStatus>();
            foreach (StorageConfig config in _storageConfigs.Values)
            {
                _syncStatus[config.Key] = new SyncStatus();
            }

            LoadAllData();

            GameLogger.Log("SaveService initialized");
        }

        /// <summary>
        /// Notify cloud provider that user has logged in.
        /// Call this after successful authentication.
        /// Enables cloud storage and syncs local data.
        /// </summary>
        public void OnUserLoggedIn()
        {
            GameLogger.Log("[SaveService] User logged in - enabling cloud storage");

            if (_cloudProvider is EOSCloudStorageProvider eosProvider)
            {
                eosProvider.OnUserLoggedIn();
            }

            // Optionally sync local data to cloud
            SyncLocalToCloudAsync().Forget();
        }

        /// <summary>
        /// Called when user logs out - clears user-specific cache
        /// </summary>
        public void OnUserLoggedOut()
        {
            GameLogger.Log("[SaveService] User logged out - clearing user-specific cache");

            // Clear user-specific cached data
            _cachedProgress = null;
            _cachedStats = null;

            // Keep device settings cached (not user-specific)
            // _cachedSettings is preserved

            // Clear sync status for user data
            if (_syncStatus.ContainsKey("progress.json"))
            {
                _syncStatus["progress.json"] = new SyncStatus();
            }
            if (_syncStatus.ContainsKey("stats.json"))
            {
                _syncStatus["stats.json"] = new SyncStatus();
            }

            // Notify cloud provider
            if (_cloudProvider is EOSCloudStorageProvider eosProvider)
            {
                eosProvider.OnUserLoggedOut();
            }

            GameLogger.Log("[SaveService] User cache cleared - device settings preserved");
        }

        private async UniTaskVoid SyncLocalToCloudAsync()
        {
            if (!_cloudProvider.IsAvailable)
            {
                GameLogger.Log("[SaveService] Cloud provider not available for sync");
                return;
            }

            GameLogger.Log("[SaveService] Syncing local data to cloud...");

            try
            {
                // Sync progress data
                if (_cachedProgress != null)
                {
                    var progressConfig = GetStorageConfig("progress.json");
                    string progressContent = SerializeData(_cachedProgress, progressConfig.EncryptData);
                    await _cloudProvider.WriteAsync("progress.json", progressContent);
                    GameLogger.Log("[SaveService] Progress synced to cloud");
                }

                // Sync stats data
                if (_cachedStats != null)
                {
                    var statsConfig = GetStorageConfig("stats.json");
                    string statsContent = SerializeData(_cachedStats, statsConfig.EncryptData);
                    await _cloudProvider.WriteAsync("stats.json", statsContent);
                    GameLogger.Log("[SaveService] Stats synced to cloud");
                }

                GameLogger.Log("[SaveService] Cloud sync complete");
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"[SaveService] Cloud sync failed: {ex.Message}");
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

        /// <summary>
        /// Get sync status for a specific data type.
        /// </summary>
        public SyncStatus GetSyncStatus(string key)
        {
            return _syncStatus.TryGetValue(key, out var status) ? status : null;
        }

        /// <summary>
        /// Force sync all cloud data.
        /// </summary>
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
                if (config.Strategy == StorageStrategy.CloudWithCache ||
                    config.Strategy == StorageStrategy.CloudOnly)
                {
                    syncTasks.Add(SyncToCloudAsync(config.Key));
                }
            }

            await UniTask.WhenAll(syncTasks);
        }

        public PlayerProgressData GetPlayerProgress()
        {
            if (_cachedProgress == null)
            {
                _cachedProgress = LoadData<PlayerProgressData>("progress.json");
            }
            return _cachedProgress;
        }

        public void SavePlayerProgress(PlayerProgressData progress)
        {
            _cachedProgress = progress;
            SaveData("progress.json", progress);
            OnPlayerProgressChanged?.Invoke(progress);
        }

        public void UpdatePlayerProgress(Action<PlayerProgressData> updateAction)
        {
            var progress = GetPlayerProgress();
            updateAction?.Invoke(progress);
            SavePlayerProgress(progress);
        }

        public PlayerStatsData GetPlayerStats()
        {
            if (_cachedStats == null)
            {
                _cachedStats = LoadData<PlayerStatsData>("stats.json");
            }
            return _cachedStats;
        }

        public void SavePlayerStats(PlayerStatsData stats)
        {
            _cachedStats = stats;
            SaveData("stats.json", stats);
            OnPlayerStatsChanged?.Invoke(stats);
        }

        public void UpdatePlayerStats(Action<PlayerStatsData> updateAction)
        {
            var stats = GetPlayerStats();
            updateAction?.Invoke(stats);
            SavePlayerStats(stats);
        }

        public void DeleteAllData()
        {
            _localProvider.Delete("settings.json");
            _localProvider.Delete("progress.json");
            _localProvider.Delete("stats.json");

            if (_cloudProvider.IsAvailable)
            {
                _cloudProvider.Delete("progress.json");
                _cloudProvider.Delete("stats.json");
            }

            _cachedSettings = new GameSettingsData();
            _cachedProgress = new PlayerProgressData();
            _cachedStats = new PlayerStatsData();

            OnSettingsChanged?.Invoke(_cachedSettings);
            OnPlayerProgressChanged?.Invoke(_cachedProgress);
            OnPlayerStatsChanged?.Invoke(_cachedStats);
        }

        /// <summary>
        /// Clear user-specific cache without deleting saved data.
        /// Useful for "Clear Cache" functionality or troubleshooting.
        /// </summary>
        public void ClearUserCache()
        {
            GameLogger.Log("[SaveService] Clearing user cache (data preserved on disk)");

            // Clear cached user data (will reload from disk on next access)
            _cachedProgress = null;
            _cachedStats = null;

            // Keep settings cached (device-level, not user-specific)

            // Clear cloud cache
            if (_cloudProvider is EOSCloudStorageProvider eosProvider)
            {
                eosProvider.OnUserLoggedOut();
            }

            GameLogger.Log("[SaveService] User cache cleared");
        }

        private void LoadAllData()
        {
            _cachedSettings = LoadData<GameSettingsData>("settings.json");
            _cachedProgress = LoadData<PlayerProgressData>("progress.json");
            _cachedStats = LoadData<PlayerStatsData>("stats.json");
        }

        public T LoadData<T>(string key) where T : class, new()
        {
            var config = GetStorageConfig(key);
            var provider = GetProviderForStrategy(config.Strategy);

            // Try primary provider
            if (provider.Exists(key))
            {
                string content = provider.Read(key);
                if (!string.IsNullOrEmpty(content))
                {
                    return DeserializeData<T>(key, content, config.EncryptData);
                }
            }

            // For CloudWithCache, try local fallback
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
                    {
                        _cloudProvider.Write(key, content);
                    }
                    else
                    {
                        GameLogger.LogWarning($"[SaveService] Cloud provider not available for {key}");
                    }
                    break;

                case StorageStrategy.CloudWithCache:
                    // Write to local immediately (fast)
                    _localProvider.Write(key, content);

                    // Queue cloud sync (async)
                    if (_cloudProvider.IsAvailable)
                    {
                        _syncStatus[key].MarkPendingChanges();
                        SyncToCloudAsync(key).Forget();
                    }
                    else
                    {
                        GameLogger.Log($"[SaveService] Cloud not available, saved to local cache: {key}");
                    }
                    break;
            }
        }

        private async UniTask SyncToCloudAsync(string key)
        {
            var config = GetStorageConfig(key);
            var status = _syncStatus[key];

            if (status.IsSyncing)
            {
                return; // Already syncing
            }

            status.MarkSyncStarted();

            try
            {
                // Read from local
                string content = _localProvider.Read(key);
                if (string.IsNullOrEmpty(content))
                {
                    status.MarkSyncCompleted();
                    return;
                }

                // Write to cloud
                await _cloudProvider.WriteAsync(key, content);

                status.MarkSyncCompleted();
                GameLogger.Log($"[SaveService] Synced {key} to cloud");
            }
            catch (Exception e)
            {
                status.MarkSyncFailed(e.Message);
                GameLogger.LogError($"[SaveService] Failed to sync {key}: {e.Message}");
            }
        }

        private string SerializeData<T>(T data, bool encrypt)
        {
            string content = UnityEngine.JsonUtility.ToJson(data, true);

            if (encrypt && _encryption != null)
            {
                content = _encryption.Encrypt(content);
            }

            return content;
        }

        private T DeserializeData<T>(string key, string content, bool encrypted) where T : new()
        {
            try
            {
                if (encrypted && _encryption != null)
                {
                    content = _encryption.Decrypt(content);
                }

                return UnityEngine.JsonUtility.FromJson<T>(content);
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"[SaveService] Failed to deserialize {key}: {ex.Message}");
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
