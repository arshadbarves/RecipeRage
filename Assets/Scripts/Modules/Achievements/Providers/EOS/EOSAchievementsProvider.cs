using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Epic.OnlineServices;
using Epic.OnlineServices.Achievements;
using Epic.OnlineServices.Stats;
using PlayEveryWare.EpicOnlineServices;
using RecipeRage.Modules.Achievements.Interfaces;
using RecipeRage.Modules.Logging;
using RecipeRage.Modules.Auth;

namespace RecipeRage.Modules.Achievements.Providers.EOS
{
    /// <summary>
    /// EOS implementation of the achievements provider.
    /// This provider uses the Epic Online Services SDK to manage achievements.
    /// 
    /// Complexity Rating: 4
    /// </summary>
    public class EOSAchievementsProvider : IAchievementsProvider
    {
        /// <summary>
        /// Provider name
        /// </summary>
        private const string PROVIDER_NAME = "EOSAchievements";

        /// <summary>
        /// Whether the provider is initialized
        /// </summary>
        private bool _isInitialized = false;

        /// <summary>
        /// Cache of achievement definitions
        /// </summary>
        private Dictionary<string, AchievementDefinition> _achievementDefinitions = new Dictionary<string, AchievementDefinition>();

        /// <summary>
        /// Cache of stat definitions
        /// </summary>
        private Dictionary<string, StatDefinition> _statDefinitions = new Dictionary<string, StatDefinition>();

        /// <summary>
        /// Last error message
        /// </summary>
        private string _lastError = string.Empty;

        /// <summary>
        /// Gets the EOS Achievements interface from the platform
        /// </summary>
        /// <returns>The achievements interface</returns>
        private AchievementsInterface GetEOSAchievementsInterface()
        {
            return EOSManager.Instance.GetEOSPlatformInterface().GetAchievementsInterface();
        }

        /// <summary>
        /// Gets the EOS Stats interface from the platform
        /// </summary>
        /// <returns>The stats interface</returns>
        private StatsInterface GetEOSStatsInterface()
        {
            return EOSManager.Instance.GetEOSPlatformInterface().GetStatsInterface();
        }

        /// <summary>
        /// Helper method to convert string[] to Utf8String[]
        /// </summary>
        /// <param name="strings">String array to convert</param>
        /// <returns>Utf8String array</returns>
        private Utf8String[] ConvertToUtf8StringArray(string[] strings)
        {
            if (strings == null)
                return null;

            Utf8String[] utf8Strings = new Utf8String[strings.Length];
            for (int i = 0; i < strings.Length; i++)
            {
                utf8Strings[i] = new Utf8String(strings[i]);
            }
            return utf8Strings;
        }

        /// <summary>
        /// Helper method to check if a DateTimeOffset has a specific value
        /// </summary>
        /// <param name="dateTime">DateTimeOffset to check</param>
        /// <param name="value">Value to compare against</param>
        /// <returns>True if the dateTime is greater than value</returns>
        private bool IsDateTimeGreaterThan(DateTimeOffset? dateTime, int value)
        {
            return dateTime.HasValue && dateTime.Value.ToUnixTimeSeconds() > value;
        }

        /// <summary>
        /// Helper method to convert DateTimeOffset to double (seconds)
        /// </summary>
        /// <param name="dateTime">DateTimeOffset to convert</param>
        /// <returns>Double value representing seconds</returns>
        private double DateTimeToSeconds(DateTimeOffset? dateTime)
        {
            return dateTime.HasValue ? dateTime.Value.ToUnixTimeSeconds() : 0;
        }

        /// <summary>
        /// Initialize the provider
        /// </summary>
        /// <param name="onComplete">Callback when initialization is complete</param>
        public void Initialize(Action<bool> onComplete = null)
        {
            if (_isInitialized)
            {
                LogHelper.Debug("EOSAchievementsProvider", "Already initialized");
                onComplete?.Invoke(true);
                return;
            }

            LogHelper.Info("EOSAchievementsProvider", "Initializing EOS Achievements provider");

            // Check if EOSManager is available
            if (EOSManager.Instance == null || GetEOSAchievementsInterface() == null)
            {
                _lastError = "EOS Manager or Achievements interface is not available";
                LogHelper.Error("EOSAchievementsProvider", _lastError);
                onComplete?.Invoke(false);
                return;
            }

            // Check if user is logged in to EOS
            if (!AuthHelper.IsSignedIn() || string.IsNullOrEmpty(AuthHelper.CurrentUser?.UserId))
            {
                _lastError = "User is not signed in to EOS";
                LogHelper.Error("EOSAchievementsProvider", _lastError);
                onComplete?.Invoke(false);
                return;
            }

            // Init is successful at this point
            _isInitialized = true;

            LogHelper.Info("EOSAchievementsProvider", "EOS Achievements provider initialized successfully");

            // Load achievement definitions
            QueryAchievementDefinitions((definitions, error) =>
            {
                if (!string.IsNullOrEmpty(error))
                {
                    LogHelper.Warning("EOSAchievementsProvider", $"Failed to load achievement definitions: {error}");
                }

                // Load stat definitions
                QueryStatDefinitions((statDefs, statError) =>
                {
                    if (!string.IsNullOrEmpty(statError))
                    {
                        LogHelper.Warning("EOSAchievementsProvider", $"Failed to load stat definitions: {statError}");
                    }

                    onComplete?.Invoke(_isInitialized);
                });
            });
        }

        /// <summary>
        /// Query player achievements from EOS
        /// </summary>
        /// <param name="onComplete">Callback with the achievements</param>
        public void QueryAchievements(Action<List<Achievement>, string> onComplete)
        {
            if (!CheckInitialized("QueryAchievements", (success, error) => onComplete?.Invoke(null, error)))
            {
                return;
            }

            LogHelper.Info("EOSAchievementsProvider", "Querying player achievements from EOS");

            // Get the product user ID
            ProductUserId productUserId = GetCurrentProductUserId();
            if (productUserId == null)
            {
                string error = "Could not get valid product user ID";
                LogHelper.Error("EOSAchievementsProvider", error);
                onComplete?.Invoke(null, error);
                return;
            }

            // Query player achievements
            var achievementsInterface = GetEOSAchievementsInterface();

            var options = new QueryPlayerAchievementsOptions
            {
                LocalUserId = productUserId
            };

            achievementsInterface.QueryPlayerAchievements(ref options, null, (ref OnQueryPlayerAchievementsCompleteCallbackInfo info) =>
            {
                if (info.ResultCode != Result.Success)
                {
                    string error = $"Failed to query player achievements: {info.ResultCode}";
                    LogHelper.Error("EOSAchievementsProvider", error);
                    onComplete?.Invoke(null, error);
                    return;
                }

                LogHelper.Debug("EOSAchievementsProvider", "QueryPlayerAchievements successful");

                // Get count of achievements
                var countOptions = new GetPlayerAchievementCountOptions
                {
                    UserId = productUserId
                };

                uint count = achievementsInterface.GetPlayerAchievementCount(ref countOptions);
                LogHelper.Debug("EOSAchievementsProvider", $"Player has {count} achievements");

                // Process achievements
                var achievements = new List<Achievement>();

                // Get each achievement
                for (uint i = 0; i < count; i++)
                {
                    var achievementOptions = new CopyPlayerAchievementByIndexOptions
                    {
                        LocalUserId = productUserId,
                        AchievementIndex = i
                    };

                    PlayerAchievement? playerAchievement = null;
                    Result result = achievementsInterface.CopyPlayerAchievementByIndex(ref achievementOptions, out playerAchievement);

                    if (result == Result.Success && playerAchievement.HasValue)
                    {
                        // Get definition from cache if available
                        if (_achievementDefinitions.TryGetValue(playerAchievement.Value.AchievementId, out AchievementDefinition definition))
                        {
                            var achievement = definition.CreateAchievement();

                            // Update from player achievement
                            achievement.Progress = (float)(playerAchievement.Value.Progress / 100.0); // EOS uses 0-100
                            achievement.IsUnlocked = playerAchievement.Value.Progress >= 100.0;

                            // Handle DateTimeOffset conversion
                            achievement.UnlockTime = IsDateTimeGreaterThan(playerAchievement.Value.UnlockTime, 0) ?
                                new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(playerAchievement.Value.UnlockTime.Value.ToUnixTimeSeconds()) :
                                null;

                            // Check if achievement is hidden by checking for ??? in name or description
                            achievement.IsHidden =
                                (playerAchievement.Value.DisplayName != null && playerAchievement.Value.DisplayName.ToString().Contains("???")) ||
                                (playerAchievement.Value.Description != null && playerAchievement.Value.Description.ToString().Contains("???"));

                            achievements.Add(achievement);
                        }
                    }
                    else
                    {
                        LogHelper.Warning("EOSAchievementsProvider", $"Failed to copy player achievement at index {i}, result: {result}");
                    }
                }

                onComplete?.Invoke(achievements, null);
            });
        }

        /// <summary>
        /// Query player stats from EOS
        /// </summary>
        /// <param name="onComplete">Callback with the stats</param>
        public void QueryStats(Action<List<PlayerStat>, string> onComplete)
        {
            if (!CheckInitialized("QueryStats", (success, error) => onComplete?.Invoke(null, error)))
            {
                return;
            }

            LogHelper.Info("EOSAchievementsProvider", "Querying player stats from EOS");

            // Get the product user ID
            ProductUserId productUserId = GetCurrentProductUserId();
            if (productUserId == null)
            {
                string error = "Could not get valid product user ID";
                LogHelper.Error("EOSAchievementsProvider", error);
                onComplete?.Invoke(null, error);
                return;
            }

            // Query player stats
            var statsInterface = GetEOSStatsInterface();

            // Get stat names from definitions
            var statNames = _statDefinitions.Keys.ToArray();

            if (statNames.Length == 0)
            {
                LogHelper.Warning("EOSAchievementsProvider", "No stat definitions found, querying with empty stats array");
            }

            // Fix QueryStats callback and array conversion
            var options = new QueryStatsOptions
            {
                LocalUserId = productUserId,
                TargetUserId = productUserId,
                StatNames = ConvertToUtf8StringArray(statNames)
            };

            statsInterface.QueryStats(ref options, null, (ref OnQueryStatsCompleteCallbackInfo info) =>
            {
                if (info.ResultCode != Result.Success)
                {
                    string error = $"Failed to query player stats: {info.ResultCode}";
                    LogHelper.Error("EOSAchievementsProvider", error);
                    onComplete?.Invoke(null, error);
                    return;
                }

                LogHelper.Info("EOSAchievementsProvider", "Player stats query successful");

                // Get stats
                var getStatsOptions = new GetStatCountOptions
                {
                    TargetUserId = productUserId
                };

                uint statCount = statsInterface.GetStatsCount(ref getStatsOptions);
                LogHelper.Info("EOSAchievementsProvider", $"Found {statCount} player stats");

                var stats = new List<PlayerStat>();

                // If no stats, return empty list
                if (statCount == 0)
                {
                    onComplete?.Invoke(stats, null);
                    return;
                }

                // Copy each stat
                for (uint i = 0; i < statCount; i++)
                {
                    var copyOptions = new CopyStatByIndexOptions
                    {
                        TargetUserId = productUserId,
                        StatIndex = i
                    };

                    Stat? stat = null;
                    Result result = statsInterface.CopyStatByIndex(ref copyOptions, out stat);

                    if (result == Result.Success)
                    {
                        // Create player stat
                        string statName = stat.Value.Name;

                        // Try to get display name from definition
                        string displayName = statName;
                        if (_statDefinitions.TryGetValue(statName, out StatDefinition definition))
                        {
                            displayName = definition.DisplayName;
                        }

                        var playerStat = new PlayerStat(
                            statName,
                            displayName,
                            stat.Value.Value,
                            PROVIDER_NAME
                        );

                        stats.Add(playerStat);
                    }
                    else
                    {
                        LogHelper.Warning("EOSAchievementsProvider", $"Failed to copy stat at index {i}: {result}");
                    }
                }

                onComplete?.Invoke(stats, null);
            });
        }

        /// <summary>
        /// Unlock achievement
        /// </summary>
        /// <param name="achievementId">ID of the achievement to unlock</param>
        /// <param name="onComplete">Callback when unlock is complete</param>
        public void UnlockAchievement(string achievementId, Action<bool, string> onComplete = null)
        {
            if (!CheckInitialized("UnlockAchievement", onComplete))
            {
                return;
            }

            LogHelper.Info("EOSAchievementsProvider", $"Unlocking achievement {achievementId}");

            // Get the product user ID
            ProductUserId productUserId = GetCurrentProductUserId();
            if (productUserId == null)
            {
                string error = "Could not get valid product user ID";
                LogHelper.Error("EOSAchievementsProvider", error);
                onComplete?.Invoke(false, error);
                return;
            }

            // Unlock achievement
            var achievementsInterface = GetEOSAchievementsInterface();

            var options = new UnlockAchievementsOptions
            {
                UserId = productUserId,
                AchievementIds = ConvertToUtf8StringArray(new[] { achievementId })
            };

            achievementsInterface.UnlockAchievements(ref options, null, (ref OnUnlockAchievementsCompleteCallbackInfo info) =>
            {
                if (info.ResultCode != Result.Success)
                {
                    string error = $"Failed to unlock achievement: {info.ResultCode}";
                    LogHelper.Error("EOSAchievementsProvider", error);
                    onComplete?.Invoke(false, error);
                    return;
                }

                LogHelper.Info("EOSAchievementsProvider", $"Achievement {achievementId} unlocked successfully");
                onComplete?.Invoke(true, null);
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
            if (!CheckInitialized("UpdateAchievementProgress", onComplete))
            {
                return;
            }

            LogHelper.Info("EOSAchievementsProvider", $"Updating achievement {achievementId} progress to {progress:P0}");

            // Convert to EOS progress (0-100)
            uint eosProgress = (uint)Mathf.RoundToInt(progress * 100);

            // If progress is 100%, use UnlockAchievement instead
            if (eosProgress >= 100)
            {
                UnlockAchievement(achievementId, onComplete);
                return;
            }

            // EOS doesn't have a direct API for updating achievement progress
            // It's tracked through stats, so this is a no-op
            // We'll return success and let the achievement progress be updated through the normal stat tracking

            LogHelper.Info("EOSAchievementsProvider", $"Achievement {achievementId} progress updated to {progress:P0} (no-op in EOS)");
            onComplete?.Invoke(true, null);
        }

        /// <summary>
        /// Get definition of all achievements
        /// </summary>
        /// <param name="onComplete">Callback with the achievements definitions</param>
        public void GetAchievementDefinitions(Action<List<AchievementDefinition>, string> onComplete)
        {
            if (!CheckInitialized("GetAchievementDefinitions", (success, error) => onComplete?.Invoke(null, error)))
            {
                return;
            }

            LogHelper.Info("EOSAchievementsProvider", "Getting achievement definitions from EOS");

            // Query achievement definitions
            QueryAchievementDefinitions(onComplete);
        }

        /// <summary>
        /// Update player stat
        /// </summary>
        /// <param name="statName">Name of the stat</param>
        /// <param name="value">New value</param>
        /// <param name="onComplete">Callback when update is complete</param>
        public void UpdateStat(string statName, double value, Action<bool, string> onComplete = null)
        {
            if (!CheckInitialized("UpdateStat", onComplete))
            {
                return;
            }

            LogHelper.Info("EOSAchievementsProvider", $"Updating stat {statName} to {value}");

            // Get the product user ID
            ProductUserId productUserId = GetCurrentProductUserId();
            if (productUserId == null)
            {
                string error = "Could not get valid product user ID";
                LogHelper.Error("EOSAchievementsProvider", error);
                onComplete?.Invoke(false, error);
                return;
            }

            // Create ingest data
            var ingestData = new IngestData
            {
                StatName = statName,
                IngestAmount = (int)value
            };

            // Ingest stat
            var statsInterface = GetEOSStatsInterface();

            var options = new IngestStatOptions
            {
                LocalUserId = productUserId,
                Stats = new[] { ingestData },
                TargetUserId = productUserId
            };

            statsInterface.IngestStat(ref options, null, (ref IngestStatCompleteCallbackInfo info) =>
            {
                if (info.ResultCode != Result.Success)
                {
                    string error = $"Failed to update stat: {info.ResultCode}";
                    LogHelper.Error("EOSAchievementsProvider", error);
                    onComplete?.Invoke(false, error);
                    return;
                }

                LogHelper.Info("EOSAchievementsProvider", $"Stat {statName} updated successfully to {value}");
                onComplete?.Invoke(true, null);
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
            if (!CheckInitialized("IncrementStat", onComplete))
            {
                return;
            }

            LogHelper.Info("EOSAchievementsProvider", $"Incrementing stat {statName} by {amount}");

            // Get the product user ID
            ProductUserId productUserId = GetCurrentProductUserId();
            if (productUserId == null)
            {
                string error = "Could not get valid product user ID";
                LogHelper.Error("EOSAchievementsProvider", error);
                onComplete?.Invoke(false, error);
                return;
            }

            // Create ingest data
            var ingestData = new IngestData
            {
                StatName = statName,
                IngestAmount = (int)amount
            };

            // Ingest stat
            var statsInterface = GetEOSStatsInterface();

            var options = new IngestStatOptions
            {
                LocalUserId = productUserId,
                Stats = new[] { ingestData },
                TargetUserId = productUserId
            };

            statsInterface.IngestStat(ref options, null, (ref IngestStatCompleteCallbackInfo info) =>
            {
                if (info.ResultCode != Result.Success)
                {
                    string error = $"Failed to increment stat: {info.ResultCode}";
                    LogHelper.Error("EOSAchievementsProvider", error);
                    onComplete?.Invoke(false, error);
                    return;
                }

                LogHelper.Info("EOSAchievementsProvider", $"Stat {statName} incremented successfully by {amount}");
                onComplete?.Invoke(true, null);
            });
        }

        /// <summary>
        /// Get stat definitions
        /// </summary>
        /// <param name="onComplete">Callback with the stat definitions</param>
        public void GetStatDefinitions(Action<List<StatDefinition>, string> onComplete)
        {
            if (!CheckInitialized("GetStatDefinitions", (success, error) => onComplete?.Invoke(null, error)))
            {
                return;
            }

            LogHelper.Info("EOSAchievementsProvider", "Getting stat definitions from EOS");

            // Query stat definitions
            QueryStatDefinitions(onComplete);
        }

        /// <summary>
        /// Display achievement UI (if supported)
        /// </summary>
        /// <param name="onComplete">Callback when UI is closed</param>
        public void DisplayAchievementUI(Action<bool> onComplete = null)
        {
            if (!CheckInitialized("DisplayAchievementUI", (success, error) => onComplete?.Invoke(false)))
            {
                return;
            }

            LogHelper.Warning("EOSAchievementsProvider", "DisplayAchievementUI is not directly supported by EOS SDK");
            onComplete?.Invoke(false);
        }

        /// <summary>
        /// Reset all achievements (if supported)
        /// </summary>
        /// <param name="onComplete">Callback when reset is complete</param>
        public void ResetAchievements(Action<bool, string> onComplete = null)
        {
            if (!CheckInitialized("ResetAchievements", onComplete))
            {
                return;
            }

            LogHelper.Warning("EOSAchievementsProvider", "ResetAchievements is not supported by EOS SDK");
            string error = "Reset achievements is not supported by EOS SDK";
            onComplete?.Invoke(false, error);
        }

        /// <summary>
        /// Reset all stats (if supported)
        /// </summary>
        /// <param name="onComplete">Callback when reset is complete</param>
        public void ResetStats(Action<bool, string> onComplete = null)
        {
            if (!CheckInitialized("ResetStats", onComplete))
            {
                return;
            }

            LogHelper.Warning("EOSAchievementsProvider", "ResetStats is not supported by EOS SDK");
            string error = "Reset stats is not supported by EOS SDK";
            onComplete?.Invoke(false, error);
        }

        /// <summary>
        /// Gets whether the provider is available
        /// </summary>
        /// <returns>True if the provider is available</returns>
        public bool IsAvailable()
        {
            // Check if everything is set up correctly
            bool eosManagerAvailable = EOSManager.Instance != null &&
                                      EOSManager.Instance.GetEOSPlatformInterface() != null;
            bool userLoggedIn = AuthHelper.IsSignedIn() && !string.IsNullOrEmpty(AuthHelper.CurrentUser?.UserId);

            return eosManagerAvailable && userLoggedIn;
        }

        /// <summary>
        /// Gets the provider name
        /// </summary>
        /// <returns>Provider name</returns>
        public string GetProviderName()
        {
            return PROVIDER_NAME;
        }

        /// <summary>
        /// Checks if the provider is initialized
        /// </summary>
        /// <param name="methodName">Name of the method being called</param>
        /// <param name="onComplete">Optional callback for error handling</param>
        /// <returns>True if initialized</returns>
        private bool CheckInitialized(string methodName, Action<bool, string> onComplete = null)
        {
            if (!_isInitialized)
            {
                _lastError = "Provider is not initialized";
                LogHelper.Error("EOSAchievementsProvider", $"{methodName}: {_lastError}");
                onComplete?.Invoke(false, _lastError);
                return false;
            }

            if (EOSManager.Instance == null || GetEOSAchievementsInterface() == null)
            {
                string error = "EOS Manager or Achievements interface is not available";
                LogHelper.Error("EOSAchievementsProvider", $"{methodName}: {error}");
                onComplete?.Invoke(false, error);
                return false;
            }

            if (!AuthHelper.IsSignedIn() || string.IsNullOrEmpty(AuthHelper.CurrentUser?.UserId))
            {
                string error = "User is not signed in to EOS";
                LogHelper.Error("EOSAchievementsProvider", $"{methodName}: {error}");
                onComplete?.Invoke(false, error);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Query achievement definitions from EOS
        /// </summary>
        /// <param name="onComplete">Callback with the definitions</param>
        private void QueryAchievementDefinitions(Action<List<AchievementDefinition>, string> onComplete)
        {
            LogHelper.Info("EOSAchievementsProvider", "Querying achievement definitions from EOS");

            // Query achievements definitions
            var achievementsInterface = GetEOSAchievementsInterface();

            var options = new QueryDefinitionsOptions();
            options.LocalUserId = GetCurrentProductUserId();

            achievementsInterface.QueryDefinitions(ref options, null, (ref OnQueryDefinitionsCompleteCallbackInfo info) =>
            {
                if (info.ResultCode != Result.Success)
                {
                    string error = $"Failed to query achievement definitions: {info.ResultCode}";
                    LogHelper.Error("EOSAchievementsProvider", error);
                    onComplete?.Invoke(null, error);
                    return;
                }

                LogHelper.Info("EOSAchievementsProvider", "Achievement definitions query successful");

                // Get achievement definitions count
                var getCountOptions = new GetAchievementDefinitionCountOptions();
                uint definitionCount = achievementsInterface.GetAchievementDefinitionCount(ref getCountOptions);
                LogHelper.Info("EOSAchievementsProvider", $"Found {definitionCount} achievement definitions");

                var definitions = new List<AchievementDefinition>();

                // If no definitions, return empty list
                if (definitionCount == 0)
                {
                    _achievementDefinitions.Clear();
                    onComplete?.Invoke(definitions, null);
                    return;
                }

                // Copy each definition
                for (uint i = 0; i < definitionCount; i++)
                {
                    var copyOptions = new CopyAchievementDefinitionByIndexOptions
                    {
                        AchievementIndex = i
                    };

                    Definition? eosDefinition = null;
                    Result result = achievementsInterface.CopyAchievementDefinitionByIndex(ref copyOptions, out eosDefinition);

                    if (result == Result.Success)
                    {
                        // Convert to our format
                        var definition = new AchievementDefinition(
                            eosDefinition.Value.AchievementId,
                            eosDefinition.Value.DisplayName,
                            eosDefinition.Value.Description,
                            PROVIDER_NAME
                        );

                        definition.LockedDescription = eosDefinition.Value.LockedDescription ?? "This achievement is locked";
                        definition.IsHidden = eosDefinition.Value.IsHidden;

                        // Check if it's stat-based by looking at the first threshold
                        if (eosDefinition.Value.StatThresholds != null && eosDefinition.Value.StatThresholds.Length > 0)
                        {
                            definition.IsStatBased = true;
                            definition.StatName = eosDefinition.Value.StatThresholds[0].Name;
                            definition.ThresholdValue = eosDefinition.Value.StatThresholds[0].Threshold;
                        }

                        // Store in dictionary and list
                        _achievementDefinitions[definition.Id] = definition;
                        definitions.Add(definition);
                    }
                    else
                    {
                        LogHelper.Warning("EOSAchievementsProvider", $"Failed to copy achievement definition at index {i}: {result}");
                    }
                }

                onComplete?.Invoke(definitions, null);
            });
        }

        /// <summary>
        /// Query stat definitions from EOS
        /// </summary>
        /// <param name="onComplete">Callback with the definitions</param>
        private void QueryStatDefinitions(Action<List<StatDefinition>, string> onComplete)
        {
            LogHelper.Info("EOSAchievementsProvider", "Creating stat definitions from EOS achievements");

            // EOS doesn't have a direct API for getting stat definitions
            // We'll derive them from achievement definitions, and add some common ones

            // Extract stat thresholds from achievement definitions
            var statDefs = new Dictionary<string, StatDefinition>();

            foreach (var achievementDef in _achievementDefinitions.Values)
            {
                if (achievementDef.IsStatBased)
                {
                    if (!statDefs.TryGetValue(achievementDef.StatName, out StatDefinition statDef))
                    {
                        // Create new stat definition
                        statDef = new StatDefinition(
                            achievementDef.StatName,
                            GetDisplayNameFromStatName(achievementDef.StatName),
                            PROVIDER_NAME
                        );

                        // Set aggregation based on stat name
                        statDef.Aggregation = GetAggregationFromStatName(achievementDef.StatName);

                        statDefs[achievementDef.StatName] = statDef;
                    }
                }
            }

            // Add common stats if not already defined
            AddCommonStatDefinition(statDefs, "Kills", "Kills", StatDefinition.AggregationMethod.Sum);
            AddCommonStatDefinition(statDefs, "Deaths", "Deaths", StatDefinition.AggregationMethod.Sum);
            AddCommonStatDefinition(statDefs, "Wins", "Wins", StatDefinition.AggregationMethod.Sum);
            AddCommonStatDefinition(statDefs, "Losses", "Losses", StatDefinition.AggregationMethod.Sum);
            AddCommonStatDefinition(statDefs, "Score", "Score", StatDefinition.AggregationMethod.Sum);
            AddCommonStatDefinition(statDefs, "HighScore", "High Score", StatDefinition.AggregationMethod.Max);
            AddCommonStatDefinition(statDefs, "TimePlayed", "Time Played", StatDefinition.AggregationMethod.Sum);

            // Save to cache
            _statDefinitions = statDefs;

            LogHelper.Info("EOSAchievementsProvider", $"Created {statDefs.Count} stat definitions");
            onComplete?.Invoke(statDefs.Values.ToList(), null);
        }

        /// <summary>
        /// Add a common stat definition if not already defined
        /// </summary>
        /// <param name="statDefs">Dictionary of stat definitions</param>
        /// <param name="name">Stat name</param>
        /// <param name="displayName">Display name</param>
        /// <param name="aggregation">Aggregation method</param>
        private void AddCommonStatDefinition(
            Dictionary<string, StatDefinition> statDefs,
            string name,
            string displayName,
            StatDefinition.AggregationMethod aggregation)
        {
            if (!statDefs.ContainsKey(name))
            {
                var statDef = new StatDefinition(name, displayName, PROVIDER_NAME)
                {
                    Aggregation = aggregation
                };

                statDefs[name] = statDef;
            }
        }

        /// <summary>
        /// Get a display name from a stat name
        /// </summary>
        /// <param name="statName">Stat name</param>
        /// <returns>Display name</returns>
        private string GetDisplayNameFromStatName(string statName)
        {
            // Add spaces before uppercase letters
            string displayName = System.Text.RegularExpressions.Regex.Replace(statName, "([a-z])([A-Z])", "$1 $2");

            // Capitalize first letter
            if (!string.IsNullOrEmpty(displayName))
            {
                displayName = char.ToUpper(displayName[0]) + displayName.Substring(1);
            }

            return displayName;
        }

        /// <summary>
        /// Get aggregation method based on stat name
        /// </summary>
        /// <param name="statName">Stat name</param>
        /// <returns>Aggregation method</returns>
        private StatDefinition.AggregationMethod GetAggregationFromStatName(string statName)
        {
            // Check for common aggregation patterns
            if (statName.StartsWith("Total") || statName.EndsWith("Count") ||
                statName.Contains("Kills") || statName.Contains("Deaths") ||
                statName.Contains("Wins") || statName.Contains("Losses"))
            {
                return StatDefinition.AggregationMethod.Sum;
            }

            if (statName.StartsWith("Max") || statName.StartsWith("High") ||
                statName.Contains("Best") || statName.Contains("Longest"))
            {
                return StatDefinition.AggregationMethod.Max;
            }

            if (statName.StartsWith("Min") || statName.Contains("Shortest") ||
                statName.Contains("Fastest"))
            {
                return StatDefinition.AggregationMethod.Min;
            }

            if (statName.StartsWith("Avg") || statName.Contains("Average"))
            {
                return StatDefinition.AggregationMethod.Average;
            }

            // Default to sum for most stats
            return StatDefinition.AggregationMethod.Sum;
        }

        /// <summary>
        /// Get the current EOS product user ID
        /// </summary>
        /// <returns>Product user ID or null if not available</returns>
        private ProductUserId GetCurrentProductUserId()
        {
            // Get the product user ID from the auth service
            string userId = AuthHelper.CurrentUser?.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                LogHelper.Error("EOSAchievementsProvider", "No user ID available");
                return null;
            }

            // Try to convert to product user ID
            ProductUserId productUserId = ProductUserId.FromString(userId);
            if (productUserId == null || !productUserId.IsValid())
            {
                LogHelper.Error("EOSAchievementsProvider", $"Invalid product user ID: {userId}");
                return null;
            }

            return productUserId;
        }
    }
}