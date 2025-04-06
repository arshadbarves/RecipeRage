using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RecipeRage.UI.Animation
{
    /// <summary>
    /// A reusable animation system for UI Toolkit elements
    /// </summary>
    public class UIAnimationSystem : MonoBehaviour
    {
        private static UIAnimationSystem _instance;

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static UIAnimationSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("UIAnimationSystem");
                    _instance = go.AddComponent<UIAnimationSystem>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Animation types available
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
            Bounce,
            Pulse,
            Shake,
            RotateIn,
            RotateOut
        }

        /// <summary>
        /// Easing functions for animations
        /// </summary>
        public enum EasingType
        {
            Linear,
            EaseInQuad,
            EaseOutQuad,
            EaseInOutQuad,
            EaseInCubic,
            EaseOutCubic,
            EaseInOutCubic,
            EaseInElastic,
            EaseOutElastic,
            EaseInOutElastic
        }

        /// <summary>
        /// Animate a VisualElement with the specified animation type
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="animationType">The type of animation to apply</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="delay">Delay before starting in seconds</param>
        /// <param name="easing">Easing function to use</param>
        /// <param name="onComplete">Callback when animation completes</param>
        public void Animate(VisualElement element, AnimationType animationType, float duration = 0.3f,
            float delay = 0f, EasingType easing = EasingType.EaseOutQuad, Action onComplete = null)
        {
            if (element == null) return;

            // Store original values for reset
            Vector2 originalPosition = element.transform.position;
            Vector2 originalScale = element.transform.scale;
            float originalOpacity = element.style.opacity.value;
            // Store rotation angle
            float originalRotation = 0f;

            // Start the animation coroutine
            StartCoroutine(AnimateCoroutine(element, animationType, duration, delay, easing,
                originalPosition, originalScale, originalOpacity, originalRotation, onComplete));
        }

        private IEnumerator AnimateCoroutine(VisualElement element, AnimationType animationType,
            float duration, float delay, EasingType easing, Vector2 originalPosition,
            Vector2 originalScale, float originalOpacity, float originalRotation, Action onComplete)
        {
            // Wait for delay
            if (delay > 0)
                yield return new WaitForSeconds(delay);

            // Setup initial state based on animation type
            SetupInitialState(element, animationType, originalPosition, originalScale, originalOpacity, originalRotation);

            float startTime = Time.time;
            float endTime = startTime + duration;

            while (Time.time < endTime)
            {
                float normalizedTime = (Time.time - startTime) / duration;
                float easedTime = ApplyEasing(normalizedTime, easing);

                // Apply animation based on type
                ApplyAnimation(element, animationType, easedTime, originalPosition, originalScale, originalOpacity, originalRotation);

                yield return null;
            }

            // Ensure final state is set correctly
            ApplyAnimation(element, animationType, 1f, originalPosition, originalScale, originalOpacity, originalRotation);

            // Call completion callback
            onComplete?.Invoke();
        }

        private void SetupInitialState(VisualElement element, AnimationType animationType,
            Vector2 originalPosition, Vector2 originalScale, float originalOpacity, float originalRotation)
        {
            switch (animationType)
            {
                case AnimationType.FadeIn:
                    element.style.opacity = 0;
                    break;

                case AnimationType.FadeOut:
                    element.style.opacity = originalOpacity;
                    break;

                case AnimationType.SlideInFromLeft:
                    element.transform.position = new Vector2(-element.resolvedStyle.width, originalPosition.y);
                    break;

                case AnimationType.SlideInFromRight:
                    element.transform.position = new Vector2(element.resolvedStyle.width, originalPosition.y);
                    break;

                case AnimationType.SlideInFromTop:
                    element.transform.position = new Vector2(originalPosition.x, -element.resolvedStyle.height);
                    break;

                case AnimationType.SlideInFromBottom:
                    element.transform.position = new Vector2(originalPosition.x, element.resolvedStyle.height);
                    break;

                case AnimationType.SlideOutToLeft:
                case AnimationType.SlideOutToRight:
                case AnimationType.SlideOutToTop:
                case AnimationType.SlideOutToBottom:
                    element.transform.position = originalPosition;
                    break;

                case AnimationType.ScaleIn:
                    element.transform.scale = new Vector2(0.01f, 0.01f);
                    break;

                case AnimationType.ScaleOut:
                    element.transform.scale = originalScale;
                    break;

                case AnimationType.Bounce:
                case AnimationType.Pulse:
                    element.transform.scale = originalScale;
                    break;

                case AnimationType.Shake:
                    element.transform.position = originalPosition;
                    break;

                case AnimationType.RotateIn:
                    element.style.rotate = new Rotate(180);
                    element.style.opacity = 0;
                    break;

                case AnimationType.RotateOut:
                    element.style.rotate = new Rotate(originalRotation);
                    element.style.opacity = originalOpacity;
                    break;
            }
        }

        private void ApplyAnimation(VisualElement element, AnimationType animationType,
            float easedTime, Vector2 originalPosition, Vector2 originalScale,
            float originalOpacity, float originalRotation)
        {
            switch (animationType)
            {
                case AnimationType.FadeIn:
                    element.style.opacity = easedTime * originalOpacity;
                    break;

                case AnimationType.FadeOut:
                    element.style.opacity = (1 - easedTime) * originalOpacity;
                    break;

                case AnimationType.SlideInFromLeft:
                case AnimationType.SlideInFromRight:
                case AnimationType.SlideInFromTop:
                case AnimationType.SlideInFromBottom:
                    Vector2 targetPosition = originalPosition;
                    Vector2 startPosition = element.transform.position;
                    element.transform.position = Vector2.Lerp(startPosition, targetPosition, easedTime);
                    break;

                case AnimationType.SlideOutToLeft:
                    element.transform.position = Vector2.Lerp(originalPosition,
                        new Vector2(-element.resolvedStyle.width, originalPosition.y), easedTime);
                    break;

                case AnimationType.SlideOutToRight:
                    element.transform.position = Vector2.Lerp(originalPosition,
                        new Vector2(element.resolvedStyle.width, originalPosition.y), easedTime);
                    break;

                case AnimationType.SlideOutToTop:
                    element.transform.position = Vector2.Lerp(originalPosition,
                        new Vector2(originalPosition.x, -element.resolvedStyle.height), easedTime);
                    break;

                case AnimationType.SlideOutToBottom:
                    element.transform.position = Vector2.Lerp(originalPosition,
                        new Vector2(originalPosition.x, element.resolvedStyle.height), easedTime);
                    break;

                case AnimationType.ScaleIn:
                    element.transform.scale = Vector2.Lerp(new Vector2(0.01f, 0.01f), originalScale, easedTime);
                    break;

                case AnimationType.ScaleOut:
                    element.transform.scale = Vector2.Lerp(originalScale, new Vector2(0.01f, 0.01f), easedTime);
                    break;

                case AnimationType.Bounce:
                    // Simple bounce effect
                    float bounce = Mathf.Sin(easedTime * Mathf.PI * 2) * 0.2f;
                    element.transform.scale = new Vector2(
                        originalScale.x * (1 + bounce),
                        originalScale.y * (1 + bounce)
                    );
                    break;

                case AnimationType.Pulse:
                    // Pulse effect (scale up and down)
                    float pulse = 0.1f * (1 - Mathf.Cos(easedTime * Mathf.PI * 4));
                    element.transform.scale = new Vector2(
                        originalScale.x * (1 + pulse),
                        originalScale.y * (1 + pulse)
                    );
                    break;

                case AnimationType.Shake:
                    // Shake effect (random position offsets that diminish over time)
                    float shakeAmount = 10f * (1 - easedTime);
                    float offsetX = UnityEngine.Random.Range(-shakeAmount, shakeAmount);
                    float offsetY = UnityEngine.Random.Range(-shakeAmount, shakeAmount);
                    element.transform.position = new Vector2(
                        originalPosition.x + offsetX,
                        originalPosition.y + offsetY
                    );
                    break;

                case AnimationType.RotateIn:
                    element.style.rotate = new Rotate(180 * (1 - easedTime));
                    element.style.opacity = easedTime * originalOpacity;
                    break;

                case AnimationType.RotateOut:
                    element.style.rotate = new Rotate(180 * easedTime);
                    element.style.opacity = (1 - easedTime) * originalOpacity;
                    break;
            }
        }

        private float ApplyEasing(float t, EasingType easing)
        {
            switch (easing)
            {
                case EasingType.Linear:
                    return t;

                case EasingType.EaseInQuad:
                    return t * t;

                case EasingType.EaseOutQuad:
                    return t * (2 - t);

                case EasingType.EaseInOutQuad:
                    return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;

                case EasingType.EaseInCubic:
                    return t * t * t;

                case EasingType.EaseOutCubic:
                    return (--t) * t * t + 1;

                case EasingType.EaseInOutCubic:
                    return t < 0.5f ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;

                case EasingType.EaseInElastic:
                    return Mathf.Sin(13 * Mathf.PI / 2 * t) * Mathf.Pow(2, 10 * (t - 1));

                case EasingType.EaseOutElastic:
                    return Mathf.Sin(-13 * Mathf.PI / 2 * (t + 1)) * Mathf.Pow(2, -10 * t) + 1;

                case EasingType.EaseInOutElastic:
                    return t < 0.5f
                        ? 0.5f * Mathf.Sin(13 * Mathf.PI / 2 * (2 * t)) * Mathf.Pow(2, 10 * ((2 * t) - 1))
                        : 0.5f * (Mathf.Sin(-13 * Mathf.PI / 2 * ((2 * t - 1) + 1)) * Mathf.Pow(2, -10 * (2 * t - 1)) + 2);

                default:
                    return t;
            }
        }

        /// <summary>
        /// Animate a sequence of elements one after another
        /// </summary>
        /// <param name="elements">List of elements to animate</param>
        /// <param name="animationType">Animation type to apply to all elements</param>
        /// <param name="duration">Duration for each animation</param>
        /// <param name="stagger">Delay between each element's animation</param>
        /// <param name="easing">Easing function to use</param>
        /// <param name="onAllComplete">Callback when all animations complete</param>
        public void AnimateSequence(List<VisualElement> elements, AnimationType animationType,
            float duration = 0.3f, float stagger = 0.05f, EasingType easing = EasingType.EaseOutQuad,
            Action onAllComplete = null)
        {
            if (elements == null || elements.Count == 0) return;

            int completedCount = 0;

            for (int i = 0; i < elements.Count; i++)
            {
                float delay = i * stagger;

                Animate(elements[i], animationType, duration, delay, easing, () =>
                {
                    completedCount++;
                    if (completedCount >= elements.Count && onAllComplete != null)
                    {
                        onAllComplete();
                    }
                });
            }
        }

        /// <summary>
        /// Chain multiple animations on a single element
        /// </summary>
        /// <param name="element">Element to animate</param>
        /// <param name="animations">List of animations to apply in sequence</param>
        /// <param name="durations">List of durations for each animation</param>
        /// <param name="delays">List of delays for each animation</param>
        /// <param name="easings">List of easing functions for each animation</param>
        /// <param name="onComplete">Callback when all animations complete</param>
        public void ChainAnimations(VisualElement element, List<AnimationType> animations,
            List<float> durations = null, List<float> delays = null, List<EasingType> easings = null,
            Action onComplete = null)
        {
            if (element == null || animations == null || animations.Count == 0) return;

            // Set default values if not provided
            if (durations == null)
            {
                durations = new List<float>();
                for (int i = 0; i < animations.Count; i++)
                    durations.Add(0.3f);
            }

            if (delays == null)
            {
                delays = new List<float>();
                for (int i = 0; i < animations.Count; i++)
                    delays.Add(0f);
            }

            if (easings == null)
            {
                easings = new List<EasingType>();
                for (int i = 0; i < animations.Count; i++)
                    easings.Add(EasingType.EaseOutQuad);
            }

            // Ensure lists have the same length
            int count = Mathf.Min(animations.Count, durations.Count, delays.Count, easings.Count);

            StartCoroutine(ChainAnimationsCoroutine(element, animations, durations, delays, easings, count, onComplete));
        }

        private IEnumerator ChainAnimationsCoroutine(VisualElement element, List<AnimationType> animations,
            List<float> durations, List<float> delays, List<EasingType> easings, int count, Action onComplete)
        {
            for (int i = 0; i < count; i++)
            {
                bool isLastAnimation = (i == count - 1);
                bool animationComplete = false;

                Animate(element, animations[i], durations[i], delays[i], easings[i], () =>
                {
                    animationComplete = true;
                    if (isLastAnimation && onComplete != null)
                        onComplete();
                });

                // Wait for this animation to complete before starting the next one
                yield return new WaitUntil(() => animationComplete);
            }
        }
    }
}
