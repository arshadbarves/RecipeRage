using System;
using System.Collections.Generic;
using KitchenClash.Domain;

namespace KitchenClash.Application
{
    public sealed class ScoreService : IScoreService
    {
        private readonly IConfigService _cfg;
        private int _a, _b;

        public ScoreService(IConfigService cfg) => _cfg = cfg;

        public int TeamAScore => _a;
        public int TeamBScore => _b;
        public event Action<ScoreChangedEvent> OnScoreChanged;

        public IReadOnlyList<int> GetAllScores() => new[] { _a, _b };

        public void AddScore(TeamId team, ScoreEvent e)
        {
            int d = Calc(e);
            if (team == TeamId.TeamA) _a = Math.Max(0, _a + d);
            else _b = Math.Max(0, _b + d);
            OnScoreChanged?.Invoke(new ScoreChangedEvent(team, d, _a, _b));
        }

        private int Calc(ScoreEvent e)
        {
            if (e.Type == ScoreEventType.BurnedServed)
                return -_cfg.Get(ScoringConfig.ScoreBurnPenalty, ScoringConfig.DefaultScoreBurnPenalty);
            if (e.Type == ScoreEventType.FirePenalty)
                return -_cfg.Get(ScoringConfig.ScoreFirePenalty, ScoringConfig.DefaultScoreFirePenalty);

            float mult = e.RecipeTier == 3
                ? _cfg.Get(ScoringConfig.ScoreTier3Mult, ScoringConfig.DefaultScoreTier3Mult)
                : e.RecipeTier == 2
                    ? _cfg.Get(ScoringConfig.ScoreTier2Mult, ScoringConfig.DefaultScoreTier2Mult)
                    : 1.0f;

            int baseScore = (int)(_cfg.Get(ScoringConfig.ScoreBase, ScoringConfig.DefaultScoreBase) * mult);
            int speed = (int)(e.SpeedRatio * _cfg.Get(ScoringConfig.ScoreSpeedMax, ScoringConfig.DefaultScoreSpeedMax));
            int rhythm = e.RhythmBonus ? _cfg.Get(ScoringConfig.ScoreRhythm, ScoringConfig.DefaultScoreRhythm) : 0;
            int combo = e.ComboCount >= 3 ? _cfg.Get(ScoringConfig.ScoreCombo, ScoringConfig.DefaultScoreCombo) : 0;

            return baseScore + speed + rhythm + combo;
        }
    }
}
