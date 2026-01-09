using UnityEngine;

namespace Core.Networking
{
    /// <summary>
    /// Unity Gaming Services configuration
    /// </summary>
    [CreateAssetMenu(fileName = "UGSConfig", menuName = "Config/UGS Config")]
    public class UGSConfig : ScriptableObject
    {
        [Header("Project Settings")]
        [Tooltip("Unity Project ID from Unity Dashboard")]
        public string projectId = "";

        [Header("Feature Flags")]
        [Tooltip("Enable UGS friends system")]
        public bool enableFriendsSystem = true;

        [Tooltip("Enable automatic authentication on startup")]
        public bool autoAuthenticate = true;

        [Header("Authentication")]
        [Tooltip("Authentication profile (for testing multiple accounts)")]
        public string authenticationProfile = "default";

        /// <summary>
        /// Validates configuration
        /// </summary>
        public bool IsValid()
        {
            if (string.IsNullOrEmpty(projectId))
            {
                Debug.LogError("UGS Project ID is not set");
                return false;
            }

            return true;
        }
    }
}
