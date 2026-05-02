namespace KitchenClash.Domain
{
    public interface ITrophyService
    {
        int Trophies { get; }
        void Initialize();
        int CalculateChange(MatchOutcome outcome);
        void ApplyChange(int delta);
    }
}
