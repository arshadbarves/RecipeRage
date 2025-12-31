using Core.Animation;
using Core.Bootstrap;
using Core.Logging;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UI.Controls;
using UI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using System;

namespace UI.Screens
{
    /// <summary>
    /// Loading screen - shows initialization progress
    /// </summary>
    [UIScreen(UIScreenType.Loading, UIScreenCategory.Overlay, "Screens/LoadingScreenTemplate")]
    public class LoadingScreen : BaseUIScreen
    {
        [Inject]
        private IAnimationService _animationService;

        private SkewedBoxElement _progressFill;
        private Label _statusText;
        private Label _percentageText;
        private Label _tipText;
        private Label _versionInfo;

        private float _currentProgress;
        private Tween _progressTween;

        private readonly string[] _tips = new[]
        {
            "Chopping onions quickly fills your rage meter!",
            "Work together with your team to plate dishes faster!",
            "Watch out for burning food - timing is everything!",
            "Use power-ups strategically to gain an advantage!",
            "Master the recipes to unlock new challenges!"
        };

        protected override void OnInitialize()
        {
            CacheUIElements();
            SetRandomTip();
            SetVersionInfo();
        }

        protected override void OnShow()
        {
            _progressTween?.Kill();
            _currentProgress = 0f;
            if (_progressFill != null) _progressFill.style.width = new Length(0, LengthUnit.Percent);
            UpdateProgress(0f, "PREHEATING OVEN...");
            SetRandomTip();
        }

        protected override void OnHide()
        {
            _progressTween?.Kill(false);
        }

        protected override void OnDispose()
        {
            _progressTween?.Kill();
        }

        private void CacheUIElements()
        {
            _progressFill = GetElement<SkewedBoxElement>("progress-fill");
            _statusText = GetElement<Label>("status-text");
            _percentageText = GetElement<Label>("percentage");
            _tipText = GetElement<Label>("tip-text");
            _versionInfo = GetElement<Label>("version-info");
        }

        public void UpdateProgress(float progress, string message = null)
        {
            progress = Mathf.Clamp01(progress);
            if (_progressFill != null)
            {
                _progressTween?.Kill();
                if (progress <= 0.01f)
                {
                    _currentProgress = 0f;
                    _progressFill.style.width = new Length(0, LengthUnit.Percent);
                    if (_percentageText != null) _percentageText.text = "0%";
                }
                else
                {
                    _progressTween = DOTween.To(() => _currentProgress, x => {
                        _currentProgress = x;
                        _progressFill.style.width = new Length(x * 100f, LengthUnit.Percent);
                        if (_percentageText != null) _percentageText.text = $"{Mathf.FloorToInt(x * 100f)}%";
                    }, progress, 0.3f).SetEase(Ease.OutQuad);
                }
            }
            if (_statusText != null && !string.IsNullOrEmpty(message)) _statusText.text = message.ToUpper();
        }

        private void SetRandomTip()
        {
            if (_tipText != null && _tips.Length > 0) _tipText.text = _tips[UnityEngine.Random.Range(0, _tips.Length)];
        }

        private void SetVersionInfo()
        {
            if (_versionInfo != null) _versionInfo.text = $"v{Application.version}";
        }
    }
}