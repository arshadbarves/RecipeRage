using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RecipeRage.Modules.Achievements.Interfaces;
using RecipeRage.Modules.Logging;

namespace RecipeRage.Modules.Achievements.Core
{
    /// <summary>
    /// Main implementation of the achievements service
    /// Manages providers and handles achievement and stat operations
    /// 
    /// Complexity Rating: 4
    /// </summary>
    public class AchievementsService : IAchievementsService
    {
        /// <summary>
        /// List of available providers
        /// </summary>
        private readonly List<IAchievementsProvider> _providers = new List<IAchievementsProvider>();
        
        /// <summary>
        /// Cache of player achievements
        /// </summary>
        private Dictionary<string, Achievement> _achievementsCache = new Dictionary<string, Achievement>();
        
        /// <summary>
        /// Cache of achievement definitions
        /// </summary>
        private Dictionary<string, AchievementDefinition> _achievementDefinitions = new Dictionary<string, AchievementDefinition>();
        
        /// <summary>
        /// Cache of player stats
        /// </summary>
        private Dictionary<string, PlayerStat> _statsCache = new Dictionary<string, PlayerStat>();
        
        /// <summary>
        /// Cache of stat definitions
        /// </summary>
        private Dictionary<string, StatDefinition> _statDefinitions = new Dictionary<string, StatDefinition>();
        
        /// <summary>
        /// Whether the service is initialized
        /// </summary>
        private bool _isInitialized = false;
        
        /// <summary>
        /// Last error message
        /// </summary>
        private string _lastError = string.Empty;
        
        /// <summary>
        /// Event triggered when an achievement is unlocked
        /// </summary>
        public event Action<Achievement> OnAchievementUnlocked;
        
        /// <summary>
        /// Event triggered when achievement progress is updated
        /// </summary>
        public event Action<Achievement, float> OnAchievementProgressUpdated;
        
        /// <summary>
        /// Event triggered when a stat is updated
        /// </summary>
        public event Action<PlayerStat, double> OnStatUpdated;
        
        /// <summary>
        /// Event triggered when achievements are synchronized
        /// </summary>
        public event Action<bool, string> OnAchievementsSynchronized;
        
        /// <summary>
        /// Event triggered when stats are synchronized
        /// </summary>
        public event Action<bool, string> OnStatsSynchronized;
        
        /// <summary>
        /// Create a new achievements service
        /// </summary>
        public AchievementsService()
        {
            LogHelper.Info("AchievementsService", "Created achievements service instance");
        }
        
        /// <summary>
        /// Initialize the achievements service
        /// </summary>
        /// <param name="onComplete">Callback when initialization is complete</param>
        public void Initialize(Action<bool> onComplete = null)
        {
            if (_isInitialized)
            {
                LogHelper.Warning("AchievementsService", "Service is already initialized");
                onComplete?.Invoke(true);
                return;
            }
            
            LogHelper.Info("AchievementsService", "Initializing achievements service");
            
            // Check if we have any providers
            if (_providers.Count == 0)
            {
                _lastError = "No providers registered";
                LogHelper.Error("AchievementsService", _lastError);
                onComplete?.Invoke(false);
                return;
            }
            
            // Initialize all providers
            int successCount = 0;
            int totalCount = _providers.Count;
            
            foreach (var provider in _providers)
            {
                provider.Initialize(success =>
                {
                    if (success)
                    {
                        LogHelper.Info("AchievementsService", $"Provider {provider.GetProviderName()} initialized successfully");
                        successCount++;
                    }
                    else
                    {
                        LogHelper.Error("AchievementsService", $"Failed to initialize provider {provider.GetProviderName()}");
                    }
                    
                    // Check if all providers are initialized
                    if (successCount + (totalCount - successCount) == totalCount)
                    {
                        bool allSuccess = successCount == totalCount;
                        _isInitialized = successCount > 0;
                        
                        if (_isInitialized)
                        {
                            LogHelper.Info("AchievementsService", $"Achievements service initialized with {successCount}/{totalCount} providers");
                            
                            // Load achievement definitions
                            LoadAchievementDefinitions();
                            
                            // Load stat definitions
                            LoadStatDefinitions();
                            
                            // Load player achievements
                            QueryAchievements(true, (achievements, error) => {
                                if (!string.IsNullOrEmpty(error))
                                {
                                    LogHelper.Warning("AchievementsService", $"Failed to load initial achievements: {error}");
                                }
                                
                                // Load player stats
                                QueryStats(true, (stats, statsError) => {
                                    if (!string.IsNullOrEmpty(statsError))
                                    {
                                        LogHelper.Warning("AchievementsService", $"Failed to load initial stats: {statsError}");
                                    }
                                    
                                    onComplete?.Invoke(_isInitialized);
                                });
                            });
                        }
                        else
                        {
                            _lastError = "Failed to initialize all providers";
                            LogHelper.Error("AchievementsService", _lastError);
                            onComplete?.Invoke(false);
                        }
                    }
                });
            }
        }
        
        /// <summary>
        /// Add a provider to the service
        /// </summary>
        /// <param name="provider">The provider to add</param>
        public void AddProvider(IAchievementsProvider provider)
        {
            if (provider == null)
            {
                LogHelper.Error("AchievementsService", "Cannot add null provider");
                return;
            }
            
            // Check if provider already exists
            if (_providers.Any(p => p.GetProviderName() == provider.GetProviderName()))
            {
                LogHelper.Warning("AchievementsService", $"Provider {provider.GetProviderName()} is already registered");
                return;
            }
            
            _providers.Add(provider);
            LogHelper.Info("AchievementsService", $"Added provider: {provider.GetProviderName()}");
        }
        
        /// <summary>
        /// Get a provider by name
        /// </summary>
        /// <param name="providerName">Name of the provider</param>
        /// <returns>The provider if found, null otherwise</returns>
        public IAchievementsProvider GetProvider(string providerName)
        {
            return _providers.FirstOrDefault(p => p.GetProviderName() == providerName);
        }
        
        /// <summary>
        /// Query player achievements from the provider
        /// </summary>
        /// <param name="forceRefresh">Whether to force a refresh from the provider</param>
        /// <param name="onComplete">Callback with the achievements</param>
        public void QueryAchievements(bool forceRefresh, Action<List<Achievement>, string> onComplete)
        {
            if (!_isInitialized)
            {
                string error = "Service is not initialized";
                LogHelper.Error("AchievementsService", error);
                onComplete?.Invoke(null, error);
                return;
            }
            
            // If we have cached achievements and don't need to refresh, return them
            if (!forceRefresh && _achievementsCache.Count > 0)
            {
                onComplete?.Invoke(_achievementsCache.Values.ToList(), null);
                return;
            }
            
            // Use the first available provider
            var provider = _providers.FirstOrDefault(p => p.IsAvailable());
            if (provider == null)
            {
                string error = "No available providers";
                LogHelper.Error("AchievementsService", error);
                onComplete?.Invoke(null, error);
                return;
            }
            
            // Query achievements from provider
            provider.QueryAchievements((achievements, error) =>
            {
                if (achievements != null)
                {
                    // Update cache
                    _achievementsCache.Clear();
                    foreach (var achievement in achievements)
                    {
                        _achievementsCache[achievement.Id] = achievement;
                    }
                    
                    LogHelper.Info("AchievementsService", $"Loaded {achievements.Count} achievements");
                    onComplete?.Invoke(achievements, null);
                }
                else
                {
                    LogHelper.Error("AchievementsService", $"Failed to query achievements: {error}");
                    onComplete?.Invoke(null, error);
                }
            });
        }
        
        /// <summary>
        /// Query player stats from the provider
        /// </summary>
        /// <param name="forceRefresh">Whether to force a refresh from the provider</param>
        /// <param name="onComplete">Callback with the stats</param>
        public void QueryStats(bool forceRefresh, Action<List<PlayerStat>, string> onComplete)
        {
            if (!_isInitialized)
            {
                string error = "Service is not initialized";
                LogHelper.Error("AchievementsService", error);
                onComplete?.Invoke(null, error);
                return;
            }
            
            // If we have cached stats and don't need to refresh, return them
            if (!forceRefresh && _statsCache.Count > 0)
            {
                onComplete?.Invoke(_statsCache.Values.ToList(), null);
                return;
            }
            
            // Use the first available provider
            var provider = _providers.FirstOrDefault(p => p.IsAvailable());
            if (provider == null)
            {
                string error = "No available providers";
                LogHelper.Error("AchievementsService", error);
                onComplete?.Invoke(null, error);
                return;
            }
            
            // Query stats from provider
            provider.QueryStats((stats, error) =>
            {
                if (stats != null)
                {
                    // Update cache
                    _statsCache.Clear();
                    foreach (var stat in stats)
                    {
                        _statsCache[stat.Name] = stat;
                    }
                    
                    LogHelper.Info("AchievementsService", $"Loaded {stats.Count} stats");
                    onComplete?.Invoke(stats, null);
                }
                else
                {
                    LogHelper.Error("AchievementsService", $"Failed to query stats: {error}");
                    onComplete?.Invoke(null, error);
                }
            });
        }
        
        /// <summary>
        /// Unlock an achievement
        /// </summary>
        /// <param name="achievementId">ID of the achievement to unlock</param>
        /// <param name="onComplete">Callback when unlock is complete</param>
        public void UnlockAchievement(string achievementId, Action<bool, string> onComplete = null)
        {
            if (!_isInitialized)
            {
                string error = "Service is not initialized";
                LogHelper.Error("AchievementsService", error);
                onComplete?.Invoke(false, error);
                return;
            }
            
            // Check if achievement exists and is not already unlocked
            if (!_achievementsCache.TryGetValue(achievementId, out Achievement achievement))
            {
                string error = $"Achievement {achievementId} not found in cache";
                LogHelper.Warning("AchievementsService", error);
                
                // Try to get achievement definition
                if (_achievementDefinitions.TryGetValue(achievementId, out AchievementDefinition definition))
                {
                    achievement = definition.CreateAchievement();
                    _achievementsCache[achievementId] = achievement;
                }
                else
                {
                    onComplete?.Invoke(false, error);
                    return;
                }
            }
            
            // Check if already unlocked
            if (achievement.IsUnlocked)
            {
                LogHelper.Info("AchievementsService", $"Achievement {achievementId} is already unlocked");
                onComplete?.Invoke(true, null);
                return;
            }
            
            // Use the first available provider
            var provider = _providers.FirstOrDefault(p => p.IsAvailable());
            if (provider == null)
            {
                string error = "No available providers";
                LogHelper.Error("AchievementsService", error);
                onComplete?.Invoke(false, error);
                return;
            }
            
            // Unlock achievement
            provider.UnlockAchievement(achievementId, (success, error) =>
            {
                if (success)
                {
                    LogHelper.Info("AchievementsService", $"Achievement {achievementId} unlocked");
                    
                    // Update cache
                    achievement.SetUnlocked();
                    
                    // Trigger event
                    OnAchievementUnlocked?.Invoke(achievement);
                    OnAchievementProgressUpdated?.Invoke(achievement, 1.0f);
                    
                    onComplete?.Invoke(true, null);
                }
                else
                {
                    LogHelper.Error("AchievementsService", $"Failed to unlock achievement {achievementId}: {error}");
                    onComplete?.Invoke(false, error);
                }
            });
        }
        
        /// <summary>
        /// Update achievement progress
        /// </summary>
        /// <param name="achievementId">ID of the achievement to update</param>
        /// <param name="progress">Progress value (0.0 to 1.0)</param>
        /// <param name="onComplete">Callback when update is complete</param>
        public void UpdateAchievementProgress(string achievementId, float progress, Action<bool, string> onComplete = null)
        {
            if (!_isInitialized)
            {
                string error = "Service is not initialized";
                LogHelper.Error("AchievementsService", error);
                onComplete?.Invoke(false, error);
                return;
            }
            
            // Validate progress value
            progress = Mathf.Clamp01(progress);
            
            // Check if achievement exists
            if (!_achievementsCache.TryGetValue(achievementId, out Achievement achievement))
            {
                string error = $"Achievement {achievementId} not found in cache";
                LogHelper.Warning("AchievementsService", error);
                
                // Try to get achievement definition
                if (_achievementDefinitions.TryGetValue(achievementId, out AchievementDefinition definition))
                {
                    achievement = definition.CreateAchievement();
                    _achievementsCache[achievementId] = achievement;
                }
                else
                {
                    onComplete?.Invoke(false, error);
                    return;
                }
            }
            
            // Check if already unlocked or progress is lower than current
            if (achievement.IsUnlocked || progress <= achievement.Progress)
            {
                LogHelper.Info("AchievementsService", 
                    achievement.IsUnlocked ? 
                    $"Achievement {achievementId} is already unlocked" : 
                    $"Progress {progress} is not higher than current progress {achievement.Progress}");
                
                onComplete?.Invoke(true, null);
                return;
            }
            
            // Use the first available provider
            var provider = _providers.FirstOrDefault(p => p.IsAvailable());
            if (provider == null)
            {
                string error = "No available providers";
                LogHelper.Error("AchievementsService", error);
                onComplete?.Invoke(false, error);
                return;
            }
            
            // Update achievement progress
            provider.UpdateAchievementProgress(achievementId, progress, (success, error) =>
            {
                if (success)
                {
                    LogHelper.Info("AchievementsService", $"Achievement {achievementId} progress updated to {progress:P0}");
                    
                    // Update cache
                    achievement.Progress = progress;
                    
                    // Check if now unlocked
                    if (progress >= 1.0f && !achievement.IsUnlocked)
                    {
                        achievement.SetUnlocked();
                        OnAchievementUnlocked?.Invoke(achievement);
                    }
                    
                    // Trigger progress event
                    OnAchievementProgressUpdated?.Invoke(achievement, progress);
                    
                    onComplete?.Invoke(true, null);
                }
                else
                {
                    LogHelper.Error("AchievementsService", $"Failed to update achievement {achievementId} progress: {error}");
                    onComplete?.Invoke(false, error);
                }
            });
        }
        
        /// <summary>
        /// Get achievement by ID
        /// </summary>
        /// <param name="achievementId">ID of the achievement</param>
        /// <returns>Achievement if found, null otherwise</returns>
        public Achievement GetAchievement(string achievementId)
        {
            if (_achievementsCache.TryGetValue(achievementId, out Achievement achievement))
            {
                return achievement.Clone(); // Return a copy to prevent modification
            }
            
            // Not found in cache, try to create from definition
            if (_achievementDefinitions.TryGetValue(achievementId, out AchievementDefinition definition))
            {
                return definition.CreateAchievement();
            }
            
            return null;
        }
        
        /// <summary>
        /// Get achievement progress
        /// </summary>
        /// <param name="achievementId">ID of the achievement</param>
        /// <returns>Progress value (0.0 to 1.0)</returns>
        public float GetAchievementProgress(string achievementId)
        {
            if (_achievementsCache.TryGetValue(achievementId, out Achievement achievement))
            {
                return achievement.Progress;
            }
            
            return 0f;
        }
        
        /// <summary>
        /// Check if an achievement is unlocked
        /// </summary>
        /// <param name="achievementId">ID of the achievement</param>
        /// <returns>True if unlocked, false otherwise</returns>
        public bool IsAchievementUnlocked(string achievementId)
        {
            if (_achievementsCache.TryGetValue(achievementId, out Achievement achievement))
            {
                return achievement.IsUnlocked;
            }
            
            return false;
        }
        
        /// <summary>
        /// Update player stat
        /// </summary>
        /// <param name="statName">Name of the stat</param>
        /// <param name="value">New value</param>
        /// <param name="onComplete">Callback when update is complete</param>
        public void UpdateStat(string statName, double value, Action<bool, string> onComplete = null)
        {
            if (!_isInitialized)
            {
                string error = "Service is not initialized";
                LogHelper.Error("AchievementsService", error);
                onComplete?.Invoke(false, error);
                return;
            }
            
            // Use the first available provider
            var provider = _providers.FirstOrDefault(p => p.IsAvailable());
            if (provider == null)
            {
                string error = "No available providers";
                LogHelper.Error("AchievementsService", error);
                onComplete?.Invoke(false, error);
                return;
            }
            
            // Apply aggregation if there's a definition
            if (_statDefinitions.TryGetValue(statName, out StatDefinition definition))
            {
                double currentValue = GetStatValue(statName);
                value = definition.GetAggregatedValue(currentValue, value);
            }
            
            // Update stat on provider
            provider.UpdateStat(statName, value, (success, error) =>
            {
                if (success)
                {
                    LogHelper.Info("AchievementsService", $"Stat {statName} updated to {value}");
                    
                    // Create or update cached stat
                    if (!_statsCache.TryGetValue(statName, out PlayerStat stat))
                    {
                        // Try to create from definition
                        if (_statDefinitions.TryGetValue(statName, out StatDefinition def))
                        {
                            stat = def.CreatePlayerStat();
                        }
                        else
                        {
                            // Create new with default values
                            stat = new PlayerStat(statName, statName, value, provider.GetProviderName());
                        }
                        
                        _statsCache[statName] = stat;
                    }
                    
                    // Update value
                    stat.UpdateValue(value);
                    
                    // Trigger event
                    OnStatUpdated?.Invoke(stat, value);
                    
                    // Check for stat-based achievements
                    CheckStatAchievements(statName, value);
                    
                    onComplete?.Invoke(true, null);
                }
                else
                {
                    LogHelper.Error("AchievementsService", $"Failed to update stat {statName}: {error}");
                    onComplete?.Invoke(false, error);
                }
            });
        }
        
        /// <summary>
        /// Increment player stat
        /// </summary>
        /// <param name="statName">Name of the stat</param>
        /// <param name="amount">Amount to increment</param>
        /// <param name="onComplete">Callback when update is complete</param>
        public void IncrementStat(string statName, double amount, Action<bool, string> onComplete = null)
        {
            if (!_isInitialized)
            {
                string error = "Service is not initialized";
                LogHelper.Error("AchievementsService", error);
                onComplete?.Invoke(false, error);
                return;
            }
            
            // Use the first available provider
            var provider = _providers.FirstOrDefault(p => p.IsAvailable());
            if (provider == null)
            {
                string error = "No available providers";
                LogHelper.Error("AchievementsService", error);
                onComplete?.Invoke(false, error);
                return;
            }
            
            // Get current value
            double currentValue = GetStatValue(statName);
            double newValue = currentValue + amount;
            
            // Update stat
            provider.IncrementStat(statName, amount, (success, error) =>
            {
                if (success)
                {
                    LogHelper.Info("AchievementsService", $"Stat {statName} incremented by {amount} to {newValue}");
                    
                    // Create or update cached stat
                    if (!_statsCache.TryGetValue(statName, out PlayerStat stat))
                    {
                        // Try to create from definition
                        if (_statDefinitions.TryGetValue(statName, out StatDefinition definition))
                        {
                            stat = definition.CreatePlayerStat();
                        }
                        else
                        {
                            // Create new with default values
                            stat = new PlayerStat(statName, statName, 0, provider.GetProviderName());
                        }
                        
                        _statsCache[statName] = stat;
                    }
                    
                    // Update value
                    stat.IncrementValue(amount);
                    
                    // Trigger event
                    OnStatUpdated?.Invoke(stat, stat.Value);
                    
                    // Check for stat-based achievements
                    CheckStatAchievements(statName, stat.Value);
                    
                    onComplete?.Invoke(true, null);
                }
                else
                {
                    LogHelper.Error("AchievementsService", $"Failed to increment stat {statName}: {error}");
                    onComplete?.Invoke(false, error);
                }
            });
        }
        
        /// <summary>
        /// Get stat value
        /// </summary>
        /// <param name="statName">Name of the stat</param>
        /// <returns>Stat value or 0 if not found</returns>
        public double GetStatValue(string statName)
        {
            if (_statsCache.TryGetValue(statName, out PlayerStat stat))
            {
                return stat.Value;
            }
            
            // Not found, check if there's a definition with default value
            if (_statDefinitions.TryGetValue(statName, out StatDefinition definition))
            {
                return definition.DefaultValue;
            }
            
            return 0;
        }
        
        /// <summary>
        /// Synchronize achievements with the provider
        /// </summary>
        /// <param name="onComplete">Callback when sync is complete</param>
        public void SynchronizeAchievements(Action<bool, string> onComplete = null)
        {
            if (!_isInitialized)
            {
                string error = "Service is not initialized";
                LogHelper.Error("AchievementsService", error);
                onComplete?.Invoke(false, error);
                OnAchievementsSynchronized?.Invoke(false, error);
                return;
            }
            
            // Query achievements
            QueryAchievements(true, (achievements, error) =>
            {
                bool success = achievements != null && string.IsNullOrEmpty(error);
                
                if (success)
                {
                    LogHelper.Info("AchievementsService", $"Achievements synchronized successfully: {achievements.Count} achievements");
                }
                else
                {
                    LogHelper.Error("AchievementsService", $"Failed to synchronize achievements: {error}");
                }
                
                onComplete?.Invoke(success, error);
                OnAchievementsSynchronized?.Invoke(success, error);
            });
        }
        
        /// <summary>
        /// Synchronize stats with the provider
        /// </summary>
        /// <param name="onComplete">Callback when sync is complete</param>
        public void SynchronizeStats(Action<bool, string> onComplete = null)
        {
            if (!_isInitialized)
            {
                string error = "Service is not initialized";
                LogHelper.Error("AchievementsService", error);
                onComplete?.Invoke(false, error);
                OnStatsSynchronized?.Invoke(false, error);
                return;
            }
            
            // Query stats
            QueryStats(true, (stats, error) =>
            {
                bool success = stats != null && string.IsNullOrEmpty(error);
                
                if (success)
                {
                    LogHelper.Info("AchievementsService", $"Stats synchronized successfully: {stats.Count} stats");
                    
                    // Check all stat-based achievements
                    foreach (var stat in stats)
                    {
                        CheckStatAchievements(stat.Name, stat.Value);
                    }
                }
                else
                {
                    LogHelper.Error("AchievementsService", $"Failed to synchronize stats: {error}");
                }
                
                onComplete?.Invoke(success, error);
                OnStatsSynchronized?.Invoke(success, error);
            });
        }
        
        /// <summary>
        /// Reset all achievements (if supported by provider)
        /// </summary>
        /// <param name="onComplete">Callback when reset is complete</param>
        public void ResetAchievements(Action<bool, string> onComplete = null)
        {
            if (!_isInitialized)
            {
                string error = "Service is not initialized";
                LogHelper.Error("AchievementsService", error);
                onComplete?.Invoke(false, error);
                return;
            }
            
            // Use the first available provider
            var provider = _providers.FirstOrDefault(p => p.IsAvailable());
            if (provider == null)
            {
                string error = "No available providers";
                LogHelper.Error("AchievementsService", error);
                onComplete?.Invoke(false, error);
                return;
            }
            
            // Reset achievements
            provider.ResetAchievements((success, error) =>
            {
                if (success)
                {
                    LogHelper.Info("AchievementsService", "Achievements reset successfully");
                    
                    // Clear cache
                    _achievementsCache.Clear();
                    
                    // Refresh achievements
                    QueryAchievements(true, (achievements, queryError) => { });
                }
                else
                {
                    LogHelper.Error("AchievementsService", $"Failed to reset achievements: {error}");
                }
                
                onComplete?.Invoke(success, error);
            });
        }
        
        /// <summary>
        /// Reset all stats (if supported by provider)
        /// </summary>
        /// <param name="onComplete">Callback when reset is complete</param>
        public void ResetStats(Action<bool, string> onComplete = null)
        {
            if (!_isInitialized)
            {
                string error = "Service is not initialized";
                LogHelper.Error("AchievementsService", error);
                onComplete?.Invoke(false, error);
                return;
            }
            
            // Use the first available provider
            var provider = _providers.FirstOrDefault(p => p.IsAvailable());
            if (provider == null)
            {
                string error = "No available providers";
                LogHelper.Error("AchievementsService", error);
                onComplete?.Invoke(false, error);
                return;
            }
            
            // Reset stats
            provider.ResetStats((success, error) =>
            {
                if (success)
                {
                    LogHelper.Info("AchievementsService", "Stats reset successfully");
                    
                    // Clear cache
                    _statsCache.Clear();
                    
                    // Refresh stats
                    QueryStats(true, (stats, queryError) => { });
                }
                else
                {
                    LogHelper.Error("AchievementsService", $"Failed to reset stats: {error}");
                }
                
                onComplete?.Invoke(success, error);
            });
        }
        
        /// <summary>
        /// Check if the service is initialized
        /// </summary>
        /// <returns>True if initialized, false otherwise</returns>
        public bool IsInitialized()
        {
            return _isInitialized;
        }
        
        /// <summary>
        /// Load achievement definitions from providers
        /// </summary>
        private void LoadAchievementDefinitions()
        {
            if (!_isInitialized)
            {
                return;
            }
            
            // Use the first available provider
            var provider = _providers.FirstOrDefault(p => p.IsAvailable());
            if (provider == null)
            {
                LogHelper.Warning("AchievementsService", "No available providers for loading achievement definitions");
                return;
            }
            
            // Get definitions
            provider.GetAchievementDefinitions((definitions, error) =>
            {
                if (definitions != null)
                {
                    // Update cache
                    _achievementDefinitions.Clear();
                    foreach (var definition in definitions)
                    {
                        _achievementDefinitions[definition.Id] = definition;
                    }
                    
                    LogHelper.Info("AchievementsService", $"Loaded {definitions.Count} achievement definitions");
                }
                else
                {
                    LogHelper.Error("AchievementsService", $"Failed to get achievement definitions: {error}");
                }
            });
        }
        
        /// <summary>
        /// Load stat definitions from providers
        /// </summary>
        private void LoadStatDefinitions()
        {
            if (!_isInitialized)
            {
                return;
            }
            
            // Use the first available provider
            var provider = _providers.FirstOrDefault(p => p.IsAvailable());
            if (provider == null)
            {
                LogHelper.Warning("AchievementsService", "No available providers for loading stat definitions");
                return;
            }
            
            // Get definitions
            provider.GetStatDefinitions((definitions, error) =>
            {
                if (definitions != null)
                {
                    // Update cache
                    _statDefinitions.Clear();
                    foreach (var definition in definitions)
                    {
                        _statDefinitions[definition.Name] = definition;
                    }
                    
                    LogHelper.Info("AchievementsService", $"Loaded {definitions.Count} stat definitions");
                }
                else
                {
                    LogHelper.Error("AchievementsService", $"Failed to get stat definitions: {error}");
                }
            });
        }
        
        /// <summary>
        /// Check for stat-based achievements
        /// </summary>
        /// <param name="statName">Name of the stat</param>
        /// <param name="value">Stat value</param>
        private void CheckStatAchievements(string statName, double value)
        {
            if (!_isInitialized)
            {
                return;
            }
            
            // Find stat-based achievements for this stat
            var statAchievements = _achievementDefinitions.Values
                .Where(d => d.IsStatBased && d.StatName == statName && d.ThresholdValue <= value)
                .ToList();
            
            // Unlock matching achievements
            foreach (var definition in statAchievements)
            {
                // Check if already unlocked
                if (IsAchievementUnlocked(definition.Id))
                {
                    continue;
                }
                
                LogHelper.Info("AchievementsService", 
                    $"Stat {statName} reached {value}, unlocking achievement {definition.Id} (threshold: {definition.ThresholdValue})");
                
                // Unlock achievement
                UnlockAchievement(definition.Id, null);
            }
        }
    }
} 