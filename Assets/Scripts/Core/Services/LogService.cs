using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using RecipeRage.Core.Patterns;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace RecipeRage.Core.Services
{
    /// <summary>
    /// Log severity levels for categorizing log messages
    /// </summary>
    public enum LogSeverity
    {
        Verbose = 0,
        Debug = 1,
        Info = 2,
        Warning = 3,
        Error = 4,
        Fatal = 5
    }

    /// <summary>
    /// Production-ready logging service that handles both local and remote logging.
    /// Implements the Singleton pattern for global access.
    /// Complexity Rating: 3
    /// </summary>
    public class LogService : Singleton<LogService>
    {

        /// <summary>
        /// Maximum number of logs to keep in the remote queue
        /// </summary>
        private const int MAX_REMOTE_QUEUE_SIZE = 100;

        /// <summary>
        /// Queue of logs to send to remote server
        /// </summary>
        private readonly Queue<string> _remoteLogQueue = new Queue<string>();
        /// <summary>
        /// Minimum log level to display in the console
        /// </summary>
        private LogSeverity _consoleLogLevel = LogSeverity.Debug;

        /// <summary>
        /// Whether to enable file logging
        /// </summary>
        private bool _enableFileLogging = true;

        /// <summary>
        /// Whether to enable remote logging
        /// </summary>
        private bool _enableRemoteLogging;

        /// <summary>
        /// Minimum log level to save to file
        /// </summary>
        private LogSeverity _fileLogLevel = LogSeverity.Info;

        /// <summary>
        /// Whether the log service has been initialized
        /// </summary>
        private bool _isInitialized;

        /// <summary>
        /// Log file path
        /// </summary>
        private string _logFilePath;

        /// <summary>
        /// Minimum log level to send to remote server
        /// </summary>
        private LogSeverity _remoteLogLevel = LogSeverity.Error;

        /// <summary>
        /// Public constructor required by Singleton<T>
        /// </summary>
        public LogService()
        {
            // Public constructor to satisfy Singleton<T> constraint
        }

        /// <summary>
        /// Initialize the log service
        /// </summary>
        /// <param name="consoleLogLevel"> Minimum log level for console </param>
        /// <param name="fileLogLevel"> Minimum log level for file </param>
        /// <param name="remoteLogLevel"> Minimum log level for remote </param>
        /// <param name="enableRemoteLogging"> Whether to enable remote logging </param>
        /// <param name="enableFileLogging"> Whether to enable file logging </param>
        public void Initialize(
            LogSeverity consoleLogLevel = LogSeverity.Debug,
            LogSeverity fileLogLevel = LogSeverity.Info,
            LogSeverity remoteLogLevel = LogSeverity.Error,
            bool enableRemoteLogging = false,
            bool enableFileLogging = true)
        {
            if (_isInitialized)
            {
                return;
            }

            _consoleLogLevel = consoleLogLevel;
            _fileLogLevel = fileLogLevel;
            _remoteLogLevel = remoteLogLevel;
            _enableRemoteLogging = enableRemoteLogging;
            _enableFileLogging = enableFileLogging;

            // Set up log file path
            if (_enableFileLogging)
            {
                string directory = Path.Combine(Application.persistentDataPath, "Logs");
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string fileName = $"log_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
                _logFilePath = Path.Combine(directory, fileName);

                // Write header to log file
                File.WriteAllText(_logFilePath, $"===== RecipeRage Log - {DateTime.Now:yyyy-MM-dd HH:mm:ss} =====\n");
                File.AppendAllText(_logFilePath, $"Device: {SystemInfo.deviceModel}\n");
                File.AppendAllText(_logFilePath, $"OS: {SystemInfo.operatingSystem}\n");
                File.AppendAllText(_logFilePath, $"App Version: {Application.version}\n");
                File.AppendAllText(_logFilePath, $"Unity Version: {Application.unityVersion}\n");
                File.AppendAllText(_logFilePath, "=================\n\n");
            }

            // Register for Unity log messages
            Application.logMessageReceived += HandleUnityLogMessage;

            // Log initialization success
            Logger.Info("LogService", "Logging system initialized");

            _isInitialized = true;
        }

        /// <summary>
        /// Handle Unity log messages
        /// </summary>
        /// <param name="logString"> Log message </param>
        /// <param name="stackTrace"> Stack trace </param>
        /// <param name="type"> Log type </param>
        private void HandleUnityLogMessage(string logString, string stackTrace, LogType type)
        {
            var severity = LogSeverity.Info;

            switch (type)
            {
                case LogType.Log:
                    severity = LogSeverity.Info;
                    break;
                case LogType.Warning:
                    severity = LogSeverity.Warning;
                    break;
                case LogType.Error:
                case LogType.Exception:
                    severity = LogSeverity.Error;
                    break;
                case LogType.Assert:
                    severity = LogSeverity.Fatal;
                    break;
            }

            // Only log messages that didn't come from our Logger class to avoid duplication
            if (!logString.StartsWith("[RecipeRage]"))
            {
                LogInternal("Unity", logString, severity, stackTrace);
            }
        }

        /// <summary>
        /// Log a message with the specified tag and severity
        /// </summary>
        /// <param name="tag"> Log tag or category </param>
        /// <param name="message"> Log message </param>
        /// <param name="severity"> Log severity </param>
        /// <param name="exception"> Optional exception </param>
        /// <param name="callerMemberName"> Name of the calling member (filled automatically) </param>
        /// <param name="callerFilePath"> Path of the calling file (filled automatically) </param>
        /// <param name="callerLineNumber"> Line number of the calling code (filled automatically) </param>
        public void Log(
            string tag,
            string message,
            LogSeverity severity = LogSeverity.Info,
            Exception exception = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            // Extract caller information for debugging
            string callerFileName = Path.GetFileName(callerFilePath);
            string stackTrace = exception?.StackTrace ?? new StackTrace(1, true).ToString();

            // Append caller information to the message
            string fullMessage = $"{message} [{callerFileName}:{callerLineNumber}, {callerMemberName}]";

            // Log the message
            LogInternal(tag, fullMessage, severity, stackTrace);
        }

        /// <summary>
        /// Internal method to log messages across different outputs
        /// </summary>
        /// <param name="tag"> Log tag or category </param>
        /// <param name="message"> Log message </param>
        /// <param name="severity"> Log severity </param>
        /// <param name="stackTrace"> Stack trace </param>
        private void LogInternal(string tag, string message, LogSeverity severity, string stackTrace = null)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string severityStr = severity.ToString().ToUpper();
            string formattedMessage = $"[RecipeRage] [{timestamp}] [{severityStr}] [{tag}] {message}";

            // Console logging
            if (severity >= _consoleLogLevel)
            {
                switch (severity)
                {
                    case LogSeverity.Verbose:
                    case LogSeverity.Debug:
                    case LogSeverity.Info:
                        Debug.Log(formattedMessage);
                        break;
                    case LogSeverity.Warning:
                        Debug.LogWarning(formattedMessage);
                        break;
                    case LogSeverity.Error:
                    case LogSeverity.Fatal:
                        Debug.LogError(formattedMessage);
                        break;
                }
            }

            // File logging
            if (_enableFileLogging && severity >= _fileLogLevel && !string.IsNullOrEmpty(_logFilePath))
            {
                try
                {
                    File.AppendAllText(_logFilePath, $"{formattedMessage}\n");

                    // Add stack trace for errors
                    if (severity >= LogSeverity.Error && !string.IsNullOrEmpty(stackTrace))
                    {
                        File.AppendAllText(_logFilePath, $"Stack Trace:\n{stackTrace}\n\n");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to write to log file: {ex.Message}");
                }
            }

            // Remote logging
            if (_enableRemoteLogging && severity >= _remoteLogLevel)
            {
                string remoteLog = $"{formattedMessage}{(severity >= LogSeverity.Error ? $"\nStack Trace:\n{stackTrace}" : "")}";

                // Add to queue
                _remoteLogQueue.Enqueue(remoteLog);

                // Trim queue if too large
                while (_remoteLogQueue.Count > MAX_REMOTE_QUEUE_SIZE)
                {
                    _remoteLogQueue.Dequeue();
                }

                // TODO: Implement remote logging - send logs to a server
                // This would typically be done in a background thread or coroutine
            }
        }

        /// <summary>
        /// Clean up resources when the application quits
        /// </summary>
        public void Shutdown()
        {
            if (!_isInitialized)
            {
                return;
            }

            // Unregister from Unity log messages
            Application.logMessageReceived -= HandleUnityLogMessage;

            // Log shutdown
            Logger.Info("LogService", "Logging system shut down");

            // Flush any pending logs
            if (_enableFileLogging && !string.IsNullOrEmpty(_logFilePath))
            {
                try
                {
                    File.AppendAllText(_logFilePath, $"\n===== Log End: {DateTime.Now:yyyy-MM-dd HH:mm:ss} =====\n");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to finalize log file: {ex.Message}");
                }
            }

            // TODO: Send any remaining logs to the remote server

            _isInitialized = false;
        }
    }

    /// <summary>
    /// Static logger class for easy access to logging functionality without needing to get an instance.
    /// Similar to Unity's Debug.Log but with more features.
    /// Complexity Rating: 1
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Log a verbose message
        /// </summary>
        /// <param name="tag"> Log tag or category </param>
        /// <param name="message"> Log message </param>
        /// <param name="exception"> Optional exception </param>
        [Conditional("ENABLE_LOGS")]
        public static void Verbose(string tag, string message, Exception exception = null)
        {
            LogService.Instance.Log(tag, message, LogSeverity.Verbose, exception);
        }

        /// <summary>
        /// Log a debug message
        /// </summary>
        /// <param name="tag"> Log tag or category </param>
        /// <param name="message"> Log message </param>
        /// <param name="exception"> Optional exception </param>
        [Conditional("ENABLE_LOGS")]
        public static void Debug(string tag, string message, Exception exception = null)
        {
            LogService.Instance.Log(tag, message, LogSeverity.Debug, exception);
        }

        /// <summary>
        /// Log an info message
        /// </summary>
        /// <param name="tag"> Log tag or category </param>
        /// <param name="message"> Log message </param>
        /// <param name="exception"> Optional exception </param>
        public static void Info(string tag, string message, Exception exception = null)
        {
            LogService.Instance.Log(tag, message, LogSeverity.Info, exception);
        }

        /// <summary>
        /// Log a warning message
        /// </summary>
        /// <param name="tag"> Log tag or category </param>
        /// <param name="message"> Log message </param>
        /// <param name="exception"> Optional exception </param>
        public static void Warning(string tag, string message, Exception exception = null)
        {
            LogService.Instance.Log(tag, message, LogSeverity.Warning, exception);
        }

        /// <summary>
        /// Log an error message
        /// </summary>
        /// <param name="tag"> Log tag or category </param>
        /// <param name="message"> Log message </param>
        /// <param name="exception"> Optional exception </param>
        public static void Error(string tag, string message, Exception exception = null)
        {
            LogService.Instance.Log(tag, message, LogSeverity.Error, exception);
        }

        /// <summary>
        /// Log a fatal error message
        /// </summary>
        /// <param name="tag"> Log tag or category </param>
        /// <param name="message"> Log message </param>
        /// <param name="exception"> Optional exception </param>
        public static void Fatal(string tag, string message, Exception exception = null)
        {
            LogService.Instance.Log(tag, message, LogSeverity.Fatal, exception);
        }
    }
}