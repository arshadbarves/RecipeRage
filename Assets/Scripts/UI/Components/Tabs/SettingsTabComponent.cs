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
            public GraphicsSettingsTab(VisualElement root, ISaveService saveService) : base(root, saveService) { }
            // Add UI logic for graphics sliders/toggles here
        }

        private class AudioSettingsTab : BaseSettingsTab
        {
            public override string TabId => "Audio";
            public AudioSettingsTab(VisualElement root, ISaveService saveService) : base(root, saveService) { }
            // Add UI logic for audio sliders here
        }

        private class ControlsSettingsTab : BaseSettingsTab
        {
            public override string TabId => "Controls";
            public ControlsSettingsTab(VisualElement root, ISaveService saveService) : base(root, saveService) { }
        }

        private class AccountSettingsTab : BaseSettingsTab
        {
            public override string TabId => "Account";
            public AccountSettingsTab(VisualElement root, ISaveService saveService) : base(root, saveService) { }
        }

        private class LegalSettingsTab : BaseSettingsTab
        {
            public override string TabId => "Social";
            public LegalSettingsTab(VisualElement root, ISaveService saveService) : base(root, saveService) { }
        }

        #endregion
    }
}