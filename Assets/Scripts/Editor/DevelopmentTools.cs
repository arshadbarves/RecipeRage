using KitchenClash.Domain;
using KitchenClash.Composition;
using KitchenClash.Application.Services;
using KitchenClash.Infrastructure.DI;
using UnityEditor;
using UnityEngine;
using VContainer;

namespace RecipeRage.Editor
{
    /// <summary>
    /// Development tools for testing and debugging
    /// </summary>
    public static class DevelopmentTools
    {
        private static RootLifetimeScope RootScope => VContainer.Unity.LifetimeScope.Find<RootLifetimeScope>() as RootLifetimeScope;

        [MenuItem("RecipeRage/Development/Clear All Saved Data")]
        public static void ClearAllSavedData()
        {
            if (EditorUtility.DisplayDialog(
                "Clear All Saved Data",
                "This will delete all saved data including:\n\n" +
                "- Settings (audio, graphics, etc.)\n" +
                "- Player progress\n" +
                "- Player stats\n" +
                "- Economy data\n\n" +
                "This action cannot be undone. Continue?",
                "Yes, Clear All Data",
                "Cancel"))
            {
                string savePath = Application.persistentDataPath;

                if (System.IO.Directory.Exists(savePath))
                {
                    string[] files = System.IO.Directory.GetFiles(savePath, "*.json");
                    foreach (string file in files)
                    {
                        System.IO.File.Delete(file);
                    }
                    Debug.Log($"[DevelopmentTools] Deleted {files.Length} save files from: {savePath}");
                }

                Debug.Log("[DevelopmentTools] All saved data cleared successfully!");
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

            Debug.Log("=== CURRENT SAVE DATA ===");
            Debug.Log($"Save path: {Application.persistentDataPath}");
            Debug.Log("========================");
        }

        [MenuItem("RecipeRage/Development/Log Current Save Data", true)]
        private static bool ValidateLogCurrentSaveData()
        {
            return Application.isPlaying;
        }
    }
}
