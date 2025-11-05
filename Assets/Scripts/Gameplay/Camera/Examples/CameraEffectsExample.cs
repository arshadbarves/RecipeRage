using UnityEngine;

namespace Gameplay.Camera.Examples
{
    /// <summary>
    /// Example script demonstrating camera effects usage.
    /// Attach to any GameObject in the Game scene to test camera features.
    /// </summary>
    public class CameraEffectsExample : MonoBehaviour
    {
        [Header("Test Controls")]
        [SerializeField] private KeyCode _shakeKey = KeyCode.Alpha1;
        [SerializeField] private KeyCode _zoomInKey = KeyCode.Alpha2;
        [SerializeField] private KeyCode _zoomOutKey = KeyCode.Alpha3;
        [SerializeField] private KeyCode _resetZoomKey = KeyCode.Alpha4;

        [Header("Effect Settings")]
        [SerializeField] private float _shakeIntensity = 0.5f;
        [SerializeField] private float _shakeDuration = 0.3f;
        [SerializeField] private float _zoomInLevel = 0.8f;
        [SerializeField] private float _zoomOutLevel = 1.5f;
        [SerializeField] private float _zoomDuration = 0.5f;

        private void Update()
        {
            // Get camera controller
            var camera = GameplayContext.CameraController;
            if (camera == null || !camera.IsInitialized)
                return;

            // Test shake
            if (Input.GetKeyDown(_shakeKey))
            {
                camera.Shake(_shakeIntensity, _shakeDuration);
                Debug.Log($"Camera shake triggered: Intensity={_shakeIntensity}, Duration={_shakeDuration}");
            }

            // Test zoom in
            if (Input.GetKeyDown(_zoomInKey))
            {
                camera.SetZoom(_zoomInLevel, _zoomDuration);
                Debug.Log($"Camera zoom in: Level={_zoomInLevel}");
            }

            // Test zoom out
            if (Input.GetKeyDown(_zoomOutKey))
            {
                camera.SetZoom(_zoomOutLevel, _zoomDuration);
                Debug.Log($"Camera zoom out: Level={_zoomOutLevel}");
            }

            // Test reset zoom
            if (Input.GetKeyDown(_resetZoomKey))
            {
                camera.SetZoom(1.0f, _zoomDuration);
                Debug.Log("Camera zoom reset to default");
            }
        }

        private void OnGUI()
        {
            // Display controls
            GUILayout.BeginArea(new Rect(10, 10, 300, 150));
            GUILayout.Label("Camera Effects Test Controls:");
            GUILayout.Label($"[{_shakeKey}] - Shake Camera");
            GUILayout.Label($"[{_zoomInKey}] - Zoom In");
            GUILayout.Label($"[{_zoomOutKey}] - Zoom Out");
            GUILayout.Label($"[{_resetZoomKey}] - Reset Zoom");
            GUILayout.EndArea();
        }
    }
}
