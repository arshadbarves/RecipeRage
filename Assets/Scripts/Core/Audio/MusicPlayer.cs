using System.Collections;
using UnityEngine;

namespace Core.Audio
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
            Object.DontDestroyOnLoad(musicObj);
            
            _musicSource = musicObj.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.spatialBlend = 0f;
            _musicSource.playOnAwake = false;
        }

        public void PlayMusic(AudioClip clip, float fadeTime, float volume)
        {
            if (clip == null) return;
            if (_currentMusic == clip && _musicSource.isPlaying) return;

            if (_musicSource.isPlaying && fadeTime > 0f)
            {
                // Fade out current, then play new
                AudioCoroutineRunner.Instance.StartCoroutine(FadeOutAndPlayNew(clip, fadeTime, volume));
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
                AudioCoroutineRunner.Instance.StartCoroutine(FadeOut(fadeTime));
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

    /// <summary>
    /// Helper MonoBehaviour for running coroutines
    /// </summary>
    public class AudioCoroutineRunner : MonoBehaviour
    {
        private static AudioCoroutineRunner _instance;
        public static AudioCoroutineRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject obj = new GameObject("AudioCoroutineRunner");
                    DontDestroyOnLoad(obj);
                    _instance = obj.AddComponent<AudioCoroutineRunner>();
                }
                return _instance;
            }
        }
    }
}
