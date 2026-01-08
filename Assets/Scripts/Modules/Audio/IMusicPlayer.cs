using UnityEngine;

namespace Modules.Audio
{
    public interface IMusicPlayer
    {
        void PlayMusic(AudioClip clip, float fadeTime = 1f);
        void StopMusic(float fadeTime = 1f);
        void PauseMusic();
        void ResumeMusic();
    }
}
