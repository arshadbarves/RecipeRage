using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.Animation
{
    /// <summary>
    /// Main animation service - facade for UI and Transform animators
    /// </summary>
    public class AnimationService : IAnimationService
    {
        public IUIAnimator UI { get; }

        public ITransformAnimator Transform { get; }

        public AnimationService(IUIAnimator uiAnimator, ITransformAnimator transformAnimator)
        {
            UI = uiAnimator ?? throw new ArgumentNullException(nameof(uiAnimator));
            Transform = transformAnimator ?? throw new ArgumentNullException(nameof(transformAnimator));
        }



        public void KillAnimations(VisualElement element)
        {
            if (element == null) return;

            DG.Tweening.DOTween.Kill(element);
        }

        public void KillAnimations(Transform transform)
        {
            if (transform == null) return;

            DG.Tweening.ShortcutExtensions.DOKill(transform);
        }

        public void KillAllAnimations()
        {
            DG.Tweening.DOTween.KillAll();
        }
    }
}