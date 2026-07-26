namespace KitchenClash.Domain
{
    public enum WinType
    {
        Dominant,
        Standard,
        Close
    }

    /// <summary>
    /// Result of a trophy calculation after a match.
    /// </summary>
    public sealed class TrophyResult
    {
        public int TrophiesChanged { get; }
        public int NewTotal { get; }
        public WinType WinType { get; }
        public bool Won { get; }
        public bool Disconnected { get; }

        public TrophyResult(int trophiesChanged, int newTotal, WinType winType, bool won, bool disconnected)
        {
            TrophiesChanged = trophiesChanged;
            NewTotal = newTotal;
            WinType = winType;
            Won = won;
            Disconnected = disconnected;
        }
    }
}
