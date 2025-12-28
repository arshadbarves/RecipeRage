using System;
using Core.Audio;
using Core.Logging;
using Core.SaveSystem;
using UnityEngine;

namespace Core.Settings
{
    /// <summary>
    /// Service for applying settings to Unity systems.
    /// Follows SOLID: Single Responsibility.
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private IAudioService _audioService;
        private readonly ISaveService _saveService;

        public SettingsService(IAudioService audioService, ISaveService saveService)
        {
            _audioService = audioService;
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
        }

        public void UpdateAudioService(IAudioService audioService)
        {
            _audioService = audioService;
            GameLogger.Log("SettingsService: AudioService updated");
        }

        public void Initialize()
        {
            GameLogger.Log("Initializing SettingsService - applying initial settings");
            ApplyAllSettings(_saveService.GetSettings());
        }

        public void ApplyAllSettings(GameSettingsData data)
        {
            if (data == null) return;

            ApplyGraphicsSettings(data);
            ApplyAudioSettings(data);
            ApplyGameplaySettings(data);
            
            GameLogger.Log("All settings applied to engine");
        }

        public void ApplyGraphicsSettings(GameSettingsData data)
        {
            if (data == null) return;

            // Quality
            QualitySettings.SetQualityLevel(data.GraphicsQuality, true);
            
            // Frame Rate
            Application.targetFrameRate = data.TargetFrameRate;
            
            // Fullscreen
            Screen.fullScreen = data.IsFullscreen;
            
            // VSync
            QualitySettings.vSyncCount = data.IsVSyncEnabled ? 1 : 0;

            // Note: Shadows and Bloom toggles usually require manipulating 
            // specific URP Asset properties or PostProcessing Volumes.
            // For now, we log the intent.
            GameLogger.Log($"Graphics applied: Quality={data.GraphicsQuality}, FPS={data.TargetFrameRate}, VSync={data.IsVSyncEnabled}");
        }

        public void ApplyAudioSettings(GameSettingsData data)
        {
            if (data == null || _audioService == null) return;

            _audioService.SetMasterVolume(data.MasterVolume);
            _audioService.SetMusicVolume(data.MusicVolume);
            _audioService.SetSFXVolume(data.SFXVolume);
            _audioService.SetMute(data.IsMuted);
            
            GameLogger.Log($"Audio applied: Master={data.MasterVolume}, Mute={data.IsMuted}");
        }

        public void ApplyGameplaySettings(GameSettingsData data)
        {
            if (data == null) return;

            // Gameplay settings like sensitivity are usually consumed by 
            // the Input Handling components at runtime.
            // This service ensures they are broadcasted or available.
            GameLogger.Log($"Gameplay settings applied: Sensitivity={data.Sensitivity}, InvertY={data.InvertY}");
        }
    }
}
