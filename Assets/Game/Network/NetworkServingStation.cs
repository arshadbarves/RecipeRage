using Unity.Netcode;
using UnityEngine;

namespace RecipeRage.Net
{
    /// <summary>
    /// Routes plate validation through the server's NetworkMatch. Same wrapper
    /// pattern as the other network stations.
    /// </summary>
    [RequireComponent(typeof(ServingStation))]
    public sealed class NetworkServingStation : NetworkBehaviour
    {
        public ServingStation Station { get; private set; }

        private void Awake()
        {
            Station = GetComponent<ServingStation>();
        }

        [ServerRpc(RequireOwnership = false)]
        public void ServePlateServerRpc(ulong playerNetworkId, int teamId)
        {
            if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkId, out var playerObject))
            {
                return;
            }

            var player = playerObject.GetComponent<PlayerController>();
            if (player == null || !player.Carry.HasPlate)
            {
                return;
            }

            var plate = player.Carry.ReleasePlate();
            var match = FindFirstObjectByType<NetworkMatch>(); // one per match scene
            if (match != null)
            {
                match.ServerServePlate(teamId, plate);
            }
            else
            {
                // No network match (offline) — hand the plate back
                player.Carry.TakePlate(plate);
            }
        }
    }
}
