using System.Threading.Tasks;
using UnityEngine;

namespace Core.Interfaces
{
    public interface IAudioService
    {
        void PlayMusic(AudioClip musicClip, float fadeInDuration = 1f);
        void StopMusic(float fadeOutDuration = 1f);
        void PauseMusic();
        void ResumeMusic();

        Task PlaySFX(AudioClip clip, Vector3 position, float volume = 1f, bool spatialize = true);
        void PlaySFXOneShot(AudioClip clip, float volume = 1f);
        void StopAllSFX();
    }
}