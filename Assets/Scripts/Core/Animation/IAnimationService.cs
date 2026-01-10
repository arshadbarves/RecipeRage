using Core.Shared.Interfaces;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.Animation
{
    public interface IAnimationService : IInitializable
    {
        void KillAnimations(VisualElement element);

        void KillAnimations(Transform transform);

        void KillAllAnimations();

        IUIAnimator UI { get; }

        ITransformAnimator Transform { get; }
    }
}