namespace KitchenClash.Domain
{
    public enum UnlockType
    {
        Starter,
        Level,
        Shop
    }

    /// <summary>
    /// Describes how a chef is unlocked: starter, player level gate, or coin purchase.
    /// </summary>
    public sealed class UnlockRequirement
    {
        public UnlockType Type { get; set; }
        public int Value { get; set; }

        public UnlockRequirement() { }

        public UnlockRequirement(UnlockType type, int value = 0)
        {
            Type = type;
            Value = value;
        }

        public static UnlockRequirement Starter() => new(UnlockType.Starter);
        public static UnlockRequirement AtLevel(int level) => new(UnlockType.Level, level);
        public static UnlockRequirement ForCoins(int coins) => new(UnlockType.Shop, coins);
    }
}
