using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Persistence.Data
{
    [Serializable]
    public class PlayerStatsData : ISerializationCallbackReceiver
    {
        // Account Linking Fields (AC3)
        [Header("Account Information")]
        public string EosProductUserId = ""; // Set when linked to EOS
        public string LinkedAccountType = ""; // "DeviceID", "Epic", "Steam", etc.
        public DateTime AccountCreatedAt = DateTime.UtcNow;
        public DateTime LastLinkedAt = DateTime.MinValue;
        public int AccountLinkingVersion = 1; // For migration tracking

        // Player info
        public string PlayerName = "";
        public int UsernameChangeCount = 0;
        public string PlayerId = Guid.NewGuid().ToString();
        public int Level = 1;
        public int Experience = 0;
        public int ExperienceToNextLevel = 100;

        // Game stats
        public int GamesPlayed = 0;
        public int GamesWon = 0;
        public int GamesLost = 0;
        public float TotalPlayTime = 0f;
        public int TotalScore = 0;
        public int TotalOrdersCompleted = 0;
        public int TotalOrdersFailed = 0;
        public int TotalIngredientsCut = 0;
        public int TotalIngredientsBurned = 0;
        public int TotalDishesServed = 0;
        public int HighestCombo = 0;

        // Character stats
        public Dictionary<string, int> CharacterUsage = new Dictionary<string, int>();
        public string FavoriteCharacter = "";

        // Game mode stats
        public Dictionary<string, int> GameModeUsage = new Dictionary<string, int>();
        public string FavoriteGameMode = "";

        // Serialization Helpers
        [SerializeField] private List<string> _charUsageKeys = new List<string>();
        [SerializeField] private List<int> _charUsageValues = new List<int>();
        [SerializeField] private List<string> _gameModeUsageKeys = new List<string>();
        [SerializeField] private List<int> _gameModeUsageValues = new List<int>();

        public void OnBeforeSerialize()
        {
            _charUsageKeys.Clear(); _charUsageValues.Clear();
            foreach (var kvp in CharacterUsage) { _charUsageKeys.Add(kvp.Key); _charUsageValues.Add(kvp.Value); }

            _gameModeUsageKeys.Clear(); _gameModeUsageValues.Clear();
            foreach (var kvp in GameModeUsage) { _gameModeUsageKeys.Add(kvp.Key); _gameModeUsageValues.Add(kvp.Value); }
        }

        public void OnAfterDeserialize()
        {
            CharacterUsage = new Dictionary<string, int>();
            for (int i = 0; i < Math.Min(_charUsageKeys.Count, _charUsageValues.Count); i++)
                CharacterUsage[_charUsageKeys[i]] = _charUsageValues[i];

            GameModeUsage = new Dictionary<string, int>();
            for (int i = 0; i < Math.Min(_gameModeUsageKeys.Count, _gameModeUsageValues.Count); i++)
                GameModeUsage[_gameModeUsageKeys[i]] = _gameModeUsageValues[i];
        }

        // Helper Methods
        public bool AddExperience(int amount)
        {
            Experience += amount;
            bool leveledUp = false;
            while (Experience >= ExperienceToNextLevel)
            {
                Experience -= ExperienceToNextLevel;
                Level++;
                ExperienceToNextLevel = 100 * Level;
                leveledUp = true;
            }
            return leveledUp;
        }

        public void RecordGamePlayed(bool won, string gameModeId, string characterId, float playTime, int score)
        {
            GamesPlayed++;
            if (won) GamesWon++; else GamesLost++;
            TotalPlayTime += playTime;
            TotalScore += score;

            if (!string.IsNullOrEmpty(characterId))
            {
                if (!CharacterUsage.ContainsKey(characterId)) CharacterUsage[characterId] = 0;
                CharacterUsage[characterId]++;
                if (string.IsNullOrEmpty(FavoriteCharacter) || CharacterUsage[characterId] > CharacterUsage[FavoriteCharacter])
                    FavoriteCharacter = characterId;
            }

            if (!string.IsNullOrEmpty(gameModeId))
            {
                if (!GameModeUsage.ContainsKey(gameModeId)) GameModeUsage[gameModeId] = 0;
                GameModeUsage[gameModeId]++;
                if (string.IsNullOrEmpty(FavoriteGameMode) || GameModeUsage[gameModeId] > GameModeUsage[FavoriteGameMode])
                    FavoriteGameMode = gameModeId;
            }
        }

        public void RecordOrderCompleted() => TotalOrdersCompleted++;
        public void RecordOrderFailed() => TotalOrdersFailed++;
        public void RecordIngredientCut() => TotalIngredientsCut++;
        public void RecordIngredientBurned() => TotalIngredientsBurned++;
        public void RecordDishServed() => TotalDishesServed++;
        public void RecordCombo(int combo) { if (combo > HighestCombo) HighestCombo = combo; }

        #region Account Linking Support (AC3)

        /// <summary>
        /// Links this guest account to a permanent EOS account.
        /// Call this after successful account linking via EOS Connect.
        /// </summary>
        public void LinkToEosAccount(string eosProductUserId, string accountType)
        {
            if (string.IsNullOrEmpty(eosProductUserId))
            {
                Debug.LogError("[PlayerStatsData] Cannot link - EOS ProductUserId is null/empty");
                return;
            }

            EosProductUserId = eosProductUserId;
            LinkedAccountType = accountType;
            LastLinkedAt = DateTime.UtcNow;
            AccountLinkingVersion++;

            Debug.Log($"[PlayerStatsData] Account linked to {accountType} - EOS PUID: {eosProductUserId}");
        }

        /// <summary>
        /// Checks if this account has been linked to a permanent account.
        /// </summary>
        public bool IsLinkedToPermanentAccount => !string.IsNullOrEmpty(EosProductUserId);

        /// <summary>
        /// Prepares data for account migration.
        /// Returns a snapshot of data that can be transferred to a linked account.
        /// </summary>
        public PlayerStatsData CreateMigrationSnapshot()
        {
            return new PlayerStatsData
            {
                // Preserve progression
                Level = this.Level,
                Experience = this.Experience,
                ExperienceToNextLevel = this.ExperienceToNextLevel,

                // Preserve stats
                GamesPlayed = this.GamesPlayed,
                GamesWon = this.GamesWon,
                GamesLost = this.GamesLost,
                TotalPlayTime = this.TotalPlayTime,
                TotalScore = this.TotalScore,
                TotalOrdersCompleted = this.TotalOrdersCompleted,
                TotalOrdersFailed = this.TotalOrdersFailed,
                TotalIngredientsCut = this.TotalIngredientsCut,
                TotalIngredientsBurned = this.TotalIngredientsBurned,
                TotalDishesServed = this.TotalDishesServed,
                HighestCombo = this.HighestCombo,

                // Preserve character usage
                CharacterUsage = new Dictionary<string, int>(this.CharacterUsage),
                FavoriteCharacter = this.FavoriteCharacter,

                // Preserve game mode usage
                GameModeUsage = new Dictionary<string, int>(this.GameModeUsage),
                FavoriteGameMode = this.FavoriteGameMode,

                // Account linking info
                EosProductUserId = this.EosProductUserId,
                LinkedAccountType = this.LinkedAccountType,
                AccountCreatedAt = this.AccountCreatedAt,
                LastLinkedAt = DateTime.UtcNow,
                AccountLinkingVersion = this.AccountLinkingVersion + 1
            };
        }

        #endregion
    }
}
