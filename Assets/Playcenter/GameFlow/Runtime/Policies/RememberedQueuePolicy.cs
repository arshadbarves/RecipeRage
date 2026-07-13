namespace Playcenter.GameFlow
{
    /// <summary>
    /// PLAY uses last mode/team/chef when the request is empty.
    /// </summary>
    public static class RememberedQueuePolicy
    {
        public static PlayRequest Resolve(PlayRequest request, FlowContext context)
        {
            if (context == null)
            {
                return request ?? PlayRequest.Empty;
            }

            if (request == null)
            {
                return context.BuildRememberedPlayRequest();
            }

            var resolved = new PlayRequest
            {
                ModeId = string.IsNullOrEmpty(request.ModeId) ? context.LastModeId : request.ModeId,
                TeamSize = request.TeamSize > 0 ? request.TeamSize : context.LastTeamSize,
                ChefId = string.IsNullOrEmpty(request.ChefId) ? context.LastChefId : request.ChefId
            };

            context.RememberQueue(resolved);
            return resolved;
        }
    }
}
