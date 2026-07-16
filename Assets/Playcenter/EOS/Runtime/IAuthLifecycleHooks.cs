namespace Playcenter.EOS
{
    /// <summary>
    /// Game-side side effects after auth (events, settings). Optional no-op allowed.
    /// </summary>
    public interface IAuthLifecycleHooks
    {
        void OnLoginSucceeded(string productUserId, string displayName, bool isGuest, string loginMethod);
        void OnLogout(string productUserId);
    }
}
