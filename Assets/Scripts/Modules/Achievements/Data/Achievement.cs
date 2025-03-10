using System;
using UnityEngine;

namespace RecipeRage.Modules.Achievements.Interfaces
{
    /// <summary>
    /// Achievement data model representing a player achievement
    /// 
    /// Complexity Rating: 2
    /// </summary>
    [Serializable]
    public class Achievement
    {
        /// <summary>
        /// Achievement ID
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
        /// Achievement icon (if loaded)
        /// </summary>
        public Sprite Icon { get; set; }
        
        /// <summary>
        /// Icon URL for loading
        /// </summary>
        public string IconUrl { get; set; }
        
        /// <summary>
        /// Is the achievement hidden until unlocked
        /// </summary>
        public bool IsHidden { get; set; }
        
        /// <summary>
        /// Is the achievement unlocked
        /// </summary>
        public bool IsUnlocked { get; set; }
        
        /// <summary>
        /// Progress towards achievement (0.0 to 1.0)
        /// </summary>
        public float Progress { get; set; }
        
        /// <summary>
        /// Unlock time (UTC)
        /// </summary>
        public DateTime? UnlockTime { get; set; }
        
        /// <summary>
        /// Achievement XP reward
        /// </summary>
        public int XpReward { get; set; }
        
        /// <summary>
        /// Achievement rarity (percentage of players who have unlocked it)
        /// </summary>
        public float Rarity { get; set; }
        
        /// <summary>
        /// Provider-specific data
        /// </summary>
        public string ProviderData { get; set; }
        
        /// <summary>
        /// Provider name
        /// </summary>
        public string Provider { get; set; }
        
        /// <summary>
        /// Create a new achievement
        /// </summary>
        /// <param name="id">Achievement ID</param>
        /// <param name="title">Achievement title</param>
        /// <param name="description">Achievement description</param>
        /// <param name="provider">Provider name</param>
        public Achievement(string id, string title, string description, string provider)
        {
            Id = id;
            Title = title;
            Description = description;
            Provider = provider;
            
            IsHidden = false;
            IsUnlocked = false;
            Progress = 0f;
            UnlockTime = null;
            XpReward = 0;
            Rarity = 0f;
            ProviderData = string.Empty;
            IconUrl = string.Empty;
        }
        
        /// <summary>
        /// Default constructor for serialization
        /// </summary>
        public Achievement()
        {
            Id = string.Empty;
            Title = string.Empty;
            Description = string.Empty;
            Provider = string.Empty;
            
            IsHidden = false;
            IsUnlocked = false;
            Progress = 0f;
            UnlockTime = null;
            XpReward = 0;
            Rarity = 0f;
            ProviderData = string.Empty;
            IconUrl = string.Empty;
        }
        
        /// <summary>
        /// Set achievement as unlocked
        /// </summary>
        public void SetUnlocked()
        {
            IsUnlocked = true;
            Progress = 1.0f;
            UnlockTime = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Create a copy of the achievement
        /// </summary>
        /// <returns>A new copy of the achievement</returns>
        public Achievement Clone()
        {
            var clone = new Achievement
            {
                Id = Id,
                Title = Title,
                Description = Description,
                Icon = Icon,
                IconUrl = IconUrl,
                IsHidden = IsHidden,
                IsUnlocked = IsUnlocked,
                Progress = Progress,
                UnlockTime = UnlockTime,
                XpReward = XpReward,
                Rarity = Rarity,
                ProviderData = ProviderData,
                Provider = Provider
            };
            
            return clone;
        }
        
        /// <summary>
        /// Get display description (handles hidden achievements)
        /// </summary>
        /// <returns>Display description</returns>
        public string GetDisplayDescription()
        {
            if (IsHidden && !IsUnlocked)
            {
                return "This achievement is still locked.";
            }
            
            return Description;
        }
        
        /// <summary>
        /// Get display title (handles hidden achievements)
        /// </summary>
        /// <returns>Display title</returns>
        public string GetDisplayTitle()
        {
            if (IsHidden && !IsUnlocked)
            {
                return "???";
            }
            
            return Title;
        }
    }
} 