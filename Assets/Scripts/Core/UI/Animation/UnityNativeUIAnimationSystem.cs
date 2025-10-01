using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace Core.UI.Animation
{
    /// <summary>
    /// Unity's native UI Toolkit animation system using experimental.animation API.
    /// Much more performant than coroutines - uses Unity's built-in animation system.
    /// </summary>
    public static class UnityNativeUIAnimationSystem
    {
        /// <summary>
        /// Animation types for predefined animations
        /// </summary>
        public enum AnimationType
        {
            FadeIn,
            FadeOut,
            SlideInFromLeft,
            SlideInFromRight,
            SlideInFromTop,
            SlideInFromBottom,
            SlideOutToLeft,
            SlideOutToRight,
            SlideOutToTop,
            SlideOutToBottom,
            ScaleIn,
            ScaleOut,
            RotateIn,
            RotateOut,
            Bounce,
            Pulse,
            Shake
        }

        /// <summary>
        /// User-friendly easing curve options
        /// </summary>
        public enum EasingCurve
        {
            Linear,
            EaseIn,
            EaseOut,
            EaseInOut,
            EaseInBack,
            EaseOutBack,
            EaseInOutBack,
            EaseInBounce,
            EaseOutBounce,
            EaseInOutBounce,
            EaseInElastic,
            EaseOutElastic,
            EaseInOutElastic,
            EaseInCirc,
            EaseOutCirc,
            EaseInOutCirc,
            EaseInCubic,
            EaseOutCubic,
            EaseInOutCubic,
            EaseInQuad,
            EaseOutQuad,
            EaseInOutQuad,
            EaseInSine,
            EaseOutSine,
            EaseInOutSine
        }

        /// <summary>
        /// Convert EasingCurve enum to Unity's easing function
        /// </summary>
        private static Func<float, float> GetEasingFunction(EasingCurve easingCurve)
        {
            return easingCurve switch
            {
                EasingCurve.Linear => Easing.Linear,
                EasingCurve.EaseIn => Easing.InQuad,
                EasingCurve.EaseOut => Easing.OutQuad,
                EasingCurve.EaseInOut => Easing.InOutQuad,
                EasingCurve.EaseInBack => Easing.InBack,
                EasingCurve.EaseOutBack => Easing.OutBack,
                EasingCurve.EaseInOutBack => Easing.InOutBack,
                EasingCurve.EaseInBounce => Easing.InBounce,
                EasingCurve.EaseOutBounce => Easing.OutBounce,
                EasingCurve.EaseInOutBounce => Easing.InOutBounce,
                EasingCurve.EaseInElastic => Easing.InElastic,
                EasingCurve.EaseOutElastic => Easing.OutElastic,
                EasingCurve.EaseInOutElastic => Easing.InOutElastic,
                EasingCurve.EaseInCirc => Easing.InCirc,
                EasingCurve.EaseOutCirc => Easing.OutCirc,
                EasingCurve.EaseInOutCirc => Easing.InOutCirc,
                EasingCurve.EaseInCubic => Easing.InCubic,
                EasingCurve.EaseOutCubic => Easing.OutCubic,
                EasingCurve.EaseInOutCubic => Easing.InOutCubic,
                EasingCurve.EaseInQuad => Easing.InQuad,
                EasingCurve.EaseOutQuad => Easing.OutQuad,
                EasingCurve.EaseInOutQuad => Easing.InOutQuad,

                EasingCurve.EaseInSine => Easing.InSine,
                EasingCurve.EaseOutSine => Easing.OutSine,
                EasingCurve.EaseInOutSine => Easing.InOutSine,
                _ => Easing.OutQuad
            };
        }

        /// <summary>
        /// Animate opacity using Unity's native animation system
        /// </summary>
        public static void AnimateOpacity(VisualElement element, float startValue, float endValue, 
            int durationMs = 300, int delayMs = 0, EasingCurve easingCurve = EasingCurve.EaseOut, Action onComplete = null)
        {
            if (element == null) return;

            element.style.opacity = startValue;

            if (delayMs > 0)
            {
                // Handle delay by starting animation after delay
                element.schedule.Execute(() =>
                {
                    var animation = element.experimental.animation.Start(startValue, endValue, durationMs, (ve, value) =>
                    {
                        ve.style.opacity = value;
                    });
                    
                    animation.Ease(GetEasingFunction(easingCurve));
                    animation.Start();
                    
                    if (onComplete != null)
                    {
                        animation.OnCompleted(() =>
                        {
                            Debug.Log("[UnityNativeUIAnimationSystem] Opacity animation completed");
                            onComplete();
                        });
                    }
                        
                }).StartingIn(delayMs);
            }
            else
            {
                // No delay, start immediately
                var animation = element.experimental.animation.Start(startValue, endValue, durationMs, (ve, value) =>
                {
                    ve.style.opacity = value;
                });
                
                animation.Ease(GetEasingFunction(easingCurve));
                animation.Start();
                
                if (onComplete != null)
                {
                    animation.OnCompleted(() =>
                    {
                        Debug.Log("[UnityNativeUIAnimationSystem] Opacity animation completed");
                        onComplete();
                    });
                }
            }
        }

        /// <summary>
        /// Animate position using Unity's native animation system
        /// </summary>
        public static void AnimatePosition(VisualElement element, Vector2 startPos, Vector2 endPos, 
            int durationMs = 300, int delayMs = 0, EasingCurve easingCurve = EasingCurve.EaseOut, Action onComplete = null)
        {
            if (element == null) return;

            element.style.left = startPos.x;
            element.style.top = startPos.y;

            if (delayMs > 0)
            {
                element.schedule.Execute(() =>
                {
                    var animation = element.experimental.animation.Start(startPos, endPos, durationMs, (ve, pos) =>
                    {
                        ve.style.left = pos.x;
                        ve.style.top = pos.y;
                    });
                    
                    animation.Ease(GetEasingFunction(easingCurve));
                    animation.Start();
                    
                    if (onComplete != null)
                        animation.OnCompleted(onComplete);
                        
                }).StartingIn(delayMs);
            }
            else
            {
                var animation = element.experimental.animation.Start(startPos, endPos, durationMs, (ve, pos) =>
                {
                    ve.style.left = pos.x;
                    ve.style.top = pos.y;
                });
                
                animation.Ease(GetEasingFunction(easingCurve));
                animation.Start();
                
                if (onComplete != null)
                    animation.OnCompleted(onComplete);
            }
        }

        /// <summary>
        /// Animate scale using Unity's native animation system
        /// </summary>
        public static void AnimateScale(VisualElement element, Vector2 startScale, Vector2 endScale, 
            int durationMs = 300, int delayMs = 0, EasingCurve easingCurve = EasingCurve.EaseOut, Action onComplete = null)
        {
            if (element == null) return;

            element.style.scale = new StyleScale(new Vector3(startScale.x, startScale.y, 1));

            if (delayMs > 0)
            {
                element.schedule.Execute(() =>
                {
                    var animation = element.experimental.animation.Start(startScale, endScale, durationMs, (ve, scale) =>
                    {
                        ve.style.scale = new StyleScale(new Vector3(scale.x, scale.y, 1));
                    });
                    
                    animation.Ease(GetEasingFunction(easingCurve));
                    animation.Start();
                    
                    if (onComplete != null)
                        animation.OnCompleted(onComplete);
                        
                }).StartingIn(delayMs);
            }
            else
            {
                var animation = element.experimental.animation.Start(startScale, endScale, durationMs, (ve, scale) =>
                {
                    ve.style.scale = new StyleScale(new Vector3(scale.x, scale.y, 1));
                });
                
                animation.Ease(GetEasingFunction(easingCurve));
                animation.Start();
                
                if (onComplete != null)
                    animation.OnCompleted(onComplete);
            }
        }

        /// <summary>
        /// Animate rotation using Unity's native animation system
        /// </summary>
        public static void AnimateRotation(VisualElement element, float startRotation, float endRotation, 
            int durationMs = 300, int delayMs = 0, EasingCurve easingCurve = EasingCurve.EaseOut, Action onComplete = null)
        {
            if (element == null) return;

            element.style.rotate = new Rotate(startRotation);

            if (delayMs > 0)
            {
                element.schedule.Execute(() =>
                {
                    var animation = element.experimental.animation.Start(startRotation, endRotation, durationMs, (ve, rotation) =>
                    {
                        ve.style.rotate = new Rotate(rotation);
                    });
                    
                    animation.Ease(GetEasingFunction(easingCurve));
                    animation.Start();
                    
                    if (onComplete != null)
                        animation.OnCompleted(onComplete);
                        
                }).StartingIn(delayMs);
            }
            else
            {
                var animation = element.experimental.animation.Start(startRotation, endRotation, durationMs, (ve, rotation) =>
                {
                    ve.style.rotate = new Rotate(rotation);
                });
                
                animation.Ease(GetEasingFunction(easingCurve));
                animation.Start();
                
                if (onComplete != null)
                    animation.OnCompleted(onComplete);
            }
        }

        /// <summary>
        /// Predefined animation presets using Unity's native system
        /// </summary>
        public static void Animate(VisualElement element, AnimationType animationType, 
            int durationMs = 300, int delayMs = 0, Action onComplete = null)
        {
            if (element == null) return;

            switch (animationType)
            {
                case AnimationType.FadeIn:
                    AnimateOpacity(element, 0f, 1f, durationMs, delayMs, EasingCurve.EaseOut, onComplete);
                    break;

                case AnimationType.FadeOut:
                    AnimateOpacity(element, 1f, 0f, durationMs, delayMs, EasingCurve.EaseIn, onComplete);
                    break;

                case AnimationType.SlideInFromLeft:
                    var startPosLeft = new Vector2(-element.resolvedStyle.width, element.resolvedStyle.top);
                    var endPosLeft = new Vector2(element.resolvedStyle.left, element.resolvedStyle.top);
                    AnimatePosition(element, startPosLeft, endPosLeft, durationMs, delayMs, EasingCurve.EaseOut, onComplete);
                    break;

                case AnimationType.SlideInFromRight:
                    var parentWidth = element.parent?.resolvedStyle.width ?? Screen.width;
                    var startPosRight = new Vector2(parentWidth, element.resolvedStyle.top);
                    var endPosRight = new Vector2(element.resolvedStyle.left, element.resolvedStyle.top);
                    AnimatePosition(element, startPosRight, endPosRight, durationMs, delayMs, EasingCurve.EaseOut, onComplete);
                    break;

                case AnimationType.SlideInFromTop:
                    var startPosTop = new Vector2(element.resolvedStyle.left, -element.resolvedStyle.height);
                    var endPosTop = new Vector2(element.resolvedStyle.left, element.resolvedStyle.top);
                    AnimatePosition(element, startPosTop, endPosTop, durationMs, delayMs, EasingCurve.EaseOut, onComplete);
                    break;

                case AnimationType.SlideInFromBottom:
                    var parentHeight = element.parent?.resolvedStyle.height ?? Screen.height;
                    var startPosBottom = new Vector2(element.resolvedStyle.left, parentHeight);
                    var endPosBottom = new Vector2(element.resolvedStyle.left, element.resolvedStyle.top);
                    AnimatePosition(element, startPosBottom, endPosBottom, durationMs, delayMs, EasingCurve.EaseOut, onComplete);
                    break;

                case AnimationType.ScaleIn:
                    AnimateScale(element, Vector2.zero, Vector2.one, durationMs, delayMs, EasingCurve.EaseOutBack, onComplete);
                    break;

                case AnimationType.ScaleOut:
                    AnimateScale(element, Vector2.one, Vector2.zero, durationMs, delayMs, EasingCurve.EaseInBack, onComplete);
                    break;

                case AnimationType.RotateIn:
                    AnimateRotation(element, -180f, 0f, durationMs, delayMs, EasingCurve.EaseOut, onComplete);
                    break;

                case AnimationType.RotateOut:
                    AnimateRotation(element, 0f, 180f, durationMs, delayMs, EasingCurve.EaseIn, onComplete);
                    break;

                case AnimationType.Bounce:
                    AnimateBounce(element, durationMs, delayMs, onComplete);
                    break;

                case AnimationType.Pulse:
                    AnimatePulse(element, durationMs, delayMs, onComplete);
                    break;

                case AnimationType.Shake:
                    AnimateShake(element, durationMs, delayMs, onComplete);
                    break;
            }
        }

        /// <summary>
        /// Bounce animation using Unity's native chained animations
        /// </summary>
        private static void AnimateBounce(VisualElement element, int durationMs, int delayMs, Action onComplete)
        {
            var originalScale = Vector2.one;
            
            // Scale up
            AnimateScale(element, originalScale, originalScale * 1.2f, durationMs / 3, delayMs, EasingCurve.EaseOut, () =>
            {
                // Scale down
                AnimateScale(element, originalScale * 1.2f, originalScale * 0.9f, durationMs / 3, 0, EasingCurve.EaseInOut, () =>
                {
                    // Scale back to original
                    AnimateScale(element, originalScale * 0.9f, originalScale, durationMs / 3, 0, EasingCurve.EaseOutElastic, onComplete);
                });
            });
        }

        /// <summary>
        /// Pulse animation using Unity's native chained animations
        /// </summary>
        private static void AnimatePulse(VisualElement element, int durationMs, int delayMs, Action onComplete)
        {
            var originalScale = Vector2.one;
            
            // Scale up
            AnimateScale(element, originalScale, originalScale * 1.1f, durationMs / 2, delayMs, EasingCurve.EaseOut, () =>
            {
                // Scale back to original
                AnimateScale(element, originalScale * 1.1f, originalScale, durationMs / 2, 0, EasingCurve.EaseIn, onComplete);
            });
        }

        /// <summary>
        /// Shake animation using Unity's native system
        /// </summary>
        private static void AnimateShake(VisualElement element, int durationMs, int delayMs, Action onComplete)
        {
            var originalPos = new Vector2(element.resolvedStyle.left, element.resolvedStyle.top);
            var shakeIntensity = 10f;
            
            if (delayMs > 0)
            {
                element.schedule.Execute(() =>
                {
                    var animation = element.experimental.animation.Start(0f, 1f, durationMs, (ve, progress) =>
                    {
                        var shakeX = Mathf.Sin(progress * 20f) * shakeIntensity * (1f - progress);
                        var shakeY = Mathf.Cos(progress * 25f) * shakeIntensity * (1f - progress);
                        
                        ve.style.left = originalPos.x + shakeX;
                        ve.style.top = originalPos.y + shakeY;
                    });
                    
                    animation.Ease(GetEasingFunction(EasingCurve.EaseOut));
                    animation.Start();
                    
                    if (onComplete != null)
                        animation.OnCompleted(() =>
                        {
                            element.style.left = originalPos.x;
                            element.style.top = originalPos.y;
                            onComplete();
                        });
                        
                }).StartingIn(delayMs);
            }
            else
            {
                var animation = element.experimental.animation.Start(0f, 1f, durationMs, (ve, progress) =>
                {
                    var shakeX = Mathf.Sin(progress * 20f) * shakeIntensity * (1f - progress);
                    var shakeY = Mathf.Cos(progress * 25f) * shakeIntensity * (1f - progress);
                    
                    ve.style.left = originalPos.x + shakeX;
                    ve.style.top = originalPos.y + shakeY;
                });
                
                animation.Ease(GetEasingFunction(EasingCurve.EaseOut));
                animation.Start();
                
                if (onComplete != null)
                    animation.OnCompleted(() =>
                    {
                        element.style.left = originalPos.x;
                        element.style.top = originalPos.y;
                        onComplete();
                    });
            }
        }

        /// <summary>
        /// Animate a sequence of elements with staggered timing using Unity's native system
        /// </summary>
        public static void AnimateSequence(VisualElement[] elements, AnimationType animationType, 
            int durationMs = 300, int staggerMs = 50, Action onAllComplete = null)
        {
            if (elements == null || elements.Length == 0)
            {
                onAllComplete?.Invoke();
                return;
            }

            int completedCount = 0;
            
            for (int i = 0; i < elements.Length; i++)
            {
                int delay = i * staggerMs;
                Animate(elements[i], animationType, durationMs, delay, () =>
                {
                    completedCount++;
                    if (completedCount >= elements.Length)
                    {
                        onAllComplete?.Invoke();
                    }
                });
            }
        }
    }
}