using Modules.Shared.Interfaces;
using Modules.Shared;
using Modules.Core.Banking;
using Modules.Core.Banking.Interfaces;
using UI.Core;
using UI.Data; // Added
using UnityEngine; // Added
using VContainer;

namespace UI.ViewModels
{
    public class ShopViewModel : BaseViewModel
    {
        private readonly SessionManager _sessionManager;
        private IBankService BankService => _sessionManager.SessionContainer?.Resolve<IBankService>();

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
            if (BankService == null) return;
            CoinsText.Value = BankService.GetBalance(BankKeys.CurrencyCoins).ToString();
            GemsText.Value = BankService.GetBalance(BankKeys.CurrencyGems).ToString();
        }

        public bool BuyItem(ShopItem item)
        {
            if (BankService == null) return false;

            // Use the new atomic Purchase method
            // Ensure currency ID is lowercase as per our convention
            string currencyId = item.currency.ToLower();
            bool success = BankService.Purchase(item.id, item.price, currencyId);

            if (success)
            {
                UpdateCurrency();
            }
            return success;
        }
    }
}