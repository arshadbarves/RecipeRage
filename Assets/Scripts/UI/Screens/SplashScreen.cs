using UI.Core;
using UI.ViewModels;
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
        [Inject] private SplashScreenViewModel _viewModel;

        private VisualElement _logo;

        protected override void OnInitialize()
        {
            // The main container holding the logo elements
            _logo = GetElement<VisualElement>("master-container");
            
            TransitionType = UITransitionType.Fade;

            // BindViewModel(); // No bindings needed for static splash
        }

        private void BindViewModel()
        {
            // if (_viewModel == null) return;
            // _viewModel.Initialize();
        }

        protected override void OnDispose()
        {
            _viewModel?.Dispose();
        }
    }
}