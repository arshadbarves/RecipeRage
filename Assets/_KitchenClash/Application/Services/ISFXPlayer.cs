using UnityEngine;

namespace KitchenClash.Application.Services
{
    public interface ISFXPlayer
    {
        AudioSource PlaySFX(AudioClip clip, float pitch = 1f);
        AudioSource PlaySFXAtPosition(AudioClip clip, Vector3 position);
        void StopSound(AudioSource source, float fadeTime = 0f);
    }
}
