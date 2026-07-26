namespace Playcenter.GameFlow
{
    /// <summary>
    /// Mutable product context carried across phases (last queue, last result, etc.).
    /// Owned by <see cref="IAppFlow"/>; ports may read it, not replace it.
    /// </summary>
    public sealed class FlowContext
    {
        public string LastModeId { get; set; }
        public int LastTeamSize { get; set; } = 2;
        public string LastChefId { get; set; }

        public MatchResolvedInfo LastMatchResolved { get; set; }
        public MatchResultInfo LastMatchResult { get; set; }

        public bool HasCompletedFirstPlay { get; set; }
        public bool SoftPopupsAllowed { get; set; } = true;

        public void RememberQueue(PlayRequest request)
        {
            if (request == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(request.ModeId))
            {
                LastModeId = request.ModeId;
            }

            if (request.TeamSize > 0)
            {
                LastTeamSize = request.TeamSize;
            }

            if (!string.IsNullOrEmpty(request.ChefId))
            {
                LastChefId = request.ChefId;
            }
        }

        public PlayRequest BuildRememberedPlayRequest()
        {
            return new PlayRequest
            {
                ModeId = LastModeId,
                TeamSize = LastTeamSize,
                ChefId = LastChefId
            };
        }
    }
}
