using System;
using UnityEngine;

namespace Core.Camera
{
    /// <summary>
    /// Interface for camera control during gameplay.
    /// Follows Interface Segregation Principle - focused on camera operations only.
    /// </summary>
    public interface ICameraController : IDisposable
    {
        /// <summary>
        /// Initialize the camera system
        /// </summary>
        void Initialize();

        /// <summary>
        /// Set the target for the camera to follow
        /// </summary>
        /// <param name="target">Transform to follow (usually local player)</param>
        void SetFollowTarget(Transform target);

        /// <summary>
        /// Clear the current follow target
        /// </summary>
        void ClearFollowTarget();

        /// <summary>
        /// Set the arena bounds to constrain camera movement
        /// </summary>
        /// <param name="bounds">Bounds of the playable area</param>
        void SetArenaBounds(Bounds bounds);

        /// <summary>
        /// Trigger a camera shake effect
        /// </summary>
        /// <param name="intensity">Shake intensity (0-1)</param>
        /// <param name="duration">Duration in seconds</param>
        void Shake(float intensity, float duration);

        /// <summary>
        /// Set camera zoom level
        /// </summary>
        /// <param name="zoomLevel">Zoom multiplier (1.0 = default)</param>
        /// <param name="duration">Transition duration in seconds</param>
        void SetZoom(float zoomLevel, float duration = 0.3f);

        /// <summary>
        /// Enable or disable camera following
        /// </summary>
        void SetFollowEnabled(bool enabled);

        /// <summary>
        /// Get the main camera
        /// </summary>
        UnityEngine.Camera MainCamera { get; }

        /// <summary>
        /// Check if camera is initialized
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Update camera system (called from GameplayState)
        /// </summary>
        void Update(float deltaTime);
    }
}
