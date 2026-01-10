using System.Collections.Generic;
using UnityEngine;

namespace Core.Audio
{
    public class AudioService : IAudioService
    {
        private readonly IMusicPlayer _musicPlayer;
        private readonly ISFXPlayer _sfxPlayer;
        private readonly IAudioVolumeController _volumeController;
        private readonly AudioSettings _audioSettings;

        public AudioService(
            IMusicPlayer musicPlayer,
            ISFXPlayer sfxPlayer,
            IAudioVolumeController volumeController,
            AudioSettings audioSettings)
        {
            _musicPlayer = musicPlayer;
            _sfxPlayer = sfxPlayer;
            _volumeController = volumeController;
            _audioSettings = audioSettings;
        }

        // Music delegation
        public void PlayMusic(AudioClip clip, float fadeTime = 1f)
            => _musicPlayer.PlayMusic(clip, fadeTime);

        public void StopMusic(float fadeTime = 1f)
            => _musicPlayer.StopMusic(fadeTime);

        public void PauseMusic()
            => _musicPlayer.PauseMusic();

        public void ResumeMusic()
            => _musicPlayer.ResumeMusic();

        // SFX delegation
        public AudioSource PlaySFX(AudioClip clip, float pitch = 1f)
            => _sfxPlayer.PlaySFX(clip, pitch);

        public AudioSource PlaySFXAtPosition(AudioClip clip, Vector3 position)
            => _sfxPlayer.PlaySFXAtPosition(clip, position);

        public void StopSound(AudioSource source, float fadeTime = 0f)
            => _sfxPlayer.StopSound(source, fadeTime);

        // UI SFX
        public void PlayUISFX()
        {
            if (_audioSettings?.UISFX != null)
            {
                PlaySFX(_audioSettings.UISFX);
            }
        }

        // Music Helpers
        public AudioClip GetMusicTrack(int index)
            => _audioSettings?.GetMusicTrack(index);

        public IReadOnlyList<AudioClip> GetAllMusicTracks()
            => _audioSettings?.GetAllMusicTracks();

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
