using Core.Logging;
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
        private Label _loadingLabel;

        protected override void OnInitialize()
        {
            _logo = GetElement<VisualElement>("splash-logo");
            _loadingLabel = GetElement<Label>("loading-text");
            
            TransitionType = UITransitionType.Fade;

            BindViewModel();
        }

        private void BindViewModel()
        {
            if (_viewModel == null) return;
            
            _viewModel.Initialize();
            _viewModel.LoadingText.Bind(text => 
            {
                if (_loadingLabel != null) _loadingLabel.text = text;
            });
        }

        protected override void OnDispose()
        {
            _viewModel?.Dispose();
        }
    }
}