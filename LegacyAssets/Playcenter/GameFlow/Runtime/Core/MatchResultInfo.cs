namespace Playcenter.GameFlow
{
    /// <summary>
    /// Snapshot when a match ends — enough for Results + Play Again.
    /// </summary>
    public sealed class MatchResultInfo
    {
        public bool Won { get; set; }
        public bool IsDraw { get; set; }
        public int WinningTeamId { get; set; }
        public int LocalTeamId { get; set; }
        public int LocalTeamScore { get; set; }
        public int OpponentTeamScore { get; set; }
        public string ModeId { get; set; }
    }
}
