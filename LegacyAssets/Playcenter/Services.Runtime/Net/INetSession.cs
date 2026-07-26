namespace Playcenter.Services
{
    public interface INetSession
    {
        bool IsActive { get; }
        NetRole? ActiveRole { get; }
        System.Threading.Tasks.Task StartAsync(NetRole role, string sessionToken, System.Threading.CancellationToken ct = default);
        System.Threading.Tasks.Task StopAsync(System.Threading.CancellationToken ct = default);
    }
}
