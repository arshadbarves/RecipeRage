using System;

namespace KitchenClash.Domain
{
    public interface ILoggingService : IDisposable
    {
        event Action<LogEntry> OnLogAdded;
        void Log(string message, LogLevel level = LogLevel.Info, string category = "General");
        void LogInfo(string message, string category = "General");
        void LogWarning(string message, string category = "General");
        void LogError(string message, string category = "General");
        void LogException(Exception exception, string category = "General");
        LogEntry[] GetLogs();
        void ClearLogs();
        void SaveLogsToFile(string filePath);
    }
}
