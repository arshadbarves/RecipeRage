using System;

namespace Core.Currency
{
    /// <summary>
    /// Serializable currency data for save system
    /// </summary>
    [Serializable]
    public class CurrencyData
    {
        public int Coins;
        public int Gems;

        public CurrencyData()
        {
            Coins = 0;
            Gems = 0;
        }

        public CurrencyData(int coins, int gems)
        {
            Coins = coins;
            Gems = gems;
        }
    }
}
