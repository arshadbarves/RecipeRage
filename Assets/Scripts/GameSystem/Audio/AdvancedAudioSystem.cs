using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using UnityEngine.ResourceManagement.ResourceLocations;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace GameSystem.Audio
{
    public class AdvancedAudioSystem : IAudioService
    {
        private const int PoolSize = 10;
        private const string SettingsFileName = "audio_settings.json";
        private readonly Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();

        private readonly Dictionary<AudioChannelType, List<AudioSource>> _audioSources =
            new Dictionary<AudioChannelType, List<AudioSource>>();

        private readonly List<string> _musicPlaylist = new List<string>();
        private AudioMixer _audioMixer;
        private AudioSettings _audioSettings = new AudioSettings();
        private int _currentMusicIndex = -1;
        private bool _isMusicLooping;

        public async Task InitializeAsync()
        {
            _audioMixer = await Addressables.LoadAssetAsync<AudioMixer>("AudioMixer").Task;
            await LoadAudioClips();
            CreateAudioSources();
            LoadSettings();
            ApplyAudioSettings();
        }

        public void PlayMusic(string clipName, float fadeInDuration = 1f, bool loop = true)
        {
            AudioSource audioSource = GetAvailableAudioSource(AudioChannelType.Music);
            audioSource.clip = GetClip(clipName);
            audioSource.loop = loop;
            audioSource.volume = 0f;
            audioSource.Play();
            FadeAudioSource(audioSource, 0f, _audioSettings.MusicVolume, fadeInDuration);
        }

        public void StopMusic(float fadeOutDuration = 1f)
        {
            List<AudioSource> musicSources = _audioSources[AudioChannelType.Music];
            foreach (AudioSource source in musicSources.Where(s => s.isPlaying))
            {
                FadeAudioSource(source, source.volume, 0f, fadeOutDuration, () => source.Stop());
            }
        }

        public void PlaySfx(string clipName, Vector3? position = null)
        {
            AudioSource audioSource = GetAvailableAudioSource(AudioChannelType.SFX);
            audioSource.clip = GetClip(clipName);
            if (position.HasValue)
            {
                audioSource.transform.position = position.Value;
                audioSource.spatialBlend = 1f;
            }
            else
            {
                audioSource.spatialBlend = 0f;
            }

            audioSource.Play();
        }

        public void PlayAmbient(string clipName, bool loop = true)
        {
            AudioSource audioSource = GetAvailableAudioSource(AudioChannelType.Ambient);
            audioSource.clip = GetClip(clipName);
            audioSource.loop = loop;
            audioSource.Play();
        }

        public void PlayVoice(string clipName)
        {
            AudioSource audioSource = GetAvailableAudioSource(AudioChannelType.Voice);
            audioSource.clip = GetClip(clipName);
            audioSource.Play();
        }

        public void SetVolume(AudioChannelType channelType, float volume)
        {
            switch (channelType)
            {
                case AudioChannelType.Master:
                    _audioSettings.MasterVolume = volume;
                    break;
                case AudioChannelType.Music:
                    _audioSettings.MusicVolume = volume;
                    break;
                case AudioChannelType.SFX:
                    _audioSettings.SfxVolume = volume;
                    break;
                case AudioChannelType.Ambient:
                    _audioSettings.AmbientVolume = volume;
                    break;
                case AudioChannelType.Voice:
                    _audioSettings.VoiceVolume = volume;
                    break;
            }

            ApplyAudioSettings();
            SaveSettings();
        }

        public float GetVolume(AudioChannelType channelType)
        {
            switch (channelType)
            {
                case AudioChannelType.Master:
                    return _audioSettings.MasterVolume;
                case AudioChannelType.Music:
                    return _audioSettings.MusicVolume;
                case AudioChannelType.SFX:
                    return _audioSettings.SfxVolume;
                case AudioChannelType.Ambient:
                    return _audioSettings.AmbientVolume;
                case AudioChannelType.Voice:
                    return _audioSettings.VoiceVolume;
                default:
                    return 0f;
            }
        }

        public void SetMusicPlaylist(List<string> playlist, bool shuffle = false)
        {
            _musicPlaylist.Clear();
            _musicPlaylist.AddRange(playlist);
            if (shuffle)
            {
                ShufflePlaylist();
            }

            _currentMusicIndex = -1;
        }

        public void PlayNextMusic(float fadeInDuration = 1f, float fadeOutDuration = 1f)
        {
            if (_musicPlaylist.Count == 0) return;

            StopMusic(fadeOutDuration);

            _currentMusicIndex = (_currentMusicIndex + 1) % _musicPlaylist.Count;
            PlayMusic(_musicPlaylist[_currentMusicIndex], fadeInDuration, _isMusicLooping);
        }

        public void ToggleMusicLoop(bool loop)
        {
            _isMusicLooping = loop;
            foreach (AudioSource source in _audioSources[AudioChannelType.Music])
            {
                if (source.isPlaying)
                {
                    source.loop = loop;
                }
            }
        }

        public void PauseChannel(AudioChannelType channelType)
        {
            foreach (AudioSource source in _audioSources[channelType])
            {
                if (source.isPlaying)
                {
                    source.Pause();
                }
            }
        }

        public void ResumeChannel(AudioChannelType channelType)
        {
            foreach (AudioSource source in _audioSources[channelType])
            {
                if (source.clip != null && !source.isPlaying)
                {
                    source.UnPause();
                }
            }
        }

        public void StopAllAudio()
        {
            foreach (List<AudioSource> sources in _audioSources.Values)
            {
                foreach (AudioSource source in sources)
                {
                    source.Stop();
                }
            }
        }

        public void SaveSettings()
        {
            string json = JsonUtility.ToJson(_audioSettings);
            string path = Path.Combine(Application.persistentDataPath, SettingsFileName);
            File.WriteAllText(path, json);
            Debug.Log($"Audio settings saved to {path}");
        }

        public void LoadSettings()
        {
            string path = Path.Combine(Application.persistentDataPath, SettingsFileName);
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                _audioSettings = JsonUtility.FromJson<AudioSettings>(json);
                Debug.Log($"Audio settings loaded from {path}");
            }
            else
            {
                Debug.Log("No saved audio settings found. Using defaults.");
            }
        }

        public Task CleanupAsync()
        {
            SaveSettings();
            StopAllAudio();
            Addressables.Release(_audioMixer);
            foreach (AudioClip clip in _audioClips.Values)
            {
                Addressables.Release(clip);
            }

            _audioClips.Clear();
            _audioSources.Clear();
            return Task.CompletedTask;
        }

        public void Update()
        {
        }

        private async Task LoadAudioClips()
        {
            IList<IResourceLocation> clipHandles = await Addressables.LoadResourceLocationsAsync("AudioClips").Task;
            foreach (IResourceLocation handle in clipHandles)
            {
                AudioClip clip = await Addressables.LoadAssetAsync<AudioClip>(handle).Task;
                _audioClips[clip.name] = clip;
            }
        }

        private void CreateAudioSources()
        {
            GameObject gameObject = new GameObject("AudioSources");
            Object.DontDestroyOnLoad(gameObject);

            foreach (AudioChannelType channelType in Enum.GetValues(typeof(AudioChannelType)))
            {
                if (channelType == AudioChannelType.Master) continue;
                _audioSources[channelType] = new List<AudioSource>();
                for (int i = 0; i < PoolSize; i++)
                {
                    AudioSource audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                    audioSource.outputAudioMixerGroup = _audioMixer.FindMatchingGroups(channelType.ToString())[0];
                    _audioSources[channelType].Add(audioSource);
                }
            }
        }

        private void ApplyAudioSettings()
        {
            _audioMixer.SetFloat("MasterVolume", LogarithmicVolume(_audioSettings.MasterVolume));
            _audioMixer.SetFloat("MusicVolume", LogarithmicVolume(_audioSettings.MusicVolume));
            _audioMixer.SetFloat("SFXVolume", LogarithmicVolume(_audioSettings.SfxVolume));
            _audioMixer.SetFloat("AmbientVolume", LogarithmicVolume(_audioSettings.AmbientVolume));
            _audioMixer.SetFloat("VoiceVolume", LogarithmicVolume(_audioSettings.VoiceVolume));
        }

        private float LogarithmicVolume(float linearVolume)
        {
            return Mathf.Log10(Mathf.Max(linearVolume, 0.0001f)) * 20f;
        }

        private AudioSource GetAvailableAudioSource(AudioChannelType channelType)
        {
            return _audioSources[channelType].FirstOrDefault(s => !s.isPlaying) ?? _audioSources[channelType][0];
        }

        private void ShufflePlaylist()
        {
            Random rng = new Random();
            int n = _musicPlaylist.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (_musicPlaylist[k], _musicPlaylist[n]) = (_musicPlaylist[n], _musicPlaylist[k]);
            }
        }

        private AudioClip GetClip(string clipName)
        {
            return _audioClips.GetValueOrDefault(clipName);
        }

        private async void FadeAudioSource(AudioSource audioSource, float startVolume, float endVolume, float duration,
            Action onComplete = null)
        {
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                audioSource.volume = Mathf.Lerp(startVolume, endVolume, t);
                await Task.Yield();
            }

            audioSource.volume = endVolume;
            onComplete?.Invoke();
        }
    }
}