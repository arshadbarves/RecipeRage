using System;
using Core.Bootstrap;
using Core.Logging;
using Core.SaveSystem;
using UI.Screens;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Components.Tabs
{
    /// <summary>
    /// Settings tab content component.
    /// Refactored to use a modular Sub-Tab system.
    /// Follows SOLID: Single Responsibility and Dependency Inversion.
    /// </summary>
    public class SettingsTabComponent : ITabComponent
    {
        private VisualElement _root;
        private readonly ISaveService _saveService;
        private TabSystem _subTabSystem;

        public string TabId => "Settings";

        // Main UI Elements
        private Button _backButton;
        private Button _resetButton;
        private Button _applyButton;
        private VisualElement _contentRoot;

        public SettingsTabComponent(ISaveService saveService)
        {
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
        }

        public void Initialize(VisualElement root)
        {
            if (root == null)
            {
                GameLogger.LogError("Root is null!");
                return;
            }

            _root = root;
            _contentRoot = _root.Q<VisualElement>("settings-content");

            // Query Main Buttons
            _backButton = _root.Q<Button>("back-btn");
            _resetButton = _root.Q<Button>("reset-btn");
            _applyButton = _root.Q<Button>("apply-btn");

            if (_backButton != null) _backButton.clicked += OnBackClicked;
            if (_resetButton != null) _resetButton.clicked += OnResetClicked;
            if (_applyButton != null) _applyButton.clicked += OnApplyClicked;

            // Initialize Sub-Tab System
            var animator = GameBootstrap.Services?.AnimationService?.UI;
            _subTabSystem = new TabSystem(_contentRoot, animator);

            InitializeSubTabs();

            GameLogger.Log("Settings Screen initialized with sub-tabs");
        }

        private void InitializeSubTabs()
        {
            // Graphics Tab
            AddSettingsTab("Graphics", "tab-graphics", "Components/SettingsTab_Graphics", root => new GraphicsSettingsTab(root, _saveService));
            
            // Audio Tab
            AddSettingsTab("Audio", "tab-audio", "Components/SettingsTab_Audio", root => new AudioSettingsTab(root, _saveService));
            
            // Controls Tab
            AddSettingsTab("Controls", "tab-controls", "Components/SettingsTab_Controls", root => new ControlsSettingsTab(root, _saveService));
            
            // Account Tab
            AddSettingsTab("Account", "tab-account", "Components/SettingsTab_Account", root => new AccountSettingsTab(root, _saveService));
            
            // Social/Legal Tab
            AddSettingsTab("Social", "tab-legal", "Components/SettingsTab_Legal", root => new LegalSettingsTab(root, _saveService));

            // Default to Graphics
            _subTabSystem.SwitchToTab("Graphics", true);
        }

        private void AddSettingsTab(string id, string btnName, string templatePath, Func<VisualElement, ITabComponent> factory)
        {
            var btn = _root.Q<Button>(btnName);
            var template = Resources.Load<VisualTreeAsset>($"UI/Templates/{templatePath}");
            if (btn != null && template != null)
            {
                var contentRoot = template.CloneTree();
                contentRoot.style.flexGrow = 1;
                contentRoot.style.display = DisplayStyle.None;
                _contentRoot.Add(contentRoot);
                
                var component = factory(contentRoot);
                component.Initialize(contentRoot);
                _subTabSystem.AddTab(id, btn, component);
            }
            else
            {
                GameLogger.LogWarning($"Failed to add settings tab: {id}. Button found: {btn != null}, Template found: {template != null}");
            }
        }

        public void OnShow()
        {
            if (_root != null) _root.style.display = DisplayStyle.Flex;
        }

        public void OnHide()
        {
            if (_root != null) _root.style.display = DisplayStyle.None;
        }

        public void Update(float deltaTime) => _subTabSystem?.Update(deltaTime);

        public void Dispose() => _subTabSystem?.Dispose();

        private void OnBackClicked()
        {
            GameBootstrap.Services?.UIService?.GoBack();
        }

        private void OnApplyClicked()
        {
            GameLogger.Log("Applying settings changes");
            // The SettingsService applies current values from SaveService
            var settingsService = (GameBootstrap.Services as ServiceContainer)?.SettingsService;
            settingsService?.ApplyAllSettings(_saveService.GetSettings());
            
            GameBootstrap.Services?.UIService?.ShowNotification("Settings applied", NotificationType.Success, 2f);
        }

        private void OnResetClicked()
        {
            GameLogger.Log("Resetting settings to defaults");
            _saveService.DeleteAllData(); // Or implementation-specific reset
            _subTabSystem.SwitchToTab("Graphics", true); // Reload UI
            GameBootstrap.Services?.UIService?.ShowNotification("Settings reset", NotificationType.Info, 2f);
        }

        #region Internal Sub-Tab Implementations

        private abstract class BaseSettingsTab : ITabComponent
        {
            protected VisualElement Root;
            protected ISaveService SaveService;
            public abstract string TabId { get; }

            public BaseSettingsTab(VisualElement root, ISaveService saveService)
            {
                Root = root;
                SaveService = saveService;
            }

            public virtual void Initialize(VisualElement root) { }
            public virtual void OnShow() => Root.style.display = DisplayStyle.Flex;
            public virtual void OnHide() => Root.style.display = DisplayStyle.None;
            public virtual void Update(float deltaTime) { }
            public virtual void Dispose() { }
        }

        private class GraphicsSettingsTab : BaseSettingsTab
        {
            public override string TabId => "Graphics";
            
            private VisualElement _qualityGroup;
            private VisualElement _fpsGroup;
            private Toggle _shadowsToggle;
            private Toggle _bloomToggle;

            public GraphicsSettingsTab(VisualElement root, ISaveService saveService) : base(root, saveService) { }

            public override void Initialize(VisualElement root)
            {
                _qualityGroup = root.Q<VisualElement>(null, "segment-control"); // First segment control
                _fpsGroup = root.Query<VisualElement>(null, "segment-control").Last();
                _shadowsToggle = root.Q<Toggle>("shadows-toggle");
                _bloomToggle = root.Q<Toggle>("bloom-toggle");

                SetupQualityButtons();
                SetupFPSButtons();
                
                if (_shadowsToggle != null)
                {
                    _shadowsToggle.RegisterValueChangedCallback(evt => SaveService.UpdateSettings(s => s.ShadowsEnabled = evt.newValue));
                }
                
                if (_bloomToggle != null)
                {
                    _bloomToggle.RegisterValueChangedCallback(evt => SaveService.UpdateSettings(s => s.BloomEnabled = evt.newValue));
                }
            }

            public override void OnShow()
            {
                base.OnShow();
                LoadCurrentSettings();
            }

            private void LoadCurrentSettings()
            {
                var settings = SaveService.GetSettings();
                
                // Update Quality Segment
                UpdateSegmentVisual(_qualityGroup, settings.GraphicsQuality);
                
                // Update FPS Segment (Map 30, 60, 120 to indices 0, 1, 2)
                int fpsIndex = settings.TargetFrameRate switch { 30 => 0, 60 => 1, 120 => 2, _ => 1 };
                UpdateSegmentVisual(_fpsGroup, fpsIndex);

                if (_shadowsToggle != null) _shadowsToggle.value = settings.ShadowsEnabled;
                if (_bloomToggle != null) _bloomToggle.value = settings.BloomEnabled;
            }

            private void SetupQualityButtons()
            {
                if (_qualityGroup == null) return;
                var btns = _qualityGroup.Query<Button>(null, "seg-btn").ToList();
                for (int i = 0; i < btns.Count; i++)
                {
                    int index = i;
                    btns[i].clicked += () => {
                        SaveService.UpdateSettings(s => s.GraphicsQuality = index);
                        UpdateSegmentVisual(_qualityGroup, index);
                    };
                }
            }

            private void SetupFPSButtons()
            {
                if (_fpsGroup == null) return;
                var btns = _fpsGroup.Query<Button>(null, "seg-btn").ToList();
                int[] rates = { 30, 60, 120 };
                for (int i = 0; i < btns.Count; i++)
                {
                    int index = i;
                    btns[i].clicked += () => {
                        SaveService.UpdateSettings(s => s.TargetFrameRate = rates[index]);
                        UpdateSegmentVisual(_fpsGroup, index);
                    };
                }
            }

            private void UpdateSegmentVisual(VisualElement group, int activeIndex)
            {
                if (group == null) return;
                var btns = group.Query<Button>(null, "seg-btn").ToList();
                for (int i = 0; i < btns.Count; i++)
                {
                    if (i == activeIndex) btns[i].AddToClassList("active");
                    else btns[i].RemoveFromClassList("active");
                }
            }
        }

        private class AudioSettingsTab : BaseSettingsTab
        {
            public override string TabId => "Audio";
            
            private Slider _masterSlider;
            private Slider _musicSlider;
            private Slider _sfxSlider;
            private Label _masterLabel;
            private Label _musicLabel;
            private Label _sfxLabel;

            public AudioSettingsTab(VisualElement root, ISaveService saveService) : base(root, saveService) { }

            public override void Initialize(VisualElement root)
            {
                _masterSlider = root.Q<Slider>("master-volume");
                _musicSlider = root.Q<Slider>("music-volume");
                _sfxSlider = root.Q<Slider>("sfx-volume");
                
                _masterLabel = root.Q<Label>("master-volume-text");
                _musicLabel = root.Q<Label>("music-volume-text");
                _sfxLabel = root.Q<Label>("sfx-volume-text");

                SetupSlider(_masterSlider, _masterLabel, (s, v) => s.MasterVolume = v);
                SetupSlider(_musicSlider, _musicLabel, (s, v) => s.MusicVolume = v);
                SetupSlider(_sfxSlider, _sfxLabel, (s, v) => s.SFXVolume = v);
            }

            public override void OnShow()
            {
                base.OnShow();
                LoadCurrentSettings();
            }

            private void LoadCurrentSettings()
            {
                var settings = SaveService.GetSettings();
                UpdateSliderVisual(_masterSlider, _masterLabel, settings.MasterVolume);
                UpdateSliderVisual(_musicSlider, _musicLabel, settings.MusicVolume);
                UpdateSliderVisual(_sfxSlider, _sfxLabel, settings.SFXVolume);
            }

            private void SetupSlider(Slider slider, Label label, Action<GameSettingsData, float> saveAction)
            {
                if (slider == null) return;
                slider.RegisterValueChangedCallback(evt => {
                    SaveService.UpdateSettings(s => saveAction(s, evt.newValue));
                    UpdateSliderVisual(slider, label, evt.newValue);
                });
            }

            private void UpdateSliderVisual(Slider slider, Label label, float value)
            {
                if (slider != null) slider.value = value;
                if (label != null) label.text = $"{Mathf.RoundToInt(value * 100)}%";
            }
        }

        private class ControlsSettingsTab : BaseSettingsTab
        {
            public override string TabId => "Controls";
            
            private Button _editControlsBtn;
            private Slider _sensitivitySlider;
            private Label _sensitivityLabel;
            private Toggle _invertYToggle;
            private Toggle _vibrationToggle;

            public ControlsSettingsTab(VisualElement root, ISaveService saveService) : base(root, saveService) { }

            public override void Initialize(VisualElement root)
            {
                _editControlsBtn = root.Q<Button>("edit-controls-btn");
                _sensitivitySlider = root.Q<Slider>("sensitivity");
                _sensitivityLabel = root.Q<Label>("sensitivity-text");
                _invertYToggle = root.Q<Toggle>("invert-y");
                _vibrationToggle = root.Q<Toggle>("vibration");

                if (_editControlsBtn != null) _editControlsBtn.clicked += OnEditControlsClicked;

                if (_sensitivitySlider != null)
                {
                    _sensitivitySlider.RegisterValueChangedCallback(evt => {
                        SaveService.UpdateSettings(s => s.Sensitivity = evt.newValue);
                        if (_sensitivityLabel != null) _sensitivityLabel.text = $"{evt.newValue:F1}";
                    });
                }

                if (_invertYToggle != null)
                {
                    _invertYToggle.RegisterValueChangedCallback(evt => SaveService.UpdateSettings(s => s.InvertY = evt.newValue));
                }

                if (_vibrationToggle != null)
                {
                    _vibrationToggle.RegisterValueChangedCallback(evt => SaveService.UpdateSettings(s => s.IsVibrationEnabled = evt.newValue));
                }
            }

            public override void OnShow()
            {
                base.OnShow();
                var settings = SaveService.GetSettings();
                if (_sensitivitySlider != null) _sensitivitySlider.value = settings.Sensitivity;
                if (_sensitivityLabel != null) _sensitivityLabel.text = $"{settings.Sensitivity:F1}";
                if (_invertYToggle != null) _invertYToggle.value = settings.InvertY;
                if (_vibrationToggle != null) _vibrationToggle.value = settings.IsVibrationEnabled;
            }

            private void OnEditControlsClicked()
            {
                GameLogger.Log("Opening HUD Editor...");
                // Open HUD Editor mode (Phase 3)
            }
        }

        private class AccountSettingsTab : BaseSettingsTab
        {
            public override string TabId => "Account";
            
            private Label _playerNameLabel;
            private Label _playerIdLabel;
            private Button _copyIdBtn;
            private Button _logoutBtn;

            public AccountSettingsTab(VisualElement root, ISaveService saveService) : base(root, saveService) { }

            public override void Initialize(VisualElement root)
            {
                _playerNameLabel = root.Q<Label>("player-name");
                _playerIdLabel = root.Q<Label>("player-id");
                _copyIdBtn = root.Q<Button>("copy-id-btn");
                _logoutBtn = root.Q<Button>("logout-btn");

                if (_copyIdBtn != null) _copyIdBtn.clicked += OnCopyIdClicked;
                if (_logoutBtn != null) _logoutBtn.clicked += OnLogoutClicked;
            }

            public override void OnShow()
            {
                base.OnShow();
                var stats = SaveService.GetPlayerStats();
                if (_playerNameLabel != null) _playerNameLabel.text = stats.PlayerName;
                if (_playerIdLabel != null) _playerIdLabel.text = $"UID: {stats.PlayerId}";
            }

            private void OnCopyIdClicked()
            {
                var stats = SaveService.GetPlayerStats();
                GUIUtility.systemCopyBuffer = stats.PlayerId;
                GameBootstrap.Services?.UIService?.ShowNotification("Player ID copied", NotificationType.Info, 2f);
            }

            private async void OnLogoutClicked()
            {
                GameLogger.Log("Requesting Logout");
                var auth = GameBootstrap.Services?.AuthenticationService;
                if (auth != null)
                {
                    await auth.LogoutAsync();
                }
            }
        }

        private class LegalSettingsTab : BaseSettingsTab
        {
            public override string TabId => "Social";
            
            private DropdownField _languageDropdown;
            private Toggle _notificationsToggle;

            public LegalSettingsTab(VisualElement root, ISaveService saveService) : base(root, saveService) { }

            public override void Initialize(VisualElement root)
            {
                _languageDropdown = root.Q<DropdownField>("language-dropdown");
                _notificationsToggle = root.Q<Toggle>("notifications");

                if (_languageDropdown != null)
                {
                    _languageDropdown.choices = new List<string> { "English", "Spanish", "French" }; // Expand as needed
                    _languageDropdown.RegisterValueChangedCallback(evt => {
                        int index = _languageDropdown.choices.IndexOf(evt.newValue);
                        SaveService.UpdateSettings(s => s.LanguageIndex = index);
                    });
                }

                if (_notificationsToggle != null)
                {
                    _notificationsToggle.RegisterValueChangedCallback(evt => SaveService.UpdateSettings(s => s.NotificationsEnabled = evt.newValue));
                }

                // URL Buttons
                SetupUrlBtn(root, "help-btn", "https://support.reciperage.com/help");
                SetupUrlBtn(root, "support-btn", "https://support.reciperage.com/contact");
                SetupUrlBtn(root, "privacy-btn", "https://reciperage.com/privacy");
                SetupUrlBtn(root, "terms-btn", "https://reciperage.com/terms");
            }

            private void SetupUrlBtn(VisualElement root, string name, string url)
            {
                var btn = root.Q<Button>(name);
                if (btn != null) btn.clicked += () => Application.OpenURL(url);
            }

            public override void OnShow()
            {
                base.OnShow();
                var settings = SaveService.GetSettings();
                if (_languageDropdown != null && settings.LanguageIndex >= 0 && settings.LanguageIndex < _languageDropdown.choices.Count)
                {
                    _languageDropdown.value = _languageDropdown.choices[settings.LanguageIndex];
                }
                if (_notificationsToggle != null) _notificationsToggle.value = settings.NotificationsEnabled;
            }
        }

        #endregion
    }
}