using UnityEngine;

namespace KitchenClash.Infrastructure.Network
{
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
