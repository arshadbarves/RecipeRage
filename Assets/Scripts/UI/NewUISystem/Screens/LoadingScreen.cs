using System;
using Core.Bootstrap;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UI.UISystem.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.UISystem.Screens
{
    /// <summary>
    /// Loading screen - shows initialization progress
    /// </summary>
    [UIScreen(UIScreenType.Loading, UIScreenPriority.Loading, "LoadingScreenTemplate")]
    public class LoadingScreen : BaseUIScreen
    {
        #region UI Elements

        private ProgressBar _loadingProgress;
        private Label _progressText;
        private Label _loadingTip;
        private Label _tipHeader;
        private Label _versionInfo;

        #endregion

        #region State

        private float _currentProgress;
        private Tween _progressTween;

        private readonly string[] _tips = new[]
        {
            "Combine ingredients to create special recipes!",
            "Work together with your team for bonus points!",
            "Watch out for burning food - timing is everything!",
            "Use power-ups strategically to gain an advantage!",
            "Master the recipes to unlock new challenges!",
            "Communication is key in team battles!",
            "Speed and accuracy both matter - find your balance!",
            "Unlock new characters with unique abilities!"
        };

        #endregion

        #region Lifecycle

        protected override void OnInitialize()
        {
            CacheUIElements();
            SetRandomTip();
            SetVersionInfo();
            Debug.Log("[LoadingScreen] Initialized");
        }

        protected override void OnShow()
        {
            _currentProgress = 0f;
            UpdateProgress(0f, "Loading");
            SetRandomTip();
        }

        protected override void OnHide()
        {
            // Kill any ongoing animations
            _progressTween?.Kill();
            
            // Fade out
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
            _loadingProgress = GetElement<ProgressBar>("loading-progress");
            _progressText = GetElement<Label>("progress-text");
            _loadingTip = GetElement<Label>("loading-tip");
            _tipHeader = GetElement<Label>("tip-header");
            _versionInfo = GetElement<Label>("version-info");

            if (_loadingProgress == null)
            {
                Debug.LogWarning("[LoadingScreen] loading-progress not found");
            }
            if (_progressText == null)
            {
                Debug.LogWarning("[LoadingScreen] progress-text not found");
            }
            if (_loadingTip == null)
            {
                Debug.LogWarning("[LoadingScreen] loading-tip not found");
            }
        }

        private void SetRandomTip()
        {
            if (_loadingTip != null && _tips.Length > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, _tips.Length);
                _loadingTip.text = _tips[randomIndex];
            }
        }

        private void SetVersionInfo()
        {
            if (_versionInfo != null)
            {
                _versionInfo.text = $"v{Application.version} (Build {Application.buildGUID.Substring(0, 8)})";
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Update loading progress
        /// </summary>
        /// <param name="progress">Progress value 0-1</param>
        /// <param name="message">Status message</param>
        public void UpdateProgress(float progress, string message = null)
        {
            progress = Mathf.Clamp01(progress);
            
            // Animate progress bar
            if (_loadingProgress != null)
            {
                _progressTween?.Kill();
                _progressTween = DOTween.To(
                    () => _currentProgress,
                    x =>
                    {
                        _currentProgress = x;
                        _loadingProgress.value = x * 100f;
                    },
                    progress,
                    0.3f
                ).SetEase(Ease.OutQuad);
            }
            
            // Update status message
            if (_progressText != null && !string.IsNullOrEmpty(message))
            {
                _progressText.text = message;
            }
        }

        /// <summary>
        /// Show completion and fade out
        /// </summary>
        public async UniTask CompleteAsync()
        {
            UpdateProgress(1f, "Complete");
            await UniTask.Delay(500);
            Hide(true);
            await UniTask.Delay(300);
        }

        #endregion
    }
}
