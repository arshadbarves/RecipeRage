using System;
using Core.Animation;
using Core.Logging;
using Cysharp.Threading.Tasks;
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

        private VisualElement _playContainer;
        private VisualElement _centerContainer;
        private Label _playText;
        private Label _centerText;
        private Label _subtitle;

        protected override void OnInitialize()
        {
            // Cache elements
            _playContainer = GetElement<VisualElement>("play-container");
            _centerContainer = GetElement<VisualElement>("center-container");
            _playText = GetElement<Label>("play-text");
            _centerText = GetElement<Label>("center-text");
            _subtitle = GetElement<Label>("subtitle");

            // We override AnimateShow, so TransitionType is less relevant, but setting to Fade as backup
            TransitionType = UITransitionType.Fade;
        }

        public override async void AnimateShow(VisualElement element, float duration, Action onComplete)
        {
            if (_animationService == null)
            {
                base.AnimateShow(element, duration, onComplete);
                return;
            }

            // 1. Initial State (Hidden)
            ResetState();
            
            // Show container
            element.style.display = DisplayStyle.Flex;
            element.style.opacity = 1f;

            // 2. Animate Play Container (Scale In)
            await _animationService.UI.ScaleIn(_playContainer, 0.4f);
            
            // 3. Animate Center Container (Slide/Scale In)
            await _animationService.UI.ScaleIn(_centerContainer, 0.4f);

            // 4. Fade in text
            var textTask1 = _animationService.UI.FadeIn(_playText, 0.3f);
            var textTask2 = _animationService.UI.FadeIn(_centerText, 0.3f);
            var textTask3 = _animationService.UI.FadeIn(_subtitle, 0.5f);

            await UniTask.WhenAll(textTask1, textTask2, textTask3);

            // Wait a bit
            await UniTask.Delay(TimeSpan.FromSeconds(1.0f));

            onComplete?.Invoke();
        }

        private void ResetState()
        {
            if (_playContainer != null) _playContainer.style.scale = new StyleScale(Vector2.zero);
            if (_centerContainer != null) _centerContainer.style.scale = new StyleScale(Vector2.zero);
            if (_playText != null) _playText.style.opacity = 0f;
            if (_centerText != null) _centerText.style.opacity = 0f;
            if (_subtitle != null) _subtitle.style.opacity = 0f;
        }
    }
}
