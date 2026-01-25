using UnityEngine;

namespace Gameplay.Camera
{
    /// <summary>
    /// Attach this to a GameObject with a BoxCollider to mark it as the camera bounds.
    /// The camera will automatically use this collider to constrain movement.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class CameraBoundsMarker : MonoBehaviour
    {
        [Header("Info")]
        [Tooltip("This collider defines the camera movement boundaries")]
        [SerializeField] private bool _showGizmos = true;

        private BoxCollider _collider;

        private void Awake()
        {
            _collider = GetComponent<BoxCollider>();
            _collider.isTrigger = true;
        }

        public BoxCollider GetBoundsCollider()
        {
            if (_collider == null)
            {
                _collider = GetComponent<BoxCollider>();
            }
            return _collider;
        }

        public Bounds GetBounds()
        {
            if (_collider == null)
            {
                _collider = GetComponent<BoxCollider>();
            }
            return _collider.bounds;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!_showGizmos) return;

            var col = GetComponent<BoxCollider>();
            if (col != null)
            {
                Gizmos.color = new Color(0, 1, 0, 0.3f);
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(col.center, col.size);

                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(col.center, col.size);

                UnityEditor.Handles.color = Color.white;
                UnityEditor.Handles.Label(transform.position + Vector3.up * 5, "CAMERA BOUNDS");
            }
        }
#endif
    }
}
