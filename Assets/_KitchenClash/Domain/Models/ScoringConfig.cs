namespace KitchenClash.Domain
{
    public static class ScoringConfig
    {
        public const string ScoreBase = "score_base";
        public const string ScoreSpeedMax = "score_speed_max";
        public const string ScoreRhythm = "score_rhythm";
        public const string ScoreCombo = "score_combo";
        public const string ScoreTier2Mult = "score_tier2_mult";
        public const string ScoreTier3Mult = "score_tier3_mult";
        public const string ScoreBurnPenalty = "score_burn_penalty";
        public const string ScoreFirePenalty = "score_fire_penalty";
        public const string ScorePlatePct = "score_plate_pct";
        public const string ScoreSpeedHigh = "score_speed_high";
        public const string ScoreSpeedLow = "score_speed_low";
        public const string RushModeThresholdSec = "rush_mode_threshold_sec";
        public const string RushModeMultiplier = "rush_mode_multiplier";

        // Defaults per GDD v3
        public const int DefaultScoreBase = 10;
        public const int DefaultScoreSpeedMax = 5;
        public const int DefaultScoreSpeedHigh = 5;
        public const int DefaultScoreSpeedLow = 3;
        public const int DefaultScoreRhythm = 1;
        public const int DefaultScoreCombo = 2;
        public const float DefaultScoreTier2Mult = 1.5f;
        public const float DefaultScoreTier3Mult = 2.0f;
        public const int DefaultScoreBurnPenalty = 2;
        public const int DefaultScoreFirePenalty = 5;
        public const float DefaultScorePlatePct = 0.10f;
        public const float DefaultRushModeThresholdSec = 60f;
        public const float DefaultRushModeMultiplier = 1.5f;
    }
}
