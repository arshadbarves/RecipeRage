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

            if (UnityEngine.Application.isPlaying) // Simple check, but doesn't guarantee thread safety
            {
                // We can use a try-catch for safety if we don't have a robust main-thread check
                 try
                 {
                     FrameCount = UnityEngine.Time.frameCount;
                 }
                 catch
                 {
                     FrameCount = -1;
                 }
            }
            else
            {
                FrameCount = 0;
            }
        }

        public override string ToString()
        {
            return $"[{Timestamp}] [{Level}] [{Category}] {Message}";
        }
    }
}
