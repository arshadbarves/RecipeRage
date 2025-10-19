using System;
using Cysharp.Threading.Tasks;

namespace Core.SaveSystem
{
    /// <summary>
    /// Interface for save/load operations with multi-provider support
    /// </summary>
    public interface ISaveService
    {
        // Settings
        GameSettingsData GetSettings();
        void SaveSettings(GameSettingsData settings);
        void UpdateSettings(Action<GameSettingsData> updateAction);
        event Action<GameSettingsData> OnSettingsChanged;

        // Player Progress
        PlayerProgressData GetPlayerProgress();
        void SavePlayerProgress(PlayerProgressData progress);
        void UpdatePlayerProgress(Action<PlayerProgressData> updateAction);
        event Action<PlayerProgressData> OnPlayerProgressChanged;

        // Player Stats
        PlayerStatsData GetPlayerStats();
        void SavePlayerStats(PlayerStatsData stats);
        void UpdatePlayerStats(Action<PlayerStatsData> updateAction);
        event Action<PlayerStatsData> OnPlayerStatsChanged;

        // Utility
        void DeleteAllData();
        
        // Cloud sync
        SyncStatus GetSyncStatus(string key);
        UniTask SyncAllCloudDataAsync();
        
        // Authentication integration
        void OnUserLoggedIn();
        void OnUserLoggedOut();
        
        // Generic save/load (for custom data like currency)
        T LoadData<T>(string key) where T : class, new();
        void SaveData<T>(string key, T data) where T : class, new();
    }
}
