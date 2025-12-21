using Core.Animation;
using Core.Bootstrap;
using Core.Logging;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UI.Controls;
using UI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Screens
{
    /// <summary>
    /// Splash screen - quick brand display on startup.
    /// Matches the HTML/CSS reference design exactly.
    /// </summary>
    [UIScreen(UIScreenType.Splash, UIScreenCategory.System, "Screens/SplashScreenTemplate")]
    public class SplashScreen : BaseUIScreen
    {
        #region Constants - Animation timings

        // Animation Durations (in seconds) - Slower for elegance
        private const float GLOBAL_SCALE_DURATION = 2.0f;
        private const float BORDER_WIPE_DURATION = 1.5f;
        private const float TEXT_FADE_DURATION = 1.5f;
        private const float BOX_FADE_DURATION = 1.5f;
        private const float BOX_SLIDE_DURATION = 1.2f;
        private const float SUBTITLE_REVEAL_DURATION = 1.5f;
        private const float FADE_OUT_DURATION = 0.8f;

        // Animation Delays (in milliseconds)
        private const int CENTER_BOX_DELAY_MS = 300;
        private const int SUBTITLE_DELAY_MS = 800;

        // Animation Values
        private const float INITIAL_SCALE = 0.9f;
        private const float CENTER_BOX_SLIDE_OFFSET = 30f;
        private const float SUBTITLE_SLIDE_OFFSET = 20f;

        #endregion

        #region UI Elements

        private VisualElement _splashContent;
        private VisualElement _masterContainer;
        private VisualElement _playContainer;
        private SkewedBoxElement _playSkewedBorder;
        private VisualElement _playText;
        private VisualElement _centerContainer;
        private SkewedBoxElement _centerSkewedBox;
        private VisualElement _subtitle;

        #endregion

        #region Lifecycle

        protected override void OnInitialize()
        {
            CacheUIElements();
            GameLogger.Log("Initialized");

            // Generate and apply procedural textures
            ApplyProceduralGraphics();
        }

        protected override void OnShow()
        {
            var anim = GameBootstrap.Services?.AnimationService;
            if (anim == null) return;

            // 1. Global Scale
            if (_masterContainer != null)
            {
                _masterContainer.style.scale = new StyleScale(new Vector2(INITIAL_SCALE, INITIAL_SCALE));
                
                DOTween.To(
                    () => INITIAL_SCALE,
                    x => _masterContainer.style.scale = new StyleScale(new Vector2(x, x)),
                    1f,
                    GLOBAL_SCALE_DURATION
                ).SetEase(Ease.OutCubic);
            }

            // 2. Play Text Fade
            if (_playText != null)
            {
                _playText.style.opacity = 0;
                
                DOTween.To(
                    () => 0f,
                    x => _playText.style.opacity = x,
                    1f,
                    TEXT_FADE_DURATION
                ).SetEase(Ease.OutQuad);
            }

            // 3. Play Border Wipe
            if (_playSkewedBorder != null)
            {
                _playSkewedBorder.BorderProgress = 0f;

                DOTween.To(
                    () => _playSkewedBorder.BorderProgress,
                    x => _playSkewedBorder.BorderProgress = x,
                    1f,
                    BORDER_WIPE_DURATION
                ).SetEase(Ease.OutExpo);
            }

            // 4. Center Box Fade & Slide
            if (_centerContainer != null)
            {
                _centerContainer.style.opacity = 0;
                _centerContainer.style.translate = new Translate(CENTER_BOX_SLIDE_OFFSET, 0);

                UniTask.Delay(CENTER_BOX_DELAY_MS).ContinueWith(() =>
                {
                    // Fade in
                    DOTween.To(
                        () => 0f,
                        x => _centerContainer.style.opacity = x,
                        1f,
                        BOX_FADE_DURATION
                    ).SetEase(Ease.OutQuad);

                    // Slide from right
                    DOTween.To(
                        () => CENTER_BOX_SLIDE_OFFSET,
                        x => _centerContainer.style.translate = new Translate(x, 0),
                        0f,
                        BOX_SLIDE_DURATION
                    ).SetEase(Ease.OutCubic);

                }).Forget();
            }

            // 5. Subtitle Reveal
            if (_subtitle != null)
            {
                _subtitle.style.opacity = 0;
                _subtitle.style.translate = new Translate(0, SUBTITLE_SLIDE_OFFSET);

                UniTask.Delay(SUBTITLE_DELAY_MS).ContinueWith(() =>
                {
                    // Fade in
                    DOTween.To(
                        () => 0f,
                        x => _subtitle.style.opacity = x,
                        1f,
                        SUBTITLE_REVEAL_DURATION
                    ).SetEase(Ease.OutQuad);

                    // Slide up
                    DOTween.To(
                        () => SUBTITLE_SLIDE_OFFSET,
                        x => _subtitle.style.translate = new Translate(0, x),
                        0f,
                        SUBTITLE_REVEAL_DURATION
                    ).SetEase(Ease.OutQuad);

                }).Forget();
            }
        }

        protected override void OnHide()
        {
            // Fade out - longer duration for elegant exit
            if (Container != null)
            {
                var animationService = GameBootstrap.Services?.AnimationService;
                if (animationService != null)
                {
                    animationService.UI.FadeOut(Container, FADE_OUT_DURATION);
                }
            }
        }

        #endregion

        #region UI Setup

        private void CacheUIElements()
        {
            _splashContent = GetElement<VisualElement>("splash-content");
            _masterContainer = GetElement<VisualElement>("master-container");
            _playContainer = GetElement<VisualElement>("play-container");
            _playSkewedBorder = GetElement<SkewedBoxElement>("play-skewed-border");
            _playText = GetElement<VisualElement>("play-text");
            _centerContainer = GetElement<VisualElement>("center-container");
            _centerSkewedBox = GetElement<SkewedBoxElement>("center-skewed-box");
            _subtitle = GetElement<VisualElement>("subtitle");

            if (_splashContent == null) GameLogger.LogWarning("splash-content not found");
            if (_playSkewedBorder == null) GameLogger.LogWarning("play-skewed-border not found");
            if (_centerSkewedBox == null) GameLogger.LogWarning("center-skewed-box not found");

            // Initial States
            if (_splashContent != null) _splashContent.style.opacity = 1;
            if (_masterContainer != null) _masterContainer.style.scale = new StyleScale(new Vector2(INITIAL_SCALE, INITIAL_SCALE));
            if (_playText != null) _playText.style.opacity = 0;
            if (_playSkewedBorder != null) _playSkewedBorder.BorderProgress = 0f;
            if (_centerContainer != null) _centerContainer.style.opacity = 0;
            if (_subtitle != null) _subtitle.style.opacity = 0;
        }

        private void ApplyProceduralGraphics()
        {
            // Background: Radial Gradient #1a1a1a -> #000000
            // HTML: radial-gradient(circle at center, #1a1a1a 0%, #000000 100%)
            if (Container != null)
            {
                var bgTex = GenerateRadialGradient(Screen.width, Screen.height,
                    new Color32(26, 26, 26, 255), // #1a1a1a
                    new Color32(0, 0, 0, 255));   // #000000
                Container.style.backgroundImage = new StyleBackground(bgTex);
            }
        }

        private Texture2D GenerateRadialGradient(int width, int height, Color centerColor, Color edgeColor)
        {
            Texture2D tex = new Texture2D(width, height);
            Color[] pixels = new Color[width * height];
            Vector2 center = new Vector2(width * 0.5f, height * 0.5f);
            float maxDist = Mathf.Sqrt(width * width + height * height) * 0.5f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float t = Mathf.Clamp01(dist / maxDist);
                    pixels[y * width + x] = Color.Lerp(centerColor, edgeColor, t);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Show splash screen
        /// </summary>
        public void Show()
        {
            base.Show(false);
        }

        #endregion
    }
}
