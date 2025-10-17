using System;
using Core.Animation;
using UI.UISystem;
using UnityEngine.UIElements;

namespace UI.UISystem.Core
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
            UIServiceAccessor.Instance?.ShowScreen(ScreenType, animate, addToHistory);
        }

        /// <summary>
        /// Hide this screen
        /// </summary>
        public void Hide(bool animate = true)
        {
            UIServiceAccessor.Instance?.HideScreen(ScreenType, animate);
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
        /// Calls the appropriate animation method on the animator directly
        /// </summary>
        public virtual void AnimateShow(IUIAnimator animator, VisualElement element, float duration, Action onComplete)
        {
            switch (ScreenType)
            {
                case UIScreenType.Splash:
                case UIScreenType.Loading:
                    animator.FadeIn(element, duration, onComplete);
                    break;

                case UIScreenType.Popup:
                case UIScreenType.Modal:
                    animator.ScaleIn(element, duration, onComplete);
                    break;

                case UIScreenType.Notification:
                    animator.SlideIn(element, SlideDirection.Top, duration, onComplete);
                    break;

                case UIScreenType.Settings:
                case UIScreenType.Pause:
                    animator.SlideIn(element, SlideDirection.Right, duration, onComplete);
                    break;

                default:
                    animator.FadeIn(element, duration, onComplete);
                    break;
            }
        }

        /// <summary>
        /// Override this to customize hide animations
        /// Calls the appropriate animation method on the animator directly
        /// </summary>
        public virtual void AnimateHide(IUIAnimator animator, VisualElement element, float duration, Action onComplete)
        {
            switch (ScreenType)
            {
                case UIScreenType.Splash:
                case UIScreenType.Loading:
                    animator.FadeOut(element, duration, onComplete);
                    break;

                case UIScreenType.Popup:
                case UIScreenType.Modal:
                    animator.ScaleOut(element, duration, onComplete);
                    break;

                case UIScreenType.Notification:
                    animator.SlideOut(element, SlideDirection.Top, duration, onComplete);
                    break;

                case UIScreenType.Settings:
                case UIScreenType.Pause:
                    animator.SlideOut(element, SlideDirection.Right, duration, onComplete);
                    break;

                default:
                    animator.FadeOut(element, duration, onComplete);
                    break;
            }
        }

        /// <summary>
        /// Get the animation duration for this screen
        /// Override this to customize animation timing
        /// </summary>
        public virtual float GetAnimationDuration()
        {
            return ScreenType switch
            {
                UIScreenType.Splash or UIScreenType.Loading => 0.5f, // Slow
                UIScreenType.Notification => 0.15f, // Fast
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