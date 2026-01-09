using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.Core.Animation
{
    /// <summary>
    /// Main animation service - facade for UI and Transform animators
    /// Follows Facade Pattern and Dependency Inversion Principle
    /// Strictly delegates logic to specialized sub-animators
    /// </summary>
    public class AnimationService : IAnimationService
    {
        private readonly IUIAnimator _uiAnimator;
        private readonly ITransformAnimator _transformAnimator;

        public AnimationService(IUIAnimator uiAnimator, ITransformAnimator transformAnimator)
        {
            _uiAnimator = uiAnimator ?? throw new ArgumentNullException(nameof(uiAnimator));
            _transformAnimator = transformAnimator ?? throw new ArgumentNullException(nameof(transformAnimator));
        }

        public void Initialize()
        {
        }

        // Kill animations
        public void KillAnimations(VisualElement element)
        {
            if (element == null)
            {
                return;
            }

            DG.Tweening.DOTween.Kill(element);
        }

        public void KillAnimations(Transform transform)
        {
            if (transform == null)
            {
                return;
            }

            DG.Tweening.ShortcutExtensions.DOKill(transform);
        }

        public void KillAllAnimations()
        {
            DG.Tweening.DOTween.KillAll();
        }

        public IUIAnimator UI => _uiAnimator;
        public ITransformAnimator Transform => _transformAnimator;
    }
}