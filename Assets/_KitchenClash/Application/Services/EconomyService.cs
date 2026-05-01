using KitchenClash.Domain;

namespace KitchenClash.Application
{
    public sealed class EconomyService
    {
        private readonly IEventBus _eventBus;
        private int _coins;
        private int _gems;

        public EconomyService(IEventBus eventBus) => _eventBus = eventBus;

        public int Coins => _coins;
        public int Gems => _gems;

        public bool TrySpendCoins(int amount)
        {
            if (_coins < amount) return false;
            _coins -= amount;
            NotifyChanged();
            return true;
        }

        public bool TrySpendGems(int amount)
        {
            if (_gems < amount) return false;
            _gems -= amount;
            NotifyChanged();
            return true;
        }

        public void AddCoins(int amount)
        {
            _coins += amount;
            NotifyChanged();
        }

        public void AddGems(int amount)
        {
            _gems += amount;
            NotifyChanged();
        }

        public void SetCurrency(int coins, int gems)
        {
            _coins = coins;
            _gems = gems;
            NotifyChanged();
        }

        private void NotifyChanged()
        {
            _eventBus.Publish(new CurrencyChangedEvent { Coins = _coins, Gems = _gems });
        }
    }
}
