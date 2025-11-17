using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Core.RemoteConfig.Models
{
    /// <summary>
    /// Configuration model for map definitions with individual rotation support
    /// </summary>
    [Serializable]
    public class MapConfig : IConfigModel
    {
        [JsonProperty("maps")]
        public List<MapDefinition> Maps { get; set; }

        [JsonProperty("rotationSchedule")]
        public MapRotationSchedule RotationSchedule { get; set; }

        public MapConfig()
        {
            Maps = new List<MapDefinition>();
        }

        public bool Validate()
        {
            if (Maps == null || Maps.Count == 0)
            {
                return false;
            }

            foreach (var map in Maps)
            {
                if (string.IsNullOrEmpty(map.MapId))
                {
                    return false;
                }

                if (map.MatchDurationSeconds <= 0)
                {
                    return false;
                }

                if (map.TrophyRewardWin < 0 || map.TrophyRewardLoss > 0)
                {
                    return false;
                }
            }

            return true;
        }
    }

    [Serializable]
    public class MapDefinition
    {
        [JsonProperty("mapId")]
        public string MapId { get; set; }

        [JsonProperty("mapName")]
        public string MapName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("difficulty")]
        public string Difficulty { get; set; }

        [JsonProperty("matchDurationSeconds")]
        public int MatchDurationSeconds { get; set; }

        [JsonProperty("minPlayers")]
        public int MinPlayers { get; set; }

        [JsonProperty("maxPlayers")]
        public int MaxPlayers { get; set; }

        // Trophy Rewards
        [JsonProperty("trophyRewardWin")]
        public int TrophyRewardWin { get; set; }

        [JsonProperty("trophyRewardLoss")]
        public int TrophyRewardLoss { get; set; }

        // Rotation Configuration
        [JsonProperty("rotationConfig")]
        public MapRotationConfig RotationConfig { get; set; }

        // Asset References
        [JsonProperty("sceneAddress")]
        public string SceneAddress { get; set; }

        [JsonProperty("thumbnailAddress")]
        public string ThumbnailAddress { get; set; }

        [JsonProperty("previewAddress")]
        public string PreviewAddress { get; set; }

        public MapDefinition()
        {
            MinPlayers = 2;
            MaxPlayers = 4;
            MatchDurationSeconds = 180;
            TrophyRewardWin = 30;
            TrophyRewardLoss = -10;
        }
    }

    [Serializable]
    public class MapRotationConfig
    {
        [JsonProperty("isInRotation")]
        public bool IsInRotation { get; set; }

        [JsonProperty("rotationDurationHours")]
        public int RotationDurationHours { get; set; }

        [JsonProperty("isAlwaysAvailable")]
        public bool IsAlwaysAvailable { get; set; }

        public MapRotationConfig()
        {
            IsInRotation = true;
            RotationDurationHours = 24;
            IsAlwaysAvailable = false;
        }
    }

    [Serializable]
    public class MapRotationSchedule
    {
        [JsonProperty("rotationPeriods")]
        public List<MapRotationPeriod> RotationPeriods { get; set; }

        [JsonProperty("defaultRotationDurationHours")]
        public int DefaultRotationDurationHours { get; set; }

        public MapRotationSchedule()
        {
            RotationPeriods = new List<MapRotationPeriod>();
            DefaultRotationDurationHours = 24;
        }
    }

    [Serializable]
    public class MapRotationPeriod
    {
        [JsonProperty("periodId")]
        public string PeriodId { get; set; }

        [JsonProperty("startTimestamp")]
        public long StartTimestamp { get; set; }

        [JsonProperty("endTimestamp")]
        public long EndTimestamp { get; set; }

        [JsonProperty("activeMapIds")]
        public List<string> ActiveMapIds { get; set; }

        public MapRotationPeriod()
        {
            ActiveMapIds = new List<string>();
        }

        public DateTime GetStartTime()
        {
            return DateTimeOffset.FromUnixTimeSeconds(StartTimestamp).UtcDateTime;
        }

        public DateTime GetEndTime()
        {
            return DateTimeOffset.FromUnixTimeSeconds(EndTimestamp).UtcDateTime;
        }
    }
}
