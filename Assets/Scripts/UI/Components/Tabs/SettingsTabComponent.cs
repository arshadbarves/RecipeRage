using System;
using System.Collections.Generic;
using Core.Bootstrap;
using Core.Logging;
using Core.SaveSystem;
using RecipeRage.Modules.Auth.Core;
using UI.Core;
using UI.Screens;
using UnityEngine;
using UnityEngine.UIElements;
using UI;

namespace UI.Components.Tabs
{
    public class SettingsTabComponent
    {
        [Inject] private ISaveService _saveService;
        [Inject] private IUIService _uiService;
        [Inject] private IAuthService _authService;

        private VisualElement _root;

        private Slider _musicVolumeSlider;
        // ... (skipping fields for match)
        private Label _versionLabel;

        private float _previousMusicVolume = 0.75f;
        private float _previousSfxVolume = 0.75f;

        public SettingsTabComponent()
        {
        }

        public void Initialize(VisualElement root)
        {
            if (root == null) return;
            _root = root;
            QueryElements();
            InitializeDropdowns();
            SetupValueChangeCallbacks();
            SetupButtons();
            LoadSettings();
            UpdateVersionInfo();
        }

        private void QueryElements()
        {
            _musicVolumeSlider = _root.Q<Slider>("music-volume");
            _sfxVolumeSlider = _root.Q<Slider>("sfx-volume");
            _muteToggle = _root.Q<Toggle>("mute-toggle");
            _musicVolumeLabel = _root.Q<Label>("music-volume-value");
            _sfxVolumeLabel = _root.Q<Label>("sfx-volume-value");
            _qualityDropdown = _root.Q<DropdownField>("quality-dropdown");
            _resolutionDropdown = _root.Q<DropdownField>("resolution-dropdown");
            _fullscreenToggle = _root.Q<Toggle>("fullscreen-toggle");
            _vsyncToggle = _root.Q<Toggle>("vsync-toggle");
            _fpsToggle = _root.Q<Toggle>("fps-toggle");
            _sensitivitySlider = _root.Q<Slider>("sensitivity-slider");
            _vibrationToggle = _root.Q<Toggle>("vibration-toggle");
            _sensitivityLabel = _root.Q<Label>("sensitivity-value");
            _languageDropdown = _root.Q<DropdownField>("language-dropdown");
            _tutorialsToggle = _root.Q<Toggle>("tutorials-toggle");
            _notificationsToggle = _root.Q<Toggle>("notifications-toggle");
            _versionLabel = _root.Q<Label>("version-label");
        }

        private void InitializeDropdowns()
        {
            if (_qualityDropdown != null) {
                _qualityDropdown.choices = new List<string>(QualitySettings.names);
                _qualityDropdown.index = QualitySettings.GetQualityLevel();
            }
            if (_resolutionDropdown != null) {
                Resolution[] resolutions = Screen.resolutions;
                List<string> choices = new();
                int current = 0;
                for (int i = 0; i < resolutions.Length; i++) {
                    choices.Add($"{resolutions[i].width}x{resolutions[i].height}");
                    if (resolutions[i].width == Screen.currentResolution.width) current = i;
                }
                _resolutionDropdown.choices = choices;
                _resolutionDropdown.index = current;
            }
            if (_languageDropdown != null) {
                _languageDropdown.choices = new List<string> { "English", "Spanish" };
                _languageDropdown.index = 0;
            }
        }

        private void SetupValueChangeCallbacks()
        {
            _musicVolumeSlider?.RegisterValueChangedCallback(evt => { AudioListener.volume = evt.newValue; UpdateVolumeLabel(_musicVolumeLabel, evt.newValue); });
            _sfxVolumeSlider?.RegisterValueChangedCallback(evt => { UpdateVolumeLabel(_sfxVolumeLabel, evt.newValue); });
            _qualityDropdown?.RegisterValueChangedCallback(evt => QualitySettings.SetQualityLevel(_qualityDropdown.index));
            _fullscreenToggle?.RegisterValueChangedCallback(evt => Screen.fullScreen = evt.newValue);
        }

        private void SetupButtons()
        {
            _root.Q<Button>("logout-button")?.RegisterCallback<ClickEvent>(_ => OnLogoutClicked());
            _root.Q<Button>("reset-button")?.RegisterCallback<ClickEvent>(_ => OnResetClicked());
        }

        private void LoadSettings()
        {
            var settings = _saveService?.GetSettings();
            if (settings == null) return;
            if (_musicVolumeSlider != null) _musicVolumeSlider.value = settings.MusicVolume;
            if (_sfxVolumeSlider != null) _sfxVolumeSlider.value = settings.SFXVolume;
        }

        private void UpdateVolumeLabel(Label label, float value) { if (label != null) label.text = $"{Mathf.RoundToInt(value * 100)}%"; }

        private void UpdateVersionInfo() { if (_versionLabel != null) _versionLabel.text = $"v{Application.version}"; }

        private async void OnLogoutClicked()
        {
            if (_authService != null) await _authService.LogoutAsync();
        }

        private void OnResetClicked() { _uiService?.ShowNotification("Settings Reset", NotificationType.Success); }

        public void Dispose() { }
    }
}