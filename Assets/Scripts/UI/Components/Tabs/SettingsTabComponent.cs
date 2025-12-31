using UI.ViewModels; // Added

// ...

    public class SettingsTabComponent
    {
        [Inject] private IUIService _uiService;

        private VisualElement _root;
        private readonly SettingsViewModel _viewModel;

        private Slider _musicVolumeSlider;
// ...
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
                _viewModel.SaveVolumeSettings(); // Autosave
            });
            _sfxVolumeSlider?.RegisterValueChangedCallback(evt => {
                _viewModel.SFXVolume.Value = evt.newValue;
                _viewModel.SaveVolumeSettings();
            });
            
            // ... (keep others)
            _qualityDropdown?.RegisterValueChangedCallback(evt => QualitySettings.SetQualityLevel(_qualityDropdown.index));
            _fullscreenToggle?.RegisterValueChangedCallback(evt => Screen.fullScreen = evt.newValue);
        }

        private void SetupButtons()
        {
            _root.Q<Button>("logout-button")?.RegisterCallback<ClickEvent>(_ => _viewModel.Logout());
            _root.Q<Button>("reset-button")?.RegisterCallback<ClickEvent>(_ => OnResetClicked());
        }

        private void LoadSettings() { } // Removed logic (moved to VM)

        private void UpdateVolumeLabel(Label label, float value) { if (label != null) label.text = $"{Mathf.RoundToInt(value * 100)}%"; }

        private void UpdateVersionInfo() { if (_versionLabel != null) _versionLabel.text = $"v{Application.version}"; }

        private void OnResetClicked() { _uiService?.ShowNotification("Settings Reset", NotificationType.Success); }

        public void Dispose() { }
    }
}