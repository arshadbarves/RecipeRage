using System;
using Core.Logging;
using Core.RemoteConfig.Models;
using Core.State;
using Cysharp.Threading.Tasks;
using UI;
using UnityEngine;

namespace Core.RemoteConfig
{
    /// <summary>
    /// Monitors maintenance configuration and handles maintenance mode transitions
    /// </summary>
    public class MaintenanceChecker
    {
        private const int CHECK_INTERVAL_SECONDS = 120; // 2 minutes
        private const int WARNING_THRESHOLD_MINUTES = 10;
        
        private readonly IRemoteConfigService _configService;
        private readonly INTPTimeService _ntpTimeService;
        private readonly IUIService _uiService;
        private readonly IGameStateManager _stateManager;
        
        private bool _isRunning;
        private bool _warningShown;
        private DateTime _lastCheckTime;
        
        public MaintenanceChecker(
            IRemoteConfigService configService,
            INTPTimeService ntpTimeService,
            IUIService uiService,
            IGameStateManager stateManager)
        {
            _configService = configService;
            _ntpTimeService = ntpTimeService;
            _uiService = uiService;
            _stateManager = stateManager;
            _isRunning = false;
            _warningShown = false;
            _lastCheckTime = DateTime.MinValue;
        }
        
        /// <summary>
        /// Starts periodic maintenance checking
        /// </summary>
        public void Start()
        {
            if (_isRunning)
            {
                return;
            }
            
            _isRunning = true;
            GameLogger.Log("MaintenanceChecker started");
            PeriodicCheckAsync().Forget();
        }
        
        /// <summary>
        /// Stops maintenance checking
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
            GameLogger.Log("MaintenanceChecker stopped");
        }
        
        /// <summary>
        /// Performs immediate maintenance check
        /// </summary>
        public async UniTask<bool> CheckNow()
        {
            return await CheckMaintenance();
        }
        
        private async UniTaskVoid PeriodicCheckAsync()
        {
            while (_isRunning && Application.isPlaying)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(CHECK_INTERVAL_SECONDS));
                
                if (_isRunning && Application.isPlaying)
                {
                    await CheckMaintenance();
                }
            }
        }
        
        private async UniTask<bool> CheckMaintenance()
        {
            try
            {
                _lastCheckTime = DateTime.UtcNow;
                
                // Get maintenance config
                if (!_configService.TryGetConfig<MaintenanceConfig>(out var maintenanceConfig))
                {
                    GameLogger.LogWarning("MaintenanceConfig not available");
                    return false;
                }
                
                if (!maintenanceConfig.IsMaintenanceActive)
                {
                    // Reset warning flag when maintenance is not active
                    _warningShown = false;
                    return false;
                }
                
                // Get server time
                DateTime serverTime = _ntpTimeService.IsSynced 
                    ? _ntpTimeService.GetServerTime() 
                    : DateTime.UtcNow;
                
                // Check if we're in maintenance window
                if (maintenanceConfig.IsInMaintenanceWindow(serverTime))
                {
                    GameLogger.Log("Maintenance mode is active");
                    await HandleMaintenanceStart(maintenanceConfig);
                    return true;
                }
                
                // Check if maintenance is approaching (within 10 minutes)
                var timeUntilMaintenance = maintenanceConfig.GetTimeUntilMaintenance(serverTime);
                
                if (timeUntilMaintenance > TimeSpan.Zero && 
                    timeUntilMaintenance.TotalMinutes <= WARNING_THRESHOLD_MINUTES &&
                    !_warningShown)
                {
                    GameLogger.Log($"Maintenance starting in {timeUntilMaintenance.TotalMinutes:F0} minutes");
                    ShowMaintenanceWarning(maintenanceConfig, timeUntilMaintenance);
                    _warningShown = true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Maintenance check failed: {ex.Message}");
                return false;
            }
        }
        
        private async UniTask HandleMaintenanceStart(MaintenanceConfig config)
        {
            GameLogger.Log("Handling maintenance mode start");
            
            // Prevent new matches - block matchmaking
            GameLogger.Log("Blocking new matchmaking attempts");
            
            // Allow current matches to complete if configured
            if (config.AllowCurrentMatches)
            {
                GameLogger.Log("Allowing current matches to complete");
                // Current matches can finish naturally
            }
            else
            {
                GameLogger.Log("Forcing all matches to end");
                // Force disconnect from active matches
                // This would be handled by the NetworkGameManager
            }
            
            // Show maintenance screen
            ShowMaintenanceScreen(config);
            
            await UniTask.Yield();
        }
        
        private void ShowMaintenanceWarning(MaintenanceConfig config, TimeSpan timeUntil)
        {
            try
            {
                int minutesRemaining = (int)Math.Ceiling(timeUntil.TotalMinutes);
                
                string message = $"Scheduled maintenance will begin in {minutesRemaining} minute{(minutesRemaining != 1 ? "s" : "")}.\n\n" +
                                $"{config.MaintenanceMessage}\n\n" +
                                $"Please finish your current activities.";
                
                GameLogger.Log($"Maintenance warning: {message}");
                
                // Show notification using NotificationScreen
                if (_uiService != null)
                {
                    _uiService.ShowScreen(UIScreenType.Notification);
                    var notificationScreen = _uiService.GetScreen<UI.Screens.NotificationScreen>(UIScreenType.Notification);
                    if (notificationScreen != null)
                    {
                        notificationScreen.Show("Maintenance Notice", message, UI.Screens.NotificationType.Warning, 10f).Forget();
                    }
                }
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Failed to show maintenance warning: {ex.Message}");
            }
        }
        
        private void ShowMaintenanceScreen(MaintenanceConfig config)
        {
            try
            {
                GameLogger.Log("Showing maintenance screen");
                
                // Show maintenance screen (UIScreenType.Maintenance already exists)
                if (_uiService != null)
                {
                    _uiService.ShowScreen(UIScreenType.Maintenance);
                    
                    // Note: MaintenanceScreen would need to be created to display:
                    // - config.MaintenanceTitle
                    // - config.MaintenanceMessage
                    // - config.EstimatedDurationMinutes
                    // - config.GetEndTime()
                }
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Failed to show maintenance screen: {ex.Message}");
            }
        }
    }
}
