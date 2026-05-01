using System;
using System.Collections.Generic;
using KitchenClash.Domain;
using UnityEngine;

namespace KitchenClash.Infrastructure.Audio
{
    [CreateAssetMenu(fileName = "AudioSettings", menuName = "RecipeRage/Audio/Audio Settings")]
    public class AudioSettings : ScriptableObject
    {
        [Header("Music Tracks")]
        [Tooltip("List of all music tracks available in the game")]
        [SerializeField] private List<AudioClip> _musicTracks = new List<AudioClip>();

        [Header("Music Track Mapping")]
        [Tooltip("Maps MusicTrack enum values to audio clips")]
        [SerializeField] private List<MusicTrackEntry> _musicTrackMap = new List<MusicTrackEntry>();

        [Header("SFX Mapping")]
        [Tooltip("Maps SFXType enum values to audio clips")]
        [SerializeField] private List<SFXEntry> _sfxMap = new List<SFXEntry>();

        [Header("UI Sound Effects")]
        [Tooltip("Single sound effect used for all UI interactions (clicks, tabs, etc.)")]
        [SerializeField] private AudioClip _uiSFX;

        private Dictionary<MusicTrack, AudioClip> _musicLookup;
        private Dictionary<SFXType, AudioClip> _sfxLookup;

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

        /// <summary>
        /// Get a music clip by MusicTrack enum.
        /// </summary>
        public AudioClip GetMusicClip(MusicTrack track)
        {
            EnsureMusicLookup();
            return _musicLookup.TryGetValue(track, out var clip) ? clip : null;
        }

        /// <summary>
        /// Get an SFX clip by SFXType enum.
        /// </summary>
        public AudioClip GetSFXClip(SFXType type)
        {
            EnsureSFXLookup();

            // Fallback to UI SFX for button sounds
            if (type == SFXType.ButtonClick || type == SFXType.ButtonHover)
            {
                if (_sfxLookup.TryGetValue(type, out var uiClip))
                    return uiClip;
                return _uiSFX;
            }

            return _sfxLookup.TryGetValue(type, out var clip) ? clip : null;
        }

        private void EnsureMusicLookup()
        {
            if (_musicLookup != null) return;
            _musicLookup = new Dictionary<MusicTrack, AudioClip>();
            foreach (var entry in _musicTrackMap)
            {
                if (entry.Clip != null)
                    _musicLookup[entry.Track] = entry.Clip;
            }
        }

        private void EnsureSFXLookup()
        {
            if (_sfxLookup != null) return;
            _sfxLookup = new Dictionary<SFXType, AudioClip>();
            foreach (var entry in _sfxMap)
            {
                if (entry.Clip != null)
                    _sfxLookup[entry.Type] = entry.Clip;
            }
        }

        [Serializable]
        public struct MusicTrackEntry
        {
            public MusicTrack Track;
            public AudioClip Clip;
        }

        [Serializable]
        public struct SFXEntry
        {
            public SFXType Type;
            public AudioClip Clip;
        }
    }
}
