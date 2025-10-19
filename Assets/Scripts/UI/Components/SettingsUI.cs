using System;
using System.Collections.Generic;
using Core.Bootstrap;
using Core.SaveSystem;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Components
{
    /// <summary>
    /// Settings tab content - embedded directly in MainMenu tabs
    /// Uses SaveService for persistence (cloud sync + encryption)
    /// Follows proper dependency injection pattern
    /// </summary>
    public class SettingsUI
    {
        private VisualElement _root;
        private readonly ISaveService _saveService;

        // Audio controls
        private Slider _musicVolumeSlider;
        private Slider _sfxVolumeSlider;
        private Toggle _muteToggle;
        private Label _musicVolumeLabel;
        private Label _sfxVolumeLabel;

        // Graphics controls
        private DropdownField _qualityDropdown;
        private DropdownField _resolutionDropdown;
        private Toggle _fullscreenToggle;
        private Toggle _vsyncToggle;
        private Toggle _fpsToggle;

        // Controls
        private Slider _sensitivitySlider;
        private Toggle _vibrationToggle;
        private Label _sensitivityLabel;

        // Gameplay
        private DropdownField _languageDropdown;
        private Toggle _tutorialsToggle;
        private Toggle _notificationsToggle;

        // Version
        private Label _versionLabel;

        private float _previousMusicVolume = 0.75f;
        private float _previousSfxVolume = 0.75f;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        /// <param name="saveService">Save service for settings persistence</param>
        public SettingsUI(ISaveService saveService)
        {
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
        }

        public void Initialize(VisualElement root)
        {
            Debug.Log("[SettingsUI] Initialize called");

            if (root == null)
            {
                Debug.LogError("[SettingsUI] Root is null!");
                return;
            }

            _root = root;

            Debug.Log($"[SettingsUI] Root element: {_root.name}");

            // Query all elements
            QueryElements();

            Debug.Log($"[SettingsUI] Elements found - Music: {_musicVolumeSlider != null}, SFX: {_sfxVolumeSlider != null}, Quality: {_qualityDropdown != null}");

            // Setup callbacks BEFORE loading settings
            InitializeDropdowns();
            SetupValueChangeCallbacks();
            SetupButtons();
            
            // Load settings AFTER callbacks are registered
            LoadSettings();
            UpdateVersionInfo();
            
            Debug.Log("[SettingsUI] Initialization complete");
        }

        private void QueryElements()
        {
            // Audio
            _musicVolumeSlider = _root.Q<Slider>("music-volume");
            _sfxVolumeSlider = _root.Q<Slider>("sfx-volume");
            _muteToggle = _root.Q<Toggle>("mute-toggle");
            _musicVolumeLabel = _root.Q<Label>("music-volume-value");
            _sfxVolumeLabel = _root.Q<Label>("sfx-volume-value");

            // Graphics
            _qualityDropdown = _root.Q<DropdownField>("quality-dropdown");
            _resolutionDropdown = _root.Q<DropdownField>("resolution-dropdown");
            _fullscreenToggle = _root.Q<Toggle>("fullscreen-toggle");
            _vsyncToggle = _root.Q<Toggle>("vsync-toggle");
            _fpsToggle = _root.Q<Toggle>("fps-toggle");

            // Controls
            _sensitivitySlider = _root.Q<Slider>("sensitivity-slider");
            _vibrationToggle = _root.Q<Toggle>("vibration-toggle");
            _sensitivityLabel = _root.Q<Label>("sensitivity-value");

            // Gameplay
            _languageDropdown = _root.Q<DropdownField>("language-dropdown");
            _tutorialsToggle = _root.Q<Toggle>("tutorials-toggle");
            _notificationsToggle = _root.Q<Toggle>("notifications-toggle");

            // Version
            _versionLabel = _root.Q<Label>("version-label");
        }

        private void InitializeDropdowns()
        {
            Debug.Log("[SettingsUI] Initializing dropdowns");
            
            // Quality dropdown
            if (_qualityDropdown != null)
            {
                string[] qualityNames = QualitySettings.names;
                _qualityDropdown.choices = new List<string>(qualityNames);
                _qualityDropdown.index = QualitySettings.GetQualityLevel();
                Debug.Log($"[SettingsUI] Quality dropdown initialized with {qualityNames.Length} options, current: {_qualityDropdown.index}");
            }
            else
            {
                Debug.LogWarning("[SettingsUI] Quality dropdown not found!");
            }

            // Resolution dropdown
            if (_resolutionDropdown != null)
            {
                Resolution[] resolutions = Screen.resolutions;
                List<string> resolutionStrings = new List<string>();
                int currentResolutionIndex = 0;

                for (int i = 0; i < resolutions.Length; i++)
                {
                    string resString = $"{resolutions[i].width} x {resolutions[i].height} @ {resolutions[i].refreshRate}Hz";
                    resolutionStrings.Add(resString);

                    if (resolutions[i].width == Screen.currentResolution.width &&
                        resolutions[i].height == Screen.currentResolution.height)
                    {
                        currentResolutionIndex = i;
                    }
                }

                _resolutionDropdown.choices = resolutionStrings;
                _resolutionDropdown.index = currentResolutionIndex;
                Debug.Log($"[SettingsUI] Resolution dropdown initialized with {resolutionStrings.Count} options, current: {currentResolutionIndex}");
            }
            else
            {
                Debug.LogWarning("[SettingsUI] Resolution dropdown not found!");
            }

            // Language dropdown
            if (_languageDropdown != null)
            {
                _languageDropdown.choices = new List<string>
                {
                    "English", "Español", "Français", "Deutsch", "Italiano",
                    "Português", "Русский", "日本語", "한국어", "中文"
                };
                var settings = _saveService.GetSettings();
                _languageDropdown.index = settings.LanguageIndex;
                Debug.Log($"[SettingsUI] Language dropdown initialized with 10 options, current: {settings.LanguageIndex}");
            }
            else
            {
                Debug.LogWarning("[SettingsUI] Language dropdown not found!");
            }
            
            Debug.Log("[SettingsUI] Dropdown initialization complete");
        }

        private void SetupButtons()
        {
            Debug.Log("[SettingsUI] Setting up buttons");
            
            // Control buttons
            Button editJoystickButton = _root.Q<Button>("edit-joystick-button");

            // Support buttons
            Button helpButton = _root.Q<Button>("help-button");
            Button supportButton = _root.Q<Button>("support-button");
            Button privacyButton = _root.Q<Button>("privacy-button");
            Button termsButton = _root.Q<Button>("terms-button");
            Button creditsButton = _root.Q<Button>("credits-button");
            Button parentGuideButton = _root.Q<Button>("parent-guide-button");

            // Account buttons
            Button logoutButton = _root.Q<Button>("logout-button");

            // Action buttons
            Button resetButton = _root.Q<Button>("reset-button");
            Button clearDataButton = _root.Q<Button>("clear-data-button");

            Debug.Log($"[SettingsUI] Buttons found - Joystick: {editJoystickButton != null}, Help: {helpButton != null}, Support: {supportButton != null}, Logout: {logoutButton != null}, Reset: {resetButton != null}");

            if (editJoystickButton != null)
            {
                editJoystickButton.clicked += OnEditJoystickClicked;
                Debug.Log("[SettingsUI] Edit joystick button listener added");
            }
            
            if (helpButton != null)
            {
                helpButton.clicked += OnHelpClicked;
                Debug.Log("[SettingsUI] Help button listener added");
            }
            
            if (supportButton != null)
            {
                supportButton.clicked += OnSupportClicked;
                Debug.Log("[SettingsUI] Support button listener added");
            }
            
            if (privacyButton != null) privacyButton.clicked += OnPrivacyClicked;
            if (termsButton != null) termsButton.clicked += OnTermsClicked;
            if (creditsButton != null) creditsButton.clicked += OnCreditsClicked;
            if (parentGuideButton != null) parentGuideButton.clicked += OnParentGuideClicked;
            
            if (logoutButton != null)
            {
                logoutButton.clicked += OnLogoutClicked;
                Debug.Log("[SettingsUI] Logout button listener added");
            }
            
            if (resetButton != null)
            {
                resetButton.clicked += OnResetClicked;
                Debug.Log("[SettingsUI] Reset button listener added");
            }
            
            if (clearDataButton != null)
            {
                clearDataButton.clicked += OnClearDataClicked;
                Debug.Log("[SettingsUI] Clear data button listener added");
            }
            
            Debug.Log("[SettingsUI] Button setup complete");
        }

        private void SetupValueChangeCallbacks()
        {
            Debug.Log("[SettingsUI] Setting up value change callbacks");
            
            // Audio callbacks
            if (_musicVolumeSlider != null)
            {
                _musicVolumeSlider.RegisterValueChangedCallback(evt =>
                {
                    Debug.Log($"[SettingsUI] Music volume changed to {evt.newValue}");
                    _previousMusicVolume = evt.newValue;
                    if (_muteToggle != null && !_muteToggle.value)
                    {
                        _saveService.UpdateSettings(s => s.MusicVolume = evt.newValue);
                        AudioListener.volume = evt.newValue;
                    }
                    UpdateVolumeLabel(_musicVolumeLabel, evt.newValue);
                });
                Debug.Log("[SettingsUI] Music volume callback registered");
            }

            if (_sfxVolumeSlider != null)
            {
                _sfxVolumeSlider.RegisterValueChangedCallback(evt =>
                {
                    Debug.Log($"[SettingsUI] SFX volume changed to {evt.newValue}");
                    _previousSfxVolume = evt.newValue;
                    _saveService.UpdateSettings(s => s.SFXVolume = evt.newValue);
                    UpdateVolumeLabel(_sfxVolumeLabel, evt.newValue);
                });
                Debug.Log("[SettingsUI] SFX volume callback registered");
            }

            if (_muteToggle != null)
            {
                _muteToggle.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                    {
                        AudioListener.volume = 0;
                    }
                    else
                    {
                        AudioListener.volume = _previousMusicVolume;
                    }
                    _saveService.UpdateSettings(s => s.IsMuted = evt.newValue);
                });
            }

            // Graphics callbacks
            if (_qualityDropdown != null)
            {
                _qualityDropdown.RegisterValueChangedCallback(evt =>
                {
                    QualitySettings.SetQualityLevel(_qualityDropdown.index);
                    _saveService.UpdateSettings(s => s.GraphicsQuality = _qualityDropdown.index);
                });
            }

            if (_resolutionDropdown != null)
            {
                _resolutionDropdown.RegisterValueChangedCallback(evt =>
                {
                    Resolution[] resolutions = Screen.resolutions;
                    if (_resolutionDropdown.index >= 0 && _resolutionDropdown.index < resolutions.Length)
                    {
                        Resolution res = resolutions[_resolutionDropdown.index];
                        Screen.SetResolution(res.width, res.height, Screen.fullScreen, res.refreshRate);
                        _saveService.UpdateSettings(s => s.ResolutionIndex = _resolutionDropdown.index);
                    }
                });
            }

            if (_fullscreenToggle != null)
            {
                _fullscreenToggle.RegisterValueChangedCallback(evt =>
                {
                    Screen.fullScreen = evt.newValue;
                    _saveService.UpdateSettings(s => s.IsFullscreen = evt.newValue);
                });
            }

            if (_vsyncToggle != null)
            {
                _vsyncToggle.RegisterValueChangedCallback(evt =>
                {
                    QualitySettings.vSyncCount = evt.newValue ? 1 : 0;
                    _saveService.UpdateSettings(s => s.IsVSyncEnabled = evt.newValue);
                });
            }

            if (_fpsToggle != null)
            {
                _fpsToggle.RegisterValueChangedCallback(evt =>
                {
                    _saveService.UpdateSettings(s => s.ShowFPS = evt.newValue);
                    // You can implement FPS counter display here
                });
            }

            // Controls callbacks
            if (_sensitivitySlider != null)
            {
                _sensitivitySlider.RegisterValueChangedCallback(evt =>
                {
                    _saveService.UpdateSettings(s => s.Sensitivity = evt.newValue);
                    UpdateSensitivityLabel(evt.newValue);
                });
            }

            if (_vibrationToggle != null)
            {
                _vibrationToggle.RegisterValueChangedCallback(evt =>
                {
                    _saveService.UpdateSettings(s => s.IsVibrationEnabled = evt.newValue);
                });
            }

            // Gameplay callbacks
            if (_languageDropdown != null)
            {
                _languageDropdown.RegisterValueChangedCallback(evt =>
                {
                    _saveService.UpdateSettings(s => s.LanguageIndex = _languageDropdown.index);
                    Debug.Log($"[SettingsUI] Language changed to: {_languageDropdown.value}");
                });
            }

            if (_tutorialsToggle != null)
            {
                _tutorialsToggle.RegisterValueChangedCallback(evt =>
                {
                    _saveService.UpdateSettings(s => s.ShowTutorials = evt.newValue);
                });
            }

            if (_notificationsToggle != null)
            {
                _notificationsToggle.RegisterValueChangedCallback(evt =>
                {
                    _saveService.UpdateSettings(s => s.NotificationsEnabled = evt.newValue);
                });
            }
        }

        private void LoadSettings()
        {
            var settings = _saveService.GetSettings();

            // Audio
            if (_musicVolumeSlider != null)
            {
                float musicVolume = settings.MusicVolume;
                _musicVolumeSlider.value = musicVolume;
                _previousMusicVolume = musicVolume;
                UpdateVolumeLabel(_musicVolumeLabel, musicVolume);
            }

            if (_sfxVolumeSlider != null)
            {
                float sfxVolume = settings.SFXVolume;
                _sfxVolumeSlider.value = sfxVolume;
                _previousSfxVolume = sfxVolume;
                UpdateVolumeLabel(_sfxVolumeLabel, sfxVolume);
            }

            if (_muteToggle != null)
                _muteToggle.value = settings.IsMuted;

            // Graphics
            if (_fullscreenToggle != null)
                _fullscreenToggle.value = settings.IsFullscreen;

            if (_vsyncToggle != null)
                _vsyncToggle.value = settings.IsVSyncEnabled;

            if (_fpsToggle != null)
                _fpsToggle.value = settings.ShowFPS;

            // Controls
            if (_sensitivitySlider != null)
            {
                float sensitivity = settings.Sensitivity;
                _sensitivitySlider.value = sensitivity;
                UpdateSensitivityLabel(sensitivity);
            }

            if (_vibrationToggle != null)
                _vibrationToggle.value = settings.IsVibrationEnabled;

            // Gameplay
            if (_tutorialsToggle != null)
                _tutorialsToggle.value = settings.ShowTutorials;

            if (_notificationsToggle != null)
                _notificationsToggle.value = settings.NotificationsEnabled;
        }

        private void UpdateVolumeLabel(Label label, float value)
        {
            if (label != null)
            {
                label.text = $"{Mathf.RoundToInt(value * 100)}%";
            }
        }

        private void UpdateSensitivityLabel(float value)
        {
            if (_sensitivityLabel != null)
            {
                _sensitivityLabel.text = $"{value:F1}x";
            }
        }

        private void UpdateVersionInfo()
        {
            if (_versionLabel != null)
            {
                _versionLabel.text = $"Version {Application.version}";
            }
        }

        private void OnEditJoystickClicked()
        {
            Debug.Log("[SettingsUI] Opening joystick editor");

            // Show the joystick editor screen using UIService
            var uiService = GameBootstrap.Services?.UIService;
            if (uiService != null && uiService.IsInitialized)
            {
                // Get the joystick editor screen
                JoystickEditorUI joystickEditor = uiService.GetScreen<JoystickEditorUI>();
                if (joystickEditor != null)
                {
                    joystickEditor.Show(true, true);
                }
                else
                {
                    Debug.LogWarning("[SettingsUI] JoystickEditorUI screen not found in UIService");
                }
            }
            else
            {
                Debug.LogWarning("[SettingsUI] UIService not available or not initialized");
            }
        }

        private void OnHelpClicked()
        {
            Debug.Log("[SettingsUI] Opening help");
            Application.OpenURL("https://yourwebsite.com/help");
        }

        private void OnSupportClicked()
        {
            Debug.Log("[SettingsUI] Opening support");
            Application.OpenURL("https://yourwebsite.com/support");
        }

        private void OnPrivacyClicked()
        {
            Debug.Log("[SettingsUI] Opening privacy policy");
            Application.OpenURL("https://yourwebsite.com/privacy");
        }

        private void OnTermsClicked()
        {
            Debug.Log("[SettingsUI] Opening terms and conditions");
            Application.OpenURL("https://yourwebsite.com/terms");
        }

        private void OnParentGuideClicked()
        {
            Debug.Log("[SettingsUI] Opening parent guide");
            Application.OpenURL("https://yourwebsite.com/parent-guide");
        }

        private void OnCreditsClicked()
        {
            Debug.Log("[SettingsUI] Opening credits");
            Application.OpenURL("https://yourwebsite.com/credits");
        }

        private void OnResetClicked()
        {
            Debug.Log("[SettingsUI] Resetting settings to defaults");

            // Reset all settings to defaults using SaveService
            _saveService.UpdateSettings(s =>
            {
                s.MusicVolume = 0.75f;
                s.SFXVolume = 0.75f;
                s.IsMuted = false;
                s.GraphicsQuality = 2;
                s.IsFullscreen = true;
                s.IsVSyncEnabled = true;
                s.ShowFPS = false;
                s.Sensitivity = 1.0f;
                s.IsVibrationEnabled = true;
                s.LanguageIndex = 0;
                s.ShowTutorials = true;
                s.NotificationsEnabled = true;
            });

            // Reload settings
            LoadSettings();

            // Apply graphics settings
            if (_qualityDropdown != null)
            {
                QualitySettings.SetQualityLevel(2);
                _qualityDropdown.index = 2;
            }

            Screen.fullScreen = true;
            QualitySettings.vSyncCount = 1;
            AudioListener.volume = 0.75f;

            Debug.Log("[SettingsUI] Settings reset complete");
        }

        private async void OnLogoutClicked()
        {
            Debug.Log("[SettingsUI] Logout button clicked");

            var authService = GameBootstrap.Services?.AuthenticationService;
            if (authService != null)
            {
                await authService.LogoutAsync();
                Debug.Log("[SettingsUI] User logged out successfully");
            }
            else
            {
                Debug.LogError("[SettingsUI] AuthenticationService not available");
            }
        }

        private void OnClearDataClicked()
        {
            Debug.Log("[SettingsUI] Clear data requested");

            // Show confirmation dialog (you can implement a proper dialog system)
            if (Application.isEditor)
            {
                Debug.LogWarning("[SettingsUI] Clear all data - This would delete all player progress!");
                // In a real implementation, show a confirmation dialog
                // For now, just log the action
            }
            else
            {
                Debug.LogWarning("[SettingsUI] Data clearing disabled in build - implement confirmation dialog first");
            }
        }
    }
}
