using KitchenClash.Domain;
using KitchenClash.Presentation.Common;
using UnityEngine.UIElements;
using Playcenter.UI;

namespace KitchenClash.Presentation.Screens
{
    /// <summary>
    /// Full-screen 3-2-1-GO overlay. Flow port owns timing; this is pure presentation.
    /// </summary>
    [UIScreen(UIScreenCategory.Overlay, "Screens/CountdownOverlayTemplate")]
    public class CountdownOverlayScreen : BaseUIScreen
    {
        private Label _countdownLabel;
        private Label _subLabel;

        protected override void OnInitialize()
        {
            _countdownLabel = GetElement<Label>("countdown-label");
            _subLabel = GetElement<Label>("countdown-sub");
            TransitionType = UITransitionType.Fade;
        }

        protected override void OnShow()
        {
            base.OnShow();
            SetCount(3);
        }

        public void SetCount(int value)
        {
            if (_countdownLabel == null)
            {
                return;
            }

            _countdownLabel.RemoveFromClassList("go");
            _countdownLabel.text = value.ToString();
            if (_subLabel != null)
            {
                _subLabel.text = string.Empty;
            }
        }

        public void SetGo()
        {
            if (_countdownLabel == null)
            {
                return;
            }

            _countdownLabel.AddToClassList("go");
            _countdownLabel.text = "GO";
            if (_subLabel != null)
            {
                _subLabel.text = "COOK!";
            }
        }
    }
}
