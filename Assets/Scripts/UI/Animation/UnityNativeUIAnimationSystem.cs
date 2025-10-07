using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace Core.UI.Animation
{
    public static class UnityNativeUIAnimationSystem
    {
        public enum AnimationType
        {
            FadeIn, FadeOut,
            SlideInFromLeft, SlideInFromRight, SlideInFromTop, SlideInFromBottom,
            SlideOutToLeft, SlideOutToRight, SlideOutToTop, SlideOutToBottom,
            ScaleIn, ScaleOut,
            RotateIn, RotateOut,
            Bounce, Pulse, Shake
        }

        public enum EasingCurve
        {
            Linear, EaseIn, EaseOut, EaseInOut,
            EaseInBack, EaseOutBack, EaseInOutBack,
            EaseInBounce, EaseOutBounce, EaseInOutBounce,
            EaseInElastic, EaseOutElastic, EaseInOutElastic,
            EaseInCirc, EaseOutCirc, EaseInOutCirc,
            EaseInCubic, EaseOutCubic, EaseInOutCubic,
            EaseInQuad, EaseOutQuad, EaseInOutQuad,
            EaseInSine, EaseOutSine, EaseInOutSine
        }

        private static readonly Dictionary<EasingCurve, Func<float, float>> EasingFunctions = 
            new Dictionary<EasingCurve, Func<float, float>>
            {
                { EasingCurve.Linear, Easing.Linear },
                { EasingCurve.EaseIn, Easing.InQuad },
                { EasingCurve.EaseOut, Easing.OutQuad },
                { EasingCurve.EaseInOut, Easing.InOutQuad },
                { EasingCurve.EaseInBack, Easing.InBack },
                { EasingCurve.EaseOutBack, Easing.OutBack },
                { EasingCurve.EaseInOutBack, Easing.InOutBack },
                { EasingCurve.EaseInBounce, Easing.InBounce },
                { EasingCurve.EaseOutBounce, Easing.OutBounce },
                { EasingCurve.EaseInOutBounce, Easing.InOutBounce },
                { EasingCurve.EaseInElastic, Easing.InElastic },
                { EasingCurve.EaseOutElastic, Easing.OutElastic },
                { EasingCurve.EaseInOutElastic, Easing.InOutElastic },
                { EasingCurve.EaseInCirc, Easing.InCirc },
                { EasingCurve.EaseOutCirc, Easing.OutCirc },
                { EasingCurve.EaseInOutCirc, Easing.InOutCirc },
                { EasingCurve.EaseInCubic, Easing.InCubic },
                { EasingCurve.EaseOutCubic, Easing.OutCubic },
                { EasingCurve.EaseInOutCubic, Easing.InOutCubic },
                { EasingCurve.EaseInQuad, Easing.InQuad },
                { EasingCurve.EaseOutQuad, Easing.OutQuad },
                { EasingCurve.EaseInOutQuad, Easing.InOutQuad },
                { EasingCurve.EaseInSine, Easing.InSine },
                { EasingCurve.EaseOutSine, Easing.OutSine },
                { EasingCurve.EaseInOutSine, Easing.InOutSine }
            };

        private static Func<float, float> GetEasingFunction(EasingCurve easingCurve)
        {
            return EasingFunctions.TryGetValue(easingCurve, out Func<float, float> func) ? func : Easing.OutQuad;
        }

        public static void AnimateOpacity(VisualElement element, float startValue, float endValue,
            float duration = 0.3f, float delay = 0f, EasingCurve easingCurve = EasingCurve.EaseOut,
            Action onComplete = null)
        {
            if (element == null) return;

            element.style.opacity = startValue;

            void ExecuteAnimation()
            {
                var animation = ValueAnimation<float>.Create(element, Mathf.Lerp);
                animation.from = startValue;
                animation.to = endValue;
                animation.durationMs = Mathf.RoundToInt(duration * 1000);
                animation.easingCurve = GetEasingFunction(easingCurve);
                animation.valueUpdated = (e, val) => e.style.opacity = val;
                animation.onAnimationCompleted = onComplete;
                animation.autoRecycle = true;
                animation.Start();
            }

            if (delay > 0)
                element.schedule.Execute(ExecuteAnimation).ExecuteLater(Mathf.RoundToInt(delay * 1000));
            else
                ExecuteAnimation();
        }

        public static void AnimatePosition(VisualElement element, Vector2 startPos, Vector2 endPos,
            float duration = 0.3f, float delay = 0f, EasingCurve easingCurve = EasingCurve.EaseOut,
            Action onComplete = null)
        {
            if (element == null) return;

            element.style.left = startPos.x;
            element.style.top = startPos.y;

            void ExecuteAnimation()
            {
                var animation = ValueAnimation<Vector2>.Create(element, Vector2.Lerp);
                animation.from = startPos;
                animation.to = endPos;
                animation.durationMs = Mathf.RoundToInt(duration * 1000);
                animation.easingCurve = GetEasingFunction(easingCurve);
                animation.valueUpdated = (e, pos) =>
                {
                    e.style.left = pos.x;
                    e.style.top = pos.y;
                };
                animation.onAnimationCompleted = onComplete;
                animation.autoRecycle = true;
                animation.Start();
            }

            if (delay > 0)
                element.schedule.Execute(ExecuteAnimation).ExecuteLater(Mathf.RoundToInt(delay * 1000));
            else
                ExecuteAnimation();
        }

        public static void AnimateScale(VisualElement element, Vector2 startScale, Vector2 endScale,
            float duration = 0.3f, float delay = 0f, EasingCurve easingCurve = EasingCurve.EaseOut,
            Action onComplete = null)
        {
            if (element == null) return;

            element.style.scale = new StyleScale(new Vector3(startScale.x, startScale.y, 1));

            void ExecuteAnimation()
            {
                var animation = ValueAnimation<Vector2>.Create(element, Vector2.Lerp);
                animation.from = startScale;
                animation.to = endScale;
                animation.durationMs = Mathf.RoundToInt(duration * 1000);
                animation.easingCurve = GetEasingFunction(easingCurve);
                animation.valueUpdated = (e, scale) => e.style.scale = new StyleScale(new Vector3(scale.x, scale.y, 1));
                animation.onAnimationCompleted = onComplete;
                animation.autoRecycle = true;
                animation.Start();
            }

            if (delay > 0)
                element.schedule.Execute(ExecuteAnimation).ExecuteLater(Mathf.RoundToInt(delay * 1000));
            else
                ExecuteAnimation();
        }

        public static void AnimateRotation(VisualElement element, float startRotation, float endRotation,
            float duration = 0.3f, float delay = 0f, EasingCurve easingCurve = EasingCurve.EaseOut,
            Action onComplete = null)
        {
            if (element == null) return;

            element.style.rotate = new Rotate(startRotation);

            void ExecuteAnimation()
            {
                var animation = ValueAnimation<float>.Create(element, Mathf.Lerp);
                animation.from = startRotation;
                animation.to = endRotation;
                animation.durationMs = Mathf.RoundToInt(duration * 1000);
                animation.easingCurve = GetEasingFunction(easingCurve);
                animation.valueUpdated = (e, rotation) => e.style.rotate = new Rotate(rotation);
                animation.onAnimationCompleted = onComplete;
                animation.autoRecycle = true;
                animation.Start();
            }

            if (delay > 0)
                element.schedule.Execute(ExecuteAnimation).ExecuteLater(Mathf.RoundToInt(delay * 1000));
            else
                ExecuteAnimation();
        }

        public static void Animate(VisualElement element, AnimationType animationType,
            float duration = 0.3f, float delay = 0f, Action onComplete = null)
        {
            if (element == null) return;

            switch (animationType)
            {
                case AnimationType.FadeIn:
                    AnimateOpacity(element, 0f, 1f, duration, delay, EasingCurve.EaseOut, onComplete);
                    break;
                case AnimationType.FadeOut:
                    AnimateOpacity(element, 1f, 0f, duration, delay, EasingCurve.EaseIn, onComplete);
                    break;
                case AnimationType.SlideInFromLeft:
                    AnimateSlideIn(element, new Vector2(-element.resolvedStyle.width, element.resolvedStyle.top), 
                        duration, delay, onComplete);
                    break;
                case AnimationType.SlideInFromRight:
                    float parentWidth = element.parent?.resolvedStyle.width ?? Screen.width;
                    AnimateSlideIn(element, new Vector2(parentWidth, element.resolvedStyle.top), 
                        duration, delay, onComplete);
                    break;
                case AnimationType.SlideInFromTop:
                    AnimateSlideIn(element, new Vector2(element.resolvedStyle.left, -element.resolvedStyle.height), 
                        duration, delay, onComplete);
                    break;
                case AnimationType.SlideInFromBottom:
                    float parentHeight = element.parent?.resolvedStyle.height ?? Screen.height;
                    AnimateSlideIn(element, new Vector2(element.resolvedStyle.left, parentHeight), 
                        duration, delay, onComplete);
                    break;
                case AnimationType.ScaleIn:
                    AnimateScale(element, Vector2.zero, Vector2.one, duration, delay, EasingCurve.EaseOutBack, onComplete);
                    break;
                case AnimationType.ScaleOut:
                    AnimateScale(element, Vector2.one, Vector2.zero, duration, delay, EasingCurve.EaseInBack, onComplete);
                    break;
                case AnimationType.RotateIn:
                    AnimateRotation(element, -180f, 0f, duration, delay, EasingCurve.EaseOut, onComplete);
                    break;
                case AnimationType.RotateOut:
                    AnimateRotation(element, 0f, 180f, duration, delay, EasingCurve.EaseIn, onComplete);
                    break;
                case AnimationType.Bounce:
                    AnimateBounce(element, duration, delay, onComplete);
                    break;
                case AnimationType.Pulse:
                    AnimatePulse(element, duration, delay, onComplete);
                    break;
                case AnimationType.Shake:
                    AnimateShake(element, duration, delay, onComplete);
                    break;
            }
        }

        private static void AnimateSlideIn(VisualElement element, Vector2 startPos, 
            float duration, float delay, Action onComplete)
        {
            var endPos = new Vector2(element.resolvedStyle.left, element.resolvedStyle.top);
            AnimatePosition(element, startPos, endPos, duration, delay, EasingCurve.EaseOut, onComplete);
        }

        private static void AnimateBounce(VisualElement element, float duration, float delay, Action onComplete)
        {
            const float scaleUp = 1.2f;
            const float scaleDown = 0.9f;
            float thirdDuration = duration / 3f;

            AnimateScale(element, Vector2.one, Vector2.one * scaleUp, thirdDuration, delay, EasingCurve.EaseOut,
                () => AnimateScale(element, Vector2.one * scaleUp, Vector2.one * scaleDown, thirdDuration, 0,
                    EasingCurve.EaseInOut, () => AnimateScale(element, Vector2.one * scaleDown, Vector2.one, 
                        thirdDuration, 0, EasingCurve.EaseOutElastic, onComplete)));
        }

        private static void AnimatePulse(VisualElement element, float duration, float delay, Action onComplete)
        {
            const float pulseScale = 1.1f;
            float halfDuration = duration * 0.5f;

            AnimateScale(element, Vector2.one, Vector2.one * pulseScale, halfDuration, delay, EasingCurve.EaseOut,
                () => AnimateScale(element, Vector2.one * pulseScale, Vector2.one, halfDuration, 0, 
                    EasingCurve.EaseIn, onComplete));
        }

        private static void AnimateShake(VisualElement element, float duration, float delay, Action onComplete)
        {
            var originalPos = new Vector2(element.resolvedStyle.left, element.resolvedStyle.top);
            const float shakeIntensity = 10f;

            void ExecuteShake()
            {
                var animation = ValueAnimation<float>.Create(element, Mathf.Lerp);
                animation.from = 0f;
                animation.to = 1f;
                animation.durationMs = Mathf.RoundToInt(duration * 1000);
                animation.easingCurve = GetEasingFunction(EasingCurve.EaseOut);
                animation.valueUpdated = (e, progress) =>
                {
                    float intensity = shakeIntensity * (1f - progress);
                    float shakeX = Mathf.Sin(progress * 20f) * intensity;
                    float shakeY = Mathf.Cos(progress * 25f) * intensity;

                    e.style.left = originalPos.x + shakeX;
                    e.style.top = originalPos.y + shakeY;
                };
                animation.onAnimationCompleted = () =>
                {
                    element.style.left = originalPos.x;
                    element.style.top = originalPos.y;
                    onComplete?.Invoke();
                };
                animation.autoRecycle = true;
                animation.Start();
            }

            if (delay > 0)
                element.schedule.Execute(ExecuteShake).ExecuteLater(Mathf.RoundToInt(delay * 1000));
            else
                ExecuteShake();
        }

        public static void AnimateSequence(VisualElement[] elements, AnimationType animationType,
            float duration = 0.3f, float stagger = 0.05f, Action onAllComplete = null)
        {
            if (elements?.Length == 0)
            {
                onAllComplete?.Invoke();
                return;
            }

            int completedCount = 0;
            int totalElements = elements.Length;

            for (int i = 0; i < totalElements; i++)
            {
                Animate(elements[i], animationType, duration, i * stagger, () =>
                {
                    if (++completedCount >= totalElements)
                        onAllComplete?.Invoke();
                });
            }
        }
    }
}