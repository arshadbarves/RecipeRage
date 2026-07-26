using System;
using UnityEngine;

namespace Playcenter.Services
{
    /// <summary>
    /// Inspector-facing clip registry. On boot, registers all clips with IAudioService.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioClipMap", menuName = "Playcenter/Audio Clip Map")]
    public sealed class AudioClipMap : ScriptableObject
    {
        [Serializable]
        public sealed class Entry
        {
            public string Id;
            public AudioClip Clip;
        }

        [SerializeField] private Entry[] _entries = Array.Empty<Entry>();

        public void RegisterAll(IAudioService audioService)
        {
            if (audioService is UnityAudioService unityAudio)
            {
                foreach (var entry in _entries)
                {
                    unityAudio.RegisterClip(entry.Id, entry.Clip);
                }
            }
        }
    }
}
