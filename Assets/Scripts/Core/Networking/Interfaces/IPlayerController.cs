using Unity.Netcode;

namespace Core.Networking.Interfaces
{
    public interface IPlayerController
    {
        ulong OwnerClientId { get; }
        NetworkObject NetworkObject { get; }
        bool IsLocalPlayer { get; }

        /// <summary>
        /// The team ID of the player.
        /// </summary>
        int TeamId { get; }

        /// <summary>
        /// Set the player's team ID (Server Only).
        /// </summary>
        void SetTeam(int teamId);
    }
}
