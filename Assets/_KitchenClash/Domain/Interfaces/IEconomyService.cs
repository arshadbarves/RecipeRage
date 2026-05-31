namespace KitchenClash.Domain
{
    public interface IEconomyService
    {
        int Coins { get; }
        int Gems { get; }
        int GetBalance(string currencyId);
        bool TrySpendCoins(int amount);
        bool TrySpendGems(int amount);
        void AddCoins(int amount);
        void AddGems(int amount);
        void AddCurrency(string currencyId, int amount);
        bool HasItem(string itemId);
        bool Purchase(string itemId, int cost, string currencyType);
        void Initialize();
        void AwardMatchReward(bool won, int score);
    }
}
