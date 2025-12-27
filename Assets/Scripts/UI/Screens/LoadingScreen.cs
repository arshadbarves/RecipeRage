using System;
using Core.Bootstrap;
using Core.Logging;
using Core.State.States;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UI.Controls;
using UI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Screens
{
    /// <summary>
    /// Loading screen - shows initialization progress
    /// </summary>
    [UIScreen(UIScreenType.Loading, UIScreenCategory.Overlay, "Screens/LoadingScreenTemplate")]
    public class LoadingScreen : BaseUIScreen
    {
        #region UI Elements

        private SkewedBoxElement _progressFill;
        private Label _statusText;
        private Label _percentageText;
        private Label _tipText;
        private Label _versionInfo;

        #endregion

        #region State

        private float _currentProgress;
        private Tween _progressTween;


        private readonly string[] _tips = new[]
        {
            "Chopping onions quickly fills your rage meter. Use it to unleash a combo!",
            "Work together with your team to plate dishes faster!",
            "Watch out for burning food - timing is everything!",
            "Use power-ups strategically to gain an advantage!",
            "Master the recipes to unlock new challenges!",
            "Communication is key in team battles!",
            "Speed and accuracy both matter - find your balance!",
            "Unlock new chefs with unique abilities!"
        };

        #endregion

        #region Lifecycle

        protected override void OnInitialize()
        {
            CacheUIElements();

            SetRandomTip();
            SetVersionInfo();
            ApplyProceduralGraphics();
        }

        protected override void OnShow()
        {
            // Explicitly kill any existing tween to stop animation
            _progressTween?.Kill();

            // Reset tracking and visual state immediately for a fresh show
            _currentProgress = 0f;
            if (_progressFill != null)
            {
                _progressFill.style.width = new Length(0, LengthUnit.Percent);
                if (_percentageText != null) _percentageText.text = "0%";
            }

            UpdateProgress(0f, "PREHEATING OVEN...");
            SetRandomTip();
        }

        protected override void OnHide()
        {
            // Stop the tween where it is; do NOT complete it (false)
            // This ensures the visual bar stays at its current width (e.g. 100%) during fade out
            _progressTween?.Kill(false);

            if (Container != null)
            {
                var animationService = GameBootstrap.Services?.AnimationService;
                if (animationService != null)
                {
                    animationService.UI.FadeOut(Container, 0.3f);
                }
            }
        }

        protected override void OnDispose()
        {
            _progressTween?.Kill();
        }

        #endregion

        #region UI Setup

        private void CacheUIElements()
        {
            _progressFill = GetElement<SkewedBoxElement>("progress-fill");
            _statusText = GetElement<Label>("status-text");
            _percentageText = GetElement<Label>("percentage");
            _tipText = GetElement<Label>("tip-text");
            _versionInfo = GetElement<Label>("version-info");

            if (_progressFill == null) GameLogger.LogWarning("progress-fill not found");
            if (_statusText == null) GameLogger.LogWarning("status-text not found");
            if (_percentageText == null) GameLogger.LogWarning("percentage not found");
            if (_tipText == null) GameLogger.LogWarning("tip-text not found");
        }

        private void ApplyProceduralGraphics()
        {
            // Background: Radial Gradient #1a1a1a -> #000000
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
            width = Mathf.Max(width, 2);
            height = Mathf.Max(height, 2);

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

        private void SetRandomTip()
        {
            if (_tipText != null && _tips.Length > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, _tips.Length);
                _tipText.text = _tips[randomIndex];
            }
        }

        private void SetVersionInfo()
        {
            if (_versionInfo != null)
            {
                _versionInfo.text = $"v{Application.version} ({Application.buildGUID.Substring(0, 8)})";
            }
        }

        #endregion

        #region Public API

        public void UpdateProgress(float progress, string message = null)
        {
            progress = Mathf.Clamp01(progress);

            // Animate Bar Width
            if (_progressFill != null)
            {
                _progressTween?.Kill();

                // If starting from scratch (0), set immediately to avoid "tweening down" artifacts
                if (progress <= 0.01f)
                {
                    _currentProgress = 0f;
                    _progressFill.style.width = new Length(0, LengthUnit.Percent);
                    if (_percentageText != null) _percentageText.text = "0%";
                }
                else
                {
                    _progressTween = DOTween.To(
                        () => _currentProgress,
                        x =>
                        {
                            _currentProgress = x;
                            _progressFill.style.width = new Length(x * 100f, LengthUnit.Percent);

                            // Update percentage text
                            if (_percentageText != null)
                            {
                                _percentageText.text = $"{Mathf.FloorToInt(x * 100f)}%";
                            }
                        },
                        progress,
                        0.3f
                    ).SetEase(Ease.OutQuad);
                }
            }

            // Update Status Message (UPPERCASE for style)
            if (_statusText != null && !string.IsNullOrEmpty(message))
            {
                _statusText.text = message.ToUpper();
            }
        }

        /// <summary>
        /// Show completion and fade out
        /// </summary>
        public async UniTask CompleteAsync()
        {
            UpdateProgress(1f, "Complete");
            await UniTask.Delay(500);
            // Hiding is now handled by the State or Service
        }

        #endregion
    }
}
