namespace KitchenClash.Domain
{
    public interface ITrophyService
    {
        int CurrentTrophies { get; }
        TrophyResult CalculateMatchResult(bool won, int scoreDifference, bool disconnected);
    }
}
