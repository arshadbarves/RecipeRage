using UnityEngine;

namespace KitchenClash.Infrastructure.Configuration
{
    /// <summary>
    /// Unity Gaming Services project settings (ScriptableObject).
    /// Lives in Configuration leaf assembly so EOS/Network do not form a cycle over config.
    /// </summary>
    [CreateAssetMenu(fileName = "UGSConfig", menuName = "KitchenClash/Config/UGS Config")]
    public class UGSConfig : ScriptableObject
    {
        [Header("Project Settings")]
        public string projectId = "";

        [Header("Feature Flags")]
        public bool enableFriendsSystem = true;
        public bool autoAuthenticate = true;

        [Header("Authentication")]
        public string authenticationProfile = "default";

        public bool IsValid() => !string.IsNullOrEmpty(projectId);
    }
}
