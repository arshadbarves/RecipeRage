using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using RecipeRage.Modules.Logging.Interfaces;

namespace RecipeRage.Modules.Logging.Core
{
    /// <summary>
    /// Implementation of the logging service
    /// 
    /// Complexity Rating: 3
    /// </summary>
    public class LogService : ILogService
    {
        private const int MAX_LOG_HISTORY = 1000;
        private readonly Queue<LogMessage> _logHistory;
        private LogLevel _minimumLevel = LogLevel.Debug;
        private bool _consoleOutput = true;
        private bool _fileOutput = false;
        private string _logFilePath;
        private static readonly object _lock = new object();
        
        /// <summary>
        /// Event triggered when a log is written
        /// </summary>
        public event Action<LogMessage> OnLogWritten;
        
        /// <summary>
        /// Singleton instance
        /// </summary>
        private static LogService _instance;
        
        /// <summary>
        /// Get the singleton instance
        /// </summary>
        public static LogService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LogService();
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        private LogService()
        {
            _logHistory = new Queue<LogMessage>(MAX_LOG_HISTORY);
            _logFilePath = Path.Combine(Application.persistentDataPath, "Logs", "app.log");
            
            // Ensure logs directory exists
            string logsDir = Path.GetDirectoryName(_logFilePath);
            if (!Directory.Exists(logsDir))
            {
                Directory.CreateDirectory(logsDir);
            }
            
            // Subscribe to Unity's log events
            Application.logMessageReceived += HandleUnityLog;
        }
        
        /// <summary>
        /// Log a debug message
        /// </summary>
        public void Debug(string module, string message)
        {
            WriteLog(new LogMessage(LogLevel.Debug, module, message));
        }
        
        /// <summary>
        /// Log an info message
        /// </summary>
        public void Info(string module, string message)
        {
            WriteLog(new LogMessage(LogLevel.Info, module, message));
        }
        
        /// <summary>
        /// Log a warning message
        /// </summary>
        public void Warning(string module, string message)
        {
            WriteLog(new LogMessage(LogLevel.Warning, module, message));
        }
        
        /// <summary>
        /// Log an error message
        /// </summary>
        public void Error(string module, string message)
        {
            WriteLog(new LogMessage(LogLevel.Error, module, message));
        }
        
        /// <summary>
        /// Log an exception
        /// </summary>
        public void Exception(string module, Exception exception, string message = null)
        {
            message = string.IsNullOrEmpty(message) ? 
                $"Exception: {exception.GetType().Name}" : 
                $"{message} | Exception: {exception.GetType().Name}";
                
            WriteLog(new LogMessage(LogLevel.Error, module, message, exception));
        }
        
        /// <summary>
        /// Set the minimum log level to display
        /// </summary>
        public void SetLogLevel(LogLevel level)
        {
            _minimumLevel = level;
        }
        
        /// <summary>
        /// Get the most recent logs
        /// </summary>
        public LogMessage[] GetRecentLogs(int count = 100)
        {
            lock (_lock)
            {
                count = Math.Min(count, _logHistory.Count);
                LogMessage[] logs = new LogMessage[count];
                LogMessage[] allLogs = _logHistory.ToArray();
                
                // Get the most recent 'count' logs
                Array.Copy(allLogs, allLogs.Length - count, logs, 0, count);
                return logs;
            }
        }
        
        /// <summary>
        /// Enable or disable console output
        /// </summary>
        public void SetConsoleOutput(bool enabled)
        {
            _consoleOutput = enabled;
        }
        
        /// <summary>
        /// Enable or disable file output
        /// </summary>
        public void SetFileOutput(bool enabled, string filePath = null)
        {
            _fileOutput = enabled;
            
            if (!string.IsNullOrEmpty(filePath))
            {
                _logFilePath = filePath;
                
                // Ensure directory exists
                string logsDir = Path.GetDirectoryName(_logFilePath);
                if (!Directory.Exists(logsDir))
                {
                    Directory.CreateDirectory(logsDir);
                }
            }
        }
        
        /// <summary>
        /// Write a log message
        /// </summary>
        private void WriteLog(LogMessage log)
        {
            if (log.Level < _minimumLevel)
                return;
                
            lock (_lock)
            {
                // Add to history, removing oldest if at capacity
                if (_logHistory.Count >= MAX_LOG_HISTORY)
                {
                    _logHistory.Dequeue();
                }
                _logHistory.Enqueue(log);
                
                // Write to console
                if (_consoleOutput)
                {
                    WriteToConsole(log);
                }
                
                // Write to file
                if (_fileOutput)
                {
                    WriteToFile(log);
                }
                
                // Trigger event
                OnLogWritten?.Invoke(log);
            }
        }
        
        /// <summary>
        /// Write a log message to the console
        /// </summary>
        private void WriteToConsole(LogMessage log)
        {
            switch (log.Level)
            {
                case LogLevel.Debug:
                    UnityEngine.Debug.Log($"[{log.Module}] {log.Message}");
                    break;
                case LogLevel.Info:
                    UnityEngine.Debug.Log($"[{log.Module}] {log.Message}");
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning($"[{log.Module}] {log.Message}");
                    break;
                case LogLevel.Error:
                    if (log.Exception != null)
                    {
                        UnityEngine.Debug.LogException(log.Exception);
                    }
                    UnityEngine.Debug.LogError($"[{log.Module}] {log.Message}");
                    break;
            }
        }
        
        /// <summary>
        /// Write a log message to the file
        /// </summary>
        private void WriteToFile(LogMessage log)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(_logFilePath, true, Encoding.UTF8))
                {
                    writer.WriteLine(log.ToString());
                }
            }
            catch (Exception ex)
            {
                // Don't use WriteLog here to avoid potential recursion
                UnityEngine.Debug.LogError($"Failed to write to log file: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Handle Unity's log events
        /// </summary>
        private void HandleUnityLog(string logString, string stackTrace, LogType type)
        {
            // Ignore logs that we generated ourselves to avoid duplication
            if (logString.StartsWith("["))
                return;
                
            LogLevel level;
            switch (type)
            {
                case LogType.Log:
                    level = LogLevel.Info;
                    break;
                case LogType.Warning:
                    level = LogLevel.Warning;
                    break;
                case LogType.Error:
                case LogType.Exception:
                case LogType.Assert:
                    level = LogLevel.Error;
                    break;
                default:
                    level = LogLevel.Info;
                    break;
            }
            
            LogMessage log = new LogMessage(level, "Unity", logString);
            
            // Only add to our history and file, not console (to avoid double logging)
            lock (_lock)
            {
                if (_logHistory.Count >= MAX_LOG_HISTORY)
                {
                    _logHistory.Dequeue();
                }
                _logHistory.Enqueue(log);
                
                if (_fileOutput)
                {
                    WriteToFile(log);
                }
                
                OnLogWritten?.Invoke(log);
            }
        }
    }
} 