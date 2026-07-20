using Playcenter.SDK;
using UnityEngine;
using UnityEngine.UIElements;

namespace Playcenter.SDK.Unity
{
    /// <summary>
    /// UI Toolkit implementation of IShellUi.
    /// Creates a DDOL GameObject with UIDocument if none is provided, then manages
    /// Splash/Loading UXML panels loaded from Resources.
    /// </summary>
    public sealed class ToolkitShellUi : IShellUi
    {
        const string LoadingUxmlPath  = "UI/Shell/LoadingShell";
        const string SplashUxmlPath   = "UI/Shell/SplashShell";
        const string DefaultUssPath   = "UI/Shell/DefaultShell";
        const string LoadingUssPath   = "UI/Shell/shell_loading";
        const string SplashUssPath    = "UI/Shell/shell_splash";

        readonly UIDocument _document;
        IShellTheme _theme;

        VisualTreeAsset _loadingAsset;
        VisualTreeAsset _splashAsset;
        StyleSheet      _defaultStyle;
        StyleSheet      _loadingStyle;
        StyleSheet      _splashStyle;

        ShellScreenId? _current;

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
                Object.DontDestroyOnLoad(go);
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

        void PreloadAssets()
        {
            _loadingAsset = Resources.Load<VisualTreeAsset>(LoadingUxmlPath);
            _splashAsset  = Resources.Load<VisualTreeAsset>(SplashUxmlPath);
            _defaultStyle = Resources.Load<StyleSheet>(DefaultUssPath);
            _loadingStyle = Resources.Load<StyleSheet>(LoadingUssPath);
            _splashStyle  = Resources.Load<StyleSheet>(SplashUssPath);
        }

        VisualTreeAsset ResolveTreeAsset(ShellScreenId id)
        {
            return id switch
            {
                ShellScreenId.Loading => _loadingAsset,
                ShellScreenId.Splash  => _splashAsset,
                _                     => null
            };
        }

        void ApplyStyleSheet(VisualElement root)
        {
            if (_defaultStyle != null && !root.styleSheets.Contains(_defaultStyle))
                root.styleSheets.Add(_defaultStyle);

            // Apply companion screen-specific USS after default
            var companion = _current switch
            {
                ShellScreenId.Loading => _loadingStyle,
                ShellScreenId.Splash  => _splashStyle,
                _                     => null
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
    }
}
