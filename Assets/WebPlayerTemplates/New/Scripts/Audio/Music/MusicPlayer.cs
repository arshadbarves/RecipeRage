using System;
using Audio.Config;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;

namespace Audio.Music
{
    public class MusicPlayer
    {
        private readonly AudioConfig _config;
        private readonly ObjectPool<AudioSource> _sourcePool;
        private Sequence _fadeSequence;
        private AudioSource _musicSource;

        public MusicPlayer(AudioConfig config, ObjectPool<AudioSource> sourcePool)
        {
            _config = config;
            _sourcePool = sourcePool;
            InitializeMusicSource();
        }

        private void InitializeMusicSource()
        {
            _musicSource = _sourcePool.Get();
            _musicSource.loop = true;
            _musicSource.spatialBlend = 0f; // Always 2D
            _musicSource.priority = 0; // Highest priority
            SetupMusicMixer();
        }

        private void SetupMusicMixer()
        {
            if (_config.mainMixer != null)
            {
                AudioMixerGroup musicGroup = _config.mainMixer.FindMatchingGroups("Music")[0];
                if (musicGroup != null)
                {
                    _musicSource.outputAudioMixerGroup = musicGroup;
                }
            }
        }

        public void PlayMusic(AudioClip musicClip, float fadeInDuration = 1f)
        {
            if (musicClip == null)
            {
                Debug.LogWarning("Attempted to play null music clip");
                return;
            }

            try
            {
                _fadeSequence?.Kill();
                _fadeSequence = DOTween.Sequence();

                if (_musicSource.isPlaying)
                {
                    // Crossfade
                    float currentVolume = _musicSource.volume;
                    _fadeSequence.Append(_musicSource.DoFade(0f, fadeInDuration * 0.5f));
                    _fadeSequence.AppendCallback(() =>
                    {
                        _musicSource.clip = musicClip;
                        _musicSource.Play();
                    });
                    _fadeSequence.Append(_musicSource.DoFade(currentVolume, fadeInDuration * 0.5f));
                }
                else
                {
                    _musicSource.clip = musicClip;
                    _musicSource.volume = 0f;
                    _musicSource.Play();
                    _fadeSequence.Append(_musicSource.DoFade(1f, fadeInDuration));
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error playing music: {e.Message}");
            }
        }

        public void StopMusic(float fadeOutDuration = 1f)
        {
            _fadeSequence?.Kill();
            _fadeSequence = DOTween.Sequence();
            _fadeSequence.Append(_musicSource.DoFade(0f, fadeOutDuration));
            _fadeSequence.AppendCallback(() => _musicSource.Stop());
        }

        public void PauseMusic()
        {
            _musicSource.Pause();
        }

        public void ResumeMusic()
        {
            if (_musicSource.clip != null)
            {
                _musicSource.UnPause();
            }
        }

        public void Dispose()
        {
            _fadeSequence?.Kill();
            _fadeSequence = null;
        }
    }
}