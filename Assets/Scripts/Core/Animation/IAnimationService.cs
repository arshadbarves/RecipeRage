using System.Threading;
using Core.Bootstrap;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.Animation
{
    /// <summary>
    /// Animation service interface - supports both UI Toolkit and GameObject animations
    /// Follows Interface Segregation Principle - focused on animation concerns only
    /// </summary>
    public interface IAnimationService : IInitializable
    {
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
