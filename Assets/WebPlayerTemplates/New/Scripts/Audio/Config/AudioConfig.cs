using UnityEngine;
using UnityEngine.Audio;

namespace Audio.Config
{
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "RecipeRage/Audio/Config")]
    public class AudioConfig : ScriptableObject
    {
        [Header("Mixer Settings")]
        public AudioMixer mainMixer;
        public AudioMixerPreset mixerPreset;

        [Header("Volume Parameters")]
        public string masterVolumeParam = "MasterVolume";
        public string musicVolumeParam = "MusicVolume";
        public string sfxVolumeParam = "SFXVolume";
        public string voiceVolumeParam = "VoiceVolume";

        [Header("Volume Ranges")]
        public float minVolume = -80f;
        public float maxVolume = 0f;

        [Header("Performance Settings")]
        [Range(8, 32)]
        public int maxSimultaneousSounds = 16;
        [Range(0f, 1f)]
        public float spatialBlend = 0.8f;
        public float minDistance = 1f;
        public float maxDistance = 20f;

        [Header("Mobile Settings")]
        public bool disableAudioInBackground = true;
        public bool useHighQualityMobile = false;
        public int mobileAudioSampleRate = 24000;

        [Header("Voice Chat Settings")]
        public bool enableVoiceChat = false;
        public int voiceSampleRate = 16000;
        public float voiceDetectionThreshold = 0.01f;
    }
}