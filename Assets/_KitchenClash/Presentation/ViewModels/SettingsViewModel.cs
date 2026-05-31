using KitchenClash.Application.Models;
using KitchenClash.Application;
using System.Collections.Generic;
using KitchenClash.Infrastructure.EOS;
using KitchenClash.Infrastructure.Persistence;
using KitchenClash.Domain;
using KitchenClash.Application.Services;
using KitchenClash.Presentation;
using KitchenClash.Presentation.Common;
using UnityEngine;
using VContainer;

namespace KitchenClash.Presentation.ViewModels
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
        public BindableProperty<float> ControlsSensitivity { get; } = new BindableProperty<float>(1f);
        public BindableProperty<bool> IsGuest { get; } = new BindableProperty<bool>(false);

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
            CheckGuestStatus();
        }

        private void CheckGuestStatus()
        {
            var settings = _saveService.GetSettings();
            if (settings != null && !string.IsNullOrEmpty(_authService.ProductUserId))
            {
                IsGuest.Value = settings.LastLoginMethod == "DeviceID";
            }
            else
            {
                IsGuest.Value = false;
            }
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
                ControlsSensitivity.Value = settings.ControlsSensitivity;

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
                s.ControlsSensitivity = ControlsSensitivity.Value;
            });

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

            MusicVolume.Value = defaultSettings.MusicVolume;
            SFXVolume.Value = defaultSettings.SFXVolume;
            IsMuted.Value = defaultSettings.IsMuted;
            VibrationEnabled.Value = defaultSettings.VibrationEnabled;
            NotificationsEnabled.Value = defaultSettings.NotificationsEnabled;
            GraphicsQuality.Value = defaultSettings.GraphicsQuality;
            Language.Value = defaultSettings.LanguageCode;
            ControlsSensitivity.Value = defaultSettings.ControlsSensitivity;

            SaveSettings();
        }

        public void ClearData()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            ResetToDefaults();
        }
    }
}
