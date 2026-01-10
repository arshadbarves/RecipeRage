using System.Collections.Generic;
using UnityEngine;

namespace Core.Audio
{
    [CreateAssetMenu(fileName = "AudioSettings", menuName = "RecipeRage/Audio/Audio Settings")]
    public class AudioSettings : ScriptableObject
    {
        [Header("Music Tracks")]
        [Tooltip("List of all music tracks available in the game")]
        [SerializeField] private List<AudioClip> _musicTracks = new List<AudioClip>();

        [Header("UI Sound Effects")]
        [Tooltip("Single sound effect used for all UI interactions (clicks, tabs, etc.)")]
        [SerializeField] private AudioClip _uiSFX;

        public AudioClip GetMusicTrack(int index)
        {
            if (index < 0 || index >= _musicTracks.Count)
            {
                Debug.LogWarning($"Music track index {index} is out of range. Total tracks: {_musicTracks.Count}");
                return null;
            }

            return _musicTracks[index];
        }

        public IReadOnlyList<AudioClip> GetAllMusicTracks()
        {
            return _musicTracks.AsReadOnly();
        }

        public int MusicTrackCount => _musicTracks.Count;

        public AudioClip UISFX => _uiSFX;
    }
}
