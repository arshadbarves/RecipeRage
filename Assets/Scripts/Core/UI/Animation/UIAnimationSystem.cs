using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.UI.Animation
{
    /// <summary>
    /// Advanced animation system for UI Toolkit elements.
    /// </summary>
    public class UIAnimationSystem : MonoBehaviour
    {
        private static UIAnimationSystem _instance;

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
            Shake,
            Custom
        }

        /// <summary>
        /// Easing types for predefined easing functions
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
            EaseInSine,
            EaseOutSine,
            EaseInOutSine,
            EaseInExpo,
            EaseOutExpo,
            EaseInOutExpo,
            EaseInElastic,
            EaseOutElastic,
            EaseInOutElastic,
            EaseInBack,
            EaseOutBack,
            EaseInOutBack,
            EaseInBounce,
            EaseOutBounce,
            EaseInOutBounce
        }

        /// <summary>
        /// Singleton instance of the animation system.
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

        private Dictionary<int, Coroutine> _activeAnimations = new Dictionary<int, Coroutine>();
        private int _nextAnimationId = 0;

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
        /// Play a fade animation on a UI element.
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="startOpacity">Starting opacity (0-1)</param>
        /// <param name="endOpacity">Ending opacity (0-1)</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="delay">Delay before starting in seconds</param>
        /// <param name="easing">Easing function to use</param>
        /// <param name="onComplete">Callback when animation completes</param>
        /// <returns>Animation ID that can be used to stop the animation</returns>
        public int Fade(VisualElement element, float startOpacity, float endOpacity, float duration,
            float delay = 0f, EasingFunction easing = null, Action onComplete = null)
        {
            if (element == null)
            {
                Debug.LogWarning("[UIAnimationSystem] Cannot animate null element");
                return -1;
            }

            int animationId = _nextAnimationId++;
            _activeAnimations[animationId] = StartCoroutine(FadeCoroutine(
                animationId, element, startOpacity, endOpacity, duration, delay, easing, onComplete));

            return animationId;
        }

        /// <summary>
        /// Play a move animation on a UI element.
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="startPosition">Starting position</param>
        /// <param name="endPosition">Ending position</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="delay">Delay before starting in seconds</param>
        /// <param name="easing">Easing function to use</param>
        /// <param name="onComplete">Callback when animation completes</param>
        /// <returns>Animation ID that can be used to stop the animation</returns>
        public int Move(VisualElement element, Vector2 startPosition, Vector2 endPosition, float duration,
            float delay = 0f, EasingFunction easing = null, Action onComplete = null)
        {
            if (element == null)
            {
                Debug.LogWarning("[UIAnimationSystem] Cannot animate null element");
                return -1;
            }

            int animationId = _nextAnimationId++;
            _activeAnimations[animationId] = StartCoroutine(MoveCoroutine(
                animationId, element, startPosition, endPosition, duration, delay, easing, onComplete));

            return animationId;
        }

        /// <summary>
        /// Play a scale animation on a UI element.
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="startScale">Starting scale</param>
        /// <param name="endScale">Ending scale</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="delay">Delay before starting in seconds</param>
        /// <param name="easing">Easing function to use</param>
        /// <param name="onComplete">Callback when animation completes</param>
        /// <returns>Animation ID that can be used to stop the animation</returns>
        public int Scale(VisualElement element, Vector2 startScale, Vector2 endScale, float duration,
            float delay = 0f, EasingFunction easing = null, Action onComplete = null)
        {
            if (element == null)
            {
                Debug.LogWarning("[UIAnimationSystem] Cannot animate null element");
                return -1;
            }

            int animationId = _nextAnimationId++;
            _activeAnimations[animationId] = StartCoroutine(ScaleCoroutine(
                animationId, element, startScale, endScale, duration, delay, easing, onComplete));

            return animationId;
        }

        /// <summary>
        /// Play a rotation animation on a UI element.
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="startRotation">Starting rotation in degrees</param>
        /// <param name="endRotation">Ending rotation in degrees</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="delay">Delay before starting in seconds</param>
        /// <param name="easing">Easing function to use</param>
        /// <param name="onComplete">Callback when animation completes</param>
        /// <returns>Animation ID that can be used to stop the animation</returns>
        public int Rotate(VisualElement element, float startRotation, float endRotation, float duration,
            float delay = 0f, EasingFunction easing = null, Action onComplete = null)
        {
            if (element == null)
            {
                Debug.LogWarning("[UIAnimationSystem] Cannot animate null element");
                return -1;
            }

            int animationId = _nextAnimationId++;
            _activeAnimations[animationId] = StartCoroutine(RotateCoroutine(
                animationId, element, startRotation, endRotation, duration, delay, easing, onComplete));

            return animationId;
        }

        /// <summary>
        /// Stop an active animation.
        /// </summary>
        /// <param name="animationId">The ID of the animation to stop</param>
        public void StopAnimation(int animationId)
        {
            if (_activeAnimations.TryGetValue(animationId, out Coroutine coroutine))
            {
                StopCoroutine(coroutine);
                _activeAnimations.Remove(animationId);
            }
        }

        /// <summary>
        /// Stop all active animations.
        /// </summary>
        public void StopAllAnimations()
        {
            foreach (var coroutine in _activeAnimations.Values)
            {
                StopCoroutine(coroutine);
            }

            _activeAnimations.Clear();
        }

        private IEnumerator FadeCoroutine(int animationId, VisualElement element, float startOpacity, float endOpacity,
            float duration, float delay, EasingFunction easing, Action onComplete)
        {
            // Create a task completion source outside the try-catch block
            TaskCompletionSource<bool> fadeAnimationTask = new TaskCompletionSource<bool>();

            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }

            try
            {
                if (element == null)
                {
                    Debug.LogWarning("[UIAnimationSystem] Cannot animate null element");
                    yield break;
                }

                float startTime = Time.time;
                float endTime = startTime + duration;

                // Set initial value
                element.style.opacity = startOpacity;

                // Create a separate coroutine for the animation loop to avoid yield in try-catch
                IEnumerator AnimationLoop()
                {
                    while (Time.time < endTime)
                    {
                        float elapsed = Time.time - startTime;
                        float normalizedTime = Mathf.Clamp01(elapsed / duration);

                        // Apply easing if provided
                        if (easing != null)
                        {
                            normalizedTime = easing(normalizedTime);
                        }

                        // Interpolate value
                        float currentOpacity = Mathf.Lerp(startOpacity, endOpacity, normalizedTime);
                        element.style.opacity = currentOpacity;

                        yield return null;
                    }
                    yield break;
                }

                // We need to avoid yield return in try-catch

                // Start the animation in a separate coroutine
                StartCoroutine(RunAnimationSafely(AnimationLoop(), fadeAnimationTask));
            }
            catch (Exception e)
            {
                Debug.LogError($"[UIAnimationSystem] Error in fade animation: {e.Message}");

                // Try to set the final value even if there was an error
                if (element != null)
                {
                    element.style.opacity = endOpacity;
                }

                // Remove from active animations
                _activeAnimations.Remove(animationId);

                // Complete the coroutine
                yield break;
            }

            // Wait for the animation to complete outside the try-catch
            while (!fadeAnimationTask.Task.IsCompleted)
            {
                yield return null;
            }

            try
            {

                // Ensure final value is set
                element.style.opacity = endOpacity;

                // Invoke completion callback
                onComplete?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[UIAnimationSystem] Error in fade animation: {e.Message}");

                // Try to set the final value even if there was an error
                if (element != null)
                {
                    element.style.opacity = endOpacity;
                }
            }
            finally
            {
                // Remove from active animations
                _activeAnimations.Remove(animationId);
            }
        }

        private IEnumerator MoveCoroutine(int animationId, VisualElement element, Vector2 startPosition, Vector2 endPosition,
            float duration, float delay, EasingFunction easing, Action onComplete)
        {
            // Create a task completion source outside the try-catch block
            TaskCompletionSource<bool> moveAnimationTask = new TaskCompletionSource<bool>();

            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }

            try
            {
                if (element == null)
                {
                    Debug.LogWarning("[UIAnimationSystem] Cannot animate null element");
                    yield break;
                }

                float startTime = Time.time;
                float endTime = startTime + duration;

                // Set initial position
                element.style.left = startPosition.x;
                element.style.top = startPosition.y;

                // Create a separate coroutine for the animation loop to avoid yield in try-catch
                IEnumerator AnimationLoop()
                {
                    while (Time.time < endTime)
                    {
                        float elapsed = Time.time - startTime;
                        float normalizedTime = Mathf.Clamp01(elapsed / duration);

                        // Apply easing if provided
                        if (easing != null)
                        {
                            normalizedTime = easing(normalizedTime);
                        }

                        // Interpolate position
                        Vector2 currentPosition = Vector2.Lerp(startPosition, endPosition, normalizedTime);
                        element.style.left = currentPosition.x;
                        element.style.top = currentPosition.y;

                        yield return null;
                    }
                    yield break;
                }

                // We need to avoid yield return in try-catch

                // Start the animation in a separate coroutine
                StartCoroutine(RunAnimationSafely(AnimationLoop(), moveAnimationTask));
            }
            catch (Exception e)
            {
                Debug.LogError($"[UIAnimationSystem] Error in move animation: {e.Message}");

                // Try to set the final position even if there was an error
                if (element != null)
                {
                    element.style.left = endPosition.x;
                    element.style.top = endPosition.y;
                }

                // Remove from active animations
                _activeAnimations.Remove(animationId);

                // Complete the coroutine
                yield break;
            }

            // Wait for the animation to complete outside the try-catch
            while (!moveAnimationTask.Task.IsCompleted)
            {
                yield return null;
            }

            try
            {

                // Ensure final position is set
                element.style.left = endPosition.x;
                element.style.top = endPosition.y;

                // Invoke completion callback
                onComplete?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[UIAnimationSystem] Error in move animation: {e.Message}");

                // Try to set the final position even if there was an error
                if (element != null)
                {
                    element.style.left = endPosition.x;
                    element.style.top = endPosition.y;
                }
            }
            finally
            {
                // Remove from active animations
                _activeAnimations.Remove(animationId);
            }
        }

        private IEnumerator ScaleCoroutine(int animationId, VisualElement element, Vector2 startScale, Vector2 endScale,
            float duration, float delay, EasingFunction easing, Action onComplete)
        {
            // Create a task completion source outside the try-catch block
            TaskCompletionSource<bool> scaleAnimationTask = new TaskCompletionSource<bool>();

            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }

            try
            {
                if (element == null)
                {
                    Debug.LogWarning("[UIAnimationSystem] Cannot animate null element");
                    yield break;
                }

                float startTime = Time.time;
                float endTime = startTime + duration;

                // Set initial scale
                element.style.scale = new StyleScale(new Vector3(startScale.x, startScale.y, 1));

                // Create a separate coroutine for the animation loop to avoid yield in try-catch
                IEnumerator AnimationLoop()
                {
                    while (Time.time < endTime)
                    {
                        float elapsed = Time.time - startTime;
                        float normalizedTime = Mathf.Clamp01(elapsed / duration);

                        // Apply easing if provided
                        if (easing != null)
                        {
                            normalizedTime = easing(normalizedTime);
                        }

                        // Interpolate scale
                        Vector2 currentScale = Vector2.Lerp(startScale, endScale, normalizedTime);
                        element.style.scale = new StyleScale(new Vector3(currentScale.x, currentScale.y, 1));

                        yield return null;
                    }
                }

                // We need to avoid yield return in try-catch

                // Start the animation in a separate coroutine
                StartCoroutine(RunAnimationSafely(AnimationLoop(), scaleAnimationTask));
            }
            catch (Exception e)
            {
                Debug.LogError($"[UIAnimationSystem] Error in scale animation: {e.Message}");

                // Try to set the final scale even if there was an error
                if (element != null)
                {
                    element.style.scale = new StyleScale(new Vector3(endScale.x, endScale.y, 1));
                }

                // Remove from active animations
                _activeAnimations.Remove(animationId);

                // Complete the coroutine
                yield break;
            }

            // Wait for the animation to complete outside the try-catch
            while (!scaleAnimationTask.Task.IsCompleted)
            {
                yield return null;
            }

            try
            {

                // Ensure final scale is set
                element.style.scale = new StyleScale(new Vector3(endScale.x, endScale.y, 1));

                // Invoke completion callback
                onComplete?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[UIAnimationSystem] Error in scale animation: {e.Message}");

                // Try to set the final scale even if there was an error
                if (element != null)
                {
                    element.style.scale = new StyleScale(new Vector3(endScale.x, endScale.y, 1));
                }
            }
            finally
            {
                // Remove from active animations
                _activeAnimations.Remove(animationId);
            }
        }

        private IEnumerator RotateCoroutine(int animationId, VisualElement element, float startRotation, float endRotation,
            float duration, float delay, EasingFunction easing, Action onComplete)
        {
            // Create a task completion source outside the try-catch block
            TaskCompletionSource<bool> rotateAnimationTask = new TaskCompletionSource<bool>();

            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }

            try
            {
                if (element == null)
                {
                    Debug.LogWarning("[UIAnimationSystem] Cannot animate null element");
                    yield break;
                }

                float startTime = Time.time;
                float endTime = startTime + duration;

                // Set initial rotation
                element.style.rotate = new Rotate(startRotation);

                // Create a separate coroutine for the animation loop to avoid yield in try-catch
                IEnumerator AnimationLoop()
                {
                    while (Time.time < endTime)
                    {
                        float elapsed = Time.time - startTime;
                        float normalizedTime = Mathf.Clamp01(elapsed / duration);

                        // Apply easing if provided
                        if (easing != null)
                        {
                            normalizedTime = easing(normalizedTime);
                        }

                        // Interpolate rotation
                        float currentRotation = Mathf.Lerp(startRotation, endRotation, normalizedTime);
                        element.style.rotate = new Rotate(currentRotation);

                        yield return null;
                    }
                }

                // We need to avoid yield return in try-catch

                // Start the animation in a separate coroutine
                StartCoroutine(RunAnimationSafely(AnimationLoop(), rotateAnimationTask));
            }
            catch (Exception e)
            {
                Debug.LogError($"[UIAnimationSystem] Error in rotate animation: {e.Message}");

                // Try to set the final rotation even if there was an error
                if (element != null)
                {
                    element.style.rotate = new Rotate(endRotation);
                }

                // Remove from active animations
                _activeAnimations.Remove(animationId);

                // Complete the coroutine
                yield break;
            }

            // Wait for the animation to complete outside the try-catch
            while (!rotateAnimationTask.Task.IsCompleted)
            {
                yield return null;
            }

            try
            {

                // Ensure final rotation is set
                element.style.rotate = new Rotate(endRotation);

                // Invoke completion callback
                onComplete?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[UIAnimationSystem] Error in rotate animation: {e.Message}");

                // Try to set the final rotation even if there was an error
                if (element != null)
                {
                    element.style.rotate = new Rotate(endRotation);
                }
            }
            finally
            {
                // Remove from active animations
                _activeAnimations.Remove(animationId);
            }
        }

        /// <summary>
        /// Helper method to run an animation coroutine safely and signal completion.
        /// </summary>
        private IEnumerator RunAnimationSafely(IEnumerator animation, TaskCompletionSource<bool> completionSource)
        {
            // We can't use yield in try-catch, so we'll use a different approach
            // Run the animation step by step
            bool isRunning = true;

            while (isRunning)
            {
                object current = null;
                bool moveNextSuccess;

                try
                {
                    // Try to move to the next step
                    moveNextSuccess = animation.MoveNext();
                    if (moveNextSuccess)
                    {
                        current = animation.Current;
                    }
                    else
                    {
                        isRunning = false;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[UIAnimationSystem] Error advancing animation: {e.Message}");
                    completionSource.SetException(e);
                    yield break;
                }

                // If we're still running, yield the current value
                if (isRunning)
                {
                    yield return current;
                }
            }

            // If we got here without errors, the animation completed successfully
            completionSource.SetResult(true);
        }

        /// <summary>
        /// Convert EasingType to EasingFunction
        /// </summary>
        private EasingFunction GetEasingFunction(EasingType easingType)
        {
            switch (easingType)
            {
                case EasingType.Linear: return UIEasing.Linear;
                case EasingType.EaseInQuad: return UIEasing.EaseInQuad;
                case EasingType.EaseOutQuad: return UIEasing.EaseOutQuad;
                case EasingType.EaseInOutQuad: return UIEasing.EaseInOutQuad;
                case EasingType.EaseInCubic: return UIEasing.EaseInCubic;
                case EasingType.EaseOutCubic: return UIEasing.EaseOutCubic;
                case EasingType.EaseInOutCubic: return UIEasing.EaseInOutCubic;
                case EasingType.EaseInSine: return UIEasing.EaseInSine;
                case EasingType.EaseOutSine: return UIEasing.EaseOutSine;
                case EasingType.EaseInOutSine: return UIEasing.EaseInOutSine;
                case EasingType.EaseInExpo: return UIEasing.EaseInExpo;
                case EasingType.EaseOutExpo: return UIEasing.EaseOutExpo;
                case EasingType.EaseInOutExpo: return UIEasing.EaseInOutExpo;
                case EasingType.EaseInElastic: return UIEasing.EaseInElastic;
                case EasingType.EaseOutElastic: return UIEasing.EaseOutElastic;
                case EasingType.EaseInOutElastic: return UIEasing.EaseInOutElastic;
                case EasingType.EaseInBack: return UIEasing.EaseInBack;
                case EasingType.EaseOutBack: return UIEasing.EaseOutBack;
                case EasingType.EaseInOutBack: return UIEasing.EaseInOutBack;
                case EasingType.EaseInBounce: return UIEasing.EaseInBounce;
                case EasingType.EaseOutBounce: return UIEasing.EaseOutBounce;
                case EasingType.EaseInOutBounce: return UIEasing.EaseInOutBounce;
                default: return UIEasing.Linear;
            }
        }

        /// <summary>
        /// Animate a sequence of elements one after another
        /// </summary>
        /// <param name="elements">List of elements to animate</param>
        /// <param name="animationType">Animation type to apply to all elements</param>
        /// <param name="duration">Duration for each animation</param>
        /// <param name="stagger">Delay between each element's animation</param>
        /// <param name="easingType">Easing type to use</param>
        /// <param name="onAllComplete">Callback when all animations complete</param>
        public void AnimateSequence(List<VisualElement> elements, AnimationType animationType,
            float duration = 0.3f, float stagger = 0.05f, EasingType easingType = EasingType.EaseOutQuad,
            Action onAllComplete = null)
        {
            if (elements == null || elements.Count == 0)
            {
                onAllComplete?.Invoke();
                return;
            }

            StartCoroutine(AnimateSequenceCoroutine(elements, animationType, duration, stagger, easingType, onAllComplete));
        }

        /// <summary>
        /// Chain multiple animations on a single element
        /// </summary>
        /// <param name="element">Element to animate</param>
        /// <param name="animations">List of animations to apply in sequence</param>
        /// <param name="durations">List of durations for each animation</param>
        /// <param name="delays">List of delays for each animation</param>
        /// <param name="easingTypes">List of easing types for each animation</param>
        /// <param name="onComplete">Callback when all animations complete</param>
        public void ChainAnimations(VisualElement element, List<AnimationType> animations,
            List<float> durations = null, List<float> delays = null, List<EasingType> easingTypes = null,
            Action onComplete = null)
        {
            if (element == null || animations == null || animations.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

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

            if (easingTypes == null)
            {
                easingTypes = new List<EasingType>();
                for (int i = 0; i < animations.Count; i++)
                    easingTypes.Add(EasingType.EaseOutQuad);
            }

            // Ensure lists have the same length
            int count = Mathf.Min(animations.Count, durations.Count, delays.Count, easingTypes.Count);

            StartCoroutine(ChainAnimationsCoroutine(element, animations, durations, delays, easingTypes, count, onComplete));
        }

        /// <summary>
        /// Animate a sequence of elements one after another
        /// </summary>
        private IEnumerator AnimateSequenceCoroutine(List<VisualElement> elements, AnimationType animationType,
            float duration, float stagger, EasingType easingType, Action onAllComplete)
        {
            int count = elements.Count;
            int completed = 0;

            // Animate each element with a staggered delay
            for (int i = 0; i < count; i++)
            {
                VisualElement element = elements[i];
                float delay = i * stagger;

                // Animate the element
                int animId = Animate(element, animationType, duration, delay, GetEasingFunction(easingType), () =>
                {
                    completed++;
                });

                // Wait a bit before starting the next animation
                if (i < count - 1)
                {
                    yield return new WaitForSeconds(stagger);
                }
            }

            // Wait for all animations to complete
            while (completed < count)
            {
                yield return null;
            }

            // Call the completion callback
            onAllComplete?.Invoke();
        }

        /// <summary>
        /// Chain multiple animations on a single element
        /// </summary>
        private IEnumerator ChainAnimationsCoroutine(VisualElement element, List<AnimationType> animations,
            List<float> durations, List<float> delays, List<EasingType> easingTypes, int count, Action onComplete)
        {
            for (int i = 0; i < count; i++)
            {
                // Create a task completion source for this animation
                TaskCompletionSource<bool> animationTask = new TaskCompletionSource<bool>();

                // Animate the element
                int animId = Animate(element, animations[i], durations[i], delays[i], GetEasingFunction(easingTypes[i]), () =>
                {
                    animationTask.SetResult(true);
                });

                // Wait for this animation to complete
                while (!animationTask.Task.IsCompleted)
                {
                    yield return null;
                }
            }

            // Call the completion callback
            onComplete?.Invoke();
        }

        /// <summary>
        /// Bounce animation coroutine
        /// </summary>
        private IEnumerator BounceCoroutine(VisualElement element, Vector2 originalScale, float duration,
            float delay, EasingFunction easing, Action onComplete)
        {
            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }

            // Scale up
            TaskCompletionSource<bool> scaleUpTask = new TaskCompletionSource<bool>();
            Scale(element, originalScale, originalScale * 1.2f, duration * 0.3f, 0f, UIEasing.EaseOutQuad, () =>
            {
                scaleUpTask.SetResult(true);
            });

            while (!scaleUpTask.Task.IsCompleted)
            {
                yield return null;
            }

            // Scale down
            TaskCompletionSource<bool> scaleDownTask = new TaskCompletionSource<bool>();
            Scale(element, originalScale * 1.2f, originalScale * 0.9f, duration * 0.3f, 0f, UIEasing.EaseInOutQuad, () =>
            {
                scaleDownTask.SetResult(true);
            });

            while (!scaleDownTask.Task.IsCompleted)
            {
                yield return null;
            }

            // Scale to original
            TaskCompletionSource<bool> scaleOriginalTask = new TaskCompletionSource<bool>();
            Scale(element, originalScale * 0.9f, originalScale, duration * 0.4f, 0f, UIEasing.EaseOutElastic, () =>
            {
                scaleOriginalTask.SetResult(true);
            });

            while (!scaleOriginalTask.Task.IsCompleted)
            {
                yield return null;
            }

            // Call the completion callback
            onComplete?.Invoke();
        }

        /// <summary>
        /// Pulse animation coroutine
        /// </summary>
        private IEnumerator PulseCoroutine(VisualElement element, Vector2 originalScale, float duration,
            float delay, EasingFunction easing, Action onComplete)
        {
            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }

            // Scale up
            TaskCompletionSource<bool> scaleUpTask = new TaskCompletionSource<bool>();
            Scale(element, originalScale, originalScale * 1.1f, duration * 0.5f, 0f, UIEasing.EaseOutQuad, () =>
            {
                scaleUpTask.SetResult(true);
            });

            while (!scaleUpTask.Task.IsCompleted)
            {
                yield return null;
            }

            // Scale to original
            TaskCompletionSource<bool> scaleOriginalTask = new TaskCompletionSource<bool>();
            Scale(element, originalScale * 1.1f, originalScale, duration * 0.5f, 0f, UIEasing.EaseInQuad, () =>
            {
                scaleOriginalTask.SetResult(true);
            });

            while (!scaleOriginalTask.Task.IsCompleted)
            {
                yield return null;
            }

            // Call the completion callback
            onComplete?.Invoke();
        }

        /// <summary>
        /// Shake animation coroutine
        /// </summary>
        private IEnumerator ShakeCoroutine(VisualElement element, Vector2 originalPosition, float duration,
            float delay, EasingFunction easing, Action onComplete)
        {
            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }

            float startTime = Time.time;
            float endTime = startTime + duration;

            // Set initial position
            Vector2 currentPosition = originalPosition;

            while (Time.time < endTime)
            {
                float elapsed = Time.time - startTime;
                float normalizedTime = Mathf.Clamp01(elapsed / duration);

                // Apply easing if provided
                if (easing != null)
                {
                    normalizedTime = easing(normalizedTime);
                }

                // Calculate shake amount (diminishes over time)
                float shakeAmount = 10f * (1 - normalizedTime);

                // Generate random offset
                float offsetX = UnityEngine.Random.Range(-shakeAmount, shakeAmount);
                float offsetY = UnityEngine.Random.Range(-shakeAmount, shakeAmount);

                // Apply offset
                element.style.left = originalPosition.x + offsetX;
                element.style.top = originalPosition.y + offsetY;

                yield return null;
            }

            // Ensure final position is set
            element.style.left = originalPosition.x;
            element.style.top = originalPosition.y;

            // Call the completion callback
            onComplete?.Invoke();
        }

        /// <summary>
        /// Animate a VisualElement with a predefined animation type
        /// </summary>
        public int Animate(VisualElement element, AnimationType animationType, float duration = 0.3f,
            float delay = 0f, EasingFunction easing = null, Action onComplete = null)
        {
            if (element == null)
            {
                Debug.LogWarning("[UIAnimationSystem] Cannot animate null element");
                return -1;
            }

            // Store original values
            Vector2 originalPosition = new Vector2(element.style.left.value.value, element.style.top.value.value);
            Vector2 originalScale = Vector2.one;
            float originalOpacity = element.style.opacity.value;
            float originalRotation = 0f;

            // Handle different animation types
            switch (animationType)
            {
                case AnimationType.FadeIn:
                    return Fade(element, 0f, 1f, duration, delay, easing, onComplete);

                case AnimationType.FadeOut:
                    return Fade(element, 1f, 0f, duration, delay, easing, onComplete);

                case AnimationType.SlideInFromLeft:
                    {
                        Vector2 startPos = new Vector2(-element.layout.width, originalPosition.y);
                        return Move(element, startPos, originalPosition, duration, delay, easing, onComplete);
                    }

                case AnimationType.SlideInFromRight:
                    {
                        Vector2 startPos = new Vector2(Screen.width, originalPosition.y);
                        return Move(element, startPos, originalPosition, duration, delay, easing, onComplete);
                    }

                case AnimationType.SlideInFromTop:
                    {
                        Vector2 startPos = new Vector2(originalPosition.x, -element.layout.height);
                        return Move(element, startPos, originalPosition, duration, delay, easing, onComplete);
                    }

                case AnimationType.SlideInFromBottom:
                    {
                        Vector2 startPos = new Vector2(originalPosition.x, Screen.height);
                        return Move(element, startPos, originalPosition, duration, delay, easing, onComplete);
                    }

                case AnimationType.SlideOutToLeft:
                    {
                        Vector2 endPos = new Vector2(-element.layout.width, originalPosition.y);
                        return Move(element, originalPosition, endPos, duration, delay, easing, onComplete);
                    }

                case AnimationType.SlideOutToRight:
                    {
                        Vector2 endPos = new Vector2(Screen.width, originalPosition.y);
                        return Move(element, originalPosition, endPos, duration, delay, easing, onComplete);
                    }

                case AnimationType.SlideOutToTop:
                    {
                        Vector2 endPos = new Vector2(originalPosition.x, -element.layout.height);
                        return Move(element, originalPosition, endPos, duration, delay, easing, onComplete);
                    }

                case AnimationType.SlideOutToBottom:
                    {
                        Vector2 endPos = new Vector2(originalPosition.x, Screen.height);
                        return Move(element, originalPosition, endPos, duration, delay, easing, onComplete);
                    }

                case AnimationType.ScaleIn:
                    return Scale(element, Vector2.zero, Vector2.one, duration, delay, easing, onComplete);

                case AnimationType.ScaleOut:
                    return Scale(element, Vector2.one, Vector2.zero, duration, delay, easing, onComplete);

                case AnimationType.RotateIn:
                    {
                        int fadeId = Fade(element, 0f, 1f, duration, delay, easing, null);
                        int rotateId = Rotate(element, 180f, 0f, duration, delay, easing, onComplete);
                        return rotateId; // Return the last animation ID
                    }

                case AnimationType.RotateOut:
                    {
                        int fadeId = Fade(element, 1f, 0f, duration, delay, easing, null);
                        int rotateId = Rotate(element, 0f, 180f, duration, delay, easing, onComplete);
                        return rotateId; // Return the last animation ID
                    }

                case AnimationType.Bounce:
                    {
                        // Start the bounce animation
                        StartCoroutine(BounceCoroutine(element, Vector2.one, duration, delay, easing, onComplete));
                        return _nextAnimationId++;
                    }

                case AnimationType.Pulse:
                    {
                        // Start the pulse animation
                        StartCoroutine(PulseCoroutine(element, Vector2.one, duration, delay, easing, onComplete));
                        return _nextAnimationId++;
                    }

                case AnimationType.Shake:
                    {
                        // Start the shake animation
                        StartCoroutine(ShakeCoroutine(element, originalPosition, duration, delay, easing, onComplete));
                        return _nextAnimationId++;
                    }

                default:
                    Debug.LogWarning($"[UIAnimationSystem] Animation type {animationType} not implemented");
                    return -1;
            }
        }
    }

    /// <summary>
    /// Delegate for easing functions.
    /// </summary>
    /// <param name="t">Normalized time (0-1)</param>
    /// <returns>Eased value</returns>
    public delegate float EasingFunction(float t);
}
