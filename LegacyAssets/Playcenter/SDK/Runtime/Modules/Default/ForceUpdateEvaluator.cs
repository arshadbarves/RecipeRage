using System;

namespace Playcenter.SDK
{
    /// <summary>Pure static version comparator ported from ForceUpdateChecker (no UnityEngine dependency).</summary>
    public static class ForceUpdateEvaluator
    {
        /// <summary>Returns negative if current &lt; minimum, 0 if equal, positive if current &gt; minimum.</summary>
        public static int CompareVersions(string current, string minimum)
        {
            int[] partsA = ParseVersion(current);
            int[] partsB = ParseVersion(minimum);

            for (int i = 0; i < 3; i++)
            {
                int diff = partsA[i] - partsB[i];
                if (diff != 0)
                    return diff;
            }
            return 0;
        }

        public static bool IsUpdateRequired(string current, string minimum)
            => !string.IsNullOrEmpty(minimum) && CompareVersions(current, minimum) < 0;

        static int[] ParseVersion(string version)
        {
            int[] result = new int[3];
            if (string.IsNullOrEmpty(version))
                return result;

            string[] parts = version.Split('.');
            for (int i = 0; i < Math.Min(parts.Length, 3); i++)
                int.TryParse(parts[i], out result[i]);

            return result;
        }
    }
}
