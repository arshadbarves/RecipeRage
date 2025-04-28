using UnityEngine;
using UnityEditor;
using System.IO;
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
        
        [MenuItem("RecipeRage/UI/Create Splash Screen Manager Prefab")]
        public static void CreateSplashScreenManagerPrefab()
        {
            // Create directory if it doesn't exist
            if (!Directory.Exists(PREFAB_DIRECTORY))
            {
                Directory.CreateDirectory(PREFAB_DIRECTORY);
                AssetDatabase.Refresh();
            }
            
            // Check if prefab already exists
            if (File.Exists(PREFAB_PATH))
            {
                Debug.Log($"[SplashScreenManagerPrefabCreator] Splash screen manager prefab already exists at {PREFAB_PATH}");
                
                // Select the existing prefab
                GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
                Selection.activeObject = existingPrefab;
                
                return;
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
            
            // Create loading screen
            GameObject loadingScreenObj = new GameObject("LoadingScreen");
            loadingScreenObj.transform.SetParent(parent.transform);
            UIDocument loadingScreenDocument = loadingScreenObj.AddComponent<UIDocument>();
            
            // Try to find the UXML asset
            var loadingScreenUXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/UXML/LoadingScreen.uxml");
            if (loadingScreenUXML != null)
            {
                loadingScreenDocument.visualTreeAsset = loadingScreenUXML;
            }
            else
            {
                Debug.LogWarning("[SplashScreenManagerPrefabCreator] Could not find LoadingScreen.uxml");
            }
            
            // Set references in SplashScreenManager
            SerializedObject serializedManager = new SerializedObject(splashScreenManager);
            serializedManager.FindProperty("_companySplashDocument").objectReferenceValue = companySplashDocument;
            serializedManager.FindProperty("_gameLogoSplashDocument").objectReferenceValue = gameLogoSplashDocument;
            serializedManager.FindProperty("_loadingScreenDocument").objectReferenceValue = loadingScreenDocument;
            
            // Set loading tips
            SerializedProperty loadingTipsProperty = serializedManager.FindProperty("_loadingTips");
            loadingTipsProperty.arraySize = 5;
            loadingTipsProperty.GetArrayElementAtIndex(0).stringValue = "Combine ingredients to create special recipes!";
            loadingTipsProperty.GetArrayElementAtIndex(1).stringValue = "Work together with your team to complete orders faster!";
            loadingTipsProperty.GetArrayElementAtIndex(2).stringValue = "Different character classes have unique abilities!";
            loadingTipsProperty.GetArrayElementAtIndex(3).stringValue = "Don't let food burn or you'll lose points!";
            loadingTipsProperty.GetArrayElementAtIndex(4).stringValue = "Complete orders quickly to earn bonus points!";
            
            serializedManager.ApplyModifiedProperties();
        }
    }
}
