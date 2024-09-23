using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GameSystem.Audio
{
    public enum AudioChannelType
    {
        Master,
        Music,
        SFX,
        Ambient,
        Voice
    }

    public interface IAudioService
    { 
        Task InitializeAsync();
        void PlayMusic(string clipName, float fadeInDuration = 1f, bool loop = true);
        void StopMusic(float fadeOutDuration = 1f);
        void PlaySfx(string clipName, Vector3? position = null);
        void PlayAmbient(string clipName, bool loop = true);
        void PlayVoice(string clipName);
        void SetVolume(AudioChannelType channelType, float volume);
        float GetVolume(AudioChannelType master);
        void SetMusicPlaylist(List<string> playlist, bool shuffle = false);
        void PlayNextMusic(float fadeInDuration = 1f, float fadeOutDuration = 1f);
        void ToggleMusicLoop(bool loop);
        void PauseChannel(AudioChannelType channelType);
        void ResumeChannel(AudioChannelType channelType);
        void StopAllAudio();
        void SaveSettings();
        void LoadSettings();
        Task CleanupAsync();
    }
}