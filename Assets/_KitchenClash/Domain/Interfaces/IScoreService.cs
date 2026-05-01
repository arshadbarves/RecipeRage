using System;

namespace KitchenClash.Domain
{
    public interface IScoreService
    {
        int TeamAScore { get; }
        int TeamBScore { get; }
        bool IsRushMode { get; }
        void AddScore(TeamId team, ScoreEvent evt);
        int CalculateEndOfMatchBonus(TeamId team);
        void SetRushMode(bool active);
        void UpdateMatchTime(float timeRemaining);
        event Action<ScoreChangedEvent> OnScoreChanged;
    }
}
