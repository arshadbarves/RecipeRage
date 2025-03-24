using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace RecipeRage.UI.Animation
{
    /// <summary>
    /// Core animation class for UI Toolkit elements.
    /// Handles animation state, callbacks, and execution.
    /// </summary>
    public class UIAnimation
    {
        private readonly string _id;
        private readonly VisualElement _element;
        private readonly List<IValueAnimation> _valueAnimations = new List<IValueAnimation>();
        private readonly List<Action> _sequenceActions = new List<Action>();

        private int _currentSequenceIndex = 0;
        private bool _isPlaying = false;
        private bool _isLooping = false;
        private Action _onComplete;
        private int _completedAnimationsCount = 0;

        /// <summary>
        /// Create a new UI animation for the specified element
        /// </summary>
        /// <param name="element">The visual element to animate</param>
        /// <param name="id">Unique identifier for this animation</param>
        public UIAnimation(VisualElement element, string id)
        {
            _element = element ?? throw new ArgumentNullException(nameof(element));
            _id = id ?? throw new ArgumentNullException(nameof(id));
        }

        /// <summary>
        /// Gets the target visual element
        /// </summary>
        public VisualElement Element => _element;

        /// <summary>
        /// Gets the animation's unique ID
        /// </summary>
        public string Id => _id;

        /// <summary>
        /// Gets whether the animation is currently playing
        /// </summary>
        public bool IsPlaying => _isPlaying;

        /// <summary>
        /// Adds a value animation to this animation
        /// </summary>
        /// <param name="animation">The value animation to add</param>
        public void AddValueAnimation(IValueAnimation animation)
        {
            _valueAnimations.Add(animation);

            // Register for animation completion via Unity's animation system
            // Since there's no direct event in IValueAnimation, we'll check for completion in Update
            _element.schedule.Execute(() => CheckAnimationCompletion());
        }

        /// <summary>
        /// Add a sequence action to run in order
        /// </summary>
        /// <param name="action">The animation action to run</param>
        public void AddSequenceAction(Action action)
        {
            _sequenceActions.Add(action);
        }

        /// <summary>
        /// Set the completion callback
        /// </summary>
        /// <param name="onComplete">Action to run when animation completes</param>
        public void SetCompletionCallback(Action onComplete)
        {
            _onComplete = onComplete;
        }

        /// <summary>
        /// Set looping behavior
        /// </summary>
        /// <param name="loop">Whether the animation should loop</param>
        public void SetLooping(bool loop)
        {
            _isLooping = loop;
        }

        /// <summary>
        /// Start playing the animation
        /// </summary>
        public void Play()
        {
            if (_isPlaying) return;

            _isPlaying = true;
            _currentSequenceIndex = 0;
            _completedAnimationsCount = 0;

            if (_sequenceActions.Count > 0)
            {
                PlayNextInSequence();
            }
            else
            {
                PlayAllAnimations();
            }
        }

        /// <summary>
        /// Stop all animations
        /// </summary>
        public void Stop()
        {
            _isPlaying = false;

            foreach (var animation in _valueAnimations)
            {
                animation.Stop();
            }
        }

        private void PlayAllAnimations()
        {
            if (_valueAnimations.Count == 0)
            {
                HandleAnimationComplete();
                return;
            }

            foreach (var animation in _valueAnimations)
            {
                animation.Start();
            }
        }

        // Uses the schedule system to check for animation completion
        private void CheckAnimationCompletion()
        {
            if (!_isPlaying) return;

            int runningCount = 0;
            foreach (var anim in _valueAnimations)
            {
                if (anim.isRunning)
                {
                    runningCount++;
                }
            }

            // If no animations are running, consider the sequence complete
            if (runningCount == 0 && _valueAnimations.Count > 0)
            {
                HandleAnimationComplete();
            }
            else if (_isPlaying)
            {
                // Continue checking until animations complete
                _element.schedule.Execute(() => CheckAnimationCompletion()).StartingIn(16); // Check roughly every frame
            }
        }

        private void PlayNextInSequence()
        {
            if (_currentSequenceIndex >= _sequenceActions.Count)
            {
                HandleAnimationComplete();
                return;
            }

            _sequenceActions[_currentSequenceIndex].Invoke();
            _currentSequenceIndex++;
        }

        private void HandleAnimationComplete()
        {
            if (_isLooping)
            {
                Play(); // Restart from beginning
            }
            else
            {
                _isPlaying = false;
                _onComplete?.Invoke();
            }
        }
    }
}