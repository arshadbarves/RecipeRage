namespace KitchenClash.Domain
{
    /// <summary>Abstraction for player controller, no Unity types.</summary>
    public interface IPlayerController
    {
        string PlayerId { get; }
        bool IsLocalPlayer { get; }
        int TeamIndex { get; }
        string DisplayName { get; }
    }
}
