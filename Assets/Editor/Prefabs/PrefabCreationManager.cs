using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Core.Audio;
using Core.SaveSystem;
using Core.GameModes;
using Core.Input;
using Core.Networking;
using Gameplay.Cooking;
using Gameplay.Scoring;
using Core;
using Core.Characters;
using Core.GameFramework.State;
using UI;
using Unity.Netcode;
using RecipeRage.Editor.UI;

namespace RecipeRage.Editor.Prefabs
{
    /// <summary>
    /// Centralized manager for creating all prefabs in the RecipeRage project.
    /// Provides utilities for creating individual prefabs and all prefabs at once.
    /// </summary>
    public static class PrefabCreationManager
    {
        // Directory paths
        private const string PREFABS_PATH = "Assets/Prefabs";
        private const string MANAGERS_PATH = "Assets/Prefabs/Managers";
        private const string UI_PREFABS_PATH = "Assets/Prefabs/UI";
        private const string PLAYER_PATH = "Assets/Prefabs/Player";
        private const string STATIONS_PATH = "Assets/Prefabs/Stations";
        private const string SCRIPTABLE_OBJECTS_PATH = "Assets/ScriptableObjects";
        private const string CHARACTER_CLASSES_PATH = "Assets/ScriptableObjects/CharacterClasses";
        private const string GAME_MODES_PATH = "Assets/ScriptableObjects/GameModes";

        /// <summary>
        /// Creates all prefabs and assets for the RecipeRage project.
        /// </summary>
        [MenuItem("RecipeRage/Create/All Game Assets (One Click Setup)", false, 0)]
        public static void CreateAllGameAssets()
        {
            Debug.Log("Starting one-click setup for RecipeRage...");

            // Create all directories
            CreateAllDirectories();

            // Create all manager prefabs
            CreateAllManagerPrefabs();

            // Create UI prefabs
            CreateAllUIPrefabs();

            // Create player prefabs
            CreatePlayerPrefab();

            // Create station prefabs
            CreateAllStationPrefabs();

            // Create audio assets
            CreateAudioDatabase();
            CreateAudioMixer();

            // Create scriptable objects
            CreateAllScriptableObjects();

            // Set up UI resources
            SetupUIResources();

            // Set up scenes
            SetupAllScenes();

            Debug.Log("One-click setup completed successfully!");

            // Show a confirmation dialog
            EditorUtility.DisplayDialog(
                "Setup Complete",
                "All RecipeRage game assets have been created successfully!\n\n" +
                "The following assets were created:\n" +
                "- All manager prefabs (GameBootstrap, NetworkManager, etc.)\n" +
                "- UI prefabs (SplashScreenManager, LoadingScreenManager, etc.)\n" +
                "- Scriptable objects (Character Classes, Game Modes)\n" +
                "- Required directories and resources\n\n" +
                "You can now proceed with scene setup.",
                "OK");
        }

        /// <summary>
        /// Creates all manager prefabs including the GameBootstrap prefab.
        /// </summary>
        public static void CreateAllManagerPrefabs()
        {
            Debug.Log("Creating all manager prefabs...");

            // Create individual manager prefabs first
            CreateNetworkManagerPrefab();
            CreateGameStateManagerPrefab();
            CreateUIManagerPrefab();
            CreateInputManagerPrefab();
            CreateGameModeManagerPrefab();
            CreateCharacterManagerPrefab();
            CreateScoreManagerPrefab();
            CreateOrderManagerPrefab();
            CreateSaveManagerPrefab();
            CreateAudioManagerPrefab();

            // Create UI manager prefabs
            CreateSplashScreenManagerPrefab();
            CreateLoadingScreenManagerPrefab();

            // Create the GameBootstrap prefab last (references all other prefabs)
            CreateGameBootstrapPrefab();

            Debug.Log("All manager prefabs created successfully!");
        }

        /// <summary>
        /// Creates the GameBootstrap prefab with references to all other manager prefabs.
        /// </summary>
        private static void CreateGameBootstrapPrefab()
        {
            Debug.Log("Creating GameBootstrap prefab...");

            // Create the GameBootstrap GameObject
            var gameBootstrapObj = new GameObject("GameBootstrap");
            var gameBootstrap = gameBootstrapObj.AddComponent<GameBootstrap>();

            // Load all manager prefabs
            var networkManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Managers/NetworkManager.prefab");
            var gameStateManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Managers/GameStateManager.prefab");
            var uiManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Managers/UIManager.prefab");
            var inputManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Managers/InputManager.prefab");
            var gameModeManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Managers/GameModeManager.prefab");
            var characterManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Managers/CharacterManager.prefab");
            var scoreManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Managers/ScoreManager.prefab");
            var orderManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Managers/OrderManager.prefab");
            var saveManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Managers/SaveManager.prefab");
            var audioManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Managers/AudioManager.prefab");
            var splashScreenManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/SplashScreenManager.prefab");
            var loadingScreenManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/LoadingScreenManager.prefab");

            // Using SerializedObject to modify serialized properties in editor
            var serializedObject = new SerializedObject(gameBootstrap);
            serializedObject.FindProperty("_networkManagerPrefab").objectReferenceValue = networkManagerPrefab;
            serializedObject.FindProperty("_gameStateManagerPrefab").objectReferenceValue = gameStateManagerPrefab;
            serializedObject.FindProperty("_uiManagerPrefab").objectReferenceValue = uiManagerPrefab;
            serializedObject.FindProperty("_inputManagerPrefab").objectReferenceValue = inputManagerPrefab;
            serializedObject.FindProperty("_gameModeManagerPrefab").objectReferenceValue = gameModeManagerPrefab;
            serializedObject.FindProperty("_characterManagerPrefab").objectReferenceValue = characterManagerPrefab;
            serializedObject.FindProperty("_scoreManagerPrefab").objectReferenceValue = scoreManagerPrefab;
            serializedObject.FindProperty("_orderManagerPrefab").objectReferenceValue = orderManagerPrefab;
            serializedObject.FindProperty("_saveManagerPrefab").objectReferenceValue = saveManagerPrefab;
            serializedObject.FindProperty("_audioManagerPrefab").objectReferenceValue = audioManagerPrefab;
            serializedObject.FindProperty("_splashScreenManagerPrefab").objectReferenceValue = splashScreenManagerPrefab;
            serializedObject.FindProperty("_loadingScreenManagerPrefab").objectReferenceValue = loadingScreenManagerPrefab;
            serializedObject.ApplyModifiedProperties();

            // Create the GameBootstrap prefab
            string prefabPath = "Assets/Prefabs/GameBootstrap.prefab";
            CreatePrefab(gameBootstrapObj, prefabPath);

            // Clean up the temporary GameObject
            Object.DestroyImmediate(gameBootstrapObj);

            Debug.Log("GameBootstrap prefab created successfully!");
        }

        /// <summary>
        /// Creates the NetworkManager prefab.
        /// </summary>
        public static void CreateNetworkManagerPrefab()
        {
            var networkManagerObj = new GameObject("NetworkManager");
            networkManagerObj.AddComponent<NetworkBootstrap>();
            CreatePrefab(networkManagerObj, "Assets/Prefabs/Managers/NetworkManager.prefab");
            Object.DestroyImmediate(networkManagerObj);
        }

        /// <summary>
        /// Creates the GameStateManager prefab.
        /// </summary>
        public static void CreateGameStateManagerPrefab()
        {
            var gameStateManagerObj = new GameObject("GameStateManager");
            gameStateManagerObj.AddComponent<GameStateManager>();
            CreatePrefab(gameStateManagerObj, "Assets/Prefabs/Managers/GameStateManager.prefab");
            Object.DestroyImmediate(gameStateManagerObj);
        }

        /// <summary>
        /// Creates the UIManager prefab.
        /// </summary>
        public static void CreateUIManagerPrefab()
        {
            var uiManagerObj = new GameObject("UIManager");
            uiManagerObj.AddComponent<UIManager>();
            CreatePrefab(uiManagerObj, "Assets/Prefabs/Managers/UIManager.prefab");
            Object.DestroyImmediate(uiManagerObj);
        }

        /// <summary>
        /// Creates the InputManager prefab.
        /// </summary>
        public static void CreateInputManagerPrefab()
        {
            var inputManagerObj = new GameObject("InputManager");
            inputManagerObj.AddComponent<InputManager>();
            CreatePrefab(inputManagerObj, "Assets/Prefabs/Managers/InputManager.prefab");
            Object.DestroyImmediate(inputManagerObj);
        }

        /// <summary>
        /// Creates the GameModeManager prefab.
        /// </summary>
        public static void CreateGameModeManagerPrefab()
        {
            var gameModeManagerObj = new GameObject("GameModeManager");
            gameModeManagerObj.AddComponent<GameModeManager>();
            CreatePrefab(gameModeManagerObj, "Assets/Prefabs/Managers/GameModeManager.prefab");
            Object.DestroyImmediate(gameModeManagerObj);
        }

        /// <summary>
        /// Creates the CharacterManager prefab.
        /// </summary>
        public static void CreateCharacterManagerPrefab()
        {
            var characterManagerObj = new GameObject("CharacterManager");
            characterManagerObj.AddComponent<CharacterManager>();
            CreatePrefab(characterManagerObj, "Assets/Prefabs/Managers/CharacterManager.prefab");
            Object.DestroyImmediate(characterManagerObj);
        }

        /// <summary>
        /// Creates the ScoreManager prefab.
        /// </summary>
        public static void CreateScoreManagerPrefab()
        {
            var scoreManagerObj = new GameObject("ScoreManager");
            scoreManagerObj.AddComponent<ScoreManager>();
            CreatePrefab(scoreManagerObj, "Assets/Prefabs/Managers/ScoreManager.prefab");
            Object.DestroyImmediate(scoreManagerObj);
        }

        /// <summary>
        /// Creates the OrderManager prefab.
        /// </summary>
        public static void CreateOrderManagerPrefab()
        {
            var orderManagerObj = new GameObject("OrderManager");
            orderManagerObj.AddComponent<OrderManager>();
            CreatePrefab(orderManagerObj, "Assets/Prefabs/Managers/OrderManager.prefab");
            Object.DestroyImmediate(orderManagerObj);
        }

        /// <summary>
        /// Creates the SaveManager prefab.
        /// </summary>
        private static void CreateSaveManagerPrefab()
        {
            // Create the SaveManager prefab
            var saveManagerObj = new GameObject("SaveManager");
            saveManagerObj.AddComponent<SaveManager>();
            CreatePrefab(saveManagerObj, "Assets/Prefabs/Managers/SaveManager.prefab");

            // Clean up the temporary GameObject
            Object.DestroyImmediate(saveManagerObj);

            Debug.Log("SaveManager prefab created successfully!");
        }

        /// <summary>
        /// Creates the AudioManager prefab.
        /// </summary>
        private static void CreateAudioManagerPrefab()
        {
            // Create the AudioManager prefab
            var audioManagerObj = new GameObject("AudioManager");
            var audioManager = audioManagerObj.AddComponent<AudioManager>();

            // Create audio source for music
            var musicSourceObj = new GameObject("MusicSource");
            musicSourceObj.transform.SetParent(audioManagerObj.transform);
            var musicSource = musicSourceObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.spatialBlend = 0f; // 2D sound

            // Create pool container
            var poolContainerObj = new GameObject("AudioSourcePool");
            poolContainerObj.transform.SetParent(audioManagerObj.transform);

            // Set references for AudioManager
            var serializedAudioManager = new SerializedObject(audioManager);
            serializedAudioManager.FindProperty("_musicSource").objectReferenceValue = musicSource;
            serializedAudioManager.FindProperty("_poolContainer").objectReferenceValue = poolContainerObj.transform;

            // Try to find and assign the audio mixer
            var mixer = AssetDatabase.LoadAssetAtPath<Object>("Assets/Audio/RecipeRageMixer.mixer");
            if (mixer != null)
            {
                serializedAudioManager.FindProperty("_audioMixer").objectReferenceValue = mixer;
            }

            serializedAudioManager.ApplyModifiedProperties();

            CreatePrefab(audioManagerObj, "Assets/Prefabs/Managers/AudioManager.prefab");

            // Clean up the temporary GameObjects
            Object.DestroyImmediate(audioManagerObj);

            Debug.Log("AudioManager prefab created successfully!");
        }

        /// <summary>
        /// Creates the SplashScreenManager prefab.
        /// </summary>
        private static void CreateSplashScreenManagerPrefab()
        {
            // Delegate to the specialized creator
            Debug.Log("Creating SplashScreenManager prefab...");
            SplashScreenManagerPrefabCreator.CreateSplashScreenManagerPrefab();
        }

        /// <summary>
        /// Creates the LoadingScreenManager prefab.
        /// </summary>
        private static void CreateLoadingScreenManagerPrefab()
        {
            // Delegate to the specialized creator
            Debug.Log("Creating LoadingScreenManager prefab...");
            LoadingScreenManagerPrefabCreator.CreateLoadingScreenManagerPrefab();
        }

        /// <summary>
        /// Creates all UI prefabs.
        /// </summary>
        public static void CreateAllUIPrefabs()
        {
            Debug.Log("Creating all UI prefabs...");

            // Create splash screen and loading screen prefabs
            CreateSplashScreenManagerPrefab();
            CreateLoadingScreenManagerPrefab();

            // Call the UIPrefabCreator for other UI prefabs
            UIPrefabCreator.CreateUIPrefabs();

            Debug.Log("All UI prefabs created successfully!");
        }

        /// <summary>
        /// Creates all scriptable objects.
        /// </summary>
        public static void CreateAllScriptableObjects()
        {
            Debug.Log("Creating all scriptable objects...");

            // Create character classes
            var characterClassGenerator = new CharacterClassGenerator();
            characterClassGenerator.GenerateCharacterClasses(CHARACTER_CLASSES_PATH);

            // Create game modes
            var gameModeGenerator = new GameModeGenerator();
            gameModeGenerator.GenerateGameModes(GAME_MODES_PATH);

            Debug.Log("All scriptable objects created successfully!");
        }

        /// <summary>
        /// Sets up UI resources.
        /// </summary>
        private static void SetupUIResources()
        {
            Debug.Log("Setting up UI resources...");

            // Call the UISetupUtility
            UISetupUtility.SetupUIResources();

            Debug.Log("UI resources set up successfully!");
        }

        /// <summary>
        /// Creates all directories needed for the game.
        /// </summary>
        private static void CreateAllDirectories()
        {
            Debug.Log("Creating all directories...");

            // Create prefab directories
            CreateDirectoryIfNotExists(PREFABS_PATH);
            CreateDirectoryIfNotExists(MANAGERS_PATH);
            CreateDirectoryIfNotExists(UI_PREFABS_PATH);
            CreateDirectoryIfNotExists(PLAYER_PATH);
            CreateDirectoryIfNotExists(STATIONS_PATH);

            // Create scriptable object directories
            CreateDirectoryIfNotExists(SCRIPTABLE_OBJECTS_PATH);
            CreateDirectoryIfNotExists(CHARACTER_CLASSES_PATH);
            CreateDirectoryIfNotExists(GAME_MODES_PATH);

            // Create UI resource directories
            CreateDirectoryIfNotExists("Assets/Resources/UI/Images");

            Debug.Log("All directories created successfully!");
        }

        /// <summary>
        /// Create directory if it doesn't exist.
        /// </summary>
        /// <param name="path">Directory path</param>
        private static void CreateDirectoryIfNotExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Debug.Log($"Created directory: {path}");
            }
        }

        /// <summary>
        /// Create a prefab from a GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to create a prefab from</param>
        /// <param name="path">The path to save the prefab to</param>
        /// <returns>The created prefab</returns>
        public static GameObject CreatePrefab(GameObject gameObject, string path)
        {
            // Create the directory if it doesn't exist
            var directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Delete the existing prefab if it exists
            DeletePrefabIfExists(path);

            // Create the prefab
            var prefab = PrefabUtility.SaveAsPrefabAsset(gameObject, path);
            Debug.Log($"Created prefab: {path}");

            return prefab;
        }

        /// <summary>
        /// Deletes a prefab if it exists at the specified path.
        /// </summary>
        /// <param name="path">The path to the prefab to delete</param>
        public static void DeletePrefabIfExists(string path)
        {
            if (File.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
                Debug.Log($"Deleted existing prefab: {path}");
            }
        }

        /// <summary>
        /// Creates the Player prefab.
        /// </summary>
        public static void CreatePlayerPrefab()
        {
            Debug.Log("Creating Player prefab...");

            // Create player object
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";

            // Add required components
            PlayerController playerController = player.AddComponent<PlayerController>();
            player.AddComponent<NetworkObject>();

            // Add rigidbody
            Rigidbody rigidbody = player.GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = player.AddComponent<Rigidbody>();
            }
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // Create model object
            GameObject model = GameObject.CreatePrimitive(PrimitiveType.Cube);
            model.name = "Model";
            model.transform.SetParent(player.transform);
            model.transform.localPosition = new Vector3(0, 0, 0.3f);
            model.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            // Create item hold point
            GameObject holdPoint = new GameObject("ItemHoldPoint");
            holdPoint.transform.SetParent(player.transform);
            holdPoint.transform.localPosition = new Vector3(0, 0.5f, 0.8f);

            // Set references
            SerializedObject serializedObject = new SerializedObject(playerController);

            // Try to set the model transform reference if it exists
            var modelTransformProperty = serializedObject.FindProperty("_modelTransform");
            if (modelTransformProperty != null)
            {
                modelTransformProperty.objectReferenceValue = model.transform;
                Debug.Log("Set player model transform reference successfully");
            }

            // Set the hold point reference (the property is named "_holdPoint" in PlayerController)
            var holdPointProperty = serializedObject.FindProperty("_holdPoint");
            if (holdPointProperty != null)
            {
                holdPointProperty.objectReferenceValue = holdPoint.transform;
                Debug.Log("Set player hold point reference successfully");
            }
            else
            {
                Debug.LogWarning("Could not find _holdPoint property in PlayerController");
            }

            // Apply all property modifications
            serializedObject.ApplyModifiedProperties();

            // Save the prefab
            string playerPrefabPath = "Assets/Prefabs/Player/Player.prefab";
            CreatePrefab(player, playerPrefabPath);

            // Clean up the temporary GameObject
            Object.DestroyImmediate(player);

            Debug.Log("Player prefab created successfully!");
        }

        /// <summary>
        /// Creates all station prefabs.
        /// </summary>
        public static void CreateAllStationPrefabs()
        {
            Debug.Log("Creating station prefabs...");

            // Create a StationGenerator instance and generate stations
            var stationGenerator = new StationGenerator();
            stationGenerator.GenerateStations(STATIONS_PATH);

            Debug.Log("Station prefabs created successfully!");
        }

        /// <summary>
        /// Creates the audio database asset.
        /// </summary>
        private static void CreateAudioDatabase()
        {
            Debug.Log("Creating audio database...");

            // Delegate to the specialized creator
            Core.Audio.Editor.AudioDatabaseCreator.CreateAudioDatabase();

            Debug.Log("Audio database created successfully!");
        }

        /// <summary>
        /// Creates the audio mixer asset.
        /// </summary>
        private static void CreateAudioMixer()
        {
            Debug.Log("Creating audio mixer...");

            // Delegate to the specialized creator
            Core.Audio.Editor.AudioMixerCreator.CreateAudioMixer();

            Debug.Log("Audio mixer created successfully!");
        }

        /// <summary>
        /// Sets up all scenes for the game and adds them to build settings.
        /// </summary>
        public static void SetupAllScenes()
        {
            Debug.Log("Setting up scenes...");

            // Create startup scene
            StartupSceneGenerator.GenerateStartupScene();

            // Create main menu scene
            SceneSetupGenerator.GenerateMainMenuScene();

            // Create game scene
            SceneSetupGenerator.GenerateGameScene();

            // Add scenes to build settings
            AddScenesToBuildSettings();

            Debug.Log("Scenes set up and added to build settings successfully!");
        }

        /// <summary>
        /// Adds all scenes to the build settings in the correct order.
        /// </summary>
        private static void AddScenesToBuildSettings()
        {
            Debug.Log("Adding scenes to build settings...");

            try
            {
                // Define the scenes to add in the correct order
                string[] scenePaths = new string[]
                {
                    "Assets/Scenes/Startup.unity",
                    "Assets/Scenes/MainMenu.unity",
                    "Assets/Scenes/Game.unity"
                };

                // Create new scene list
                var sceneList = new List<EditorBuildSettingsScene>();

                // Add each scene
                foreach (string scenePath in scenePaths)
                {
                    if (File.Exists(scenePath))
                    {
                        sceneList.Add(new EditorBuildSettingsScene(scenePath, true));
                        Debug.Log($"Added scene to build settings: {scenePath}");
                    }
                    else
                    {
                        Debug.LogWarning($"Scene not found: {scenePath}");
                    }
                }

                // Set the build settings
                EditorBuildSettings.scenes = sceneList.ToArray();

                Debug.Log("Scenes added to build settings successfully!");
                EditorUtility.DisplayDialog("Build Settings Updated",
                    "The following scenes have been added to the build settings in order:\n\n" +
                    "1. Startup.unity\n" +
                    "2. MainMenu.unity\n" +
                    "3. Game.unity", "OK");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error adding scenes to build settings: {ex.Message}\n{ex.StackTrace}");
                EditorUtility.DisplayDialog("Build Settings Error",
                    $"An error occurred while adding scenes to build settings:\n\n{ex.Message}", "OK");
            }
        }
    }
}
