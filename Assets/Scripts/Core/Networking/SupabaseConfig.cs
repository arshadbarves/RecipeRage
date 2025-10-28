using UnityEngine;

namespace Core.Networking
{
    /// <summary>
    /// Supabase configuration
    /// Store your Supabase project URL and anon key here
    /// </summary>
    [CreateAssetMenu(fileName = "SupabaseConfig", menuName = "RecipeRage/Supabase Config")]
    public class SupabaseConfig : ScriptableObject
    {
        [Header("Supabase Credentials")]
        [Tooltip("Your Supabase project URL (e.g., https://xxxxx.supabase.co)")]
        public string projectUrl = "https://your-project.supabase.co";
        
        [Tooltip("Your Supabase anon/public key (safe to expose in client)")]
        [TextArea(3, 10)]
        public string anonKey = "your-anon-key-here";
        
        [Header("API Settings")]
        [Tooltip("Request timeout in seconds")]
        public int timeoutSeconds = 30;
        
        /// <summary>
        /// Get REST API base URL
        /// </summary>
        public string RestApiUrl => $"{projectUrl}/rest/v1";
        
        /// <summary>
        /// Validate configuration
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(projectUrl) 
                && !string.IsNullOrEmpty(anonKey)
                && projectUrl.StartsWith("https://")
                && anonKey.Length > 50; // anon keys are long JWT tokens
        }
    }
}
