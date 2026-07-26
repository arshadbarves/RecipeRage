using System;
using System.Collections.Generic;

namespace Playcenter.Services
{
    public sealed class CoinWalletService : IWalletService
    {
        private const string CoinsKey = "wallet_coins";

        private readonly ISaveService _save;
        private readonly IAnalyticsService _analytics;
        private CoinData _data;

        public event Action<int> OnCoinsChanged;

        public CoinWalletService(ISaveService save, IAnalyticsService analytics)
        {
            _save = save;
            _analytics = analytics;
            _data = _save.Load(CoinsKey, new CoinData { Coins = 0 });
        }

        public int GetCoins() => _data.Coins;

        public void AddCoins(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            _data.Coins += amount;
            _save.Save(CoinsKey, _data);
            _analytics.TrackEvent("coins_earned", new Dictionary<string, object>
            {
                { "amount", amount },
                { "total", _data.Coins }
            });
            OnCoinsChanged?.Invoke(_data.Coins);
        }

        public bool TrySpendCoins(int amount)
        {
            if (amount <= 0 || _data.Coins < amount)
            {
                return false;
            }

            _data.Coins -= amount;
            _save.Save(CoinsKey, _data);
            _analytics.TrackEvent("coins_spent", new Dictionary<string, object>
            {
                { "amount", amount },
                { "total", _data.Coins }
            });
            OnCoinsChanged?.Invoke(_data.Coins);
            return true;
        }

        [Serializable]
        private sealed class CoinData
        {
            public int Coins;
        }
    }
}
