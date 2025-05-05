using Core;
using Core.Audio;
using Core.Characters;
using Core.GameFramework.State;
using Core.GameModes;
using Core.Input;
using Core.Networking;
using Core.SaveSystem;
using Gameplay.Cooking;
using Gameplay.Scoring;
using System.IO;
using UnityEngine;
using UnityEditor;
using UI;

namespace RecipeRage.Editor
{
    /// <summary>
    /// Editor utility for creating the GameBootstrap prefab.
    /// </summary>
    public class GameBootstrapPrefabCreator
    {
        // Paths
        private const string PREFABS_PATH = "Assets/Prefabs";
        private const string MANAGERS_PATH = "Assets/Prefabs/Managers";
        private const string UI_PREFABS_PATH = "Assets/Prefabs/UI";
        private const string SCRIPTABLE_OBJECTS_PATH = "Assets/ScriptableObjects";
        private const string CHARACTER_CLASSES_PATH = "Assets/ScriptableObjects/CharacterClasses";
        private const string GAME_MODES_PATH = "Assets/ScriptableObjects/GameModes";
        private const string PLAYER_PATH = "Assets/Prefabs/Player";
        private const string STATIONS_PATH = "Assets/Prefabs/Stations";

        [MenuItem("RecipeRage/Create/GameBootstrap Prefab")]
        public static void CreateGameBootstrapPrefab()
        {
            // Create the GameBootstrap prefab with all required manager references
            CreateGameBootstrapPrefabInternal();
        }

        [MenuItem("RecipeRage/Create/All Game Assets")]
        public static void CreateAllGameAssets()
        {
            // Create all directories
            CreateAllDirectories();

            // Create all manager prefabs and GameBootstrap prefab
            CreateGameBootstrapPrefabInternal();

            // Create UI prefabs
            CreateAllUIPrefabs();

            // Create scriptable objects
            CreateAllScriptableObjects();

            // Set up UI resources
            SetupUIResources();

            Debug.Log("All game assets created successfully!");
        }

        [MenuItem("RecipeRage/Audio/Create Audio Manager Prefab")]
        public static void CreateAudioManagerPrefab()
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

        [MenuItem("RecipeRage/Save System/Create Save Manager Prefab")]
        public static void CreateSaveManagerPrefab()
        {
            // Create the SaveManager prefab
            var saveManagerObj = new GameObject("SaveManager");
            saveManagerObj.AddComponent<SaveManager>();
            CreatePrefab(saveManagerObj, "Assets/Prefabs/Managers/SaveManager.prefab");

            // Clean up the temporary GameObject
            Object.DestroyImmediate(saveManagerObj);

            Debug.Log("SaveManager prefab created successfully!");
        }

        [MenuItem("RecipeRage/UI/Create Splash Screen Manager Prefab", false, 100)]
        public static void CreateSplashScreenManagerPrefab()
        {
            // Use the dedicated SplashScreenSetupWizard to create the prefab
            Debug.Log("Delegating to specialized SplashScreenManager creator...");
            Core.UI.Editor.SplashScreenSetupWizard.CreateSplashScreenManagerPrefab();
        }

        /// <summary>
        /// Creates the GameBootstrap prefab with references to all required manager prefabs.
        /// This is an internal implementation that creates all necessary manager prefabs first.
        /// </summary>
        private static void CreateGameBootstrapPrefabInternal()
        {
            // Create the GameBootstrap GameObject
            var gameBootstrapObj = new GameObject("GameBootstrap");
            var gameBootstrap = gameBootstrapObj.AddComponent<GameBootstrap>();

            // Create the NetworkManager prefab
            var networkManagerObj = new GameObject("NetworkManager");
            networkManagerObj.AddComponent<NetworkBootstrap>();
            var networkManagerPrefab = CreatePrefab(networkManagerObj, "Assets/Prefabs/Managers/NetworkManager.prefab");

            // Create the GameStateManager prefab
            var gameStateManagerObj = new GameObject("GameStateManager");
            gameStateManagerObj.AddComponent<GameStateManager>();
            var gameStateManagerPrefab = CreatePrefab(gameStateManagerObj, "Assets/Prefabs/Managers/GameStateManager.prefab");

            // Create the UIManager prefab
            var uiManagerObj = new GameObject("UIManager");
            uiManagerObj.AddComponent<UIManager>();
            var uiManagerPrefab = CreatePrefab(uiManagerObj, "Assets/Prefabs/Managers/UIManager.prefab");

            // Create the InputManager prefab
            var inputManagerObj = new GameObject("InputManager");
            inputManagerObj.AddComponent<InputManager>();
            var inputManagerPrefab = CreatePrefab(inputManagerObj, "Assets/Prefabs/Managers/InputManager.prefab");

            // Create the GameModeManager prefab
            var gameModeManagerObj = new GameObject("GameModeManager");
            gameModeManagerObj.AddComponent<GameModeManager>();
            var gameModeManagerPrefab = CreatePrefab(gameModeManagerObj, "Assets/Prefabs/Managers/GameModeManager.prefab");

            // Create the CharacterManager prefab
            var characterManagerObj = new GameObject("CharacterManager");
            characterManagerObj.AddComponent<CharacterManager>();
            var characterManagerPrefab = CreatePrefab(characterManagerObj, "Assets/Prefabs/Managers/CharacterManager.prefab");

            // Create the ScoreManager prefab
            var scoreManagerObj = new GameObject("ScoreManager");
            scoreManagerObj.AddComponent<ScoreManager>();
            var scoreManagerPrefab = CreatePrefab(scoreManagerObj, "Assets/Prefabs/Managers/ScoreManager.prefab");

            // Create the OrderManager prefab
            var orderManagerObj = new GameObject("OrderManager");
            orderManagerObj.AddComponent<OrderManager>();
            var orderManagerPrefab = CreatePrefab(orderManagerObj, "Assets/Prefabs/Managers/OrderManager.prefab");

            // Create the SaveManager prefab
            CreateSaveManagerPrefab();
            var saveManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Managers/SaveManager.prefab");

            // Create the AudioManager prefab
            CreateAudioManagerPrefab();
            var audioManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Managers/AudioManager.prefab");

            // Create the SplashScreenManager prefab
            CreateSplashScreenManagerPrefab();
            var splashScreenManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/SplashScreenManager.prefab");

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
            serializedObject.ApplyModifiedProperties();

            // Create the GameBootstrap prefab
            CreatePrefab(gameBootstrapObj, "Assets/Prefabs/GameBootstrap.prefab");

            // Clean up the temporary GameObjects
            Object.DestroyImmediate(gameBootstrapObj);
            Object.DestroyImmediate(networkManagerObj);
            Object.DestroyImmediate(gameStateManagerObj);
            Object.DestroyImmediate(uiManagerObj);
            Object.DestroyImmediate(inputManagerObj);
            Object.DestroyImmediate(gameModeManagerObj);
            Object.DestroyImmediate(characterManagerObj);
            Object.DestroyImmediate(scoreManagerObj);
            Object.DestroyImmediate(orderManagerObj);

            Debug.Log("GameBootstrap prefab created successfully!");
        }

        /// <summary>
        /// Create all directories needed for the game.
        /// </summary>
        private static void CreateAllDirectories()
        {
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
        /// Create all UI prefabs.
        /// </summary>
        private static void CreateAllUIPrefabs()
        {
            // Call the UIPrefabCreator
            UIPrefabCreator.CreateUIPrefabs();
        }

        /// <summary>
        /// Create all scriptable objects.
        /// </summary>
        private static void CreateAllScriptableObjects()
        {
            // Create character classes
            var characterClassGenerator = new CharacterClassGenerator();
            characterClassGenerator.GenerateCharacterClasses(CHARACTER_CLASSES_PATH);

            // Create game modes
            var gameModeGenerator = new GameModeGenerator();
            gameModeGenerator.GenerateGameModes(GAME_MODES_PATH);

            Debug.Log("All scriptable objects created successfully!");
        }

        /// <summary>
        /// Set up UI resources.
        /// </summary>
        private static void SetupUIResources()
        {
            // Call the UISetupUtility
            UISetupUtility.SetupUIResources();
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
        private static GameObject CreatePrefab(GameObject gameObject, string path)
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
        private static void DeletePrefabIfExists(string path)
        {
            if (File.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
                Debug.Log($"Deleted existing prefab: {path}");
            }
        }
    }
}