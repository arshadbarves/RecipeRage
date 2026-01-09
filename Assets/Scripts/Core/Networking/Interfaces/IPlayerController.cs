using Unity.Netcode;

namespace Core.Core.Networking.Interfaces
{
    public interface IPlayerController
    {
        ulong OwnerClientId { get; }
        NetworkObject NetworkObject { get; }
        bool IsLocalPlayer { get; }
    }
}
