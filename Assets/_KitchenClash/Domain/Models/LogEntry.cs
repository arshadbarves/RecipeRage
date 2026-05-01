using System;

namespace KitchenClash.Domain
{
    public sealed class LogEntry
    {
        public string Message { get; }
        public LogLevel Level { get; }
        public string Category { get; }
        public string Timestamp { get; }
        public string StackTrace { get; }

        public LogEntry(string message, LogLevel level, string category, string stackTrace = null)
        {
            Message = message;
            Level = level;
            Category = category;
            Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            StackTrace = stackTrace;
        }

        public override string ToString() => $"[{Timestamp}] [{Level}] [{Category}] {Message}";
    }
}
