using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace RecipeRage.UI.Animation
{
    /// <summary>
    /// Centralized controller for managing UI Toolkit animations.
    /// Provides a fluent API for creating and chaining animations.
    /// </summary>
    public class UIAnimationController
    {
        private static UIAnimationController _instance;
        private readonly Dictionary<string, UIAnimation> _animations = new Dictionary<string, UIAnimation>();

        /// <summary>
        /// Get the singleton instance of the animation controller
        /// </summary>
        public static UIAnimationController Instance => _instance ??= new UIAnimationController();

        /// <summary>
        /// Create a new animation for a visual element
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="id">Optional unique identifier for the animation (generates a GUID if not provided)</param>
        /// <returns>A new animation builder</returns>
        public UIAnimationBuilder Create(VisualElement element, string id = null)
        {
            id ??= Guid.NewGuid().ToString();
            var animation = new UIAnimation(element, id);
            _animations[id] = animation;
            return new UIAnimationBuilder(animation);
        }

        /// <summary>
        /// Get an existing animation by ID
        /// </summary>
        /// <param name="id">The animation ID</param>
        /// <returns>The animation builder if found, null otherwise</returns>
        public UIAnimationBuilder Get(string id)
        {
            return _animations.TryGetValue(id, out var animation)
                ? new UIAnimationBuilder(animation)
                : null;
        }

        /// <summary>
        /// Stop and remove an animation
        /// </summary>
        /// <param name="id">The animation ID</param>
        public void Stop(string id)
        {
            if (_animations.TryGetValue(id, out var animation))
            {
                animation.Stop();
                _animations.Remove(id);
            }
        }

        /// <summary>
        /// Stop all animations
        /// </summary>
        public void StopAll()
        {
            foreach (var animation in _animations.Values)
            {
                animation.Stop();
            }
            _animations.Clear();
        }
    }
}