using Core.UI;
using DG.Tweening;

using Core.UI.Core;
using UnityEngine.UIElements;
using VContainer;
using SkewedBoxElement = Core.UI.Controls.SkewedBoxElement;
using Gameplay.UI.Localization;
using Core.Localization;

namespace Gameplay.UI.Features.Loading
{
    [UIScreen(UIScreenCategory.Overlay, "Screens/LoadingViewTemplate")]
    public class LoadingView : BaseUIScreen
    {
        [Inject] private LoadingViewModel _viewModel;
        [Inject] private ILocalizationManager _localizationManager;

        private SkewedBoxElement _progressFill;
        private Label _statusText;
        private Label _percentageText;
        private Label _tipText;
        private Label _tipTitle;
        private Label _versionInfo;

        private Tween _progressTween;

        protected override void OnInitialize()
        {
            _progressFill = GetElement<SkewedBoxElement>("progress-fill");
            _statusText = GetElement<Label>("status-text");
            _percentageText = GetElement<Label>("percentage");
            _tipText = GetElement<Label>("tip-text");
            _tipTitle = GetElement<Label>("tip-title");
            _versionInfo = GetElement<Label>("version-info");

            TransitionType = UITransitionType.Fade;

            BindViewModel();
            BindLocalization();
        }

        private void BindLocalization()
        {
            if (_localizationManager == null) return;
            // The ViewModel properties (TipTitle, StatusText as defaults) are already localized 
            // but don't react to language change unless we re-initialize or bind them to a key update.
            // Since LoadingScreen might be active when language changes (in theory), we can bind them.
            _localizationManager.RegisterBinding(this, LocKeys.LoadingTipTitle, _ => _viewModel.Initialize());
        }

        private void BindViewModel()
        {
            if (_viewModel == null) return;

            _viewModel.Initialize();

            _viewModel.StatusText.Bind(text => { if (_statusText != null) _statusText.text = text; });
            _viewModel.ProgressText.Bind(text => { if (_percentageText != null) _percentageText.text = text; });
            _viewModel.TipTitle.Bind(text => { if (_tipTitle != null) _tipTitle.text = text; });
            _viewModel.TipText.Bind(text => { if (_tipText != null) _tipText.text = text; });
            _viewModel.VersionText.Bind(text => { if (_versionInfo != null) _versionInfo.text = text; });

            _viewModel.ProgressValue.Bind(AnimateProgress);
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
            }, targetWidth, 0.3f).SetEase(Ease.OutQuad);
        }

        public void UpdateProgress(float progress, string message = null)
        {
            _viewModel?.UpdateProgress(progress, message);
        }

        protected override void OnDispose()
        {
            _progressTween?.Kill();
            _viewModel?.Dispose();
            _localizationManager?.UnregisterAll(this);
        }
    }
}