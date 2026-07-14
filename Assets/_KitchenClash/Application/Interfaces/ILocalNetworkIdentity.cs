namespace KitchenClash.Application
{
    /// <summary>
    /// Platform-neutral local multiplayer identity (e.g. EOS Product User Id string).
    /// Network code depends on this port instead of vendor SDK types.
    /// </summary>
    public interface ILocalNetworkIdentity
    {
        /// <summary>Local user id string, or null if not signed in.</summary>
        string LocalUserId { get; }
    }
}
