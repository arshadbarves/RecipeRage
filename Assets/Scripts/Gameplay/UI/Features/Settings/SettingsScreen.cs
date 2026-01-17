using System.Collections.Generic;
using Core.UI;
using Core.UI.Interfaces;
using Gameplay.UI.Extensions;
using Gameplay.UI.Localization;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using Core.Auth;
using Core.Session;
using Gameplay.Persistence;

using Core.UI.Core;

namespace Gameplay.UI.Features.Settings
{
    [UIScreen(UIScreenCategory.Screen, "Screens/SettingsTemplate")]
    public class SettingsScreen : BaseUIScreen
    {
        [Inject] private SettingsViewModel _viewModel;
        [Inject] private IUIService _uiService;
        [Inject] private SessionManager _sessionManager;
        [Inject] private IAuthService _authService;

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
        private Label _accountNameLabel;
        private Label _accountUidLabel;

        private VisualElement _guestWarningBlock;

        [Inject] private Core.Localization.ILocalizationManager _localizationManager;

        protected override void OnInitialize()
        {
            QueryElements();
            InitializeDropdowns();
            SetupValueChangeCallbacks();
            SetupButtons();
            BindLocalization();
        }

        protected override void OnDispose()
        {
            _localizationManager?.UnregisterAll(this);
        }

        private void BindLocalization()
        {
            if (_localizationManager == null) return;

            // Sidebar
            var sidebarTitles = Container?.Query<Label>(className: "sidebar-title").ToList();
            if (sidebarTitles != null) foreach (var l in sidebarTitles) _localizationManager.Bind(l, LocKeys.SettingsTitle, this);

            // Tabs
            _localizationManager.Bind(GetElement<Tab>("tab-general"), LocKeys.SettingsTabGeneral, this);
            _localizationManager.Bind(GetElement<Tab>("tab-graphics"), LocKeys.SettingsTabGraphics, this);
            _localizationManager.Bind(GetElement<Tab>("tab-controls"), LocKeys.SettingsTabControls, this);
            _localizationManager.Bind(GetElement<Tab>("tab-gameplay"), LocKeys.SettingsTabGameplay, this);
            _localizationManager.Bind(GetElement<Tab>("tab-support"), LocKeys.SettingsTabSupport, this);
            _localizationManager.Bind(GetElement<Tab>("tab-account"), LocKeys.SettingsTabAccount, this);
            _localizationManager.Bind(GetElement<Tab>("tab-legal"), LocKeys.SettingsTabLegal, this); 
            
            // Sidebars can also be bound directly if there are more
            // (already handled above for .sidebar-title)

            // Headers & Sections
            // Helper to bind headers inside tabs
            BindHeaderInTab("tab-general", LocKeys.SettingsTabGeneral, LocKeys.SettingsHeaderAudio);
            BindHeaderInTab("tab-graphics", LocKeys.SettingsTabGraphics, LocKeys.SettingsHeaderDisplay);
            BindHeaderInTab("tab-controls", LocKeys.SettingsTabControls, LocKeys.SettingsHeaderInput);
            BindHeaderInTab("tab-gameplay", LocKeys.SettingsTabGameplay, LocKeys.SettingsHeaderGame);
            BindHeaderInTab("tab-legal", LocKeys.SettingsTabSupport, LocKeys.SettingsHeaderLegal);
            BindHeaderInTab("tab-account", LocKeys.SettingsTabAccount, LocKeys.SettingsHeaderProfile);
            
            // Section Labels
            BindSectionLabelInTab("tab-general", LocKeys.SettingsSectAudio);
            BindSectionLabelInTab("tab-graphics", LocKeys.SettingsSectDisplay);
            BindSectionLabelInTab("tab-controls", LocKeys.SettingsSectInput);
            BindSectionLabelInTab("tab-gameplay", LocKeys.SettingsSectOptions);
            BindSectionLabelInTab("tab-legal", LocKeys.SettingsSectInfo, 0);
            BindSectionLabelInTab("tab-legal", LocKeys.SettingsSectSupport, 1);
            BindSectionLabelInTab("tab-account", LocKeys.SettingsSectConnections, 0);
            BindSectionLabelInTab("tab-account", LocKeys.SettingsSectData, 1);

            // General Options
            BindOptionLabels("music-volume", LocKeys.SettingsOptMusic, LocKeys.SettingsDescMusic);
            BindOptionLabels("sfx-volume", LocKeys.SettingsOptSfx, LocKeys.SettingsDescSfx);
            BindOptionLabels("mute-toggle", LocKeys.SettingsOptMute, LocKeys.SettingsDescMute);

            // Graphics Options
            BindOptionLabels("quality-dropdown", LocKeys.SettingsOptQuality, LocKeys.SettingsDescQuality);

            // Controls Options
            BindOptionLabels("vibration-toggle", LocKeys.SettingsOptVibration, LocKeys.SettingsDescVibration);
            _localizationManager.Bind(GetElement<Button>("edit-joystick-button"), LocKeys.SettingsBtnEditJoystick, this, "edit-joystick-label");
            _localizationManager.Bind(GetElement<Button>("edit-joystick-button"), LocKeys.SettingsBtnEdit, this, "edit-btn-label");

            // Gameplay Options
            BindOptionLabels("language-dropdown", LocKeys.SettingsOptLanguage, LocKeys.SettingsDescLanguage);
            BindOptionLabels("notifications-toggle", LocKeys.SettingsOptNotifications, LocKeys.SettingsDescNotifications);

            // Buttons
            _localizationManager.Bind(GetElement<Button>("logout-button"), LocKeys.SettingsBtnLogout, this, "btn-logout-label");
            _localizationManager.Bind(GetElement<Button>("reset-button"), LocKeys.SettingsBtnReset, this, "action-btn-label");
            _localizationManager.Bind(GetElement<Button>("clear-data-button"), LocKeys.SettingsBtnClear, this, "action-btn-label");
            _localizationManager.Bind(GetElement<Button>("back-button"), LocKeys.SettingsBtnBack, this, "btn-back-label");

            // Legal/Support Options
            BindOptionLabels("help-button", LocKeys.SettingsOptHelpCenter, LocKeys.SettingsDescHelpCenter);
            BindOptionLabels("support-button", LocKeys.SettingsOptSupport, LocKeys.SettingsDescSupport);
            _localizationManager.Bind(GetElement<Button>("help-button"), LocKeys.SettingsBtnOpen, this, "edit-btn-label"); 
            _localizationManager.Bind(GetElement<Button>("support-button"), LocKeys.SettingsBtnChat, this, "edit-btn-label");

            // Account Options
            BindAccountOptionLabels(0, LocKeys.SettingsOptGooglePlay, LocKeys.SettingsDescGooglePlay);
            BindAccountOptionLabels(1, LocKeys.SettingsOptFacebook, LocKeys.SettingsDescFacebook);
            
            // Status and Link Buttons
            var connectionRows = GetElement<Tab>("tab-account")?.Query<VisualElement>(className: "option-row").ToList();
            if (connectionRows != null && connectionRows.Count > 0)
            {
                _localizationManager.Bind(connectionRows[0].Q<Label>(className: "status-connected"), LocKeys.SettingsStatusConnected, this);
                _localizationManager.Bind(connectionRows[1].Q<Button>()?.Q<Label>(), LocKeys.SettingsBtnLink, this);
            }

            // Guest Warning
            if (_guestWarningBlock != null)
            {
                _localizationManager.Bind(_guestWarningBlock.Q<Label>(className: "warning-text"), LocKeys.SettingsWarningGuest, this);
            }
            
            // Re-binding remaining legal buttons if they exist
            _localizationManager.Bind(GetElement<Button>("privacy-button"), LocKeys.SettingsBtnPrivacy, this, "legal-text");
            _localizationManager.Bind(GetElement<Button>("terms-button"), LocKeys.SettingsBtnTerms, this, "legal-text");
            _localizationManager.Bind(GetElement<Button>("credits-button"), LocKeys.SettingsBtnCredits, this, "legal-text");
            _localizationManager.Bind(GetElement<Button>("parent-guide-button"), LocKeys.SettingsBtnParent, this, "legal-text");
        }

        private void BindSectionLabelInTab(string tabName, string key, int index = 0)
        {
            var tab = GetElement<Tab>(tabName);
            if (tab == null) return;
            var sections = tab.Query<VisualElement>(className: "section-label").ToList();
            if (index < sections.Count)
            {
                _localizationManager.Bind(sections[index].Q<Label>(), key, this);
            }
        }

        private void BindAccountOptionLabels(int rowIndex, string titleKey, string descKey)
        {
            var tab = GetElement<Tab>("tab-account");
            var rows = tab?.Query<VisualElement>(className: "option-row").ToList();
            if (rows != null && rowIndex < rows.Count)
            {
                _localizationManager.Bind(rows[rowIndex].Q<Label>(className: "opt-title"), titleKey, this);
                _localizationManager.Bind(rows[rowIndex].Q<Label>(className: "opt-desc"), descKey, this);
            }
        }

        private void BindHeaderInTab(string tabName, string titleKey, string subKey = null)
        {
            var tab = GetElement<Tab>(tabName);
            if (tab == null) return;
            _localizationManager.Bind(tab.Q<Label>(className: "header-title"), titleKey, this);
            if (!string.IsNullOrEmpty(subKey))
            {
                _localizationManager.Bind(tab.Q<Label>(className: "header-sub"), subKey, this);
            }
        }

        private void BindOptionLabels(string controlName, string titleKey, string descKey)
        {
            var control = GetElement<VisualElement>(controlName);
            if (control == null) return;

            var parent = control.parent;
            while (parent != null && !parent.ClassListContains("option-row"))
            {
                parent = parent.parent;
            }

            if (parent != null)
            {
                _localizationManager.Bind(parent.Q<Label>(className: "opt-title"), titleKey, this);
                _localizationManager.Bind(parent.Q<Label>(className: "opt-desc"), descKey, this);
            }
        }

        protected override void OnShow()
        {
            _viewModel.Initialize();
            BindViewModel();
            UpdateAccountInfo();
            UpdateVersionInfo();
        }

        private void UpdateAccountInfo()
        {
            if (_sessionManager?.IsSessionActive != true) return;

            var playerDataService = _sessionManager.SessionContainer?.Resolve<PlayerDataService>();
            if (playerDataService != null)
            {
                var stats = playerDataService.GetStats();
                if (_accountNameLabel != null)
                    _accountNameLabel.text = string.IsNullOrEmpty(stats?.PlayerName) ? "GUEST" : stats.PlayerName.ToUpper();
            }

            if (_accountUidLabel != null && _authService != null)
            {
                string uid = string.IsNullOrEmpty(_authService.EosProductUserId) ? "NOT SIGNED IN" : _authService.EosProductUserId;
                _accountUidLabel.text = $"UID: {uid}";
            }
        }

        private void OnCopyUidClicked()
        {
            if (_authService != null && !string.IsNullOrEmpty(_authService.EosProductUserId))
            {
                GUIUtility.systemCopyBuffer = _authService.EosProductUserId;
                _uiService?.ShowNotification("Account UID copied to clipboard!", NotificationType.Success);
            }
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
            _accountNameLabel = GetElement<Label>("account-name");
            _accountUidLabel = GetElement<Label>("account-uid");
            _guestWarningBlock = GetElement<VisualElement>("guest-warning-block");
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

            _viewModel.IsGuest.Bind(isGuest =>
            {
                if (_guestWarningBlock != null)
                {
                    _guestWarningBlock.style.display = isGuest ? DisplayStyle.Flex : DisplayStyle.None;
                }
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
            
            _accountUidLabel?.RegisterCallback<ClickEvent>(_ => OnCopyUidClicked());
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
