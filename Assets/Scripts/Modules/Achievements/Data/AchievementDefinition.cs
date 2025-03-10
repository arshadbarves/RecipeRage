using System;
using UnityEngine;

namespace RecipeRage.Modules.Achievements.Interfaces
{
    /// <summary>
    /// Achievement definition from provider
    /// Contains static achievement data from provider
    /// 
    /// Complexity Rating: 2
    /// </summary>
    [Serializable]
    public class AchievementDefinition
    {
        /// <summary>
        /// Achievement ID (unique identifier)
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Achievement title
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Achievement description
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Locked description (for hidden achievements)
        /// </summary>
        public string LockedDescription { get; set; }
        
        /// <summary>
        /// Achievement icon (if loaded)
        /// </summary>
        public Sprite UnlockedIcon { get; set; }
        
        /// <summary>
        /// Locked icon (if loaded)
        /// </summary>
        public Sprite LockedIcon { get; set; }
        
        /// <summary>
        /// Unlocked icon URL for loading
        /// </summary>
        public string UnlockedIconUrl { get; set; }
        
        /// <summary>
        /// Locked icon URL for loading
        /// </summary>
        public string LockedIconUrl { get; set; }
        
        /// <summary>
        /// Is the achievement hidden until unlocked
        /// </summary>
        public bool IsHidden { get; set; }
        
        /// <summary>
        /// Achievement XP reward
        /// </summary>
        public int XpReward { get; set; }
        
        /// <summary>
        /// Is the achievement a stat threshold achievement (requires reaching a stat value)
        /// </summary>
        public bool IsStatBased { get; set; }
        
        /// <summary>
        /// Stat name (if IsStatBased is true)
        /// </summary>
        public string StatName { get; set; }
        
        /// <summary>
        /// Threshold value required to unlock (if IsStatBased is true)
        /// </summary>
        public double ThresholdValue { get; set; }
        
        /// <summary>
        /// Provider-specific data
        /// </summary>
        public string ProviderData { get; set; }
        
        /// <summary>
        /// Provider name
        /// </summary>
        public string Provider { get; set; }
        
        /// <summary>
        /// Create a new achievement definition
        /// </summary>
        /// <param name="id">Achievement ID</param>
        /// <param name="title">Achievement title</param>
        /// <param name="description">Achievement description</param>
        /// <param name="provider">Provider name</param>
        public AchievementDefinition(string id, string title, string description, string provider)
        {
            Id = id;
            Title = title;
            Description = description;
            Provider = provider;
            
            IsHidden = false;
            LockedDescription = "This achievement is locked";
            XpReward = 0;
            IsStatBased = false;
            StatName = string.Empty;
            ThresholdValue = 0;
            ProviderData = string.Empty;
            UnlockedIconUrl = string.Empty;
            LockedIconUrl = string.Empty;
        }
        
        /// <summary>
        /// Default constructor for serialization
        /// </summary>
        public AchievementDefinition()
        {
            Id = string.Empty;
            Title = string.Empty;
            Description = string.Empty;
            LockedDescription = "This achievement is locked";
            Provider = string.Empty;
            
            IsHidden = false;
            XpReward = 0;
            IsStatBased = false;
            StatName = string.Empty;
            ThresholdValue = 0;
            ProviderData = string.Empty;
            UnlockedIconUrl = string.Empty;
            LockedIconUrl = string.Empty;
        }
        
        /// <summary>
        /// Create an Achievement from this definition
        /// </summary>
        /// <returns>Achievement object</returns>
        public Achievement CreateAchievement()
        {
            var achievement = new Achievement
            {
                Id = Id,
                Title = Title,
                Description = Description,
                Provider = Provider,
                IsHidden = IsHidden,
                IconUrl = UnlockedIconUrl,
                Icon = UnlockedIcon,
                XpReward = XpReward,
                ProviderData = ProviderData
            };
            
            return achievement;
        }
    }
} 