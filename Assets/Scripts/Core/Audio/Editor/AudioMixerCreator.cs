using UnityEngine;
using UnityEditor;
using UnityEngine.Audio;
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
            
            // Create the audio mixer
            AudioMixer mixer = AudioMixerController.CreateAudioMixer();
            
            // Get the master group
            AudioMixerGroup masterGroup = mixer.FindMatchingGroups("Master")[0];
            
            // Create groups
            AudioMixerGroup musicGroup = mixer.CreateGroup("Music");
            AudioMixerGroup sfxGroup = mixer.CreateGroup("SFX");
            AudioMixerGroup voiceGroup = mixer.CreateGroup("Voice");
            AudioMixerGroup uiGroup = mixer.CreateGroup("UI");
            
            // Set parent groups
            musicGroup.audioMixer.SetGroupParent(musicGroup, masterGroup);
            sfxGroup.audioMixer.SetGroupParent(sfxGroup, masterGroup);
            voiceGroup.audioMixer.SetGroupParent(voiceGroup, masterGroup);
            uiGroup.audioMixer.SetGroupParent(uiGroup, sfxGroup); // UI sounds are a type of SFX
            
            // Create volume parameters
            mixer.SetFloat("MasterVolume", 0f);
            mixer.SetFloat("MusicVolume", 0f);
            mixer.SetFloat("SFXVolume", 0f);
            mixer.SetFloat("VoiceVolume", 0f);
            
            // Save the mixer asset
            AssetDatabase.CreateAsset(mixer, MIXER_PATH);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"[AudioMixerCreator] Created audio mixer at {MIXER_PATH}");
            
            // Select the mixer in the Project window
            Selection.activeObject = mixer;
        }
    }
}
