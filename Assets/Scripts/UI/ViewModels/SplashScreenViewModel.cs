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
            UpdateLocalizedText();
            _localizationManager.OnLanguageChanged += UpdateLocalizedText;
        }

        public override void Dispose()
        {
            if (_localizationManager != null)
            {
                _localizationManager.OnLanguageChanged -= UpdateLocalizedText;
            }
            base.Dispose();
        }

        private void UpdateLocalizedText()
        {
            LoadingText.Value = _localizationManager.GetText("splash_loading");
        }
    }
}