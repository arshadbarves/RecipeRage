using System;
using Core.Animation;
using Core.Bootstrap;
using UI;
using UnityEngine.UIElements;
using VContainer;

namespace UI.Core
{
    /// <summary>
    /// Pure C# base class for all UI screens - no MonoBehaviour dependency
    /// Professional screen-class based architecture
    /// </summary>
    public abstract class BaseUIScreen
    {
        #region Properties

        /// <summary>
        /// Screen type identifier
        /// </summary>
        public UIScreenType ScreenType { get; private set; }

        /// <summary>
        /// Screen category for animation and layering
        /// </summary>
        public UIScreenCategory Category { get; private set; }

        /// <summary>
        /// Screen priority for layering
        /// </summary>
        public UIScreenPriority Priority { get; private set; }

        /// <summary>
        /// Whether the screen is currently visible
        /// </summary>
        public bool IsVisible { get; private set; }

        /// <summary>
        /// Whether the screen is currently animating
        /// </summary>
        public bool IsAnimating { get; private set; }

        /// <summary>
        /// The UI controller managing this screen's visual elements
        /// </summary>
        protected UIScreenController Controller { get; private set; }

        /// <summary>
        /// The main container element for this screen
        /// </summary>
        public VisualElement Container => Controller?.Container;

        /// <summary>
        /// The template container element for this screen (for controlling picking mode, etc.)
        /// </summary>
        public TemplateContainer TemplateContainer => Controller?.TemplateContainer;

        #endregion

        #region Events

        /// <summary>
        /// Event triggered when the screen is shown
        /// </summary>
        public event Action<BaseUIScreen> OnShown;

        /// <summary>
        /// Event triggered when the screen is hidden
        /// </summary>
        public event Action<BaseUIScreen> OnHidden;

        /// <summary>
        /// Event triggered when screen starts animating
        /// </summary>
        public event Action<BaseUIScreen> OnAnimationStarted;

        /// <summary>
        /// Event triggered when screen finishes animating
        /// </summary>
        public event Action<BaseUIScreen> OnAnimationCompleted;

        #endregion

        #region Lifecycle

        /// <summary>
        /// Initialize the screen with its controller
        /// Called automatically by the UI system
        /// </summary>
        public void Initialize(UIScreenType screenType, UIScreenPriority priority, UIScreenController controller)
        {
            ScreenType = screenType;
            Priority = priority;
            Controller = controller;

            // Get category from UIService
            if (GameBootstrap.Container != null)
            {
                var uiService = GameBootstrap.Container.Resolve<IUIService>();
                if (uiService != null)
                {
                    Category = uiService.GetScreenCategory(screenType);
                }
            }

            // Subscribe to controller events
            if (Controller != null)
            {
                Controller.OnShown += OnControllerShown;
                Controller.OnHidden += OnControllerHidden;
                Controller.OnAnimationStarted += OnControllerAnimationStarted;
                Controller.OnAnimationCompleted += OnControllerAnimationCompleted;
            }

            // Initialize screen-specific logic
            OnInitialize();
        }

        /// <summary>
        /// Called when the screen is first initialized
        /// Override this to set up screen-specific logic
        /// </summary>
        protected virtual void OnInitialize()
        {
            // Override in derived classes
        }

        /// <summary>
        /// Called when the screen is shown
        /// Override this for custom show logic
        /// </summary>
        protected virtual void OnShow()
        {
            // Override in derived classes
        }

        /// <summary>
        /// Called when the screen is hidden
        /// Override this for custom hide logic
        /// </summary>
        protected virtual void OnHide()
        {
            // Override in derived classes
        }

        /// <summary>
        /// Called every frame while the screen is visible
        /// Override this for update logic
        /// </summary>
        public virtual void Update(float deltaTime)
        {
            // Override in derived classes
        }

        /// <summary>
        /// Clean up resources when screen is destroyed
        /// </summary>
        public virtual void Dispose()
        {
            if (Controller != null)
            {
                Controller.OnShown -= OnControllerShown;
                Controller.OnHidden -= OnControllerHidden;
                Controller.OnAnimationStarted -= OnControllerAnimationStarted;
                Controller.OnAnimationCompleted -= OnControllerAnimationCompleted;
            }

            OnDispose();
        }

        /// <summary>
        /// Called when the screen is disposed
        /// Override this for cleanup logic
        /// </summary>
        protected virtual void OnDispose()
        {
            // Override in derived classes
        }

        #endregion

        #region Public API

        /// <summary>
        /// Show this screen
        /// </summary>
        public void Show(bool animate = true, bool addToHistory = true)
        {
            if (GameBootstrap.Container != null)
            {
                GameBootstrap.Container.Resolve<IUIService>()?.ShowScreen(ScreenType, animate, addToHistory);
            }
        }

        /// <summary>
        /// Hide this screen
        /// </summary>
        public void Hide(bool animate = true)
        {
            if (GameBootstrap.Container != null)
            {
                GameBootstrap.Container.Resolve<IUIService>()?.HideScreen(ScreenType, animate);
            }
        }

        /// <summary>
        /// Toggle screen visibility
        /// </summary>
        public void Toggle(bool animate = true)
        {
            if (IsVisible)
                Hide(animate);
            else
                Show(animate);
        }

        #endregion

        #region UI Element Access

        /// <summary>
        /// Get a UI element by name from this screen
        /// </summary>
        protected T GetElement<T>(string elementName) where T : VisualElement
        {
            return Controller?.GetElement<T>(elementName);
        }

        /// <summary>
        /// Get a UI element by name and class from this screen
        /// </summary>
        protected T GetElement<T>(string elementName, string className) where T : VisualElement
        {
            return Controller?.GetElement<T>(elementName, className);
        }

        /// <summary>
        /// Get all UI elements of a specific type from this screen
        /// </summary>
        protected UQueryBuilder<T>? GetElements<T>() where T : VisualElement
        {
            return Controller?.GetElements<T>();
        }

        /// <summary>
        /// Register a callback for a UI element
        /// </summary>
        protected void RegisterCallback<T>(string elementName, EventCallback<T> callback) where T : EventBase<T>, new()
        {
            VisualElement element = GetElement<VisualElement>(elementName);
            element?.RegisterCallback(callback);
        }

        /// <summary>
        /// Unregister a callback for a UI element
        /// </summary>
        protected void UnregisterCallback<T>(string elementName, EventCallback<T> callback) where T : EventBase<T>, new()
        {
            VisualElement element = GetElement<VisualElement>(elementName);
            element?.UnregisterCallback(callback);
        }

        #endregion

        #region Controller Event Handlers

        private void OnControllerShown(UIScreenController controller)
        {
            IsVisible = true;
            OnShow();
            OnShown?.Invoke(this);
        }

        private void OnControllerHidden(UIScreenController controller)
        {
            IsVisible = false;
            OnHide();
            OnHidden?.Invoke(this);
        }

        private void OnControllerAnimationStarted(UIScreenController controller)
        {
            IsAnimating = true;
            OnAnimationStarted?.Invoke(this);
        }

        private void OnControllerAnimationCompleted(UIScreenController controller)
        {
            IsAnimating = false;
            OnAnimationCompleted?.Invoke(this);
        }

        #endregion

        #region Animation Configuration

        /// <summary>
        /// Override this to customize show animations
        /// Uses screen category for consistent animation behavior
        /// </summary>
        public virtual void AnimateShow(IUIAnimator animator, VisualElement element, float duration, Action onComplete)
        {
            switch (Category)
            {
                case UIScreenCategory.System:
                    // System screens (Splash, Maintenance) - simple fade
                    animator.FadeIn(element, duration, onComplete);
                    break;

                case UIScreenCategory.Overlay:
                    // Overlay screens (Loading, Login) - fade in
                    animator.FadeIn(element, duration, onComplete);
                    break;

                case UIScreenCategory.Modal:
                    // Modal dialogs - overlay fades, content bounces
                    animator.PopupIn(element, duration, onComplete);
                    break;

                case UIScreenCategory.Popup:
                    // Popups - overlay fades, content bounces (playful, Brawl Stars style)
                    animator.PopupIn(element, duration, onComplete);
                    break;

                case UIScreenCategory.Screen:
                    // Main screens - fade in
                    animator.FadeIn(element, duration, onComplete);
                    break;

                case UIScreenCategory.Persistent:
                    // Persistent UI (HUD) - instant or fade
                    animator.FadeIn(element, duration, onComplete);
                    break;

                default:
                    animator.FadeIn(element, duration, onComplete);
                    break;
            }
        }

        /// <summary>
        /// Override this to customize hide animations
        /// Uses screen category for consistent animation behavior
        /// </summary>
        public virtual void AnimateHide(IUIAnimator animator, VisualElement element, float duration, Action onComplete)
        {
            switch (Category)
            {
                case UIScreenCategory.System:
                    // System screens - simple fade
                    animator.FadeOut(element, duration, onComplete);
                    break;

                case UIScreenCategory.Overlay:
                    // Overlay screens - fade out
                    animator.FadeOut(element, duration, onComplete);
                    break;

                case UIScreenCategory.Modal:
                    // Modal dialogs - content bounces out, overlay fades
                    animator.PopupOut(element, duration, onComplete);
                    break;

                case UIScreenCategory.Popup:
                    // Popups - content bounces out, overlay fades (playful, Brawl Stars style)
                    animator.PopupOut(element, duration, onComplete);
                    break;

                case UIScreenCategory.Screen:
                    // Main screens - fade out
                    animator.FadeOut(element, duration, onComplete);
                    break;

                case UIScreenCategory.Persistent:
                    // Persistent UI - instant or fade
                    animator.FadeOut(element, duration, onComplete);
                    break;

                default:
                    animator.FadeOut(element, duration, onComplete);
                    break;
            }
        }

        /// <summary>
        /// Get the animation duration for this screen
        /// Uses screen category for consistent timing
        /// </summary>
        public virtual float GetAnimationDuration()
        {
            return Category switch
            {
                UIScreenCategory.System => 0.5f, // Slow (Splash, Maintenance)
                UIScreenCategory.Overlay => 0.3f, // Medium (Loading, Login)
                UIScreenCategory.Modal => 0.4f, // Bouncy (needs time for elastic effect)
                UIScreenCategory.Popup => 0.4f, // Bouncy (needs time for elastic effect)
                UIScreenCategory.Screen => 0.3f, // Medium (Main screens)
                UIScreenCategory.Persistent => 0.2f, // Fast (HUD)
                _ => 0.3f // Default
            };
        }

        /// <summary>
        /// Called before show animation starts
        /// Override this for custom pre-animation setup
        /// </summary>
        public virtual void OnBeforeShowAnimation()
        {
            // Override in derived classes
        }

        /// <summary>
        /// Called after show animation completes
        /// Override this for custom post-animation logic
        /// </summary>
        public virtual void OnAfterShowAnimation()
        {
            // Override in derived classes
        }

        /// <summary>
        /// Called before hide animation starts
        /// Override this for custom pre-animation setup
        /// </summary>
        public virtual void OnBeforeHideAnimation()
        {
            // Override in derived classes
        }

        /// <summary>
        /// Called after hide animation completes
        /// Override this for custom post-animation logic
        /// </summary>
        public virtual void OnAfterHideAnimation()
        {
            // Override in derived classes
        }

        #endregion

        #region Builder Pattern Support

        /// <summary>
        /// Configure this screen using a builder pattern
        /// </summary>
        public T Configure<T>(Action<T> configureAction) where T : BaseUIScreen
        {
            configureAction?.Invoke((T)this);
            return (T)this;
        }

        #endregion
    }
}