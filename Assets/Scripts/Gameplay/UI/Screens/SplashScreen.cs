using Core.UI;
using DG.Tweening;
using Core.UI.Core;
using Core.UI.Interfaces;
using UnityEngine;
using UnityEngine.UIElements;
using SkewedBoxElement = Core.UI.Controls.SkewedBoxElement;

namespace Gameplay.UI.Screens
{
    /// <summary>
    /// Splash screen shown at startup.
    /// Implements the "Conductor Workflow" for intro animation sequencing.
    /// </summary>
    [UIScreen(UIScreenType.Splash, UIScreenCategory.System, "Screens/SplashScreenTemplate")]
    public class SplashScreen : BaseUIScreen
    {


        private VisualElement _masterContainer;
        private VisualElement _playContainer;
        private SkewedBoxElement _playBorder;
        private Label _playText;
        private VisualElement _centerContainer;
        private Label _centerText;
        private Label _studioText;

        protected override void OnInitialize()
        {
            // Query elements
            _masterContainer = GetElement<VisualElement>("master-container");
            _playContainer = GetElement<VisualElement>("play-container");
            _playBorder = GetElement<SkewedBoxElement>("play-skewed-border");
            _playText = GetElement<Label>("play-text");
            _centerContainer = GetElement<VisualElement>("center-container");
            _centerText = GetElement<Label>("center-text");
            _studioText = GetElement<Label>("subtitle");

            // --- Set Initial Visibility States ---
            if (_masterContainer != null) _masterContainer.style.scale = new Scale(Vector3.one * 0.9f);

            if (_playContainer != null) _playContainer.style.opacity = 0;
            if (_playText != null) _playText.style.opacity = 0;
            if (_playBorder != null) _playBorder.BorderProgress = 0f;

            if (_centerContainer != null)
            {
                _centerContainer.style.opacity = 0;
                _centerContainer.style.translate = new Translate(50, 0, 0);
            }
            if (_centerText != null) _centerText.style.opacity = 0;
            if (_studioText != null) _studioText.style.opacity = 0;

            TransitionType = UITransitionType.Fade;
        }

        protected override void OnShow()
        {
            base.OnShow();
            PlayIntroSequence();
        }

        private void PlayIntroSequence()
        {
            if (_masterContainer == null) return;

            // --- 1. Prepare for Play ---
            if (_playContainer != null) _playContainer.style.opacity = 1f;

            // --- 2. Sequence (The Conductor) ---
            Sequence conductor = DOTween.Sequence();

            // A. Global Scale: 0.9 -> 1.0 (2.0s, Cubic Out)
            float currentScale = 0.9f;
            conductor.Append(DOTween.To(() => currentScale, x =>
            {
                currentScale = x;
                _masterContainer.style.scale = new Scale(Vector3.one * currentScale);
            }, 1.0f, 2.0f).SetEase(Ease.OutCubic));

            // B. Border Wipe: 0 -> 1 (1.2s, EaseOutCubic) - Starts with delay
            if (_playBorder != null)
            {
                conductor.Insert(0.2f, DOTween.To(() => _playBorder.BorderProgress, x => _playBorder.BorderProgress = x, 1f, 1.2f)
                    .SetEase(Ease.OutCubic));
            }

            // C. Play Text Fade: 0 -> 1 (1.0s) - Starts after wiping starts
            if (_playText != null)
            {
                 conductor.Insert(0.4f, DOTween.To(() => _playText.style.opacity.value,
                     x => _playText.style.opacity = x, 1f, 1.0f).SetEase(Ease.OutQuad));
            }

            // D. Center Box Fade + Slide - Starts at 0.6s
            float slideStartTime = 0.6f;

            if (_centerContainer != null)
            {
                // Fade In (1.0s)
                conductor.Insert(slideStartTime, DOTween.To(() => _centerContainer.style.opacity.value,
                    x => _centerContainer.style.opacity = x, 1f, 1.0f).SetEase(Ease.OutQuad));

                // Slide X: 50 -> 0 (1.0s)
                float currentX = 50f;
                conductor.Insert(slideStartTime, DOTween.To(() => currentX, x =>
                {
                    currentX = x;
                    _centerContainer.style.translate = new Translate(currentX, 0, 0);
                }, 0f, 1.0f).SetEase(Ease.OutCubic));
            }

            // E. Center Text Fade
             if (_centerText != null)
            {
                 conductor.Insert(slideStartTime + 0.15f, DOTween.To(() => _centerText.style.opacity.value,
                     x => _centerText.style.opacity = x, 1f, 0.8f).SetEase(Ease.OutQuad));
            }

            // F. Studio Text Fade - Starts late
            if (_studioText != null)
            {
                conductor.Insert(slideStartTime + 0.6f, DOTween.To(() => _studioText.style.opacity.value,
                    x => _studioText.style.opacity = x, 1f, 0.8f).SetEase(Ease.OutQuad));
            }
        }

        protected override void OnDispose()
        {
        }
    }
}