using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Logging;

namespace UI.Data
{
    /// <summary>
    /// Map information data structure
    /// </summary>
    [Serializable]
    public class MapInfo
    {
        public string id;
        public string name;
        public string subtitle;
        public string description;
        public string icon;
        public string thumbnail;
        public int maxPlayers;
        public int rotationTime; // in seconds
        public bool isAvailable;
    }

    /// <summary>
    /// Map database loaded from JSON
    /// </summary>
    [Serializable]
    public class MapDatabase
    {
        public List<MapInfo> maps;
        public string currentMapId;
        public string rotationStartTime; // ISO 8601 format
        
        /// <summary>
        /// Get map by ID
        /// </summary>
        public MapInfo GetMapById(string mapId)
        {
            return maps?.Find(m => m.id == mapId);
        }
        
        /// <summary>
        /// Get current map
        /// </summary>
        public MapInfo GetCurrentMap()
        {
            return GetMapById(currentMapId);
        }
        
        /// <summary>
        /// Get available maps
        /// </summary>
        public List<MapInfo> GetAvailableMaps()
        {
            return maps?.FindAll(m => m.isAvailable) ?? new List<MapInfo>();
        }
        
        /// <summary>
        /// Calculate time remaining until next rotation
        /// </summary>
        public TimeSpan GetTimeUntilRotation()
        {
            if (string.IsNullOrEmpty(rotationStartTime))
                return TimeSpan.Zero;
            
            try
            {
                DateTime startTime = DateTime.Parse(rotationStartTime);
                MapInfo currentMap = GetCurrentMap();
                
                if (currentMap == null)
                    return TimeSpan.Zero;
                
                DateTime nextRotation = startTime.AddSeconds(currentMap.rotationTime);
                TimeSpan remaining = nextRotation - DateTime.UtcNow;
                
                return remaining.TotalSeconds > 0 ? remaining : TimeSpan.Zero;
            }
            catch (Exception e)
            {
                GameLogger.LogError($"Error calculating rotation time: {e.Message}");
                return TimeSpan.Zero;
            }
        }
    }
}
