using KitchenClash.Application;
using KitchenClash.Application.Models;
using KitchenClash.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KitchenClash.Infrastructure.Persistence
{
    public class PlayerDataService : IPlayerDataService
    {
        private readonly ISaveService _saveService;
        private readonly Dictionary<string, int> _characterLevels = new();
        private readonly HashSet<string> _unlockedCharacters = new();

        public event Action<PlayerProgressData> OnProgressChanged;
        public event Action<PlayerStatsData> OnStatsChanged;
        public event Action OnLevelUp;

        public PlayerDataService(ISaveService saveService)
        {
            _saveService = saveService;
        }

        public void Initialize() { }

        public PlayerProgressData GetProgress() => new PlayerProgressData();
        public PlayerStatsData GetStats() => new PlayerStatsData();

        public void SetPlayerName(string name)
        {
            _saveService.Save("displayName", name);
        }

        public void RecordGamePlayed(bool won, string gameModeId, string characterId, float playTime, int score, int xp) { }

        public int GetCharacterLevel(string characterId)
        {
            return _characterLevels.TryGetValue(characterId, out int level) ? level : 1;
        }

        public bool UpgradeCharacter(string characterId, int cost)
        {
            int currentLevel = GetCharacterLevel(characterId);
            _characterLevels[characterId] = currentLevel + 1;
            OnLevelUp?.Invoke();
            return true;
        }

        public void UnlockCharacter(string characterId)
        {
            _unlockedCharacters.Add(characterId);
        }

        public Task<string> LoadAsync(string key)
        {
            return Task.FromResult(_saveService.Load<string>(key, null));
        }

        public Task SaveAsync(string key, string data)
        {
            _saveService.Save(key, data);
            return Task.CompletedTask;
        }
    }
}
