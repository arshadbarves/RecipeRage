using Core.Currency;
using Core.Reactive;
using UI.Core;
using UI.Data; // Added
using UnityEngine; // Added
using VContainer;

namespace UI.ViewModels
{
using Core.Bootstrap; // Added

// ...

    public class ShopViewModel : BaseViewModel
    {
        private readonly SessionManager _sessionManager;
        private ICurrencyService CurrencyService => _sessionManager.SessionContainer?.Resolve<ICurrencyService>();

        public BindableProperty<string> CoinsText { get; } = new BindableProperty<string>("0");
        public BindableProperty<string> GemsText { get; } = new BindableProperty<string>("0");

        [Inject]
        public ShopViewModel(SessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }

        public override void Initialize()
        {
            base.Initialize();
            UpdateCurrency();
        }

        public void UpdateCurrency()
        {
            if (CurrencyService == null) return;
            CoinsText.Value = CurrencyService.FormatCurrency(CurrencyService.Coins);
            GemsText.Value = CurrencyService.FormatCurrency(CurrencyService.Gems);
        }

        public bool BuyItem(ShopItem item)
        {
            if (CurrencyService == null) return false;

            bool success = false;
            if (item.currency == "coins")
            {
                success = CurrencyService.SpendCoins(item.price);
            }
            else if (item.currency == "gems")
            {
                success = CurrencyService.SpendGems(item.price);
            }

            if (success)
            {
                UpdateCurrency();
                // Persist ownership (Legacy: PlayerPrefs)
                PlayerPrefs.SetInt($"Owned_{item.id}", 1);
                if (item.type == "skin") PlayerPrefs.SetInt($"Unlocked_{item.id}", 1);
                PlayerPrefs.Save();
            }
            return success;
        }
    }
}