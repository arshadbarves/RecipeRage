using System;
using System.Collections.Generic;

namespace RecipeRage.Modules.Achievements.Interfaces
{
    /// <summary>
    /// Interface for achievements providers
    /// Implemented by specific achievement services like EOS
    /// 
    /// Complexity Rating: 3
    /// </summary>
    public interface IAchievementsProvider
    {
        /// <summary>
        /// Initialize the provider
        /// </summary>
        /// <param name="onComplete">Callback when initialization is complete</param>
        void Initialize(Action<bool> onComplete = null);
        
        /// <summary>
        /// Query player achievements from the provider
        /// </summary>
        /// <param name="onComplete">Callback with the achievements</param>
        void QueryAchievements(Action<List<Achievement>, string> onComplete);
        
        /// <summary>
        /// Query player stats from the provider
        /// </summary>
        /// <param name="onComplete">Callback with the stats</param>
        void QueryStats(Action<List<PlayerStat>, string> onComplete);
        
        /// <summary>
        /// Unlock achievement
        /// </summary>
        /// <param name="achievementId">ID of the achievement to unlock</param>
        /// <param name="onComplete">Callback when unlock is complete</param>
        void UnlockAchievement(string achievementId, Action<bool, string> onComplete = null);
        
        /// <summary>
        /// Update achievement progress
        /// </summary>
        /// <param name="achievementId">ID of the achievement to update</param>
        /// <param name="progress">Progress value (0.0 to 1.0)</param>
        /// <param name="onComplete">Callback when update is complete</param>
        void UpdateAchievementProgress(string achievementId, float progress, Action<bool, string> onComplete = null);
        
        /// <summary>
        /// Get definition of all achievements
        /// </summary>
        /// <param name="onComplete">Callback with the achievements definitions</param>
        void GetAchievementDefinitions(Action<List<AchievementDefinition>, string> onComplete);
        
        /// <summary>
        /// Update player stat
        /// </summary>
        /// <param name="statName">Name of the stat</param>
        /// <param name="value">New value</param>
        /// <param name="onComplete">Callback when update is complete</param>
        void UpdateStat(string statName, double value, Action<bool, string> onComplete = null);
        
        /// <summary>
        /// Increment player stat
        /// </summary>
        /// <param name="statName">Name of the stat</param>
        /// <param name="amount">Amount to increment</param>
        /// <param name="onComplete">Callback when update is complete</param>
        void IncrementStat(string statName, double amount, Action<bool, string> onComplete = null);
        
        /// <summary>
        /// Get stat definitions
        /// </summary>
        /// <param name="onComplete">Callback with the stat definitions</param>
        void GetStatDefinitions(Action<List<StatDefinition>, string> onComplete);
        
        /// <summary>
        /// Display achievement UI (if supported)
        /// </summary>
        /// <param name="onComplete">Callback when UI is closed</param>
        void DisplayAchievementUI(Action<bool> onComplete = null);
        
        /// <summary>
        /// Reset all achievements (if supported)
        /// </summary>
        /// <param name="onComplete">Callback when reset is complete</param>
        void ResetAchievements(Action<bool, string> onComplete = null);
        
        /// <summary>
        /// Reset all stats (if supported)
        /// </summary>
        /// <param name="onComplete">Callback when reset is complete</param>
        void ResetStats(Action<bool, string> onComplete = null);
        
        /// <summary>
        /// Get the provider name
        /// </summary>
        /// <returns>Provider name</returns>
        string GetProviderName();
        
        /// <summary>
        /// Check if the provider is available
        /// </summary>
        /// <returns>True if available, false otherwise</returns>
        bool IsAvailable();
    }
} 