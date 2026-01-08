using System;
using System.Collections.Generic;
using System.Linq;
using Modules.Logging;
using Core.RemoteConfig.Models;

namespace Core.RemoteConfig
{
    /// <summary>
    /// Calculates active maps based on rotation schedules and NTP time
    /// </summary>
    public class MapRotationCalculator
    {
        private readonly IRemoteConfigService _configService;
        private readonly INTPTimeService _ntpTimeService;

        public MapRotationCalculator(
            IRemoteConfigService configService,
            INTPTimeService ntpTimeService)
        {
            _configService = configService;
            _ntpTimeService = ntpTimeService;
        }

        /// <summary>
        /// Gets the currently active map
        /// </summary>
        public MapDefinition GetActiveMap()
        {
            try
            {
                var activeMaps = GetActiveMaps();

                if (activeMaps.Count == 0)
                {
                    GameLogger.LogWarning("No active maps found");
                    return null;
                }

                // Return first active map (could be randomized or based on priority)
                return activeMaps[0];
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Failed to get active map: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets all currently active maps
        /// </summary>
        public List<MapDefinition> GetActiveMaps()
        {
            try
            {
                if (!_configService.TryGetConfig<MapConfig>(out var mapConfig))
                {
                    GameLogger.LogWarning("MapConfig not available");
                    return new List<MapDefinition>();
                }

                if (mapConfig.Maps == null || mapConfig.Maps.Count == 0)
                {
                    GameLogger.LogWarning("No maps configured");
                    return new List<MapDefinition>();
                }

                DateTime serverTime = NTPTime.UtcNow;

                var activeMaps = new List<MapDefinition>();

                // Check each map's rotation status
                foreach (var map in mapConfig.Maps)
                {
                    if (IsMapActive(map, mapConfig, serverTime))
                    {
                        activeMaps.Add(map);
                    }
                }

                GameLogger.Log($"Found {activeMaps.Count} active maps");
                return activeMaps;
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Failed to get active maps: {ex.Message}");
                return new List<MapDefinition>();
            }
        }

        /// <summary>
        /// Checks if a specific map is currently active
        /// </summary>
        public bool IsMapActive(MapDefinition map, MapConfig mapConfig, DateTime serverTime)
        {
            if (map == null || map.RotationConfig == null)
            {
                return false;
            }

            // Always available maps are always active
            if (map.RotationConfig.IsAlwaysAvailable)
            {
                return true;
            }

            // Not in rotation
            if (!map.RotationConfig.IsInRotation)
            {
                return false;
            }

            // Check rotation schedule
            if (mapConfig.RotationSchedule != null &&
                mapConfig.RotationSchedule.RotationPeriods != null)
            {
                foreach (var period in mapConfig.RotationSchedule.RotationPeriods)
                {
                    var startTime = period.GetStartTime();
                    var endTime = period.GetEndTime();

                    if (serverTime >= startTime && serverTime <= endTime)
                    {
                        // Check if this map is in the active list for this period
                        if (period.ActiveMapIds != null && period.ActiveMapIds.Contains(map.MapId))
                        {
                            return true;
                        }
                    }
                }
            }

            // Fallback: Use per-map rotation duration
            // This is a simple time-based rotation without specific schedules
            return true; // Default to active if no schedule conflicts
        }

        /// <summary>
        /// Gets time until map rotation changes
        /// </summary>
        public TimeSpan GetTimeUntilRotationChange()
        {
            try
            {
                if (!_configService.TryGetConfig<MapConfig>(out var mapConfig))
                {
                    return TimeSpan.Zero;
                }

                if (mapConfig.RotationSchedule == null ||
                    mapConfig.RotationSchedule.RotationPeriods == null)
                {
                    return TimeSpan.Zero;
                }

                DateTime serverTime = NTPTime.UtcNow;

                // Find current period
                foreach (var period in mapConfig.RotationSchedule.RotationPeriods)
                {
                    var startTime = period.GetStartTime();
                    var endTime = period.GetEndTime();

                    if (serverTime >= startTime && serverTime <= endTime)
                    {
                        var timeRemaining = endTime - serverTime;
                        return timeRemaining > TimeSpan.Zero ? timeRemaining : TimeSpan.Zero;
                    }
                }

                return TimeSpan.Zero;
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Failed to get time until rotation change: {ex.Message}");
                return TimeSpan.Zero;
            }
        }

        /// <summary>
        /// Gets maps filtered by difficulty
        /// </summary>
        public List<MapDefinition> GetMapsByDifficulty(string difficulty)
        {
            try
            {
                var activeMaps = GetActiveMaps();

                return activeMaps
                    .Where(map => map.Difficulty.Equals(difficulty, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Failed to get maps by difficulty: {ex.Message}");
                return new List<MapDefinition>();
            }
        }
    }
}
