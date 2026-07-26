using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Playcenter.Services
{
    /// <summary>
    /// Audio via Unity AudioMixer (Master/Music/SFX groups) + pooled AudioSources.
    /// Clips are registered by id via AudioClipMap.
    /// </summary>
    public sealed class UnityAudioService : IAudioService
    {
        private const string MasterParam = "MasterVolume";
        private const string MusicParam = "MusicVolume";
        private const string SfxParam = "SFXVolume";

        private readonly AudioMixer _mixer;
        private readonly AudioSource _musicSource;
        private readonly List<AudioSource> _sfxPool = new List<AudioSource>(8);
        private readonly Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>(32);

        public UnityAudioService(AudioMixer mixer, Transform poolParent)
        {
            _mixer = mixer;

            var musicGo = new GameObject("MusicSource");
            musicGo.transform.SetParent(poolParent, false);
            _musicSource = musicGo.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.outputAudioMixerGroup = _mixer.FindMatchingGroups("Music")[0];

            for (int i = 0; i < 8; i++)
            {
                var go = new GameObject($"SfxSource_{i}");
                go.transform.SetParent(poolParent, false);
                var source = go.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.outputAudioMixerGroup = _mixer.FindMatchingGroups("SFX")[0];
                _sfxPool.Add(source);
            }
        }

        public void RegisterClip(string id, AudioClip clip)
        {
            if (clip != null)
            {
                _clips[id] = clip;
            }
        }

        public void Play(string sfxId)
        {
            if (!_clips.TryGetValue(sfxId, out var clip))
            {
                return;
            }

            var source = GetFreeSource();
            source.PlayOneShot(clip);
        }

        public void PlayMusic(string musicId)
        {
            if (!_clips.TryGetValue(musicId, out var clip))
            {
                return;
            }

            if (_musicSource.clip == clip && _musicSource.isPlaying)
            {
                return;
            }

            _musicSource.clip = clip;
            _musicSource.Play();
        }

        public void StopMusic()
        {
            _musicSource.Stop();
        }

        public void SetMasterVolume(float volume01) => SetVolume(MasterParam, volume01);
        public void SetMusicVolume(float volume01) => SetVolume(MusicParam, volume01);
        public void SetSfxVolume(float volume01) => SetVolume(SfxParam, volume01);

        private void SetVolume(string param, float volume01)
        {
            var db = volume01 <= 0.0001f ? -80f : Mathf.Log10(Mathf.Clamp01(volume01)) * 20f;
            _mixer.SetFloat(param, db);
        }

        private AudioSource GetFreeSource()
        {
            foreach (var source in _sfxPool)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }
            return _sfxPool[0];
        }
    }
}
