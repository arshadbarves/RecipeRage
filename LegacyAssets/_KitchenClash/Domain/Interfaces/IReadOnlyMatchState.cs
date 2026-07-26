namespace KitchenClash.Domain
{
    public interface IReadOnlyMatchState
    {
        GamePhase CurrentPhase { get; }
        float TimeRemaining { get; }
        int TeamAScore { get; }
        int TeamBScore { get; }
        bool IsRushMode { get; }
    }
}
