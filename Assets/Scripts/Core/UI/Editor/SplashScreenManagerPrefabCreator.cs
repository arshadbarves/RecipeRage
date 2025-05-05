using UnityEngine;
using UnityEditor;
using System.IO;
using Core.UI.SplashScreen;
using UnityEngine.UIElements;

namespace Core.UI.Editor
{
    /// <summary>
    /// Editor utility for creating the splash screen manager prefab.
    /// </summary>
    public static class SplashScreenManagerPrefabCreator
    {
        private const string PREFAB_PATH = "Assets/Prefabs/UI/SplashScreenManager.prefab";
        private const string PREFAB_DIRECTORY = "Assets/Prefabs/UI";

        /// <summary>
        /// Creates the SplashScreenManager prefab.
        /// This method is called by PrefabCreationManager.
        /// </summary>
        public static void CreateSplashScreenManagerPrefab()
        {
            // Create directory if it doesn't exist
            if (!Directory.Exists(PREFAB_DIRECTORY))
            {
                Directory.CreateDirectory(PREFAB_DIRECTORY);
                AssetDatabase.Refresh();
            }

            // Delete existing prefab if it exists
            if (File.Exists(PREFAB_PATH))
            {
                AssetDatabase.DeleteAsset(PREFAB_PATH);
                Debug.Log($"[SplashScreenManagerPrefabCreator] Deleted existing splash screen manager prefab at {PREFAB_PATH}");
                AssetDatabase.Refresh();
            }

            // Create the splash screen manager GameObject
            GameObject splashScreenManagerObj = new GameObject("SplashScreenManager");

            // Add SplashScreenManager component
            SplashScreenManager splashScreenManager = splashScreenManagerObj.AddComponent<SplashScreenManager>();

            // Create UI Documents for splash screens
            CreateUIDocuments(splashScreenManagerObj, splashScreenManager);

            // Create the prefab
            PrefabUtility.SaveAsPrefabAsset(splashScreenManagerObj, PREFAB_PATH);

            // Destroy the temporary GameObject
            Object.DestroyImmediate(splashScreenManagerObj);

            Debug.Log($"[SplashScreenManagerPrefabCreator] Created splash screen manager prefab at {PREFAB_PATH}");

            // Select the prefab in the Project window
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
            Selection.activeObject = prefab;
        }

        /// <summary>
        /// Create UI Documents for splash screens.
        /// </summary>
        /// <param name="parent">The parent GameObject</param>
        /// <param name="splashScreenManager">The SplashScreenManager component</param>
        private static void CreateUIDocuments(GameObject parent, SplashScreenManager splashScreenManager)
        {
            // Create company splash screen
            GameObject companySplashObj = new GameObject("CompanySplashScreen");
            companySplashObj.transform.SetParent(parent.transform);
            UIDocument companySplashDocument = companySplashObj.AddComponent<UIDocument>();

            // Try to find the UXML asset
            var companySplashUXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/UXML/CompanySplashScreen.uxml");
            if (companySplashUXML != null)
            {
                companySplashDocument.visualTreeAsset = companySplashUXML;
            }
            else
            {
                Debug.LogWarning("[SplashScreenManagerPrefabCreator] Could not find CompanySplashScreen.uxml");
            }

            // Create game logo splash screen
            GameObject gameLogoSplashObj = new GameObject("GameLogoSplashScreen");
            gameLogoSplashObj.transform.SetParent(parent.transform);
            UIDocument gameLogoSplashDocument = gameLogoSplashObj.AddComponent<UIDocument>();

            // Try to find the UXML asset
            var gameLogoSplashUXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/UXML/GameLogoSplashScreen.uxml");
            if (gameLogoSplashUXML != null)
            {
                gameLogoSplashDocument.visualTreeAsset = gameLogoSplashUXML;
            }
            else
            {
                Debug.LogWarning("[SplashScreenManagerPrefabCreator] Could not find GameLogoSplashScreen.uxml");
            }

            // Note: Loading screen is now handled by LoadingScreenManager

            // Set references in SplashScreenManager
            splashScreenManager.SetCompanySplashDocument(companySplashDocument);
            splashScreenManager.SetGameLogoSplashDocument(gameLogoSplashDocument);

            // Note: Loading tips are now handled by LoadingScreenManager
        }
    }
}
