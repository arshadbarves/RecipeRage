using Core.Currency;
using Core.Reactive;
using UI.Core;
using VContainer;

namespace UI.ViewModels
{
    public class ShopViewModel : BaseViewModel
    {
        private readonly ICurrencyService _currencyService;

        public BindableProperty<string> CoinsText { get; } = new BindableProperty<string>("0");
        public BindableProperty<string> GemsText { get; } = new BindableProperty<string>("0");

        [Inject]
        public ShopViewModel(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        public override void Initialize()
        {
            base.Initialize();
            UpdateCurrency();
        }

        public void UpdateCurrency()
        {
            CoinsText.Value = _currencyService.FormatCurrency(_currencyService.Coins);
            GemsText.Value = _currencyService.FormatCurrency(_currencyService.Gems);
        }
    }
}