using System;

namespace RecipeRage.Modules.Logging.Interfaces
{
    /// <summary>
    /// Interface for the logging service
    /// Provides unified logging capabilities throughout the application
    /// 
    /// Complexity Rating: 2
    /// </summary>
    public interface ILogService
    {
        /// <summary>
        /// Event triggered when a log is written
        /// </summary>
        event Action<LogMessage> OnLogWritten;
        
        /// <summary>
        /// Log a debug message
        /// </summary>
        /// <param name="module">Source module name</param>
        /// <param name="message">Log message</param>
        void Debug(string module, string message);
        
        /// <summary>
        /// Log an info message
        /// </summary>
        /// <param name="module">Source module name</param>
        /// <param name="message">Log message</param>
        void Info(string module, string message);
        
        /// <summary>
        /// Log a warning message
        /// </summary>
        /// <param name="module">Source module name</param>
        /// <param name="message">Log message</param>
        void Warning(string module, string message);
        
        /// <summary>
        /// Log an error message
        /// </summary>
        /// <param name="module">Source module name</param>
        /// <param name="message">Log message</param>
        void Error(string module, string message);
        
        /// <summary>
        /// Log an exception
        /// </summary>
        /// <param name="module">Source module name</param>
        /// <param name="exception">Exception to log</param>
        /// <param name="message">Optional additional message</param>
        void Exception(string module, Exception exception, string message = null);
        
        /// <summary>
        /// Set the minimum log level to display
        /// </summary>
        /// <param name="level">Minimum log level</param>
        void SetLogLevel(LogLevel level);
        
        /// <summary>
        /// Get the most recent logs
        /// </summary>
        /// <param name="count">Number of logs to retrieve</param>
        /// <returns>Array of recent log messages</returns>
        LogMessage[] GetRecentLogs(int count = 100);
        
        /// <summary>
        /// Enable or disable console output
        /// </summary>
        /// <param name="enabled">Whether to enable console output</param>
        void SetConsoleOutput(bool enabled);
        
        /// <summary>
        /// Enable or disable file output
        /// </summary>
        /// <param name="enabled">Whether to enable file output</param>
        /// <param name="filePath">Optional file path for logs</param>
        void SetFileOutput(bool enabled, string filePath = null);
    }
    
    /// <summary>
    /// Log severity levels
    /// 
    /// Complexity Rating: 1
    /// </summary>
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
        None = 4
    }
    
    /// <summary>
    /// Represents a log message
    /// 
    /// Complexity Rating: 1
    /// </summary>
    public class LogMessage
    {
        /// <summary>
        /// Log severity level
        /// </summary>
        public LogLevel Level { get; set; }
        
        /// <summary>
        /// Source module name
        /// </summary>
        public string Module { get; set; }
        
        /// <summary>
        /// Log message content
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Exception details (if applicable)
        /// </summary>
        public Exception Exception { get; set; }
        
        /// <summary>
        /// Timestamp when the log was created
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Create a new log message
        /// </summary>
        public LogMessage(LogLevel level, string module, string message, Exception exception = null)
        {
            Level = level;
            Module = module;
            Message = message;
            Exception = exception;
            Timestamp = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Convert the log message to a string
        /// </summary>
        public override string ToString()
        {
            string levelStr = Level.ToString().ToUpper();
            string timestamp = Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string baseMessage = $"[{timestamp}] [{levelStr}] [{Module}] {Message}";
            
            if (Exception != null)
            {
                return $"{baseMessage}\nException: {Exception.Message}\nStack Trace: {Exception.StackTrace}";
            }
            
            return baseMessage;
        }
    }
} 