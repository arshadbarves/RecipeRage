using Core.Localization;
using Core.Reactive;
using UI.Core;
using VContainer;

namespace UI.ViewModels
{
    public class SplashScreenViewModel : BaseViewModel
    {
        private readonly ILocalizationManager _localizationManager;

        public BindableProperty<string> LoadingText { get; private set; } = new BindableProperty<string>("Loading...");

        [Inject]
        public SplashScreenViewModel(ILocalizationManager localizationManager)
        {
            _localizationManager = localizationManager;
        }

        public override void Initialize()
        {
            base.Initialize();
            // In a real app, we might load the initial string from localization
            // LoadingText.Value = _localizationManager.GetText("splash_loading");
        }
    }
}