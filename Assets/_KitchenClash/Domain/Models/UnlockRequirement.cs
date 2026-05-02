namespace KitchenClash.Domain
{
    public enum UnlockType
    {
        Starter,
        Level,
        Shop,
        Wins,
        Trophies,
        Matches,
        BattlePass
    }

    /// <summary>
    /// Describes how a chef is unlocked per GDD v3.
    /// </summary>
    public sealed class UnlockRequirement
    {
        public UnlockType Type { get; set; }
        public int Value { get; set; }
        /// <summary>Season identifier for BattlePass unlocks (e.g. "S1").</summary>
        public string Season { get; set; }

        public UnlockRequirement() { }

        public UnlockRequirement(UnlockType type, int value = 0, string season = null)
        {
            Type = type;
            Value = value;
            Season = season;
        }

        public static UnlockRequirement Starter() => new(UnlockType.Starter);
        public static UnlockRequirement AtLevel(int level) => new(UnlockType.Level, level);
        public static UnlockRequirement ForCoins(int coins) => new(UnlockType.Shop, coins);
        public static UnlockRequirement ForWins(int wins) => new(UnlockType.Wins, wins);
        public static UnlockRequirement ForTrophies(int trophies) => new(UnlockType.Trophies, trophies);
        public static UnlockRequirement ForMatches(int matches) => new(UnlockType.Matches, matches);
        public static UnlockRequirement ForBattlePass(string season, int tier) => new(UnlockType.BattlePass, tier, season);
    }
}
