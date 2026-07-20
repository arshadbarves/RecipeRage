using KitchenClash.Domain;
using Playcenter.Services;

namespace KitchenClash.Application
{
    /// <summary>
    /// Pluggable win-condition strategy for each game mode.
    /// Implemented by: TugOfWarWinCondition, RaceToScoreWinCondition, BestOfRoundsWinCondition.
    /// </summary>
    public interface IModeWinCondition
    {
        /// <summary>
        /// Update with the latest score state. Returns WinResult.None if match continues.
        /// </summary>
        ModeWinResult Evaluate(int scoreA, int scoreB);

        /// <summary>Called when the mode/round starts (resets internal state).</summary>
        void Reset();
    }

    public readonly struct ModeWinResult
    {
        public static readonly ModeWinResult None = new(false, TeamId.TeamA);

        public bool   HasWinner { get; }
        public TeamId Winner    { get; }

        public ModeWinResult(bool hasWinner, TeamId winner)
        {
            HasWinner = hasWinner;
            Winner    = winner;
        }

        public static ModeWinResult Win(TeamId winner) => new(true, winner);
    }
}
