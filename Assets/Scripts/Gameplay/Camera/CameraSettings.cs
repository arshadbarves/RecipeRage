using UnityEngine;

namespace Gameplay.Camera
{
    /// <summary>
    /// Camera configuration settings for Brawl Stars-like top-down gameplay.
    /// Can be created as a ScriptableObject for easy tweaking.
    /// </summary>
    [CreateAssetMenu(fileName = "CameraSettings", menuName = "RecipeRage/Camera/Settings")]
    public class CameraSettings : ScriptableObject
    {
        [Header("Camera Perspective")]
        [Tooltip("Camera angle from horizontal (45-60 degrees for top-down)")]
        [Range(30f, 90f)]
        public float cameraAngle = 50f;

        [Tooltip("Use orthographic projection (recommended for Brawl Stars style)")]
        public bool useOrthographic = true;

        [Tooltip("Orthographic size (higher = more zoomed out)")]
        [Range(5f, 20f)]
        public float orthographicSize = 10f;

        [Tooltip("Field of view for perspective camera")]
        [Range(30f, 90f)]
        public float fieldOfView = 60f;

        [Header("Follow Settings")]
        [Tooltip("How quickly camera follows target (lower = smoother)")]
        [Range(0.01f, 1f)]
        public float followSmoothTime = 0.15f;

        [Tooltip("Offset from target position")]
        public Vector3 followOffset = new Vector3(0f, 15f, -10f);

        [Tooltip("Look-ahead distance based on movement")]
        [Range(0f, 5f)]
        public float lookAheadDistance = 2f;

        [Header("Zoom Settings")]
        [Tooltip("Minimum zoom level (zoomed in)")]
        [Range(0.5f, 1f)]
        public float minZoom = 0.8f;

        [Tooltip("Maximum zoom level (zoomed out)")]
        [Range(1f, 2f)]
        public float maxZoom = 1.5f;

        [Tooltip("Default zoom level")]
        [Range(0.5f, 2f)]
        public float defaultZoom = 1f;

        [Tooltip("Zoom transition speed")]
        [Range(0.1f, 2f)]
        public float zoomSpeed = 0.5f;

        [Header("Shake Settings")]
        [Tooltip("Maximum shake intensity")]
        [Range(0.1f, 2f)]
        public float maxShakeIntensity = 0.5f;

        [Tooltip("Shake frequency")]
        [Range(10f, 50f)]
        public float shakeFrequency = 25f;

        [Header("Bounds Settings")]
        [Tooltip("Padding from arena edges")]
        [Range(0f, 5f)]
        public float boundsPadding = 2f;

        [Tooltip("Enable bounds constraint")]
        public bool enableBounds = true;

        [Header("Performance")]
        [Tooltip("Update rate for camera calculations (0 = every frame)")]
        [Range(0f, 0.1f)]
        public float updateInterval = 0f;

        /// <summary>
        /// Create default settings
        /// </summary>
        public static CameraSettings CreateDefault()
        {
            var settings = CreateInstance<CameraSettings>();
            // Default values are already set in field initializers
            return settings;
        }
    }
}
