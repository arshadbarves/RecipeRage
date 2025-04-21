using System.Collections.Generic;
using UI.Animation;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Screens
{
    /// <summary>
    /// Settings screen
    /// </summary>
    public class SettingsScreen : UIScreen
    {
        // Header Elements
        private Button _backButton;
        
        // Audio Settings
        private Slider _masterVolumeSlider;
        private Slider _musicVolumeSlider;
        private Slider _sfxVolumeSlider;
        private Slider _voiceVolumeSlider;
        private Toggle _muteToggle;
        
        // Graphics Settings
        private DropdownField _qualityDropdown;
        private DropdownField _resolutionDropdown;
        private Toggle _fullscreenToggle;
        private Toggle _vsyncToggle;
        private Slider _brightnessSlider;
        
        // Gameplay Settings
        private Toggle _tutorialToggle;
        private Toggle _cameraShakeToggle;
        private Toggle _autoPickupToggle;
        private DropdownField _difficultyDropdown;
        
        // Controls Settings
        private Toggle _invertYToggle;
        private Slider _sensitivitySlider;
        private Toggle _vibrationToggle;
        private Button _resetControlsButton;
        
        // Account Settings
        private TextField _usernameField;
        private TextField _emailField;
        private Toggle _notificationsToggle;
        private Button _changePasswordButton;
        private Button _logoutButton;
        
        // Bottom Buttons
        private Button _resetAllButton;
        private Button _applyButton;
        
        // Settings values
        private Dictionary<string, float> _sliderValues = new Dictionary<string, float>();
        private Dictionary<string, bool> _toggleValues = new Dictionary<string, bool>();
        private Dictionary<string, string> _dropdownValues = new Dictionary<string, string>();
        private Dictionary<string, string> _textFieldValues = new Dictionary<string, string>();
        
        /// <summary>
        /// Initialize the settings screen
        /// </summary>
        protected override void InitializeScreen()
        {
            // Get references to UI elements
            GetUIReferences();
            
            // Set up button listeners
            SetupButtonListeners();
            
            // Initialize dropdown options
            InitializeDropdowns();
            
            // Set initial values
            LoadSettings();
        }
        
        /// <summary>
        /// Get references to UI elements
        /// </summary>
        private void GetUIReferences()
        {
            // Header Elements
            _backButton = _root.Q<Button>("back-button");
            
            // Audio Settings
            _masterVolumeSlider = _root.Q<Slider>("master-volume-slider");
            _musicVolumeSlider = _root.Q<Slider>("music-volume-slider");
            _sfxVolumeSlider = _root.Q<Slider>("sfx-volume-slider");
            _voiceVolumeSlider = _root.Q<Slider>("voice-volume-slider");
            _muteToggle = _root.Q<Toggle>("mute-toggle");
            
            // Graphics Settings
            _qualityDropdown = _root.Q<DropdownField>("quality-dropdown");
            _resolutionDropdown = _root.Q<DropdownField>("resolution-dropdown");
            _fullscreenToggle = _root.Q<Toggle>("fullscreen-toggle");
            _vsyncToggle = _root.Q<Toggle>("vsync-toggle");
            _brightnessSlider = _root.Q<Slider>("brightness-slider");
            
            // Gameplay Settings
            _tutorialToggle = _root.Q<Toggle>("tutorial-toggle");
            _cameraShakeToggle = _root.Q<Toggle>("camera-shake-toggle");
            _autoPickupToggle = _root.Q<Toggle>("auto-pickup-toggle");
            _difficultyDropdown = _root.Q<DropdownField>("difficulty-dropdown");
            
            // Controls Settings
            _invertYToggle = _root.Q<Toggle>("invert-y-toggle");
            _sensitivitySlider = _root.Q<Slider>("sensitivity-slider");
            _vibrationToggle = _root.Q<Toggle>("vibration-toggle");
            _resetControlsButton = _root.Q<Button>("reset-controls-button");
            
            // Account Settings
            _usernameField = _root.Q<TextField>("username-field");
            _emailField = _root.Q<TextField>("email-field");
            _notificationsToggle = _root.Q<Toggle>("notifications-toggle");
            _changePasswordButton = _root.Q<Button>("change-password-button");
            _logoutButton = _root.Q<Button>("logout-button");
            
            // Bottom Buttons
            _resetAllButton = _root.Q<Button>("reset-all-button");
            _applyButton = _root.Q<Button>("apply-button");
        }
        
        /// <summary>
        /// Set up button listeners
        /// </summary>
        private void SetupButtonListeners()
        {
            // Back button
            if (_backButton != null)
            {
                _backButton.clicked += OnBackButtonClicked;
            }
            
            // Reset controls button
            if (_resetControlsButton != null)
            {
                _resetControlsButton.clicked += OnResetControlsButtonClicked;
            }
            
            // Change password button
            if (_changePasswordButton != null)
            {
                _changePasswordButton.clicked += OnChangePasswordButtonClicked;
            }
            
            // Logout button
            if (_logoutButton != null)
            {
                _logoutButton.clicked += OnLogoutButtonClicked;
            }
            
            // Reset all button
            if (_resetAllButton != null)
            {
                _resetAllButton.clicked += OnResetAllButtonClicked;
            }
            
            // Apply button
            if (_applyButton != null)
            {
                _applyButton.clicked += OnApplyButtonClicked;
            }
            
            // Set up slider value change listeners
            SetupSliderListeners();
            
            // Set up toggle value change listeners
            SetupToggleListeners();
            
            // Set up dropdown value change listeners
            SetupDropdownListeners();
            
            // Set up text field value change listeners
            SetupTextFieldListeners();
        }
        
        /// <summary>
        /// Initialize dropdown options
        /// </summary>
        private void InitializeDropdowns()
        {
            // Quality dropdown
            if (_qualityDropdown != null)
            {
                _qualityDropdown.choices = new List<string>
                {
                    "Low",
                    "Medium",
                    "High",
                    "Ultra"
                };
                _qualityDropdown.index = 2; // Default to High
            }
            
            // Resolution dropdown
            if (_resolutionDropdown != null)
            {
                _resolutionDropdown.choices = new List<string>
                {
                    "1280x720",
                    "1920x1080",
                    "2560x1440",
                    "3840x2160"
                };
                _resolutionDropdown.index = 1; // Default to 1080p
            }
            
            // Difficulty dropdown
            if (_difficultyDropdown != null)
            {
                _difficultyDropdown.choices = new List<string>
                {
                    "Easy",
                    "Normal",
                    "Hard",
                    "Expert"
                };
                _difficultyDropdown.index = 1; // Default to Normal
            }
        }
        
        /// <summary>
        /// Set up slider value change listeners
        /// </summary>
        private void SetupSliderListeners()
        {
            // Master volume slider
            if (_masterVolumeSlider != null)
            {
                _masterVolumeSlider.RegisterValueChangedCallback(evt => {
                    _sliderValues["MasterVolume"] = evt.newValue;
                    UpdateSliderValueLabel(_masterVolumeSlider, evt.newValue);
                });
            }
            
            // Music volume slider
            if (_musicVolumeSlider != null)
            {
                _musicVolumeSlider.RegisterValueChangedCallback(evt => {
                    _sliderValues["MusicVolume"] = evt.newValue;
                    UpdateSliderValueLabel(_musicVolumeSlider, evt.newValue);
                });
            }
            
            // SFX volume slider
            if (_sfxVolumeSlider != null)
            {
                _sfxVolumeSlider.RegisterValueChangedCallback(evt => {
                    _sliderValues["SFXVolume"] = evt.newValue;
                    UpdateSliderValueLabel(_sfxVolumeSlider, evt.newValue);
                });
            }
            
            // Voice volume slider
            if (_voiceVolumeSlider != null)
            {
                _voiceVolumeSlider.RegisterValueChangedCallback(evt => {
                    _sliderValues["VoiceVolume"] = evt.newValue;
                    UpdateSliderValueLabel(_voiceVolumeSlider, evt.newValue);
                });
            }
            
            // Brightness slider
            if (_brightnessSlider != null)
            {
                _brightnessSlider.RegisterValueChangedCallback(evt => {
                    _sliderValues["Brightness"] = evt.newValue;
                    UpdateSliderValueLabel(_brightnessSlider, evt.newValue);
                });
            }
            
            // Sensitivity slider
            if (_sensitivitySlider != null)
            {
                _sensitivitySlider.RegisterValueChangedCallback(evt => {
                    _sliderValues["Sensitivity"] = evt.newValue;
                    UpdateSliderValueLabel(_sensitivitySlider, evt.newValue);
                });
            }
        }
        
        /// <summary>
        /// Set up toggle value change listeners
        /// </summary>
        private void SetupToggleListeners()
        {
            // Mute toggle
            if (_muteToggle != null)
            {
                _muteToggle.RegisterValueChangedCallback(evt => {
                    _toggleValues["Mute"] = evt.newValue;
                });
            }
            
            // Fullscreen toggle
            if (_fullscreenToggle != null)
            {
                _fullscreenToggle.RegisterValueChangedCallback(evt => {
                    _toggleValues["Fullscreen"] = evt.newValue;
                });
            }
            
            // VSync toggle
            if (_vsyncToggle != null)
            {
                _vsyncToggle.RegisterValueChangedCallback(evt => {
                    _toggleValues["VSync"] = evt.newValue;
                });
            }
            
            // Tutorial toggle
            if (_tutorialToggle != null)
            {
                _tutorialToggle.RegisterValueChangedCallback(evt => {
                    _toggleValues["Tutorial"] = evt.newValue;
                });
            }
            
            // Camera shake toggle
            if (_cameraShakeToggle != null)
            {
                _cameraShakeToggle.RegisterValueChangedCallback(evt => {
                    _toggleValues["CameraShake"] = evt.newValue;
                });
            }
            
            // Auto pickup toggle
            if (_autoPickupToggle != null)
            {
                _autoPickupToggle.RegisterValueChangedCallback(evt => {
                    _toggleValues["AutoPickup"] = evt.newValue;
                });
            }
            
            // Invert Y toggle
            if (_invertYToggle != null)
            {
                _invertYToggle.RegisterValueChangedCallback(evt => {
                    _toggleValues["InvertY"] = evt.newValue;
                });
            }
            
            // Vibration toggle
            if (_vibrationToggle != null)
            {
                _vibrationToggle.RegisterValueChangedCallback(evt => {
                    _toggleValues["Vibration"] = evt.newValue;
                });
            }
            
            // Notifications toggle
            if (_notificationsToggle != null)
            {
                _notificationsToggle.RegisterValueChangedCallback(evt => {
                    _toggleValues["Notifications"] = evt.newValue;
                });
            }
        }
        
        /// <summary>
        /// Set up dropdown value change listeners
        /// </summary>
        private void SetupDropdownListeners()
        {
            // Quality dropdown
            if (_qualityDropdown != null)
            {
                _qualityDropdown.RegisterValueChangedCallback(evt => {
                    _dropdownValues["Quality"] = evt.newValue;
                });
            }
            
            // Resolution dropdown
            if (_resolutionDropdown != null)
            {
                _resolutionDropdown.RegisterValueChangedCallback(evt => {
                    _dropdownValues["Resolution"] = evt.newValue;
                });
            }
            
            // Difficulty dropdown
            if (_difficultyDropdown != null)
            {
                _difficultyDropdown.RegisterValueChangedCallback(evt => {
                    _dropdownValues["Difficulty"] = evt.newValue;
                });
            }
        }
        
        /// <summary>
        /// Set up text field value change listeners
        /// </summary>
        private void SetupTextFieldListeners()
        {
            // Username field
            if (_usernameField != null)
            {
                _usernameField.RegisterValueChangedCallback(evt => {
                    _textFieldValues["Username"] = evt.newValue;
                });
            }
            
            // Email field
            if (_emailField != null)
            {
                _emailField.RegisterValueChangedCallback(evt => {
                    _textFieldValues["Email"] = evt.newValue;
                });
            }
        }
        
        /// <summary>
        /// Update the value label for a slider
        /// </summary>
        /// <param name="slider">The slider</param>
        /// <param name="value">The new value</param>
        private void UpdateSliderValueLabel(Slider slider, float value)
        {
            // Find the parent row
            VisualElement row = slider.parent;
            if (row == null) return;
            
            // Find the value label
            Label valueLabel = row.Q<Label>("setting-value");
            if (valueLabel == null) return;
            
            // Update the label text
            valueLabel.text = $"{Mathf.RoundToInt(value)}%";
        }
        
        /// <summary>
        /// Load settings from PlayerPrefs
        /// </summary>
        private void LoadSettings()
        {
            // Load slider values
            _sliderValues["MasterVolume"] = PlayerPrefs.GetFloat("MasterVolume", 80f);
            _sliderValues["MusicVolume"] = PlayerPrefs.GetFloat("MusicVolume", 70f);
            _sliderValues["SFXVolume"] = PlayerPrefs.GetFloat("SFXVolume", 90f);
            _sliderValues["VoiceVolume"] = PlayerPrefs.GetFloat("VoiceVolume", 85f);
            _sliderValues["Brightness"] = PlayerPrefs.GetFloat("Brightness", 50f);
            _sliderValues["Sensitivity"] = PlayerPrefs.GetFloat("Sensitivity", 65f);
            
            // Load toggle values
            _toggleValues["Mute"] = PlayerPrefs.GetInt("Mute", 0) == 1;
            _toggleValues["Fullscreen"] = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
            _toggleValues["VSync"] = PlayerPrefs.GetInt("VSync", 1) == 1;
            _toggleValues["Tutorial"] = PlayerPrefs.GetInt("Tutorial", 1) == 1;
            _toggleValues["CameraShake"] = PlayerPrefs.GetInt("CameraShake", 1) == 1;
            _toggleValues["AutoPickup"] = PlayerPrefs.GetInt("AutoPickup", 0) == 1;
            _toggleValues["InvertY"] = PlayerPrefs.GetInt("InvertY", 0) == 1;
            _toggleValues["Vibration"] = PlayerPrefs.GetInt("Vibration", 1) == 1;
            _toggleValues["Notifications"] = PlayerPrefs.GetInt("Notifications", 1) == 1;
            
            // Load dropdown values
            _dropdownValues["Quality"] = PlayerPrefs.GetString("Quality", "High");
            _dropdownValues["Resolution"] = PlayerPrefs.GetString("Resolution", "1920x1080");
            _dropdownValues["Difficulty"] = PlayerPrefs.GetString("Difficulty", "Normal");
            
            // Load text field values
            _textFieldValues["Username"] = PlayerPrefs.GetString("Username", "Chef123");
            _textFieldValues["Email"] = PlayerPrefs.GetString("Email", "chef123@example.com");
            
            // Apply loaded values to UI elements
            ApplySettingsToUI();
        }
        
        /// <summary>
        /// Apply settings to UI elements
        /// </summary>
        private void ApplySettingsToUI()
        {
            // Apply slider values
            if (_masterVolumeSlider != null)
            {
                _masterVolumeSlider.value = _sliderValues["MasterVolume"];
                UpdateSliderValueLabel(_masterVolumeSlider, _sliderValues["MasterVolume"]);
            }
            
            if (_musicVolumeSlider != null)
            {
                _musicVolumeSlider.value = _sliderValues["MusicVolume"];
                UpdateSliderValueLabel(_musicVolumeSlider, _sliderValues["MusicVolume"]);
            }
            
            if (_sfxVolumeSlider != null)
            {
                _sfxVolumeSlider.value = _sliderValues["SFXVolume"];
                UpdateSliderValueLabel(_sfxVolumeSlider, _sliderValues["SFXVolume"]);
            }
            
            if (_voiceVolumeSlider != null)
            {
                _voiceVolumeSlider.value = _sliderValues["VoiceVolume"];
                UpdateSliderValueLabel(_voiceVolumeSlider, _sliderValues["VoiceVolume"]);
            }
            
            if (_brightnessSlider != null)
            {
                _brightnessSlider.value = _sliderValues["Brightness"];
                UpdateSliderValueLabel(_brightnessSlider, _sliderValues["Brightness"]);
            }
            
            if (_sensitivitySlider != null)
            {
                _sensitivitySlider.value = _sliderValues["Sensitivity"];
                UpdateSliderValueLabel(_sensitivitySlider, _sliderValues["Sensitivity"]);
            }
            
            // Apply toggle values
            if (_muteToggle != null) _muteToggle.value = _toggleValues["Mute"];
            if (_fullscreenToggle != null) _fullscreenToggle.value = _toggleValues["Fullscreen"];
            if (_vsyncToggle != null) _vsyncToggle.value = _toggleValues["VSync"];
            if (_tutorialToggle != null) _tutorialToggle.value = _toggleValues["Tutorial"];
            if (_cameraShakeToggle != null) _cameraShakeToggle.value = _toggleValues["CameraShake"];
            if (_autoPickupToggle != null) _autoPickupToggle.value = _toggleValues["AutoPickup"];
            if (_invertYToggle != null) _invertYToggle.value = _toggleValues["InvertY"];
            if (_vibrationToggle != null) _vibrationToggle.value = _toggleValues["Vibration"];
            if (_notificationsToggle != null) _notificationsToggle.value = _toggleValues["Notifications"];
            
            // Apply dropdown values
            if (_qualityDropdown != null)
            {
                int index = _qualityDropdown.choices.IndexOf(_dropdownValues["Quality"]);
                if (index >= 0) _qualityDropdown.index = index;
            }
            
            if (_resolutionDropdown != null)
            {
                int index = _resolutionDropdown.choices.IndexOf(_dropdownValues["Resolution"]);
                if (index >= 0) _resolutionDropdown.index = index;
            }
            
            if (_difficultyDropdown != null)
            {
                int index = _difficultyDropdown.choices.IndexOf(_dropdownValues["Difficulty"]);
                if (index >= 0) _difficultyDropdown.index = index;
            }
            
            // Apply text field values
            if (_usernameField != null) _usernameField.value = _textFieldValues["Username"];
            if (_emailField != null) _emailField.value = _textFieldValues["Email"];
        }
        
        /// <summary>
        /// Save settings to PlayerPrefs
        /// </summary>
        private void SaveSettings()
        {
            // Save slider values
            foreach (var pair in _sliderValues)
            {
                PlayerPrefs.SetFloat(pair.Key, pair.Value);
            }
            
            // Save toggle values
            foreach (var pair in _toggleValues)
            {
                PlayerPrefs.SetInt(pair.Key, pair.Value ? 1 : 0);
            }
            
            // Save dropdown values
            foreach (var pair in _dropdownValues)
            {
                PlayerPrefs.SetString(pair.Key, pair.Value);
            }
            
            // Save text field values
            foreach (var pair in _textFieldValues)
            {
                PlayerPrefs.SetString(pair.Key, pair.Value);
            }
            
            // Save changes
            PlayerPrefs.Save();
            
            // Apply settings to the game
            ApplySettingsToGame();
        }
        
        /// <summary>
        /// Apply settings to the game
        /// </summary>
        private void ApplySettingsToGame()
        {
            // TODO: Apply settings to the game
            // This would include things like:
            // - Setting audio volumes
            // - Applying graphics settings
            // - Updating gameplay preferences
            // - Adjusting control settings
            
            Debug.Log("[SettingsScreen] Settings applied to game");
        }
        
        /// <summary>
        /// Reset settings to defaults
        /// </summary>
        private void ResetSettings()
        {
            // Reset slider values
            _sliderValues["MasterVolume"] = 80f;
            _sliderValues["MusicVolume"] = 70f;
            _sliderValues["SFXVolume"] = 90f;
            _sliderValues["VoiceVolume"] = 85f;
            _sliderValues["Brightness"] = 50f;
            _sliderValues["Sensitivity"] = 65f;
            
            // Reset toggle values
            _toggleValues["Mute"] = false;
            _toggleValues["Fullscreen"] = true;
            _toggleValues["VSync"] = true;
            _toggleValues["Tutorial"] = true;
            _toggleValues["CameraShake"] = true;
            _toggleValues["AutoPickup"] = false;
            _toggleValues["InvertY"] = false;
            _toggleValues["Vibration"] = true;
            _toggleValues["Notifications"] = true;
            
            // Reset dropdown values
            _dropdownValues["Quality"] = "High";
            _dropdownValues["Resolution"] = "1920x1080";
            _dropdownValues["Difficulty"] = "Normal";
            
            // Apply reset values to UI
            ApplySettingsToUI();
        }
        
        /// <summary>
        /// Reset control settings to defaults
        /// </summary>
        private void ResetControlSettings()
        {
            // Reset control-related settings
            _sliderValues["Sensitivity"] = 65f;
            _toggleValues["InvertY"] = false;
            _toggleValues["Vibration"] = true;
            
            // Apply reset values to UI
            if (_sensitivitySlider != null)
            {
                _sensitivitySlider.value = _sliderValues["Sensitivity"];
                UpdateSliderValueLabel(_sensitivitySlider, _sliderValues["Sensitivity"]);
            }
            
            if (_invertYToggle != null) _invertYToggle.value = _toggleValues["InvertY"];
            if (_vibrationToggle != null) _vibrationToggle.value = _toggleValues["Vibration"];
        }
        
        /// <summary>
        /// Show the settings screen with animations
        /// </summary>
        /// <param name="animate">Whether to animate the transition</param>
        public override void Show(bool animate = true)
        {
            base.Show(animate);
            
            if (animate && _container != null)
            {
                // Animate UI elements
                AnimateUIElements();
            }
        }
        
        /// <summary>
        /// Animate UI elements when showing the screen
        /// </summary>
        private void AnimateUIElements()
        {
            // Reset elements
            var header = _root.Q<VisualElement>("header");
            var settingsContainer = _root.Q<VisualElement>("settings-container");
            var bottomButtons = _root.Q<VisualElement>("bottom-buttons");
            
            header.style.opacity = 0;
            header.transform.position = new Vector2(0, -50);
            
            settingsContainer.style.opacity = 0;
            
            bottomButtons.style.opacity = 0;
            bottomButtons.transform.position = new Vector2(0, 50);
            
            // Animate header
            UIAnimationSystem.Instance.Animate(
                header,
                UIAnimationSystem.AnimationType.FadeIn,
                0.5f,
                0.2f,
                UIAnimationSystem.EasingType.EaseOutCubic
            );
            
            // Animate settings container
            UIAnimationSystem.Instance.Animate(
                settingsContainer,
                UIAnimationSystem.AnimationType.FadeIn,
                0.5f,
                0.4f,
                UIAnimationSystem.EasingType.EaseOutCubic
            );
            
            // Animate bottom buttons
            UIAnimationSystem.Instance.Animate(
                bottomButtons,
                UIAnimationSystem.AnimationType.FadeIn,
                0.5f,
                0.6f,
                UIAnimationSystem.EasingType.EaseOutCubic
            );
            
            // Animate settings sections
            var sections = _root.Query<VisualElement>("settings-section").ToList();
            for (int i = 0; i < sections.Count; i++)
            {
                float delay = 0.5f + (i * 0.1f);
                UIAnimationSystem.Instance.Animate(
                    sections[i],
                    UIAnimationSystem.AnimationType.SlideInFromLeft,
                    0.5f,
                    delay,
                    UIAnimationSystem.EasingType.EaseOutCubic
                );
            }
        }
        
        #region Button Handlers
        
        /// <summary>
        /// Handle back button click
        /// </summary>
        private void OnBackButtonClicked()
        {
            Debug.Log("[SettingsScreen] Back button clicked");
            
            // Hide this screen
            Hide(true);
            
            // Show main menu screen
            UIManager.Instance.ShowScreen<MainMenuScreen>(true);
        }
        
        /// <summary>
        /// Handle reset controls button click
        /// </summary>
        private void OnResetControlsButtonClicked()
        {
            Debug.Log("[SettingsScreen] Reset controls button clicked");
            
            // Reset control settings
            ResetControlSettings();
            
            // Animate button
            UIAnimationSystem.Instance.Animate(
                _resetControlsButton,
                UIAnimationSystem.AnimationType.Pulse,
                0.5f,
                0f,
                UIAnimationSystem.EasingType.EaseOutElastic
            );
        }
        
        /// <summary>
        /// Handle change password button click
        /// </summary>
        private void OnChangePasswordButtonClicked()
        {
            Debug.Log("[SettingsScreen] Change password button clicked");
            
            // TODO: Show change password dialog
            
            // Animate button
            UIAnimationSystem.Instance.Animate(
                _changePasswordButton,
                UIAnimationSystem.AnimationType.Pulse,
                0.5f,
                0f,
                UIAnimationSystem.EasingType.EaseOutElastic
            );
        }
        
        /// <summary>
        /// Handle logout button click
        /// </summary>
        private void OnLogoutButtonClicked()
        {
            Debug.Log("[SettingsScreen] Logout button clicked");
            
            // TODO: Implement logout logic
            
            // Animate button
            UIAnimationSystem.Instance.Animate(
                _logoutButton,
                UIAnimationSystem.AnimationType.Pulse,
                0.5f,
                0f,
                UIAnimationSystem.EasingType.EaseOutElastic
            );
        }
        
        /// <summary>
        /// Handle reset all button click
        /// </summary>
        private void OnResetAllButtonClicked()
        {
            Debug.Log("[SettingsScreen] Reset all button clicked");
            
            // Reset all settings
            ResetSettings();
            
            // Animate button
            UIAnimationSystem.Instance.Animate(
                _resetAllButton,
                UIAnimationSystem.AnimationType.Pulse,
                0.5f,
                0f,
                UIAnimationSystem.EasingType.EaseOutElastic
            );
        }
        
        /// <summary>
        /// Handle apply button click
        /// </summary>
        private void OnApplyButtonClicked()
        {
            Debug.Log("[SettingsScreen] Apply button clicked");
            
            // Save settings
            SaveSettings();
            
            // Animate button
            UIAnimationSystem.Instance.Animate(
                _applyButton,
                UIAnimationSystem.AnimationType.Pulse,
                0.5f,
                0f,
                UIAnimationSystem.EasingType.EaseOutElastic
            );
        }
        
        #endregion
    }
}
