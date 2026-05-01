using System;
using KitchenClash.Domain;

namespace KitchenClash.Infrastructure.Configuration
{
    [Serializable]
    public class GameSettingsConfig : IConfigModel
    {
        public string Version { get; set; } = "1.0.0";
        public int MaxPlayersPerMatch { get; set; } = 6;
        public float MatchDurationSeconds { get; set; } = 180f;
        public bool MaintenanceMode { get; set; }
        public string MinimumAppVersion { get; set; } = "1.0.0";
        
        public bool IsValid() => !string.IsNullOrEmpty(Version);
    }
}
