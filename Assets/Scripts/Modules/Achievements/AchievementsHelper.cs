using System;
using System.Collections.Generic;
using UnityEngine;
using RecipeRage.Modules.Achievements.Core;
using RecipeRage.Modules.Achievements.Interfaces;
using RecipeRage.Modules.Achievements.Providers.EOS;
using RecipeRage.Modules.Logging;

namespace RecipeRage.Modules.Achievements
{
    /// <summary>
    /// Static helper class for easy access to achievements functionality
    /// 
    /// Complexity Rating: 2
    /// </summary>
    public static class AchievementsHelper
    {
        /// <summary>
        /// Achievements service instance
        /// </summary>
        private static IAchievementsService _achievementsService;
        
        /// <summary>
        /// Whether the service is initialized
        /// </summary>
        private static bool _isInitialized = false;
        
        /// <summary>
        /// Static constructor
        /// </summary>
        static AchievementsHelper()
        {
            // Create service instance if it doesn't exist
            if (_achievementsService == null)
            {
                _achievementsService = new AchievementsService();
                LogHelper.Info("AchievementsHelper", "Created achievements service instance");
            }
        }
        
        /// <summary>
        /// Initialize the achievements service
        /// </summary>
        /// <param name="onComplete">Callback when initialization is complete</param>
        public static void Initialize(Action<bool> onComplete = null)
        {
            if (_isInitialized)
            {
                LogHelper.Warning("AchievementsHelper", "Achievements service is already initialized");
                onComplete?.Invoke(true);
                return;
            }
            
            EnsureServiceCreated();
            
            // Add EOS provider
            AddEOSProvider();
            
            // Initialize service
            _achievementsService.Initialize(success =>
            {
                if (success)
                {
                    _isInitialized = true;
                    LogHelper.Info("AchievementsHelper", "Achievements service initialized successfully");
                }
                else
                {
                    LogHelper.Error("AchievementsHelper", "Failed to initialize achievements service");
                }
                
                onComplete?.Invoke(success);
            });
        }
        
        /// <summary>
        /// Query player achievements
        /// </summary>
        /// <param name="forceRefresh">Whether to force a refresh from the provider</param>
        /// <param name="onComplete">Callback with achievements</param>
        public static void QueryAchievements(bool forceRefresh, Action<List<Achievement>, string> onComplete)
        {
            if (!CheckInitialized("QueryAchievements"))
            {
                onComplete?.Invoke(null, "Achievements service is not initialized");
                return;
            }
            
            _achievementsService.QueryAchievements(forceRefresh, onComplete);
        }
        
        /// <summary>
        /// Query player stats
        /// </summary>
        /// <param name="forceRefresh">Whether to force a refresh from the provider</param>
        /// <param name="onComplete">Callback with stats</param>
        public static void QueryStats(bool forceRefresh, Action<List<PlayerStat>, string> onComplete)
        {
            if (!CheckInitialized("QueryStats"))
            {
                onComplete?.Invoke(null, "Achievements service is not initialized");
                return;
            }
            
            _achievementsService.QueryStats(forceRefresh, onComplete);
        }
        
        /// <summary>
        /// Unlock an achievement
        /// </summary>
        /// <param name="achievementId">ID of the achievement to unlock</param>
        /// <param name="onComplete">Callback when unlock is complete</param>
        public static void UnlockAchievement(string achievementId, Action<bool, string> onComplete = null)
        {
            if (!CheckInitialized("UnlockAchievement"))
            {
                onComplete?.Invoke(false, "Achievements service is not initialized");
                return;
            }
            
            _achievementsService.UnlockAchievement(achievementId, onComplete);
        }
        
        /// <summary>
        /// Update achievement progress
        /// </summary>
        /// <param name="achievementId">ID of the achievement to update</param>
        /// <param name="progress">Progress value (0.0 to 1.0)</param>
        /// <param name="onComplete">Callback when update is complete</param>
        public static void UpdateAchievementProgress(string achievementId, float progress, Action<bool, string> onComplete = null)
        {
            if (!CheckInitialized("UpdateAchievementProgress"))
            {
                onComplete?.Invoke(false, "Achievements service is not initialized");
                return;
            }
            
            _achievementsService.UpdateAchievementProgress(achievementId, progress, onComplete);
        }
        
        /// <summary>
        /// Get achievement by ID
        /// </summary>
        /// <param name="achievementId">ID of the achievement</param>
        /// <returns>Achievement if found, null otherwise</returns>
        public static Achievement GetAchievement(string achievementId)
        {
            if (!CheckInitialized("GetAchievement"))
            {
                return null;
            }
            
            return _achievementsService.GetAchievement(achievementId);
        }
        
        /// <summary>
        /// Get achievement progress
        /// </summary>
        /// <param name="achievementId">ID of the achievement</param>
        /// <returns>Progress value (0.0 to 1.0)</returns>
        public static float GetAchievementProgress(string achievementId)
        {
            if (!CheckInitialized("GetAchievementProgress"))
            {
                return 0f;
            }
            
            return _achievementsService.GetAchievementProgress(achievementId);
        }
        
        /// <summary>
        /// Check if an achievement is unlocked
        /// </summary>
        /// <param name="achievementId">ID of the achievement</param>
        /// <returns>True if unlocked, false otherwise</returns>
        public static bool IsAchievementUnlocked(string achievementId)
        {
            if (!CheckInitialized("IsAchievementUnlocked"))
            {
                return false;
            }
            
            return _achievementsService.IsAchievementUnlocked(achievementId);
        }
        
        /// <summary>
        /// Update player stat
        /// </summary>
        /// <param name="statName">Name of the stat</param>
        /// <param name="value">New value</param>
        /// <param name="onComplete">Callback when update is complete</param>
        public static void UpdateStat(string statName, double value, Action<bool, string> onComplete = null)
        {
            if (!CheckInitialized("UpdateStat"))
            {
                onComplete?.Invoke(false, "Achievements service is not initialized");
                return;
            }
            
            _achievementsService.UpdateStat(statName, value, onComplete);
        }
        
        /// <summary>
        /// Increment player stat
        /// </summary>
        /// <param name="statName">Name of the stat</param>
        /// <param name="amount">Amount to increment</param>
        /// <param name="onComplete">Callback when update is complete</param>
        public static void IncrementStat(string statName, double amount, Action<bool, string> onComplete = null)
        {
            if (!CheckInitialized("IncrementStat"))
            {
                onComplete?.Invoke(false, "Achievements service is not initialized");
                return;
            }
            
            _achievementsService.IncrementStat(statName, amount, onComplete);
        }
        
        /// <summary>
        /// Get stat value
        /// </summary>
        /// <param name="statName">Name of the stat</param>
        /// <returns>Stat value or 0 if not found</returns>
        public static double GetStatValue(string statName)
        {
            if (!CheckInitialized("GetStatValue"))
            {
                return 0;
            }
            
            return _achievementsService.GetStatValue(statName);
        }
        
        /// <summary>
        /// Synchronize achievements with the provider
        /// </summary>
        /// <param name="onComplete">Callback when sync is complete</param>
        public static void SynchronizeAchievements(Action<bool, string> onComplete = null)
        {
            if (!CheckInitialized("SynchronizeAchievements"))
            {
                onComplete?.Invoke(false, "Achievements service is not initialized");
                return;
            }
            
            _achievementsService.SynchronizeAchievements(onComplete);
        }
        
        /// <summary>
        /// Synchronize stats with the provider
        /// </summary>
        /// <param name="onComplete">Callback when sync is complete</param>
        public static void SynchronizeStats(Action<bool, string> onComplete = null)
        {
            if (!CheckInitialized("SynchronizeStats"))
            {
                onComplete?.Invoke(false, "Achievements service is not initialized");
                return;
            }
            
            _achievementsService.SynchronizeStats(onComplete);
        }
        
        /// <summary>
        /// Display achievement UI (if supported by provider)
        /// </summary>
        /// <param name="onComplete">Callback when UI is closed</param>
        public static void DisplayAchievementUI(Action<bool> onComplete = null)
        {
            if (!CheckInitialized("DisplayAchievementUI"))
            {
                onComplete?.Invoke(false);
                return;
            }
            
            // Get EOS provider
            var provider = _achievementsService.GetProvider("EOSAchievements");
            if (provider != null)
            {
                provider.DisplayAchievementUI(onComplete);
            }
            else
            {
                LogHelper.Warning("AchievementsHelper", "No provider available to display achievement UI");
                onComplete?.Invoke(false);
            }
        }
        
        /// <summary>
        /// Reset all achievements (if supported by provider)
        /// </summary>
        /// <param name="onComplete">Callback when reset is complete</param>
        public static void ResetAchievements(Action<bool, string> onComplete = null)
        {
            if (!CheckInitialized("ResetAchievements"))
            {
                onComplete?.Invoke(false, "Achievements service is not initialized");
                return;
            }
            
            _achievementsService.ResetAchievements(onComplete);
        }
        
        /// <summary>
        /// Reset all stats (if supported by provider)
        /// </summary>
        /// <param name="onComplete">Callback when reset is complete</param>
        public static void ResetStats(Action<bool, string> onComplete = null)
        {
            if (!CheckInitialized("ResetStats"))
            {
                onComplete?.Invoke(false, "Achievements service is not initialized");
                return;
            }
            
            _achievementsService.ResetStats(onComplete);
        }
        
        /// <summary>
        /// Check if the achievements service is initialized
        /// </summary>
        /// <returns>True if initialized, false otherwise</returns>
        public static bool IsInitialized()
        {
            return _isInitialized && _achievementsService != null && _achievementsService.IsInitialized();
        }
        
        /// <summary>
        /// Register for achievement unlocked events
        /// </summary>
        /// <param name="callback">Callback for achievement unlocked events</param>
        public static void RegisterAchievementUnlockedCallback(Action<Achievement> callback)
        {
            if (!CheckInitialized("RegisterAchievementUnlockedCallback"))
            {
                return;
            }
            
            _achievementsService.OnAchievementUnlocked += callback;
        }
        
        /// <summary>
        /// Unregister from achievement unlocked events
        /// </summary>
        /// <param name="callback">Callback to unregister</param>
        public static void UnregisterAchievementUnlockedCallback(Action<Achievement> callback)
        {
            if (_achievementsService != null)
            {
                _achievementsService.OnAchievementUnlocked -= callback;
            }
        }
        
        /// <summary>
        /// Register for achievement progress updated events
        /// </summary>
        /// <param name="callback">Callback for achievement progress updated events</param>
        public static void RegisterAchievementProgressUpdatedCallback(Action<Achievement, float> callback)
        {
            if (!CheckInitialized("RegisterAchievementProgressUpdatedCallback"))
            {
                return;
            }
            
            _achievementsService.OnAchievementProgressUpdated += callback;
        }
        
        /// <summary>
        /// Unregister from achievement progress updated events
        /// </summary>
        /// <param name="callback">Callback to unregister</param>
        public static void UnregisterAchievementProgressUpdatedCallback(Action<Achievement, float> callback)
        {
            if (_achievementsService != null)
            {
                _achievementsService.OnAchievementProgressUpdated -= callback;
            }
        }
        
        /// <summary>
        /// Register for stat updated events
        /// </summary>
        /// <param name="callback">Callback for stat updated events</param>
        public static void RegisterStatUpdatedCallback(Action<PlayerStat, double> callback)
        {
            if (!CheckInitialized("RegisterStatUpdatedCallback"))
            {
                return;
            }
            
            _achievementsService.OnStatUpdated += callback;
        }
        
        /// <summary>
        /// Unregister from stat updated events
        /// </summary>
        /// <param name="callback">Callback to unregister</param>
        public static void UnregisterStatUpdatedCallback(Action<PlayerStat, double> callback)
        {
            if (_achievementsService != null)
            {
                _achievementsService.OnStatUpdated -= callback;
            }
        }
        
        /// <summary>
        /// Register for achievements synchronized events
        /// </summary>
        /// <param name="callback">Callback for achievements synchronized events</param>
        public static void RegisterAchievementsSynchronizedCallback(Action<bool, string> callback)
        {
            if (!CheckInitialized("RegisterAchievementsSynchronizedCallback"))
            {
                return;
            }
            
            _achievementsService.OnAchievementsSynchronized += callback;
        }
        
        /// <summary>
        /// Unregister from achievements synchronized events
        /// </summary>
        /// <param name="callback">Callback to unregister</param>
        public static void UnregisterAchievementsSynchronizedCallback(Action<bool, string> callback)
        {
            if (_achievementsService != null)
            {
                _achievementsService.OnAchievementsSynchronized -= callback;
            }
        }
        
        /// <summary>
        /// Register for stats synchronized events
        /// </summary>
        /// <param name="callback">Callback for stats synchronized events</param>
        public static void RegisterStatsSynchronizedCallback(Action<bool, string> callback)
        {
            if (!CheckInitialized("RegisterStatsSynchronizedCallback"))
            {
                return;
            }
            
            _achievementsService.OnStatsSynchronized += callback;
        }
        
        /// <summary>
        /// Unregister from stats synchronized events
        /// </summary>
        /// <param name="callback">Callback to unregister</param>
        public static void UnregisterStatsSynchronizedCallback(Action<bool, string> callback)
        {
            if (_achievementsService != null)
            {
                _achievementsService.OnStatsSynchronized -= callback;
            }
        }
        
        /// <summary>
        /// Set the achievements service instance
        /// </summary>
        /// <param name="service">Service instance</param>
        public static void SetAchievementsService(IAchievementsService service)
        {
            if (service == null)
            {
                LogHelper.Error("AchievementsHelper", "Cannot set null achievements service");
                return;
            }
            
            _achievementsService = service;
            _isInitialized = service.IsInitialized();
            LogHelper.Info("AchievementsHelper", "Achievements service instance set externally");
        }
        
        /// <summary>
        /// Add a provider to the service
        /// </summary>
        /// <param name="provider">Provider to add</param>
        public static void AddProvider(IAchievementsProvider provider)
        {
            if (provider == null)
            {
                LogHelper.Error("AchievementsHelper", "Cannot add null provider");
                return;
            }
            
            EnsureServiceCreated();
            _achievementsService.AddProvider(provider);
            LogHelper.Info("AchievementsHelper", $"Added provider: {provider.GetProviderName()}");
        }
        
        /// <summary>
        /// Add EOS provider
        /// </summary>
        private static void AddEOSProvider()
        {
            try
            {
                var eosProvider = new EOSAchievementsProvider();
                AddProvider(eosProvider);
            }
            catch (Exception ex)
            {
                LogHelper.Error("AchievementsHelper", $"Failed to create EOS provider: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Ensure service instance is created
        /// </summary>
        private static void EnsureServiceCreated()
        {
            if (_achievementsService == null)
            {
                _achievementsService = new AchievementsService();
                LogHelper.Info("AchievementsHelper", "Created achievements service instance");
            }
        }
        
        /// <summary>
        /// Check if the achievements service is initialized
        /// </summary>
        /// <param name="methodName">Method name for logging</param>
        /// <returns>True if initialized, false otherwise</returns>
        private static bool CheckInitialized(string methodName = null)
        {
            if (!_isInitialized || _achievementsService == null || !_achievementsService.IsInitialized())
            {
                if (!string.IsNullOrEmpty(methodName))
                {
                    LogHelper.Warning("AchievementsHelper", $"{methodName}: Achievements service is not initialized");
                }
                
                return false;
            }
            
            return true;
        }
    }
} 