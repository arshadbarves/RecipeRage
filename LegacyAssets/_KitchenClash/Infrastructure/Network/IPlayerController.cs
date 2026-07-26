using Unity.Netcode;
using Playcenter.Services;

namespace KitchenClash.Infrastructure.Network
{
    public interface IPlayerController
    {
        ulong OwnerClientId { get; }
        NetworkObject NetworkObject { get; }
        bool IsLocalPlayer { get; }
        int TeamId { get; }
        void SetTeam(int teamId);
    }
}
