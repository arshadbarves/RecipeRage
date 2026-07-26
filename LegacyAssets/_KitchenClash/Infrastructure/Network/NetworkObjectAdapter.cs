using Unity.Netcode;

namespace KitchenClash.Infrastructure.Network
{
    /// <summary>
    /// Thin MonoBehaviour wrapper for NGO NetworkBehaviour.
    /// All game state sync goes through this adapter — no other MonoBehaviours
    /// should inherit from NetworkBehaviour directly per GDD v3 architecture rules.
    /// </summary>
    public class NetworkObjectAdapter : NetworkBehaviour
    {
        // Exposes network state to pure C# services via events/delegates
        public new bool IsOwner => base.IsOwner;
        public new bool IsServer => base.IsServer;
        public new bool IsClient => base.IsClient;
        public new ulong OwnerClientId => base.OwnerClientId;

        public event System.Action OnNetworkSpawnEvent;
        public event System.Action OnNetworkDespawnEvent;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            OnNetworkSpawnEvent?.Invoke();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            OnNetworkDespawnEvent?.Invoke();
        }
    }
}
