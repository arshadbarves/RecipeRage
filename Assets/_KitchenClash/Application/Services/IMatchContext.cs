namespace KitchenClash.Application.Services
{
    public interface IMatchContext
    {
        void Refresh();
        void ShutdownNetworkSession();
        bool IsHost { get; }
        bool IsServer { get; }
        bool IsClient { get; }
    }
}
