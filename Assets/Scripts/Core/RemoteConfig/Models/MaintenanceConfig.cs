using System;
using Core.Core.RemoteConfig.Interfaces;
using Newtonsoft.Json;

namespace Core.Core.RemoteConfig.Models
{
    /// <summary>
    /// Configuration model for maintenance mode
    /// </summary>
    [Serializable]
    public class MaintenanceConfig : IConfigModel
    {
        [JsonProperty("isMaintenanceActive")]
        public bool IsMaintenanceActive { get; set; }

        [JsonProperty("maintenanceStartTimestamp")]
        public long MaintenanceStartTimestamp { get; set; }

        [JsonProperty("maintenanceEndTimestamp")]
        public long MaintenanceEndTimestamp { get; set; }

        [JsonProperty("estimatedDurationMinutes")]
        public int EstimatedDurationMinutes { get; set; }

        [JsonProperty("maintenanceMessage")]
        public string MaintenanceMessage { get; set; }

        [JsonProperty("maintenanceTitle")]
        public string MaintenanceTitle { get; set; }

        [JsonProperty("allowCurrentMatches")]
        public bool AllowCurrentMatches { get; set; }

        public MaintenanceConfig()
        {
            IsMaintenanceActive = false;
            AllowCurrentMatches = true;
            MaintenanceTitle = "Scheduled Maintenance";
            MaintenanceMessage = "We're performing scheduled maintenance to improve your experience.";
        }

        public bool Validate()
        {
            if (IsMaintenanceActive)
            {
                if (MaintenanceStartTimestamp <= 0 || MaintenanceEndTimestamp <= 0)
                {
                    return false;
                }

                if (MaintenanceEndTimestamp <= MaintenanceStartTimestamp)
                {
                    return false;
                }

                if (EstimatedDurationMinutes <= 0)
                {
                    return false;
                }
            }

            return true;
        }

        public DateTime GetStartTime()
        {
            return DateTimeOffset.FromUnixTimeSeconds(MaintenanceStartTimestamp).UtcDateTime;
        }

        public DateTime GetEndTime()
        {
            return DateTimeOffset.FromUnixTimeSeconds(MaintenanceEndTimestamp).UtcDateTime;
        }

        public bool IsInMaintenanceWindow(DateTime currentTime)
        {
            if (!IsMaintenanceActive)
            {
                return false;
            }

            return currentTime >= GetStartTime() && currentTime <= GetEndTime();
        }

        public TimeSpan GetTimeUntilMaintenance(DateTime currentTime)
        {
            var startTime = GetStartTime();
            return startTime > currentTime ? startTime - currentTime : TimeSpan.Zero;
        }
    }
}
