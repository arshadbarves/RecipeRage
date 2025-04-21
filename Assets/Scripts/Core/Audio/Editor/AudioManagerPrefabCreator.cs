using UnityEngine;
using UnityEditor;
using System.IO;

namespace Core.Audio.Editor
{
    /// <summary>
    /// Editor utility for creating the audio manager prefab.
    /// </summary>
    public static class AudioManagerPrefabCreator
    {
        private const string PREFAB_PATH = "Assets/Prefabs/Managers/AudioManager.prefab";
        private const string PREFAB_DIRECTORY = "Assets/Prefabs/Managers";

        [MenuItem("RecipeRage/Audio/Create Audio Manager Prefab")]
        public static void CreateAudioManagerPrefab()
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
                Debug.Log($"[AudioManagerPrefabCreator] Audio manager prefab already exists at {PREFAB_PATH}");

                // Select the existing prefab
                GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
                Selection.activeObject = existingPrefab;

                return;
            }

            // Create the audio manager GameObject
            GameObject audioManagerObj = new GameObject("AudioManager");

            // Add AudioManager component
            AudioManager audioManager = audioManagerObj.AddComponent<AudioManager>();

            // Create audio source for music
            GameObject musicSourceObj = new GameObject("MusicSource");
            musicSourceObj.transform.SetParent(audioManagerObj.transform);
            AudioSource musicSource = musicSourceObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.spatialBlend = 0f; // 2D sound

            // Create pool container
            GameObject poolContainerObj = new GameObject("AudioSourcePool");
            poolContainerObj.transform.SetParent(audioManagerObj.transform);

            // Set references
            SerializedObject serializedManager = new SerializedObject(audioManager);
            serializedManager.FindProperty("_musicSource").objectReferenceValue = musicSource;
            serializedManager.FindProperty("_poolContainer").objectReferenceValue = poolContainerObj.transform;

            // Try to find and assign the audio mixer
            Object mixer = AssetDatabase.LoadAssetAtPath<Object>("Assets/Audio/RecipeRageMixer.mixer");
            if (mixer != null)
            {
                serializedManager.FindProperty("_audioMixer").objectReferenceValue = mixer;
            }

            serializedManager.ApplyModifiedProperties();

            // Create the prefab
            PrefabUtility.SaveAsPrefabAsset(audioManagerObj, PREFAB_PATH);

            // Destroy the temporary GameObject
            Object.DestroyImmediate(audioManagerObj);

            Debug.Log($"[AudioManagerPrefabCreator] Created audio manager prefab at {PREFAB_PATH}");

            // Select the prefab in the Project window
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
            Selection.activeObject = prefab;
        }
    }
}
