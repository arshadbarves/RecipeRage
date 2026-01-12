using System;
using Core.Persistence.Models;
using Cysharp.Threading.Tasks;

namespace Core.Persistence
{
    /// <summary>
    /// Generic interface for save/load operations with multi-provider support.
    /// Does NOT hold game-specific data models.
    /// </summary>
    public interface ISaveService
    {
        // Settings (Core-owned, device-level)
        GameSettingsData GetSettings();
        void SaveSettings(GameSettingsData settings);
        void UpdateSettings(Action<GameSettingsData> updateAction);
        event Action<GameSettingsData> OnSettingsChanged;

        // Cloud sync
        SyncStatus GetSyncStatus(string key);
        UniTask SyncAllCloudDataAsync();

        // Authentication integration
        void OnUserLoggedIn();
        void OnUserLoggedOut();

        // Generic save/load (for any data type)
        T LoadData<T>(string key) where T : class, new();
        void SaveData<T>(string key, T data) where T : class, new();
        
        // Dynamic storage config
        void RegisterStorageConfig(string key, StorageStrategy strategy, bool encrypt);
    }
}
