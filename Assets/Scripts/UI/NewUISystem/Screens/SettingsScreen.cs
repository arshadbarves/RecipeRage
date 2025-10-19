using System;
using Core.Animation;
using Core.Bootstrap;
using Core.SaveSystem;
using UI.UISystem.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.UISystem.Screens
{
    /// <summary>
    /// Settings screen for game configuration
    /// Pure C# implementation with programmatic configuration
    /// Uses SaveService instead of PlayerPrefs for persistence
    /// </summary>
    [UIScreen(UIScreenType.Settings, UIScreenPriority.Settings, "SettingsTemplate")]
    public class SettingsScreen : BaseUIScreen
    {
        #region Configuration Properties

        public float MasterVolume { get; set; } = 0.8f;
        public float MusicVolume { get; set; } = 0.7f;
        public float SFXVolume { get; set; } = 0.9f;
        public bool Fullscreen { get; set; } = true;
        public bool VSync { get; set; } = true;

        // Default values for reset functionality
        public float DefaultMasterVolume { get; set; } = 0.8f;
        public float DefaultMusicVolume { get; set; } = 0.7f;
        public float DefaultSFXVolume { get; set; } = 0.9f;
        public bool DefaultFullscreen { get; set; } = true;
        public bool DefaultVSync { get; set; } = true;

        #endregion

        #region UI Elements

        private Button _closeButton;
        private Button _resetButton;
        private Button _applyButton;
        private Button _logoutButton;

        // Audio controls
        private Slider _masterVolumeSlider;
        private Slider _musicVolumeSlider;
        private Slider _sfxVolumeSlider;

        // Graphics controls
        private Toggle _fullscreenToggle;
        private Toggle _vsyncToggle;
        
        // Account controls
        private TextField _playerNameField;

        #endregion

        #region Events

        public event Action<SettingsData> OnSettingsApplied;
        public event Action OnSettingsReset;
        public event Action OnSettingsClosed;
        public event Action OnLogoutRequested;
        public event Action<float> OnMasterVolumeChanged;
        public event Action<float> OnMusicVolumeChanged;
        public event Action<float> OnSFXVolumeChanged;
        public event Action<bool> OnFullscreenChanged;
        public event Action<bool> OnVSyncChanged;

        #endregion

        #region Lifecycle

        protected override void OnInitialize()
        {
            CacheUIElements();
            SetupEventHandlers();
            LoadCurrentSettings();
            
            Debug.Log("[SettingsScreen] Initialized with pure C# implementation");
        }

        protected override void OnShow()
        {
            LoadCurrentSettings();
            UpdateUIFromSettings();
        }

        protected override void OnDispose()
        {
            UnregisterEventHandlers();
        }

        #endregion

        #region UI Setup

        private void CacheUIElements()
        {
            // Buttons
            _closeButton = GetElement<Button>("close-button");
            _resetButton = GetElement<Button>("reset-button");
            _applyButton = GetElement<Button>("apply-button");
            _logoutButton = GetElement<Button>("logout-button");

            // Audio sliders
            _masterVolumeSlider = GetElement<Slider>("master-volume-slider");
            _musicVolumeSlider = GetElement<Slider>("music-volume-slider");
            _sfxVolumeSlider = GetElement<Slider>("sfx-volume-slider");

            // Graphics toggles
            _fullscreenToggle = GetElement<Toggle>("fullscreen-toggle");
            _vsyncToggle = GetElement<Toggle>("vsync-toggle");
            
            // Account controls
            _playerNameField = GetElement<TextField>("player-name-field");

            // Log missing elements for debugging
            if (_closeButton == null)
            {
                Debug.LogWarning("[SettingsScreen] close-button not found in template");
            }
            if (_masterVolumeSlider == null)
            {
                Debug.LogWarning("[SettingsScreen] master-volume-slider not found in template");
            }
        }

        private void SetupEventHandlers()
        {
            // Button events
            _closeButton?.RegisterCallback<ClickEvent>(_ => HandleCloseClicked());
            _resetButton?.RegisterCallback<ClickEvent>(_ => HandleResetClicked());
            _applyButton?.RegisterCallback<ClickEvent>(_ => HandleApplyClicked());
            _logoutButton?.RegisterCallback<ClickEvent>(_ => HandleLogoutClicked());

            // Slider events
            _masterVolumeSlider?.RegisterValueChangedCallback(evt => HandleMasterVolumeChanged(evt.newValue));
            _musicVolumeSlider?.RegisterValueChangedCallback(evt => HandleMusicVolumeChanged(evt.newValue));
            _sfxVolumeSlider?.RegisterValueChangedCallback(evt => HandleSFXVolumeChanged(evt.newValue));

            // Toggle events
            _fullscreenToggle?.RegisterValueChangedCallback(evt => HandleFullscreenChanged(evt.newValue));
            _vsyncToggle?.RegisterValueChangedCallback(evt => HandleVSyncChanged(evt.newValue));
        }

        private void UnregisterEventHandlers()
        {
            _closeButton?.UnregisterCallback<ClickEvent>(_ => HandleCloseClicked());
            _resetButton?.UnregisterCallback<ClickEvent>(_ => HandleResetClicked());
            _applyButton?.UnregisterCallback<ClickEvent>(_ => HandleApplyClicked());
            _logoutButton?.UnregisterCallback<ClickEvent>(_ => HandleLogoutClicked());

            _masterVolumeSlider?.UnregisterValueChangedCallback(evt => HandleMasterVolumeChanged(evt.newValue));
            _musicVolumeSlider?.UnregisterValueChangedCallback(evt => HandleMusicVolumeChanged(evt.newValue));
            _sfxVolumeSlider?.UnregisterValueChangedCallback(evt => HandleSFXVolumeChanged(evt.newValue));

            _fullscreenToggle?.UnregisterValueChangedCallback(evt => HandleFullscreenChanged(evt.newValue));
            _vsyncToggle?.UnregisterValueChangedCallback(evt => HandleVSyncChanged(evt.newValue));
        }

        #endregion

        #region Public API

        /// <summary>
        /// Configure settings with custom values
        /// </summary>
        public SettingsScreen ConfigureSettings(float masterVolume, float musicVolume, float sfxVolume, bool fullscreen, bool vSync)
        {
            MasterVolume = Mathf.Clamp01(masterVolume);
            MusicVolume = Mathf.Clamp01(musicVolume);
            SFXVolume = Mathf.Clamp01(sfxVolume);
            Fullscreen = fullscreen;
            VSync = vSync;

            UpdateUIFromSettings();
            return this;
        }

        /// <summary>
        /// Set default values for reset functionality
        /// </summary>
        public SettingsScreen SetDefaults(float masterVolume, float musicVolume, float sfxVolume, bool fullscreen, bool vSync)
        {
            DefaultMasterVolume = Mathf.Clamp01(masterVolume);
            DefaultMusicVolume = Mathf.Clamp01(musicVolume);
            DefaultSFXVolume = Mathf.Clamp01(sfxVolume);
            DefaultFullscreen = fullscreen;
            DefaultVSync = vSync;
            return this;
        }

        /// <summary>
        /// Get current settings as data structure
        /// </summary>
        public SettingsData GetCurrentSettings()
        {
            return new SettingsData
            {
                masterVolume = MasterVolume,
                musicVolume = MusicVolume,
                sfxVolume = SFXVolume,
                fullscreen = Fullscreen,
                vSync = VSync
            };
        }

        /// <summary>
        /// Set settings from data structure
        /// </summary>
        public SettingsScreen SetSettings(SettingsData settings)
        {
            MasterVolume = settings.masterVolume;
            MusicVolume = settings.musicVolume;
            SFXVolume = settings.sfxVolume;
            Fullscreen = settings.fullscreen;
            VSync = settings.vSync;

            UpdateUIFromSettings();
            return this;
        }

        /// <summary>
        /// Reset settings to defaults
        /// </summary>
        public SettingsScreen ResetToDefaults()
        {
            MasterVolume = DefaultMasterVolume;
            MusicVolume = DefaultMusicVolume;
            SFXVolume = DefaultSFXVolume;
            Fullscreen = DefaultFullscreen;
            VSync = DefaultVSync;

            UpdateUIFromSettings();
            OnSettingsReset?.Invoke();
            return this;
        }

        /// <summary>
        /// Apply current settings
        /// </summary>
        public SettingsScreen ApplySettings()
        {
            SaveSettings();
            ApplyGraphicsSettings();
            ApplyAudioSettings();

            SettingsData settingsData = GetCurrentSettings();
            OnSettingsApplied?.Invoke(settingsData);

            Debug.Log("[SettingsScreen] Settings applied and saved");
            return this;
        }

        #endregion

        #region Internal Methods

        private void LoadCurrentSettings()
        {
            // Load from SaveService or use defaults
            var saveService = GameBootstrap.Services?.SaveService;
            if (saveService != null)
            {
                var settings = saveService.GetSettings();
                MasterVolume = settings.MasterVolume;
                MusicVolume = settings.MusicVolume;
                SFXVolume = settings.SFXVolume;
                Fullscreen = settings.Fullscreen;
                VSync = settings.VSync;
                
                // Load player name from stats (cloud storage)
                var stats = saveService.GetPlayerStats();
                if (_playerNameField != null)
                {
                    _playerNameField.value = stats.PlayerName;
                }
            }
            else
            {
                // Fallback to defaults if SaveService not available
                MasterVolume = DefaultMasterVolume;
                MusicVolume = DefaultMusicVolume;
                SFXVolume = DefaultSFXVolume;
                Fullscreen = DefaultFullscreen;
                VSync = DefaultVSync;
            }
        }

        private void SaveSettings()
        {
            // Save to SaveService (local storage)
            var saveService = GameBootstrap.Services?.SaveService;
            if (saveService != null)
            {
                // Save settings (local storage)
                saveService.UpdateSettings(settings =>
                {
                    settings.MasterVolume = MasterVolume;
                    settings.MusicVolume = MusicVolume;
                    settings.SFXVolume = SFXVolume;
                    settings.Fullscreen = Fullscreen;
                    settings.VSync = VSync;
                });
                
                // Save player name to stats (cloud storage)
                if (_playerNameField != null && !string.IsNullOrWhiteSpace(_playerNameField.value))
                {
                    saveService.UpdatePlayerStats(stats =>
                    {
                        stats.PlayerName = _playerNameField.value;
                    });
                }
                
                Debug.Log("[SettingsScreen] Settings saved to SaveService");
            }
            else
            {
                Debug.LogWarning("[SettingsScreen] SaveService not available, settings not saved");
            }
        }

        private void UpdateUIFromSettings()
        {
            // Update sliders
            if (_masterVolumeSlider != null)
            {
                _masterVolumeSlider.value = MasterVolume;
            }
            if (_musicVolumeSlider != null)
            {
                _musicVolumeSlider.value = MusicVolume;
            }
            if (_sfxVolumeSlider != null)
            {
                _sfxVolumeSlider.value = SFXVolume;
            }

            // Update toggles
            if (_fullscreenToggle != null)
            {
                _fullscreenToggle.value = Fullscreen;
            }
            if (_vsyncToggle != null)
            {
                _vsyncToggle.value = VSync;
            }
        }

        private void ApplyGraphicsSettings()
        {
            Screen.fullScreen = Fullscreen;
            QualitySettings.vSyncCount = VSync ? 1 : 0;
        }

        private void ApplyAudioSettings()
        {
            AudioListener.volume = MasterVolume;
            // Additional audio system integration would go here
        }

        #endregion

        #region Animation Customization

        /// <summary>
        /// Settings screen slides in from the right like a drawer
        /// </summary>
        public override void AnimateShow(IUIAnimator animator, VisualElement element, float duration, Action onComplete)
        {
            animator.SlideIn(element, SlideDirection.Right, duration, onComplete);
        }

        /// <summary>
        /// Settings screen slides out to the right when closing
        /// </summary>
        public override void AnimateHide(IUIAnimator animator, VisualElement element, float duration, Action onComplete)
        {
            animator.SlideOut(element, SlideDirection.Right, duration, onComplete);
        }

        /// <summary>
        /// Settings screen uses a slightly longer animation for a smooth drawer effect
        /// </summary>
        public override float GetAnimationDuration()
        {
            return 0.4f; // Slightly slower for smooth drawer feel
        }

        /// <summary>
        /// Prepare the settings screen before showing
        /// </summary>
        public override void OnBeforeShowAnimation()
        {
            // Load the latest settings before showing
            LoadCurrentSettings();
            UpdateUIFromSettings();
        }

        /// <summary>
        /// Focus the first interactive element after animation completes
        /// </summary>
        public override void OnAfterShowAnimation()
        {
            // Focus the master volume slider for immediate interaction
            _masterVolumeSlider?.Focus();
        }

        /// <summary>
        /// Save settings before hiding (auto-save behavior)
        /// </summary>
        public override void OnBeforeHideAnimation()
        {
            // Auto-save settings when closing
            SaveSettings();
        }

        #endregion

        #region Event Handlers

        private void HandleCloseClicked()
        {
            Debug.Log("[SettingsScreen] Close button clicked");
            OnSettingsClosed?.Invoke();
            Hide(true);
        }

        private void HandleResetClicked()
        {
            Debug.Log("[SettingsScreen] Reset button clicked");
            ResetToDefaults();
        }

        private void HandleApplyClicked()
        {
            Debug.Log("[SettingsScreen] Apply button clicked");
            ApplySettings();
            Hide(true);
        }

        private void HandleMasterVolumeChanged(float value)
        {
            MasterVolume = value;
            OnMasterVolumeChanged?.Invoke(value);
            
            // Apply immediately for preview
            AudioListener.volume = value;
            
            Debug.Log($"[SettingsScreen] Master volume changed to {value:F2}");
        }

        private void HandleMusicVolumeChanged(float value)
        {
            MusicVolume = value;
            OnMusicVolumeChanged?.Invoke(value);
            
            Debug.Log($"[SettingsScreen] Music volume changed to {value:F2}");
        }

        private void HandleSFXVolumeChanged(float value)
        {
            SFXVolume = value;
            OnSFXVolumeChanged?.Invoke(value);
            
            Debug.Log($"[SettingsScreen] SFX volume changed to {value:F2}");
        }

        private void HandleFullscreenChanged(bool value)
        {
            Fullscreen = value;
            OnFullscreenChanged?.Invoke(value);
            
            Debug.Log($"[SettingsScreen] Fullscreen changed to {value}");
        }

        private void HandleVSyncChanged(bool value)
        {
            VSync = value;
            OnVSyncChanged?.Invoke(value);
            
            Debug.Log($"[SettingsScreen] VSync changed to {value}");
        }

        private async void HandleLogoutClicked()
        {
            Debug.Log("[SettingsScreen] Logout button clicked");
            
            // Use AuthenticationService
            var authService = GameBootstrap.Services?.AuthenticationService;
            if (authService != null)
            {
                await authService.LogoutAsync();
                // Bootstrap will handle the rest via OnLogoutComplete event
            }
            else
            {
                Debug.LogError("[SettingsScreen] AuthenticationService not available");
            }
        }

        #endregion
    }

    /// <summary>
    /// Data structure for settings
    /// </summary>
    [System.Serializable]
    public struct SettingsData
    {
        public float masterVolume;
        public float musicVolume;
        public float sfxVolume;
        public bool fullscreen;
        public bool vSync;
    }
}