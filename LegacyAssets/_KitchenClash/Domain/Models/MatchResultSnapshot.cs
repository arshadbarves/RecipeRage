namespace KitchenClash.Domain
{
    /// <summary>
    /// Vendor-neutral match-end snapshot for Application/Presentation.
    /// Infrastructure maps NGO <c>MatchResultState</c> into this type at the boundary.
    /// </summary>
    public readonly struct MatchResultSnapshot
    {
        public static MatchResultSnapshot None => new(false, -1, 0, false, MatchEndReason.None);

        public MatchResultSnapshot(
            bool hasResult,
            int winningTeamId,
            int winningScore,
            bool isDraw,
            MatchEndReason endReason)
        {
            HasResult = hasResult;
            WinningTeamId = winningTeamId;
            WinningScore = winningScore;
            IsDraw = isDraw;
            EndReason = endReason;
        }

        public bool HasResult { get; }
        public int WinningTeamId { get; }
        public int WinningScore { get; }
        public bool IsDraw { get; }
        public MatchEndReason EndReason { get; }
    }
}
