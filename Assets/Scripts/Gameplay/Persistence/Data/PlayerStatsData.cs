using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Persistence.Data
{
    [Serializable]
    public class PlayerStatsData : ISerializationCallbackReceiver
    {
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
    }
}
