using System;
using RecipeRage.Modules.Logging.Core;
using RecipeRage.Modules.Logging.Interfaces;

namespace RecipeRage.Modules.Logging
{
    /// <summary>
    /// Static helper class for easy access to logging functionality
    /// 
    /// Complexity Rating: 1
    /// </summary>
    public static class LogHelper
    {
        private static ILogService _logService = LogService.Instance;
        
        /// <summary>
        /// Event triggered when a log is written
        /// </summary>
        public static event Action<LogMessage> OnLogWritten
        {
            add { _logService.OnLogWritten += value; }
            remove { _logService.OnLogWritten -= value; }
        }
        
        /// <summary>
        /// Log a debug message
        /// </summary>
        /// <param name="module">Source module name</param>
        /// <param name="message">Log message</param>
        public static void Debug(string module, string message)
        {
            _logService.Debug(module, message);
        }
        
        /// <summary>
        /// Log an info message
        /// </summary>
        /// <param name="module">Source module name</param>
        /// <param name="message">Log message</param>
        public static void Info(string module, string message)
        {
            _logService.Info(module, message);
        }
        
        /// <summary>
        /// Log a warning message
        /// </summary>
        /// <param name="module">Source module name</param>
        /// <param name="message">Log message</param>
        public static void Warning(string module, string message)
        {
            _logService.Warning(module, message);
        }
        
        /// <summary>
        /// Log an error message
        /// </summary>
        /// <param name="module">Source module name</param>
        /// <param name="message">Log message</param>
        public static void Error(string module, string message)
        {
            _logService.Error(module, message);
        }
        
        /// <summary>
        /// Log an exception
        /// </summary>
        /// <param name="module">Source module name</param>
        /// <param name="exception">Exception to log</param>
        /// <param name="message">Optional additional message</param>
        public static void Exception(string module, Exception exception, string message = null)
        {
            _logService.Exception(module, exception, message);
        }
        
        /// <summary>
        /// Set the minimum log level to display
        /// </summary>
        /// <param name="level">Minimum log level</param>
        public static void SetLogLevel(LogLevel level)
        {
            _logService.SetLogLevel(level);
        }
        
        /// <summary>
        /// Get the most recent logs
        /// </summary>
        /// <param name="count">Number of logs to retrieve</param>
        /// <returns>Array of recent log messages</returns>
        public static LogMessage[] GetRecentLogs(int count = 100)
        {
            return _logService.GetRecentLogs(count);
        }
        
        /// <summary>
        /// Enable or disable console output
        /// </summary>
        /// <param name="enabled">Whether to enable console output</param>
        public static void SetConsoleOutput(bool enabled)
        {
            _logService.SetConsoleOutput(enabled);
        }
        
        /// <summary>
        /// Enable or disable file output
        /// </summary>
        /// <param name="enabled">Whether to enable file output</param>
        /// <param name="filePath">Optional file path for logs</param>
        public static void SetFileOutput(bool enabled, string filePath = null)
        {
            _logService.SetFileOutput(enabled, filePath);
        }
        
        /// <summary>
        /// Set a custom log service implementation
        /// </summary>
        /// <param name="logService">Log service implementation</param>
        public static void SetLogService(ILogService logService)
        {
            _logService = logService ?? LogService.Instance;
        }
    }
} 