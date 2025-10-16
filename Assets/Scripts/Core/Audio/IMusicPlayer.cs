using UnityEngine;

namespace Core.Audio
{
    public interface IMusicPlayer
    {
        void PlayMusic(AudioClip clip, float fadeTime, float volume);
        void StopMusic(float fadeTime);
        void PauseMusic();
        void ResumeMusic();
    }
}
