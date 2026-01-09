using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Core.Logging
{
    /// <summary>
    /// Static helper for easy logging throughout the codebase
    /// Only active in development builds
    /// Uses [HideInCallstack] to show correct file/line in Unity Console
    /// Automatically captures source file name as category using [CallerFilePath]
    /// </summary>
    public static class GameLogger
    {
        private static ILoggingService _logger;

        public static void Initialize(ILoggingService logger)
        {
            _logger = logger;
        }

        private static ILoggingService Logger => _logger;

        /// <summary>
        /// Extracts clean file name from full path (e.g., "PlayerController" from "Assets/Scripts/Core/Characters/PlayerController.cs")
        /// </summary>
        private static string GetCategoryFromFilePath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return "General";

            return Path.GetFileNameWithoutExtension(filePath);
        }

        [HideInCallstack]
        public static void Log(string message, [CallerFilePath] string filePath = "")
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            Logger?.LogInfo(message, GetCategoryFromFilePath(filePath));
#endif
        }

        [HideInCallstack]
        public static void LogInfo(string message, [CallerFilePath] string filePath = "")
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            Logger?.LogInfo(message, GetCategoryFromFilePath(filePath));
#endif
        }

        [HideInCallstack]
        public static void LogWarning(string message, [CallerFilePath] string filePath = "")
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            Logger?.LogWarning(message, GetCategoryFromFilePath(filePath));
#endif
        }

        [HideInCallstack]
        public static void LogError(string message, [CallerFilePath] string filePath = "")
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            Logger?.LogError(message, GetCategoryFromFilePath(filePath));
#endif
        }

        [HideInCallstack]
        public static void LogException(System.Exception exception, [CallerFilePath] string filePath = "")
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            Logger?.LogException(exception, GetCategoryFromFilePath(filePath));
#endif
        }

    }
}
