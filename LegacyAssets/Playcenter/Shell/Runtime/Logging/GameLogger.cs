using System;

namespace Playcenter.Shell
{
    /// <summary>
    /// Static logging facade. Requires <see cref="SetService"/> from the game
    /// composition root (build callback + <c>LoggingBootstrap</c>) before any log call.
    /// Fail-closed: no Console fallback — mis-wiring surfaces immediately.
    /// </summary>
    public static class GameLogger
    {
        private static ILoggingService _service;

        /// <summary>True after a non-null service has been installed.</summary>
        public static bool IsWired => _service != null;

        public static void SetService(ILoggingService service) => _service = service;

        /// <summary>Clears the facade (EditMode tests / domain teardown).</summary>
        public static void ClearService() => _service = null;

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
                    "GameLogger has no ILoggingService. Wire via RootLifetimeScope RegisterBuildCallback " +
                    "and LoggingBootstrap before any product log call.");
            }
        }
    }
}
