using UnityEngine;

namespace Gameplay.Shared.Events
{
    // ============================================
    // PLAYER EVENTS
    // ============================================

    /// <summary>
    /// Event fired when the local player object is spawned and ready.
    /// Used by systems that need to track the local player (Camera, UI, etc.)
    /// </summary>
    public class LocalPlayerSpawnedEvent
    {
        public Transform PlayerTransform { get; set; }
        public GameObject PlayerObject { get; set; }
    }

    /// <summary>
    /// Event fired when the local player object is being despawned.
    /// </summary>
    public class LocalPlayerDespawnedEvent
    {
    }
}
