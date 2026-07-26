using Unity.Netcode;
using UnityEngine;

namespace RecipeRage.Net
{
    /// <summary>
    /// Server holds the placed ingredient; chop taps arrive via RPC and progress
    /// replicates to clients. Follows the NetworkCookingStation wrapper pattern.
    /// </summary>
    [RequireComponent(typeof(CuttingStation))]
    public sealed class NetworkCuttingStation : NetworkBehaviour
    {
        public readonly NetworkVariable<float> Progress = new NetworkVariable<float>(
            0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public readonly NetworkVariable<bool> HasIngredient = new NetworkVariable<bool>(
            false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public CuttingStation Station { get; private set; }

        private void Awake()
        {
            Station = GetComponent<CuttingStation>();
        }

        private void Update()
        {
            if (!IsServer)
            {
                return;
            }

            Progress.Value = Station.Progress01;
            HasIngredient.Value = Station.HasIngredient;
        }

        [ServerRpc(RequireOwnership = false)]
        public void InteractServerRpc(ulong playerNetworkId)
        {
            if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkId, out var playerObject))
            {
                var player = playerObject.GetComponent<PlayerController>();
                if (player != null && Station.CanInteract(player))
                {
                    Station.Interact(player);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void ChopTapServerRpc()
        {
            Station.ChopTapFromNetwork();
        }
    }
}
