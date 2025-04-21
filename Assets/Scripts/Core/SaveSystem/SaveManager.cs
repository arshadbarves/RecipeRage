using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Core.Patterns;

namespace Core.SaveSystem
{
    /// <summary>
    /// Manages saving and loading of game data.
    /// </summary>
    public class SaveManager : MonoBehaviourSingleton<SaveManager>
    {
        [Header("Save Settings")]
        [SerializeField] private bool _useEncryption = false;
        [SerializeField] private string _encryptionKey = "RecipeRage";
        
        // Path constants
        private const string SETTINGS_FILENAME = "settings.json";
        private const string PLAYER_PROGRESS_FILENAME = "player_progress.json";
        private const string PLAYER_STATS_FILENAME = "player_stats.json";
        
        // Cached data
        private GameSettingsData _cachedSettings;
        private PlayerProgressData _cachedPlayerProgress;
        private PlayerStatsData _cachedPlayerStats;
        
        // Events
        public event Action<GameSettingsData> OnSettingsChanged;
        public event Action<PlayerProgressData> OnPlayerProgressChanged;
        public event Action<PlayerStatsData> OnPlayerStatsChanged;
        
        /// <summary>
        /// Initialize the save manager.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            
            // Load all data on startup
            LoadAllData();
            
            Debug.Log("[SaveManager] Initialized");
        }
        
        #region Public API
        
        /// <summary>
        /// Get the current game settings.
        /// </summary>
        /// <returns>The current game settings</returns>
        public GameSettingsData GetSettings()
        {
            if (_cachedSettings == null)
            {
                LoadSettings();
            }
            
            return _cachedSettings;
        }
        
        /// <summary>
        /// Save game settings.
        /// </summary>
        /// <param name="settings">The settings to save</param>
        public void SaveSettings(GameSettingsData settings)
        {
            _cachedSettings = settings;
            SaveData(settings, SETTINGS_FILENAME);
            OnSettingsChanged?.Invoke(settings);
            
            Debug.Log("[SaveManager] Settings saved");
        }
        
        /// <summary>
        /// Update specific settings without changing others.
        /// </summary>
        /// <param name="updateAction">Action to update the settings</param>
        public void UpdateSettings(Action<GameSettingsData> updateAction)
        {
            if (_cachedSettings == null)
            {
                LoadSettings();
            }
            
            updateAction?.Invoke(_cachedSettings);
            SaveSettings(_cachedSettings);
        }
        
        /// <summary>
        /// Get the current player progress.
        /// </summary>
        /// <returns>The current player progress</returns>
        public PlayerProgressData GetPlayerProgress()
        {
            if (_cachedPlayerProgress == null)
            {
                LoadPlayerProgress();
            }
            
            return _cachedPlayerProgress;
        }
        
        /// <summary>
        /// Save player progress.
        /// </summary>
        /// <param name="progress">The progress to save</param>
        public void SavePlayerProgress(PlayerProgressData progress)
        {
            _cachedPlayerProgress = progress;
            SaveData(progress, PLAYER_PROGRESS_FILENAME);
            OnPlayerProgressChanged?.Invoke(progress);
            
            Debug.Log("[SaveManager] Player progress saved");
        }
        
        /// <summary>
        /// Update specific player progress without changing others.
        /// </summary>
        /// <param name="updateAction">Action to update the progress</param>
        public void UpdatePlayerProgress(Action<PlayerProgressData> updateAction)
        {
            if (_cachedPlayerProgress == null)
            {
                LoadPlayerProgress();
            }
            
            updateAction?.Invoke(_cachedPlayerProgress);
            SavePlayerProgress(_cachedPlayerProgress);
        }
        
        /// <summary>
        /// Get the current player stats.
        /// </summary>
        /// <returns>The current player stats</returns>
        public PlayerStatsData GetPlayerStats()
        {
            if (_cachedPlayerStats == null)
            {
                LoadPlayerStats();
            }
            
            return _cachedPlayerStats;
        }
        
        /// <summary>
        /// Save player stats.
        /// </summary>
        /// <param name="stats">The stats to save</param>
        public void SavePlayerStats(PlayerStatsData stats)
        {
            _cachedPlayerStats = stats;
            SaveData(stats, PLAYER_STATS_FILENAME);
            OnPlayerStatsChanged?.Invoke(stats);
            
            Debug.Log("[SaveManager] Player stats saved");
        }
        
        /// <summary>
        /// Update specific player stats without changing others.
        /// </summary>
        /// <param name="updateAction">Action to update the stats</param>
        public void UpdatePlayerStats(Action<PlayerStatsData> updateAction)
        {
            if (_cachedPlayerStats == null)
            {
                LoadPlayerStats();
            }
            
            updateAction?.Invoke(_cachedPlayerStats);
            SavePlayerStats(_cachedPlayerStats);
        }
        
        /// <summary>
        /// Delete all saved data.
        /// </summary>
        public void DeleteAllData()
        {
            DeleteFile(SETTINGS_FILENAME);
            DeleteFile(PLAYER_PROGRESS_FILENAME);
            DeleteFile(PLAYER_STATS_FILENAME);
            
            // Reset cached data
            _cachedSettings = new GameSettingsData();
            _cachedPlayerProgress = new PlayerProgressData();
            _cachedPlayerStats = new PlayerStatsData();
            
            // Trigger events
            OnSettingsChanged?.Invoke(_cachedSettings);
            OnPlayerProgressChanged?.Invoke(_cachedPlayerProgress);
            OnPlayerStatsChanged?.Invoke(_cachedPlayerStats);
            
            Debug.Log("[SaveManager] All data deleted");
        }
        
        /// <summary>
        /// Import settings from PlayerPrefs (for migration from old system).
        /// </summary>
        public void ImportFromPlayerPrefs()
        {
            // Load current settings
            if (_cachedSettings == null)
            {
                LoadSettings();
            }
            
            // Audio settings
            _cachedSettings.MasterVolume = PlayerPrefs.GetFloat("MasterVolume", 80f) / 100f;
            _cachedSettings.MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 70f) / 100f;
            _cachedSettings.SfxVolume = PlayerPrefs.GetFloat("SFXVolume", 90f) / 100f;
            _cachedSettings.VoiceVolume = PlayerPrefs.GetFloat("VoiceVolume", 85f) / 100f;
            _cachedSettings.IsMuted = PlayerPrefs.GetInt("Mute", 0) == 1;
            
            // Graphics settings
            _cachedSettings.IsFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
            _cachedSettings.QualityLevel = PlayerPrefs.GetString("Quality", "High");
            _cachedSettings.Resolution = PlayerPrefs.GetString("Resolution", "1920x1080");
            _cachedSettings.Brightness = PlayerPrefs.GetFloat("Brightness", 50f) / 100f;
            _cachedSettings.VSync = PlayerPrefs.GetInt("VSync", 1) == 1;
            
            // Gameplay settings
            _cachedSettings.Sensitivity = PlayerPrefs.GetFloat("Sensitivity", 65f) / 100f;
            _cachedSettings.InvertY = PlayerPrefs.GetInt("InvertY", 0) == 1;
            _cachedSettings.CameraShake = PlayerPrefs.GetInt("CameraShake", 1) == 1;
            _cachedSettings.AutoPickup = PlayerPrefs.GetInt("AutoPickup", 0) == 1;
            _cachedSettings.Vibration = PlayerPrefs.GetInt("Vibration", 1) == 1;
            _cachedSettings.ShowTutorial = PlayerPrefs.GetInt("Tutorial", 1) == 1;
            _cachedSettings.Notifications = PlayerPrefs.GetInt("Notifications", 1) == 1;
            
            // Player info
            if (_cachedPlayerStats == null)
            {
                LoadPlayerStats();
            }
            
            _cachedPlayerStats.PlayerName = PlayerPrefs.GetString("PlayerName", "Player");
            _cachedPlayerStats.Level = PlayerPrefs.GetInt("PlayerLevel", 1);
            _cachedPlayerStats.Coins = PlayerPrefs.GetInt("PlayerCoins", 0);
            _cachedPlayerStats.Gems = PlayerPrefs.GetInt("PlayerGems", 0);
            
            // Save the imported data
            SaveSettings(_cachedSettings);
            SavePlayerStats(_cachedPlayerStats);
            
            Debug.Log("[SaveManager] Imported data from PlayerPrefs");
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Load all data.
        /// </summary>
        private void LoadAllData()
        {
            LoadSettings();
            LoadPlayerProgress();
            LoadPlayerStats();
        }
        
        /// <summary>
        /// Load settings.
        /// </summary>
        private void LoadSettings()
        {
            _cachedSettings = LoadData<GameSettingsData>(SETTINGS_FILENAME);
            
            // If no settings file exists, create default settings
            if (_cachedSettings == null)
            {
                _cachedSettings = new GameSettingsData();
                SaveSettings(_cachedSettings);
            }
        }
        
        /// <summary>
        /// Load player progress.
        /// </summary>
        private void LoadPlayerProgress()
        {
            _cachedPlayerProgress = LoadData<PlayerProgressData>(PLAYER_PROGRESS_FILENAME);
            
            // If no progress file exists, create default progress
            if (_cachedPlayerProgress == null)
            {
                _cachedPlayerProgress = new PlayerProgressData();
                SavePlayerProgress(_cachedPlayerProgress);
            }
        }
        
        /// <summary>
        /// Load player stats.
        /// </summary>
        private void LoadPlayerStats()
        {
            _cachedPlayerStats = LoadData<PlayerStatsData>(PLAYER_STATS_FILENAME);
            
            // If no stats file exists, create default stats
            if (_cachedPlayerStats == null)
            {
                _cachedPlayerStats = new PlayerStatsData();
                SavePlayerStats(_cachedPlayerStats);
            }
        }
        
        /// <summary>
        /// Save data to a file.
        /// </summary>
        /// <typeparam name="T">The type of data to save</typeparam>
        /// <param name="data">The data to save</param>
        /// <param name="filename">The filename to save to</param>
        private void SaveData<T>(T data, string filename)
        {
            string path = Path.Combine(Application.persistentDataPath, filename);
            string json = JsonUtility.ToJson(data, true);
            
            // Encrypt if needed
            if (_useEncryption)
            {
                json = EncryptDecrypt(json);
            }
            
            try
            {
                File.WriteAllText(path, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Error saving data to {path}: {e.Message}");
            }
        }
        
        /// <summary>
        /// Load data from a file.
        /// </summary>
        /// <typeparam name="T">The type of data to load</typeparam>
        /// <param name="filename">The filename to load from</param>
        /// <returns>The loaded data, or null if the file doesn't exist</returns>
        private T LoadData<T>(string filename) where T : new()
        {
            string path = Path.Combine(Application.persistentDataPath, filename);
            
            if (!File.Exists(path))
            {
                return new T();
            }
            
            try
            {
                string json = File.ReadAllText(path);
                
                // Decrypt if needed
                if (_useEncryption)
                {
                    json = EncryptDecrypt(json);
                }
                
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Error loading data from {path}: {e.Message}");
                return new T();
            }
        }
        
        /// <summary>
        /// Delete a file.
        /// </summary>
        /// <param name="filename">The filename to delete</param>
        private void DeleteFile(string filename)
        {
            string path = Path.Combine(Application.persistentDataPath, filename);
            
            if (File.Exists(path))
            {
                try
                {
                    File.Delete(path);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SaveManager] Error deleting file {path}: {e.Message}");
                }
            }
        }
        
        /// <summary>
        /// Simple XOR encryption/decryption.
        /// </summary>
        /// <param name="data">The data to encrypt/decrypt</param>
        /// <returns>The encrypted/decrypted data</returns>
        private string EncryptDecrypt(string data)
        {
            char[] result = new char[data.Length];
            
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (char)(data[i] ^ _encryptionKey[i % _encryptionKey.Length]);
            }
            
            return new string(result);
        }
        
        #endregion
    }
}
