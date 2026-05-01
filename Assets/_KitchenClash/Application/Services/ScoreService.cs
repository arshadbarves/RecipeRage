using System;
using KitchenClash.Domain;

namespace KitchenClash.Application
{
    public sealed class ScoreService : IScoreService
    {
        private readonly IConfigService _cfg;
        private readonly IEventBus _eventBus;
        private int _a, _b;
        private bool _rushMode;

        public ScoreService(IConfigService cfg, IEventBus eventBus)
        {
            _cfg = cfg;
            _eventBus = eventBus;
        }

        public int TeamAScore => _a;
        public int TeamBScore => _b;
        public bool IsRushMode => _rushMode;
        public event Action<ScoreChangedEvent> OnScoreChanged;

        public void SetRushMode(bool active)
        {
            if (!_rushMode && active)
            {
                _eventBus?.Publish(new SFXEvent(SFXType.RushModeActivated));
                _eventBus?.Publish(new MusicEvent(MusicTrack.Gameplay_Rush, 1.5f));
            }
            _rushMode = active;
        }

        public void UpdateMatchTime(float timeRemaining)
        {
            float threshold = _cfg.Get(ScoringConfig.RushModeThresholdSec, ScoringConfig.DefaultRushModeThresholdSec);
            if (!_rushMode && timeRemaining <= threshold)
                SetRushMode(true);
        }

        public int CalculateEndOfMatchBonus(TeamId team)
        {
            float pct = _cfg.Get(ScoringConfig.ScorePlatePct, ScoringConfig.DefaultScorePlatePct);
            int total = team == TeamId.TeamA ? TeamAScore : TeamBScore;
            return (int)(total * pct);
        }

        public void AddScore(TeamId team, ScoreEvent e)
        {
            int d = Calc(e);
            if (team == TeamId.TeamA) _a = Math.Max(0, _a + d);
            else _b = Math.Max(0, _b + d);
            OnScoreChanged?.Invoke(new ScoreChangedEvent(team, d, _a, _b));
            _eventBus?.Publish(new SFXEvent(SFXType.ScorePoint));
        }

        private int Calc(ScoreEvent e)
        {
            if (e.Type == ScoreEventType.BurnedServed)
                return ApplyRush(-_cfg.Get(ScoringConfig.ScoreBurnPenalty, ScoringConfig.DefaultScoreBurnPenalty));
            if (e.Type == ScoreEventType.FirePenalty)
                return ApplyRush(-_cfg.Get(ScoringConfig.ScoreFirePenalty, ScoringConfig.DefaultScoreFirePenalty));

            float mult = e.RecipeTier == 3
                ? _cfg.Get(ScoringConfig.ScoreTier3Mult, ScoringConfig.DefaultScoreTier3Mult)
                : e.RecipeTier == 2
                    ? _cfg.Get(ScoringConfig.ScoreTier2Mult, ScoringConfig.DefaultScoreTier2Mult)
                    : 1.0f;

            int baseScore = (int)(_cfg.Get(ScoringConfig.ScoreBase, ScoringConfig.DefaultScoreBase) * mult);

            // GDD v3: +5 if delivered < 50% time, +3 if < 75% time, 0 otherwise
            // SpeedRatio: fraction of time used (0 = instant, 1 = full time)
            int speedHigh = _cfg.Get(ScoringConfig.ScoreSpeedHigh, ScoringConfig.DefaultScoreSpeedHigh);
            int speedLow = _cfg.Get(ScoringConfig.ScoreSpeedLow, ScoringConfig.DefaultScoreSpeedLow);
            int speed = e.SpeedRatio < 0.50f ? speedHigh
                      : e.SpeedRatio < 0.75f ? speedLow
                      : 0;

            int rhythm = e.RhythmBonus ? _cfg.Get(ScoringConfig.ScoreRhythm, ScoringConfig.DefaultScoreRhythm) : 0;
            int combo = e.ComboCount >= 3 ? _cfg.Get(ScoringConfig.ScoreCombo, ScoringConfig.DefaultScoreCombo) : 0;

            return ApplyRush(baseScore + speed + rhythm + combo);
        }

        private int ApplyRush(int raw)
        {
            if (!_rushMode) return raw;
            float mult = _cfg.Get(ScoringConfig.RushModeMultiplier, ScoringConfig.DefaultRushModeMultiplier);
            return (int)(raw * mult);
        }
    }
}
