using System.IO;
using UI;
using UI.Screens;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RecipeRage.Editor.UI
{
    /// <summary>
    /// Utility for creating UI prefabs
    /// </summary>
    public class UIPrefabCreator : EditorWindow
    {
        public static void CreateUIPrefabs()
        {
            // Create directories if they don't exist
            CreateDirectoryIfNotExists("Assets/Prefabs/UI");

            // Create UI prefabs
            CreateSplashScreenPrefab();
            CreateMainMenuPrefab();
            CreateCharacterSelectionPrefab();
            CreateGameModeSelectionPrefab();
            CreateSettingsPrefab();
            CreateUIManagerPrefab();

            // Refresh asset database
            AssetDatabase.Refresh();

            Debug.Log("[UIPrefabCreator] UI prefabs created successfully");
        }

        /// <summary>
        /// Create directory if it doesn't exist
        /// </summary>
        /// <param name="path">Directory path</param>
        private static void CreateDirectoryIfNotExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Debug.Log($"[UIPrefabCreator] Created directory: {path}");
            }
        }

        /// <summary>
        /// Create splash screen prefab
        /// </summary>
        private static void CreateSplashScreenPrefab()
        {
            // Create game object
            GameObject prefab = new GameObject("SplashScreen");

            // Add UI Document component
            UIDocument uiDocument = prefab.AddComponent<UIDocument>();

            // Load UXML and USS assets
            VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/UXML/SplashScreen.uxml");
            StyleSheet uss = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI/USS/SplashScreen.uss");
            StyleSheet commonUss = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI/USS/Common.uss");

            if (uxml != null)
            {
                uiDocument.visualTreeAsset = uxml;
            }
            else
            {
                Debug.LogError("[UIPrefabCreator] Failed to load SplashScreen.uxml");
            }

            if (uss != null)
            {
                uiDocument.rootVisualElement.styleSheets.Add(uss);
            }

            if (commonUss != null)
            {
                uiDocument.rootVisualElement.styleSheets.Add(commonUss);
            }

            // We're using Core.UI.SplashScreen.SplashScreenManager instead of a screen-specific component
            // No need to add a component here

            // Save as prefab
            string prefabPath = "Assets/Prefabs/UI/SplashScreen.prefab";
            PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);

            // Destroy the temporary game object
            Object.DestroyImmediate(prefab);

            Debug.Log($"[UIPrefabCreator] Created prefab: {prefabPath}");
        }

        /// <summary>
        /// Create main menu prefab
        /// </summary>
        private static void CreateMainMenuPrefab()
        {
            // Create game object
            GameObject prefab = new GameObject("MainMenuScreen");

            // Add UI Document component
            UIDocument uiDocument = prefab.AddComponent<UIDocument>();

            // Load UXML and USS assets
            VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/UXML/MainMenuUI.uxml");
            StyleSheet uss = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI/USS/MainMenuUI.uss");
            StyleSheet commonUss = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI/USS/Common.uss");

            if (uxml != null)
            {
                uiDocument.visualTreeAsset = uxml;
            }
            else
            {
                Debug.LogError("[UIPrefabCreator] Failed to load MainMenuUI.uxml");
            }

            if (uss != null)
            {
                uiDocument.rootVisualElement.styleSheets.Add(uss);
            }

            if (commonUss != null)
            {
                uiDocument.rootVisualElement.styleSheets.Add(commonUss);
            }

            // Add MainMenuScreen component
            prefab.AddComponent<MainMenuScreen>();

            // Save as prefab
            string prefabPath = "Assets/Prefabs/UI/MainMenuScreen.prefab";
            PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);

            // Destroy the temporary game object
            Object.DestroyImmediate(prefab);

            Debug.Log($"[UIPrefabCreator] Created prefab: {prefabPath}");
        }

        /// <summary>
        /// Create character selection prefab
        /// </summary>
        private static void CreateCharacterSelectionPrefab()
        {
            // Create game object
            GameObject prefab = new GameObject("CharacterSelectionScreen");

            // Add UI Document component
            UIDocument uiDocument = prefab.AddComponent<UIDocument>();

            // Load UXML and USS assets
            VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/UXML/CharacterSelectionScreen.uxml");
            StyleSheet uss = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI/USS/CharacterSelectionScreen.uss");
            StyleSheet commonUss = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI/USS/Common.uss");

            if (uxml != null)
            {
                uiDocument.visualTreeAsset = uxml;
            }
            else
            {
                Debug.LogError("[UIPrefabCreator] Failed to load CharacterSelectionScreen.uxml");
            }

            if (uss != null)
            {
                uiDocument.rootVisualElement.styleSheets.Add(uss);
            }

            if (commonUss != null)
            {
                uiDocument.rootVisualElement.styleSheets.Add(commonUss);
            }

            // Add CharacterSelectionScreen component
            prefab.AddComponent<CharacterSelectionScreen>();

            // Save as prefab
            string prefabPath = "Assets/Prefabs/UI/CharacterSelectionScreen.prefab";
            PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);

            // Destroy the temporary game object
            Object.DestroyImmediate(prefab);

            Debug.Log($"[UIPrefabCreator] Created prefab: {prefabPath}");
        }

        /// <summary>
        /// Create game mode selection prefab
        /// </summary>
        private static void CreateGameModeSelectionPrefab()
        {
            // Create game object
            GameObject prefab = new GameObject("GameModeSelectionScreen");

            // Add UI Document component
            UIDocument uiDocument = prefab.AddComponent<UIDocument>();

            // Load UXML and USS assets
            VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/UXML/GameModeSelectionScreen.uxml");
            StyleSheet uss = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI/USS/GameModeSelectionScreen.uss");
            StyleSheet commonUss = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI/USS/Common.uss");

            if (uxml != null)
            {
                uiDocument.visualTreeAsset = uxml;
            }
            else
            {
                Debug.LogError("[UIPrefabCreator] Failed to load GameModeSelectionScreen.uxml");
            }

            if (uss != null)
            {
                uiDocument.rootVisualElement.styleSheets.Add(uss);
            }

            if (commonUss != null)
            {
                uiDocument.rootVisualElement.styleSheets.Add(commonUss);
            }

            // Add GameModeSelectionScreen component
            prefab.AddComponent<GameModeSelectionScreen>();

            // Save as prefab
            string prefabPath = "Assets/Prefabs/UI/GameModeSelectionScreen.prefab";
            PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);

            // Destroy the temporary game object
            Object.DestroyImmediate(prefab);

            Debug.Log($"[UIPrefabCreator] Created prefab: {prefabPath}");
        }

        /// <summary>
        /// Create settings prefab
        /// </summary>
        private static void CreateSettingsPrefab()
        {
            // Create game object
            GameObject prefab = new GameObject("SettingsScreen");

            // Add UI Document component
            UIDocument uiDocument = prefab.AddComponent<UIDocument>();

            // Load UXML and USS assets
            VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/UXML/SettingsScreen.uxml");
            StyleSheet uss = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI/USS/SettingsScreen.uss");
            StyleSheet commonUss = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI/USS/Common.uss");

            if (uxml != null)
            {
                uiDocument.visualTreeAsset = uxml;
            }
            else
            {
                Debug.LogError("[UIPrefabCreator] Failed to load SettingsScreen.uxml");
            }

            if (uss != null)
            {
                uiDocument.rootVisualElement.styleSheets.Add(uss);
            }

            if (commonUss != null)
            {
                uiDocument.rootVisualElement.styleSheets.Add(commonUss);
            }

            // Add SettingsScreen component
            prefab.AddComponent<SettingsScreen>();

            // Save as prefab
            string prefabPath = "Assets/Prefabs/UI/SettingsScreen.prefab";
            PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);

            // Destroy the temporary game object
            Object.DestroyImmediate(prefab);

            Debug.Log($"[UIPrefabCreator] Created prefab: {prefabPath}");
        }

        /// <summary>
        /// Create UI manager prefab
        /// </summary>
        private static void CreateUIManagerPrefab()
        {
            // Create game object
            GameObject prefab = new GameObject("UIManager");

            // Add UIInitializer component
            prefab.AddComponent<UIInitializer>();

            // Set up references
            UIInitializer initializer = prefab.GetComponent<UIInitializer>();

            // Load UXML assets
            // Note: SplashScreen is handled by SplashScreenManager, not UIInitializer
            VisualTreeAsset mainMenuScreenUXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/UXML/MainMenuUI.uxml");
            VisualTreeAsset characterSelectionScreenUXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/UXML/CharacterSelectionScreen.uxml");
            VisualTreeAsset gameModeSelectionScreenUXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/UXML/GameModeSelectionScreen.uxml");
            VisualTreeAsset settingsScreenUXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/UXML/SettingsScreen.uxml");

            // Load USS assets
            StyleSheet commonUSS = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI/USS/Common.uss");
            StyleSheet mainMenuScreenUSS = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI/USS/MainMenuUI.uss");
            StyleSheet characterSelectionScreenUSS = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI/USS/CharacterSelectionScreen.uss");
            StyleSheet gameModeSelectionScreenUSS = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI/USS/GameModeSelectionScreen.uss");
            StyleSheet settingsScreenUSS = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI/USS/SettingsScreen.uss");

            // Set references using SerializedObject
            SerializedObject serializedObject = new SerializedObject(initializer);

            // Set UXML references
            serializedObject.FindProperty("_mainMenuScreenUXML").objectReferenceValue = mainMenuScreenUXML;
            serializedObject.FindProperty("_characterSelectionScreenUXML").objectReferenceValue = characterSelectionScreenUXML;
            serializedObject.FindProperty("_gameModeSelectionScreenUXML").objectReferenceValue = gameModeSelectionScreenUXML;
            serializedObject.FindProperty("_settingsScreenUXML").objectReferenceValue = settingsScreenUXML;

            // Set USS references
            serializedObject.FindProperty("_commonUSS").objectReferenceValue = commonUSS;
            serializedObject.FindProperty("_mainMenuScreenUSS").objectReferenceValue = mainMenuScreenUSS;
            serializedObject.FindProperty("_characterSelectionScreenUSS").objectReferenceValue = characterSelectionScreenUSS;
            serializedObject.FindProperty("_gameModeSelectionScreenUSS").objectReferenceValue = gameModeSelectionScreenUSS;
            serializedObject.FindProperty("_settingsScreenUSS").objectReferenceValue = settingsScreenUSS;

            // Apply changes
            serializedObject.ApplyModifiedProperties();

            // Save as prefab
            string prefabPath = "Assets/Prefabs/UI/UIManager.prefab";
            PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);

            // Destroy the temporary game object
            Object.DestroyImmediate(prefab);

            Debug.Log($"[UIPrefabCreator] Created prefab: {prefabPath}");
        }
    }
}
