using UnityEngine;

namespace Modules.Shared.Utilities
{
    public static class PlatformUtils
    {
        public static string GetPlatform()
        {
#if UNITY_IOS
            return "ios";
#elif UNITY_ANDROID
            return "android";
#elif UNITY_STANDALONE
            return "pc";
#else
            return "unknown";
#endif
        }

        public static string GetEnvironment()
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            return "development";
#elif STAGING
            return "staging";
#else
            return "production";
#endif
        }

        public static bool IsMobile => Application.isMobilePlatform;
        public static bool IsEditor => Application.isEditor;
        public static bool IsDevelopment => Debug.isDebugBuild;
    }
}
