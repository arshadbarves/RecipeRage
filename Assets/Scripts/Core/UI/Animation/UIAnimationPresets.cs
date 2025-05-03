using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.UI.Animation
{
    /// <summary>
    /// Collection of predefined animation presets for UI elements.
    /// </summary>
    public static class UIAnimationPresets
    {
        /// <summary>
        /// Fade in animation.
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="delay">Delay before starting in seconds</param>
        /// <param name="onComplete">Callback when animation completes</param>
        /// <returns>Animation ID</returns>
        public static int FadeIn(VisualElement element, float duration = 0.5f, float delay = 0f, Action onComplete = null)
        {
            return UIAnimationSystem.Instance.Fade(element, 0f, 1f, duration, delay, UIEasing.EaseOutCubic, onComplete);
        }
        
        /// <summary>
        /// Fade out animation.
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="delay">Delay before starting in seconds</param>
        /// <param name="onComplete">Callback when animation completes</param>
        /// <returns>Animation ID</returns>
        public static int FadeOut(VisualElement element, float duration = 0.5f, float delay = 0f, Action onComplete = null)
        {
            return UIAnimationSystem.Instance.Fade(element, 1f, 0f, duration, delay, UIEasing.EaseInCubic, onComplete);
        }
        
        /// <summary>
        /// Slide in from left animation.
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="distance">Distance to slide in pixels</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="delay">Delay before starting in seconds</param>
        /// <param name="onComplete">Callback when animation completes</param>
        /// <returns>Animation ID</returns>
        public static int SlideInFromLeft(VisualElement element, float distance = 100f, float duration = 0.5f, float delay = 0f, Action onComplete = null)
        {
            float currentLeft = element.style.left.value.value;
            return UIAnimationSystem.Instance.Move(element, 
                new Vector2(currentLeft - distance, element.style.top.value.value), 
                new Vector2(currentLeft, element.style.top.value.value), 
                duration, delay, UIEasing.EaseOutCubic, onComplete);
        }
        
        /// <summary>
        /// Slide in from right animation.
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="distance">Distance to slide in pixels</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="delay">Delay before starting in seconds</param>
        /// <param name="onComplete">Callback when animation completes</param>
        /// <returns>Animation ID</returns>
        public static int SlideInFromRight(VisualElement element, float distance = 100f, float duration = 0.5f, float delay = 0f, Action onComplete = null)
        {
            float currentLeft = element.style.left.value.value;
            return UIAnimationSystem.Instance.Move(element, 
                new Vector2(currentLeft + distance, element.style.top.value.value), 
                new Vector2(currentLeft, element.style.top.value.value), 
                duration, delay, UIEasing.EaseOutCubic, onComplete);
        }
        
        /// <summary>
        /// Slide in from top animation.
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="distance">Distance to slide in pixels</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="delay">Delay before starting in seconds</param>
        /// <param name="onComplete">Callback when animation completes</param>
        /// <returns>Animation ID</returns>
        public static int SlideInFromTop(VisualElement element, float distance = 100f, float duration = 0.5f, float delay = 0f, Action onComplete = null)
        {
            float currentTop = element.style.top.value.value;
            return UIAnimationSystem.Instance.Move(element, 
                new Vector2(element.style.left.value.value, currentTop - distance), 
                new Vector2(element.style.left.value.value, currentTop), 
                duration, delay, UIEasing.EaseOutCubic, onComplete);
        }
        
        /// <summary>
        /// Slide in from bottom animation.
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="distance">Distance to slide in pixels</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="delay">Delay before starting in seconds</param>
        /// <param name="onComplete">Callback when animation completes</param>
        /// <returns>Animation ID</returns>
        public static int SlideInFromBottom(VisualElement element, float distance = 100f, float duration = 0.5f, float delay = 0f, Action onComplete = null)
        {
            float currentTop = element.style.top.value.value;
            return UIAnimationSystem.Instance.Move(element, 
                new Vector2(element.style.left.value.value, currentTop + distance), 
                new Vector2(element.style.left.value.value, currentTop), 
                duration, delay, UIEasing.EaseOutCubic, onComplete);
        }
        
        /// <summary>
        /// Slide out to left animation.
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="distance">Distance to slide in pixels</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="delay">Delay before starting in seconds</param>
        /// <param name="onComplete">Callback when animation completes</param>
        /// <returns>Animation ID</returns>
        public static int SlideOutToLeft(VisualElement element, float distance = 100f, float duration = 0.5f, float delay = 0f, Action onComplete = null)
        {
            float currentLeft = element.style.left.value.value;
            return UIAnimationSystem.Instance.Move(element, 
                new Vector2(currentLeft, element.style.top.value.value), 
                new Vector2(currentLeft - distance, element.style.top.value.value), 
                duration, delay, UIEasing.EaseInCubic, onComplete);
        }
        
        /// <summary>
        /// Slide out to right animation.
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="distance">Distance to slide in pixels</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="delay">Delay before starting in seconds</param>
        /// <param name="onComplete">Callback when animation completes</param>
        /// <returns>Animation ID</returns>
        public static int SlideOutToRight(VisualElement element, float distance = 100f, float duration = 0.5f, float delay = 0f, Action onComplete = null)
        {
            float currentLeft = element.style.left.value.value;
            return UIAnimationSystem.Instance.Move(element, 
                new Vector2(currentLeft, element.style.top.value.value), 
                new Vector2(currentLeft + distance, element.style.top.value.value), 
                duration, delay, UIEasing.EaseInCubic, onComplete);
        }
        
        /// <summary>
        /// Slide out to top animation.
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="distance">Distance to slide in pixels</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="delay">Delay before starting in seconds</param>
        /// <param name="onComplete">Callback when animation completes</param>
        /// <returns>Animation ID</returns>
        public static int SlideOutToTop(VisualElement element, float distance = 100f, float duration = 0.5f, float delay = 0f, Action onComplete = null)
        {
            float currentTop = element.style.top.value.value;
            return UIAnimationSystem.Instance.Move(element, 
                new Vector2(element.style.left.value.value, currentTop), 
                new Vector2(element.style.left.value.value, currentTop - distance), 
                duration, delay, UIEasing.EaseInCubic, onComplete);
        }
        
        /// <summary>
        /// Slide out to bottom animation.
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="distance">Distance to slide in pixels</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="delay">Delay before starting in seconds</param>
        /// <param name="onComplete">Callback when animation completes</param>
        /// <returns>Animation ID</returns>
        public static int SlideOutToBottom(VisualElement element, float distance = 100f, float duration = 0.5f, float delay = 0f, Action onComplete = null)
        {
            float currentTop = element.style.top.value.value;
            return UIAnimationSystem.Instance.Move(element, 
                new Vector2(element.style.left.value.value, currentTop), 
                new Vector2(element.style.left.value.value, currentTop + distance), 
                duration, delay, UIEasing.EaseInCubic, onComplete);
        }
        
        /// <summary>
        /// Scale in animation.
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="delay">Delay before starting in seconds</param>
        /// <param name="onComplete">Callback when animation completes</param>
        /// <returns>Animation ID</returns>
        public static int ScaleIn(VisualElement element, float duration = 0.5f, float delay = 0f, Action onComplete = null)
        {
            return UIAnimationSystem.Instance.Scale(element, 
                new Vector2(0.5f, 0.5f), 
                new Vector2(1f, 1f), 
                duration, delay, UIEasing.EaseOutBack, onComplete);
        }
        
        /// <summary>
        /// Scale out animation.
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="delay">Delay before starting in seconds</param>
        /// <param name="onComplete">Callback when animation completes</param>
        /// <returns>Animation ID</returns>
        public static int ScaleOut(VisualElement element, float duration = 0.5f, float delay = 0f, Action onComplete = null)
        {
            return UIAnimationSystem.Instance.Scale(element, 
                new Vector2(1f, 1f), 
                new Vector2(0.5f, 0.5f), 
                duration, delay, UIEasing.EaseInBack, onComplete);
        }
        
        /// <summary>
        /// Bounce in animation.
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="delay">Delay before starting in seconds</param>
        /// <param name="onComplete">Callback when animation completes</param>
        /// <returns>Animation ID</returns>
        public static int BounceIn(VisualElement element, float duration = 0.8f, float delay = 0f, Action onComplete = null)
        {
            return UIAnimationSystem.Instance.Scale(element, 
                new Vector2(0.3f, 0.3f), 
                new Vector2(1f, 1f), 
                duration, delay, UIEasing.EaseOutBounce, onComplete);
        }
        
        /// <summary>
        /// Bounce out animation.
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="delay">Delay before starting in seconds</param>
        /// <param name="onComplete">Callback when animation completes</param>
        /// <returns>Animation ID</returns>
        public static int BounceOut(VisualElement element, float duration = 0.8f, float delay = 0f, Action onComplete = null)
        {
            return UIAnimationSystem.Instance.Scale(element, 
                new Vector2(1f, 1f), 
                new Vector2(0.3f, 0.3f), 
                duration, delay, UIEasing.EaseInBounce, onComplete);
        }
        
        /// <summary>
        /// Rotate in animation.
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="startAngle">Starting angle in degrees</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="delay">Delay before starting in seconds</param>
        /// <param name="onComplete">Callback when animation completes</param>
        /// <returns>Animation ID</returns>
        public static int RotateIn(VisualElement element, float startAngle = -180f, float duration = 0.7f, float delay = 0f, Action onComplete = null)
        {
            return UIAnimationSystem.Instance.Rotate(element, 
                startAngle, 0f, 
                duration, delay, UIEasing.EaseOutQuart, onComplete);
        }
        
        /// <summary>
        /// Rotate out animation.
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="endAngle">Ending angle in degrees</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="delay">Delay before starting in seconds</param>
        /// <param name="onComplete">Callback when animation completes</param>
        /// <returns>Animation ID</returns>
        public static int RotateOut(VisualElement element, float endAngle = 180f, float duration = 0.7f, float delay = 0f, Action onComplete = null)
        {
            return UIAnimationSystem.Instance.Rotate(element, 
                0f, endAngle, 
                duration, delay, UIEasing.EaseInQuart, onComplete);
        }
        
        /// <summary>
        /// Pulse animation.
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="intensity">Pulse intensity (scale factor)</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="delay">Delay before starting in seconds</param>
        /// <param name="onComplete">Callback when animation completes</param>
        /// <returns>Animation ID</returns>
        public static int Pulse(VisualElement element, float intensity = 1.1f, float duration = 0.5f, float delay = 0f, Action onComplete = null)
        {
            var sequence = new UIAnimationSequence()
                .Scale(element, new Vector2(1f, 1f), new Vector2(intensity, intensity), duration / 2, delay, UIEasing.EaseOutQuad)
                .Scale(element, new Vector2(intensity, intensity), new Vector2(1f, 1f), duration / 2, 0f, UIEasing.EaseInQuad)
                .OnComplete(onComplete);
            
            sequence.Play();
            return 0; // Sequence handles its own lifecycle
        }
        
        /// <summary>
        /// Shake animation.
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="intensity">Shake intensity in pixels</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="delay">Delay before starting in seconds</param>
        /// <param name="onComplete">Callback when animation completes</param>
        /// <returns>Animation ID</returns>
        public static int Shake(VisualElement element, float intensity = 10f, float duration = 0.5f, float delay = 0f, Action onComplete = null)
        {
            float currentLeft = element.style.left.value.value;
            
            var sequence = new UIAnimationSequence();
            
            if (delay > 0)
            {
                sequence.Delay(delay);
            }
            
            // Store original position
            Vector2 originalPosition = new Vector2(currentLeft, element.style.top.value.value);
            
            // Add shake steps
            float stepDuration = duration / 6;
            sequence
                .Move(element, originalPosition, new Vector2(originalPosition.x + intensity, originalPosition.y), stepDuration, 0f, UIEasing.EaseInOutQuad)
                .Move(element, new Vector2(originalPosition.x + intensity, originalPosition.y), new Vector2(originalPosition.x - intensity, originalPosition.y), stepDuration, 0f, UIEasing.EaseInOutQuad)
                .Move(element, new Vector2(originalPosition.x - intensity, originalPosition.y), new Vector2(originalPosition.x + intensity * 0.5f, originalPosition.y), stepDuration, 0f, UIEasing.EaseInOutQuad)
                .Move(element, new Vector2(originalPosition.x + intensity * 0.5f, originalPosition.y), new Vector2(originalPosition.x - intensity * 0.5f, originalPosition.y), stepDuration, 0f, UIEasing.EaseInOutQuad)
                .Move(element, new Vector2(originalPosition.x - intensity * 0.5f, originalPosition.y), new Vector2(originalPosition.x + intensity * 0.25f, originalPosition.y), stepDuration, 0f, UIEasing.EaseInOutQuad)
                .Move(element, new Vector2(originalPosition.x + intensity * 0.25f, originalPosition.y), originalPosition, stepDuration, 0f, UIEasing.EaseInOutQuad)
                .OnComplete(onComplete);
            
            sequence.Play();
            return 0; // Sequence handles its own lifecycle
        }
        
        /// <summary>
        /// Fade and slide in animation.
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="fromDirection">Direction to slide from</param>
        /// <param name="distance">Distance to slide in pixels</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="delay">Delay before starting in seconds</param>
        /// <param name="onComplete">Callback when animation completes</param>
        /// <returns>Animation ID</returns>
        public static int FadeAndSlideIn(VisualElement element, SlideDirection fromDirection, float distance = 100f, 
            float duration = 0.5f, float delay = 0f, Action onComplete = null)
        {
            float currentLeft = element.style.left.value.value;
            float currentTop = element.style.top.value.value;
            
            Vector2 startPosition = new Vector2(currentLeft, currentTop);
            
            switch (fromDirection)
            {
                case SlideDirection.Left:
                    startPosition.x -= distance;
                    break;
                case SlideDirection.Right:
                    startPosition.x += distance;
                    break;
                case SlideDirection.Top:
                    startPosition.y -= distance;
                    break;
                case SlideDirection.Bottom:
                    startPosition.y += distance;
                    break;
            }
            
            var sequence = new UIAnimationSequence()
                .Fade(element, 0f, 1f, duration, delay, UIEasing.EaseOutCubic)
                .Move(element, startPosition, new Vector2(currentLeft, currentTop), duration, delay, UIEasing.EaseOutCubic)
                .OnComplete(onComplete);
            
            sequence.Play();
            return 0; // Sequence handles its own lifecycle
        }
        
        /// <summary>
        /// Fade and slide out animation.
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="toDirection">Direction to slide to</param>
        /// <param name="distance">Distance to slide in pixels</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="delay">Delay before starting in seconds</param>
        /// <param name="onComplete">Callback when animation completes</param>
        /// <returns>Animation ID</returns>
        public static int FadeAndSlideOut(VisualElement element, SlideDirection toDirection, float distance = 100f, 
            float duration = 0.5f, float delay = 0f, Action onComplete = null)
        {
            float currentLeft = element.style.left.value.value;
            float currentTop = element.style.top.value.value;
            
            Vector2 endPosition = new Vector2(currentLeft, currentTop);
            
            switch (toDirection)
            {
                case SlideDirection.Left:
                    endPosition.x -= distance;
                    break;
                case SlideDirection.Right:
                    endPosition.x += distance;
                    break;
                case SlideDirection.Top:
                    endPosition.y -= distance;
                    break;
                case SlideDirection.Bottom:
                    endPosition.y += distance;
                    break;
            }
            
            var sequence = new UIAnimationSequence()
                .Fade(element, 1f, 0f, duration, delay, UIEasing.EaseInCubic)
                .Move(element, new Vector2(currentLeft, currentTop), endPosition, duration, delay, UIEasing.EaseInCubic)
                .OnComplete(onComplete);
            
            sequence.Play();
            return 0; // Sequence handles its own lifecycle
        }
    }
    
    /// <summary>
    /// Direction for slide animations.
    /// </summary>
    public enum SlideDirection
    {
        Left,
        Right,
        Top,
        Bottom
    }
}
