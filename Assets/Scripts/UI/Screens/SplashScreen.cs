using System;
using Core.Animation;
using Core.Logging;
using Core.Extensions; // Added
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace UI.Screens
{
    /// <summary>
    /// Splash screen shown at startup.
    /// Features a choreographed logo animation sequence.
    /// </summary>
    [UIScreen(UIScreenType.Splash, UIScreenCategory.System, "Screens/SplashScreenTemplate")]
    public class SplashScreen : BaseUIScreen
    {
        [Inject] private IAnimationService _animationService;

        private VisualElement _masterContainer;
        private VisualElement _playSkewedBorder;
        private VisualElement _centerContainer;
        private Label _playText;
        private Label _centerText;
        private Label _subtitle;

        protected override void OnInitialize()
        {
            // Cache elements
            _masterContainer = GetElement<VisualElement>("master-container");
            _playSkewedBorder = GetElement<VisualElement>("play-skewed-border");
            _centerContainer = GetElement<VisualElement>("center-container");
            _playText = GetElement<Label>("play-text");
            _centerText = GetElement<Label>("center-text");
            _subtitle = GetElement<Label>("subtitle");

            TransitionType = UITransitionType.Fade;
        }

        public override async void AnimateShow(VisualElement element, float duration, Action onComplete)
        {
            if (_animationService == null)
            {
                base.AnimateShow(element, duration, onComplete);
                return;
            }

            // 1. Initial State
            ResetState();
            
            // Show screen
            element.style.display = DisplayStyle.Flex;
            element.style.opacity = 1f;

            // --- Animation Sequence ---
            
            // Global Scale (1.5s)
            var scaleTask = DOTween.To(() => 0.9f, x => _masterContainer.style.scale = new StyleScale(new Vector2(x, x)), 1.0f, 1.5s)
                .SetEase(Ease.OutCubic) // cubic-bezier(0.215, 0.610, 0.355, 1.000) approx
                .ToUniTask();

            // Border Wipe (Width 0 -> 100%) (1s)
            var borderTask = DOTween.To(() => 0f, x => _playSkewedBorder.style.width = new Length(x, LengthUnit.Percent), 100f, 1.0f)
                .SetEase(Ease.OutCubic)
                .ToUniTask();

            // Play Text Fade (1s)
            var playTextTask = _animationService.AnimateOpacity(_playText, 0f, 1f, 1.0f);

            // Center Box Slide & Fade (Starts at 0.2s)
            var centerTask = UniTask.Create(async () => 
            {
                await UniTask.Delay(200);
                var slide = DOTween.To(() => 30f, x => _centerContainer.style.translate = new StyleTranslate(new Translate(x, 0)), 0f, 0.8f)
                    .SetEase(Ease.OutCubic).ToUniTask();
                var fade = _animationService.AnimateOpacity(_centerContainer, 0f, 1f, 1.0f);
                await UniTask.WhenAll(slide, fade);
            });

            // Subtitle Fade (Late)
            var subTask = UniTask.Create(async () =>
            {
                await UniTask.Delay(800);
                await _animationService.AnimateOpacity(_subtitle, 0f, 1f, 0.5f);
            });

            await UniTask.WhenAll(scaleTask, borderTask, playTextTask, centerTask, subTask);

            // Hold
            await UniTask.Delay(1000);

            onComplete?.Invoke();
        }

        private void ResetState()
        {
            if (_masterContainer != null) _masterContainer.style.scale = new StyleScale(new Vector2(0.9f, 0.9f));
            if (_playSkewedBorder != null) _playSkewedBorder.style.width = new Length(0, LengthUnit.Percent);
            if (_playText != null) _playText.style.opacity = 0f;
            
            if (_centerContainer != null) 
            {
                _centerContainer.style.opacity = 0f;
                _centerContainer.style.translate = new StyleTranslate(new Translate(30, 0));
            }
            
            if (_subtitle != null) _subtitle.style.opacity = 0f;
        }
    }
}
