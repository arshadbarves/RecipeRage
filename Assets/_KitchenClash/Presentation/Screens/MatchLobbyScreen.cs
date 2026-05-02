using KitchenClash.Domain;
using KitchenClash.Presentation.Common;
using UnityEngine.UIElements;

namespace KitchenClash.Presentation.Screens
{
    [UIScreen(UIScreenCategory.Screen, "Screens/MatchLobbyViewTemplate")]
    public class MatchLobbyScreen : BaseUIScreen
    {
        private Label _timerLabel;
        private Button _readyButton;

        protected override void OnInitialize()
        {
            _timerLabel = GetElement<Label>("lbl-lobby-timer");
            _readyButton = GetElement<Button>("btn-ready");

            if (_readyButton != null) _readyButton.clicked += OnReadyClicked;
        }

        protected override void OnShow()
        {
            if (_timerLabel != null) _timerLabel.text = "Waiting for players...";
        }

        private void OnReadyClicked()
        {
            if (_readyButton != null)
            {
                _readyButton.SetEnabled(false);
                _readyButton.text = "READY ✓";
            }
        }

        protected override void OnDispose()
        {
            if (_readyButton != null) _readyButton.clicked -= OnReadyClicked;
        }
    }
}
