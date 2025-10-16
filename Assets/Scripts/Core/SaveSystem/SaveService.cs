using System;

namespace Core.SaveSystem
{
    /// <summary>
    /// Pure C# save service - no MonoBehaviour needed
    /// </summary>
    public class SaveService : ISaveService
    {
        private readonly IFileStorage _storage;
        private readonly IEncryptionService _encryption;

        // Cached data
        private GameSettingsData _cachedSettings;
        private PlayerProgressData _cachedProgress;
        private PlayerStatsData _cachedStats;

        // Events
        public event Action<GameSettingsData> OnSettingsChanged;
        public event Action<PlayerProgressData> OnPlayerProgressChanged;
        public event Action<PlayerStatsData> OnPlayerStatsChanged;

        public SaveService(IFileStorage storage, IEncryptionService encryption)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _encryption = encryption;

            LoadAllData();
        }

        public GameSettingsData GetSettings()
        {
            if (_cachedSettings == null)
            {
                _cachedSettings = Load<GameSettingsData>("settings.json");
            }
            return _cachedSettings;
        }

        public void SaveSettings(GameSettingsData settings)
        {
            _cachedSettings = settings;
            Save("settings.json", settings);
            OnSettingsChanged?.Invoke(settings);
        }

        public void UpdateSettings(Action<GameSettingsData> updateAction)
        {
            var settings = GetSettings();
            updateAction?.Invoke(settings);
            SaveSettings(settings);
        }

        public PlayerProgressData GetPlayerProgress()
        {
            if (_cachedProgress == null)
            {
                _cachedProgress = Load<PlayerProgressData>("progress.json");
            }
            return _cachedProgress;
        }

        public void SavePlayerProgress(PlayerProgressData progress)
        {
            _cachedProgress = progress;
            Save("progress.json", progress);
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
                _cachedStats = Load<PlayerStatsData>("stats.json");
            }
            return _cachedStats;
        }

        public void SavePlayerStats(PlayerStatsData stats)
        {
            _cachedStats = stats;
            Save("stats.json", stats);
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
            _storage.Delete("settings.json");
            _storage.Delete("progress.json");
            _storage.Delete("stats.json");

            _cachedSettings = new GameSettingsData();
            _cachedProgress = new PlayerProgressData();
            _cachedStats = new PlayerStatsData();

            OnSettingsChanged?.Invoke(_cachedSettings);
            OnPlayerProgressChanged?.Invoke(_cachedProgress);
            OnPlayerStatsChanged?.Invoke(_cachedStats);
        }

        private void LoadAllData()
        {
            _cachedSettings = Load<GameSettingsData>("settings.json");
            _cachedProgress = Load<PlayerProgressData>("progress.json");
            _cachedStats = Load<PlayerStatsData>("stats.json");
        }

        private T Load<T>(string filename) where T : new()
        {
            if (!_storage.Exists(filename))
            {
                return new T();
            }

            string content = _storage.Read(filename);
            
            if (_encryption != null && !string.IsNullOrEmpty(content))
            {
                content = _encryption.Decrypt(content);
            }

            return UnityEngine.JsonUtility.FromJson<T>(content);
        }

        private void Save<T>(string filename, T data)
        {
            string content = UnityEngine.JsonUtility.ToJson(data, true);
            
            if (_encryption != null)
            {
                content = _encryption.Encrypt(content);
            }

            _storage.Write(filename, content);
        }
    }
}
