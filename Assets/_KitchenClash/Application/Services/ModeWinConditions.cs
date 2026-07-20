using KitchenClash.Application.Config;
using KitchenClash.Domain;
using Playcenter.Services;

namespace KitchenClash.Application
{
    // ═══════════════════════════════════════════════════════════════════════
    // Mode 1: Rush Service — Tug-of-War bar (first team to ±100 from centre)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Tug-of-war win condition for Rush Service (2v2).
    ///
    /// Internal bar starts at 0 (centred between −target and +target).
    /// Each ScoreChangedEvent is translated to a bar delta:
    ///   TeamA scores → bar moves toward +target
    ///   TeamB scores → bar moves toward −target
    ///
    /// When bar reaches ±target, the corresponding team wins.
    /// </summary>
    public sealed class TugOfWarWinCondition : IModeWinCondition
    {
        private readonly IConfigService _cfg;
        private int _bar; // current bar position, 0 = centred

        public int Bar => _bar;

        public TugOfWarWinCondition(IConfigService cfg) => _cfg = cfg;

        public void Reset() => _bar = 0;

        /// <summary>
        /// Instead of raw team scores, this condition tracks cumulative score deltas.
        /// scoreA / scoreB here are interpreted as the latest delta each time Evaluate is called.
        /// Use EvaluateDelta for clarity.
        /// </summary>
        public ModeWinResult Evaluate(int scoreA, int scoreB)
        {
            int target = _cfg.Get(RemoteConfigKeys.RushServiceTarget, RemoteConfigKeys.Defaults.RushServiceTarget);

            if (_bar >= target)  return ModeWinResult.Win(TeamId.TeamA);
            if (_bar <= -target) return ModeWinResult.Win(TeamId.TeamB);

            return ModeWinResult.None;
        }

        /// <summary>Apply a scored dish delta and check win.</summary>
        public ModeWinResult EvaluateDelta(TeamId team, int delta)
        {
            _bar += team == TeamId.TeamA ? delta : -delta;
            return Evaluate(0, 0);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Mode 2: Hell's Kitchen — Race to score target
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// First-to-target win condition for Hell's Kitchen (3v3).
    /// Checks cumulative team scores against hell_kitchen_target_score RC key.
    /// </summary>
    public sealed class RaceToScoreWinCondition : IModeWinCondition
    {
        private readonly IConfigService _cfg;

        public RaceToScoreWinCondition(IConfigService cfg) => _cfg = cfg;

        public void Reset() { /* stateless — scores held in ScoreService */ }

        public ModeWinResult Evaluate(int scoreA, int scoreB)
        {
            int target = _cfg.Get(RemoteConfigKeys.HellKitchenTarget, RemoteConfigKeys.Defaults.HellKitchenTarget);

            bool aWon = scoreA >= target;
            bool bWon = scoreB >= target;

            if (aWon && bWon)
                return scoreA >= scoreB ? ModeWinResult.Win(TeamId.TeamA) : ModeWinResult.Win(TeamId.TeamB);

            if (aWon) return ModeWinResult.Win(TeamId.TeamA);
            if (bWon) return ModeWinResult.Win(TeamId.TeamB);

            return ModeWinResult.None;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Mode 3: Last Plate Standing — Best of 3 rounds, no respawn per round
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Best-of-rounds win condition for Last Plate Standing (2v2).
    ///
    /// Each round ends when:
    ///   • A team delivers last_plate_round_dish_target dishes, OR
    ///   • The round timer expires (team with more dishes wins the round; tie = neither)
    ///
    /// Match ends when a team reaches ceil(totalRounds / 2) round wins.
    /// Default: 3 rounds → first to 2 round wins.
    /// </summary>
    public sealed class BestOfRoundsWinCondition : IModeWinCondition
    {
        private readonly IConfigService _cfg;

        private int _roundWinsA;
        private int _roundWinsB;
        private int _roundDishesA;
        private int _roundDishesB;

        public int RoundWinsA   => _roundWinsA;
        public int RoundWinsB   => _roundWinsB;
        public int RoundDishesA => _roundDishesA;
        public int RoundDishesB => _roundDishesB;

        public BestOfRoundsWinCondition(IConfigService cfg) => _cfg = cfg;

        public void Reset()
        {
            _roundWinsA   = 0;
            _roundWinsB   = 0;
            ResetRound();
        }

        /// <summary>Reset per-round dish counters between rounds.</summary>
        public void ResetRound()
        {
            _roundDishesA = 0;
            _roundDishesB = 0;
        }

        /// <summary>Record a dish delivery within the current round.</summary>
        public void RecordRoundDish(TeamId team)
        {
            if (team == TeamId.TeamA) _roundDishesA++;
            else                      _roundDishesB++;
        }

        /// <summary>
        /// Evaluate whether the current round has been won.
        /// Call after every dish delivery.
        /// </summary>
        public ModeWinResult EvaluateRound()
        {
            int target = _cfg.Get(RemoteConfigKeys.LastPlateDishTarget, RemoteConfigKeys.Defaults.LastPlateDishTarget);

            bool aWon = _roundDishesA >= target;
            bool bWon = _roundDishesB >= target;

            if (!aWon && !bWon) return ModeWinResult.None;

            // Both hit simultaneously — tiebreak by count
            TeamId roundWinner = (aWon && _roundDishesA >= _roundDishesB) ? TeamId.TeamA : TeamId.TeamB;
            CommitRoundWin(roundWinner);
            return Evaluate(0, 0);
        }

        /// <summary>Called by match system when round timer expires.</summary>
        public ModeWinResult EvaluateRoundExpiry()
        {
            if (_roundDishesA == _roundDishesB) return ModeWinResult.None; // tie round — no winner
            TeamId roundWinner = _roundDishesA > _roundDishesB ? TeamId.TeamA : TeamId.TeamB;
            CommitRoundWin(roundWinner);
            return Evaluate(0, 0);
        }

        private void CommitRoundWin(TeamId team)
        {
            if (team == TeamId.TeamA) _roundWinsA++;
            else                      _roundWinsB++;
            ResetRound();
        }

        /// <summary>
        /// Check if the match (not the round) has been won.
        /// scoreA / scoreB params unused — uses internal round win counts.
        /// </summary>
        public ModeWinResult Evaluate(int scoreA, int scoreB)
        {
            const int totalRounds = 3;
            int winsNeeded = (totalRounds / 2) + 1; // 2 of 3

            if (_roundWinsA >= winsNeeded) return ModeWinResult.Win(TeamId.TeamA);
            if (_roundWinsB >= winsNeeded) return ModeWinResult.Win(TeamId.TeamB);
            return ModeWinResult.None;
        }
    }
}
