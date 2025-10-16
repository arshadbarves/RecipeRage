using UnityEngine;

namespace Core.Audio
{
    public interface ISFXPlayer
    {
        AudioSource PlaySFX(AudioClip clip, float volume, float pitch);
        AudioSource PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume);
        void StopSound(AudioSource source, float fadeTime);
    }
}
