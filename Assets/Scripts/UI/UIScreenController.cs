using System;
using Core.Animation;
using UnityEngine;
using UnityEngine.UIElements;
using Core.Logging;

namespace UI
{
    /// <summary>
    /// Controls individual UI screens within the unified UI system
    /// </summary>
    public class UIScreenController
    {
        public UIScreenType ScreenType { get; private set; }
        public UIScreenPriority Priority { get; private set; }
        public VisualElement Container { get; private set; }
        public TemplateContainer TemplateContainer { get; private set; }
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
                TemplateContainer = _template.Instantiate();
                
                // Ensure template instance fills the container
                TemplateContainer.style.width = Length.Percent(100);
                TemplateContainer.style.height = Length.Percent(100);
                TemplateContainer.style.position = Position.Absolute;
                TemplateContainer.style.left = 0;
                TemplateContainer.style.top = 0;
                TemplateContainer.style.right = 0;
                TemplateContainer.style.bottom = 0;
                
                Container.Add(TemplateContainer);
                
                // Find the main screen container within the template
                VisualElement screenContainer = TemplateContainer.Q<VisualElement>("screen-container");
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
                GameLogger.LogError($"Failed to load template for {ScreenType}: {e.Message}");
            }
        }

        /// <summary>
        /// Show the screen with animation
        /// </summary>
        public void Show(IUIAnimator animator, Action<IUIAnimator, VisualElement, float, Action> animateCallback, float duration, bool animate, Action onComplete = null)
        {
            if (IsVisible || IsAnimating) return;

            IsAnimating = true;
            OnAnimationStarted?.Invoke(this);

            // Make container visible
            Container.style.display = DisplayStyle.Flex;

            if (animate)
            {
                // Call the animation callback which will invoke the specific animation method
                animateCallback?.Invoke(animator, Container, duration, () =>
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
        public void Hide(IUIAnimator animator, Action<IUIAnimator, VisualElement, float, Action> animateCallback, float duration, bool animate, Action onComplete = null)
        {
            if (!IsVisible || IsAnimating) return;

            IsAnimating = true;
            OnAnimationStarted?.Invoke(this);

            if (animate)
            {
                animateCallback?.Invoke(animator, Container, duration, () =>
                {
                    CompleteHide(onComplete);
                });
            }
            else
            {
                CompleteHide(onComplete);
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