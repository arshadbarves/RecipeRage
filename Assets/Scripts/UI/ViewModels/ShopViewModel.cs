using Core.Bootstrap;
using Core.Reactive;
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
            CoinsText.Value = BankService.FormatCurrency(BankService.Coins);
            GemsText.Value = BankService.FormatCurrency(BankService.Gems);
        }

        public bool BuyItem(ShopItem item)
        {
            if (BankService == null) return false;

            bool success = false;
            if (item.currency == "coins")
            {
                success = BankService.SpendCoins(item.price);
            }
            else if (item.currency == "gems")
            {
                success = BankService.SpendGems(item.price);
            }

            if (success)
            {
                UpdateCurrency();
                
                if (item.type == "skin")
                {
                    BankService.UnlockSkin(item.id);
                }
            }
            return success;
        }
    }
}