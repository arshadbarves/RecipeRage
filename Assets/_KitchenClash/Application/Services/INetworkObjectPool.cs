using Unity.Netcode;
using UnityEngine;

namespace KitchenClash.Application.Services
{
    public interface INetworkObjectPool
    {
        NetworkObject Get(GameObject prefab, Vector3 position, Quaternion rotation);
        void Return(NetworkObject networkObject);
        void Prewarm(GameObject prefab, int count);
        void Clear();
    }
}
