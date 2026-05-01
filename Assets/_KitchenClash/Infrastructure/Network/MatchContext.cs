using KitchenClash.Domain;
using KitchenClash.Application.Services;
using KitchenClash.Infrastructure.Network.Cooking;
using KitchenClash.Infrastructure.Network.Spawning;

using Unity.Netcode;
using UnityEngine;

namespace KitchenClash.Infrastructure.Network
{
    public class MatchContext : IMatchContext, IMatchRuntimeRegistry, System.IDisposable
    {
        private readonly IEventBus _eventBus;

        public bool IsHost => Unity.Netcode.NetworkManager.Singleton != null && Unity.Netcode.NetworkManager.Singleton.IsHost;
        public bool IsServer => Unity.Netcode.NetworkManager.Singleton != null && Unity.Netcode.NetworkManager.Singleton.IsServer;
        public bool IsClient => Unity.Netcode.NetworkManager.Singleton != null && Unity.Netcode.NetworkManager.Singleton.IsClient;
        public ulong? LocalClientId => Unity.Netcode.NetworkManager.Singleton != null ? Unity.Netcode.NetworkManager.Singleton.LocalClientId : null;

        public IKitchenSupportRuntime KitchenSupportRuntime { get; private set; }
        public ScoreManager ScoreManager { get; private set; }
        public NetworkScoreManager NetworkScoreManager { get; private set; }
        public OrderManager OrderManager { get; private set; }
        public RoundTimer RoundTimer { get; private set; }
        public GamePhaseSync GamePhaseSync { get; private set; }
        public MatchResultSync MatchResultSync { get; private set; }

        public PlayerController LocalPlayer { get; private set; }
        public SpawnManager SpawnManager { get; private set; }
        public NetworkManager NetworkManager => Unity.Netcode.NetworkManager.Singleton;
        public IngredientNetworkSpawner IngredientNetworkSpawner { get; private set; }
        public int LocalTeamId => LocalPlayer != null ? LocalPlayer.TeamId : 0;

        public MatchContext(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public void Refresh()
        {
            if (ScoreManager == null) ScoreManager = Object.FindObjectOfType<ScoreManager>();
            if (NetworkScoreManager == null) NetworkScoreManager = Object.FindObjectOfType<NetworkScoreManager>();
            if (OrderManager == null) OrderManager = Object.FindObjectOfType<OrderManager>();
            if (RoundTimer == null) RoundTimer = Object.FindObjectOfType<RoundTimer>();
            if (GamePhaseSync == null) GamePhaseSync = Object.FindObjectOfType<GamePhaseSync>();
            if (MatchResultSync == null) MatchResultSync = Object.FindObjectOfType<MatchResultSync>();
            if (SpawnManager == null) SpawnManager = Object.FindObjectOfType<SpawnManager>();
            if (IngredientNetworkSpawner == null) IngredientNetworkSpawner = Object.FindObjectOfType<IngredientNetworkSpawner>();

            if (KitchenSupportRuntime == null)
            {
                var binder = Object.FindObjectOfType<MatchRuntimeSceneBinder>();
                if (binder != null) KitchenSupportRuntime = binder;
            }
            if (LocalPlayer == null && LocalClientId.HasValue)
            {
                foreach (var pc in Object.FindObjectsOfType<PlayerController>())
                {
                    if (pc.OwnerClientId == LocalClientId.Value)
                    {
                        LocalPlayer = pc;
                        break;
                    }
                }
            }
        }

        public bool TryGetSpawnedObject(ulong networkObjectId, out NetworkObject networkObject)
        {
            networkObject = null;
            if (Unity.Netcode.NetworkManager.Singleton == null || !Unity.Netcode.NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out networkObject))
                return false;
            return networkObject != null;
        }

        public void ShutdownNetworkSession()
        {
            Unity.Netcode.NetworkManager.Singleton?.Shutdown();
            ClearSceneRuntime();
        }

        public void ClearSceneRuntime()
        {
            ScoreManager = null;
            NetworkScoreManager = null;
            OrderManager = null;
            RoundTimer = null;
            GamePhaseSync = null;
            MatchResultSync = null;
            SpawnManager = null;
            IngredientNetworkSpawner = null;

            KitchenSupportRuntime = null;
            LocalPlayer = null;
        }

        public void Dispose() => ClearSceneRuntime();
    }
}
