namespace KitchenClash.Application.Config
{
    /// <summary>
    /// All remote-config key constants from GDD v3 with their server-side defaults.
    /// </summary>
    public static class RemoteConfigKeys
    {
        // ── Scoring ──
        public const string ScoreBase = "score_base";                       // 10
        public const string ScoreSpeedMax = "score_speed_max";              // 5
        public const string ScoreRhythm = "score_rhythm";                   // 1
        public const string ScoreCombo = "score_combo";                     // 2
        public const string ScoreTier2Mult = "score_tier2_mult";            // 1.5
        public const string ScoreTier3Mult = "score_tier3_mult";            // 2.0
        public const string ScoreBurnPenalty = "score_burn_penalty";        // 2
        public const string ScoreFirePenalty = "score_fire_penalty";        // 5
        public const string ScorePlatePct = "score_plate_pct";              // 0.10

        // ── Gameplay ──
        public const string MatchDurationSec = "match_duration_sec";        // 180
        public const string MatchDurationRankedSec = "match_duration_ranked_sec"; // 300
        public const string RushStartSec = "rush_start_sec";                // 60
        public const string RushOrderMult = "rush_order_mult";              // 1.5
        public const string ChopTapsLettuce = "chop_taps_lettuce";          // 3
        public const string ChopTapsCarrot = "chop_taps_carrot";            // 4
        public const string ChopTapsFish = "chop_taps_fish";                // 3
        public const string ChopTapsMeat = "chop_taps_meat";                // 5
        public const string ChopTapCapPerSec = "chop_tap_cap_per_sec";      // 10
        public const string OrderGenRateNormal = "order_gen_rate_normal";    // 1.0
        public const string FireExtinguishWindowSec = "fire_extinguish_window_sec"; // 5
        public const string BotFillDelaySec = "bot_fill_delay_sec";          // 40
        public const string AbilityCooldownDefault = "ability_cooldown_default"; // 10

        // ── Matchmaking + Trophies ──
        public const string TrophyWinDominant = "trophy_win_dominant";       // 35
        public const string TrophyWinStandard = "trophy_win_standard";       // 25
        public const string TrophyWinClose = "trophy_win_close";             // 20
        public const string TrophyLossClose = "trophy_loss_close";           // -15
        public const string TrophyLossStandard = "trophy_loss_standard";     // -20
        public const string TrophyDisconnect = "trophy_disconnect";          // -30
        public const string TrophyBracketTight = "trophy_bracket_tight";     // 200

        // ── Map Slots ──
        public const string SlotAMapId = "slot_a_map_id";                    // "sushi_shuffle"
        public const string SlotAMode = "slot_a_mode";                       // "quick_2v2"
        public const string SlotARotationHrs = "slot_a_rotation_hrs";        // 6
        public const string SlotBMapId = "slot_b_map_id";                    // "pirate_pot"
        public const string SlotBMode = "slot_b_mode";                       // "quick_3v3"
        public const string SlotCMapId = "slot_c_map_id";                    // "burger_boulevard"
        public const string SlotCMode = "slot_c_mode";                       // "ranked"
        public const string SlotDMapId = "slot_d_map_id";                    // "clash_kitchen"
        public const string SlotDMode = "slot_d_mode";                       // "event"
        public const string SlotDDurationHrs = "slot_d_duration_hrs";        // 48

        // ── Ads + Store ──
        public const string AdInterstitialEnabled = "ad_interstitial_enabled";       // true
        public const string AdInterstitialFrequency = "ad_interstitial_frequency";   // 3
        public const string AdInterstitialMinGapSec = "ad_interstitial_min_gap_sec"; // 180
        public const string AdRewardedEnabled = "ad_rewarded_enabled";               // true
        public const string BattlePassDurationDays = "battle_pass_duration_days";    // 56
        public const string DailyStreakCycleDays = "daily_streak_cycle_days";        // 60

        // ── Defaults (for fallback config) ──
        public static class Defaults
        {
            public const int ScoreBase = 10;
            public const int ScoreSpeedMax = 5;
            public const int ScoreRhythm = 1;
            public const int ScoreCombo = 2;
            public const float ScoreTier2Mult = 1.5f;
            public const float ScoreTier3Mult = 2.0f;
            public const int ScoreBurnPenalty = 2;
            public const int ScoreFirePenalty = 5;
            public const float ScorePlatePct = 0.10f;

            public const int MatchDurationSec = 180;
            public const int MatchDurationRankedSec = 300;
            public const int RushStartSec = 60;
            public const float RushOrderMult = 1.5f;
            public const int ChopTapsLettuce = 3;
            public const int ChopTapsCarrot = 4;
            public const int ChopTapsFish = 3;
            public const int ChopTapsMeat = 5;
            public const int ChopTapCapPerSec = 10;
            public const float OrderGenRateNormal = 1.0f;
            public const int FireExtinguishWindowSec = 5;
            public const int BotFillDelaySec = 40;
            public const int AbilityCooldownDefault = 10;

            public const int TrophyWinDominant = 35;
            public const int TrophyWinStandard = 25;
            public const int TrophyWinClose = 20;
            public const int TrophyLossClose = -15;
            public const int TrophyLossStandard = -20;
            public const int TrophyDisconnect = -30;
            public const int TrophyBracketTight = 200;

            public const string SlotAMapId = "sushi_shuffle";
            public const string SlotAMode = "quick_2v2";
            public const int SlotARotationHrs = 6;
            public const string SlotBMapId = "pirate_pot";
            public const string SlotBMode = "quick_3v3";
            public const string SlotCMapId = "burger_boulevard";
            public const string SlotCMode = "ranked";
            public const string SlotDMapId = "clash_kitchen";
            public const string SlotDMode = "event";
            public const int SlotDDurationHrs = 48;

            public const bool AdInterstitialEnabled = true;
            public const int AdInterstitialFrequency = 3;
            public const int AdInterstitialMinGapSec = 180;
            public const bool AdRewardedEnabled = true;
            public const int BattlePassDurationDays = 56;
            public const int DailyStreakCycleDays = 60;
        }
    }
}
