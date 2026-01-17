using System.Collections.Generic;
using Core.Auth;
using Core.Persistence;
using Core.Shared;
using Core.UI.Core;
using Core.Localization;
using UnityEngine;
using VContainer;

namespace Gameplay.UI.Features.Settings
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly ISaveService _saveService;
        private readonly IAuthService _authService;
        private readonly ILocalizationManager _localizationManager;

        public BindableProperty<float> MusicVolume { get; } = new BindableProperty<float>(1f);
        public BindableProperty<bool> IsMuted { get; } = new BindableProperty<bool>(false);

        public BindableProperty<float> SFXVolume { get; } = new BindableProperty<float>(1f);
        public BindableProperty<bool> VibrationEnabled { get; } = new BindableProperty<bool>(true);
        public BindableProperty<bool> NotificationsEnabled { get; } = new BindableProperty<bool>(true);
        public BindableProperty<int> GraphicsQuality { get; } = new BindableProperty<int>(2);
        public BindableProperty<string> Language { get; } = new BindableProperty<string>("English");

        public IReadOnlyCollection<string> AvailableLanguages => _localizationManager.AvailableLanguages;

        [Inject]
        public SettingsViewModel(ISaveService saveService, IAuthService authService, ILocalizationManager localizationManager)
        {
            _saveService = saveService;
            _authService = authService;
            _localizationManager = localizationManager;
        }

        public override void Initialize()
        {
            base.Initialize();
            LoadSettings();
        }

        private void LoadSettings()
        {
            var settings = _saveService.GetSettings();
            if (settings != null)
            {
                MusicVolume.Value = settings.MusicVolume;
                SFXVolume.Value = settings.SFXVolume;
                IsMuted.Value = settings.IsMuted;
                VibrationEnabled.Value = settings.VibrationEnabled;
                NotificationsEnabled.Value = settings.NotificationsEnabled;
                GraphicsQuality.Value = settings.GraphicsQuality;
                Language.Value = settings.LanguageCode;

                // Apply initial state
                AudioListener.pause = settings.IsMuted;
                QualitySettings.SetQualityLevel(settings.GraphicsQuality);

                if (_localizationManager.CurrentLanguage != settings.LanguageCode)
                {
                    _localizationManager.SetLanguage(settings.LanguageCode);
                }
            }
        }

        public void SaveSettings()
        {
            _saveService.UpdateSettings(s =>
            {
                s.MusicVolume = MusicVolume.Value;
                s.SFXVolume = SFXVolume.Value;
                s.IsMuted = IsMuted.Value;
                s.VibrationEnabled = VibrationEnabled.Value;
                s.NotificationsEnabled = NotificationsEnabled.Value;
                s.GraphicsQuality = GraphicsQuality.Value;
                s.LanguageCode = Language.Value;
            });

            // Apply side effects immediately when saving
            AudioListener.pause = IsMuted.Value;
            QualitySettings.SetQualityLevel(GraphicsQuality.Value);

            if (_localizationManager.CurrentLanguage != Language.Value)
            {
                _localizationManager.SetLanguage(Language.Value);
            }
        }

        public async void Logout()
        {
            await _authService.LogoutAsync();
        }

        public void ResetToDefaults()
        {
            var defaultSettings = new GameSettingsData();

            // Apply to properties to notify UI
            MusicVolume.Value = defaultSettings.MusicVolume;
            SFXVolume.Value = defaultSettings.SFXVolume;
            IsMuted.Value = defaultSettings.IsMuted;
            VibrationEnabled.Value = defaultSettings.VibrationEnabled;
            NotificationsEnabled.Value = defaultSettings.NotificationsEnabled;
            GraphicsQuality.Value = defaultSettings.GraphicsQuality;
            Language.Value = defaultSettings.LanguageCode;

            SaveSettings();
        }

        public void ClearData()
        {
            // Clear all local preferences
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();

            // Reset in-memory settings
            ResetToDefaults();

            // Ideally should also clear SaveService files if they exist outside PlayerPrefs
            // But relying on SaveService to handle its own persistence later
        }
    }
}