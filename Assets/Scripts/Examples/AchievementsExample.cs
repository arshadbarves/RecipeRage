using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using RecipeRage.Modules.Achievements;
using RecipeRage.Modules.Achievements.Interfaces;
using RecipeRage.Modules.Logging;

namespace RecipeRage.Examples
{
    /// <summary>
    /// Example script demonstrating how to use the Achievements module
    /// Shows unlocking achievements, tracking stats, and managing achievement progress
    /// 
    /// Complexity Rating: 2
    /// </summary>
    public class AchievementsExample : MonoBehaviour
    {
        [Header("Achievement Settings")]
        [SerializeField] private string _achievementId = "achievement_example";
        [SerializeField] private float _progressStep = 0.25f;
        
        [Header("Stat Settings")]
        [SerializeField] private string _statName = "ExampleStat";
        [SerializeField] private double _statIncrement = 1.0;
        
        [Header("UI Elements")]
        [SerializeField] private InputField _achievementIdInput;
        [SerializeField] private InputField _statNameInput;
        [SerializeField] private InputField _statValueInput;
        [SerializeField] private Button _unlockAchievementButton;
        [SerializeField] private Button _incrementProgressButton;
        [SerializeField] private Button _updateStatButton;
        [SerializeField] private Button _incrementStatButton;
        [SerializeField] private Button _queryAchievementsButton;
        [SerializeField] private Button _queryStatsButton;
        [SerializeField] private Button _resetButton;
        [SerializeField] private Text _statusText;
        [SerializeField] private Text _achievementsText;
        [SerializeField] private Text _statsText;
        [SerializeField] private Toggle _autoSyncToggle;
        
        // Track initialization state
        private bool _isInitialized = false;
        
        /// <summary>
        /// Initialize on enable
        /// </summary>
        private void OnEnable()
        {
            InitializeAchievements();
            SetupUI();
            RegisterEventHandlers();
        }
        
        /// <summary>
        /// Clean up on disable
        /// </summary>
        private void OnDisable()
        {
            UnregisterEventHandlers();
        }
        
        /// <summary>
        /// Initialize achievements
        /// </summary>
        private void InitializeAchievements()
        {
            UpdateStatusText("Initializing achievements...");
            
            AchievementsHelper.Initialize(success =>
            {
                _isInitialized = success;
                
                if (success)
                {
                    UpdateStatusText("Achievements initialized successfully");
                    
                    // Query initial achievements and stats
                    QueryAchievements();
                    QueryStats();
                }
                else
                {
                    UpdateStatusText("Failed to initialize achievements");
                }
                
                // Update UI button states
                UpdateUIState();
            });
        }
        
        /// <summary>
        /// Set up UI elements
        /// </summary>
        private void SetupUI()
        {
            // Set default values
            if (_achievementIdInput != null)
            {
                _achievementIdInput.text = _achievementId;
            }
            
            if (_statNameInput != null)
            {
                _statNameInput.text = _statName;
            }
            
            if (_statValueInput != null)
            {
                _statValueInput.text = _statIncrement.ToString();
            }
            
            // Set up button callbacks
            if (_unlockAchievementButton != null)
            {
                _unlockAchievementButton.onClick.AddListener(OnUnlockAchievementClicked);
            }
            
            if (_incrementProgressButton != null)
            {
                _incrementProgressButton.onClick.AddListener(OnIncrementProgressClicked);
            }
            
            if (_updateStatButton != null)
            {
                _updateStatButton.onClick.AddListener(OnUpdateStatClicked);
            }
            
            if (_incrementStatButton != null)
            {
                _incrementStatButton.onClick.AddListener(OnIncrementStatClicked);
            }
            
            if (_queryAchievementsButton != null)
            {
                _queryAchievementsButton.onClick.AddListener(OnQueryAchievementsClicked);
            }
            
            if (_queryStatsButton != null)
            {
                _queryStatsButton.onClick.AddListener(OnQueryStatsClicked);
            }
            
            if (_resetButton != null)
            {
                _resetButton.onClick.AddListener(OnResetClicked);
            }
            
            // Update UI state
            UpdateUIState();
        }
        
        /// <summary>
        /// Register event handlers
        /// </summary>
        private void RegisterEventHandlers()
        {
            AchievementsHelper.RegisterAchievementUnlockedCallback(OnAchievementUnlocked);
            AchievementsHelper.RegisterAchievementProgressUpdatedCallback(OnAchievementProgressUpdated);
            AchievementsHelper.RegisterStatUpdatedCallback(OnStatUpdated);
        }
        
        /// <summary>
        /// Unregister event handlers
        /// </summary>
        private void UnregisterEventHandlers()
        {
            AchievementsHelper.UnregisterAchievementUnlockedCallback(OnAchievementUnlocked);
            AchievementsHelper.UnregisterAchievementProgressUpdatedCallback(OnAchievementProgressUpdated);
            AchievementsHelper.UnregisterStatUpdatedCallback(OnStatUpdated);
        }
        
        /// <summary>
        /// Handle unlock achievement button click
        /// </summary>
        private void OnUnlockAchievementClicked()
        {
            if (!CheckInitialized())
            {
                return;
            }
            
            string achievementId = GetAchievementId();
            if (string.IsNullOrEmpty(achievementId))
            {
                UpdateStatusText("Achievement ID cannot be empty");
                return;
            }
            
            UpdateStatusText($"Unlocking achievement {achievementId}...");
            
            AchievementsHelper.UnlockAchievement(achievementId, (success, error) =>
            {
                if (success)
                {
                    UpdateStatusText($"Achievement {achievementId} unlocked successfully");
                }
                else
                {
                    UpdateStatusText($"Failed to unlock achievement: {error}");
                }
                
                // Sync if auto-sync is enabled
                if (_autoSyncToggle != null && _autoSyncToggle.isOn)
                {
                    SyncAchievements();
                }
            });
        }
        
        /// <summary>
        /// Handle increment progress button click
        /// </summary>
        private void OnIncrementProgressClicked()
        {
            if (!CheckInitialized())
            {
                return;
            }
            
            string achievementId = GetAchievementId();
            if (string.IsNullOrEmpty(achievementId))
            {
                UpdateStatusText("Achievement ID cannot be empty");
                return;
            }
            
            // Get current progress
            float currentProgress = AchievementsHelper.GetAchievementProgress(achievementId);
            float newProgress = Mathf.Min(currentProgress + _progressStep, 1.0f);
            
            UpdateStatusText($"Updating achievement {achievementId} progress to {newProgress:P0}...");
            
            AchievementsHelper.UpdateAchievementProgress(achievementId, newProgress, (success, error) =>
            {
                if (success)
                {
                    UpdateStatusText($"Achievement {achievementId} progress updated to {newProgress:P0}");
                }
                else
                {
                    UpdateStatusText($"Failed to update achievement progress: {error}");
                }
                
                // Sync if auto-sync is enabled
                if (_autoSyncToggle != null && _autoSyncToggle.isOn)
                {
                    SyncAchievements();
                }
            });
        }
        
        /// <summary>
        /// Handle update stat button click
        /// </summary>
        private void OnUpdateStatClicked()
        {
            if (!CheckInitialized())
            {
                return;
            }
            
            string statName = GetStatName();
            if (string.IsNullOrEmpty(statName))
            {
                UpdateStatusText("Stat name cannot be empty");
                return;
            }
            
            // Try to parse value
            if (!double.TryParse(_statValueInput.text, out double value))
            {
                UpdateStatusText("Invalid stat value");
                return;
            }
            
            UpdateStatusText($"Updating stat {statName} to {value}...");
            
            AchievementsHelper.UpdateStat(statName, value, (success, error) =>
            {
                if (success)
                {
                    UpdateStatusText($"Stat {statName} updated to {value}");
                }
                else
                {
                    UpdateStatusText($"Failed to update stat: {error}");
                }
                
                // Sync if auto-sync is enabled
                if (_autoSyncToggle != null && _autoSyncToggle.isOn)
                {
                    SyncStats();
                }
            });
        }
        
        /// <summary>
        /// Handle increment stat button click
        /// </summary>
        private void OnIncrementStatClicked()
        {
            if (!CheckInitialized())
            {
                return;
            }
            
            string statName = GetStatName();
            if (string.IsNullOrEmpty(statName))
            {
                UpdateStatusText("Stat name cannot be empty");
                return;
            }
            
            // Try to parse increment value
            if (!double.TryParse(_statValueInput.text, out double increment))
            {
                increment = _statIncrement;
            }
            
            UpdateStatusText($"Incrementing stat {statName} by {increment}...");
            
            AchievementsHelper.IncrementStat(statName, increment, (success, error) =>
            {
                if (success)
                {
                    double newValue = AchievementsHelper.GetStatValue(statName);
                    UpdateStatusText($"Stat {statName} incremented by {increment} to {newValue}");
                }
                else
                {
                    UpdateStatusText($"Failed to increment stat: {error}");
                }
                
                // Sync if auto-sync is enabled
                if (_autoSyncToggle != null && _autoSyncToggle.isOn)
                {
                    SyncStats();
                }
            });
        }
        
        /// <summary>
        /// Handle query achievements button click
        /// </summary>
        private void OnQueryAchievementsClicked()
        {
            QueryAchievements();
        }
        
        /// <summary>
        /// Handle query stats button click
        /// </summary>
        private void OnQueryStatsClicked()
        {
            QueryStats();
        }
        
        /// <summary>
        /// Handle reset button click
        /// </summary>
        private void OnResetClicked()
        {
            if (!CheckInitialized())
            {
                return;
            }
            
            UpdateStatusText("Resetting achievements and stats...");
            
            AchievementsHelper.ResetAchievements((success, error) =>
            {
                if (success)
                {
                    UpdateStatusText("Achievements reset successfully");
                }
                else
                {
                    UpdateStatusText($"Failed to reset achievements: {error}");
                }
                
                // Also reset stats
                AchievementsHelper.ResetStats((statSuccess, statError) =>
                {
                    if (statSuccess)
                    {
                        UpdateStatusText("Stats reset successfully");
                    }
                    else
                    {
                        UpdateStatusText($"Failed to reset stats: {statError}");
                    }
                    
                    // Query updated achievements and stats
                    QueryAchievements();
                    QueryStats();
                });
            });
        }
        
        /// <summary>
        /// Query achievements
        /// </summary>
        private void QueryAchievements()
        {
            if (!CheckInitialized())
            {
                return;
            }
            
            UpdateStatusText("Querying achievements...");
            
            AchievementsHelper.QueryAchievements(true, (achievements, error) =>
            {
                if (achievements != null)
                {
                    UpdateStatusText($"Retrieved {achievements.Count} achievements");
                    UpdateAchievementsText(achievements);
                }
                else
                {
                    UpdateStatusText($"Failed to query achievements: {error}");
                }
            });
        }
        
        /// <summary>
        /// Query stats
        /// </summary>
        private void QueryStats()
        {
            if (!CheckInitialized())
            {
                return;
            }
            
            UpdateStatusText("Querying stats...");
            
            AchievementsHelper.QueryStats(true, (stats, error) =>
            {
                if (stats != null)
                {
                    UpdateStatusText($"Retrieved {stats.Count} stats");
                    UpdateStatsText(stats);
                }
                else
                {
                    UpdateStatusText($"Failed to query stats: {error}");
                }
            });
        }
        
        /// <summary>
        /// Synchronize achievements
        /// </summary>
        private void SyncAchievements()
        {
            if (!CheckInitialized())
            {
                return;
            }
            
            UpdateStatusText("Synchronizing achievements...");
            
            AchievementsHelper.SynchronizeAchievements((success, error) =>
            {
                if (success)
                {
                    UpdateStatusText("Achievements synchronized successfully");
                    QueryAchievements();
                }
                else
                {
                    UpdateStatusText($"Failed to synchronize achievements: {error}");
                }
            });
        }
        
        /// <summary>
        /// Synchronize stats
        /// </summary>
        private void SyncStats()
        {
            if (!CheckInitialized())
            {
                return;
            }
            
            UpdateStatusText("Synchronizing stats...");
            
            AchievementsHelper.SynchronizeStats((success, error) =>
            {
                if (success)
                {
                    UpdateStatusText("Stats synchronized successfully");
                    QueryStats();
                }
                else
                {
                    UpdateStatusText($"Failed to synchronize stats: {error}");
                }
            });
        }
        
        /// <summary>
        /// Update the UI state based on initialization
        /// </summary>
        private void UpdateUIState()
        {
            bool enabled = _isInitialized;
            
            if (_unlockAchievementButton != null) _unlockAchievementButton.interactable = enabled;
            if (_incrementProgressButton != null) _incrementProgressButton.interactable = enabled;
            if (_updateStatButton != null) _updateStatButton.interactable = enabled;
            if (_incrementStatButton != null) _incrementStatButton.interactable = enabled;
            if (_queryAchievementsButton != null) _queryAchievementsButton.interactable = enabled;
            if (_queryStatsButton != null) _queryStatsButton.interactable = enabled;
            if (_resetButton != null) _resetButton.interactable = enabled;
            if (_achievementIdInput != null) _achievementIdInput.interactable = enabled;
            if (_statNameInput != null) _statNameInput.interactable = enabled;
            if (_statValueInput != null) _statValueInput.interactable = enabled;
            if (_autoSyncToggle != null) _autoSyncToggle.interactable = enabled;
        }
        
        /// <summary>
        /// Get achievement ID from input
        /// </summary>
        /// <returns>Achievement ID</returns>
        private string GetAchievementId()
        {
            return _achievementIdInput != null ? _achievementIdInput.text : _achievementId;
        }
        
        /// <summary>
        /// Get stat name from input
        /// </summary>
        /// <returns>Stat name</returns>
        private string GetStatName()
        {
            return _statNameInput != null ? _statNameInput.text : _statName;
        }
        
        /// <summary>
        /// Update status text
        /// </summary>
        /// <param name="status">Status message</param>
        private void UpdateStatusText(string status)
        {
            if (_statusText != null)
            {
                _statusText.text = status;
                LogHelper.Info("AchievementsExample", status);
            }
        }
        
        /// <summary>
        /// Update achievements text
        /// </summary>
        /// <param name="achievements">List of achievements</param>
        private void UpdateAchievementsText(List<Achievement> achievements)
        {
            if (_achievementsText == null)
            {
                return;
            }
            
            if (achievements == null || achievements.Count == 0)
            {
                _achievementsText.text = "No achievements found";
                return;
            }
            
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Achievements ({achievements.Count}):");
            
            foreach (var achievement in achievements)
            {
                sb.AppendLine($"- {achievement.Id}: {achievement.Title}");
                sb.AppendLine($"  Progress: {achievement.Progress:P0} | Unlocked: {achievement.IsUnlocked}");
                if (achievement.UnlockTime.HasValue)
                {
                    sb.AppendLine($"  Unlocked on: {achievement.UnlockTime.Value}");
                }
                sb.AppendLine();
            }
            
            _achievementsText.text = sb.ToString();
        }
        
        /// <summary>
        /// Update stats text
        /// </summary>
        /// <param name="stats">List of stats</param>
        private void UpdateStatsText(List<PlayerStat> stats)
        {
            if (_statsText == null)
            {
                return;
            }
            
            if (stats == null || stats.Count == 0)
            {
                _statsText.text = "No stats found";
                return;
            }
            
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Stats ({stats.Count}):");
            
            foreach (var stat in stats)
            {
                sb.AppendLine($"- {stat.Name}: {stat.GetFormattedValue()}");
                sb.AppendLine($"  Last Updated: {stat.LastUpdated}");
                sb.AppendLine();
            }
            
            _statsText.text = sb.ToString();
        }
        
        /// <summary>
        /// Check if achievements are initialized
        /// </summary>
        /// <returns>True if initialized, false otherwise</returns>
        private bool CheckInitialized()
        {
            if (!_isInitialized)
            {
                UpdateStatusText("Achievements service is not initialized");
                return false;
            }
            
            return true;
        }
        
        #region Event Handlers
        
        /// <summary>
        /// Handle achievement unlocked event
        /// </summary>
        /// <param name="achievement">The unlocked achievement</param>
        private void OnAchievementUnlocked(Achievement achievement)
        {
            UpdateStatusText($"Achievement unlocked: {achievement.Title}");
            
            // Update achievements list
            QueryAchievements();
        }
        
        /// <summary>
        /// Handle achievement progress updated event
        /// </summary>
        /// <param name="achievement">The achievement</param>
        /// <param name="progress">New progress value</param>
        private void OnAchievementProgressUpdated(Achievement achievement, float progress)
        {
            UpdateStatusText($"Achievement progress updated: {achievement.Title} - {progress:P0}");
            
            // Update achievements list
            QueryAchievements();
        }
        
        /// <summary>
        /// Handle stat updated event
        /// </summary>
        /// <param name="stat">The stat</param>
        /// <param name="value">New value</param>
        private void OnStatUpdated(PlayerStat stat, double value)
        {
            UpdateStatusText($"Stat updated: {stat.Name} - {stat.GetFormattedValue()}");
            
            // Update stats list
            QueryStats();
        }
        
        #endregion
    }
} 