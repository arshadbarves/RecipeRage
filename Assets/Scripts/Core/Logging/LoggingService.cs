using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Core.Logging
{
    /// <summary>
    /// Powerful logging service with filtering, export, and Unity log capture
    /// </summary>
    public class LoggingService : ILoggingService
    {
        private readonly List<LogEntry> _logs = new List<LogEntry>();
        private readonly HashSet<string> _disabledCategories = new HashSet<string>();
        private readonly int _maxLogEntries;
        private LogLevel _minLogLevel = LogLevel.Verbose;
        private bool _isLoggingInternally = false; // Prevent infinite loop

        private readonly int _mainThreadId;

        public event Action<LogEntry> OnLogAdded;

        public LoggingService(int maxLogEntries = 5000)
        {
            _mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            _maxLogEntries = maxLogEntries;

            Application.logMessageReceived += HandleUnityLog;
        }

        public void Initialize() { }

        [HideInCallstack]
        public void Log(string message, LogLevel level = LogLevel.Info, string category = "General")
        {
            if (System.Threading.Thread.CurrentThread.ManagedThreadId == _mainThreadId)
            {
                LogInternal(message, level, category);
            }
            else
            {
                Cysharp.Threading.Tasks.UniTask.Post(async () =>
                {
                    await Cysharp.Threading.Tasks.UniTask.SwitchToMainThread();
                    LogInternal(message, level, category);
                }, Cysharp.Threading.Tasks.PlayerLoopTiming.Update);
            }
        }

        private void LogInternal(string message, LogLevel level, string category)
        {
            if (level < _minLogLevel) return;
            if (_disabledCategories.Contains(category)) return;

            var entry = new LogEntry(message, level, category);
            AddLogEntry(entry);

            OutputToUnityConsole(entry);
        }

        [HideInCallstack]
        public void LogInfo(string message, string category = "General")
        {
            Log(message, LogLevel.Info, category);
        }

        [HideInCallstack]
        public void LogWarning(string message, string category = "General")
        {
            Log(message, LogLevel.Warning, category);
        }

        [HideInCallstack]
        public void LogError(string message, string category = "General")
        {
            Log(message, LogLevel.Error, category);
        }

        [HideInCallstack]
        public void LogException(Exception exception, string category = "General")
        {
            if (System.Threading.Thread.CurrentThread.ManagedThreadId == _mainThreadId)
            {
                LogExceptionInternal(exception, category);
            }
            else
            {
                Cysharp.Threading.Tasks.UniTask.Post(async () =>
                {
                    await Cysharp.Threading.Tasks.UniTask.SwitchToMainThread();
                    LogExceptionInternal(exception, category);
                }, Cysharp.Threading.Tasks.PlayerLoopTiming.Update);
            }
        }

        private void LogExceptionInternal(Exception exception, string category)
        {
            var entry = new LogEntry(
                exception.Message,
                LogLevel.Critical,
                category,
                exception.StackTrace
            );
            AddLogEntry(entry);

            Debug.LogException(exception);
        }

        private void HandleUnityLog(string logString, string stackTrace, LogType type)
        {
            LogLevel level = type switch
            {
                LogType.Error => LogLevel.Error,
                LogType.Assert => LogLevel.Error,
                LogType.Warning => LogLevel.Warning,
                LogType.Log => LogLevel.Info,
                LogType.Exception => LogLevel.Critical,
                _ => LogLevel.Info
            };

            var entry = new LogEntry(logString, level, "Unity", stackTrace);

            lock (_logs)
            {
                _logs.Add(entry);
                if (_logs.Count > _maxLogEntries)
                {
                    _logs.RemoveAt(0);
                }
            }

            OnLogAdded?.Invoke(entry);
        }

        private void AddLogEntry(LogEntry entry)
        {
            lock (_logs)
            {
                _logs.Add(entry);

                if (_logs.Count > _maxLogEntries)
                {
                    _logs.RemoveAt(0);
                }
            }

            OnLogAdded?.Invoke(entry);
        }

        public LogEntry[] GetLogs()
        {
            lock (_logs)
            {
                return _logs.ToArray();
            }
        }

        public LogEntry[] GetLogsByLevel(LogLevel level)
        {
            lock (_logs)
            {
                return _logs.Where(log => log.Level == level).ToArray();
            }
        }

        public LogEntry[] GetLogsByCategory(string category)
        {
            lock (_logs)
            {
                return _logs.Where(log => log.Category == category).ToArray();
            }
        }

        public void ClearLogs()
        {
            lock (_logs)
            {
                _logs.Clear();
            }
        }

        public string ExportLogs()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Game Logs Export ===");
            sb.AppendLine($"Export Time: {DateTime.Now}");
            sb.AppendLine($"Total Entries: {_logs.Count}");
            sb.AppendLine($"Unity Version: {Application.unityVersion}");
            sb.AppendLine($"Platform: {Application.platform}");
            sb.AppendLine($"Device: {SystemInfo.deviceModel}");
            sb.AppendLine("========================\n");

            lock (_logs)
            {
                foreach (var log in _logs)
                {
                    sb.AppendLine(log.ToString());
                    if (!string.IsNullOrEmpty(log.StackTrace))
                    {
                        sb.AppendLine($"Stack Trace:\n{log.StackTrace}");
                    }
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        public void SaveLogsToFile(string filePath)
        {
            try
            {
                string content = ExportLogs();
                File.WriteAllText(filePath, content);
                Debug.Log($"Logs saved to: {filePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save logs: {ex.Message}");
            }
        }

        public void SetLogLevel(LogLevel minLevel)
        {
            _minLogLevel = minLevel;
        }

        public void EnableCategory(string category)
        {
            _disabledCategories.Remove(category);
        }

        public void DisableCategory(string category)
        {
            _disabledCategories.Add(category);
        }

        /// <summary>
        /// Outputs log entry to Unity Console with proper formatting
        /// </summary>
        [HideInCallstack]
        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        private void OutputToUnityConsole(LogEntry entry)
        {
            string formattedMessage = $"[RecipeRage] - [{entry.Category}] {entry.Message}";

            switch (entry.Level)
            {
                case LogLevel.Verbose:
                case LogLevel.Info:
                    Debug.Log(formattedMessage);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(formattedMessage);
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    Debug.LogError(formattedMessage);
                    break;
            }
        }

        public void Dispose()
        {
            Application.logMessageReceived -= HandleUnityLog;
        }
    }
}