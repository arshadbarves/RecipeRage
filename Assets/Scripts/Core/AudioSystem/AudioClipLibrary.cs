using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.AudioSystem
{
    /// <summary>
    /// Stores and manages audio clips for the game.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioClipLibrary", menuName = "RecipeRage/Audio/Audio Clip Library")]
    public class AudioClipLibrary : ScriptableObject
    {
        /// <summary>
        /// Categories of audio clips.
        /// </summary>
        public enum AudioCategory
        {
            Music,
            Ambience,
            UI,
            Player,
            Cooking,
            Items,
            Environment
        }
        
        /// <summary>
        /// A single audio clip entry.
        /// </summary>
        [Serializable]
        public class AudioClipEntry
        {
            /// <summary>
            /// The name/ID of the audio clip.
            /// </summary>
            public string Name;
            
            /// <summary>
            /// The audio clip.
            /// </summary>
            public AudioClip Clip;
            
            /// <summary>
            /// The category of the audio clip.
            /// </summary>
            public AudioCategory Category;
            
            /// <summary>
            /// The default volume of the audio clip (0-1).
            /// </summary>
            [Range(0f, 1f)]
            public float Volume = 1f;
            
            /// <summary>
            /// The default pitch of the audio clip (0.5-1.5).
            /// </summary>
            [Range(0.5f, 1.5f)]
            public float Pitch = 1f;
            
            /// <summary>
            /// Whether the audio clip should loop by default.
            /// </summary>
            public bool Loop = false;
        }
        
        /// <summary>
        /// List of music clips.
        /// </summary>
        [Header("Music")]
        public List<AudioClipEntry> MusicClips = new List<AudioClipEntry>();
        
        /// <summary>
        /// List of ambience clips.
        /// </summary>
        [Header("Ambience")]
        public List<AudioClipEntry> AmbienceClips = new List<AudioClipEntry>();
        
        /// <summary>
        /// List of UI sound clips.
        /// </summary>
        [Header("UI")]
        public List<AudioClipEntry> UIClips = new List<AudioClipEntry>();
        
        /// <summary>
        /// List of player sound clips.
        /// </summary>
        [Header("Player")]
        public List<AudioClipEntry> PlayerClips = new List<AudioClipEntry>();
        
        /// <summary>
        /// List of cooking sound clips.
        /// </summary>
        [Header("Cooking")]
        public List<AudioClipEntry> CookingClips = new List<AudioClipEntry>();
        
        /// <summary>
        /// List of item sound clips.
        /// </summary>
        [Header("Items")]
        public List<AudioClipEntry> ItemClips = new List<AudioClipEntry>();
        
        /// <summary>
        /// List of environment sound clips.
        /// </summary>
        [Header("Environment")]
        public List<AudioClipEntry> EnvironmentClips = new List<AudioClipEntry>();
        
        // Dictionary for quick lookup by name
        private Dictionary<string, AudioClipEntry> _clipDictionary;
        
        /// <summary>
        /// Initialize the clip dictionary.
        /// </summary>
        public void Initialize()
        {
            _clipDictionary = new Dictionary<string, AudioClipEntry>();
            
            // Add all clips to the dictionary
            AddClipsToDictionary(MusicClips);
            AddClipsToDictionary(AmbienceClips);
            AddClipsToDictionary(UIClips);
            AddClipsToDictionary(PlayerClips);
            AddClipsToDictionary(CookingClips);
            AddClipsToDictionary(ItemClips);
            AddClipsToDictionary(EnvironmentClips);
        }
        
        /// <summary>
        /// Add clips to the dictionary.
        /// </summary>
        /// <param name="clips">The clips to add</param>
        private void AddClipsToDictionary(List<AudioClipEntry> clips)
        {
            foreach (AudioClipEntry entry in clips)
            {
                if (entry.Clip != null && !string.IsNullOrEmpty(entry.Name))
                {
                    _clipDictionary[entry.Name] = entry;
                }
            }
        }
        
        /// <summary>
        /// Get an audio clip entry by name.
        /// </summary>
        /// <param name="name">The name of the clip</param>
        /// <returns>The audio clip entry, or null if not found</returns>
        public AudioClipEntry GetClipEntry(string name)
        {
            // Initialize if needed
            if (_clipDictionary == null)
            {
                Initialize();
            }
            
            // Try to get the clip from the dictionary
            if (_clipDictionary.TryGetValue(name, out AudioClipEntry entry))
            {
                return entry;
            }
            
            return null;
        }
        
        /// <summary>
        /// Get an audio clip by name.
        /// </summary>
        /// <param name="name">The name of the clip</param>
        /// <returns>The audio clip, or null if not found</returns>
        public AudioClip GetClip(string name)
        {
            AudioClipEntry entry = GetClipEntry(name);
            return entry?.Clip;
        }
        
        /// <summary>
        /// Get all clips in a category.
        /// </summary>
        /// <param name="category">The category</param>
        /// <returns>A list of audio clip entries in the category</returns>
        public List<AudioClipEntry> GetClipsInCategory(AudioCategory category)
        {
            switch (category)
            {
                case AudioCategory.Music:
                    return MusicClips;
                case AudioCategory.Ambience:
                    return AmbienceClips;
                case AudioCategory.UI:
                    return UIClips;
                case AudioCategory.Player:
                    return PlayerClips;
                case AudioCategory.Cooking:
                    return CookingClips;
                case AudioCategory.Items:
                    return ItemClips;
                case AudioCategory.Environment:
                    return EnvironmentClips;
                default:
                    return new List<AudioClipEntry>();
            }
        }
    }
}
