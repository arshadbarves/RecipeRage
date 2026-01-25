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

    /// <summary>
    /// Event fired when any player (including remote players) spawns.
    /// Used for Brawl Stars-style camera target group tracking.
    /// </summary>
    public class PlayerSpawnedEvent
    {
        public string PlayerId { get; set; }
        public Transform PlayerTransform { get; set; }
        public GameObject PlayerObject { get; set; }
        public bool IsLocalPlayer { get; set; }
    }

    /// <summary>
    /// Event fired when any player despawns.
    /// </summary>
    public class PlayerDespawnedEvent
    {
        public string PlayerId { get; set; }
        public Transform PlayerTransform { get; set; }
    }

    // ============================================
    // CAMERA EVENTS
    // ============================================

    /// <summary>
    /// Event fired to trigger camera shake effect.
    /// </summary>
    public class CameraShakeEvent
    {
        public float Intensity { get; }
        public float Duration { get; }

        public CameraShakeEvent(float intensity, float duration)
        {
            Intensity = intensity;
            Duration = duration;
        }
    }
}
