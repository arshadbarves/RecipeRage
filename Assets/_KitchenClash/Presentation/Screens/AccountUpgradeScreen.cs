using KitchenClash.Domain;
using KitchenClash.Presentation.Common;
using KitchenClash.Presentation.ViewModels;
using UnityEngine.UIElements;
using VContainer;

namespace KitchenClash.Presentation.Screens
{
    /// <summary>
    /// Shown once after tutorial completion.
    ///
    /// Layout (bottom-sheet slide-up over a blurred lobby background):
    ///   ┌─────────────────────────────────────┐
    ///   │  🏆 You earned 0 trophies!           │   ← social proof
    ///   │  Save your progress                  │
    ///   │                                      │
    ///   │  [ G  Continue with Google  ]        │
    ///   │  [ f  Continue with Facebook]        │
    ///   │  [ 🍎 Continue with Apple   ]  (iOS) │
    ///   │                                      │
    ///   │  ─── or ───                          │
    ///   │  Continue as Guest (data may be lost)│
    ///   └─────────────────────────────────────┘
    ///
    /// UXML template: Screens/AccountUpgradeViewTemplate
    /// </summary>
    [UIScreen(UIScreenCategory.Modal, "Screens/AccountUpgradeViewTemplate")]
    public class AccountUpgradeScreen : BaseUIScreen
    {
        [Inject] private AccountUpgradeViewModel _viewModel;

        // ── Element refs ──────────────────────────────────────────────────
        private Label  _trophyLabel;
        private Button _googleButton;
        private Button _facebookButton;
        private Button _appleButton;
        private VisualElement _appleRow;
        private Button _guestButton;
        private Label  _statusLabel;
        private VisualElement _spinnerOverlay;

        protected override void OnInitialize()
        {
            TransitionType = UITransitionType.SlideUp;

            _trophyLabel    = GetElement<Label>("trophy-count");
            _googleButton   = GetElement<Button>("btn-google");
            _facebookButton = GetElement<Button>("btn-facebook");
            _appleButton    = GetElement<Button>("btn-apple");
            _appleRow       = GetElement<VisualElement>("apple-row");
            _guestButton    = GetElement<Button>("btn-guest");
            _statusLabel    = GetElement<Label>("status-text");
            _spinnerOverlay = GetElement<VisualElement>("spinner-overlay");

            _googleButton?.RegisterCallback<ClickEvent>(_   => _viewModel?.LinkWithGoogle());
            _facebookButton?.RegisterCallback<ClickEvent>(_ => _viewModel?.LinkWithFacebook());
            _appleButton?.RegisterCallback<ClickEvent>(_    => _viewModel?.LinkWithApple());
            _guestButton?.RegisterCallback<ClickEvent>(_    => _viewModel?.ContinueAsGuest());

            BindViewModel();
        }

        private void BindViewModel()
        {
            if (_viewModel == null) return;

            _viewModel.Initialize();

            _viewModel.TrophyCount.Bind(t =>
            {
                if (_trophyLabel != null)
                    _trophyLabel.text = t > 0 ? $"🏆 {t} trophies earned" : "Start your journey!";
            });

            _viewModel.StatusText.Bind(s =>
            {
                if (_statusLabel != null) _statusLabel.text = s;
            });

            _viewModel.IsLinking.Bind(loading =>
            {
                if (_spinnerOverlay != null)
                    _spinnerOverlay.style.display = loading ? DisplayStyle.Flex : DisplayStyle.None;
                _googleButton?.SetEnabled(!loading);
                _facebookButton?.SetEnabled(!loading);
                _appleButton?.SetEnabled(!loading);
                _guestButton?.SetEnabled(!loading);
            });

            _viewModel.AppleVisible.Bind(visible =>
            {
                if (_appleRow != null)
                    _appleRow.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            });
        }

        protected override void OnDispose()
        {
            _viewModel?.Dispose();
        }
    }
}
