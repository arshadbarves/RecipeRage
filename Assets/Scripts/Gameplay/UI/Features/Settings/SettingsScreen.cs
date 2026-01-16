using System.Collections.Generic;
using Core.UI;
using Core.UI.Interfaces;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

using Core.UI.Core;

namespace Gameplay.UI.Features.Settings
{
    [UIScreen(UIScreenCategory.Screen, "Screens/SettingsTemplate")]
    public class SettingsScreen : BaseUIScreen
    {
        [Inject] private SettingsViewModel _viewModel;
        [Inject] private IUIService _uiService;

        private Slider _musicVolumeSlider;
        private Slider _sfxVolumeSlider;
        private Toggle _muteToggle;
        private Label _musicVolumeLabel;
        private Label _sfxVolumeLabel;
        private DropdownField _qualityDropdown;
        private Toggle _vibrationToggle;
        private DropdownField _languageDropdown;
        private Toggle _notificationsToggle;
        private Label _versionLabel;

        [Inject] private Core.Localization.ILocalizationManager _localizationManager;

        protected override void OnInitialize()
        {
            QueryElements();
            InitializeDropdowns();
            SetupValueChangeCallbacks();
            SetupButtons();

            if (_localizationManager != null)
                _localizationManager.OnLanguageChanged += RefreshLocalization;
        }

        protected override void OnDispose()
        {
            if (_localizationManager != null)
                _localizationManager.OnLanguageChanged -= RefreshLocalization;
        }

        private void RefreshLocalization()
        {
            if (_localizationManager == null) return;

            // Sidebar
            SetTextByClass("sidebar-title", "settings_title");

            // Tabs
            SetText("tab-general", "settings_tab_general");
            SetText("tab-graphics", "settings_tab_graphics");
            SetText("tab-controls", "settings_tab_controls");
            SetText("tab-gameplay", "settings_tab_gameplay");
            SetText("tab-support", "settings_tab_support");
            SetText("tab-account", "settings_tab_account");

            // Headers
            // Since we can't easily target specific headers by class without ID, we rely on structure or unique logic.
            // However, headers usually just have static text in UXML.
            // Let's assume we can find them by context if needed, but for now we'll stick to what we can target reliably.
            // Actually, we can update headers by searching for "header-title" specifically
            // But they all share the same class "header-title".
            // We can iterate the tabs and find the header in each.
            SetHeaderInTab("tab-general", "settings_tab_general", "settings_header_audio");
            SetHeaderInTab("tab-graphics", "settings_tab_graphics", "settings_header_display");
            SetHeaderInTab("tab-controls", "settings_tab_controls", "settings_header_input");
            SetHeaderInTab("tab-gameplay", "settings_tab_gameplay", "settings_header_game");
            SetHeaderInTab("tab-support", "settings_tab_support", "settings_header_legal");
            SetHeaderInTab("tab-account", "settings_tab_account", "settings_header_profile");

            // Section Labels (using similar tab-based search)
            SetSectionLabelInTab("tab-general", "settings_sect_audio");
            SetSectionLabelInTab("tab-graphics", "settings_sect_display");
            SetSectionLabelInTab("tab-controls", "settings_sect_input");
            SetSectionLabelInTab("tab-gameplay", "settings_sect_options");
            SetSectionLabelsInSupportTab(); // Multiple sections
            SetSectionLabelsInAccountTab(); // Multiple sections

            // General Options
            SetOptionLabels("music-volume", "settings_opt_music", "settings_desc_music");
            SetOptionLabels("sfx-volume", "settings_opt_sfx", "settings_desc_sfx");
            SetOptionLabels("mute-toggle", "settings_opt_mute", "settings_desc_mute");

            // Graphics Options
            SetOptionLabels("quality-dropdown", "settings_opt_quality", "settings_desc_quality");

            // Controls Options
            SetOptionLabels("vibration-toggle", "settings_opt_vibration", "settings_desc_vibration");
            SetButtonLabel("edit-joystick-button", "settings_btn_edit_joystick", "edit-joystick-label");
            SetButtonLabel("edit-joystick-button", "settings_btn_edit", "edit-btn-label"); // Sub-button

            // Gameplay Options
            SetOptionLabels("language-dropdown", "settings_opt_language", "settings_desc_language");
            SetOptionLabels("notifications-toggle", "settings_opt_notifications", "settings_desc_notifications");

            // Account Options
            SetOptionLabels("logout-button", "settings_opt_logout", "settings_desc_logout"); // Button inside option row

            // Buttons
            SetButtonLabel("logout-button", "settings_btn_logout", "action-btn-label");
            SetButtonLabel("reset-button", "settings_btn_reset", "action-btn-label");
            SetButtonLabel("clear-data-button", "settings_btn_clear", "action-btn-label");
            SetButtonLabel("back-button", "settings_btn_back", "btn-back-label");

            // Legal Grid
            SetButtonLabel("help-button", "settings_btn_help", "legal-text");
            SetButtonLabel("support-button", "settings_btn_support", "legal-text");
            SetButtonLabel("privacy-button", "settings_btn_privacy", "legal-text");
            SetButtonLabel("terms-button", "settings_btn_terms", "legal-text");
            SetButtonLabel("credits-button", "settings_btn_credits", "legal-text");
            SetButtonLabel("parent-guide-button", "settings_btn_parent", "legal-text");
        }

        private void SetText(string elemName, string key)
        {
            var elem = GetElement<VisualElement>(elemName);
            if (elem is Label l) l.text = _localizationManager.GetText(key);
            else if (elem is Tab t) t.label = _localizationManager.GetText(key);
        }

        private void SetTextByClass(string className, string key)
        {
            var labels = Container?.Query<Label>(className: className).ToList();
            if (labels != null) foreach (var l in labels) l.text = _localizationManager.GetText(key);
        }

        private void SetButtonLabel(string buttonName, string key, string labelClass)
        {
            var btn = GetElement<Button>(buttonName);
            var label = btn?.Q<Label>(className: labelClass);
            if (label != null) label.text = _localizationManager.GetText(key);
        }

        private void SetOptionLabels(string controlName, string titleKey, string descKey)
        {
            var control = GetElement<VisualElement>(controlName);
            if (control == null) return;

            // Walk up to option-row
            // Structure: option-row -> (opt-info, wrapper/control)
            // So control.parent is usually wrapper or option-row directly
            var parent = control.parent;
            while (parent != null && !parent.ClassListContains("option-row"))
            {
                parent = parent.parent;
            }

            if (parent != null)
            {
                var title = parent.Q<Label>(className: "opt-title");
                var desc = parent.Q<Label>(className: "opt-desc");
                if (title != null) title.text = _localizationManager.GetText(titleKey);
                if (desc != null) desc.text = _localizationManager.GetText(descKey);
            }
        }

        private void SetHeaderInTab(string tabName, string titleKey, string subKey)
        {
            var tab = GetElement<Tab>(tabName);
            if (tab == null) return;
            var title = tab.Q<Label>(className: "header-title");
            var sub = tab.Q<Label>(className: "header-sub");
            if (title != null) title.text = _localizationManager.GetText(titleKey);
            if (sub != null) sub.text = _localizationManager.GetText(subKey);
        }

        private void SetSectionLabelInTab(string tabName, string key)
        {
            var tab = GetElement<Tab>(tabName);
            if (tab == null) return;
            // Assumes one section per tab for the simpler ones, or first one found
            var label = tab.Q<Label>(className: "section-box") ?? tab.Q<VisualElement>(className: "section-label").Q<Label>();
            // Correct query: .section-label > Label
            if (label != null) label.text = _localizationManager.GetText(key);
        }

        private void SetSectionLabelsInSupportTab()
        {
            var tab = GetElement<Tab>("tab-support");
            if (tab == null) return;
            var sections = tab.Query<VisualElement>(className: "section-label").ToList();
            if (sections.Count > 0) sections[0].Q<Label>().text = _localizationManager.GetText("settings_sect_info");
        }

        private void SetSectionLabelsInAccountTab()
        {
            var tab = GetElement<Tab>("tab-account");
            if (tab == null) return;
            var sections = tab.Query<VisualElement>(className: "section-label").ToList();
            if (sections.Count > 0) sections[0].Q<Label>().text = _localizationManager.GetText("settings_sect_session");
            if (sections.Count > 1) sections[1].Q<Label>().text = _localizationManager.GetText("settings_sect_data");
        }

        protected override void OnShow()
        {
            _viewModel.Initialize();
            BindViewModel();
            UpdateVersionInfo();
        }

        private void QueryElements()
        {
            _musicVolumeSlider = GetElement<Slider>("music-volume");
            _sfxVolumeSlider = GetElement<Slider>("sfx-volume");
            _muteToggle = GetElement<Toggle>("mute-toggle");
            _musicVolumeLabel = GetElement<Label>("music-volume-value");
            _sfxVolumeLabel = GetElement<Label>("sfx-volume-value");
            _qualityDropdown = GetElement<DropdownField>("quality-dropdown");
            _vibrationToggle = GetElement<Toggle>("vibration-toggle");
            _languageDropdown = GetElement<DropdownField>("language-dropdown");
            _notificationsToggle = GetElement<Toggle>("notifications-toggle");
            _versionLabel = GetElement<Label>("version-label");
        }

        private void InitializeDropdowns()
        {
            if (_qualityDropdown != null)
            {
                _qualityDropdown.choices = new List<string>(QualitySettings.names);
            }

            if (_languageDropdown != null)
            {
                _languageDropdown.choices = new List<string>(_viewModel.AvailableLanguages);
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

            _viewModel.VibrationEnabled.Bind(val =>
            {
                if (_vibrationToggle != null) _vibrationToggle.value = val;
            });

            _viewModel.NotificationsEnabled.Bind(val =>
            {
                if (_notificationsToggle != null) _notificationsToggle.value = val;
            });

            _viewModel.IsMuted.Bind(val =>
            {
                if (_muteToggle != null) _muteToggle.value = val;
            });

            _viewModel.GraphicsQuality.Bind(val =>
            {
               if (_qualityDropdown != null) _qualityDropdown.index = val;
            });

            _viewModel.Language.Bind(val =>
            {
                if (_languageDropdown != null) _languageDropdown.value = val;
            });
        }

        private void SetupValueChangeCallbacks()
        {
            _musicVolumeSlider?.RegisterValueChangedCallback(evt =>
            {
                _viewModel.MusicVolume.Value = evt.newValue;
                _viewModel.SaveSettings();
            });

            _sfxVolumeSlider?.RegisterValueChangedCallback(evt =>
            {
                _viewModel.SFXVolume.Value = evt.newValue;
                _viewModel.SaveSettings();
            });

            _vibrationToggle?.RegisterValueChangedCallback(evt =>
            {
                _viewModel.VibrationEnabled.Value = evt.newValue;
                _viewModel.SaveSettings();
            });

            _notificationsToggle?.RegisterValueChangedCallback(evt =>
            {
                _viewModel.NotificationsEnabled.Value = evt.newValue;
                _viewModel.SaveSettings();
            });

            _muteToggle?.RegisterValueChangedCallback(evt =>
            {
                _viewModel.IsMuted.Value = evt.newValue;
                _viewModel.SaveSettings();
            });

            _qualityDropdown?.RegisterValueChangedCallback(evt =>
            {
                if (_qualityDropdown.index >= 0)
                {
                    _viewModel.GraphicsQuality.Value = _qualityDropdown.index;
                    _viewModel.SaveSettings();
                }
            });

            _languageDropdown?.RegisterValueChangedCallback(evt =>
            {
                _viewModel.Language.Value = evt.newValue;
                _viewModel.SaveSettings();
            });
        }

        private void SetupButtons()
        {
            GetElement<Button>("logout-button")?.RegisterCallback<ClickEvent>(_ => _viewModel.Logout());
            GetElement<Button>("reset-button")?.RegisterCallback<ClickEvent>(_ => OnResetClicked());
            GetElement<Button>("clear-data-button")?.RegisterCallback<ClickEvent>(_ => OnClearDataClicked());
            GetElement<Button>("back-button")?.RegisterCallback<ClickEvent>(_ => OnBackClicked());
            GetElement<Button>("edit-joystick-button")?.RegisterCallback<ClickEvent>(_ => OnEditJoystickClicked());
            GetElement<Button>("help-button")?.RegisterCallback<ClickEvent>(_ => OnHelpClicked());
            GetElement<Button>("support-button")?.RegisterCallback<ClickEvent>(_ => OnSupportClicked());
            GetElement<Button>("privacy-button")?.RegisterCallback<ClickEvent>(_ => OnPrivacyClicked());
            GetElement<Button>("terms-button")?.RegisterCallback<ClickEvent>(_ => OnTermsClicked());
            GetElement<Button>("credits-button")?.RegisterCallback<ClickEvent>(_ => OnCreditsClicked());
            GetElement<Button>("parent-guide-button")?.RegisterCallback<ClickEvent>(_ => OnParentGuideClicked());
        }

        private void UpdateVolumeLabel(Label label, float value)
        {
            if (label != null) label.text = $"{Mathf.RoundToInt(value * 100)}%";
        }



        private void UpdateVersionInfo()
        {
            if (_versionLabel != null) _versionLabel.text = $"v{Application.version}";
        }

        private void OnResetClicked()
        {
            _viewModel.ResetToDefaults();
            _uiService?.ShowNotification("Settings Reset to Defaults", NotificationType.Success);
        }

        private void OnClearDataClicked()
        {
            _viewModel.ClearData();
            _uiService?.ShowNotification("All Data Cleared", NotificationType.Warning);
        }

        private void OnBackClicked()
        {
            _uiService?.GoBack(false);
        }

        private void OnEditJoystickClicked()
        {
            _uiService?.ShowNotification("Joystick Editor Coming Soon", NotificationType.Info);
        }

        private void OnHelpClicked()
        {
            Application.OpenURL("https://reciperage.game/help");
        }

        private void OnSupportClicked()
        {
            Application.OpenURL("https://reciperage.game/support");
        }

        private void OnPrivacyClicked()
        {
            Application.OpenURL("https://reciperage.game/privacy");
        }

        private void OnTermsClicked()
        {
            Application.OpenURL("https://reciperage.game/terms");
        }

        private void OnCreditsClicked()
        {
            _uiService?.ShowNotification("Credits", NotificationType.Info);
        }

        private void OnParentGuideClicked()
        {
            Application.OpenURL("https://reciperage.game/parents");
        }

    }
}
