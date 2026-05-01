using Unity.Netcode;

namespace KitchenClash.Application.Services
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
