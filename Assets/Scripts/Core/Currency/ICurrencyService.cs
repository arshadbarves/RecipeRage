namespace Core.Currency
{
    /// <summary>
    /// Currency service interface - manages player coins and gems
    /// Follows Single Responsibility Principle - only handles currency logic
    /// </summary>
    public interface ICurrencyService
    {
        /// <summary>
        /// Current coin amount
        /// </summary>
        int Coins { get; }

        /// <summary>
        /// Current gem amount
        /// </summary>
        int Gems { get; }

        /// <summary>
        /// Add coins to player's balance
        /// </summary>
        void AddCoins(int amount);

        /// <summary>
        /// Add gems to player's balance
        /// </summary>
        void AddGems(int amount);

        /// <summary>
        /// Spend coins if player has enough
        /// </summary>
        /// <returns>True if successful, false if insufficient funds</returns>
        bool SpendCoins(int amount);

        /// <summary>
        /// Spend gems if player has enough
        /// </summary>
        /// <returns>True if successful, false if insufficient funds</returns>
        bool SpendGems(int amount);

        /// <summary>
        /// Load currency from save system
        /// </summary>
        void LoadFromSave();

        /// <summary>
        /// Save currency to disk
        /// </summary>
        void SaveToDisk();

        /// <summary>
        /// Format currency amount for display (1K, 1M, etc.)
        /// </summary>
        string FormatCurrency(int amount);
    }
}
