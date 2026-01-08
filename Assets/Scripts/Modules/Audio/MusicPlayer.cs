using System.Collections;
using Core.Utilities;
using UnityEngine;

namespace Modules.Audio
{
    /// <summary>
    /// Handles music playback - needs MonoBehaviour for coroutines
    /// </summary>
    public class MusicPlayer : IMusicPlayer
    {
        private readonly IAudioVolumeController _volumeController;
        private AudioSource _musicSource;
        private AudioClip _currentMusic;
        private Coroutine _fadeCoroutine;

        public MusicPlayer(IAudioVolumeController volumeController)
        {
            _volumeController = volumeController;
            CreateMusicSource();
        }

        private void CreateMusicSource()
        {
            GameObject musicObj = new GameObject("MusicSource");
            if (Application.isPlaying)
            {
                Object.DontDestroyOnLoad(musicObj);
            }

            _musicSource = musicObj.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.spatialBlend = 0f;
            _musicSource.playOnAwake = false;
        }

        public void PlayMusic(AudioClip clip, float fadeTime)
        {
            if (clip == null) return;
            if (_currentMusic == clip && _musicSource.isPlaying) return;

            float volume = _volumeController.GetMusicVolume();

            if (_musicSource.isPlaying && fadeTime > 0f)
            {
                // Fade out current, then play new
                CoroutineRunner.Run(FadeOutAndPlayNew(clip, fadeTime, volume));
            }
            else
            {
                _musicSource.clip = clip;
                _musicSource.volume = volume;
                _musicSource.Play();
                _currentMusic = clip;
            }
        }

        public void StopMusic(float fadeTime)
        {
            if (!_musicSource.isPlaying) return;

            if (fadeTime > 0f)
            {
                CoroutineRunner.Run(FadeOut(fadeTime));
            }
            else
            {
                _musicSource.Stop();
                _currentMusic = null;
            }
        }

        public void PauseMusic()
        {
            if (_musicSource.isPlaying)
            {
                _musicSource.Pause();
            }
        }

        public void ResumeMusic()
        {
            if (!_musicSource.isPlaying && _currentMusic != null)
            {
                _musicSource.UnPause();
            }
        }

        private IEnumerator FadeOutAndPlayNew(AudioClip newClip, float fadeTime, float targetVolume)
        {
            float startVolume = _musicSource.volume;
            float elapsed = 0f;

            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                _musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeTime);
                yield return null;
            }

            _musicSource.Stop();
            _musicSource.clip = newClip;
            _musicSource.volume = 0f;
            _musicSource.Play();
            _currentMusic = newClip;

            elapsed = 0f;
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                _musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / fadeTime);
                yield return null;
            }

            _musicSource.volume = targetVolume;
        }

        private IEnumerator FadeOut(float fadeTime)
        {
            float startVolume = _musicSource.volume;
            float elapsed = 0f;

            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                _musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeTime);
                yield return null;
            }

            _musicSource.Stop();
            _currentMusic = null;
        }
    }
}
