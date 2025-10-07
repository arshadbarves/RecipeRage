using System;
using System.Collections.Generic;
using System.Linq;
using Core.Patterns;
using Core.UI.Animation;
using UI.UISystem.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.UISystem
{
    /// <summary>
    /// Professional UI Manager using single UIDocument with priority-based sorting
    /// Supports Splash, Loading, Popup, Modal, and regular screens
    /// </summary>
    public class UIManager : MonoBehaviourSingleton<UIManager>
    {

        
        private UIDocument _uiDocument;
        private VisualElement _root;
        private readonly Dictionary<UIScreenType, UIScreenController> _controllers = new();
        private readonly Dictionary<UIScreenType, BaseUIScreen> _screens = new();
        private readonly List<BaseUIScreen> _visibleScreens = new();
        private readonly Stack<BaseUIScreen> _screenHistory = new();

        // Events
        public event Action<UIScreenType> OnScreenShown;
        public event Action<UIScreenType> OnScreenHidden;
        public event Action OnAllScreensHidden;

        protected override void Awake()
        {
            base.Awake();
            
            UIScreenRegistry.Initialize();
            
            InitializeUISystem();
        }

        private void InitializeUISystem()
        {
            // Get or create UIDocument
            _uiDocument = GetComponent<UIDocument>();
            if (_uiDocument == null)
            {
                _uiDocument = gameObject.AddComponent<UIDocument>();
            }

            // Wait for UI to be ready
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
            
            // Ensure root element fills the entire screen
            _root.style.width = Length.Percent(100);
            _root.style.height = Length.Percent(100);
            _root.style.position = Position.Absolute;
            _root.style.left = 0;
            _root.style.top = 0;
            _root.style.right = 0;
            _root.style.bottom = 0;
            _root.AddToClassList("ui-root");
            
            // Create all screen controllers
            CreateScreenControllers();
            
            // Hide all screens initially
            HideAllScreens();
            
            Debug.Log("[UIManager] UI System initialized successfully");
        }

        private void CreateScreenControllers()
        {
            // Create screens for all registered screen types
            foreach (UIScreenType screenType in UIScreenRegistry.GetRegisteredScreenTypes())
            {
                CreateScreen(screenType);
            }
            
            Debug.Log($"[UIManager] Created {_screens.Count} screens from registry");
        }

        private void CreateScreen(UIScreenType screenType)
        {
            // Get screen attribute for configuration
            UIScreenAttribute attribute = UIScreenRegistry.GetScreenAttribute(screenType);
            if (attribute == null)
            {
                Debug.LogError($"[UIManager] No attribute found for screen type {screenType}");
                return;
            }

            // Load template from attribute path
            VisualTreeAsset template = LoadTemplateFromPath(attribute.TemplatePath);

            // Create controller
            var controller = new UIScreenController(screenType, attribute.Priority, template, _root);
            _controllers[screenType] = controller;
            
            // Sort screens by priority after adding
            SortScreensByPriority();

            // Create screen class instance
            BaseUIScreen screen = UIScreenRegistry.CreateScreen(screenType);
            if (screen != null)
            {
                screen.Initialize(screenType, attribute.Priority, controller);
                _screens[screenType] = screen;
                
                Debug.Log($"[UIManager] Created screen: {screenType} -> {screen.GetType().Name}");
            }
            else
            {
                Debug.LogError($"[UIManager] Failed to create screen instance for {screenType}");
            }
        }

        private VisualTreeAsset LoadTemplateFromPath(string templatePath)
        {
            if (string.IsNullOrEmpty(templatePath))
            {
                Debug.LogWarning("[UIManager] Template path is null or empty");
                return null;
            }

            try
            {
                string resourcePath = $"UI/Templates/{templatePath}";
                VisualTreeAsset template = Resources.Load<VisualTreeAsset>(resourcePath);
                
                if (template != null)
                {
                    Debug.Log($"[UIManager] Loaded template '{templatePath}' from Resources at '{resourcePath}'");
                    return template;
                }
                else
                {
                    Debug.LogError($"[UIManager] Template '{templatePath}' not found at '{resourcePath}'. Make sure the template exists in Resources/UI/Templates/");
                    return null;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] Failed to load template '{templatePath}': {e.Message}");
                return null;
            }
        }

        protected override void OnDestroy()
        {
            // Dispose all screens
            foreach (BaseUIScreen screen in _screens.Values)
            {
                screen.Dispose();
            }
            
            _screens.Clear();
            _controllers.Clear();
            _visibleScreens.Clear();
            _screenHistory.Clear();
            
            base.OnDestroy();
        }

        #region Public API

        /// <summary>
        /// Show a screen with optional animation
        /// </summary>
        public void ShowScreen(UIScreenType screenType, bool animate = true, bool addToHistory = true)
        {
            if (!_screens.TryGetValue(screenType, out BaseUIScreen screen))
            {
                Debug.LogError($"[UIManager] Screen {screenType} not found");
                return;
            }

            // Add current visible screen to history if requested
            if (addToHistory && _visibleScreens.Count > 0)
            {
                BaseUIScreen currentScreen = _visibleScreens.LastOrDefault();
                if (currentScreen != null && currentScreen != screen)
                {
                    _screenHistory.Push(currentScreen);
                }
            }

            // Show the screen
            ShowScreenInternal(screen, animate);
        }

        /// <summary>
        /// Get a screen instance by type
        /// </summary>
        public T GetScreen<T>() where T : BaseUIScreen
        {
            foreach (BaseUIScreen screen in _screens.Values)
            {
                if (screen is T typedScreen)
                    return typedScreen;
            }
            return null;
        }

        /// <summary>
        /// Get a screen instance by screen type
        /// </summary>
        public BaseUIScreen GetScreen(UIScreenType screenType)
        {
            _screens.TryGetValue(screenType, out BaseUIScreen screen);
            return screen;
        }

        /// <summary>
        /// Hide a specific screen
        /// </summary>
        public void HideScreen(UIScreenType screenType, bool animate = true)
        {
            if (!_screens.TryGetValue(screenType, out BaseUIScreen screen))
            {
                Debug.LogError($"[UIManager] Screen {screenType} not found");
                return;
            }

            HideScreenInternal(screen, animate);
        }

        /// <summary>
        /// Hide all screens of a specific type (useful for popups/notifications)
        /// </summary>
        public void HideScreensOfType(UIScreenType screenType, bool animate = true)
        {
            var screensToHide = _visibleScreens.Where(s => s.ScreenType == screenType).ToList();
            foreach (BaseUIScreen screen in screensToHide)
            {
                HideScreenInternal(screen, animate);
            }
        }

        /// <summary>
        /// Hide all popup screens (Popup, Modal, Notification)
        /// </summary>
        public void HideAllPopups(bool animate = true)
        {
            UIScreenType[] popupTypes = new[] { UIScreenType.Popup, UIScreenType.Modal, UIScreenType.Notification };
            var popupsToHide = _visibleScreens.Where(s => popupTypes.Contains(s.ScreenType)).ToList();
            
            foreach (BaseUIScreen popup in popupsToHide)
            {
                HideScreenInternal(popup, animate);
            }
        }

        /// <summary>
        /// Hide all screens except system screens (Splash, Loading)
        /// </summary>
        public void HideAllGameScreens(bool animate = true)
        {
            UIScreenType[] systemTypes = new[] { UIScreenType.Splash, UIScreenType.Loading };
            var screensToHide = _visibleScreens.Where(s => !systemTypes.Contains(s.ScreenType)).ToList();
            
            foreach (BaseUIScreen screen in screensToHide)
            {
                HideScreenInternal(screen, animate);
            }
        }

        /// <summary>
        /// Hide all screens
        /// </summary>
        public void HideAllScreens(bool animate = false)
        {
            var screensToHide = _visibleScreens.ToList();
            foreach (BaseUIScreen screen in screensToHide)
            {
                HideScreenInternal(screen, animate);
            }
        }

        /// <summary>
        /// Go back to previous screen
        /// </summary>
        public bool GoBack(bool animate = true)
        {
            if (_screenHistory.Count == 0) return false;

            BaseUIScreen previousScreen = _screenHistory.Pop();
            
            // Hide current screen
            if (_visibleScreens.Count > 0)
            {
                BaseUIScreen currentScreen = _visibleScreens.Last();
                HideScreenInternal(currentScreen, animate);
            }

            // Show previous screen
            ShowScreenInternal(previousScreen, animate);
            return true;
        }

        /// <summary>
        /// Check if a screen is visible
        /// </summary>
        public bool IsScreenVisible(UIScreenType screenType)
        {
            return _screens.TryGetValue(screenType, out BaseUIScreen screen) && screen.IsVisible;
        }

        /// <summary>
        /// Get all visible screens sorted by priority
        /// </summary>
        public IReadOnlyList<BaseUIScreen> GetVisibleScreens()
        {
            return _visibleScreens.AsReadOnly();
        }

        /// <summary>
        /// Get all screens sorted by priority
        /// </summary>
        public IReadOnlyList<BaseUIScreen> GetScreensByPriority()
        {
            return _screens.Values.OrderByDescending(s => (int)s.Priority).ToList().AsReadOnly();
        }

        /// <summary>
        /// Clear screen history
        /// </summary>
        public void ClearHistory()
        {
            _screenHistory.Clear();
        }

        #endregion

        #region Internal Methods

        private void ShowScreenInternal(BaseUIScreen screen, bool animate)
        {
            if (screen.IsVisible) return;

            // Get the controller for this screen
            if (!_controllers.TryGetValue(screen.ScreenType, out UIScreenController controller))
            {
                Debug.LogError($"[UIManager] No controller found for screen {screen.ScreenType}");
                return;
            }

            SortScreensByPriority();

            // Let the screen determine its own animation configuration
            UnityNativeUIAnimationSystem.AnimationType animationType = screen.GetShowAnimationType();
            float duration = screen.GetAnimationDuration();

            // Call screen's pre-animation hook
            screen.OnBeforeShowAnimation();

            controller.Show(animationType, duration, animate, () =>
            {
                if (!_visibleScreens.Contains(screen))
                {
                    _visibleScreens.Add(screen);
                    _visibleScreens.Sort((a, b) => ((int)b.Priority).CompareTo((int)a.Priority));
                }
                
                // Call screen's post-animation hook
                screen.OnAfterShowAnimation();
                
                OnScreenShown?.Invoke(screen.ScreenType);
            });
        }

        private void HideScreenInternal(BaseUIScreen screen, bool animate)
        {
            if (!screen.IsVisible) return;

            // Get the controller for this screen
            if (!_controllers.TryGetValue(screen.ScreenType, out UIScreenController controller))
            {
                Debug.LogError($"[UIManager] No controller found for screen {screen.ScreenType}");
                return;
            }

            // Let the screen determine its own animation configuration
            UnityNativeUIAnimationSystem.AnimationType animationType = screen.GetHideAnimationType();
            float duration = screen.GetAnimationDuration();

            // Call screen's pre-animation hook
            screen.OnBeforeHideAnimation();

            controller.Hide(animationType, duration, animate, () =>
            {
                _visibleScreens.Remove(screen);
                
                // Call screen's post-animation hook
                screen.OnAfterHideAnimation();
                
                OnScreenHidden?.Invoke(screen.ScreenType);
                
                if (_visibleScreens.Count == 0)
                {
                    OnAllScreensHidden?.Invoke();
                }
            });
        }

        private void Update()
        {
            // Update all visible screens
            float deltaTime = Time.deltaTime;
            foreach (BaseUIScreen screen in _visibleScreens)
            {
                screen.Update(deltaTime);
            }
        }



        private void SortScreensByPriority()
        {
            // Sort by priority ASCENDING (lowest to highest)
            // BringToFront() moves element to end of parent's children list
            // So we call it from lowest to highest priority
            // Result: highest priority ends up at the end (rendered on top)
            var sortedControllers = _controllers.Values.OrderBy(c => (int)c.Priority).ToList();
            
            // Reorder in the visual tree
            foreach (UIScreenController controller in sortedControllers)
            {
                if (controller.Container?.parent != null)
                {
                    controller.Container.BringToFront();
                }
            }
            
            Debug.Log($"[UIManager] Sorted {sortedControllers.Count} screens by priority");
        }

        #endregion

        #region Debug Methods

        [ContextMenu("Debug UI State")]
        public void DebugUIState()
        {
            Debug.Log($"[UIManager] Visible Screens ({_visibleScreens.Count}):");
            foreach (BaseUIScreen screen in _visibleScreens)
            {
                Debug.Log($"  - {screen.ScreenType} (Priority: {screen.Priority})");
            }
            
            Debug.Log($"[UIManager] Screen History ({_screenHistory.Count}):");
            foreach (BaseUIScreen screen in _screenHistory)
            {
                Debug.Log($"  - {screen.ScreenType}");
            }
        }

        [ContextMenu("Debug Screen Sizes")]
        public void DebugScreenSizes()
        {
            Debug.Log($"[UIManager] Root Element Size: {_root.resolvedStyle.width}x{_root.resolvedStyle.height}");
            
            foreach (KeyValuePair<UIScreenType, UIScreenController> kvp in _controllers)
            {
                UIScreenController controller = kvp.Value;
                VisualElement container = controller.Container;
                Debug.Log($"[UIManager] {kvp.Key} Container Size: {container.resolvedStyle.width}x{container.resolvedStyle.height}, Position: ({container.resolvedStyle.left}, {container.resolvedStyle.top}), Display: {container.resolvedStyle.display}");
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
            Debug.Log("[UIManager] Forced resize of all screen containers");
        }

        #endregion
    }
}