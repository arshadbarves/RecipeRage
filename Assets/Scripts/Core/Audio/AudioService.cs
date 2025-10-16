using UnityEngine;

namespace Core.Audio
{
    /// <summary>
    /// Main audio service - delegates to specialized components
    /// Pure C# class, no MonoBehaviour
    /// </summary>
    public class AudioService : IAudioService
    {
        private readonly IMusicPlayer _musicPlayer;
        private readonly ISFXPlayer _sfxPlayer;
        private readonly IAudioVolumeController _volumeController;

        public AudioService(
            IMusicPlayer musicPlayer,
            ISFXPlayer sfxPlayer,
            IAudioVolumeController volumeController)
        {
            _musicPlayer = musicPlayer;
            _sfxPlayer = sfxPlayer;
            _volumeController = volumeController;
        }

        // Music delegation
        public void PlayMusic(AudioClip clip, float fadeTime = 1f, float volume = 1f)
            => _musicPlayer.PlayMusic(clip, fadeTime, volume);

        public void StopMusic(float fadeTime = 1f)
            => _musicPlayer.StopMusic(fadeTime);

        public void PauseMusic()
            => _musicPlayer.PauseMusic();

        public void ResumeMusic()
            => _musicPlayer.ResumeMusic();

        // SFX delegation
        public AudioSource PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
            => _sfxPlayer.PlaySFX(clip, volume, pitch);

        public AudioSource PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
            => _sfxPlayer.PlaySFXAtPosition(clip, position, volume);

        public void StopSound(AudioSource source, float fadeTime = 0f)
            => _sfxPlayer.StopSound(source, fadeTime);

        // Volume delegation
        public void SetMasterVolume(float volume)
            => _volumeController.SetMasterVolume(volume);

        public void SetMusicVolume(float volume)
            => _volumeController.SetMusicVolume(volume);

        public void SetSFXVolume(float volume)
            => _volumeController.SetSFXVolume(volume);

        public void SetMute(bool mute)
            => _volumeController.SetMute(mute);
    }
}
