using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.Animation
{
    /// <summary>
    /// Animation service interface - supports both UI Toolkit and GameObject animations
    /// Follows Interface Segregation Principle - focused on animation concerns only
    /// </summary>
    public interface IAnimationService
    {
        /// <summary>
        /// Animate UI element opacity
        /// </summary>
        void AnimateOpacity(VisualElement element, float from, float to, float duration, Action onComplete = null);
        
        /// <summary>
        /// Animate UI element position
        /// </summary>
        void AnimatePosition(VisualElement element, Vector2 from, Vector2 to, float duration, Action onComplete = null);
        
        /// <summary>
        /// Animate UI element scale
        /// </summary>
        void AnimateScale(VisualElement element, Vector2 from, Vector2 to, float duration, Action onComplete = null);
        
        /// <summary>
        /// Animate UI element rotation
        /// </summary>
        void AnimateRotation(VisualElement element, float from, float to, float duration, Action onComplete = null);
        
        /// <summary>
        /// Animate GameObject position
        /// </summary>
        void AnimateTransformPosition(Transform transform, Vector3 to, float duration, Action onComplete = null);
        
        /// <summary>
        /// Animate GameObject scale
        /// </summary>
        void AnimateTransformScale(Transform transform, Vector3 to, float duration, Action onComplete = null);
        
        /// <summary>
        /// Animate GameObject rotation
        /// </summary>
        void AnimateTransformRotation(Transform transform, Vector3 to, float duration, Action onComplete = null);
        
        /// <summary>
        /// Kill all animations on a UI element
        /// </summary>
        void KillAnimations(VisualElement element);
        
        /// <summary>
        /// Kill all animations on a GameObject
        /// </summary>
        void KillAnimations(Transform transform);
        
        /// <summary>
        /// Kill all active animations
        /// </summary>
        void KillAllAnimations();
        
        /// <summary>
        /// Access to specialized UI animator
        /// </summary>
        IUIAnimator UI { get; }
        
        /// <summary>
        /// Access to specialized Transform animator
        /// </summary>
        ITransformAnimator Transform { get; }
    }
}
