using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using RecipeRage.Core.Patterns;
using RecipeRage.Modules.Logging;
using System.Linq; // Added for Linq operations
using System; // Added for Guid

namespace RecipeRage.UI.Animation
{
    /// <summary>
    /// Manages all active UI animations.
    /// </summary>
    public class UIAnimationController : MonoBehaviourSingleton<UIAnimationController>
    {
        // No longer need to manually track/update animations here
        // The UIAnimation class handles its lifecycle via scheduling

        // Still useful for creating animations and stopping them globally
        private HashSet<UIAnimation> _registeredAnimations = new HashSet<UIAnimation>();

        /// <summary>
        /// Creates a new animation builder for the specified element.
        /// </summary>
        public UIAnimationBuilder Create(VisualElement element)
        {
            if (element == null)
            {
                LogHelper.Error("UIAnimationController", "Cannot create animation for a null VisualElement.");
                return null;
            }
            // Generate a unique ID for the animation instance
            string animId = $"{element.name ?? "element"}_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var newAnimation = new UIAnimation(element, animId);
            var builder = new UIAnimationBuilder(newAnimation);
            RegisterAnimation(newAnimation); // Register it so we can stop it later if needed
            return builder;
        }

        // Register an animation instance
        internal void RegisterAnimation(UIAnimation animation)
        {
            if (_registeredAnimations.Add(animation))
            {
                LogHelper.Debug("UIAnimationController", $"Registered animation: {animation.Id} for {animation.Element?.name ?? "UnnamedElement"}");
            }
        }

        // Unregister an animation instance (e.g., when it completes or is stopped)
        internal void UnregisterAnimation(UIAnimation animation)
        {
            if (_registeredAnimations.Remove(animation))
            {
                LogHelper.Debug("UIAnimationController", $"Unregistered animation: {animation.Id} for {animation.Element?.name ?? "UnnamedElement"}");
            }
        }

        /// <summary>
        /// Stops all animations targeting a specific VisualElement.
        /// </summary>
        public void StopAnimations(VisualElement element)
        {
            if (element == null) return;

            // Find registered animations targeting this element
            var animationsToStop = _registeredAnimations.Where(anim => anim.Element == element).ToList(); // ToList to avoid modification issues
            int count = 0;
            foreach (var anim in animationsToStop)
            {
                anim.Stop();
                UnregisterAnimation(anim); // Remove from tracking
                count++;
            }
            if (count > 0)
            {
                LogHelper.Debug("UIAnimationController", $"Stopped {count} animations for element: {element.name ?? "UnnamedElement"}");
            }
        }

        /// <summary>
        /// Stops all active animations managed by this controller.
        /// </summary>
        public void StopAllAnimations()
        {
            LogHelper.Debug("UIAnimationController", "Stopping all animations.");
            var allAnimations = _registeredAnimations.ToList(); // Copy to avoid modification issues
            foreach (var anim in allAnimations)
            {
                anim.Stop();
                UnregisterAnimation(anim);
            }
        }

        // Optional: Cleanup on destroy
        protected override void OnDestroy()
        {
            StopAllAnimations();
            base.OnDestroy();
        }
    }
}
