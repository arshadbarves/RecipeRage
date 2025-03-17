using System;
using System.Collections.Generic;

namespace RecipeRage.Modules.Achievements.Interfaces
{
    /// <summary>
    /// Interface for achievements services
    /// Provides unified achievements and stats capabilities throughout the application
    /// Complexity Rating: 3
    /// </summary>
    public interface IAchievementsService
    {
        /// <summary>
        /// Event triggered when an achievement is unlocked
        /// </summary>
        event Action<Achievement> OnAchievementUnlocked;

        /// <summary>
        /// Event triggered when achievement progress is updated
        /// </summary>
        event Action<Achievement, float> OnAchievementProgressUpdated;

        /// <summary>
        /// Event triggered when a stat is updated
        /// </summary>
        event Action<PlayerStat, double> OnStatUpdated;

        /// <summary>
        /// Event triggered when achievements are synchronized
        /// </summary>
        event Action<bool, string> OnAchievementsSynchronized;

        /// <summary>
        /// Event triggered when stats are synchronized
        /// </summary>
        event Action<bool, string> OnStatsSynchronized;

        /// <summary>
        /// Initialize the achievements service
        /// </summary>
        /// <param name="onComplete"> Callback when initialization is complete </param>
        void Initialize(Action<bool> onComplete = null);

        /// <summary>
        /// Query player achievements from the provider
        /// </summary>
        /// <param name="forceRefresh"> Whether to force a refresh from the provider </param>
        /// <param name="onComplete"> Callback with the achievements </param>
        void QueryAchievements(bool forceRefresh, Action<List<Achievement>, string> onComplete);

        /// <summary>
        /// Query player stats from the provider
        /// </summary>
        /// <param name="forceRefresh"> Whether to force a refresh from the provider </param>
        /// <param name="onComplete"> Callback with the stats </param>
        void QueryStats(bool forceRefresh, Action<List<PlayerStat>, string> onComplete);

        /// <summary>
        /// Unlock an achievement
        /// </summary>
        /// <param name="achievementId"> ID of the achievement to unlock </param>
        /// <param name="onComplete"> Callback when unlock is complete </param>
        void UnlockAchievement(string achievementId, Action<bool, string> onComplete = null);

        /// <summary>
        /// Update achievement progress
        /// </summary>
        /// <param name="achievementId"> ID of the achievement to update </param>
        /// <param name="progress"> Progress value (0.0 to 1.0) </param>
        /// <param name="onComplete"> Callback when update is complete </param>
        void UpdateAchievementProgress(string achievementId, float progress, Action<bool, string> onComplete = null);

        /// <summary>
        /// Get achievement by ID
        /// </summary>
        /// <param name="achievementId"> ID of the achievement </param>
        /// <returns> Achievement if found, null otherwise </returns>
        Achievement GetAchievement(string achievementId);

        /// <summary>
        /// Get achievement progress
        /// </summary>
        /// <param name="achievementId"> ID of the achievement </param>
        /// <returns> Progress value (0.0 to 1.0) </returns>
        float GetAchievementProgress(string achievementId);

        /// <summary>
        /// Check if an achievement is unlocked
        /// </summary>
        /// <param name="achievementId"> ID of the achievement </param>
        /// <returns> True if unlocked, false otherwise </returns>
        bool IsAchievementUnlocked(string achievementId);

        /// <summary>
        /// Update player stat
        /// </summary>
        /// <param name="statName"> Name of the stat </param>
        /// <param name="value"> New value </param>
        /// <param name="onComplete"> Callback when update is complete </param>
        void UpdateStat(string statName, double value, Action<bool, string> onComplete = null);

        /// <summary>
        /// Increment player stat
        /// </summary>
        /// <param name="statName"> Name of the stat </param>
        /// <param name="amount"> Amount to increment </param>
        /// <param name="onComplete"> Callback when update is complete </param>
        void IncrementStat(string statName, double amount, Action<bool, string> onComplete = null);

        /// <summary>
        /// Get stat value
        /// </summary>
        /// <param name="statName"> Name of the stat </param>
        /// <returns> Stat value or 0 if not found </returns>
        double GetStatValue(string statName);

        /// <summary>
        /// Synchronize achievements with the provider
        /// </summary>
        /// <param name="onComplete"> Callback when sync is complete </param>
        void SynchronizeAchievements(Action<bool, string> onComplete = null);

        /// <summary>
        /// Synchronize stats with the provider
        /// </summary>
        /// <param name="onComplete"> Callback when sync is complete </param>
        void SynchronizeStats(Action<bool, string> onComplete = null);

        /// <summary>
        /// Reset all achievements (if supported by provider)
        /// </summary>
        /// <param name="onComplete"> Callback when reset is complete </param>
        void ResetAchievements(Action<bool, string> onComplete = null);

        /// <summary>
        /// Reset all stats (if supported by provider)
        /// </summary>
        /// <param name="onComplete"> Callback when reset is complete </param>
        void ResetStats(Action<bool, string> onComplete = null);

        /// <summary>
        /// Check if the service is initialized
        /// </summary>
        /// <returns> True if initialized, false otherwise </returns>
        bool IsInitialized();

        /// <summary>
        /// Add an achievements provider
        /// </summary>
        /// <param name="provider"> The provider to add </param>
        void AddProvider(IAchievementsProvider provider);

        /// <summary>
        /// Get provider by name
        /// </summary>
        /// <param name="providerName"> Name of the provider </param>
        /// <returns> The provider if found, null otherwise </returns>
        IAchievementsProvider GetProvider(string providerName);
    }
}