using System;
using System.Threading.Tasks;
using KitchenClash.Application.Models;

namespace KitchenClash.Application
{
    public interface IPlayerDataService
    {
        event Action<PlayerProgressData> OnProgressChanged;
        event Action<PlayerStatsData> OnStatsChanged;
        event Action OnLevelUp;

        void Initialize();
        PlayerProgressData GetProgress();
        PlayerStatsData GetStats();
        void SetPlayerName(string name);
        void RecordGamePlayed(bool won, string gameModeId, string characterId, float playTime, int score, int xp);

        /// <summary>Generic key-value load for subsystem data (e.g. daily streak).</summary>
        Task<string> LoadAsync(string key);
        /// <summary>Generic key-value save for subsystem data.</summary>
        Task SaveAsync(string key, string data);
    }
}
