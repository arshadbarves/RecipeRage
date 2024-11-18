using System;
using System.Collections.Generic;
using GameSystem.Progression;
using Newtonsoft.Json;
using UnityEngine;

namespace GameSystem.Player
{
    [Serializable]
    public class PlayerData
    {
        [JsonProperty] private readonly Dictionary<string, int> _brawlerExperience = new Dictionary<string, int>();
        [JsonProperty] private readonly Dictionary<string, int> _brawlerLevels = new Dictionary<string, int>();
        [JsonProperty] private readonly List<string> _unlockedBrawlers = new List<string>();
        [JsonProperty] public int Trophies { get; set; }
        [JsonProperty] public int Coins { get; set; }
        [JsonProperty] public int Gems { get; set; }
        [JsonProperty] public int Tokens { get; set; }
        [JsonProperty] public int Level { get; set; }
        [JsonProperty] public int Experience { get; set; }
        public IReadOnlyList<string> UnlockedBrawlers => _unlockedBrawlers;
        public IReadOnlyDictionary<string, int> BrawlerLevels => _brawlerLevels;
        public IReadOnlyDictionary<string, int> BrawlerExperience => _brawlerExperience;
        public void AddTrophies(int amount)
        {
            Trophies += Math.Max(0, amount);
        }
        public void AddCoins(int amount)
        {
            Coins += Math.Max(0, amount);
        }
        public void AddGems(int amount)
        {
            Gems += Math.Max(0, amount);
        }
        public void AddTokens(int amount)
        {
            Tokens += Math.Max(0, amount);
        }
        public void AddExperience(int amount)
        {
            Experience += Math.Max(0, amount);
        }

        public void SetBrawlerLevel(string brawlerName, int level)
        {
            level = Math.Max(0, level);
            _brawlerLevels[brawlerName] = level;
        }

        public void SetBrawlerExperience(string brawlerName, int experience)
        {
            experience = Math.Max(0, experience);
            _brawlerExperience[brawlerName] = experience;
        }

        public int GetBrawlerLevel(string brawlerName)
        {
            return _brawlerLevels.GetValueOrDefault(brawlerName, 1);
        }

        public int GetBrawlerExperience(string brawlerName)
        {
            return _brawlerExperience.GetValueOrDefault(brawlerName, 0);
        }

        public bool IsBrawlerUnlocked(string brawlerName)
        {
            return _unlockedBrawlers.Contains(brawlerName);
        }

        public void UnlockBrawler(string brawlerName)
        {
            if (!_unlockedBrawlers.Contains(brawlerName))
            {
                _unlockedBrawlers.Add(brawlerName);
            }
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
        public static PlayerData FromJson(string json)
        {
            return JsonConvert.DeserializeObject<PlayerData>(json);
        }

        public void UpdateStat(StatType statType, int value)
        {
            switch (statType)
            {
                case StatType.Trophies:
                    AddTrophies(value);
                    break;
                case StatType.Experience:
                    AddExperience(value);
                    break;
                case StatType.Level:
                    Level += value;
                    break;
                case StatType.Coins:
                    AddCoins(value);
                    break;
                case StatType.Tokens:
                    AddTokens(value);
                    break;
                default:
                    Debug.LogWarning($"StatType {statType} not handled");
                    break;
            }
        }
    }
}