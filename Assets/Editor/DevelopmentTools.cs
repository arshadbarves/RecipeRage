using UnityEditor;
using UnityEngine;
using PlayEveryWare.EpicOnlineServices;

namespace Editor
{
    /// <summary>
    /// Development tools for clearing authentication and player data.
    /// </summary>
    public static class DevelopmentTools
    {
        [MenuItem("Tools/Development/Clear All Data (EOS + PlayerPrefs)", priority = 1)]
        public static void ClearAllData()
        {
            if (EditorUtility.DisplayDialog(
                "Clear All Data",
                "This will clear:\n\n" +
                "• EOS Authentication (logout)\n" +
                "• All PlayerPrefs data\n" +
                "• Saved login methods\n\n" +
                "Are you sure you want to continue?",
                "Yes, Clear Everything",
                "Cancel"))
            {
                ClearEOSAuth();
                ClearPlayerPrefs();
                
                Debug.Log("<color=green>[DevelopmentTools] All data cleared successfully! You can now start fresh.</color>");
                EditorUtility.DisplayDialog("Success", "All data cleared successfully!\n\nYou can now start from the beginning.", "OK");
            }
        }

        [MenuItem("Tools/Development/Clear EOS Authentication", priority = 2)]
        public static void ClearEOSAuth()
        {
            if (Application.isPlaying)
            {
                // Clear at runtime
                if (Core.Authentication.AuthenticationManager.Instance != null)
                {
                    Core.Authentication.AuthenticationManager.Instance.Logout();
                    Debug.Log("<color=yellow>[DevelopmentTools] EOS Authentication cleared (runtime)</color>");
                }
                else
                {
                    Debug.LogWarning("[DevelopmentTools] AuthenticationManager not found in scene");
                }
            }
            else
            {
                // Clear in editor mode
                PlayerPrefs.DeleteKey("EOS_LastLoginMethod");
                PlayerPrefs.Save();
                Debug.Log("<color=yellow>[DevelopmentTools] EOS Authentication data cleared (editor mode)</color>");
            }
        }

        [MenuItem("Tools/Development/Clear PlayerPrefs", priority = 3)]
        public static void ClearPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("<color=yellow>[DevelopmentTools] All PlayerPrefs cleared</color>");
        }

        [MenuItem("Tools/Development/Show PlayerPrefs Info", priority = 20)]
        public static void ShowPlayerPrefsInfo()
        {
            string lastLoginMethod = PlayerPrefs.GetString("EOS_LastLoginMethod", "None");
            
            string info = "=== PlayerPrefs Info ===\n\n";
            info += $"Last Login Method: {lastLoginMethod}\n";
            info += $"\nNote: PlayerPrefs are stored at:\n";
            
            #if UNITY_EDITOR_OSX
            info += $"~/Library/Preferences/com.{Application.companyName}.{Application.productName}.plist";
            #elif UNITY_EDITOR_WIN
            info += $"HKEY_CURRENT_USER\\Software\\{Application.companyName}\\{Application.productName}";
            #else
            info += "Platform-specific location";
            #endif
            
            Debug.Log(info);
            EditorUtility.DisplayDialog("PlayerPrefs Info", info, "OK");
        }
    }
}
