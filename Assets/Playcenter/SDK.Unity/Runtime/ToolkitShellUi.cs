using Playcenter.SDK;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Playcenter.SDK.Unity
{
    /// <summary>
    /// UI Toolkit implementation of IShellUi.
    /// Creates a DDOL GameObject with UIDocument if none is provided, then manages
    /// Splash/Loading/Gates/Settings UXML panels loaded from Resources.
    /// </summary>
    public sealed class ToolkitShellUi : IShellUi
    {
        const string LoadingUxmlPath       = "UI/Shell/LoadingShell";
        const string SplashUxmlPath        = "UI/Shell/SplashShell";
        const string NoConnectionUxmlPath  = "UI/Shell/NoConnectionShell";
        const string ForceUpdateUxmlPath   = "UI/Shell/ForceUpdateShell";
        const string MaintenanceUxmlPath   = "UI/Shell/MaintenanceShell";
        const string SettingsUxmlPath      = "UI/Shell/SettingsShell";

        const string DefaultUssPath   = "UI/Shell/DefaultShell";
        const string LoadingUssPath   = "UI/Shell/shell_loading";
        const string SplashUssPath    = "UI/Shell/shell_splash";
        const string GateUssPath      = "UI/Shell/shell_gate";
        const string SettingsUssPath  = "UI/Shell/shell_settings";

        readonly UIDocument _document;
        IShellTheme _theme;
        IPlaycenterServices _services;

        VisualTreeAsset _loadingAsset;
        VisualTreeAsset _splashAsset;
        VisualTreeAsset _noConnectionAsset;
        VisualTreeAsset _forceUpdateAsset;
        VisualTreeAsset _maintenanceAsset;
        VisualTreeAsset _settingsAsset;

        StyleSheet _defaultStyle;
        StyleSheet _loadingStyle;
        StyleSheet _splashStyle;
        StyleSheet _gateStyle;
        StyleSheet _settingsStyle;

        ShellScreenId? _current;

        public Action OnRetryRequested;
        public Action OnQuitRequested;
        public Action OnUpdateRequested;

        /// <summary>Creates ToolkitShellUi and builds a DDOL UIDocument if none supplied.</summary>
        public ToolkitShellUi(UIDocument document = null)
        {
            if (document != null)
            {
                _document = document;
            }
            else
            {
                var go = new GameObject("[Playcenter Shell]");
                UnityEngine.Object.DontDestroyOnLoad(go);
                _document = go.AddComponent<UIDocument>();
                // Sort order above game UI so shell always renders on top
                _document.sortingOrder = 1000;
                var panelSettings = Resources.Load<PanelSettings>("UI/Shell/ShellPanelSettings");
                if (panelSettings != null)
                    _document.panelSettings = panelSettings;
            }

            PreloadAssets();
        }

        // ── IShellUi ──────────────────────────────────────────────────────────

        public void Show(ShellScreenId id)
        {
            _current = id;
            var root = _document.rootVisualElement;
            root.Clear();
            ApplyStyleSheet(root);

            var tree = ResolveTreeAsset(id);
            if (tree != null)
            {
                tree.CloneTree(root);
                WireButtons(id, root);
            }
            else
            {
                // Placeholder panel so gate screens don't NRE before Task 7
                var placeholder = new VisualElement();
                placeholder.style.flexGrow = 1;
                placeholder.style.alignItems = Align.Center;
                placeholder.style.justifyContent = Justify.Center;
                var label = new Label(id.ToString());
                label.style.fontSize = 24;
                placeholder.Add(label);
                root.Add(placeholder);
            }
        }

        public void Hide(ShellScreenId id)
        {
            if (_current == id)
                ClearRoot();
        }

        public void HideAll()
        {
            _current = null;
            ClearRoot();
        }

        public void SetProgress(float overall01, string status)
        {
            var root = _document.rootVisualElement;

            var statusLabel = root.Q<Label>("status-label");
            if (statusLabel != null)
                statusLabel.text = status ?? string.Empty;

            var bar = root.Q<ProgressBar>("progress-bar");
            if (bar != null)
                bar.value = overall01 * 100f;
        }

        public void SetTheme(IShellTheme theme)
        {
            _theme = theme;
            // Re-apply if a screen is already visible
            if (_current.HasValue)
                Show(_current.Value);
        }

        // ── Private helpers ───────────────────────────────────────────────────

        public void SetServices(IPlaycenterServices services)
        {
            _services = services;
        }

        void PreloadAssets()
        {
            _loadingAsset       = Resources.Load<VisualTreeAsset>(LoadingUxmlPath);
            _splashAsset        = Resources.Load<VisualTreeAsset>(SplashUxmlPath);
            _noConnectionAsset  = Resources.Load<VisualTreeAsset>(NoConnectionUxmlPath);
            _forceUpdateAsset   = Resources.Load<VisualTreeAsset>(ForceUpdateUxmlPath);
            _maintenanceAsset   = Resources.Load<VisualTreeAsset>(MaintenanceUxmlPath);
            _settingsAsset      = Resources.Load<VisualTreeAsset>(SettingsUxmlPath);

            _defaultStyle   = Resources.Load<StyleSheet>(DefaultUssPath);
            _loadingStyle   = Resources.Load<StyleSheet>(LoadingUssPath);
            _splashStyle    = Resources.Load<StyleSheet>(SplashUssPath);
            _gateStyle      = Resources.Load<StyleSheet>(GateUssPath);
            _settingsStyle  = Resources.Load<StyleSheet>(SettingsUssPath);
        }

        VisualTreeAsset ResolveTreeAsset(ShellScreenId id)
        {
            return id switch
            {
                ShellScreenId.Loading       => _loadingAsset,
                ShellScreenId.Splash        => _splashAsset,
                ShellScreenId.NoConnection  => _noConnectionAsset,
                ShellScreenId.ForceUpdate   => _forceUpdateAsset,
                ShellScreenId.Maintenance   => _maintenanceAsset,
                ShellScreenId.Settings      => _settingsAsset,
                _                           => null
            };
        }

        void ApplyStyleSheet(VisualElement root)
        {
            if (_defaultStyle != null && !root.styleSheets.Contains(_defaultStyle))
                root.styleSheets.Add(_defaultStyle);

            // Apply companion screen-specific USS after default
            var companion = _current switch
            {
                ShellScreenId.Loading       => _loadingStyle,
                ShellScreenId.Splash        => _splashStyle,
                ShellScreenId.NoConnection  => _gateStyle,
                ShellScreenId.ForceUpdate   => _gateStyle,
                ShellScreenId.Maintenance   => _gateStyle,
                ShellScreenId.Settings      => _settingsStyle,
                _                           => null
            };
            if (companion != null && !root.styleSheets.Contains(companion))
                root.styleSheets.Add(companion);

            if (_theme?.OverrideUssResourcesPath != null)
            {
                var overrideSheet = Resources.Load<StyleSheet>(_theme.OverrideUssResourcesPath);
                if (overrideSheet != null && !root.styleSheets.Contains(overrideSheet))
                    root.styleSheets.Add(overrideSheet);
            }
        }

        void ClearRoot()
        {
            _document.rootVisualElement?.Clear();
        }

        void WireButtons(ShellScreenId id, VisualElement root)
        {
            switch (id)
            {
                case ShellScreenId.NoConnection:
                case ShellScreenId.Maintenance:
                    root.Q<Button>("retry-button")?.RegisterCallback<ClickEvent>(_ => OnRetryRequested?.Invoke());
                    root.Q<Button>("quit-button")?.RegisterCallback<ClickEvent>(_ => OnQuitRequested?.Invoke());
                    break;

                case ShellScreenId.ForceUpdate:
                    root.Q<Button>("update-button")?.RegisterCallback<ClickEvent>(_ => OnUpdateRequested?.Invoke());
                    root.Q<Button>("quit-button")?.RegisterCallback<ClickEvent>(_ => OnQuitRequested?.Invoke());
                    break;

                case ShellScreenId.Settings:
                    WireSettings(root);
                    break;
            }
        }

        void WireSettings(VisualElement root)
        {
            root.Q<Button>("close-button")?.RegisterCallback<ClickEvent>(_ => HideAll());

            // ISettingsService lives in Playcenter.Services which SDK.Unity doesn't reference.
            // Use object + reflection to access it without direct dependency.
            if (_services == null)
            {
                return;
            }

            // Try to get ISettingsService dynamically
            var settingsServiceType = System.Type.GetType("Playcenter.Services.ISettingsService, Playcenter.Services");
            if (settingsServiceType == null)
            {
                return;
            }

            var tryGetMethod = typeof(IPlaycenterServices).GetMethod("TryGet")?.MakeGenericMethod(settingsServiceType);
            if (tryGetMethod == null)
            {
                return;
            }

            var parameters = new System.Object[] { null };
            var hasSettings = (bool)tryGetMethod.Invoke(_services, parameters);
            if (!hasSettings)
            {
                return;
            }

            var settingsService = parameters[0];
            if (settingsService == null)
            {
                return;
            }

            // Get Current property via reflection
            var currentProperty = settingsServiceType.GetProperty("Current");
            if (currentProperty == null)
            {
                return;
            }

            var current = currentProperty.GetValue(settingsService);
            if (current == null)
            {
                return;
            }

            var musicSlider = root.Q<Slider>("music-volume-slider");
            var sfxSlider = root.Q<Slider>("sfx-volume-slider");
            var musicValueLabel = root.Q<Label>("music-volume-value");
            var sfxValueLabel = root.Q<Label>("sfx-volume-value");

            var settingsType = current.GetType();
            var musicVolumeProp = settingsType.GetProperty("MusicVolume");
            var sfxVolumeProp = settingsType.GetProperty("SfxVolume");
            var cloneMethod = settingsType.GetMethod("Clone");
            var applyMethod = settingsServiceType.GetMethod("Apply");
            var saveAsyncMethod = settingsServiceType.GetMethod("SaveAsync");

            if (musicSlider != null && musicVolumeProp != null)
            {
                float musicVol = (float)musicVolumeProp.GetValue(current);
                musicSlider.value = musicVol;
                musicSlider.RegisterValueChangedCallback(evt =>
                {
                    var cloned = cloneMethod?.Invoke(current, null);
                    if (cloned != null)
                    {
                        musicVolumeProp.SetValue(cloned, evt.newValue);
                        applyMethod?.Invoke(settingsService, new[] { cloned });
                        saveAsyncMethod?.Invoke(settingsService, new System.Object[] { System.Threading.CancellationToken.None });
                    }
                    if (musicValueLabel != null)
                        musicValueLabel.text = $"{Mathf.RoundToInt(evt.newValue * 100)}%";
                });
                if (musicValueLabel != null)
                    musicValueLabel.text = $"{Mathf.RoundToInt(musicVol * 100)}%";
            }

            if (sfxSlider != null && sfxVolumeProp != null)
            {
                float sfxVol = (float)sfxVolumeProp.GetValue(current);
                sfxSlider.value = sfxVol;
                sfxSlider.RegisterValueChangedCallback(evt =>
                {
                    var cloned = cloneMethod?.Invoke(current, null);
                    if (cloned != null)
                    {
                        sfxVolumeProp.SetValue(cloned, evt.newValue);
                        applyMethod?.Invoke(settingsService, new[] { cloned });
                        saveAsyncMethod?.Invoke(settingsService, new System.Object[] { System.Threading.CancellationToken.None });
                    }
                    if (sfxValueLabel != null)
                        sfxValueLabel.text = $"{Mathf.RoundToInt(evt.newValue * 100)}%";
                });
                if (sfxValueLabel != null)
                    sfxValueLabel.text = $"{Mathf.RoundToInt(sfxVol * 100)}%";
            }

            var versionLabel = root.Q<Label>("version-label");
            if (versionLabel != null)
            {
                string version = Application.version;
                if (_services.TryGet<IAppVersion>(out var appVersion))
                    version = appVersion.Current;
                versionLabel.text = version;
            }
        }
    }
}
