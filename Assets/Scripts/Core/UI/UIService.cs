using System;
using System.Collections.Generic;
using System.Linq;
using Core.Logging;
using Core.UI.Core;
using Core.UI.Interfaces;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer.Unity;

namespace Core.UI
{
    public class UIService : IUIService, IStartable, IDisposable
    {
        private UIDocument _uiDocument;
        private VisualElement _root;
        private readonly Dictionary<Type, UIScreenController> _controllers = new();
        private readonly Dictionary<Type, BaseUIScreen> _screens = new();
        private readonly UIScreenStackManager _stackManager;
        private readonly VContainer.IObjectResolver _container;

        public event Action<Type> OnScreenShown;
        public event Action<Type> OnScreenHidden;
        public event Action OnAllScreensHidden;

        private bool _isInitialized = false;

        public UIService(VContainer.IObjectResolver container, UIDocument uiDocument)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _uiDocument = uiDocument;
            _stackManager = new UIScreenStackManager();
        }

        /// <summary>
        /// IStartable.Start - Called by VContainer after initialization.
        /// Ensures UIDocument is ready (OnEnable has run).
        /// </summary>
        public void Start()
        {
            if (_isInitialized) return;

            if (_uiDocument != null)
            {
                InitializeWithDocument(_uiDocument);
                InitializeScreens();
            }
            else
            {
                GameLogger.LogError("UIDocument is null - cannot initialize UIService");
            }
        }

        /// <summary>
        /// Initialize with a UIDocument (can be called manually if needed)
        /// </summary>
        public void InitializeWithDocument(UIDocument uiDocument)
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
                // Fix: Cannot register callback on null root.
                // Since we are in Start(), root SHOULD be ready. If not, we are in trouble.
                GameLogger.LogError("UIDocument.rootVisualElement is null in Start(). Ensure UIDocument is Enabled in scene.");
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
            foreach (Type screenType in UIScreenRegistry.GetRegisteredScreenTypes())
            {
                CreateScreen(screenType);
            }

            // Sort screens by category after all are created to ensure proper layering
            SortScreensByCategory();
        }

        private void CreateScreen(Type screenType)
        {
            UIScreenAttribute attribute = UIScreenRegistry.GetScreenAttribute(screenType);
            if (attribute == null)
            {
                GameLogger.LogError($"No attribute found for screen type {screenType.Name}");
                return;
            }

            VisualTreeAsset template = LoadTemplateFromPath(attribute.TemplatePath);

            var controller = new UIScreenController(screenType, attribute.Priority, attribute.Category, template, _root);
            _controllers[screenType] = controller;

            SortScreensByCategory();

            var screen = (BaseUIScreen)_container.Resolve(screenType);
            if (screen != null)
            {
                screen.Initialize(attribute.Priority, attribute.Category, controller);
                _screens[screenType] = screen;
            }
            else
            {
                GameLogger.LogError($"Failed to create screen instance for {screenType.Name}");
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
                string resourcePath = $"UI/Templates/{templatePath}";
                VisualTreeAsset template = Resources.Load<VisualTreeAsset>(resourcePath);

                if (template != null)
                {
                    return template;
                }
                else
                {
                    GameLogger.LogError($"Template '{templatePath}' not found at '{resourcePath}'.");
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
            _stackManager.ClearAll();

            _isInitialized = false;
        }

        #region Public API

        public bool IsInitialized => _isInitialized && _root != null;

        public void Show<T>(bool animate = true, bool addToHistory = true) where T : class
        {
            Show(typeof(T), animate, addToHistory);
        }

        public void Show(Type screenType, bool animate = true, bool addToHistory = true)
        {
            if (!_screens.TryGetValue(screenType, out BaseUIScreen screen))
            {
                GameLogger.LogError($"Screen {screenType.Name} not found");
                return;
            }

            // Get category for this screen
            UIScreenCategory category = _controllers[screenType].Category;

            // Handle category-specific logic
            switch (category)
            {
                case UIScreenCategory.System:
                    HideAllScreens(animate: false);
                    _stackManager.ClearAll();
                    break;

                case UIScreenCategory.Screen:
                    Type currentScreenType = _stackManager.Peek(UIScreenCategory.Screen);
                    if (currentScreenType != null && currentScreenType != screenType)
                    {
                        if (_screens.TryGetValue(currentScreenType, out BaseUIScreen current))
                        {
                            HideScreenInternal(current, animate);
                        }
                    }
                    break;

                case UIScreenCategory.Overlay:
                case UIScreenCategory.Modal:
                case UIScreenCategory.Popup:
                case UIScreenCategory.Persistent:
                    break;
            }

            if (addToHistory)
            {
                bool pushed = _stackManager.Push(screenType, category);
                if (!pushed)
                {
                    GameLogger.LogWarning($"Failed to push {screenType.Name} to stack");
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

        public void Hide<T>(bool animate = true) where T : class
        {
            Hide(typeof(T), animate);
        }

        public void Hide(Type screenType, bool animate = true)
        {
            if (!_screens.TryGetValue(screenType, out BaseUIScreen screen))
            {
                GameLogger.LogError($"Screen {screenType.Name} not found");
                return;
            }

            UIScreenCategory category = _controllers[screenType].Category;
            _stackManager.PopSpecific(screenType, category);

            HideScreenInternal(screen, animate);
        }

        public void HideAllPopups(bool animate = true)
        {
            var popups = _stackManager.GetVisibleInCategory(UIScreenCategory.Popup);
            foreach (Type popupType in popups)
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
            foreach (Type modalType in modals)
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
            _stackManager.ClearCategory(UIScreenCategory.Screen);
            _stackManager.ClearCategory(UIScreenCategory.Overlay);
            _stackManager.ClearCategory(UIScreenCategory.Modal);
            _stackManager.ClearCategory(UIScreenCategory.Popup);

            var allVisible = _stackManager.GetAllVisible();
            foreach (Type screenType in allVisible)
            {
                UIScreenCategory category = _controllers[screenType].Category;
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
            foreach (Type screenType in allVisible)
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
            GameLogger.Log("GoBack called");

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
                GameLogger.Log($"Category {category} stack depth: {depth}");

                if (depth > 1)
                {
                    Type current = _stackManager.Pop(category);
                    GameLogger.Log($"Popped {current?.Name} from {category}");

                    if (current != null && _screens.TryGetValue(current, out BaseUIScreen currentScreen))
                    {
                        HideScreenInternal(currentScreen, animate);
                    }

                    Type previous = _stackManager.Peek(category);
                    GameLogger.Log($"Showing previous screen: {previous?.Name}");

                    if (previous != null && _screens.TryGetValue(previous, out BaseUIScreen previousScreen))
                    {
                        ShowScreenInternal(previousScreen, animate);
                        return true;
                    }
                }
            }

            GameLogger.LogWarning("No screen to go back to");
            return false;
        }

        public bool IsScreenVisible<T>() where T : class
        {
            return IsScreenVisible(typeof(T));
        }

        public bool IsScreenVisible(Type screenType)
        {
            return _stackManager.IsVisible(screenType);
        }

        private IReadOnlyList<BaseUIScreen> GetVisibleScreens()
        {
            var visibleTypes = _stackManager.GetAllVisible();
            var visibleScreens = new List<BaseUIScreen>();

            foreach (Type screenType in visibleTypes)
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

        public async UniTask ShowNotification(string message, NotificationType type = NotificationType.Info, float duration = 3f)
        {
            var notificationScreen = GetScreen<INotificationScreen>();
            if (notificationScreen == null)
            {
                GameLogger.LogWarning("NotificationScreen not found - make sure it's registered");
                return;
            }

            await notificationScreen.Show(message, type, duration);
        }

        public async UniTask ShowNotification(string title, string message, NotificationType type = NotificationType.Info, float duration = 3f)
        {
            var notificationScreen = GetScreen<INotificationScreen>();
            if (notificationScreen == null)
            {
                GameLogger.LogWarning("NotificationScreen not found - make sure it's registered");
                return;
            }

            await notificationScreen.Show(title, message, type, duration);
        }

        #endregion

        #region Internal Methods

        private void ShowScreenInternal(BaseUIScreen screen, bool animate)
        {
            if (screen.IsVisible) return;

            Type screenType = screen.GetType();
            if (!_controllers.TryGetValue(screenType, out UIScreenController controller))
            {
                GameLogger.LogError($"No controller found for screen {screenType.Name}");
                return;
            }

            SortScreensByCategory();

            float duration = screen.GetAnimationDuration();
            screen.OnBeforeShowAnimation();

            controller.Show(screen.AnimateShow, duration, animate, () =>
            {
                screen.OnAfterShowAnimation();
                OnScreenShown?.Invoke(screenType);
            });
        }

        private void HideScreenInternal(BaseUIScreen screen, bool animate)
        {
            if (!screen.IsVisible) return;

            Type screenType = screen.GetType();
            if (!_controllers.TryGetValue(screenType, out UIScreenController controller))
            {
                GameLogger.LogError($"No controller found for screen {screenType.Name}");
                return;
            }

            float duration = screen.GetAnimationDuration();
            screen.OnBeforeHideAnimation();

            controller.Hide(screen.AnimateHide, duration, animate, () =>
            {
                screen.OnAfterHideAnimation();
                OnScreenHidden?.Invoke(screenType);

                if (_stackManager.GetAllVisible().Count == 0)
                {
                    OnAllScreensHidden?.Invoke();
                }

                // Check if we should reset the state
                if (!_stackManager.IsInHistory(screenType))
                {
                    screen.ResetState();
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
            var sortedControllers = _controllers.Values
                .OrderByDescending(c => (int)c.Category)
                .ThenBy(c => (int)c.Priority)
                .ToList();

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
            GameLogger.Log($"Root Element Size: {_root.resolvedStyle.width}x{_root.resolvedStyle.height}");

            foreach (KeyValuePair<Type, UIScreenController> kvp in _controllers)
            {
                UIScreenController controller = kvp.Value;
                VisualElement container = controller.Container;
                GameLogger.Log($"{kvp.Key.Name} Container Size: {container.resolvedStyle.width}x{container.resolvedStyle.height}, Position: ({container.resolvedStyle.left}, {container.resolvedStyle.top}), Display: {container.resolvedStyle.display}");
            }
        }

        [ContextMenu("Force Resize All Screens")]
        public void ForceResizeAllScreens()
        {
            foreach (KeyValuePair<Type, UIScreenController> kvp in _controllers)
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