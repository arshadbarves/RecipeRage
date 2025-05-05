using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Core.UI.Loading;

namespace Core.UI.Editor
{
    /// <summary>
    /// Editor utility for creating the LoadingScreenManager prefab.
    /// </summary>
    public static class LoadingScreenManagerPrefabCreator
    {
        private const string PREFABS_DIRECTORY = "Assets/Prefabs/UI";
        private const string UXML_DIRECTORY = "Assets/UI/UXML";
        private const string USS_DIRECTORY = "Assets/UI/USS";

        /// <summary>
        /// Create the LoadingScreenManager prefab.
        /// </summary>
        [MenuItem("RecipeRage/UI/Create LoadingScreenManager Prefab", false, 102)]
        public static void CreateLoadingScreenManagerPrefab()
        {
            // Create directories if they don't exist
            if (!System.IO.Directory.Exists(PREFABS_DIRECTORY))
            {
                System.IO.Directory.CreateDirectory(PREFABS_DIRECTORY);
                AssetDatabase.Refresh();
            }

            // Delete existing prefab if it exists
            string prefabPath = $"{PREFABS_DIRECTORY}/LoadingScreenManager.prefab";
            if (System.IO.File.Exists(prefabPath))
            {
                AssetDatabase.DeleteAsset(prefabPath);
                Debug.Log($"[LoadingScreenManagerPrefabCreator] Deleted existing LoadingScreenManager prefab at {prefabPath}");
                AssetDatabase.Refresh();
            }

            // Create the LoadingScreenManager GameObject
            var loadingScreenManagerObj = new GameObject("LoadingScreenManager");
            var loadingScreenManager = loadingScreenManagerObj.AddComponent<LoadingScreenManager>();

            // Create UI Documents for loading screen
            var loadingScreenObj = new GameObject("LoadingScreen");
            loadingScreenObj.transform.SetParent(loadingScreenManagerObj.transform);
            var loadingScreenDocument = loadingScreenObj.AddComponent<UIDocument>();

            // Try to find the UXML assets
            var loadingScreenUXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{UXML_DIRECTORY}/LoadingScreen.uxml");
            if (loadingScreenUXML != null)
            {
                loadingScreenDocument.visualTreeAsset = loadingScreenUXML;
            }
            else
            {
                Debug.LogWarning($"[LoadingScreenManagerPrefabCreator] Could not find LoadingScreen.uxml at {UXML_DIRECTORY}/LoadingScreen.uxml");
            }

            // Try to find the USS assets
            var loadingScreenUSS = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{USS_DIRECTORY}/LoadingScreen.uss");
            if (loadingScreenUSS != null)
            {
                loadingScreenDocument.rootVisualElement.styleSheets.Add(loadingScreenUSS);
            }
            else
            {
                Debug.LogWarning($"[LoadingScreenManagerPrefabCreator] Could not find LoadingScreen.uss at {USS_DIRECTORY}/LoadingScreen.uss");
            }

            // Set references in LoadingScreenManager
            loadingScreenManager.SetLoadingScreenDocument(loadingScreenDocument);

            // Set loading tips
            loadingScreenManager.SetLoadingTips(new string[]
            {
                "Combine ingredients to create special recipes!",
                "Work together with your team to complete orders faster!",
                "Different character classes have unique abilities!",
                "Don't let food burn or you'll lose points!",
                "Complete orders quickly to earn bonus points!"
            });

            // Save as prefab
            PrefabUtility.SaveAsPrefabAsset(loadingScreenManagerObj, prefabPath);

            // Destroy the temporary game object
            Object.DestroyImmediate(loadingScreenManagerObj);

            Debug.Log($"[LoadingScreenManagerPrefabCreator] Created LoadingScreenManager prefab at {prefabPath}");
        }
    }
}
