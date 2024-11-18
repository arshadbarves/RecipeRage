using System;
using System.Collections.Generic;
using UnityEngine;

namespace Audio.SFX
{
    [CreateAssetMenu(fileName = "SFXLibrary", menuName = "RecipeRage/Audio/SFXLibrary")]
    public class SfxLibrary : ScriptableObject
    {

        public List<SfxCategory> categories = new List<SfxCategory>();
        private Dictionary<string, AudioClipData> _clipLookup;

        public void Initialize()
        {
            _clipLookup = new Dictionary<string, AudioClipData>();
            foreach (SfxCategory category in categories)
            {
                foreach (AudioClipData clipData in category.clips)
                {
                    if (!string.IsNullOrEmpty(clipData.id) && clipData.clip != null)
                    {
                        _clipLookup[clipData.id] = clipData;
                    }
                }
            }
        }

        public AudioClipData GetClipData(string id)
        {
            if (_clipLookup == null)
            {
                Initialize();
            }

            return _clipLookup != null && _clipLookup.TryGetValue(id, out AudioClipData clipData) ? clipData : null;
        }
        [Serializable]
        public class SfxCategory
        {
            public string categoryName;
            public List<AudioClipData> clips = new List<AudioClipData>();
        }

        [Serializable]
        public class AudioClipData
        {
            public string id;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume = 1f;
            public bool spatialize = true;
        }
    }
}