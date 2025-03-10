using System;
using System.Collections;
using UnityEngine;
using RecipeRage.Modules.Logging;

namespace RecipeRage.Examples
{
    /// <summary>
    /// Example script demonstrating how to use the Logging module.
    /// Complexity: 2 - Basic example with comments
    /// </summary>
    public class LoggingExample : MonoBehaviour
    {
        [SerializeField] private bool _enableConsoleOutput = true;
        [SerializeField] private bool _enableFileOutput = true;
        [SerializeField] private LogLevel _logLevel = LogLevel.Debug;
        [SerializeField] private float _logInterval = 2.0f;
        
        private bool _isRunning = false;
        private int _counter = 0;
        
        /// <summary>
        /// Initialize the logging system when the component is enabled.
        /// </summary>
        private void OnEnable()
        {
            // Initialize the logging system with the configured settings
            LogHelper.SetConsoleOutput(_enableConsoleOutput);
            LogHelper.SetFileOutput(_enableFileOutput);
            LogHelper.SetLogLevel(_logLevel);
            
            // Subscribe to log events
            LogHelper.OnLogWritten += HandleLogWritten;
            
            // Log initialization
            LogHelper.Info("LoggingExample", "Logging system initialized");
            
            // Start the logging coroutine
            _isRunning = true;
            StartCoroutine(GenerateLogMessages());
        }
        
        /// <summary>
        /// Clean up when the component is disabled.
        /// </summary>
        private void OnDisable()
        {
            // Stop the logging coroutine
            _isRunning = false;
            
            // Unsubscribe from log events
            LogHelper.OnLogWritten -= HandleLogWritten;
            
            // Log shutdown
            LogHelper.Info("LoggingExample", "Logging system shutdown");
        }
        
        /// <summary>
        /// Coroutine that generates log messages at regular intervals.
        /// </summary>
        private IEnumerator GenerateLogMessages()
        {
            while (_isRunning)
            {
                // Generate a log message at each level
                _counter++;
                
                LogHelper.Debug("LoggingExample", $"Debug message #{_counter} - This is detailed information for debugging");
                LogHelper.Info("LoggingExample", $"Info message #{_counter} - This is general information about system operation");
                LogHelper.Warning("LoggingExample", $"Warning message #{_counter} - This is a potentially harmful situation");
                
                // Every 5th message, generate an error
                if (_counter % 5 == 0)
                {
                    LogHelper.Error("LoggingExample", $"Error message #{_counter} - This is an error that might cause features to malfunction");
                    
                    // Also demonstrate exception logging
                    try
                    {
                        // Deliberately cause an exception
                        throw new InvalidOperationException("This is a test exception");
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Exception("LoggingExample", ex, "Caught a test exception");
                    }
                }
                
                // Wait for the next interval
                yield return new WaitForSeconds(_logInterval);
            }
        }
        
        /// <summary>
        /// Handle log events.
        /// </summary>
        /// <param name="logMessage">The log message that was written.</param>
        private void HandleLogWritten(LogMessage logMessage)
        {
            // This demonstrates how to handle log events
            // In a real application, you might want to display critical logs in the UI,
            // send them to a server, or trigger other actions
            
            if (logMessage.Level == LogLevel.Error)
            {
                Debug.Log($"<color=red>CRITICAL LOG EVENT: [{logMessage.Timestamp}] {logMessage.Message}</color>");
                
                // You could also trigger UI notifications, vibrations, etc.
            }
        }
        
        /// <summary>
        /// Display recent logs when a button is pressed.
        /// </summary>
        public void DisplayRecentLogs()
        {
            LogMessage[] recentLogs = LogHelper.GetRecentLogs(10);
            
            Debug.Log("=== RECENT LOGS ===");
            foreach (var log in recentLogs)
            {
                Debug.Log($"[{log.Timestamp}] [{log.Level}] [{log.Module}] {log.Message}");
            }
            Debug.Log("===================");
        }
        
        /// <summary>
        /// Change the log level at runtime.
        /// </summary>
        /// <param name="level">The new log level.</param>
        public void SetLogLevel(int level)
        {
            LogLevel newLevel = (LogLevel)level;
            LogHelper.SetLogLevel(newLevel);
            LogHelper.Info("LoggingExample", $"Log level changed to {newLevel}");
        }
        
        /// <summary>
        /// Toggle console output at runtime.
        /// </summary>
        /// <param name="enabled">Whether console output should be enabled.</param>
        public void ToggleConsoleOutput(bool enabled)
        {
            _enableConsoleOutput = enabled;
            LogHelper.SetConsoleOutput(enabled);
            LogHelper.Info("LoggingExample", $"Console output {(enabled ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// Toggle file output at runtime.
        /// </summary>
        /// <param name="enabled">Whether file output should be enabled.</param>
        public void ToggleFileOutput(bool enabled)
        {
            _enableFileOutput = enabled;
            LogHelper.SetFileOutput(enabled);
            LogHelper.Info("LoggingExample", $"File output {(enabled ? "enabled" : "disabled")}");
        }
    }
} 