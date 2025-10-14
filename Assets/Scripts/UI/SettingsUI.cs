using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    /// <summary>
    /// Settings tab content - embedded directly in MainMenu tabs
    /// </summary>
    public class SettingsUI
    {
        private VisualElement _root;
        
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

            InitializeDropdowns();
            SetupButtons();
            LoadSettings();
            SetupValueChangeCallbacks();
            UpdateVersionInfo();
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
            // Quality dropdown
            if (_qualityDropdown != null)
            {
                string[] qualityNames = QualitySettings.names;
                _qualityDropdown.choices = new List<string>(qualityNames);
                _qualityDropdown.index = QualitySettings.GetQualityLevel();
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
            }

            // Language dropdown
            if (_languageDropdown != null)
            {
                _languageDropdown.choices = new List<string>
                {
                    "English", "Español", "Français", "Deutsch", "Italiano",
                    "Português", "Русский", "日本語", "한국어", "中文"
                };
                _languageDropdown.index = PlayerPrefs.GetInt("Language", 0);
            }
        }

        private void SetupButtons()
        {
            // Control buttons
            Button editJoystickButton = _root.Q<Button>("edit-joystick-button");
            
            // Support buttons
            Button helpButton = _root.Q<Button>("help-button");
            Button supportButton = _root.Q<Button>("support-button");
            Button privacyButton = _root.Q<Button>("privacy-button");
            Button termsButton = _root.Q<Button>("terms-button");
            Button creditsButton = _root.Q<Button>("credits-button");
            Button parentGuideButton = _root.Q<Button>("parent-guide-button");
            
            // Action buttons
            Button resetButton = _root.Q<Button>("reset-button");
            Button clearDataButton = _root.Q<Button>("clear-data-button");

            Debug.Log($"[SettingsUI] Buttons found - Joystick: {editJoystickButton != null}, Help: {helpButton != null}, Support: {supportButton != null}");

            if (editJoystickButton != null) 
            {
                editJoystickButton.clicked += OnEditJoystickClicked;
                Debug.Log("[SettingsUI] Edit joystick button listener added");
            }
            if (helpButton != null) helpButton.clicked += OnHelpClicked;
            if (supportButton != null) supportButton.clicked += OnSupportClicked;
            if (privacyButton != null) privacyButton.clicked += OnPrivacyClicked;
            if (termsButton != null) termsButton.clicked += OnTermsClicked;
            if (creditsButton != null) creditsButton.clicked += OnCreditsClicked;
            if (parentGuideButton != null) parentGuideButton.clicked += OnParentGuideClicked;
            if (resetButton != null) resetButton.clicked += OnResetClicked;
            if (clearDataButton != null) clearDataButton.clicked += OnClearDataClicked;
        }

        private void SetupValueChangeCallbacks()
        {
            // Audio callbacks
            if (_musicVolumeSlider != null)
            {
                _musicVolumeSlider.RegisterValueChangedCallback(evt =>
                {
                    _previousMusicVolume = evt.newValue;
                    if (_muteToggle != null && !_muteToggle.value)
                    {
                        PlayerPrefs.SetFloat("MusicVolume", evt.newValue);
                        AudioListener.volume = evt.newValue;
                    }
                    UpdateVolumeLabel(_musicVolumeLabel, evt.newValue);
                });
            }

            if (_sfxVolumeSlider != null)
            {
                _sfxVolumeSlider.RegisterValueChangedCallback(evt =>
                {
                    _previousSfxVolume = evt.newValue;
                    PlayerPrefs.SetFloat("SFXVolume", evt.newValue);
                    UpdateVolumeLabel(_sfxVolumeLabel, evt.newValue);
                });
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
                    PlayerPrefs.SetInt("Muted", evt.newValue ? 1 : 0);
                });
            }

            // Graphics callbacks
            if (_qualityDropdown != null)
            {
                _qualityDropdown.RegisterValueChangedCallback(evt =>
                {
                    QualitySettings.SetQualityLevel(_qualityDropdown.index);
                    PlayerPrefs.SetInt("Quality", _qualityDropdown.index);
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
                        PlayerPrefs.SetInt("ResolutionIndex", _resolutionDropdown.index);
                    }
                });
            }

            if (_fullscreenToggle != null)
            {
                _fullscreenToggle.RegisterValueChangedCallback(evt =>
                {
                    Screen.fullScreen = evt.newValue;
                    PlayerPrefs.SetInt("Fullscreen", evt.newValue ? 1 : 0);
                });
            }

            if (_vsyncToggle != null)
            {
                _vsyncToggle.RegisterValueChangedCallback(evt =>
                {
                    QualitySettings.vSyncCount = evt.newValue ? 1 : 0;
                    PlayerPrefs.SetInt("VSync", evt.newValue ? 1 : 0);
                });
            }

            if (_fpsToggle != null)
            {
                _fpsToggle.RegisterValueChangedCallback(evt =>
                {
                    PlayerPrefs.SetInt("ShowFPS", evt.newValue ? 1 : 0);
                    // You can implement FPS counter display here
                });
            }

            // Controls callbacks
            if (_sensitivitySlider != null)
            {
                _sensitivitySlider.RegisterValueChangedCallback(evt =>
                {
                    PlayerPrefs.SetFloat("Sensitivity", evt.newValue);
                    UpdateSensitivityLabel(evt.newValue);
                });
            }

            if (_vibrationToggle != null)
            {
                _vibrationToggle.RegisterValueChangedCallback(evt =>
                {
                    PlayerPrefs.SetInt("Vibration", evt.newValue ? 1 : 0);
                });
            }

            // Gameplay callbacks
            if (_languageDropdown != null)
            {
                _languageDropdown.RegisterValueChangedCallback(evt =>
                {
                    PlayerPrefs.SetInt("Language", _languageDropdown.index);
                    Debug.Log($"[SettingsUI] Language changed to: {_languageDropdown.value}");
                });
            }

            if (_tutorialsToggle != null)
            {
                _tutorialsToggle.RegisterValueChangedCallback(evt =>
                {
                    PlayerPrefs.SetInt("Tutorials", evt.newValue ? 1 : 0);
                });
            }

            if (_notificationsToggle != null)
            {
                _notificationsToggle.RegisterValueChangedCallback(evt =>
                {
                    PlayerPrefs.SetInt("Notifications", evt.newValue ? 1 : 0);
                });
            }
        }

        private void LoadSettings()
        {
            // Audio
            if (_musicVolumeSlider != null)
            {
                float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
                _musicVolumeSlider.value = musicVolume;
                _previousMusicVolume = musicVolume;
                UpdateVolumeLabel(_musicVolumeLabel, musicVolume);
            }

            if (_sfxVolumeSlider != null)
            {
                float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
                _sfxVolumeSlider.value = sfxVolume;
                _previousSfxVolume = sfxVolume;
                UpdateVolumeLabel(_sfxVolumeLabel, sfxVolume);
            }

            if (_muteToggle != null)
                _muteToggle.value = PlayerPrefs.GetInt("Muted", 0) == 1;

            // Graphics
            if (_fullscreenToggle != null)
                _fullscreenToggle.value = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

            if (_vsyncToggle != null)
                _vsyncToggle.value = PlayerPrefs.GetInt("VSync", 1) == 1;

            if (_fpsToggle != null)
                _fpsToggle.value = PlayerPrefs.GetInt("ShowFPS", 0) == 1;

            // Controls
            if (_sensitivitySlider != null)
            {
                float sensitivity = PlayerPrefs.GetFloat("Sensitivity", 1.0f);
                _sensitivitySlider.value = sensitivity;
                UpdateSensitivityLabel(sensitivity);
            }

            if (_vibrationToggle != null)
                _vibrationToggle.value = PlayerPrefs.GetInt("Vibration", 1) == 1;

            // Gameplay
            if (_tutorialsToggle != null)
                _tutorialsToggle.value = PlayerPrefs.GetInt("Tutorials", 1) == 1;

            if (_notificationsToggle != null)
                _notificationsToggle.value = PlayerPrefs.GetInt("Notifications", 1) == 1;
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
            
            // Show the joystick editor screen using UIManager
            UI.UISystem.UIManager uiManager = UI.UISystem.UIManager.Instance;
            if (uiManager != null && uiManager.IsInitialized)
            {
                // Get the joystick editor screen
                JoystickEditorUI joystickEditor = uiManager.GetScreen<JoystickEditorUI>();
                if (joystickEditor != null)
                {
                    joystickEditor.Show(true, true);
                }
                else
                {
                    Debug.LogWarning("[SettingsUI] JoystickEditorUI screen not found in UIManager");
                }
            }
            else
            {
                Debug.LogWarning("[SettingsUI] UIManager not available or not initialized");
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
            
            // Reset all settings to defaults
            PlayerPrefs.SetFloat("MusicVolume", 0.75f);
            PlayerPrefs.SetFloat("SFXVolume", 0.75f);
            PlayerPrefs.SetInt("Muted", 0);
            PlayerPrefs.SetInt("Quality", 2);
            PlayerPrefs.SetInt("Fullscreen", 1);
            PlayerPrefs.SetInt("VSync", 1);
            PlayerPrefs.SetInt("ShowFPS", 0);
            PlayerPrefs.SetFloat("Sensitivity", 1.0f);
            PlayerPrefs.SetInt("Vibration", 1);
            PlayerPrefs.SetInt("Language", 0);
            PlayerPrefs.SetInt("Tutorials", 1);
            PlayerPrefs.SetInt("Notifications", 1);
            PlayerPrefs.Save();
            
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
                // PlayerPrefs.DeleteAll();
                // PlayerPrefs.Save();
                Debug.LogWarning("[SettingsUI] Data clearing disabled in build - implement confirmation dialog first");
            }
        }
    }
}
