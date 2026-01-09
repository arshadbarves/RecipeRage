using System;
using System.Collections.Generic;
using System.Linq;
using Modules.Animation;
using Modules.Shared.Interfaces;
using Modules.Logging;
using Cysharp.Threading.Tasks;
using Modules.UI.Core;
using Modules.UI.Interfaces;
using UnityEngine;
using UnityEngine.UIElements;

namespace Modules.UI
{
    /// <summary>
    /// Professional UI Service using category-based stack management
    /// Each category (System, Overlay, Modal, Popup, Screen, Persistent) has its own stack
    /// Eliminates priority conflicts and scales to 100+ screens
    /// Follows service-based architecture - no MonoBehaviour dependency
    /// </summary>
    public class UIService : IUIService, IDisposable
    {
        private UIDocument _uiDocument;
        private VisualElement _root;
        private readonly Dictionary<UIScreenType, UIScreenController> _controllers = new();
        private readonly Dictionary<UIScreenType, BaseUIScreen> _screens = new();
        private readonly UIScreenStackManager _stackManager;
        private readonly IAnimationService _animationService;
        private readonly VContainer.IObjectResolver _container;
        private readonly ILoggingService _loggingService;

        public event Action<UIScreenType> OnScreenShown;
        public event Action<UIScreenType> OnScreenHidden;
        public event Action OnAllScreensHidden;

        private bool _isInitialized = false;

        public UIService(IAnimationService animationService, VContainer.IObjectResolver container, ILoggingService loggingService)
        {
            _animationService = animationService ?? throw new ArgumentNullException(nameof(animationService));
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _stackManager = new UIScreenStackManager();
        }

        /// <summary>
        /// Called after all services are constructed (IInitializable).
        /// </summary>
        void IInitializable.Initialize()
        {
            // UIService uses Initialize(object) for document-specific setup
        }

        public void Initialize(object uiDocumentObj)
        {
            if (_isInitialized)
            {
                _loggingService.LogWarning("Already initialized");
                return;
            }

            if (uiDocumentObj is UIDocument uiDocument)
            {
                _uiDocument = uiDocument;
                UIScreenRegistry.Initialize();
                SetupUIDocument();
            }
            else
            {
                _loggingService.LogError("Initialize called with invalid type. Expected UIDocument.");
            }
        }

        public void InitializeScreens()
        {
            if (_isInitialized)
            {
                _loggingService.LogWarning("Screens already initialized");
                return;
            }

            if (_root == null)
            {
                _loggingService.LogError("Root element not ready. Call Initialize(UIDocument) first.");
                return;
            }

            CreateScreenControllers();
            HideAllScreens();

            _isInitialized = true;
        }

        private void SetupUIDocument()
        {
            if (_uiDocument == null)
            {
                _loggingService.LogError("UIDocument is null. Call Initialize(UIDocument) first.");
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
        }

        private void CreateScreenControllers()
        {
            foreach (UIScreenType screenType in UIScreenRegistry.GetRegisteredScreenTypes())
            {
                CreateScreen(screenType);
            }

            // Sort screens by category after all are created to ensure proper layering
            SortScreensByCategory();
        }

        private void CreateScreen(UIScreenType screenType)
        {
            UIScreenAttribute attribute = UIScreenRegistry.GetScreenAttribute(screenType);
            if (attribute == null)
            {
                _loggingService.LogError($"No attribute found for screen type {screenType}");
                return;
            }

            VisualTreeAsset template = LoadTemplateFromPath(attribute.TemplatePath);

            var controller = new UIScreenController(screenType, attribute.Priority, template, _root);
            _controllers[screenType] = controller;

            SortScreensByCategory();

            Type screenClass = UIScreenRegistry.GetScreenClassType(screenType);
            if (screenClass == null)
            {
                _loggingService.LogError($"No screen class registered for {screenType}");
                return;
            }

            BaseUIScreen screen = (BaseUIScreen)_container.Resolve(screenClass);
            if (screen != null)
            {
                screen.Initialize(screenType, attribute.Priority, controller);
                _screens[screenType] = screen;
            }
            else
            {
                _loggingService.LogError($"Failed to create screen instance for {screenType}");
            }
        }

        private VisualTreeAsset LoadTemplateFromPath(string templatePath)
        {
            if (string.IsNullOrEmpty(templatePath))
            {
                _loggingService.LogWarning("Template path is null or empty");
                return null;
            }

            try
            {
                // Template path should already include the category (Screens/, Components/, Popups/)
                string resourcePath = $"UI/Templates/{templatePath}";
                VisualTreeAsset template = Resources.Load<VisualTreeAsset>(resourcePath);

                if (template != null)
                {
                    return template;
                }
                else
                {
                    _loggingService.LogError($"Template '{templatePath}' not found at '{resourcePath}'. Make sure the template exists in Resources/UI/Templates/ with proper category (Screens/, Components/, Popups/)");
                    return null;
                }
            }
            catch (System.Exception e)
            {
                _loggingService.LogError($"Failed to load template '{templatePath}': {e.Message}");
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
            _stackManager.ClearAll();

            _isInitialized = false;
        }

        #region Public API

        public bool IsInitialized => _isInitialized && _root != null;

        public void ShowScreen(UIScreenType screenType, bool animate = true, bool addToHistory = true)
        {
            if (!_screens.TryGetValue(screenType, out BaseUIScreen screen))
            {
                _loggingService.LogError($"Screen {screenType} not found");
                return;
            }

            // Get category for this screen
            UIScreenCategory category = _stackManager.GetCategory(screenType);

            // Handle category-specific logic
            switch (category)
            {
                case UIScreenCategory.System:
                    // System screens replace all other UI
                    HideAllScreens(animate: false);
                    _stackManager.ClearAll();
                    break;

                case UIScreenCategory.Screen:
                    // Only one main screen at a time
                    UIScreenType? currentScreen = _stackManager.Peek(UIScreenCategory.Screen);
                    if (currentScreen.HasValue && currentScreen.Value != screenType)
                    {
                        if (_screens.TryGetValue(currentScreen.Value, out BaseUIScreen current))
                        {
                            HideScreenInternal(current, animate);
                        }
                    }
                    break;

                case UIScreenCategory.Overlay:
                case UIScreenCategory.Modal:
                case UIScreenCategory.Popup:
                case UIScreenCategory.Persistent:
                    // These can stack or coexist
                    break;
            }

            if (addToHistory)
            {
                bool pushed = _stackManager.Push(screenType, category);
                if (!pushed)
                {
                    _loggingService.LogWarning($"Failed to push {screenType} to stack");
                    return;
                }
            }

            ShowScreenInternal(screen, animate);
        }

        public T GetScreen<T>() where T : class
        {
            foreach (BaseUIScreen screen in _screens.Values)
            {
                if (screen is T typedScreen)
                    return typedScreen;
            }
            return null;
        }

        public object GetScreen(UIScreenType screenType)
        {
            _screens.TryGetValue(screenType, out BaseUIScreen screen);
            return screen;
        }

        public T GetScreen<T>(UIScreenType screenType) where T : class
        {
            _screens.TryGetValue(screenType, out BaseUIScreen screen);
            return screen as T;
        }

        public void HideScreen(UIScreenType screenType, bool animate = true)
        {
            if (!_screens.TryGetValue(screenType, out BaseUIScreen screen))
            {
                _loggingService.LogError($"Screen {screenType} not found");
                return;
            }

            UIScreenCategory category = _stackManager.GetCategory(screenType);
            bool removed = _stackManager.PopSpecific(screenType, category);

            if (!removed)
            {
                _loggingService.LogWarning($"Screen {screenType} not in stack");
            }

            HideScreenInternal(screen, animate);
        }

        public void HideScreensOfType(UIScreenType screenType, bool animate = true)
        {
            if (_stackManager.IsVisible(screenType))
            {
                HideScreen(screenType, animate);
            }
        }

        public void HideAllPopups(bool animate = true)
        {
            var popups = _stackManager.GetVisibleInCategory(UIScreenCategory.Popup);
            foreach (UIScreenType popupType in popups)
            {
                if (_screens.TryGetValue(popupType, out BaseUIScreen popup))
                {
                    HideScreenInternal(popup, animate);
                }
            }
            _stackManager.ClearCategory(UIScreenCategory.Popup);
        }

        public void HideAllModals(bool animate = true)
        {
            var modals = _stackManager.GetVisibleInCategory(UIScreenCategory.Modal);
            foreach (UIScreenType modalType in modals)
            {
                if (_screens.TryGetValue(modalType, out BaseUIScreen modal))
                {
                    HideScreenInternal(modal, animate);
                }
            }
            _stackManager.ClearCategory(UIScreenCategory.Modal);
        }

        public void HideAllGameScreens(bool animate = true)
        {
            // Clear all categories except System and Persistent
            _stackManager.ClearCategory(UIScreenCategory.Screen);
            _stackManager.ClearCategory(UIScreenCategory.Overlay);
            _stackManager.ClearCategory(UIScreenCategory.Modal);
            _stackManager.ClearCategory(UIScreenCategory.Popup);

            var allVisible = _stackManager.GetAllVisible();
            foreach (UIScreenType screenType in allVisible)
            {
                UIScreenCategory category = _stackManager.GetCategory(screenType);
                if (category != UIScreenCategory.Persistent && category != UIScreenCategory.System)
                {
                    if (_screens.TryGetValue(screenType, out BaseUIScreen screen))
                    {
                        HideScreenInternal(screen, animate);
                    }
                }
            }
        }

        public void HideAllScreens(bool animate = false)
        {
            var allVisible = _stackManager.GetAllVisible();
            foreach (UIScreenType screenType in allVisible)
            {
                if (_screens.TryGetValue(screenType, out BaseUIScreen screen))
                {
                    HideScreenInternal(screen, animate);
                }
            }
            _stackManager.ClearAll();
        }

        public bool GoBack(bool animate = true)
        {
            _loggingService.Log("GoBack called");

            // Try to go back in each category (highest priority first)
            UIScreenCategory[] categories = new[]
            {
                UIScreenCategory.Modal,
                UIScreenCategory.Popup,
                UIScreenCategory.Overlay,
                UIScreenCategory.Screen
            };

            foreach (UIScreenCategory category in categories)
            {
                int depth = _stackManager.GetStackDepth(category);
                _loggingService.Log($"Category {category} stack depth: {depth}");

                if (depth > 1)
                {
                    UIScreenType? current = _stackManager.Pop(category);
                    _loggingService.Log($"Popped {current} from {category}");

                    if (current.HasValue && _screens.TryGetValue(current.Value, out BaseUIScreen currentScreen))
                    {
                        HideScreenInternal(currentScreen, animate);
                    }

                    UIScreenType? previous = _stackManager.Peek(category);
                    _loggingService.Log($"Showing previous screen: {previous}");

                    if (previous.HasValue && _screens.TryGetValue(previous.Value, out BaseUIScreen previousScreen))
                    {
                        ShowScreenInternal(previousScreen, animate);
                        return true;
                    }
                }
            }

            _loggingService.LogWarning("No screen to go back to");
            return false;
        }

        public bool IsScreenVisible(UIScreenType screenType)
        {
            return _stackManager.IsVisible(screenType);
        }

        public IReadOnlyList<BaseUIScreen> GetVisibleScreens()
        {
            var visibleTypes = _stackManager.GetAllVisible();
            var visibleScreens = new List<BaseUIScreen>();

            foreach (UIScreenType screenType in visibleTypes)
            {
                if (_screens.TryGetValue(screenType, out BaseUIScreen screen))
                {
                    visibleScreens.Add(screen);
                }
            }

            return visibleScreens.AsReadOnly();
        }

        public IReadOnlyList<BaseUIScreen> GetScreensByPriority()
        {
            return _screens.Values.OrderByDescending(s => (int)s.Priority).ToList().AsReadOnly();
        }

        public void ClearHistory()
        {
            _stackManager.ClearAll();
        }

        public UIScreenCategory GetScreenCategory(UIScreenType screenType)
        {
            return _stackManager.GetCategory(screenType);
        }

        public bool IsInteractionBlocked(UIScreenType screenType)
        {
            UIScreenCategory category = _stackManager.GetCategory(screenType);
            return _stackManager.IsBlockedByHigherCategory(category);
        }

        public async UniTask ShowNotification(string message, NotificationType type = NotificationType.Info, float duration = 3f)
        {
            var notificationScreen = GetScreen<INotificationScreen>(UIScreenType.Notification);
            if (notificationScreen == null)
            {
                _loggingService.LogWarning("NotificationScreen not found - make sure it's registered");
                return;
            }

            await notificationScreen.Show(message, type, duration);
        }

        public async UniTask ShowNotification(string title, string message, NotificationType type = NotificationType.Info, float duration = 3f)
        {
            var notificationScreen = GetScreen<INotificationScreen>(UIScreenType.Notification);
            if (notificationScreen == null)
            {
                _loggingService.LogWarning("NotificationScreen not found - make sure it's registered");
                return;
            }

            await notificationScreen.Show(title, message, type, duration);
        }

        #endregion

        #region Internal Methods

        private void ShowScreenInternal(BaseUIScreen screen, bool animate)
        {
            if (screen.IsVisible) return;

            if (!_controllers.TryGetValue(screen.ScreenType, out UIScreenController controller))
            {
                _loggingService.LogError($"No controller found for screen {screen.ScreenType}");
                return;
            }

            // Ensure proper layering before showing
            SortScreensByCategory();

            float duration = screen.GetAnimationDuration();
            screen.OnBeforeShowAnimation();

            controller.Show(screen.AnimateShow, duration, animate, () =>
            {
                screen.OnAfterShowAnimation();
                OnScreenShown?.Invoke(screen.ScreenType);
            });
        }

        private void HideScreenInternal(BaseUIScreen screen, bool animate)
        {
            if (!screen.IsVisible) return;

            if (!_controllers.TryGetValue(screen.ScreenType, out UIScreenController controller))
            {
                _loggingService.LogError($"No controller found for screen {screen.ScreenType}");
                return;
            }

            float duration = screen.GetAnimationDuration();
            screen.OnBeforeHideAnimation();

            controller.Hide(screen.AnimateHide, duration, animate, () =>
            {
                screen.OnAfterHideAnimation();
                OnScreenHidden?.Invoke(screen.ScreenType);

                if (_stackManager.GetAllVisible().Count == 0)
                {
                    OnAllScreensHidden?.Invoke();
                }
            });
        }

        public void Update(float deltaTime)
        {
            var visibleScreens = GetVisibleScreens();
            foreach (BaseUIScreen screen in visibleScreens)
            {
                screen.Update(deltaTime);
            }
        }

        private void SortScreensByCategory()
        {
            // Sort controllers by category priority
            // Lower category enum value = higher visual priority (appears on top)
            // Order: Persistent(5) -> Screen(4) -> Popup(3) -> Modal(2) -> Overlay(1) -> System(0)
            var sortedControllers = _controllers.Values
                .OrderByDescending(c => (int)_stackManager.GetCategory(c.ScreenType))
                .ThenBy(c => (int)c.Priority)
                .ToList();

            // BringToFront in order so highest priority categories are on top
            foreach (UIScreenController controller in sortedControllers)
            {
                if (controller.Container?.parent != null)
                {
                    controller.Container.BringToFront();
                }
            }
        }

        #endregion

        #region Debug Methods

        [ContextMenu("Debug UI State")]
        public void DebugUIState()
        {
            _stackManager.DebugPrintState();
        }

        [ContextMenu("Debug Screen Sizes")]
        public void DebugScreenSizes()
        {
            _loggingService.Log($"Root Element Size: {_root.resolvedStyle.width}x{_root.resolvedStyle.height}");

            foreach (KeyValuePair<UIScreenType, UIScreenController> kvp in _controllers)
            {
                UIScreenController controller = kvp.Value;
                VisualElement container = controller.Container;
                _loggingService.Log($"{kvp.Key} Container Size: {container.resolvedStyle.width}x{container.resolvedStyle.height}, Position: ({container.resolvedStyle.left}, {container.resolvedStyle.top}), Display: {container.resolvedStyle.display}");
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
            _loggingService.Log("Forced resize of all screen containers");
        }

        #endregion
    }
}