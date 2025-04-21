using UnityEngine;
using UnityEditor;
using System.IO;

namespace Core.Audio.Editor
{
    /// <summary>
    /// Editor utility for creating the audio mixer asset.
    /// </summary>
    public static class AudioMixerCreator
    {
        private const string MIXER_PATH = "Assets/Audio/RecipeRageMixer.mixer";
        private const string MIXER_DIRECTORY = "Assets/Audio";

        [MenuItem("RecipeRage/Audio/Create Audio Mixer")]
        public static void CreateAudioMixer()
        {
            // Create directory if it doesn't exist
            if (!Directory.Exists(MIXER_DIRECTORY))
            {
                Directory.CreateDirectory(MIXER_DIRECTORY);
                AssetDatabase.Refresh();
            }

            // Check if mixer already exists
            if (File.Exists(MIXER_PATH))
            {
                Debug.Log($"[AudioMixerCreator] Audio mixer already exists at {MIXER_PATH}");
                return;
            }

            // We can't programmatically create an AudioMixer directly
            // So we'll create a dummy asset and instruct the user to replace it

            // Create a dummy asset
            var dummyAsset = new TextAsset("This is a placeholder for an AudioMixer asset. Please create a proper AudioMixer asset here.");
            AssetDatabase.CreateAsset(dummyAsset, MIXER_PATH);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // We can't programmatically create groups, so we'll need to instruct the user
            Debug.Log("[AudioMixerCreator] Please manually add the following groups to the mixer:\n" +
                      "1. Music (child of Master)\n" +
                      "2. SFX (child of Master)\n" +
                      "3. Voice (child of Master)\n" +
                      "4. UI (child of SFX)");

            // Instruct the user to set up volume parameters
            Debug.Log("[AudioMixerCreator] Please add the following parameters to the mixer:\n" +
                      "- MasterVolume\n" +
                      "- MusicVolume\n" +
                      "- SFXVolume\n" +
                      "- VoiceVolume");

            // Instruct the user to create a proper AudioMixer
            Debug.Log("[AudioMixerCreator] Please create a proper AudioMixer asset at " + MIXER_PATH);

            Debug.Log($"[AudioMixerCreator] Created placeholder at {MIXER_PATH}");

            // Select the created asset in the Project window
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(MIXER_PATH);
        }
    }
}
