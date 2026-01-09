using UnityEngine;

namespace Core.Audio
{
    /// <summary>
    /// Main audio service interface
    /// </summary>
    public interface IAudioService
    {
        // Music
        void PlayMusic(AudioClip clip, float fadeTime = 1f);
        void StopMusic(float fadeTime = 1f);
        void PauseMusic();
        void ResumeMusic();

        // SFX
        AudioSource PlaySFX(AudioClip clip, float pitch = 1f);
        AudioSource PlaySFXAtPosition(AudioClip clip, Vector3 position);
        void StopSound(AudioSource source, float fadeTime = 0f);

        // Volume
        void SetMasterVolume(float volume);
        void SetMusicVolume(float volume);
        void SetSFXVolume(float volume);
        void SetMute(bool mute);
    }
}
