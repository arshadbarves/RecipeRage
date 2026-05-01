using System.Collections.Generic;
using UnityEngine;

namespace KitchenClash.Application
{
    public interface IAudioService
    {
        void PlayMusic(AudioClip clip, float fadeTime = 1f);
        void StopMusic(float fadeTime = 1f);
        void PauseMusic();
        void ResumeMusic();
        AudioSource PlaySFX(AudioClip clip, float pitch = 1f);
        AudioSource PlaySFXAtPosition(AudioClip clip, Vector3 position);
        void StopSound(AudioSource source, float fadeTime = 0f);
        void PlayUISFX();
        AudioClip GetMusicTrack(int index);
        IReadOnlyList<AudioClip> GetAllMusicTracks();
        void SetMasterVolume(float volume);
        void SetMusicVolume(float volume);
        void SetSFXVolume(float volume);
        void SetMute(bool mute);
    }
}
