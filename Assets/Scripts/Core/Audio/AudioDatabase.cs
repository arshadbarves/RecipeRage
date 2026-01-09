using System;
using System.Collections.Generic;
using Core.Core.Logging;
using UnityEngine;

namespace Core.Core.Audio
{
    /// <summary>
    /// Scriptable object that stores references to audio clips.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioDatabase", menuName = "RecipeRage/Audio/Audio Database")]
    public class AudioDatabase : ScriptableObject
    {
        [Header("Music")]
        [SerializeField] private List<AudioClipReference> _musicClips = new List<AudioClipReference>();

        [Header("Sound Effects")]
        [SerializeField] private List<AudioClipReference> _sfxClips = new List<AudioClipReference>();

        [Header("Voice")]
        [SerializeField] private List<AudioClipReference> _voiceClips = new List<AudioClipReference>();

        [Header("UI Sounds")]
        [SerializeField] private List<AudioClipReference> _uiClips = new List<AudioClipReference>();

        // Cached dictionaries for faster lookup
        private Dictionary<string, AudioClip> _musicDict;
        private Dictionary<string, AudioClip> _sfxDict;
        private Dictionary<string, AudioClip> _voiceDict;
        private Dictionary<string, AudioClip> _uiDict;

        /// <summary>
        /// Initialize the dictionaries.
        /// </summary>
        private void OnEnable()
        {
            InitializeDictionaries();
        }

        /// <summary>
        /// Initialize the dictionaries.
        /// </summary>
        private void InitializeDictionaries()
        {
            // Initialize music dictionary
            _musicDict = new Dictionary<string, AudioClip>();
            foreach (AudioClipReference reference in _musicClips)
            {
                if (reference.Clip != null && !string.IsNullOrEmpty(reference.Id))
                {
                    _musicDict[reference.Id] = reference.Clip;
                }
            }

            // Initialize SFX dictionary
            _sfxDict = new Dictionary<string, AudioClip>();
            foreach (AudioClipReference reference in _sfxClips)
            {
                if (reference.Clip != null && !string.IsNullOrEmpty(reference.Id))
                {
                    _sfxDict[reference.Id] = reference.Clip;
                }
            }

            // Initialize voice dictionary
            _voiceDict = new Dictionary<string, AudioClip>();
            foreach (AudioClipReference reference in _voiceClips)
            {
                if (reference.Clip != null && !string.IsNullOrEmpty(reference.Id))
                {
                    _voiceDict[reference.Id] = reference.Clip;
                }
            }

            // Initialize UI dictionary
            _uiDict = new Dictionary<string, AudioClip>();
            foreach (AudioClipReference reference in _uiClips)
            {
                if (reference.Clip != null && !string.IsNullOrEmpty(reference.Id))
                {
                    _uiDict[reference.Id] = reference.Clip;
                }
            }
        }

        /// <summary>
        /// Get a music clip by ID.
        /// </summary>
        /// <param name="id">The ID of the music clip</param>
        /// <returns>The music clip, or null if not found</returns>
        public AudioClip GetMusicClip(string id)
        {
            if (_musicDict == null)
            {
                InitializeDictionaries();
            }

            if (_musicDict.TryGetValue(id, out AudioClip clip))
            {
                return clip;
            }

            GameLogger.LogWarning($"Music clip not found: {id}");
            return null;
        }

        /// <summary>
        /// Get a SFX clip by ID.
        /// </summary>
        /// <param name="id">The ID of the SFX clip</param>
        /// <returns>The SFX clip, or null if not found</returns>
        public AudioClip GetSFXClip(string id)
        {
            if (_sfxDict == null)
            {
                InitializeDictionaries();
            }

            if (_sfxDict.TryGetValue(id, out AudioClip clip))
            {
                return clip;
            }

            GameLogger.LogWarning($"SFX clip not found: {id}");
            return null;
        }

        /// <summary>
        /// Get a voice clip by ID.
        /// </summary>
        /// <param name="id">The ID of the voice clip</param>
        /// <returns>The voice clip, or null if not found</returns>
        public AudioClip GetVoiceClip(string id)
        {
            if (_voiceDict == null)
            {
                InitializeDictionaries();
            }

            if (_voiceDict.TryGetValue(id, out AudioClip clip))
            {
                return clip;
            }

            GameLogger.LogWarning($"Voice clip not found: {id}");
            return null;
        }

        /// <summary>
        /// Get a UI clip by ID.
        /// </summary>
        /// <param name="id">The ID of the UI clip</param>
        /// <returns>The UI clip, or null if not found</returns>
        public AudioClip GetUIClip(string id)
        {
            if (_uiDict == null)
            {
                InitializeDictionaries();
            }

            if (_uiDict.TryGetValue(id, out AudioClip clip))
            {
                return clip;
            }

            GameLogger.LogWarning($"UI clip not found: {id}");
            return null;
        }
    }

    /// <summary>
    /// Reference to an audio clip with an ID.
    /// </summary>
    [Serializable]
    public class AudioClipReference
    {
        [Tooltip("Unique identifier for the audio clip")]
        public string Id;

        [Tooltip("The audio clip")]
        public AudioClip Clip;

        [Tooltip("Description of the audio clip")]
        [TextArea(1, 3)]
        public string Description;
    }
}
