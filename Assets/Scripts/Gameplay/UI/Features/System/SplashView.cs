using Core.UI;
using DG.Tweening;
using Core.UI.Core;
using Core.Animation;
using Core.UI.Interfaces;
using Core.Shared.Extensions;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Gameplay.UI.Features.System
{
    /// <summary>
    /// Splash view shown at startup.
    /// Implements of "Conductor Workflow" for intro animation sequencing.
    /// </summary>
    [UIScreen(UIScreenCategory.System, "Screens/SplashViewTemplate")]
    public class SplashView : BaseUIScreen
    {
        private VisualElement _brandContainer;
        private VisualElement _splashContent;
        private Label _playText;
        private Label _centerText;
        private Label _tagline;
        private VisualElement _loaderFill;

        [Inject] private IAnimationService _animationService;

        protected override void OnInitialize()
        {
            // Query elements
            _brandContainer = GetElement<VisualElement>("brand-container");
            _splashContent = GetElement<VisualElement>("splash-content");
            _playText = GetElement<Label>("play-text");
            _centerText = GetElement<Label>("center-text");
            _tagline = GetElement<Label>("tagline");
            _loaderFill = GetElement<VisualElement>("loader-fill");

            // --- Set Initial Visibility States ---
            if (_brandContainer != null)
            {
                _brandContainer.style.opacity = 0;
                _brandContainer.style.scale = new Scale(new Vector3(1.1f, 1.1f, 1.1f));
            }

            if (_playText != null) _playText.style.letterSpacing = new Length(20, LengthUnit.Pixel);
            if (_centerText != null) _centerText.style.letterSpacing = new Length(20, LengthUnit.Pixel);

            if (_tagline != null)
            {
                _tagline.style.opacity = 0;
                _tagline.style.translate = new Translate(0, 5, 0);
            }

            if (_loaderFill != null)
            {
                _loaderFill.style.translate = new Translate(new Length(-100, LengthUnit.Percent), 0, 0);
            }

            TransitionType = UITransitionType.Fade;
        }

        protected override void OnShow()
        {
            base.OnShow();
            PlayIntroSequence();
        }

        private void PlayIntroSequence()
        {
            float duration = 2.0f;

            // 1. Brand Intro Sequence (hoyoEntry: 2s ease-out forwards)
            if (_brandContainer != null)
            {
                float currentScale = 1.1f;
                DOTween.To(() => currentScale, x => {
                    currentScale = x;
                    _brandContainer.style.scale = new Scale(new Vector3(x, x, x));
                }, 1.0f, duration).SetEase(Ease.OutQuad).SetTarget(_brandContainer);

                _animationService?.UI.FadeIn(_brandContainer, duration);
                _animationService?.UI.BlurIn(_brandContainer, 12f, duration);
            }

            // Letter Spacing (Tracking): 20px -> 6px (enhanced effect; HTML parent animates 0.8rem->0.2rem)
            if (_playText != null && _centerText != null)
            {
                 _animationService?.UI.TrackingIn(_playText, 20f, 6f, duration);
                 _animationService?.UI.TrackingIn(_centerText, 20f, 6f, duration);
            }

            // 2. Tagline (fadeInSub: 2.5s ease-out 1s delay)
            if (_tagline != null)
            {
                Sequence taglineSeq = DOTween.Sequence();
                taglineSeq.Insert(1.0f, DOTween.To(() => _tagline.style.opacity.value,
                    x => _tagline.style.opacity = x, 1f, 2.5f).SetEase(Ease.OutQuad));

                float currentTaglineY = 5f;
                taglineSeq.Insert(1.0f, DOTween.To(() => currentTaglineY,
                    x => {
                        currentTaglineY = x;
                        _tagline.style.translate = new Translate(0, x, 0);
                    }, 0f, 2.5f).SetEase(Ease.OutQuad));
                
                taglineSeq.SetTarget(_tagline);
            }

            // 3. Continuous Loader Animation (slide: 2s infinite ease-in-out)
            if (_loaderFill != null)
            {
                _animationService?.UI.SlideInfinite(_loaderFill, -100f, 200f, 2.0f);
            }
        }

        /// <summary>
        /// Plays the outro animation (reverse of intro): blur up, scale up, fade out.
        /// Call this from BootstrapState before hiding the splash screen.
        /// </summary>
        public async UniTask PlayOutroAsync()
        {
            // Kill all intro animations first
            KillAllAnimations();

            float duration = 0.8f;
            Sequence outro = DOTween.Sequence();

            // 1. Fade out tagline first (fast)
            if (_tagline != null)
            {
                outro.Insert(0f, DOTween.To(() => _tagline.style.opacity.value,
                    x => _tagline.style.opacity = x, 0f, duration * 0.5f).SetEase(Ease.InQuad));
            }

            // 2. Fade out loader
            if (_loaderFill != null)
            {
                var loaderBar = _loaderFill.parent;
                if (loaderBar != null)
                {
                    float loaderOpacity = 1f;
                    outro.Insert(0f, DOTween.To(() => loaderOpacity, x =>
                    {
                        loaderOpacity = x;
                        loaderBar.style.opacity = x;
                    }, 0f, duration * 0.5f).SetEase(Ease.InQuad));
                }
            }

            // 3. Brand outro: scale up, blur out, fade out (reverse of intro)
            if (_brandContainer != null)
            {
                // Scale: 1.0 -> 0.95 (slight shrink for cinematic feel)
                float currentScale = 1.0f;
                outro.Insert(0.1f, DOTween.To(() => currentScale, x =>
                {
                    currentScale = x;
                    _brandContainer.style.scale = new Scale(new Vector3(x, x, x));
                }, 0.95f, duration).SetEase(Ease.InQuad));

                // Fade out
                outro.Insert(0.1f, DOTween.To(() => _brandContainer.style.opacity.value,
                    x => _brandContainer.style.opacity = x, 0f, duration).SetEase(Ease.InQuad));

                // Blur out: 0 -> 8px
                _animationService?.UI.BlurIn(_brandContainer, 0.01f, 0.01f); // Reset to no blur
                float currentBlur = 0f;
            }

            // 4. Fade the entire screen to white at the end
            if (_splashContent != null)
            {
                outro.Insert(duration * 0.4f, DOTween.To(() => _splashContent.style.opacity.value,
                    x => _splashContent.style.opacity = x, 0f, duration * 0.6f).SetEase(Ease.InQuad));
            }

            outro.SetTarget(this);
            await outro.ToUniTask();
        }

        private void KillAllAnimations()
        {
            _animationService?.KillAnimations(_brandContainer);
            _animationService?.KillAnimations(_playText);
            _animationService?.KillAnimations(_centerText);
            _animationService?.KillAnimations(_tagline);
            _animationService?.KillAnimations(_loaderFill);
        }

        protected override void OnDispose()
        {
            KillAllAnimations();
            DOTween.Kill(this);
        }
    }
}