using System;
using Core.Core.Shared.Interfaces;

namespace Core.Core.Logging
{
    /// <summary>
    /// Interface for the logging service
    /// </summary>
    public interface ILoggingService : IInitializable, IDisposable
    {
        event Action<LogEntry> OnLogAdded;

        void Log(string message, LogLevel level = LogLevel.Info, string category = "General");
        void LogInfo(string message, string category = "General");
        void LogWarning(string message, string category = "General");
        void LogError(string message, string category = "General");
        void LogException(System.Exception exception, string category = "General");

        LogEntry[] GetLogs();
        LogEntry[] GetLogsByLevel(LogLevel level);
        LogEntry[] GetLogsByCategory(string category);

        void ClearLogs();
        string ExportLogs();
        void SaveLogsToFile(string filePath);

        void SetLogLevel(LogLevel minLevel);
        void EnableCategory(string category);
        void DisableCategory(string category);
    }
}
