using System;
using Audio.Music;
using Audio.SFX;
using UnityEngine;
using UnityEngine.Serialization;

namespace Audio.Config
{
    [CreateAssetMenu(fileName = "AudioContainer", menuName = "RecipeRage/Audio/Container")]
    public class AudioContainer : ScriptableObject
    {

        [Header("Configuration")]
        public AudioConfig config;

        [Header("Playlists")]
        public PlaylistContainer[] playlists;

        [Header("Sound Libraries")]
        public SfxLibraryContainer[] sfxLibraries;

        [Header("Default Settings")]
        public float defaultMasterVolume = 1f;
        public float defaultMusicVolume = 0.8f;
        [FormerlySerializedAs("defaultSFXVolume")] public float defaultSfxVolume = 1f;
        public float defaultVoiceVolume = 1f;
        public AudioMixerPreset mixerPreset;
        [Serializable]
        public class PlaylistContainer
        {
            public string key;
            public PlayList playlist;
        }

        [Serializable]
        public class SfxLibraryContainer
        {
            public string key;
            public SfxLibrary library;
        }
    }
}