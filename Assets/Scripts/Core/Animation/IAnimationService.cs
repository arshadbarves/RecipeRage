using UnityEngine;
using UnityEngine.UIElements;

namespace Core.Animation
{
    public interface IAnimationService
    {
        void KillAnimations(VisualElement element);

        void KillAnimations(Transform transform);

        void KillAllAnimations();

        IUIAnimator UI { get; }

        ITransformAnimator Transform { get; }
    }
}