using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.UI.Animation
{
    /// <summary>
    /// Sequence of UI animations that can be played in order.
    /// </summary>
    public class UIAnimationSequence
    {
        private List<AnimationStep> _steps = new List<AnimationStep>();
        private Action _onComplete;
        private bool _isPlaying = false;
        private Coroutine _currentCoroutine;
        
        /// <summary>
        /// Add a fade animation to the sequence.
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="startOpacity">Starting opacity (0-1)</param>
        /// <param name="endOpacity">Ending opacity (0-1)</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="delay">Delay before starting in seconds</param>
        /// <param name="easing">Easing function to use</param>
        /// <returns>This sequence for chaining</returns>
        public UIAnimationSequence Fade(VisualElement element, float startOpacity, float endOpacity, float duration,
            float delay = 0f, EasingFunction easing = null)
        {
            _steps.Add(new AnimationStep
            {
                Type = AnimationType.Fade,
                Element = element,
                StartValue = new Vector3(startOpacity, 0, 0),
                EndValue = new Vector3(endOpacity, 0, 0),
                Duration = duration,
                Delay = delay,
                Easing = easing
            });
            
            return this;
        }
        
        /// <summary>
        /// Add a move animation to the sequence.
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="startPosition">Starting position</param>
        /// <param name="endPosition">Ending position</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="delay">Delay before starting in seconds</param>
        /// <param name="easing">Easing function to use</param>
        /// <returns>This sequence for chaining</returns>
        public UIAnimationSequence Move(VisualElement element, Vector2 startPosition, Vector2 endPosition, float duration,
            float delay = 0f, EasingFunction easing = null)
        {
            _steps.Add(new AnimationStep
            {
                Type = AnimationType.Move,
                Element = element,
                StartValue = new Vector3(startPosition.x, startPosition.y, 0),
                EndValue = new Vector3(endPosition.x, endPosition.y, 0),
                Duration = duration,
                Delay = delay,
                Easing = easing
            });
            
            return this;
        }
        
        /// <summary>
        /// Add a scale animation to the sequence.
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="startScale">Starting scale</param>
        /// <param name="endScale">Ending scale</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="delay">Delay before starting in seconds</param>
        /// <param name="easing">Easing function to use</param>
        /// <returns>This sequence for chaining</returns>
        public UIAnimationSequence Scale(VisualElement element, Vector2 startScale, Vector2 endScale, float duration,
            float delay = 0f, EasingFunction easing = null)
        {
            _steps.Add(new AnimationStep
            {
                Type = AnimationType.Scale,
                Element = element,
                StartValue = new Vector3(startScale.x, startScale.y, 0),
                EndValue = new Vector3(endScale.x, endScale.y, 0),
                Duration = duration,
                Delay = delay,
                Easing = easing
            });
            
            return this;
        }
        
        /// <summary>
        /// Add a rotation animation to the sequence.
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="startRotation">Starting rotation in degrees</param>
        /// <param name="endRotation">Ending rotation in degrees</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="delay">Delay before starting in seconds</param>
        /// <param name="easing">Easing function to use</param>
        /// <returns>This sequence for chaining</returns>
        public UIAnimationSequence Rotate(VisualElement element, float startRotation, float endRotation, float duration,
            float delay = 0f, EasingFunction easing = null)
        {
            _steps.Add(new AnimationStep
            {
                Type = AnimationType.Rotate,
                Element = element,
                StartValue = new Vector3(startRotation, 0, 0),
                EndValue = new Vector3(endRotation, 0, 0),
                Duration = duration,
                Delay = delay,
                Easing = easing
            });
            
            return this;
        }
        
        /// <summary>
        /// Add a delay to the sequence.
        /// </summary>
        /// <param name="duration">Duration of the delay in seconds</param>
        /// <returns>This sequence for chaining</returns>
        public UIAnimationSequence Delay(float duration)
        {
            _steps.Add(new AnimationStep
            {
                Type = AnimationType.Delay,
                Duration = duration
            });
            
            return this;
        }
        
        /// <summary>
        /// Add a callback to the sequence.
        /// </summary>
        /// <param name="callback">The callback to invoke</param>
        /// <returns>This sequence for chaining</returns>
        public UIAnimationSequence Callback(Action callback)
        {
            _steps.Add(new AnimationStep
            {
                Type = AnimationType.Callback,
                Callback = callback
            });
            
            return this;
        }
        
        /// <summary>
        /// Set a callback to be invoked when the sequence completes.
        /// </summary>
        /// <param name="callback">The callback to invoke</param>
        /// <returns>This sequence for chaining</returns>
        public UIAnimationSequence OnComplete(Action callback)
        {
            _onComplete = callback;
            return this;
        }
        
        /// <summary>
        /// Play the animation sequence.
        /// </summary>
        public void Play()
        {
            if (_isPlaying)
            {
                Debug.LogWarning("[UIAnimationSequence] Sequence is already playing");
                return;
            }
            
            _isPlaying = true;
            _currentCoroutine = UIAnimationSystem.Instance.StartCoroutine(PlaySequence());
        }
        
        /// <summary>
        /// Stop the animation sequence.
        /// </summary>
        public void Stop()
        {
            if (!_isPlaying || _currentCoroutine == null)
            {
                return;
            }
            
            UIAnimationSystem.Instance.StopCoroutine(_currentCoroutine);
            _isPlaying = false;
            _currentCoroutine = null;
        }
        
        private IEnumerator PlaySequence()
        {
            try
            {
                foreach (var step in _steps)
                {
                    yield return PlayStep(step);
                }
                
                _onComplete?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[UIAnimationSequence] Error playing sequence: {e.Message}");
            }
            finally
            {
                _isPlaying = false;
                _currentCoroutine = null;
            }
        }
        
        private IEnumerator PlayStep(AnimationStep step)
        {
            switch (step.Type)
            {
                case AnimationType.Fade:
                    yield return FadeElement(step.Element, step.StartValue.x, step.EndValue.x, step.Duration, step.Delay, step.Easing);
                    break;
                    
                case AnimationType.Move:
                    yield return MoveElement(step.Element, 
                        new Vector2(step.StartValue.x, step.StartValue.y), 
                        new Vector2(step.EndValue.x, step.EndValue.y), 
                        step.Duration, step.Delay, step.Easing);
                    break;
                    
                case AnimationType.Scale:
                    yield return ScaleElement(step.Element, 
                        new Vector2(step.StartValue.x, step.StartValue.y), 
                        new Vector2(step.EndValue.x, step.EndValue.y), 
                        step.Duration, step.Delay, step.Easing);
                    break;
                    
                case AnimationType.Rotate:
                    yield return RotateElement(step.Element, step.StartValue.x, step.EndValue.x, step.Duration, step.Delay, step.Easing);
                    break;
                    
                case AnimationType.Delay:
                    yield return new WaitForSeconds(step.Duration);
                    break;
                    
                case AnimationType.Callback:
                    step.Callback?.Invoke();
                    break;
            }
        }
        
        private IEnumerator FadeElement(VisualElement element, float startOpacity, float endOpacity, 
            float duration, float delay, EasingFunction easing)
        {
            if (element == null)
            {
                yield break;
            }
            
            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }
            
            float startTime = Time.time;
            float endTime = startTime + duration;
            
            // Set initial value
            element.style.opacity = startOpacity;
            
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
            
            // Ensure final value is set
            element.style.opacity = endOpacity;
        }
        
        private IEnumerator MoveElement(VisualElement element, Vector2 startPosition, Vector2 endPosition,
            float duration, float delay, EasingFunction easing)
        {
            if (element == null)
            {
                yield break;
            }
            
            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }
            
            float startTime = Time.time;
            float endTime = startTime + duration;
            
            // Set initial position
            element.style.left = startPosition.x;
            element.style.top = startPosition.y;
            
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
            
            // Ensure final position is set
            element.style.left = endPosition.x;
            element.style.top = endPosition.y;
        }
        
        private IEnumerator ScaleElement(VisualElement element, Vector2 startScale, Vector2 endScale,
            float duration, float delay, EasingFunction easing)
        {
            if (element == null)
            {
                yield break;
            }
            
            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }
            
            float startTime = Time.time;
            float endTime = startTime + duration;
            
            // Set initial scale
            element.style.scale = new Scale(startScale.x, startScale.y, 1);
            
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
                element.style.scale = new Scale(currentScale.x, currentScale.y, 1);
                
                yield return null;
            }
            
            // Ensure final scale is set
            element.style.scale = new Scale(endScale.x, endScale.y, 1);
        }
        
        private IEnumerator RotateElement(VisualElement element, float startRotation, float endRotation,
            float duration, float delay, EasingFunction easing)
        {
            if (element == null)
            {
                yield break;
            }
            
            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }
            
            float startTime = Time.time;
            float endTime = startTime + duration;
            
            // Set initial rotation
            element.style.rotate = new Rotate(startRotation);
            
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
            
            // Ensure final rotation is set
            element.style.rotate = new Rotate(endRotation);
        }
        
        /// <summary>
        /// Types of animations in a sequence.
        /// </summary>
        private enum AnimationType
        {
            Fade,
            Move,
            Scale,
            Rotate,
            Delay,
            Callback
        }
        
        /// <summary>
        /// Step in an animation sequence.
        /// </summary>
        private class AnimationStep
        {
            public AnimationType Type;
            public VisualElement Element;
            public Vector3 StartValue;
            public Vector3 EndValue;
            public float Duration;
            public float Delay;
            public EasingFunction Easing;
            public Action Callback;
        }
    }
}
