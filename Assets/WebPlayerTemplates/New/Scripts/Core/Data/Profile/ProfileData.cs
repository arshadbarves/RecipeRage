using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Data.Profile
{
    [Serializable]
    public class ProfileData
    {
        [Header("Player Info")]
        public string playerId;
        public string playerName;
        public int playerLevel;
        public float experiencePoints;

        [Header("Statistics")]
        public int totalGamesPlayed;
        public int gamesWon;
        public int totalOrders;
        public int perfectOrders;
        public float totalPlayTime;
        public int highestCombo;
        public float bestTime;

        [Header("Currency")]
        public int coins;
        public int gems;
        public int seasonPass;

        [Header("Progression")]
        public int[] unlockedRecipes;
        public int[] unlockedCharacters;
        public int[] unlockedKitchens;

        [Header("Settings")]
        public GameSettings settings;
        public DateTime AccountCreationDate;
        public Dictionary<string, int> AchievementProgress;
        public DateTime LastLoginDate;

        public ProfileData(string id, string name)
        {
            playerId = id;
            playerName = name;
            AccountCreationDate = DateTime.UtcNow;
            InitializeDefaultValues();
        }

        private void InitializeDefaultValues()
        {
            playerLevel = 1;
            experiencePoints = 0;
            LastLoginDate = DateTime.UtcNow;

            coins = 1000; // Starting coins
            gems = 50; // Starting gems

            unlockedRecipes = new[] {
                1
            }; // Basic recipe
            unlockedCharacters = new[] {
                1
            }; // Default character
            unlockedKitchens = new[] {
                1
            }; // First kitchen

            AchievementProgress = new Dictionary<string, int>();
            settings = new GameSettings();
        }

        [Serializable]
        public class GameSettings
        {
            public float masterVolume = 1f;
            public float musicVolume = 1f;
            public float sfxVolume = 1f;
            public bool vibrationEnabled = true;
            public string preferredRegion = "auto";
            public int graphicsQuality = 1;
            public int targetFrameRate = 60;
            public bool showFPS;
        }
    }
}