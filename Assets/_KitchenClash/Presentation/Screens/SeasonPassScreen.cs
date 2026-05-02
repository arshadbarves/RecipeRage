using KitchenClash.Domain;
using KitchenClash.Presentation.Common;
using UnityEngine.UIElements;

namespace KitchenClash.Presentation.Screens
{
    [UIScreen(UIScreenCategory.Screen, "Screens/SeasonPassViewTemplate")]
    public class SeasonPassScreen : BaseUIScreen
    {
        private Button _backButton;
        private Button _buyButton;
        private Label _titleLabel;
        private VisualElement _rewardTrack;
        private VisualElement _progressBar;

        protected override void OnInitialize()
        {
            _backButton = GetElement<Button>("back-button");
            _buyButton = GetElement<Button>("buy-pass-button");
            _titleLabel = GetElement<Label>("season-title");
            _rewardTrack = GetElement<VisualElement>("reward-track");
            _progressBar = GetElement<VisualElement>("progress-fill");

            if (_backButton != null) _backButton.clicked += OnBackClicked;
            if (_buyButton != null) _buyButton.clicked += OnBuyClicked;
        }

        protected override void OnShow()
        {
            if (_titleLabel != null) _titleLabel.text = "SEASON PASS";
        }

        private void OnBackClicked() => UIService?.GoBack();
        private void OnBuyClicked() { /* Purchase flow */ }

        protected override void OnDispose()
        {
            if (_backButton != null) _backButton.clicked -= OnBackClicked;
            if (_buyButton != null) _buyButton.clicked -= OnBuyClicked;
        }
    }
}
