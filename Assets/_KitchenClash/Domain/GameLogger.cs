using System;

namespace KitchenClash.Domain
{
    /// <summary>
    /// Static logging facade. Delegates to an ILoggingService if registered,
    /// otherwise writes to System.Console.
    /// </summary>
    public static class GameLogger
    {
        private static ILoggingService _service;

        public static void SetService(ILoggingService service) => _service = service;

        public static void Log(string message)
        {
            if (_service != null) _service.Log(message);
            else Console.WriteLine(message);
        }

        public static void LogInfo(string message) => Log(message);

        public static void LogWarning(string message)
        {
            if (_service != null) _service.LogWarning(message);
            else Console.WriteLine($"[WARN] {message}");
        }

        public static void LogError(string message)
        {
            if (_service != null) _service.LogError(message);
            else Console.WriteLine($"[ERROR] {message}");
        }

        public static void LogException(Exception ex)
        {
            LogError($"{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
        }
    }
}
