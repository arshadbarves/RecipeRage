using System;
using Core.Animation;
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

        public UIScreenType ScreenType { get; private set; }
        public UIScreenCategory Category { get; private set; }
        public UIScreenPriority Priority { get; private set; }
        public bool IsVisible { get; private set; }
        public bool IsAnimating { get; private set; }

        protected UIScreenController Controller { get; private set; }

        public VisualElement Container => Controller?.Container;
        public TemplateContainer TemplateContainer => Controller?.TemplateContainer;

        [Inject]
        protected IUIService UIService { get; set; }

        #endregion

        #region Events

        public event Action<BaseUIScreen> OnShown;
        public event Action<BaseUIScreen> OnHidden;
        public event Action<BaseUIScreen> OnAnimationStarted;
        public event Action<BaseUIScreen> OnAnimationCompleted;

        #endregion

        #region Lifecycle

        public void Initialize(UIScreenType screenType, UIScreenPriority priority, UIScreenController controller)
        {
            ScreenType = screenType;
            Priority = priority;
            Controller = controller;

            if (UIService != null)
            {
                Category = UIService.GetScreenCategory(screenType);
            }

            if (Controller != null)
            {
                Controller.OnShown += OnControllerShown;
                Controller.OnHidden += OnControllerHidden;
                Controller.OnAnimationStarted += OnControllerAnimationStarted;
                Controller.OnAnimationCompleted += OnControllerAnimationCompleted;
            }

            OnInitialize();
        }

        protected virtual void OnInitialize() { }
        protected virtual void OnShow() { }
        protected virtual void OnHide() { }
        
        public virtual void Update(float deltaTime) { }

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

        protected virtual void OnDispose() { }

        #endregion

        #region Public API

        public void Show(bool animate = true, bool addToHistory = true)
        {
            UIService?.ShowScreen(ScreenType, animate, addToHistory);
        }

        public void Hide(bool animate = true)
        {
            UIService?.HideScreen(ScreenType, animate);
        }

        public void Toggle(bool animate = true)
        {
            if (IsVisible) Hide(animate);
            else Show(animate);
        }

        #endregion

        #region UI Element Access

        protected T GetElement<T>(string elementName) where T : VisualElement
        {
            return Controller?.GetElement<T>(elementName);
        }

        protected T GetElement<T>(string elementName, string className) where T : VisualElement
        {
            return Controller?.GetElement<T>(elementName, className);
        }

        protected UQueryBuilder<T>? GetElements<T>() where T : VisualElement
        {
            return Controller?.GetElements<T>();
        }

        protected void RegisterCallback<T>(string elementName, EventCallback<T> callback) where T : EventBase<T>, new()
        {
            VisualElement element = GetElement<VisualElement>(elementName);
            element?.RegisterCallback(callback);
        }

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

        public virtual void AnimateShow(IUIAnimator animator, VisualElement element, float duration, Action onComplete)
        {
            switch (Category)
            {
                case UIScreenCategory.Modal:
                case UIScreenCategory.Popup:
                    animator.PopupIn(element, duration, onComplete);
                    break;
                default:
                    animator.FadeIn(element, duration, onComplete);
                    break;
            }
        }

        public virtual void AnimateHide(IUIAnimator animator, VisualElement element, float duration, Action onComplete)
        {
            switch (Category)
            {
                case UIScreenCategory.Modal:
                case UIScreenCategory.Popup:
                    animator.PopupOut(element, duration, onComplete);
                    break;
                default:
                    animator.FadeOut(element, duration, onComplete);
                    break;
            }
        }

        public virtual float GetAnimationDuration()
        {
            return Category switch
            {
                UIScreenCategory.System => 0.5f,
                UIScreenCategory.Overlay => 0.3f,
                UIScreenCategory.Modal => 0.4f,
                UIScreenCategory.Popup => 0.4f,
                UIScreenCategory.Screen => 0.3f,
                UIScreenCategory.Persistent => 0.2f,
                _ => 0.3f
            };
        }

        public virtual void OnBeforeShowAnimation() { }
        public virtual void OnAfterShowAnimation() { }
        public virtual void OnBeforeHideAnimation() { }
        public virtual void OnAfterHideAnimation() { }

        #endregion

        #region Builder Pattern Support

        public T Configure<T>(Action<T> configureAction) where T : BaseUIScreen
        {
            configureAction?.Invoke((T)this);
            return (T)this;
        }

        #endregion
    }
}