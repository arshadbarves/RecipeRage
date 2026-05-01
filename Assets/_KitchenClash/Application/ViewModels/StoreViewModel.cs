using KitchenClash.Domain;

namespace KitchenClash.Application
{
    public sealed class StoreViewModel : ScreenViewModel
    {
        private readonly EconomyService _economy;

        public BindableProperty<int> Coins { get; } = new(0);
        public BindableProperty<int> Gems { get; } = new(0);

        public StoreViewModel(EconomyService economy)
        {
            _economy = economy;
        }

        public override void OnEnter(object param)
        {
            Coins.Value = _economy.Coins;
            Gems.Value = _economy.Gems;
        }

        public bool TryPurchase(string itemId, int cost, string currency)
        {
            bool success = currency == EconomyKeys.CurrencyGems
                ? _economy.TrySpendGems(cost)
                : _economy.TrySpendCoins(cost);

            if (success)
            {
                Coins.Value = _economy.Coins;
                Gems.Value = _economy.Gems;
            }
            return success;
        }
    }
}
