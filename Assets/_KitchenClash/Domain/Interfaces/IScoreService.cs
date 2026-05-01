using System;

namespace KitchenClash.Domain
{
    public interface IScoreService
    {
        int TeamAScore { get; }
        int TeamBScore { get; }
        void AddScore(TeamId team, ScoreEvent evt);
        int CalculateEndOfMatchBonus(TeamId team);
        event Action<ScoreChangedEvent> OnScoreChanged;
    }
}
