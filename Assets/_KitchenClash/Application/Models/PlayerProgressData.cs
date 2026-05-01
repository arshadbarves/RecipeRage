using System;
using System.Collections.Generic;
using UnityEngine;

namespace KitchenClash.Application.Models
{
    [Serializable]
    public class PlayerProgressData : ISerializationCallbackReceiver
    {
        public string EosProductUserId = "";
        public int DataVersion = 1;
        public List<string> UnlockedCharacters = new List<string>();
        public List<string> UnlockedMaps = new List<string>();
        public List<string> UnlockedCosmetics = new List<string>();
        public int HighestLevel = 0;
        public Dictionary<string, int> GameModeHighScores = new Dictionary<string, int>();
        public Dictionary<string, float> GameModeBestTimes = new Dictionary<string, float>();
        public List<string> CompletedAchievements = new List<string>();
        public Dictionary<string, int> AchievementProgress = new Dictionary<string, int>();
        public Dictionary<string, int> CharacterLevels = new Dictionary<string, int>();
        public bool TutorialCompleted = false;

        [SerializeField] private List<string> _highScoreKeys = new List<string>();
        [SerializeField] private List<int> _highScoreValues = new List<int>();
        [SerializeField] private List<string> _bestTimeKeys = new List<string>();
        [SerializeField] private List<float> _bestTimeValues = new List<float>();
        [SerializeField] private List<string> _charLevelKeys = new List<string>();
        [SerializeField] private List<int> _charLevelValues = new List<int>();

        public void OnBeforeSerialize()
        {
            _highScoreKeys.Clear(); _highScoreValues.Clear();
            foreach (var kvp in GameModeHighScores) { _highScoreKeys.Add(kvp.Key); _highScoreValues.Add(kvp.Value); }
            _bestTimeKeys.Clear(); _bestTimeValues.Clear();
            foreach (var kvp in GameModeBestTimes) { _bestTimeKeys.Add(kvp.Key); _bestTimeValues.Add(kvp.Value); }
            _charLevelKeys.Clear(); _charLevelValues.Clear();
            foreach (var kvp in CharacterLevels) { _charLevelKeys.Add(kvp.Key); _charLevelValues.Add(kvp.Value); }
        }

        public void OnAfterDeserialize()
        {
            GameModeHighScores = new Dictionary<string, int>();
            for (int i = 0; i < Math.Min(_highScoreKeys.Count, _highScoreValues.Count); i++)
                GameModeHighScores[_highScoreKeys[i]] = _highScoreValues[i];
            GameModeBestTimes = new Dictionary<string, float>();
            for (int i = 0; i < Math.Min(_bestTimeKeys.Count, _bestTimeValues.Count); i++)
                GameModeBestTimes[_bestTimeKeys[i]] = _bestTimeValues[i];
            CharacterLevels = new Dictionary<string, int>();
            for (int i = 0; i < Math.Min(_charLevelKeys.Count, _charLevelValues.Count); i++)
                CharacterLevels[_charLevelKeys[i]] = _charLevelValues[i];
        }

        public void UnlockCharacter(string id) { if (!UnlockedCharacters.Contains(id)) UnlockedCharacters.Add(id); }
        public void UnlockMap(string id) { if (!UnlockedMaps.Contains(id)) UnlockedMaps.Add(id); }
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
