using Unity.Netcode;
using UnityEngine;

namespace RecipeRage.Net
{
    /// <summary>
    /// Server owns the CookingStation simulation; phase + progress replicate so
    /// every client renders identical progress bars and off-screen indicators.
    /// </summary>
    [RequireComponent(typeof(CookingStation))]
    public sealed class NetworkCookingStation : NetworkBehaviour
    {
        public readonly NetworkVariable<float> Progress = new NetworkVariable<float>(
            0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public readonly NetworkVariable<byte> Phase = new NetworkVariable<byte>(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public CookingStation Station { get; private set; }

        private void Awake()
        {
            Station = GetComponent<CookingStation>();
        }

        public override void OnNetworkSpawn()
        {
            Station.LocalTickEnabled = IsServer;
            var registry = FindFirstObjectByType<MatchRuntimeRegistry>(); // registry itself is a scene singleton placed in the map
            registry?.Register(this);
        }

        public override void OnNetworkDespawn()
        {
            var registry = FindFirstObjectByType<MatchRuntimeRegistry>();
            registry?.Unregister(this);
        }

        private void Update()
        {
            if (!IsServer)
            {
                return;
            }

            Station.Tick(Time.deltaTime);
            Progress.Value = Station.Progress01;
            Phase.Value = (byte)(Station.IsBurning ? 2 : Station.HasReadyItem ? 1 : Station.IsActive ? 1 : 0);
        }

        [ServerRpc(RequireOwnership = false)]
        public void InteractServerRpc(ulong playerNetworkId)
        {
            if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkId, out var playerObject))
            {
                var player = playerObject.GetComponent<PlayerController>();
                if (player != null)
                {
                    Station.ServerInteract(player);
                }
            }
        }
    }
}
