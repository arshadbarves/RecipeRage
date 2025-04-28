using Core;
using Core.Audio;
using Core.Characters;
using Core.GameFramework.State;
using Core.GameModes;
using Core.Input;
using Core.Networking;
using Core.SaveSystem;
using Core.UI;
using Core.UI.SplashScreen;
using Gameplay.Cooking;
using Gameplay.Scoring;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UI;
using UI.Screens;

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
        [MenuItem("RecipeRage/Create/All Manager Prefabs")]
        public static void CreateGameBootstrapPrefab()
        {
            CreateAllPrefabs();
        }

        [MenuItem("RecipeRage/Create/All Game Assets")]
        public static void CreateAllGameAssets()
        {
            // Create all directories
            CreateAllDirectories();

            // Create all prefabs
            CreateAllPrefabs();

            // Create UI prefabs
            CreateAllUIPrefabs();

            // Create scriptable objects
            CreateAllScriptableObjects();

            // Set up UI resources
            SetupUIResources();

            Debug.Log("All game assets created successfully!");
        }

        [MenuItem("RecipeRage/Create/GameBootstrap Prefab")]
        public static void CreateOnlyGameBootstrapPrefab()
        {
            CreateAllPrefabs();
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

        [MenuItem("RecipeRage/UI/Create Splash Screen Manager Prefab")]
        public static void CreateSplashScreenManagerPrefab()
        {
            // Create the SplashScreenManager prefab
            var splashScreenManagerObj = new GameObject("SplashScreenManager");
            var splashScreenManager = splashScreenManagerObj.AddComponent<SplashScreenManager>();

            // Create UI Documents for splash screens
            var companySplashObj = new GameObject("CompanySplashScreen");
            companySplashObj.transform.SetParent(splashScreenManagerObj.transform);
            var companySplashDocument = companySplashObj.AddComponent<UIDocument>();

            var gameLogoSplashObj = new GameObject("GameLogoSplashScreen");
            gameLogoSplashObj.transform.SetParent(splashScreenManagerObj.transform);
            var gameLogoSplashDocument = gameLogoSplashObj.AddComponent<UIDocument>();

            var loadingScreenObj = new GameObject("LoadingScreen");
            loadingScreenObj.transform.SetParent(splashScreenManagerObj.transform);
            var loadingScreenDocument = loadingScreenObj.AddComponent<UIDocument>();

            // Try to find the UXML assets
            var companySplashUXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/UXML/CompanySplashScreen.uxml");
            if (companySplashUXML != null)
            {
                companySplashDocument.visualTreeAsset = companySplashUXML;
            }

            var gameLogoSplashUXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/UXML/GameLogoSplashScreen.uxml");
            if (gameLogoSplashUXML != null)
            {
                gameLogoSplashDocument.visualTreeAsset = gameLogoSplashUXML;
            }

            var loadingScreenUXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/UXML/LoadingScreen.uxml");
            if (loadingScreenUXML != null)
            {
                loadingScreenDocument.visualTreeAsset = loadingScreenUXML;
            }

            // Set references in SplashScreenManager
            var serializedManager = new SerializedObject(splashScreenManager);
            serializedManager.FindProperty("_companySplashDocument").objectReferenceValue = companySplashDocument;
            serializedManager.FindProperty("_gameLogoSplashDocument").objectReferenceValue = gameLogoSplashDocument;
            serializedManager.FindProperty("_loadingScreenDocument").objectReferenceValue = loadingScreenDocument;

            // Set loading tips
            var loadingTipsProperty = serializedManager.FindProperty("_loadingTips");
            loadingTipsProperty.arraySize = 5;
            loadingTipsProperty.GetArrayElementAtIndex(0).stringValue = "Combine ingredients to create special recipes!";
            loadingTipsProperty.GetArrayElementAtIndex(1).stringValue = "Work together with your team to complete orders faster!";
            loadingTipsProperty.GetArrayElementAtIndex(2).stringValue = "Different character classes have unique abilities!";
            loadingTipsProperty.GetArrayElementAtIndex(3).stringValue = "Don't let food burn or you'll lose points!";
            loadingTipsProperty.GetArrayElementAtIndex(4).stringValue = "Complete orders quickly to earn bonus points!";

            serializedManager.ApplyModifiedProperties();

            CreatePrefab(splashScreenManagerObj, "Assets/Prefabs/UI/SplashScreenManager.prefab");

            // Clean up the temporary GameObjects
            Object.DestroyImmediate(splashScreenManagerObj);

            Debug.Log("SplashScreenManager prefab created successfully!");
        }

        private static void CreateAllPrefabs()
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
            var saveManagerObj = new GameObject("SaveManager");
            saveManagerObj.AddComponent<SaveManager>();
            var saveManagerPrefab = CreatePrefab(saveManagerObj, "Assets/Prefabs/Managers/SaveManager.prefab");

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

            var audioManagerPrefab = CreatePrefab(audioManagerObj, "Assets/Prefabs/Managers/AudioManager.prefab");

            // Create the SplashScreenManager prefab
            var splashScreenManagerObj = new GameObject("SplashScreenManager");
            var splashScreenManager = splashScreenManagerObj.AddComponent<SplashScreenManager>();

            // Create UI Documents for splash screens
            var companySplashObj = new GameObject("CompanySplashScreen");
            companySplashObj.transform.SetParent(splashScreenManagerObj.transform);
            var companySplashDocument = companySplashObj.AddComponent<UIDocument>();

            var gameLogoSplashObj = new GameObject("GameLogoSplashScreen");
            gameLogoSplashObj.transform.SetParent(splashScreenManagerObj.transform);
            var gameLogoSplashDocument = gameLogoSplashObj.AddComponent<UIDocument>();

            var loadingScreenObj = new GameObject("LoadingScreen");
            loadingScreenObj.transform.SetParent(splashScreenManagerObj.transform);
            var loadingScreenDocument = loadingScreenObj.AddComponent<UIDocument>();

            // Try to find the UXML assets
            var companySplashUXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/UXML/CompanySplashScreen.uxml");
            if (companySplashUXML != null)
            {
                companySplashDocument.visualTreeAsset = companySplashUXML;
            }

            var gameLogoSplashUXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/UXML/GameLogoSplashScreen.uxml");
            if (gameLogoSplashUXML != null)
            {
                gameLogoSplashDocument.visualTreeAsset = gameLogoSplashUXML;
            }

            var loadingScreenUXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/UXML/LoadingScreen.uxml");
            if (loadingScreenUXML != null)
            {
                loadingScreenDocument.visualTreeAsset = loadingScreenUXML;
            }

            // Set references in SplashScreenManager
            var serializedSplashManager = new SerializedObject(splashScreenManager);
            serializedSplashManager.FindProperty("_companySplashDocument").objectReferenceValue = companySplashDocument;
            serializedSplashManager.FindProperty("_gameLogoSplashDocument").objectReferenceValue = gameLogoSplashDocument;
            serializedSplashManager.FindProperty("_loadingScreenDocument").objectReferenceValue = loadingScreenDocument;

            // Set loading tips
            var loadingTipsProperty = serializedSplashManager.FindProperty("_loadingTips");
            loadingTipsProperty.arraySize = 5;
            loadingTipsProperty.GetArrayElementAtIndex(0).stringValue = "Combine ingredients to create special recipes!";
            loadingTipsProperty.GetArrayElementAtIndex(1).stringValue = "Work together with your team to complete orders faster!";
            loadingTipsProperty.GetArrayElementAtIndex(2).stringValue = "Different character classes have unique abilities!";
            loadingTipsProperty.GetArrayElementAtIndex(3).stringValue = "Don't let food burn or you'll lose points!";
            loadingTipsProperty.GetArrayElementAtIndex(4).stringValue = "Complete orders quickly to earn bonus points!";

            serializedSplashManager.ApplyModifiedProperties();

            var splashScreenManagerPrefab = CreatePrefab(splashScreenManagerObj, "Assets/Prefabs/UI/SplashScreenManager.prefab");

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
            Object.DestroyImmediate(saveManagerObj);
            Object.DestroyImmediate(audioManagerObj);
            Object.DestroyImmediate(musicSourceObj);
            Object.DestroyImmediate(poolContainerObj);
            Object.DestroyImmediate(splashScreenManagerObj);
            Object.DestroyImmediate(companySplashObj);
            Object.DestroyImmediate(gameLogoSplashObj);
            Object.DestroyImmediate(loadingScreenObj);

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

            // Create the prefab
            var prefab = PrefabUtility.SaveAsPrefabAsset(gameObject, path);

            return prefab;
        }
    }
}
