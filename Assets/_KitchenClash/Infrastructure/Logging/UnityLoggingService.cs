using System;
using System.Collections.Generic;
using System.Linq;
using KitchenClash.Domain;

namespace KitchenClash.Infrastructure.Logging
{
    public sealed class UnityLoggingService : ILoggingService
    {
        private readonly List<LogEntry> _logs = new();
        private LogLevel _minLevel = LogLevel.Verbose;

        public event Action<LogEntry> OnLogAdded;

        public void Log(string message, LogLevel level = LogLevel.Info, string category = "General")
        {
            if (level < _minLevel) return;
            var entry = new LogEntry(message, level, category);
            _logs.Add(entry);
            OnLogAdded?.Invoke(entry);

            switch (level)
            {
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning($"[{category}] {message}");
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    UnityEngine.Debug.LogError($"[{category}] {message}");
                    break;
                default:
                    UnityEngine.Debug.Log($"[{category}] {message}");
                    break;
            }
        }

        public void LogInfo(string message, string category = "General") => Log(message, LogLevel.Info, category);
        public void LogWarning(string message, string category = "General") => Log(message, LogLevel.Warning, category);
        public void LogError(string message, string category = "General") => Log(message, LogLevel.Error, category);

        public void LogException(Exception exception, string category = "General")
        {
            var entry = new LogEntry(exception.Message, LogLevel.Error, category, exception.StackTrace);
            _logs.Add(entry);
            OnLogAdded?.Invoke(entry);
            UnityEngine.Debug.LogException(exception);
        }

        public LogEntry[] GetLogs() => _logs.ToArray();
        public void ClearLogs() => _logs.Clear();
        public void Dispose() => _logs.Clear();
    }
}
