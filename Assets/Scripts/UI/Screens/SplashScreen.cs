using Core.Animation;
using Core.Logging;
using UI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace UI.Screens
{
    /// <summary>
    /// Splash screen shown at startup
    /// </summary>
    [UIScreen(UIScreenType.Splash, UIScreenCategory.System, "Screens/SplashScreenTemplate")]
    public class SplashScreen : BaseUIScreen
    {
        private VisualElement _logo;
        private Label _loadingLabel;

        protected override void OnInitialize()
        {
            _logo = GetElement<VisualElement>("splash-logo");
            _loadingLabel = GetElement<Label>("loading-text");
        }

        protected override void OnShow()
        {
            if (_logo != null)
            {
                // Animation logic could go here
            }
        }
    }
}