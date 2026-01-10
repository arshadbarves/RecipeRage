using System;
using System.Collections.Generic;
using Core.RemoteConfig.Interfaces;
using Newtonsoft.Json;

namespace Core.RemoteConfig.Models
{
    /// <summary>
    /// Configuration model for global game settings
    /// Contains game rules, currency rates, and feature flags
    /// </summary>
    [Serializable]
    public class GameSettingsConfig : IConfigModel
    {
        // Game Rules
        [JsonProperty("minPlayers")]
        public int MinPlayers { get; set; }

        [JsonProperty("maxPlayers")]
        public int MaxPlayers { get; set; }

        [JsonProperty("matchDurationSeconds")]
        public int MatchDurationSeconds { get; set; }

        [JsonProperty("scoreMultiplier")]
        public float ScoreMultiplier { get; set; }

        [JsonProperty("perfectDishBonus")]
        public int PerfectDishBonus { get; set; }

        // Currency Rates
        [JsonProperty("coinsPerWin")]
        public int CoinsPerWin { get; set; }

        [JsonProperty("coinsPerLoss")]
        public int CoinsPerLoss { get; set; }

        [JsonProperty("gemsPerRankedWin")]
        public int GemsPerRankedWin { get; set; }

        [JsonProperty("dailyRewardCoins")]
        public int DailyRewardCoins { get; set; }

        // Feature Flags
        [JsonProperty("enableRankedMode")]
        public bool EnableRankedMode { get; set; }

        [JsonProperty("enableDailyRewards")]
        public bool EnableDailyRewards { get; set; }

        [JsonProperty("enableSeasonPass")]
        public bool EnableSeasonPass { get; set; }

        [JsonProperty("enableChatSystem")]
        public bool EnableChatSystem { get; set; }

        [JsonProperty("enableSpectatorMode")]
        public bool EnableSpectatorMode { get; set; }

        [JsonProperty("customGameRules")]
        public Dictionary<string, object> CustomGameRules { get; set; }

        public GameSettingsConfig()
        {
            CustomGameRules = new Dictionary<string, object>();

            // Default values
            MinPlayers = 2;
            MaxPlayers = 4;
            MatchDurationSeconds = 180;
            ScoreMultiplier = 1.0f;
            PerfectDishBonus = 100;
            CoinsPerWin = 50;
            CoinsPerLoss = 10;
            GemsPerRankedWin = 5;
            DailyRewardCoins = 100;
            EnableRankedMode = true;
            EnableDailyRewards = true;
            EnableSeasonPass = true;
            EnableChatSystem = true;
            EnableSpectatorMode = false;
        }

        public bool Validate()
        {
            if (MinPlayers < 1 || MinPlayers > MaxPlayers)
            {
                return false;
            }

            if (MaxPlayers < 2 || MaxPlayers > 8)
            {
                return false;
            }

            if (MatchDurationSeconds < 30 || MatchDurationSeconds > 600)
            {
                return false;
            }

            if (ScoreMultiplier < 0.1f || ScoreMultiplier > 10.0f)
            {
                return false;
            }

            if (CoinsPerWin < 0 || CoinsPerLoss < 0 || GemsPerRankedWin < 0)
            {
                return false;
            }

            return true;
        }
    }
}
