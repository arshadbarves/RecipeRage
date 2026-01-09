using Unity.Netcode;

namespace Modules.Networking.Interfaces
{
    public interface IPlayerController
    {
        ulong OwnerClientId { get; }
        NetworkObject NetworkObject { get; }
        bool IsLocalPlayer { get; }
    }
}
