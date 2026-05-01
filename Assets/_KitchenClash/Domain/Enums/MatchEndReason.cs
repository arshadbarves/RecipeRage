namespace KitchenClash.Domain
{
    public enum MatchEndReason : byte
    {
        None = 0,
        TimerExpired = 1,
        ScoreLimitReached = 2
    }
}
