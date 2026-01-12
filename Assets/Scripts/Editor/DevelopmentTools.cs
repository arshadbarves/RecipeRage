using Core.Persistence;
using Gameplay.Bootstrap;
using Gameplay.Persistence;
using Gameplay.Economy;
using Core.Session;
using UnityEditor;
using UnityEngine;
using VContainer;

namespace Editor
{
    /// <summary>
    /// Development tools for testing and debugging
    /// </summary>
    public static class DevelopmentTools
    {
        private static GameLifetimeScope GameScope => VContainer.Unity.LifetimeScope.Find<GameLifetimeScope>() as GameLifetimeScope;
        private static ISaveService SaveService => GameScope?.Container?.Resolve<ISaveService>();
        private static SessionManager SessionManager => GameScope?.Container?.Resolve<SessionManager>();

        [MenuItem("RecipeRage/Development/Clear All Saved Data")]
        public static void ClearAllSavedData()
        {
            if (EditorUtility.DisplayDialog(
                "Clear All Saved Data",
                "This will delete all saved data including:\n\n" +
                "• Settings (audio, graphics, etc.)\n" +
                "• Player progress\n" +
                "• Player stats\n" +
                "• Economy data\n\n" +
                "This action cannot be undone. Continue?",
                "Yes, Clear All Data",
                "Cancel"))
            {
                // Clear save files manually
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
                if (Application.isPlaying && SaveService != null)
                {
                    SaveService.UpdateSettings(s => s.LastLoginMethod = "");
                    Debug.Log("[DevelopmentTools] ✅ Authentication data cleared (runtime)");
                }
                else
                {
                    string savePath = Application.persistentDataPath;
                    string settingsFile = System.IO.Path.Combine(savePath, "settings.json");

                    if (System.IO.File.Exists(settingsFile))
                    {
                        System.IO.File.Delete(settingsFile);
                        Debug.Log("[DevelopmentTools] ✅ Settings file deleted");
                    }
                    else
                    {
                        Debug.Log("[DevelopmentTools] No settings file found");
                    }
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

            var saveService = SaveService;
            if (saveService == null)
            {
                Debug.LogWarning("[DevelopmentTools] SaveService not available");
                return;
            }

            var settings = saveService.GetSettings();

            Debug.Log("=== CURRENT SAVE DATA ===");
            Debug.Log($"Settings: {JsonUtility.ToJson(settings, true)}");

            // Get player data from session if active
            if (SessionManager?.IsSessionActive == true)
            {
                var playerDataService = SessionManager.SessionContainer?.Resolve<PlayerDataService>();
                var economyService = SessionManager.SessionContainer?.Resolve<EconomyService>();

                if (playerDataService != null)
                {
                    var progress = playerDataService.GetProgress();
                    var stats = playerDataService.GetStats();
                    Debug.Log($"Progress: {JsonUtility.ToJson(progress, true)}");
                    Debug.Log($"Stats: {JsonUtility.ToJson(stats, true)}");
                }

                if (economyService != null)
                {
                    Debug.Log($"Coins: {economyService.GetBalance(EconomyKeys.CurrencyCoins)}");
                    Debug.Log($"Gems: {economyService.GetBalance(EconomyKeys.CurrencyGems)}");
                }
            }
            else
            {
                Debug.Log("Session not active - player data not available");
            }

            Debug.Log("========================");
        }

        [MenuItem("RecipeRage/Development/Log Current Save Data", true)]
        private static bool ValidateLogCurrentSaveData()
        {
            return Application.isPlaying;
        }
    }
}
