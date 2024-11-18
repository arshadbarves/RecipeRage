using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Audio.Music
{
    [CreateAssetMenu(fileName = "Playlist", menuName = "RecipeRage/Audio/Playlist")]
    public class PlayList : ScriptableObject
    {

        public enum PlaybackMode
        {
            Sequential,
            Random,
            SingleTrack
        }

        [Header("Playlist Settings")]
        public string playlistName;
        public PlaybackMode playbackMode = PlaybackMode.Sequential;
        public bool autoPlay = true;
        public List<MusicTrack> tracks = new List<MusicTrack>();

        [Header("Transition Settings")]
        public float defaultFadeInDuration = 1f;
        public float defaultFadeOutDuration = 1f;
        public bool crossFadeEnabled = true;
        private readonly List<int> _playHistory = new List<int>();

        private int _currentTrackIndex = -1;

        public void Reset()
        {
            _currentTrackIndex = -1;
            _playHistory.Clear();
        }

    #if UNITY_EDITOR
        private void OnValidate()
        {
            // Ensure all tracks have valid settings
            foreach (MusicTrack track in tracks)
            {
                track.crossFadeDuration = Mathf.Max(0f, track.crossFadeDuration);
                track.delayBeforeNext = Mathf.Max(0f, track.delayBeforeNext);
                track.volume = Mathf.Clamp01(track.volume);
            }
        }
    #endif

        public MusicTrack GetNextTrack()
        {
            if (tracks == null || tracks.Count == 0)
                return null;

            switch (playbackMode)
            {
                case PlaybackMode.Sequential:
                    return GetNextSequentialTrack();
                case PlaybackMode.Random:
                    return GetRandomTrack();
                case PlaybackMode.SingleTrack:
                    return GetSingleTrack();
                default:
                    return null;
            }
        }

        private MusicTrack GetNextSequentialTrack()
        {
            _currentTrackIndex = (_currentTrackIndex + 1) % tracks.Count;
            return tracks[_currentTrackIndex];
        }

        private MusicTrack GetRandomTrack()
        {
            if (tracks.Count == 1)
                return tracks[0];

            int nextIndex;
            do
            {
                nextIndex = Random.Range(0, tracks.Count);
            } while (nextIndex == _currentTrackIndex);

            _currentTrackIndex = nextIndex;

            // Keep track of play history
            _playHistory.Add(_currentTrackIndex);
            if (_playHistory.Count > tracks.Count)
                _playHistory.RemoveAt(0);

            return tracks[_currentTrackIndex];
        }

        private MusicTrack GetSingleTrack()
        {
            if (_currentTrackIndex == -1)
                _currentTrackIndex = 0;
            return tracks[_currentTrackIndex];
        }

        public MusicTrack GetCurrentTrack()
        {
            if (_currentTrackIndex >= 0 && _currentTrackIndex < tracks.Count)
                return tracks[_currentTrackIndex];
            return null;
        }

        public float GetCrossFadeDuration(int currentIndex, int nextIndex)
        {
            if (currentIndex < 0 || currentIndex >= tracks.Count ||
                nextIndex < 0 || nextIndex >= tracks.Count)
                return defaultFadeOutDuration;

            return tracks[currentIndex].crossFadeDuration;
        }

        public bool IsLastTrack()
        {
            return playbackMode == PlaybackMode.Sequential &&
                   _currentTrackIndex == tracks.Count - 1;
        }
        [Serializable]
        public class MusicTrack
        {
            public string trackName;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume = 1f;
            public bool loop = true;
            [Tooltip("Delay before playing next track")]
            public float delayBeforeNext;
            [Tooltip("Duration of fade when transitioning to next track")]
            public float crossFadeDuration = 1f;
        }
    }
}