using System.Collections.Generic;
using Modules.UI;
using UI.ViewModels;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace UI.Components.Tabs
{
    public class SettingsTabComponent
    {
        [Inject] private IUIService _uiService;

        private VisualElement _root;
        private readonly SettingsViewModel _viewModel;

        private Slider _musicVolumeSlider;
        private Slider _sfxVolumeSlider;
        private Toggle _muteToggle;
        private Label _musicVolumeLabel;
        private Label _sfxVolumeLabel;
        private DropdownField _qualityDropdown;
        private DropdownField _resolutionDropdown;
        private Toggle _fullscreenToggle;
        private Toggle _vsyncToggle;
        private Toggle _fpsToggle;
        private Slider _sensitivitySlider;
        private Toggle _vibrationToggle;
        private Label _sensitivityLabel;
        private DropdownField _languageDropdown;
        private Toggle _tutorialsToggle;
        private Toggle _notificationsToggle;
        private Label _versionLabel;

        public SettingsTabComponent(SettingsViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public void Initialize(VisualElement root)
        {
            if (root == null) return;
            _root = root;
            QueryElements();
            InitializeDropdowns();
            SetupValueChangeCallbacks();
            SetupButtons();

            BindViewModel();
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

        private void BindViewModel()
        {
            _viewModel.MusicVolume.Bind(val =>
            {
                if (_musicVolumeSlider != null) _musicVolumeSlider.value = val;
                UpdateVolumeLabel(_musicVolumeLabel, val);
                AudioListener.volume = val;
            });
            _viewModel.SFXVolume.Bind(val =>
            {
                if (_sfxVolumeSlider != null) _sfxVolumeSlider.value = val;
                UpdateVolumeLabel(_sfxVolumeLabel, val);
            });
        }

        private void SetupValueChangeCallbacks()
        {
            _musicVolumeSlider?.RegisterValueChangedCallback(evt => {
                _viewModel.MusicVolume.Value = evt.newValue;
                _viewModel.SaveVolumeSettings();
            });
            _sfxVolumeSlider?.RegisterValueChangedCallback(evt => {
                _viewModel.SFXVolume.Value = evt.newValue;
                _viewModel.SaveVolumeSettings();
            });

            _qualityDropdown?.RegisterValueChangedCallback(evt => QualitySettings.SetQualityLevel(_qualityDropdown.index));
            _fullscreenToggle?.RegisterValueChangedCallback(evt => Screen.fullScreen = evt.newValue);
        }

        private void SetupButtons()
        {
            _root.Q<Button>("logout-button")?.RegisterCallback<ClickEvent>(_ => _viewModel.Logout());
            _root.Q<Button>("reset-button")?.RegisterCallback<ClickEvent>(_ => OnResetClicked());
        }

        private void UpdateVolumeLabel(Label label, float value) { if (label != null) label.text = $"{Mathf.RoundToInt(value * 100)}%"; }

        private void UpdateVersionInfo() { if (_versionLabel != null) _versionLabel.text = $"v{Application.version}"; }

        private void OnResetClicked() { _uiService?.ShowNotification("Settings Reset", NotificationType.Success); }

        public void Dispose() { }
    }
}