using Gameplay.UI.Data;
using Gameplay.Economy;
using Core.Shared;
using Core.UI.Core;
using Core.Session;
using VContainer;

namespace Gameplay.UI.Features.Shop
{
    public class ShopViewModel : BaseViewModel
    {
        private readonly SessionManager _sessionManager;
        private EconomyService EconomyService => _sessionManager.SessionContainer?.Resolve<EconomyService>();

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
            if (EconomyService == null) return;
            CoinsText.Value = EconomyService.GetBalance(EconomyKeys.CurrencyCoins).ToString();
            GemsText.Value = EconomyService.GetBalance(EconomyKeys.CurrencyGems).ToString();
        }

        public bool BuyItem(ShopItem item)
        {
            if (EconomyService == null) return false;

            string currencyId = item.currency.ToLower();
            bool success = EconomyService.Purchase(item.id, item.price, currencyId);

            if (success) UpdateCurrency();
            return success;
        }
    }
}