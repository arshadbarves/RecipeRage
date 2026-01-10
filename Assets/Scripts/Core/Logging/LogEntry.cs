using System;

namespace Core.Logging
{
    [Serializable]
    public class LogEntry
    {
        public string Message;
        public LogLevel Level;
        public string Category;
        public string Timestamp;
        public string StackTrace;

        public LogEntry(string message, LogLevel level, string category, string stackTrace = null)
        {
            Message = message;
            Level = level;
            Category = category;
            Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            StackTrace = stackTrace;
        }

        public override string ToString()
        {
            return $"[{Timestamp}] [{Level}] [{Category}] {Message}";
        }
    }
}
