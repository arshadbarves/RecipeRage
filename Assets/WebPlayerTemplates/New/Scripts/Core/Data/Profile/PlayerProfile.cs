using System;
using System.Threading.Tasks;
using Core.Data.Save;
using UnityEngine;

namespace Core.Data.Profile
{
    public class PlayerProfile
    {
        private readonly string _profileId;
        private readonly SaveManager _saveManager;

        public PlayerProfile(string id, SaveManager saveManager)
        {
            _profileId = id;
            _saveManager = saveManager;
        }

        public ProfileData Data { get; private set; }

        public event Action<float> OnExperienceGained;
        public event Action<int> OnLevelUp;
        public event Action<int> OnCoinsChanged;
        public event Action<int> OnGemsChanged;

        public async Task Initialize()
        {
            Data = await LoadProfile();
            if (Data == null)
            {
                Data = CreateNewProfile();
                await SaveProfile();
            }
            await UpdateLastLoginDate();
        }

        private async Task<ProfileData> LoadProfile()
        {
            return await _saveManager.LoadData<ProfileData>($"profile_{_profileId}");
        }

        private ProfileData CreateNewProfile()
        {
            // TODO: Implement a more robust way to generate player names
            return new ProfileData(_profileId, $"Player_{_profileId.Substring(0, 6)}");
        }

        public async Task SaveProfile()
        {
            await _saveManager.SaveData($"profile_{_profileId}", Data);
        }

        public async Task UpdateLastLoginDate()
        {
            Data.LastLoginDate = DateTime.UtcNow;
            await SaveProfile();
        }

    #region Statistics

        public async Task UpdateGameStatistics(bool won, int ordersCompleted, int perfectOrders, float playTime)
        {
            Data.totalGamesPlayed++;
            if (won) Data.gamesWon++;
            Data.totalOrders += ordersCompleted;
            Data.perfectOrders += perfectOrders;
            Data.totalPlayTime += playTime;

            await SaveProfile();
        }

    #endregion

    #region Experience and Leveling

        public async Task AddExperience(float amount)
        {
            Data.experiencePoints += amount;
            OnExperienceGained?.Invoke(amount);

            while (ShouldLevelUp())
            {
                await LevelUp();
            }
        }

        private bool ShouldLevelUp()
        {
            return Data.experiencePoints >= GetExperienceRequiredForNextLevel();
        }

        private float GetExperienceRequiredForNextLevel()
        {
            return Mathf.Pow(Data.playerLevel * 100, 1.1f);
        }

        private async Task LevelUp()
        {
            Data.playerLevel++;
            OnLevelUp?.Invoke(Data.playerLevel);
            await SaveProfile();
        }

    #endregion

    #region Currency Management

        public async Task<bool> AddCoins(int amount)
        {
            Data.coins += amount;
            OnCoinsChanged?.Invoke(Data.coins);
            await SaveProfile();
            return true;
        }

        public async Task<bool> SpendCoins(int amount)
        {
            if (Data.coins < amount)
                return false;

            Data.coins -= amount;
            OnCoinsChanged?.Invoke(Data.coins);
            await SaveProfile();
            return true;
        }

        public async Task<bool> AddGems(int amount)
        {
            Data.gems += amount;
            OnGemsChanged?.Invoke(Data.gems);
            await SaveProfile();
            return true;
        }

        public async Task<bool> SpendGems(int amount)
        {
            if (Data.gems < amount)
                return false;

            Data.gems -= amount;
            OnGemsChanged?.Invoke(Data.gems);
            await SaveProfile();
            return true;
        }

    #endregion
    }
}