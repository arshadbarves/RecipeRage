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
                var animationTask = new TaskCompletionSource<bool>();

                // Start the animation in a separate coroutine
                StartCoroutine(RunAnimationSafely(AnimationLoop(), animationTask));
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
            while (!animationTask.Task.IsCompleted)
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
                var animationTask = new TaskCompletionSource<bool>();

                // Start the animation in a separate coroutine
                StartCoroutine(RunAnimationSafely(AnimationLoop(), animationTask));
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
            while (!animationTask.Task.IsCompleted)
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
                var scaleAnimationTask = new TaskCompletionSource<bool>();

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
                var rotateAnimationTask = new TaskCompletionSource<bool>();

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
                bool moveNextSuccess = false;
                object current = null;

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
    }

    /// <summary>
    /// Delegate for easing functions.
    /// </summary>
    /// <param name="t">Normalized time (0-1)</param>
    /// <returns>Eased value</returns>
    public delegate float EasingFunction(float t);
}
