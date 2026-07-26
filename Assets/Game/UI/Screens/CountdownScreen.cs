using Playcenter;
using Playcenter.UI;
using UnityEngine.UIElements;

namespace RecipeRage.UI
{
    /// <summary>
    /// Big centered 3-2-1. Ticks are driven by CountdownState (audio) — this
    /// screen mirrors the count visually with scale-up/fade per number.
    /// </summary>
    [UIScreen]
    public sealed class CountdownScreen : BaseUIScreen
    {
        private Label _number;
        private int _lastShown = -1;
        private float _remaining = 3f;

        protected override void OnShow()
        {
            _number = Root.Q<Label>("countdown-number");
            _lastShown = -1;
            _remaining = 3f;
        }

        private void Update()
        {
            _remaining -= UnityEngine.Time.deltaTime;
            var whole = (int)_remaining + 1;
            if (whole != _lastShown && whole >= 1)
            {
                _lastShown = whole;
                _number.text = whole.ToString();
                UIAnimation.ScaleBounce(_number, 0.3f);
            }
            else if (_remaining <= 0f && _lastShown != 0)
            {
                _lastShown = 0;
                _number.text = "Cook!";
                UIAnimation.ScaleBounce(_number, 0.3f);
            }
        }
    }
}
