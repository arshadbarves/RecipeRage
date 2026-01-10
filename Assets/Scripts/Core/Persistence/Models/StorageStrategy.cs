namespace Core.Persistence.Models
{
    /// <summary>
    /// Defines how data should be stored and retrieved.
    /// </summary>
    public enum StorageStrategy
    {
        /// <summary>
        /// Store only in local storage (fast, offline).
        /// Use for: Settings, cache, temporary data.
        /// </summary>
        LocalOnly,

        /// <summary>
        /// Store only in cloud storage (persistent, cross-device).
        /// Use for: Temporary cloud data that doesn't need local cache.
        /// </summary>
        CloudOnly,

        /// <summary>
        /// Store in cloud with local cache (best of both worlds).
        /// Writes: Local immediately + cloud async.
        /// Reads: Cloud first, fallback to local cache.
        /// Use for: Player progress, stats, achievements.
        /// </summary>
        CloudWithCache
    }
}
