using System;
using System.Collections.Generic;
using System.Linq;
using Core.Animation;
using Core.Logging;
using Cysharp.Threading.Tasks;
using UI.Core;
using UI.Screens;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
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
                GameLogger.LogWarning("Already initialized");
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
                GameLogger.LogWarning("Screens already initialized");
                return;
            }

            if (_root == null)
            {
                GameLogger.LogError("Root element not ready. Call Initialize(UIDocument) first.");
                return;
            }

            CreateScreenControllers();
            HideAllScreens();

            _isInitialized = true;
            GameLogger.Log("UI screens initialized successfully");
        }

        private void SetupUIDocument()
        {
            if (_uiDocument == null)
            {
                GameLogger.LogError("UIDocument is null. Call Initialize(UIDocument) first.");
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

            GameLogger.Log("UIDocument ready");
        }

        private void CreateScreenControllers()
        {
            foreach (UIScreenType screenType in UIScreenRegistry.GetRegisteredScreenTypes())
            {
                CreateScreen(screenType);
            }

            GameLogger.Log($"Created {_screens.Count} screens from registry");
        }

        private void CreateScreen(UIScreenType screenType)
        {
            UIScreenAttribute attribute = UIScreenRegistry.GetScreenAttribute(screenType);
            if (attribute == null)
            {
                GameLogger.LogError($"No attribute found for screen type {screenType}");
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

                GameLogger.Log($"Created screen: {screenType} -> {screen.GetType().Name}");
            }
            else
            {
                GameLogger.LogError($"Failed to create screen instance for {screenType}");
            }
        }

        private VisualTreeAsset LoadTemplateFromPath(string templatePath)
        {
            if (string.IsNullOrEmpty(templatePath))
            {
                GameLogger.LogWarning("Template path is null or empty");
                return null;
            }

            try
            {
                // Template path should already include the category (Screens/, Components/, Popups/)
                string resourcePath = $"UI/Templates/{templatePath}";
                VisualTreeAsset template = Resources.Load<VisualTreeAsset>(resourcePath);

                if (template != null)
                {
                    GameLogger.Log($"Loaded template '{templatePath}' from Resources at '{resourcePath}'");
                    return template;
                }
                else
                {
                    GameLogger.LogError($"Template '{templatePath}' not found at '{resourcePath}'. Make sure the template exists in Resources/UI/Templates/ with proper category (Screens/, Components/, Popups/)");
                    return null;
                }
            }
            catch (System.Exception e)
            {
                GameLogger.LogError($"Failed to load template '{templatePath}': {e.Message}");
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
                GameLogger.LogError($"Screen {screenType} not found");
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
                GameLogger.LogError($"Screen {screenType} not found");
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

        public async UniTask ShowToast(string message, ToastType type = ToastType.Info, float duration = 3f)
        {
            var toastScreen = GetScreen<ToastScreen>(UIScreenType.Toast);
            if (toastScreen == null)
            {
                GameLogger.LogWarning("ToastScreen not found - make sure it's registered");
                return;
            }

            await toastScreen.Show(message, type, duration);
        }

        #endregion

        #region Internal Methods

        private void ShowScreenInternal(BaseUIScreen screen, bool animate)
        {
            if (screen.IsVisible) return;

            if (!_controllers.TryGetValue(screen.ScreenType, out UIScreenController controller))
            {
                GameLogger.LogError($"No controller found for screen {screen.ScreenType}");
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
                GameLogger.LogError($"No controller found for screen {screen.ScreenType}");
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

            GameLogger.Log($"Sorted {sortedControllers.Count} screens by priority");
        }

        #endregion

        #region Debug Methods

        [ContextMenu("Debug UI State")]
        public void DebugUIState()
        {
            GameLogger.Log($"Visible Screens ({_visibleScreens.Count}):");
            foreach (BaseUIScreen screen in _visibleScreens)
            {
                GameLogger.Log($"  - {screen.ScreenType} (Priority: {screen.Priority})");
            }

            GameLogger.Log($"Screen History ({_screenHistory.Count}):");
            foreach (BaseUIScreen screen in _screenHistory)
            {
                GameLogger.Log($"  - {screen.ScreenType}");
            }
        }

        [ContextMenu("Debug Screen Sizes")]
        public void DebugScreenSizes()
        {
            GameLogger.Log($"Root Element Size: {_root.resolvedStyle.width}x{_root.resolvedStyle.height}");

            foreach (KeyValuePair<UIScreenType, UIScreenController> kvp in _controllers)
            {
                UIScreenController controller = kvp.Value;
                VisualElement container = controller.Container;
                GameLogger.Log($"{kvp.Key} Container Size: {container.resolvedStyle.width}x{container.resolvedStyle.height}, Position: ({container.resolvedStyle.left}, {container.resolvedStyle.top}), Display: {container.resolvedStyle.display}");
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
            GameLogger.Log("Forced resize of all screen containers");
        }

        #endregion
    }
}