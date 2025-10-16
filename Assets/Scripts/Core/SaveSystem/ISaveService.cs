using System;

namespace Core.SaveSystem
{
    /// <summary>
    /// Interface for save/load operations
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
    }
}
