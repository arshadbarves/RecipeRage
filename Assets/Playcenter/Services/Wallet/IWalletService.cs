using System;

namespace Playcenter.Services
{
    /// <summary>
    /// Coin wallet. Coins are only earned and spent — never lost per match.
    /// </summary>
    public interface IWalletService
    {
        event Action<int> OnCoinsChanged;
        int GetCoins();
        void AddCoins(int amount);
        bool TrySpendCoins(int amount);
    }
}
