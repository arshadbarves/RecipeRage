using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Core;
using RecipeRage.Editor.UI;

namespace RecipeRage.Editor
{
    /// <summary>
    /// Generates and sets up the startup scene for RecipeRage.
    /// </summary>
    public class StartupSceneGenerator
    {
        [MenuItem("RecipeRage/Setup/Generate Startup Scene")]
        public static void GenerateStartupScene()
        {
            Debug.Log("GenerateStartupScene: Starting...");

            try
            {
                // Create a new scene
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                scene.name = "Startup";
                Debug.Log("GenerateStartupScene: Created new scene");

                // Create the camera
                var mainCamera = new GameObject("Main Camera");
                var camera = mainCamera.AddComponent<Camera>();
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
                camera.transform.position = new Vector3(0, 1, -10);
                mainCamera.tag = "MainCamera";
                Debug.Log("GenerateStartupScene: Created camera");

                // Create the GameBootstrap
                var gameBootstrapObj = new GameObject("GameBootstrap");
                var gameBootstrap = gameBootstrapObj.AddComponent<GameBootstrap>();
                Debug.Log("GenerateStartupScene: Created GameBootstrap");

                // Load prefabs
                var splashScreenManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/SplashScreenManager.prefab");
                var loadingScreenManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/LoadingScreenManager.prefab");
                var gameStateManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/GameStateManager.prefab");
                var uiManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UIManager.prefab");
                var inputManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/InputManager.prefab");
                var gameModeManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/GameModeManager.prefab");
                var characterManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/CharacterManager.prefab");
                var scoreManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ScoreManager.prefab");
                var orderManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/OrderManager.prefab");
                var saveManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/SaveManager.prefab");
                var audioManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/AudioManager.prefab");
                var networkManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/NetworkManager.prefab");

                // Set references in GameBootstrap
                var serializedObject = new SerializedObject(gameBootstrap);
                
                // Set prefab references
                if (splashScreenManagerPrefab != null)
                    serializedObject.FindProperty("_splashScreenManagerPrefab").objectReferenceValue = splashScreenManagerPrefab;
                
                if (loadingScreenManagerPrefab != null)
                    serializedObject.FindProperty("_loadingScreenManagerPrefab").objectReferenceValue = loadingScreenManagerPrefab;
                
                if (gameStateManagerPrefab != null)
                    serializedObject.FindProperty("_gameStateManagerPrefab").objectReferenceValue = gameStateManagerPrefab;
                
                if (uiManagerPrefab != null)
                    serializedObject.FindProperty("_uiManagerPrefab").objectReferenceValue = uiManagerPrefab;
                
                if (inputManagerPrefab != null)
                    serializedObject.FindProperty("_inputManagerPrefab").objectReferenceValue = inputManagerPrefab;
                
                if (gameModeManagerPrefab != null)
                    serializedObject.FindProperty("_gameModeManagerPrefab").objectReferenceValue = gameModeManagerPrefab;
                
                if (characterManagerPrefab != null)
                    serializedObject.FindProperty("_characterManagerPrefab").objectReferenceValue = characterManagerPrefab;
                
                if (scoreManagerPrefab != null)
                    serializedObject.FindProperty("_scoreManagerPrefab").objectReferenceValue = scoreManagerPrefab;
                
                if (orderManagerPrefab != null)
                    serializedObject.FindProperty("_orderManagerPrefab").objectReferenceValue = orderManagerPrefab;
                
                if (saveManagerPrefab != null)
                    serializedObject.FindProperty("_saveManagerPrefab").objectReferenceValue = saveManagerPrefab;
                
                if (audioManagerPrefab != null)
                    serializedObject.FindProperty("_audioManagerPrefab").objectReferenceValue = audioManagerPrefab;
                
                if (networkManagerPrefab != null)
                    serializedObject.FindProperty("_networkManagerPrefab").objectReferenceValue = networkManagerPrefab;

                // Set initialization settings
                serializedObject.FindProperty("_showSplashScreens").boolValue = true;
                serializedObject.FindProperty("_initialState").enumValueIndex = (int)GameBootstrap.GameStateType.MainMenu;

                serializedObject.ApplyModifiedProperties();
                Debug.Log("GenerateStartupScene: Set GameBootstrap references");

                // Save the scene
                string scenePath = "Assets/Scenes/Startup.unity";
                EditorSceneManager.SaveScene(scene, scenePath);

                Debug.Log($"Startup scene setup complete. Saved to {scenePath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"GenerateStartupScene: Error - {ex.Message}\n{ex.StackTrace}");
            }
        }

        [MenuItem("RecipeRage/Setup/Create Splash and Loading Screen Prefabs")]
        public static void CreateSplashAndLoadingScreenPrefabs()
        {
            Debug.Log("Creating SplashScreenManager prefab...");
            SplashScreenSetupWizard.CreateSplashScreenManagerPrefab();
            
            Debug.Log("Creating LoadingScreenManager prefab...");
            LoadingScreenManagerPrefabCreator.CreateLoadingScreenManagerPrefab();
            
            Debug.Log("Splash and Loading Screen prefabs created successfully!");
        }

        [MenuItem("RecipeRage/Setup/Setup Complete Startup")]
        public static void SetupCompleteStartup()
        {
            // Create the prefabs first
            CreateSplashAndLoadingScreenPrefabs();
            
            // Then generate the scene
            GenerateStartupScene();
            
            Debug.Log("Complete startup setup finished successfully!");
        }
    }
}
