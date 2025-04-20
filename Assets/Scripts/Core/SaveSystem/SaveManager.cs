using System;
using System.IO;
using UnityEngine;
using Core.Patterns;

namespace Core.SaveSystem
{
    /// <summary>
    /// Manages saving and loading game data.
    /// This class is a singleton that persists throughout the game.
    /// </summary>
    public class SaveManager : MonoBehaviourSingleton<SaveManager>
    {
        /// <summary>
        /// Event triggered when settings are changed.
        /// </summary>
        public event Action OnSettingsChanged;

        /// <summary>
        /// The current player settings.
        /// </summary>
        private PlayerSettings _settings;

        /// <summary>
        /// Path to the settings file.
        /// </summary>
        private string SettingsPath => Path.Combine(Application.persistentDataPath, "settings.json");

        /// <summary>
        /// Initialize the save manager.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            
            // Load settings
            LoadSettings();
        }

        #region Settings Management

        /// <summary>
        /// Get the current player settings.
        /// </summary>
        /// <returns>The current player settings</returns>
        public PlayerSettings GetSettings()
        {
            if (_settings == null)
            {
                LoadSettings();
            }
            
            return _settings;
        }

        /// <summary>
        /// Save the current player settings.
        /// </summary>
        public void SaveSettings()
        {
            if (_settings == null)
            {
                _settings = new PlayerSettings();
            }
            
            try
            {
                string json = JsonUtility.ToJson(_settings, true);
                File.WriteAllText(SettingsPath, json);
                Debug.Log($"[SaveManager] Settings saved to {SettingsPath}");
                
                // Trigger settings changed event
                OnSettingsChanged?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Error saving settings: {e.Message}");
            }
        }

        /// <summary>
        /// Load player settings from disk.
        /// </summary>
        public void LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);
                    _settings = JsonUtility.FromJson<PlayerSettings>(json);
                    Debug.Log($"[SaveManager] Settings loaded from {SettingsPath}");
                }
                else
                {
                    // Create default settings if file doesn't exist
                    _settings = new PlayerSettings();
                    SaveSettings();
                }
                
                // Trigger settings changed event
                OnSettingsChanged?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Error loading settings: {e.Message}");
                _settings = new PlayerSettings();
            }
        }

        /// <summary>
        /// Apply the current settings to the game.
        /// </summary>
        public void ApplySettings()
        {
            if (_settings == null)
            {
                LoadSettings();
            }
            
            _settings.Apply();
        }

        #endregion

        #region PlayerPrefs Migration

        /// <summary>
        /// Migrate settings from PlayerPrefs to the new save system.
        /// </summary>
        public void MigrateFromPlayerPrefs()
        {
            if (_settings == null)
            {
                _settings = new PlayerSettings();
            }
            
            // Audio settings
            _settings.MasterVolume = PlayerPrefs.GetFloat("MasterVolume", _settings.MasterVolume);
            _settings.MusicVolume = PlayerPrefs.GetFloat("MusicVolume", _settings.MusicVolume);
            _settings.SfxVolume = PlayerPrefs.GetFloat("SFXVolume", _settings.SfxVolume);
            _settings.Muted = PlayerPrefs.GetInt("Mute", _settings.Muted ? 1 : 0) == 1;
            
            // Graphics settings
            _settings.Fullscreen = PlayerPrefs.GetInt("Fullscreen", _settings.Fullscreen ? 1 : 0) == 1;
            _settings.QualityLevel = PlayerPrefs.GetInt("Quality", _settings.QualityLevel);
            _settings.ResolutionIndex = PlayerPrefs.GetInt("Resolution", _settings.ResolutionIndex);
            
            // Gameplay settings
            _settings.CameraShake = PlayerPrefs.GetInt("CameraShake", _settings.CameraShake ? 1 : 0) == 1;
            _settings.AutoPickup = PlayerPrefs.GetInt("AutoPickup", _settings.AutoPickup ? 1 : 0) == 1;
            _settings.InvertY = PlayerPrefs.GetInt("InvertY", _settings.InvertY ? 1 : 0) == 1;
            _settings.Vibration = PlayerPrefs.GetInt("Vibration", _settings.Vibration ? 1 : 0) == 1;
            _settings.Sensitivity = PlayerPrefs.GetFloat("Sensitivity", _settings.Sensitivity);
            _settings.ShowTutorial = PlayerPrefs.GetInt("Tutorial", _settings.ShowTutorial ? 1 : 0) == 1;
            
            // Player info
            _settings.PlayerName = PlayerPrefs.GetString("PlayerName", _settings.PlayerName);
            
            // Save the migrated settings
            SaveSettings();
            
            Debug.Log("[SaveManager] Settings migrated from PlayerPrefs");
        }

        #endregion
    }
}
