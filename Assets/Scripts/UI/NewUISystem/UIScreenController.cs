using System;
using Core.UI.Animation;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.UISystem
{
    /// <summary>
    /// Controls individual UI screens within the unified UI system
    /// </summary>
    public class UIScreenController
    {
        public UIScreenType ScreenType { get; private set; }
        public UIScreenPriority Priority { get; private set; }
        public VisualElement Container { get; private set; }
        public bool IsVisible { get; private set; }
        public bool IsAnimating { get; private set; }

        private readonly VisualTreeAsset _template;
        private readonly VisualElement _root;

        // Events
        public event Action<UIScreenController> OnShown;
        public event Action<UIScreenController> OnHidden;
        public event Action<UIScreenController> OnAnimationStarted;
        public event Action<UIScreenController> OnAnimationCompleted;

        public UIScreenController(UIScreenType screenType, UIScreenPriority priority, VisualTreeAsset template, VisualElement root)
        {
            ScreenType = screenType;
            Priority = priority;
            _template = template;
            _root = root;

            CreateContainer();
        }

        private void CreateContainer()
        {
            // Create main container for this screen
            Container = new VisualElement();
            Container.name = $"screen-{ScreenType.ToString().ToLower()}";
            Container.AddToClassList("ui-screen");
            Container.AddToClassList($"ui-screen--{ScreenType.ToString().ToLower()}");
            
            // Set initial state - FULL SCREEN COVERAGE
            Container.style.display = DisplayStyle.None;
            Container.style.position = Position.Absolute;
            Container.style.left = 0;
            Container.style.top = 0;
            Container.style.right = 0;
            Container.style.bottom = 0;
            Container.style.width = Length.Percent(100);
            Container.style.height = Length.Percent(100);

            // Add to root
            _root.Add(Container);

            // Load template if provided
            if (_template != null)
            {
                LoadTemplate();
            }
        }

        private void LoadTemplate()
        {
            try
            {
                TemplateContainer templateInstance = _template.Instantiate();
                
                // Ensure template instance fills the container
                templateInstance.style.width = Length.Percent(100);
                templateInstance.style.height = Length.Percent(100);
                templateInstance.style.position = Position.Absolute;
                templateInstance.style.left = 0;
                templateInstance.style.top = 0;
                templateInstance.style.right = 0;
                templateInstance.style.bottom = 0;
                
                Container.Add(templateInstance);
                
                // Find the main screen container within the template
                VisualElement screenContainer = templateInstance.Q<VisualElement>("screen-container");
                if (screenContainer != null)
                {
                    // Apply screen-specific styling and ensure full coverage
                    screenContainer.AddToClassList($"screen-{ScreenType.ToString().ToLower()}");
                    screenContainer.style.width = Length.Percent(100);
                    screenContainer.style.height = Length.Percent(100);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[UIScreenController] Failed to load template for {ScreenType}: {e.Message}");
            }
        }

        /// <summary>
        /// Show the screen with animation
        /// </summary>
        public void Show(UnityNativeUIAnimationSystem.AnimationType animationType, float duration, bool animate, Action onComplete = null)
        {
            if (IsVisible || IsAnimating) return;

            IsAnimating = true;
            OnAnimationStarted?.Invoke(this);

            // Make container visible
            Container.style.display = DisplayStyle.Flex;

            if (animate)
            {
                // Prepare for animation based on type
                PrepareForShowAnimation(animationType);

                // Animate
                UnityNativeUIAnimationSystem.Animate(Container, animationType, duration, 0f, () =>
                {
                    CompleteShow(onComplete);
                });
            }
            else
            {
                // Show immediately
                Container.style.opacity = 1f;
                Container.style.scale = new StyleScale(Vector2.one);
                CompleteShow(onComplete);
            }
        }

        /// <summary>
        /// Hide the screen with animation
        /// </summary>
        public void Hide(UnityNativeUIAnimationSystem.AnimationType animationType, float duration, bool animate, Action onComplete = null)
        {
            if (!IsVisible || IsAnimating) return;

            IsAnimating = true;
            OnAnimationStarted?.Invoke(this);

            if (animate)
            {
                UnityNativeUIAnimationSystem.Animate(Container, animationType, duration, 0f, () =>
                {
                    CompleteHide(onComplete);
                });
            }
            else
            {
                CompleteHide(onComplete);
            }
        }

        private void PrepareForShowAnimation(UnityNativeUIAnimationSystem.AnimationType animationType)
        {
            switch (animationType)
            {
                case UnityNativeUIAnimationSystem.AnimationType.FadeIn:
                    Container.style.opacity = 0f;
                    break;
                
                case UnityNativeUIAnimationSystem.AnimationType.ScaleIn:
                    Container.style.opacity = 0f;
                    Container.style.scale = new StyleScale(Vector2.zero);
                    break;
                
                case UnityNativeUIAnimationSystem.AnimationType.SlideInFromTop:
                    Container.style.opacity = 1f;
                    Container.style.translate = new StyleTranslate(new Translate(0, -Container.resolvedStyle.height));
                    break;
                
                case UnityNativeUIAnimationSystem.AnimationType.SlideInFromRight:
                    Container.style.opacity = 1f;
                    Container.style.translate = new StyleTranslate(new Translate(Container.resolvedStyle.width, 0));
                    break;
                
                case UnityNativeUIAnimationSystem.AnimationType.SlideInFromBottom:
                    Container.style.opacity = 1f;
                    Container.style.translate = new StyleTranslate(new Translate(0, Container.resolvedStyle.height));
                    break;
                
                case UnityNativeUIAnimationSystem.AnimationType.SlideInFromLeft:
                    Container.style.opacity = 1f;
                    Container.style.translate = new StyleTranslate(new Translate(-Container.resolvedStyle.width, 0));
                    break;
            }
        }

        private void CompleteShow(Action onComplete)
        {
            IsVisible = true;
            IsAnimating = false;
            
            // Ensure final state
            Container.style.opacity = 1f;
            Container.style.scale = new StyleScale(Vector2.one);
            Container.style.translate = new StyleTranslate(Translate.None());
            
            OnShown?.Invoke(this);
            OnAnimationCompleted?.Invoke(this);
            onComplete?.Invoke();
        }

        private void CompleteHide(Action onComplete)
        {
            IsVisible = false;
            IsAnimating = false;
            Container.style.display = DisplayStyle.None;
            
            // Reset transform properties
            Container.style.opacity = 1f;
            Container.style.scale = new StyleScale(Vector2.one);
            Container.style.translate = new StyleTranslate(Translate.None());
            
            OnHidden?.Invoke(this);
            OnAnimationCompleted?.Invoke(this);
            onComplete?.Invoke();
        }

        /// <summary>
        /// Get a UI element by name from this screen
        /// </summary>
        public T GetElement<T>(string elementName) where T : VisualElement
        {
            return Container.Q<T>(elementName);
        }

        /// <summary>
        /// Get a UI element by name and class from this screen
        /// </summary>
        public T GetElement<T>(string elementName, string className) where T : VisualElement
        {
            return Container.Q<T>(elementName, className);
        }

        /// <summary>
        /// Get all UI elements of a specific type from this screen
        /// </summary>
        public UQueryBuilder<T> GetElements<T>() where T : VisualElement
        {
            return Container.Query<T>();
        }

        /// <summary>
        /// Update the screen's priority (will trigger re-sorting)
        /// </summary>
        public void UpdatePriority(UIScreenPriority newPriority)
        {
            Priority = newPriority;
            // The manager should handle re-sorting when this is called
        }

        /// <summary>
        /// Set custom content for screens without templates
        /// </summary>
        public void SetCustomContent(VisualElement content)
        {
            Container.Clear();
            Container.Add(content);
        }

        /// <summary>
        /// Add a child element to this screen
        /// </summary>
        public void AddElement(VisualElement element)
        {
            Container.Add(element);
        }

        /// <summary>
        /// Remove a child element from this screen
        /// </summary>
        public void RemoveElement(VisualElement element)
        {
            Container.Remove(element);
        }

        /// <summary>
        /// Clear all content from this screen
        /// </summary>
        public void ClearContent()
        {
            Container.Clear();
        }
    }
}