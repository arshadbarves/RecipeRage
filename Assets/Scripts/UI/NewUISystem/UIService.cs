using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Animation;
using Core.Utilities;
using UI.UISystem.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.UISystem
{
    /// <summary>
    /// Professional UI Service using single UIDocument with priority-based sorting
    /// Supports Splash, Loading, Popup, Modal, and regular screens
    /// Follows service-based architecture - no MonoBehaviour dependency
    /// </summary>
    public class UIService : IUIService, IDisposable
    {
        private UIDocument _uiDocument;
        private VisualElement _root;
        private readonly Dictionary<UIScreenType, UIScreenController> _controllers = new();
        private readonly Dictionary<UIScreenType, BaseUIScreen> _screens = new();
        private readonly List<BaseUIScreen> _visibleScreens = new();
        private readonly Stack<BaseUIScreen> _screenHistory = new();
        private readonly IAnimationService _animationService;

        // Events
        public event Action<UIScreenType> OnScreenShown;
        public event Action<UIScreenType> OnScreenHidden;
        public event Action OnAllScreensHidden;

        private bool _isInitialized = false;

        public UIService(IAnimationService animationService)
        {
            _animationService = animationService ?? throw new ArgumentNullException(nameof(animationService));
        }

        public void Initialize(UIDocument uiDocument)
        {
            if (_isInitialized)
            {
                Debug.LogWarning("[UIService] Already initialized");
                return;
            }

            _uiDocument = uiDocument;
            UIScreenRegistry.Initialize();
            SetupUIDocument();
        }

        public void InitializeScreens()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("[UIService] Screens already initialized");
                return;
            }

            if (_root == null)
            {
                Debug.LogError("[UIService] Root element not ready. Call Initialize(UIDocument) first.");
                return;
            }

            CreateScreenControllers();
            HideAllScreens();

            _isInitialized = true;
            Debug.Log("[UIService] UI screens initialized successfully");
        }

        private void SetupUIDocument()
        {
            if (_uiDocument == null)
            {
                Debug.LogError("[UIService] UIDocument is null. Call Initialize(UIDocument) first.");
                return;
            }

            if (_uiDocument.rootVisualElement != null)
            {
                OnUIDocumentReady();
            }
            else
            {
                _uiDocument.rootVisualElement?.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            }
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            _uiDocument.rootVisualElement.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            OnUIDocumentReady();
        }

        private void OnUIDocumentReady()
        {
            _root = _uiDocument.rootVisualElement;
            _root.name = "ui-root";
            _root.style.width = Length.Percent(100);
            _root.style.height = Length.Percent(100);
            _root.style.position = Position.Absolute;
            _root.style.left = 0;
            _root.style.top = 0;
            _root.style.right = 0;
            _root.style.bottom = 0;
            _root.AddToClassList("ui-root");

            Debug.Log("[UIService] UIDocument ready");
        }

        private void CreateScreenControllers()
        {
            foreach (UIScreenType screenType in UIScreenRegistry.GetRegisteredScreenTypes())
            {
                CreateScreen(screenType);
            }

            Debug.Log($"[UIService] Created {_screens.Count} screens from registry");
        }

        private void CreateScreen(UIScreenType screenType)
        {
            UIScreenAttribute attribute = UIScreenRegistry.GetScreenAttribute(screenType);
            if (attribute == null)
            {
                Debug.LogError($"[UIService] No attribute found for screen type {screenType}");
                return;
            }

            VisualTreeAsset template = LoadTemplateFromPath(attribute.TemplatePath);

            var controller = new UIScreenController(screenType, attribute.Priority, template, _root);
            _controllers[screenType] = controller;

            SortScreensByPriority();

            BaseUIScreen screen = UIScreenRegistry.CreateScreen(screenType);
            if (screen != null)
            {
                screen.Initialize(screenType, attribute.Priority, controller);
                _screens[screenType] = screen;

                Debug.Log($"[UIService] Created screen: {screenType} -> {screen.GetType().Name}");
            }
            else
            {
                Debug.LogError($"[UIService] Failed to create screen instance for {screenType}");
            }
        }

        private VisualTreeAsset LoadTemplateFromPath(string templatePath)
        {
            if (string.IsNullOrEmpty(templatePath))
            {
                Debug.LogWarning("[UIService] Template path is null or empty");
                return null;
            }

            try
            {
                string resourcePath = $"UI/Templates/{templatePath}";
                VisualTreeAsset template = Resources.Load<VisualTreeAsset>(resourcePath);

                if (template != null)
                {
                    Debug.Log($"[UIService] Loaded template '{templatePath}' from Resources at '{resourcePath}'");
                    return template;
                }
                else
                {
                    Debug.LogError($"[UIService] Template '{templatePath}' not found at '{resourcePath}'. Make sure the template exists in Resources/UI/Templates/");
                    return null;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIService] Failed to load template '{templatePath}': {e.Message}");
                return null;
            }
        }

        public void Dispose()
        {
            foreach (BaseUIScreen screen in _screens.Values)
            {
                screen.Dispose();
            }

            _screens.Clear();
            _controllers.Clear();
            _visibleScreens.Clear();
            _screenHistory.Clear();
            
            _isInitialized = false;
        }

        #region Public API

        public bool IsInitialized => _isInitialized && _root != null;

        public void ShowScreen(UIScreenType screenType, bool animate = true, bool addToHistory = true)
        {
            if (!_screens.TryGetValue(screenType, out BaseUIScreen screen))
            {
                Debug.LogError($"[UIService] Screen {screenType} not found");
                return;
            }

            if (addToHistory && _visibleScreens.Count > 0)
            {
                BaseUIScreen currentScreen = _visibleScreens.LastOrDefault();
                if (currentScreen != null && currentScreen != screen)
                {
                    _screenHistory.Push(currentScreen);
                }
            }

            ShowScreenInternal(screen, animate);
        }

        public T GetScreen<T>() where T : BaseUIScreen
        {
            foreach (BaseUIScreen screen in _screens.Values)
            {
                if (screen is T typedScreen)
                    return typedScreen;
            }
            return null;
        }

        public BaseUIScreen GetScreen(UIScreenType screenType)
        {
            _screens.TryGetValue(screenType, out BaseUIScreen screen);
            return screen;
        }

        public T GetScreen<T>(UIScreenType screenType) where T : BaseUIScreen
        {
            _screens.TryGetValue(screenType, out BaseUIScreen screen);
            return screen as T;
        }

        public void HideScreen(UIScreenType screenType, bool animate = true)
        {
            if (!_screens.TryGetValue(screenType, out BaseUIScreen screen))
            {
                Debug.LogError($"[UIService] Screen {screenType} not found");
                return;
            }

            HideScreenInternal(screen, animate);
        }

        public void HideScreensOfType(UIScreenType screenType, bool animate = true)
        {
            var screensToHide = _visibleScreens.Where(s => s.ScreenType == screenType).ToList();
            foreach (BaseUIScreen screen in screensToHide)
            {
                HideScreenInternal(screen, animate);
            }
        }

        public void HideAllPopups(bool animate = true)
        {
            UIScreenType[] popupTypes = new[] { UIScreenType.Popup, UIScreenType.Modal, UIScreenType.Notification };
            var popupsToHide = _visibleScreens.Where(s => popupTypes.Contains(s.ScreenType)).ToList();

            foreach (BaseUIScreen popup in popupsToHide)
            {
                HideScreenInternal(popup, animate);
            }
        }

        public void HideAllGameScreens(bool animate = true)
        {
            UIScreenType[] systemTypes = new[] { UIScreenType.Splash, UIScreenType.Loading };
            var screensToHide = _visibleScreens.Where(s => !systemTypes.Contains(s.ScreenType)).ToList();

            foreach (BaseUIScreen screen in screensToHide)
            {
                HideScreenInternal(screen, animate);
            }
        }

        public void HideAllScreens(bool animate = false)
        {
            var screensToHide = _visibleScreens.ToList();
            foreach (BaseUIScreen screen in screensToHide)
            {
                HideScreenInternal(screen, animate);
            }
        }

        public bool GoBack(bool animate = true)
        {
            if (_screenHistory.Count == 0) return false;

            BaseUIScreen previousScreen = _screenHistory.Pop();

            if (_visibleScreens.Count > 0)
            {
                BaseUIScreen currentScreen = _visibleScreens.Last();
                HideScreenInternal(currentScreen, animate);
            }

            ShowScreenInternal(previousScreen, animate);
            return true;
        }

        public bool IsScreenVisible(UIScreenType screenType)
        {
            return _screens.TryGetValue(screenType, out BaseUIScreen screen) && screen.IsVisible;
        }

        public IReadOnlyList<BaseUIScreen> GetVisibleScreens()
        {
            return _visibleScreens.AsReadOnly();
        }

        public IReadOnlyList<BaseUIScreen> GetScreensByPriority()
        {
            return _screens.Values.OrderByDescending(s => (int)s.Priority).ToList().AsReadOnly();
        }

        public void ClearHistory()
        {
            _screenHistory.Clear();
        }

        #endregion

        #region Internal Methods

        private void ShowScreenInternal(BaseUIScreen screen, bool animate)
        {
            if (screen.IsVisible) return;

            if (!_controllers.TryGetValue(screen.ScreenType, out UIScreenController controller))
            {
                Debug.LogError($"[UIService] No controller found for screen {screen.ScreenType}");
                return;
            }

            SortScreensByPriority();

            float duration = screen.GetAnimationDuration();
            screen.OnBeforeShowAnimation();

            controller.Show(_animationService.UI, screen.AnimateShow, duration, animate, () =>
            {
                if (!_visibleScreens.Contains(screen))
                {
                    _visibleScreens.Add(screen);
                    _visibleScreens.Sort((a, b) => ((int)b.Priority).CompareTo((int)a.Priority));
                }

                screen.OnAfterShowAnimation();
                OnScreenShown?.Invoke(screen.ScreenType);
            });
        }

        private void HideScreenInternal(BaseUIScreen screen, bool animate)
        {
            if (!screen.IsVisible) return;

            if (!_controllers.TryGetValue(screen.ScreenType, out UIScreenController controller))
            {
                Debug.LogError($"[UIService] No controller found for screen {screen.ScreenType}");
                return;
            }

            float duration = screen.GetAnimationDuration();
            screen.OnBeforeHideAnimation();

            controller.Hide(_animationService.UI, screen.AnimateHide, duration, animate, () =>
            {
                _visibleScreens.Remove(screen);
                screen.OnAfterHideAnimation();
                OnScreenHidden?.Invoke(screen.ScreenType);

                if (_visibleScreens.Count == 0)
                {
                    OnAllScreensHidden?.Invoke();
                }
            });
        }

        public void Update(float deltaTime)
        {
            foreach (BaseUIScreen screen in _visibleScreens)
            {
                screen.Update(deltaTime);
            }
        }

        private void SortScreensByPriority()
        {
            var sortedControllers = _controllers.Values.OrderBy(c => (int)c.Priority).ToList();

            foreach (UIScreenController controller in sortedControllers)
            {
                if (controller.Container?.parent != null)
                {
                    controller.Container.BringToFront();
                }
            }

            Debug.Log($"[UIService] Sorted {sortedControllers.Count} screens by priority");
        }

        #endregion

        #region Debug Methods

        [ContextMenu("Debug UI State")]
        public void DebugUIState()
        {
            Debug.Log($"[UIService] Visible Screens ({_visibleScreens.Count}):");
            foreach (BaseUIScreen screen in _visibleScreens)
            {
                Debug.Log($"  - {screen.ScreenType} (Priority: {screen.Priority})");
            }

            Debug.Log($"[UIService] Screen History ({_screenHistory.Count}):");
            foreach (BaseUIScreen screen in _screenHistory)
            {
                Debug.Log($"  - {screen.ScreenType}");
            }
        }

        [ContextMenu("Debug Screen Sizes")]
        public void DebugScreenSizes()
        {
            Debug.Log($"[UIService] Root Element Size: {_root.resolvedStyle.width}x{_root.resolvedStyle.height}");

            foreach (KeyValuePair<UIScreenType, UIScreenController> kvp in _controllers)
            {
                UIScreenController controller = kvp.Value;
                VisualElement container = controller.Container;
                Debug.Log($"[UIService] {kvp.Key} Container Size: {container.resolvedStyle.width}x{container.resolvedStyle.height}, Position: ({container.resolvedStyle.left}, {container.resolvedStyle.top}), Display: {container.resolvedStyle.display}");
            }
        }

        [ContextMenu("Force Resize All Screens")]
        public void ForceResizeAllScreens()
        {
            foreach (KeyValuePair<UIScreenType, UIScreenController> kvp in _controllers)
            {
                VisualElement container = kvp.Value.Container;
                container.style.width = Length.Percent(100);
                container.style.height = Length.Percent(100);
                container.style.position = Position.Absolute;
                container.style.left = 0;
                container.style.top = 0;
                container.style.right = 0;
                container.style.bottom = 0;
            }
            Debug.Log("[UIService] Forced resize of all screen containers");
        }

        #endregion
    }
}