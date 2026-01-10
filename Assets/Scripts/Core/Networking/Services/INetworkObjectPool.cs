using Unity.Netcode;
using UnityEngine;

namespace Core.Networking.Services
{
    /// <summary>
    /// Interface for network object pooling.
    /// Follows Object Pool Pattern for efficient network object reuse.
    /// </summary>
    public interface INetworkObjectPool
    {
        /// <summary>
        /// Get a network object from the pool.
        /// </summary>
        /// <param name="prefab">The prefab to get</param>
        /// <param name="position">The position to spawn at</param>
        /// <param name="rotation">The rotation to spawn with</param>
        /// <returns>The network object</returns>
        NetworkObject Get(GameObject prefab, Vector3 position, Quaternion rotation);

        /// <summary>
        /// Return a network object to the pool.
        /// </summary>
        /// <param name="networkObject">The network object to return</param>
        void Return(NetworkObject networkObject);

        /// <summary>
        /// Prewarm the pool with a specific prefab.
        /// </summary>
        /// <param name="prefab">The prefab to prewarm</param>
        /// <param name="count">The number of instances to create</param>
        void Prewarm(GameObject prefab, int count);

        /// <summary>
        /// Clear all pooled objects.
        /// </summary>
        void Clear();
    }
}
