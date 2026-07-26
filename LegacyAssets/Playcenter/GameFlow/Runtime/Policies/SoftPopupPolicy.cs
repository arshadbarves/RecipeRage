namespace Playcenter.GameFlow
{
    /// <summary>
    /// Soft offers never block the first PLAY of a session.
    /// </summary>
    public sealed class SoftPopupPolicy : IPopupPolicyPort
    {
        public bool CanShowSoftPopup(FlowContext context)
        {
            if (context == null)
            {
                return false;
            }

            if (!context.SoftPopupsAllowed)
            {
                return false;
            }

            // First PLAY of the product session is sacred.
            return context.HasCompletedFirstPlay;
        }

        public void OnHomeEntered(FlowContext context)
        {
            // Hook for future: schedule deferred offers after first play.
        }
    }
}
