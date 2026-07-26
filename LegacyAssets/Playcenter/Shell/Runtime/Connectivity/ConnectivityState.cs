namespace Playcenter.Shell
{
    /// <summary>
    /// Brawl-class connectivity state machine (online / offline menu / offline match / host dropped).
    /// </summary>
    public enum ConnectivityState
    {
        Online,
        OfflineMenu,
        OfflineMatch,
        HostDropped
    }
}
