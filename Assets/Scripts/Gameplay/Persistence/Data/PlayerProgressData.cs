using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Persistence.Data
{
    [Serializable]
    public class PlayerProgressData : ISerializationCallbackReceiver
    {
        // Unlocked content
        public List<string> UnlockedCharacters = new List<string>();
        public List<string> UnlockedMaps = new List<string>();
        public List<string> UnlockedCosmetics = new List<string>();

        // Game progress
        public int HighestLevel = 0;
        public Dictionary<string, int> GameModeHighScores = new Dictionary<string, int>();
        public Dictionary<string, float> GameModeBestTimes = new Dictionary<string, float>();

        // Achievements
        public List<string> CompletedAchievements = new List<string>();
        public Dictionary<string, int> AchievementProgress = new Dictionary<string, int>();

        // Tutorial
        public bool TutorialCompleted = false;

        // Serialization Helpers
        [SerializeField] private List<string> _highScoreKeys = new List<string>();
        [SerializeField] private List<int> _highScoreValues = new List<int>();
        [SerializeField] private List<string> _bestTimeKeys = new List<string>();
        [SerializeField] private List<float> _bestTimeValues = new List<float>();
        [SerializeField] private List<string> _achievementProgressKeys = new List<string>();
        [SerializeField] private List<int> _achievementProgressValues = new List<int>();

        public void OnBeforeSerialize()
        {
            _highScoreKeys.Clear(); _highScoreValues.Clear();
            foreach (var kvp in GameModeHighScores) { _highScoreKeys.Add(kvp.Key); _highScoreValues.Add(kvp.Value); }

            _bestTimeKeys.Clear(); _bestTimeValues.Clear();
            foreach (var kvp in GameModeBestTimes) { _bestTimeKeys.Add(kvp.Key); _bestTimeValues.Add(kvp.Value); }

            _achievementProgressKeys.Clear(); _achievementProgressValues.Clear();
            foreach (var kvp in AchievementProgress) { _achievementProgressKeys.Add(kvp.Key); _achievementProgressValues.Add(kvp.Value); }
        }

        public void OnAfterDeserialize()
        {
            GameModeHighScores = new Dictionary<string, int>();
            for (int i = 0; i < Math.Min(_highScoreKeys.Count, _highScoreValues.Count); i++)
                GameModeHighScores[_highScoreKeys[i]] = _highScoreValues[i];

            GameModeBestTimes = new Dictionary<string, float>();
            for (int i = 0; i < Math.Min(_bestTimeKeys.Count, _bestTimeValues.Count); i++)
                GameModeBestTimes[_bestTimeKeys[i]] = _bestTimeValues[i];

            AchievementProgress = new Dictionary<string, int>();
            for (int i = 0; i < Math.Min(_achievementProgressKeys.Count, _achievementProgressValues.Count); i++)
                AchievementProgress[_achievementProgressKeys[i]] = _achievementProgressValues[i];
        }

        // Helper Methods
        public bool IsCharacterUnlocked(string id) => UnlockedCharacters.Contains(id);
        public void UnlockCharacter(string id) { if (!UnlockedCharacters.Contains(id)) UnlockedCharacters.Add(id); }
        public bool IsMapUnlocked(string id) => UnlockedMaps.Contains(id);
        public void UnlockMap(string id) { if (!UnlockedMaps.Contains(id)) UnlockedMaps.Add(id); }
        public bool IsCosmeticUnlocked(string id) => UnlockedCosmetics.Contains(id);
        public void UnlockCosmetic(string id) { if (!UnlockedCosmetics.Contains(id)) UnlockedCosmetics.Add(id); }

        public bool UpdateHighScore(string gameModeId, int score)
        {
            if (!GameModeHighScores.ContainsKey(gameModeId) || score > GameModeHighScores[gameModeId])
            {
                GameModeHighScores[gameModeId] = score;
                return true;
            }
            return false;
        }

        public bool UpdateBestTime(string gameModeId, float time)
        {
            if (!GameModeBestTimes.ContainsKey(gameModeId) || time < GameModeBestTimes[gameModeId])
            {
                GameModeBestTimes[gameModeId] = time;
                return true;
            }
            return false;
        }
    }
}
