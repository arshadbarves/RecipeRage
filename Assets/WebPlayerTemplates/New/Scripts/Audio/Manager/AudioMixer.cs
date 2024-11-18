using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Audio.Config;
using UnityEngine;
using UnityEngine.Audio;
using VContainer;

namespace Audio.Manager
{

    public class AudioMixer : IDisposable
    {
        private const float DefaultTransitionTime = 0.5f;
        private readonly AudioConfig _config;
        private readonly Dictionary<string, float> _currentValues;
        private readonly Dictionary<string, float> _defaultValues;

        // Snapshot handling
        private readonly Dictionary<string, AudioMixerSnapshot> _snapshots;
        private AudioMixerSnapshot _currentSnapshot;
        private bool _isMuted;

        [Inject]
        public AudioMixer(AudioConfig config)
        {
            _config = config;
            _defaultValues = new Dictionary<string, float>();
            _currentValues = new Dictionary<string, float>();
            _snapshots = new Dictionary<string, AudioMixerSnapshot>();
            InitializeMixer();
        }

        public void Dispose()
        {
            // Save current values before disposing
            foreach (KeyValuePair<string, float> kvp in _currentValues)
            {
                float normalizedValue = Mathf.InverseLerp(_config.minVolume, _config.maxVolume, kvp.Value);
                PlayerPrefs.SetFloat(kvp.Key, normalizedValue);
            }
            PlayerPrefs.Save();
        }

        private void InitializeMixer()
        {
            if (_config.mainMixer == null)
                return;

            // Cache default values
            CacheParameters();
            LoadSavedValues();
            InitializeSnapshots();
            ApplyMobileOptimizations();
        }

        private void CacheParameters()
        {
            CacheValue(_config.masterVolumeParam);
            CacheValue(_config.musicVolumeParam);
            CacheValue(_config.sfxVolumeParam);
            CacheValue(_config.voiceVolumeParam);
        }

        private void CacheValue(string parameter)
        {
            if (_config.mainMixer.GetFloat(parameter, out float value))
            {
                _defaultValues[parameter] = value;
                _currentValues[parameter] = value;
            }
        }

        private void LoadSavedValues()
        {
            SetVolume(_config.masterVolumeParam, PlayerPrefs.GetFloat("MasterVolume", 1f));
            SetVolume(_config.musicVolumeParam, PlayerPrefs.GetFloat("MusicVolume", 1f));
            SetVolume(_config.sfxVolumeParam, PlayerPrefs.GetFloat("SFXVolume", 1f));
            SetVolume(_config.voiceVolumeParam, PlayerPrefs.GetFloat("VoiceVolume", 1f));
        }

        private void InitializeSnapshots()
        {
            if (_config.mainMixer == null) return;

            foreach (AudioMixerPreset.SnapshotSettings snapshot in _config.mixerPreset.snapshots)
            {
                if (snapshot.snapshot != null)
                {
                    _snapshots[snapshot.name] = snapshot.snapshot;
                }
            }
        }

        private void ApplyMobileOptimizations()
        {
            if (Application.isMobilePlatform)
            {
                // Apply mobile-specific mixer settings
                if (_snapshots.ContainsKey("Mobile"))
                {
                    TransitionToSnapshot("Mobile", 0f);
                }

                // Optimize DSP buffer size
                AudioConfiguration config = AudioSettings.GetConfiguration();
                if (config.dspBufferSize > 256)
                {
                    config.dspBufferSize = 256;
                    AudioSettings.Reset(config);
                }
            }
        }

        public void SetVolume(string parameter, float normalizedVolume)
        {
            if (_config.mainMixer == null) return;

            normalizedVolume = Mathf.Clamp01(normalizedVolume);
            float volume = Mathf.Lerp(_config.minVolume, _config.maxVolume, normalizedVolume);

            _config.mainMixer.SetFloat(parameter, volume);
            _currentValues[parameter] = volume;

            SaveVolume(parameter, normalizedVolume);
        }

        private void SaveVolume(string parameter, float normalizedVolume)
        {
            PlayerPrefs.SetFloat(parameter, normalizedVolume);
            PlayerPrefs.Save();
        }

        public float GetVolume(string parameter)
        {
            if (_config.mainMixer == null || !_currentValues.TryGetValue(parameter, out float currentValue))
                return 0f;

            return Mathf.InverseLerp(_config.minVolume, _config.maxVolume, currentValue);
        }

        public void MuteAll(bool mute)
        {
            if (_isMuted == mute) return;

            _isMuted = mute;
            if (mute)
            {
                foreach (string param in _currentValues.Keys)
                {
                    _config.mainMixer.SetFloat(param, _config.minVolume);
                }
            }
            else
            {
                foreach (string param in _currentValues.Keys)
                {
                    _config.mainMixer.SetFloat(param, _currentValues[param]);
                }
            }
        }

        public void TransitionToSnapshot(string snapshotName, float timeToReach = DefaultTransitionTime)
        {
            if (!_snapshots.TryGetValue(snapshotName, out AudioMixerSnapshot snapshot))
                return;

            _currentSnapshot = snapshot;
            _currentSnapshot.TransitionTo(timeToReach);
        }

        public async Task FadeOut(float duration)
        {
            float startVolume = GetVolume(_config.masterVolumeParam);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float volume = Mathf.Lerp(startVolume, 0f, t);
                SetVolume(_config.masterVolumeParam, volume);
                await Task.Yield();
            }
        }

        public async Task FadeIn(float duration)
        {
            float targetVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float volume = Mathf.Lerp(0f, targetVolume, t);
                SetVolume(_config.masterVolumeParam, volume);
                await Task.Yield();
            }
        }

        public void ResetToDefaults()
        {
            foreach (KeyValuePair<string, float> kvp in _defaultValues)
            {
                SetVolume(kvp.Key, Mathf.InverseLerp(_config.minVolume, _config.maxVolume, kvp.Value));
            }
        }
    }
}