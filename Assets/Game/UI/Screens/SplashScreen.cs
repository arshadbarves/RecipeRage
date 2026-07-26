using Playcenter.UI;
using UnityEngine.UIElements;

namespace RecipeRage.UI
{
    /// <summary>
    /// Splash screen shown during SDK boot. Animated logo + loading pulse.
    /// Hidden automatically when MainMenuState shows the next screen.
    /// </summary>
    [UIScreen]
    public sealed class SplashScreen : BaseUIScreen
    {
        protected override void OnShow()
        {
            UIAnimation.ScaleBounce(Root.Q<VisualElement>("logo"));
            UIAnimation.ScalePulse(Root.Q<VisualElement>("loading-bar"), 1.2f);
        }
    }
}
