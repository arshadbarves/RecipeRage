using System;
using KitchenClash.Application.Services;
using System.Collections.Generic;
using System.Linq;
using KitchenClash.Domain;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer.Unity;

namespace KitchenClash.Presentation.Common
{
    public class UIService : IUIService, IStartable, ITickable, IDisposable
    {
        private static readonly UIScreenCategory[] LayerOrder =
        {
            UIScreenCategory.Screen,
            UIScreenCategory.HUD,
            UIScreenCategory.Popup,
            UIScreenCategory.Modal,
            UIScreenCategory.Overlay,
            UIScreenCategory.System,
            UIScreenCategory.Toast
        };

        private UIDocument _uiDocument;
        private VisualElement _root;
        private readonly Dictionary<UIScreenCategory, VisualElement> _layerRoots = new();
        private readonly Dictionary<Type, UIScreenController> _controllers = new();
        private readonly Dictionary<Type, BaseUIScreen> _screens = new();
        private readonly IUIScreenStackManager _stackManager;
        private readonly VContainer.IObjectResolver _container;
        private VContainer.IObjectResolver _currentScope;

        public event Action<Type> OnScreenShown;
        public event Action<Type> OnScreenHidden;
        public event Action OnAllScreensHidden;

        private bool _isInitialized;

        public UIService(
            VContainer.IObjectResolver container,
            UIDocument uiDocument,
            IUIScreenStackManager stackManager)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _uiDocument = uiDocument;
            _stackManager = stackManager ?? throw new ArgumentNullException(nameof(stackManager));
        }

        public bool IsInitialized => _isInitialized && _root != null;

        public void SetCurrentScope(VContainer.IObjectResolver scope)
        {
            _currentScope = scope;
        }

        public void Start()
        {
            if (_isInitialized) return;

            if (_uiDocument == null)
            {
                GameLogger.LogError("UIDocument is null - cannot initialize UIService");
                return;
            }

            InitializeWithDocument(_uiDocument);
            InitializeScreens();
        }

        public void InitializeWithDocument(UIDocument uiDocument)
        {
            if (_isInitialized) return;

            _uiDocument = uiDocument;
            UIScreenRegistry.Initialize();
            SetupUIDocument();
        }

        public void InitializeScreens()
        {
            if (_isInitialized) return;

            if (_root == null)
            {
                GameLogger.LogError("Root element not ready.");
                return;
            }

            CreateScreenControllers();
            _isInitialized = true;
        }

        private void SetupUIDocument()
        {
            if (_uiDocument == null || _uiDocument.rootVisualElement == null) return;
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

            CreateLayerRoots();
        }

        private void CreateLayerRoots()
        {
            _layerRoots.Clear();

            foreach (UIScreenCategory category in LayerOrder)
            {
                VisualElement layer = new VisualElement();
                layer.name = $"ui-layer-{category.ToString().ToLowerInvariant()}";
                layer.AddToClassList("ui-layer");
                layer.pickingMode = PickingMode.Ignore;
                layer.style.position = Position.Absolute;
                layer.style.left = 0;
                layer.style.top = 0;
                layer.style.right = 0;
                layer.style.bottom = 0;
                layer.style.width = Length.Percent(100);
                layer.style.height = Length.Percent(100);

                _root.Add(layer);
                _layerRoots[category] = layer;
            }
        }

        private VisualElement GetLayerRoot(UIScreenCategory category)
        {
            if (_layerRoots.TryGetValue(category, out VisualElement layer)) return layer;
            return _root;
        }

        private void CreateScreenControllers()
        {
            foreach (Type screenType in UIScreenRegistry.GetRegisteredScreenTypes())
            {
                CreateScreenController(screenType);
            }
        }

        private void CreateScreenController(Type screenType)
        {
            UIScreenAttribute attribute = UIScreenRegistry.GetScreenAttribute(screenType);
            if (attribute == null) return;

            VisualTreeAsset template = LoadTemplateFromPath(attribute.TemplatePath);
            VisualElement layerRoot = GetLayerRoot(attribute.Category);

            var controller = new UIScreenController(screenType, attribute.Priority, attribute.Category, template, layerRoot);
            _controllers[screenType] = controller;
        }

        private BaseUIScreen ResolveScreen(Type screenType)
        {
            if (_screens.TryGetValue(screenType, out BaseUIScreen existing))
                return existing;

            UIScreenAttribute attribute = UIScreenRegistry.GetScreenAttribute(screenType);
            if (attribute == null) return null;
            if (!_controllers.TryGetValue(screenType, out UIScreenController controller)) return null;

            VContainer.IObjectResolver resolver = _currentScope ?? _container;
            var screen = (BaseUIScreen)resolver.Resolve(screenType);
            if (screen == null) return null;

            screen.Initialize(attribute.Priority, attribute.Category, controller);
            _screens[screenType] = screen;
            return screen;
        }

        private VisualTreeAsset LoadTemplateFromPath(string templatePath)
        {
            if (string.IsNullOrEmpty(templatePath)) return null;

            string resourcePath = $"UI/Templates/{templatePath}";
            return Resources.Load<VisualTreeAsset>(resourcePath);
        }

        public void Dispose()
        {
            foreach (BaseUIScreen screen in _screens.Values)
            {
                screen.Dispose();
            }

            _screens.Clear();
            _controllers.Clear();
            _layerRoots.Clear();
            _stackManager.ClearAll();
            _isInitialized = false;
        }

        public void SetRootScreen<T>(bool animate = true) where T : class => SetRootScreen(typeof(T), animate);
        public void SetRootScreen(Type screenType, bool animate = true)
        {
            HideVisibleScreensInCategory(UIScreenCategory.Screen, animate);
            _stackManager.ClearCategory(UIScreenCategory.Screen);
            ShowAndTrack(screenType, UIScreenCategory.Screen, animate, false);
        }

        public void PushScreen<T>(bool animate = true) where T : class => PushScreen(typeof(T), animate);
        public void PushScreen(Type screenType, bool animate = true)
        {
            Type current = _stackManager.Peek(UIScreenCategory.Screen);
            if (current != null && current != screenType && _screens.TryGetValue(current, out BaseUIScreen currentScreen))
            {
                HideScreenInternal(currentScreen, animate);
            }
            ShowAndTrack(screenType, UIScreenCategory.Screen, animate, false);
        }

        public void ShowSystem<T>(bool animate = true) where T : class => ShowSystem(typeof(T), animate);
        public void ShowSystem(Type screenType, bool animate = true)
        {
            HideVisibleScreensInCategory(UIScreenCategory.System, animate);
            _stackManager.ClearCategory(UIScreenCategory.System);
            ShowAndTrack(screenType, UIScreenCategory.System, animate, false);
        }

        public void HideSystem<T>(bool animate = true) where T : class => Hide(typeof(T), animate);
        public void HideSystem(Type screenType, bool animate = true) => Hide(screenType, animate);

        public void ShowOverlay<T>(bool animate = true) where T : class => ShowOverlay(typeof(T), animate);
        public void ShowOverlay(Type screenType, bool animate = true)
        {
            ShowAndTrack(screenType, UIScreenCategory.Overlay, animate, false);
        }

        public void HideOverlay<T>(bool animate = true) where T : class => Hide(typeof(T), animate);
        public void HideOverlay(Type screenType, bool animate = true) => Hide(screenType, animate);

        public void PushModal<T>(bool animate = true) where T : class => PushModal(typeof(T), animate);
        public void PushModal(Type screenType, bool animate = true)
        {
            ShowAndTrack(screenType, UIScreenCategory.Modal, animate, false);
        }

        public void PushPopup<T>(bool animate = true) where T : class => PushPopup(typeof(T), animate);
        public void PushPopup(Type screenType, bool animate = true)
        {
            ShowAndTrack(screenType, UIScreenCategory.Popup, animate, false);
        }

        public void ShowHud<T>(bool animate = true) where T : class => ShowHud(typeof(T), animate);
        public void ShowHud(Type screenType, bool animate = true)
        {
            HideVisibleScreensInCategory(UIScreenCategory.HUD, animate);
            _stackManager.ClearCategory(UIScreenCategory.HUD);
            ShowAndTrack(screenType, UIScreenCategory.HUD, animate, false);
        }

        public void HideHud<T>(bool animate = true) where T : class => Hide(typeof(T), animate);
        public void HideHud(Type screenType, bool animate = true) => Hide(screenType, animate);

        public bool Back(bool animate = true)
        {
            if (TryPopTopmost(UIScreenCategory.Modal, animate)) return true;
            if (TryPopTopmost(UIScreenCategory.Popup, animate)) return true;
            if (TryPopTopmost(UIScreenCategory.Overlay, animate)) return true;
            if (TryPopTopmost(UIScreenCategory.Screen, animate, true)) return true;
            return false;
        }

        public async UniTask ShowToast(string message, NotificationType type = NotificationType.Info, float duration = 3f)
        {
            INotificationScreen notificationScreen = GetScreen<INotificationScreen>();
            if (notificationScreen == null) return;
            EnsureNotificationHostMounted(notificationScreen);
            await notificationScreen.Show(message, type, duration);
        }

        public async UniTask ShowToast(string title, string message, NotificationType type = NotificationType.Info, float duration = 3f)
        {
            INotificationScreen notificationScreen = GetScreen<INotificationScreen>();
            if (notificationScreen == null) return;
            EnsureNotificationHostMounted(notificationScreen);
            await notificationScreen.Show(title, message, type, duration);
        }

        public void Show<T>(bool animate = true, bool addToHistory = true) where T : class => Show(typeof(T), animate, addToHistory);
        public void Show(Type screenType, bool animate = true, bool addToHistory = true)
        {
            UIScreenCategory? category = GetCategory(screenType);
            if (category == null) return;

            switch (category.Value)
            {
                case UIScreenCategory.System: ShowSystem(screenType, animate); break;
                case UIScreenCategory.Overlay: ShowOverlay(screenType, animate); break;
                case UIScreenCategory.Modal: PushModal(screenType, animate); break;
                case UIScreenCategory.Popup: PushPopup(screenType, animate); break;
                case UIScreenCategory.Screen:
                    if (addToHistory) PushScreen(screenType, animate);
                    else SetRootScreen(screenType, animate);
                    break;
                case UIScreenCategory.HUD: ShowHud(screenType, animate); break;
                case UIScreenCategory.Toast:
                    if (_screens.TryGetValue(screenType, out BaseUIScreen toastScreen))
                        ShowScreenInternal(toastScreen, false);
                    break;
            }
        }

        public void Hide<T>(bool animate = true) where T : class => Hide(typeof(T), animate);
        public void Hide(Type screenType, bool animate = true)
        {
            if (!_screens.TryGetValue(screenType, out BaseUIScreen screen)) return;
            UIScreenCategory? category = GetCategory(screenType);
            if (category == null) return;
            if (category != UIScreenCategory.Toast) _stackManager.PopSpecific(screenType, category.Value);
            HideScreenInternal(screen, animate);
        }

        public void HideAllPopups(bool animate = true)
        {
            HideVisibleScreensInCategory(UIScreenCategory.Popup, animate);
            _stackManager.ClearCategory(UIScreenCategory.Popup);
        }

        public void HideAllModals(bool animate = true)
        {
            HideVisibleScreensInCategory(UIScreenCategory.Modal, animate);
            _stackManager.ClearCategory(UIScreenCategory.Modal);
        }

        public void HideAllGameScreens(bool animate = true)
        {
            HideVisibleScreensInCategory(UIScreenCategory.Popup, animate);
            HideVisibleScreensInCategory(UIScreenCategory.Modal, animate);
            HideVisibleScreensInCategory(UIScreenCategory.Overlay, animate);
            HideVisibleScreensInCategory(UIScreenCategory.Screen, animate);
            HideVisibleScreensInCategory(UIScreenCategory.HUD, animate);
            _stackManager.ClearCategory(UIScreenCategory.Popup);
            _stackManager.ClearCategory(UIScreenCategory.Modal);
            _stackManager.ClearCategory(UIScreenCategory.Overlay);
            _stackManager.ClearCategory(UIScreenCategory.Screen);
            _stackManager.ClearCategory(UIScreenCategory.HUD);
        }

        public void HideAllScreens(bool animate = false)
        {
            foreach (BaseUIScreen screen in _screens.Values.Where(s => s != null && s.IsVisible))
            {
                HideScreenInternal(screen, animate);
            }
            _stackManager.ClearAll();
        }

        public T GetScreen<T>() where T : class
        {
            foreach (Type screenType in _controllers.Keys)
            {
                if (typeof(T).IsAssignableFrom(screenType))
                {
                    BaseUIScreen screen = ResolveScreen(screenType);
                    if (screen is T typedScreen) return typedScreen;
                }
            }

            foreach (BaseUIScreen screen in _screens.Values)
            {
                if (screen is T typedScreen) return typedScreen;
            }
            return null;
        }

        public bool IsScreenVisible<T>() where T : class => IsScreenVisible(typeof(T));
        public bool IsScreenVisible(Type screenType)
        {
            return _screens.TryGetValue(screenType, out BaseUIScreen screen) && screen.IsVisible;
        }

        public bool GoBack(bool animate = true) => Back(animate);

        public void ClearHistory()
        {
            _stackManager.ClearAll();
        }

        public async UniTask ShowNotification(string message, NotificationType type = NotificationType.Info, float duration = 3f)
        {
            await ShowToast(message, type, duration);
        }

        public async UniTask ShowNotification(string title, string message, NotificationType type = NotificationType.Info, float duration = 3f)
        {
            await ShowToast(title, message, type, duration);
        }

        public void Update(float deltaTime)
        {
            foreach (BaseUIScreen screen in _screens.Values.Where(s => s != null && s.IsVisible))
            {
                screen.Update(deltaTime);
            }
        }

        public void Tick()
        {
            Update(Time.deltaTime);
        }

        private void ShowAndTrack(Type screenType, UIScreenCategory category, bool animate, bool clearExisting)
        {
            BaseUIScreen screen = ResolveScreen(screenType);
            if (screen == null) return;

            if (clearExisting)
            {
                HideVisibleScreensInCategory(category, animate);
                _stackManager.ClearCategory(category);
            }

            if (!screen.IsVisible)
            {
                _stackManager.Push(screenType, category);
                ShowScreenInternal(screen, animate);
            }
        }

        private UIScreenCategory? GetCategory(Type screenType)
        {
            if (!_controllers.TryGetValue(screenType, out UIScreenController controller)) return null;
            return controller.Category;
        }

        private void HideVisibleScreensInCategory(UIScreenCategory category, bool animate)
        {
            foreach (BaseUIScreen screen in _screens.Values.Where(s => s != null && s.Category == category && s.IsVisible))
            {
                HideScreenInternal(screen, animate);
            }
        }

        private void EnsureNotificationHostMounted(INotificationScreen notificationScreen)
        {
            if (notificationScreen is BaseUIScreen screen && !screen.IsVisible)
            {
                ShowScreenInternal(screen, false);
            }
        }

        private void ShowScreenInternal(BaseUIScreen screen, bool animate)
        {
            if (screen.IsVisible) return;

            Type screenType = screen.GetType();
            if (!_controllers.TryGetValue(screenType, out UIScreenController controller)) return;

            controller.Container?.BringToFront();

            float duration = screen.GetAnimationDuration();
            screen.OnBeforeShowAnimation();

            controller.Show(screen.AnimateShow, duration, animate, () =>
            {
                controller.Container?.BringToFront();
                screen.OnAfterShowAnimation();
                OnScreenShown?.Invoke(screenType);
            });
        }

        private void HideScreenInternal(BaseUIScreen screen, bool animate)
        {
            if (!screen.IsVisible) return;

            Type screenType = screen.GetType();
            if (!_controllers.TryGetValue(screenType, out UIScreenController controller)) return;

            float duration = screen.GetAnimationDuration();
            screen.OnBeforeHideAnimation();

            controller.Hide(screen.AnimateHide, duration, animate, () =>
            {
                screen.OnAfterHideAnimation();
                OnScreenHidden?.Invoke(screenType);

                if (_screens.Values.Where(v => v.Category != UIScreenCategory.Toast).All(v => !v.IsVisible))
                {
                    OnAllScreensHidden?.Invoke();
                }

                if (!_stackManager.IsInHistory(screenType))
                {
                    screen.ResetState();
                }
            });
        }

        private bool TryPopTopmost(UIScreenCategory category, bool animate, bool requireHistory = false)
        {
            int stackDepth = _stackManager.GetStackDepth(category);
            if (stackDepth == 0 || (requireHistory && stackDepth <= 1)) return false;

            Type currentType = _stackManager.Pop(category);
            if (currentType != null && _screens.TryGetValue(currentType, out BaseUIScreen currentScreen))
            {
                HideScreenInternal(currentScreen, animate);
            }

            Type previousType = _stackManager.Peek(category);
            if (previousType != null && _screens.TryGetValue(previousType, out BaseUIScreen previousScreen))
            {
                ShowScreenInternal(previousScreen, animate);
            }

            return currentType != null;
        }
    }
}
