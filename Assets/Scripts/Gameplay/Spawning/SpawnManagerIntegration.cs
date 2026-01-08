using Core.Bootstrap;
using Modules.Logging;
using Gameplay.Spawning;
using UnityEngine;

namespace Gameplay
{
    /// <summary>
    /// Integration helper for SpawnManager with GameplayContext.
    /// Provides easy access to spawn management during gameplay.
    /// </summary>
    public static class SpawnManagerIntegration
    {
        private static SpawnManager _spawnManager;

        /// <summary>
        /// Get or find the SpawnManager in the scene
        /// </summary>
        public static SpawnManager GetSpawnManager()
        {
            if (_spawnManager == null)
            {
                _spawnManager = Object.FindObjectOfType<SpawnManager>();
                
                if (_spawnManager == null)
                {
                    GameLogger.LogWarning("[SpawnManagerIntegration] No SpawnManager found in scene!");
                }
            }

            return _spawnManager;
        }

        /// <summary>
        /// Clear cached reference (call when exiting gameplay)
        /// </summary>
        public static void Clear()
        {
            _spawnManager = null;
        }
    }
}
