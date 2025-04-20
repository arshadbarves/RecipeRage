using UnityEngine;
using UnityEditor;
using UnityEngine.Audio;
using System.IO;

namespace Core.AudioSystem.Editor
{
    /// <summary>
    /// Editor utility for creating the AudioManager prefab.
    /// </summary>
    public static class AudioManagerPrefabCreator
    {
        private const string PrefabPath = "Assets/Prefabs/Audio/AudioManager.prefab";
        private const string MixerPath = "Assets/Audio/Mixers/AudioMixer.mixer";
        private const string LibraryPath = "Assets/Audio/AudioClipLibrary.asset";
        
        [MenuItem("RecipeRage/Audio/Create AudioManager Prefab")]
        public static void CreateAudioManagerPrefab()
        {
            // Create directories if they don't exist
            Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));
            Directory.CreateDirectory(Path.GetDirectoryName(MixerPath));
            Directory.CreateDirectory(Path.GetDirectoryName(LibraryPath));
            
            // Create audio mixer if it doesn't exist
            AudioMixer audioMixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
            if (audioMixer == null)
            {
                audioMixer = CreateAudioMixer();
            }
            
            // Create audio clip library if it doesn't exist
            AudioClipLibrary audioClipLibrary = AssetDatabase.LoadAssetAtPath<AudioClipLibrary>(LibraryPath);
            if (audioClipLibrary == null)
            {
                audioClipLibrary = CreateAudioClipLibrary();
            }
            
            // Create AudioManager GameObject
            GameObject audioManagerObject = new GameObject("AudioManager");
            
            // Add AudioManager component
            AudioManager audioManager = audioManagerObject.AddComponent<AudioManager>();
            
            // Create music source
            GameObject musicSourceObject = new GameObject("Music Source");
            musicSourceObject.transform.SetParent(audioManagerObject.transform);
            AudioSource musicSource = musicSourceObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            
            // Create ambience source
            GameObject ambienceSourceObject = new GameObject("Ambience Source");
            ambienceSourceObject.transform.SetParent(audioManagerObject.transform);
            AudioSource ambienceSource = ambienceSourceObject.AddComponent<AudioSource>();
            ambienceSource.playOnAwake = false;
            ambienceSource.loop = true;
            
            // Set up AudioManager properties
            SerializedObject serializedObject = new SerializedObject(audioManager);
            serializedObject.FindProperty("_audioMixer").objectReferenceValue = audioMixer;
            serializedObject.FindProperty("_musicSource").objectReferenceValue = musicSource;
            serializedObject.FindProperty("_ambienceSource").objectReferenceValue = ambienceSource;
            serializedObject.FindProperty("_audioClipLibrary").objectReferenceValue = audioClipLibrary;
            serializedObject.ApplyModifiedProperties();
            
            // Set mixer groups if available
            if (audioMixer != null)
            {
                AudioMixerGroup[] musicGroups = audioMixer.FindMatchingGroups("Music");
                if (musicGroups.Length > 0)
                {
                    musicSource.outputAudioMixerGroup = musicGroups[0];
                }
                
                AudioMixerGroup[] ambienceGroups = audioMixer.FindMatchingGroups("Ambience");
                if (ambienceGroups.Length > 0)
                {
                    ambienceSource.outputAudioMixerGroup = ambienceGroups[0];
                }
            }
            
            // Create prefab
            bool success = false;
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            
            if (existingPrefab != null)
            {
                // Replace existing prefab
                success = PrefabUtility.SaveAsPrefabAsset(audioManagerObject, PrefabPath);
            }
            else
            {
                // Create new prefab
                success = PrefabUtility.SaveAsPrefabAsset(audioManagerObject, PrefabPath);
            }
            
            // Clean up
            Object.DestroyImmediate(audioManagerObject);
            
            if (success)
            {
                Debug.Log($"AudioManager prefab created at {PrefabPath}");
                
                // Select the prefab in the Project window
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            }
            else
            {
                Debug.LogError("Failed to create AudioManager prefab");
            }
        }
        
        /// <summary>
        /// Create an audio mixer asset.
        /// </summary>
        /// <returns>The created audio mixer</returns>
        private static AudioMixer CreateAudioMixer()
        {
            // Create audio mixer
            AudioMixer audioMixer = AudioMixer.Create("AudioMixer", 4);
            
            // Save asset
            AssetDatabase.CreateAsset(audioMixer, MixerPath);
            AssetDatabase.SaveAssets();
            
            // Get the main group
            AudioMixerGroup masterGroup = audioMixer.FindMatchingGroups("Master")[0];
            
            // Create groups
            AudioMixerGroup musicGroup = audioMixer.CreateGroup("Music");
            AudioMixerGroup sfxGroup = audioMixer.CreateGroup("SFX");
            AudioMixerGroup ambienceGroup = audioMixer.CreateGroup("Ambience");
            AudioMixerGroup uiGroup = audioMixer.CreateGroup("UI");
            
            // Set parent groups
            musicGroup.audioMixer.SetGroupParent(musicGroup, masterGroup);
            sfxGroup.audioMixer.SetGroupParent(sfxGroup, masterGroup);
            ambienceGroup.audioMixer.SetGroupParent(ambienceGroup, masterGroup);
            uiGroup.audioMixer.SetGroupParent(uiGroup, masterGroup);
            
            // Create exposed parameters
            audioMixer.SetFloat("MasterVolume", 0f);
            audioMixer.SetFloat("MusicVolume", 0f);
            audioMixer.SetFloat("SFXVolume", 0f);
            audioMixer.SetFloat("AmbienceVolume", 0f);
            audioMixer.SetFloat("UIVolume", 0f);
            
            // Save changes
            EditorUtility.SetDirty(audioMixer);
            AssetDatabase.SaveAssets();
            
            return audioMixer;
        }
        
        /// <summary>
        /// Create an audio clip library asset.
        /// </summary>
        /// <returns>The created audio clip library</returns>
        private static AudioClipLibrary CreateAudioClipLibrary()
        {
            // Create audio clip library
            AudioClipLibrary audioClipLibrary = ScriptableObject.CreateInstance<AudioClipLibrary>();
            
            // Save asset
            AssetDatabase.CreateAsset(audioClipLibrary, LibraryPath);
            AssetDatabase.SaveAssets();
            
            return audioClipLibrary;
        }
    }
}
