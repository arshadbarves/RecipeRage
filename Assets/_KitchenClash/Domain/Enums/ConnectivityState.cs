namespace KitchenClash.Domain
{
    /// <summary>
    /// GDD Section 5: Connectivity state machine.
    /// </summary>
    public enum ConnectivityState
    {
        Online,
        OfflineMenu,
        OfflineMatch,
        HostDropped
    }
}
