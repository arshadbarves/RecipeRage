using KitchenClash.Application;
using KitchenClash.Presentation;
using KitchenClash.Domain;
using DG.Tweening;
using KitchenClash.Presentation.Common;
using KitchenClash.Infrastructure.Animation;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using KitchenClash.Application.Services;
using KitchenClash.Presentation.ViewModels;

namespace KitchenClash.Presentation.Screens
{
    [UIScreen(UIScreenCategory.Overlay, "Screens/LoadingViewTemplate")]
    public class LoadingScreen : BaseUIScreen
    {
        [Inject] private LoadingViewModel _viewModel;
        [Inject] private ILocalizationManager _localizationManager;
        [Inject] private IAnimationService _animationService;

        private VisualElement _gameTitle;
        private VisualElement _progressFill;
        private Label _percentageText;
        private Label _tipText;

        private Tween _progressTween;
        private bool _isFirstTip = true;

        protected override void OnInitialize()
        {
            _gameTitle = GetElement<VisualElement>("game-title");
            _progressFill = GetElement<VisualElement>("progress-fill");
            _percentageText = GetElement<Label>("percentage");
            _tipText = GetElement<Label>("tip-text");

            TransitionType = UITransitionType.Fade;

            BindViewModel();
            BindLocalization();
        }

        private void BindLocalization()
        {
            if (_localizationManager == null) return;
            _localizationManager.RegisterBinding(this, LocKeys.LoadingTipTitle, _ => _viewModel.Initialize());
        }

        private void BindViewModel()
        {
            if (_viewModel == null) return;

            _viewModel.Initialize();

            _viewModel.ProgressText.Bind(text => { if (_percentageText != null) _percentageText.text = text; });
            _viewModel.TipText.Bind(OnTipChanged);
            _viewModel.ProgressValue.Bind(AnimateProgress);
        }

        protected override void OnShow()
        {
            base.OnShow();
            _animationService?.FloatYoyo(_gameTitle, -12f, 2.0f);
        }

        private void OnTipChanged(string newTip)
        {
            if (_tipText == null) return;

            if (_isFirstTip)
            {
                _isFirstTip = false;
                _tipText.text = newTip;
                _tipText.style.opacity = 1f;
                return;
            }

            _animationService?.CrossfadeLabel(_tipText, newTip, 15f, 0.8f);
        }

        private void AnimateProgress(float progress)
        {
            if (_progressFill == null) return;

            _progressTween?.Kill();
            float currentWidth = _progressFill.style.width.value.value;
            float targetWidth = progress * 100f;

            _progressTween = DOTween.To(() => currentWidth, x =>
            {
                currentWidth = x;
                _progressFill.style.width = new Length(x, LengthUnit.Percent);
            }, targetWidth, 0.3f).SetEase(Ease.OutQuad).SetTarget(_progressFill);
        }

        public void UpdateProgress(float progress, string message = null)
        {
            _viewModel?.UpdateProgress(progress, message);
        }

        protected override void OnDispose()
        {
            _progressTween?.Kill();
            _animationService?.KillAnimations(_gameTitle);
            _animationService?.KillAnimations(_tipText);
            _viewModel?.Dispose();
            _localizationManager?.UnregisterAll(this);
        }
    }
}
