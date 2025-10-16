using Core.Bootstrap;

namespace Core.Logging
{
    /// <summary>
    /// Static helper for easy logging throughout the codebase
    /// Only active in development builds
    /// </summary>
    public static class GameLogger
    {
        private static ILoggingService Logger => GameBootstrap.Services?.LoggingService;

        public static void Log(string message, string category = "General")
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            Logger?.LogInfo(message, category);
#endif
        }

        public static void LogInfo(string message, string category = "General")
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            Logger?.LogInfo(message, category);
#endif
        }

        public static void LogWarning(string message, string category = "General")
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            Logger?.LogWarning(message, category);
#endif
        }

        public static void LogError(string message, string category = "General")
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            Logger?.LogError(message, category);
#endif
        }

        public static void LogException(System.Exception exception, string category = "General")
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            Logger?.LogException(exception, category);
#endif
        }

        // Category-specific helpers
        public static class Network
        {
            public static void Log(string message) => LogInfo(message, "Network");
            public static void LogWarning(string message) => GameLogger.LogWarning(message, "Network");
            public static void LogError(string message) => GameLogger.LogError(message, "Network");
        }

        public static class Audio
        {
            public static void Log(string message) => LogInfo(message, "Audio");
            public static void LogWarning(string message) => GameLogger.LogWarning(message, "Audio");
            public static void LogError(string message) => GameLogger.LogError(message, "Audio");
        }

        public static class Save
        {
            public static void Log(string message) => LogInfo(message, "SaveSystem");
            public static void LogWarning(string message) => GameLogger.LogWarning(message, "SaveSystem");
            public static void LogError(string message) => GameLogger.LogError(message, "SaveSystem");
        }

        public static class Auth
        {
            public static void Log(string message) => LogInfo(message, "Authentication");
            public static void LogWarning(string message) => GameLogger.LogWarning(message, "Authentication");
            public static void LogError(string message) => GameLogger.LogError(message, "Authentication");
        }

        public static class UI
        {
            public static void Log(string message) => LogInfo(message, "UI");
            public static void LogWarning(string message) => GameLogger.LogWarning(message, "UI");
            public static void LogError(string message) => GameLogger.LogError(message, "UI");
        }
    }
}
