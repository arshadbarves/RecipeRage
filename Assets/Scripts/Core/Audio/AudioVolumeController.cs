using Core.SaveSystem;
using UnityEngine;
using UnityEngine.Audio;

namespace Core.Audio
{
    /// <summary>
    /// Handles volume control - pure C# class
    /// </summary>
    public class AudioVolumeController : IAudioVolumeController
    {
        private readonly ISaveService _saveService;
        private AudioMixer _audioMixer;

        private const string MASTER_PARAM = "MasterVolume";
        private const string MUSIC_PARAM = "MusicVolume";
        private const string SFX_PARAM = "SFXVolume";

        public AudioVolumeController(ISaveService saveService)
        {
            _saveService = saveService;
            LoadAudioMixer();
            ApplySettings();
        }

        private void LoadAudioMixer()
        {
            _audioMixer = Resources.Load<AudioMixer>("Audio/MainMixer");
            if (_audioMixer == null)
            {
                Debug.LogWarning("[AudioVolumeController] MainMixer not found in Resources/Audio");
            }
        }

        private void ApplySettings()
        {
            if (_saveService == null) return;

            var settings = _saveService.GetSettings();
            SetMasterVolume(settings.MasterVolume);
            SetMusicVolume(settings.MusicVolume);
            SetSFXVolume(settings.SfxVolume);
            SetMute(settings.IsMuted);
        }

        public void SetMasterVolume(float volume)
        {
            SetMixerVolume(MASTER_PARAM, volume);
            _saveService?.UpdateSettings(s => s.MasterVolume = volume);
        }

        public void SetMusicVolume(float volume)
        {
            SetMixerVolume(MUSIC_PARAM, volume);
            _saveService?.UpdateSettings(s => s.MusicVolume = volume);
        }

        public void SetSFXVolume(float volume)
        {
            SetMixerVolume(SFX_PARAM, volume);
            _saveService?.UpdateSettings(s => s.SfxVolume = volume);
        }

        public void SetMute(bool mute)
        {
            float volume = mute ? 0f : GetMasterVolume();
            SetMixerVolume(MASTER_PARAM, volume);
            _saveService?.UpdateSettings(s => s.IsMuted = mute);
        }

        public float GetMasterVolume() => _saveService?.GetSettings().MasterVolume ?? 0.8f;
        public float GetMusicVolume() => _saveService?.GetSettings().MusicVolume ?? 0.7f;
        public float GetSFXVolume() => _saveService?.GetSettings().SfxVolume ?? 0.9f;

        private void SetMixerVolume(string paramName, float volume)
        {
            if (_audioMixer == null) return;

            float decibelValue = volume > 0.0001f ? 20f * Mathf.Log10(volume) : -80f;
            _audioMixer.SetFloat(paramName, decibelValue);
        }
    }
}
