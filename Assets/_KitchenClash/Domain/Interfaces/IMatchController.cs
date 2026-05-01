namespace KitchenClash.Domain
{
    public interface IMatchController
    {
        void StartMatch();
        void EndMatch(MatchEndReason reason);
        void SetPhase(GamePhase phase);
        void Pause();
        void Resume();
    }
}
