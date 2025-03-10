using System;

namespace RecipeRage.Modules.Achievements.Interfaces
{
    /// <summary>
    /// Player stat data model
    /// Represents a player statistic tracked by the achievements system
    /// 
    /// Complexity Rating: 1
    /// </summary>
    [Serializable]
    public class PlayerStat
    {
        /// <summary>
        /// Stat name (unique identifier)
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Display name (human-readable)
        /// </summary>
        public string DisplayName { get; set; }
        
        /// <summary>
        /// Stat value
        /// </summary>
        public double Value { get; set; }
        
        /// <summary>
        /// Last updated time (UTC)
        /// </summary>
        public DateTime LastUpdated { get; set; }
        
        /// <summary>
        /// Provider-specific data
        /// </summary>
        public string ProviderData { get; set; }
        
        /// <summary>
        /// Provider name
        /// </summary>
        public string Provider { get; set; }
        
        /// <summary>
        /// Create a new player stat
        /// </summary>
        /// <param name="name">Stat name</param>
        /// <param name="displayName">Display name</param>
        /// <param name="value">Initial value</param>
        /// <param name="provider">Provider name</param>
        public PlayerStat(string name, string displayName, double value, string provider)
        {
            Name = name;
            DisplayName = displayName;
            Value = value;
            Provider = provider;
            
            LastUpdated = DateTime.UtcNow;
            ProviderData = string.Empty;
        }
        
        /// <summary>
        /// Default constructor for serialization
        /// </summary>
        public PlayerStat()
        {
            Name = string.Empty;
            DisplayName = string.Empty;
            Value = 0;
            Provider = string.Empty;
            
            LastUpdated = DateTime.UtcNow;
            ProviderData = string.Empty;
        }
        
        /// <summary>
        /// Update the stat value
        /// </summary>
        /// <param name="newValue">New value</param>
        public void UpdateValue(double newValue)
        {
            Value = newValue;
            LastUpdated = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Increment the stat value
        /// </summary>
        /// <param name="amount">Amount to increment</param>
        public void IncrementValue(double amount)
        {
            Value += amount;
            LastUpdated = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Create a copy of the stat
        /// </summary>
        /// <returns>A new copy of the stat</returns>
        public PlayerStat Clone()
        {
            return new PlayerStat
            {
                Name = Name,
                DisplayName = DisplayName,
                Value = Value,
                LastUpdated = LastUpdated,
                ProviderData = ProviderData,
                Provider = Provider
            };
        }
        
        /// <summary>
        /// Get formatted value string
        /// </summary>
        /// <returns>Formatted value string</returns>
        public string GetFormattedValue()
        {
            // Handle integer values
            if (Math.Abs(Value - Math.Floor(Value)) < double.Epsilon)
            {
                return ((int)Value).ToString();
            }
            
            // Return with 2 decimal places for floating point
            return Value.ToString("F2");
        }
    }
} 