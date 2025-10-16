using System;

namespace Core.Logging
{
    /// <summary>
    /// Represents a single log entry
    /// </summary>
    [Serializable]
    public class LogEntry
    {
        public string Message;
        public LogLevel Level;
        public string Category;
        public string Timestamp;
        public string StackTrace;
        public int FrameCount;

        public LogEntry(string message, LogLevel level, string category, string stackTrace = null)
        {
            Message = message;
            Level = level;
            Category = category;
            Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            StackTrace = stackTrace;
            FrameCount = UnityEngine.Time.frameCount;
        }

        public override string ToString()
        {
            return $"[{Timestamp}] [{Level}] [{Category}] {Message}";
        }
    }
}
