using System;
using System.Collections.Generic;
using KitchenClash.Domain;

namespace KitchenClash.Application
{
    public sealed class EconomyService
    {
        private readonly IEventBus _eventBus;
        private int _coins;
        private int _gems;
        private readonly HashSet<string> _ownedItems = new HashSet<string>();

        public event Action<string, long> OnBalanceChanged;

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

        public int GetBalance(string currencyId)
        {
            if (currencyId == EconomyKeys.CurrencyGems) return _gems;
            return _coins;
        }

        public void AddCurrency(string currencyId, int amount)
        {
            if (currencyId == EconomyKeys.CurrencyGems) AddGems(amount);
            else AddCoins(amount);
        }

        private void NotifyChanged()
        {
            _eventBus.Publish(new CurrencyChangedEvent { Coins = _coins, Gems = _gems });
            OnBalanceChanged?.Invoke("coins", _coins);
        }

        /// <summary>
        /// Check if the player owns a specific item.
        /// </summary>
        public bool HasItem(string itemId) => _ownedItems.Contains(itemId);

        /// <summary>
        /// Attempt to purchase an item using the specified currency.
        /// </summary>
        public bool Purchase(string itemId, int cost, string currencyType)
        {
            bool spent = currencyType == EconomyKeys.CurrencyGems
                ? TrySpendGems(cost)
                : TrySpendCoins(cost);

            if (spent)
            {
                _ownedItems.Add(itemId);
            }

            return spent;
        }

        /// <summary>
        /// Initialize the economy service (load persisted data, etc.).
        /// </summary>
        public void Initialize()
        {
            // TODO: Load persisted economy data
        }
    }
}
