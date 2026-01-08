using System;
using System.Collections.Generic;
using UnityEngine;
using Modules.Logging;
using Core.RemoteConfig;

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
        public string gameMode; // "2v2", "3v3", "4v4"
        public int rotationTime; // in seconds
        public string timerText; // "New Event in: 20h 45m"
        public bool isAvailable;
    }

    /// <summary>
    /// Map category (like Brawl Stars)
    /// </summary>
    [Serializable]
    public class MapCategory
    {
        public string id;
        public string name;
        public string backgroundColor;
        public List<MapInfo> maps;
    }

    /// <summary>
    /// Map database loaded from JSON
    /// </summary>
    [Serializable]
    public class MapDatabase
    {
        public List<MapCategory> categories;
        public string currentMapId;
        public string rotationStartTime; // ISO 8601 format
        
        /// <summary>
        /// Get map by ID (search all categories)
        /// </summary>
        public MapInfo GetMapById(string mapId)
        {
            if (categories == null) return null;
            
            foreach (var category in categories)
            {
                var map = category.maps?.Find(m => m.id == mapId);
                if (map != null) return map;
            }
            return null;
        }
        
        /// <summary>
        /// Get current map
        /// </summary>
        public MapInfo GetCurrentMap()
        {
            return GetMapById(currentMapId);
        }
        
        /// <summary>
        /// Get available maps from all categories
        /// </summary>
        public List<MapInfo> GetAvailableMaps()
        {
            var allMaps = new List<MapInfo>();
            if (categories == null) return allMaps;
            
            foreach (var category in categories)
            {
                if (category.maps != null)
                {
                    allMaps.AddRange(category.maps.FindAll(m => m.isAvailable));
                }
            }
            return allMaps;
        }
        
        /// <summary>
        /// Get category by ID
        /// </summary>
        public MapCategory GetCategoryById(string categoryId)
        {
            return categories?.Find(c => c.id == categoryId);
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
                TimeSpan remaining = nextRotation - NTPTime.UtcNow;
                
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
