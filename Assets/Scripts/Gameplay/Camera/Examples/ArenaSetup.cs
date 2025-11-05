using UnityEngine;

namespace Gameplay.Camera.Examples
{
    /// <summary>
    /// Example script for setting up arena bounds for the camera.
    /// Attach to your arena/map GameObject in the Game scene.
    /// </summary>
    public class ArenaSetup : MonoBehaviour
    {
        [Header("Arena Configuration")]
        [Tooltip("Size of the playable arena")]
        [SerializeField] private Vector3 _arenaSize = new Vector3(50f, 0f, 50f);

        [Tooltip("Center position of the arena")]
        [SerializeField] private Vector3 _arenaCenter = Vector3.zero;

        [Header("Visualization")]
        [Tooltip("Show arena bounds in Scene view")]
        [SerializeField] private bool _showGizmos = true;

        [Tooltip("Color of the bounds gizmo")]
        [SerializeField] private Color _gizmoColor = Color.yellow;

        private void Start()
        {
            SetupCameraBounds();
        }

        private void SetupCameraBounds()
        {
            var camera = GameplayContext.CameraController;
            if (camera == null || !camera.IsInitialized)
            {
                Debug.LogWarning("Camera controller not available - bounds not set");
                return;
            }

            // Create bounds
            Bounds arenaBounds = new Bounds(_arenaCenter, _arenaSize);
            
            // Set camera bounds
            camera.SetArenaBounds(arenaBounds);
            
            Debug.Log($"Arena bounds set: Center={_arenaCenter}, Size={_arenaSize}");
        }

        private void OnDrawGizmos()
        {
            if (!_showGizmos)
                return;

            // Draw arena bounds
            Gizmos.color = _gizmoColor;
            Gizmos.DrawWireCube(_arenaCenter, _arenaSize);
        }

        private void OnDrawGizmosSelected()
        {
            if (!_showGizmos)
                return;

            // Draw filled bounds when selected
            Gizmos.color = new Color(_gizmoColor.r, _gizmoColor.g, _gizmoColor.b, 0.1f);
            Gizmos.DrawCube(_arenaCenter, _arenaSize);
        }
    }
}
