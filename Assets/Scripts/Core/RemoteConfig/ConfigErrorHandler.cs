using System;
using Core.Logging;
using UI;
using UnityEngine;

namespace Core.RemoteConfig
{
    /// <summary>
    /// Centralized error handling for configuration system
    /// </summary>
    public class ConfigErrorHandler
    {
        private readonly IUIService _uiService;
        
        public ConfigErrorHandler(IUIService uiService)
        {
            _uiService = uiService;
        }
        
        /// <summary>
        /// Handles configuration fetch errors
        /// </summary>
        public void HandleFetchError(string configKey, Exception exception, bool showUserMessage = true)
        {
            GameLogger.LogError($"Failed to fetch config '{configKey}': {exception.Message}");
            
            if (showUserMessage && IsNetworkError(exception))
            {
                ShowNoInternetPopup(() => 
                {
                    GameLogger.Log($"User requested retry for config: {configKey}");
                });
            }
        }
        
        /// <summary>
        /// Handles configuration validation errors
        /// </summary>
        public void HandleValidationError(string configKey, IConfigModel config)
        {
            GameLogger.LogError($"Config validation failed: {configKey}");
            
            if (config != null)
            {
                GameLogger.LogError($"Config details - Key: {config.ConfigKey}, Version: {config.Version}");
            }
            
            // Use fallback/default values
            GameLogger.Log($"Using fallback values for: {configKey}");
        }
        
        /// <summary>
        /// Shows "No Internet Connection" popup
        /// </summary>
        public void ShowNoInternetPopup(Action onRetry)
        {
            try
            {
                GameLogger.LogWarning("No Internet Connection - Showing retry popup");
                
                // Show NoInternetPopup
                if (_uiService != null)
                {
                    _uiService.ShowScreen(UIScreenType.NoInternet);
                    
                    var popup = _uiService.GetScreen<UI.Popups.NoInternetPopup>(UIScreenType.NoInternet);
                    if (popup != null)
                    {
                        popup.SetData(
                            "No Internet Connection",
                            "Unable to connect to the server. Please check your internet connection and try again.",
                            onRetry,
                            null
                        );
                    }
                }
                else
                {
                    GameLogger.LogError("UIService not available to show no internet popup");
                }
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Failed to show no internet popup: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Determines if an exception is network-related
        /// </summary>
        private bool IsNetworkError(Exception exception)
        {
            if (exception == null)
            {
                return false;
            }
            
            var exceptionType = exception.GetType().Name;
            var message = exception.Message?.ToLower() ?? "";
            
            return exceptionType.Contains("Network") ||
                   exceptionType.Contains("Socket") ||
                   exceptionType.Contains("Timeout") ||
                   message.Contains("network") ||
                   message.Contains("connection") ||
                   message.Contains("timeout") ||
                   message.Contains("unreachable");
        }
    }
}
