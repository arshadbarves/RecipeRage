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
        [MenuItem("RecipeRage/Create/All Manager Prefabs")]
        public static void CreateGameBootstrapPrefab()
        {
            CreateAllPrefabs();
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

            Debug.Log("GameBootstrap prefab created successfully!");
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
            var directory = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            // Create the prefab
            var prefab = PrefabUtility.SaveAsPrefabAsset(gameObject, path);

            return prefab;
        }
    }
}
