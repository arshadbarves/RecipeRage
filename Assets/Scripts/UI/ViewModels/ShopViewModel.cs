using Core.Currency;
using Core.Reactive;
using UI.Core;
using UI.Data; // Added
using UnityEngine; // Added
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

        public bool BuyItem(ShopItem item)
        {
            bool success = false;
            if (item.currency == "coins")
            {
                success = _currencyService.SpendCoins(item.price);
            }
            else if (item.currency == "gems")
            {
                success = _currencyService.SpendGems(item.price);
            }

            if (success)
            {
                UpdateCurrency();
                // Persist ownership (Legacy: PlayerPrefs)
                // Ideally move this to SaveService/ShopService
                PlayerPrefs.SetInt($"Owned_{item.id}", 1);
                if (item.type == "skin") PlayerPrefs.SetInt($"Unlocked_{item.id}", 1);
                PlayerPrefs.Save();
            }
            return success;
        }
    }
}