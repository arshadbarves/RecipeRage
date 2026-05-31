using Unity.Netcode;
using UnityEngine;

namespace KitchenClash.Infrastructure.Network
{
    public interface INetworkGameManager
    {
        void StartGame();
        void EndGame();
        void SpawnPlayer(ulong clientId, Vector3 spawnPosition);
        void DespawnPlayer(ulong clientId);
        NetworkObject SpawnNetworkObject(GameObject prefab, Vector3 position, Quaternion rotation);
        void DespawnNetworkObject(NetworkObject networkObject);
        bool IsGameActive { get; }
    }
}
