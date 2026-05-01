using System;
using System.Collections.Generic;

namespace KitchenClash.Domain
{
    public interface IScoreService
    {
        int TeamAScore { get; }
        int TeamBScore { get; }
        void AddScore(TeamId team, ScoreEvent evt);
        IReadOnlyList<int> GetAllScores();
        event Action<ScoreChangedEvent> OnScoreChanged;
    }
}
