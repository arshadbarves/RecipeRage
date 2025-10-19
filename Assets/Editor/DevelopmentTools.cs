using Core.Bootstrap;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// Development tools for testing and debugging
    /// </summary>
    public static class DevelopmentTools
    {
        [MenuItem("RecipeRage/Development/Clear All Saved Data")]
        public static void ClearAllSavedData()
        {
            if (EditorUtility.DisplayDialog(
                "Clear All Saved Data",
                "This will delete all saved data including:\n\n" +
                "• Settings (audio, graphics, etc.)\n" +
                "• Player progress\n" +
                "• Player stats\n" +
                "• Authentication data\n\n" +
                "This action cannot be undone. Continue?",
                "Yes, Clear All Data",
                "Cancel"))
            {
                // Clear PlayerPrefs (used by EOS and other systems)
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                
                // Clear save files if game is running
                if (Application.isPlaying && GameBootstrap.Services?.SaveService != null)
                {
                    GameBootstrap.Services.SaveService.DeleteAllData();
                    Debug.Log("[DevelopmentTools] ✅ All saved data cleared (runtime)");
                }
                else
                {
                    // Clear save files manually when not playing
                    string savePath = Application.persistentDataPath;
                    
                    if (System.IO.Directory.Exists(savePath))
                    {
                        string[] files = System.IO.Directory.GetFiles(savePath, "*.json");
                        foreach (string file in files)
                        {
                            System.IO.File.Delete(file);
                        }
                        Debug.Log($"[DevelopmentTools] ✅ Deleted {files.Length} save files from: {savePath}");
                    }
                }
                
                Debug.Log("[DevelopmentTools] ✅ All saved data cleared successfully!");
                EditorUtility.DisplayDialog(
                    "Success",
                    "All saved data has been cleared.\n\n" +
                    "The game will start fresh on next launch.",
                    "OK");
            }
        }

        [MenuItem("RecipeRage/Development/Clear Authentication Data Only")]
        public static void ClearAuthenticationData()
        {
            if (EditorUtility.DisplayDialog(
                "Clear Authentication Data",
                "This will clear only authentication data:\n\n" +
                "• Last login method\n" +
                "• Saved credentials\n\n" +
                "You will need to login again. Continue?",
                "Yes, Clear Auth Data",
                "Cancel"))
            {
                if (Application.isPlaying && GameBootstrap.Services?.SaveService != null)
                {
                    GameBootstrap.Services.SaveService.UpdateSettings(s => s.LastLoginMethod = "");
                    Debug.Log("[DevelopmentTools] ✅ Authentication data cleared (runtime)");
                }
                else
                {
                    // Clear auth-related PlayerPrefs
                    PlayerPrefs.DeleteKey("LastLoginMethod");
                    PlayerPrefs.Save();
                    Debug.Log("[DevelopmentTools] ✅ Authentication data cleared");
                }
                
                EditorUtility.DisplayDialog(
                    "Success",
                    "Authentication data has been cleared.\n\n" +
                    "You will see the login screen on next launch.",
                    "OK");
            }
        }

        [MenuItem("RecipeRage/Development/Show Save File Location")]
        public static void ShowSaveFileLocation()
        {
            string savePath = Application.persistentDataPath;
            EditorUtility.RevealInFinder(savePath);
            Debug.Log($"[DevelopmentTools] Save files location: {savePath}");
        }

        [MenuItem("RecipeRage/Development/Log Current Save Data")]
        public static void LogCurrentSaveData()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[DevelopmentTools] Game must be playing to log save data");
                return;
            }

            if (GameBootstrap.Services?.SaveService == null)
            {
                Debug.LogWarning("[DevelopmentTools] SaveService not available");
                return;
            }

            var settings = GameBootstrap.Services.SaveService.GetSettings();
            var progress = GameBootstrap.Services.SaveService.GetPlayerProgress();
            var stats = GameBootstrap.Services.SaveService.GetPlayerStats();

            Debug.Log("=== CURRENT SAVE DATA ===");
            Debug.Log($"Settings: {JsonUtility.ToJson(settings, true)}");
            Debug.Log($"Progress: {JsonUtility.ToJson(progress, true)}");
            Debug.Log($"Stats: {JsonUtility.ToJson(stats, true)}");
            Debug.Log("========================");
        }

        [MenuItem("RecipeRage/Development/Log Current Save Data", true)]
        private static bool ValidateLogCurrentSaveData()
        {
            return Application.isPlaying;
        }
    }
}
