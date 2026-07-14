using System;

namespace Playcenter.Shell
{
    /// <summary>
    /// Static logging facade. Requires <see cref="SetService"/> from game
    /// <c>LoggingBootstrap</c> before any log call. No Console fallback.
    /// </summary>
    public static class GameLogger
    {
        private static ILoggingService _service;

        public static void SetService(ILoggingService service) => _service = service;

        public static void Log(string message)
        {
            EnsureService();
            _service.Log(message);
        }

        public static void LogInfo(string message)
        {
            EnsureService();
            _service.LogInfo(message);
        }

        public static void LogWarning(string message)
        {
            EnsureService();
            _service.LogWarning(message);
        }

        public static void LogError(string message)
        {
            EnsureService();
            _service.LogError(message);
        }

        public static void LogException(Exception ex)
        {
            EnsureService();
            _service.LogException(ex);
        }

        private static void EnsureService()
        {
            if (_service == null)
            {
                throw new InvalidOperationException(
                    "GameLogger has no ILoggingService. Register LoggingBootstrap at root DI before logging.");
            }
        }
    }
}
