using UnityEngine;

namespace Gameplay.Camera
{
    /// <summary>
    /// Helper component for editor-only cameras.
    /// Automatically disables the camera when entering Play Mode,
    /// allowing the runtime camera system to take over.
    /// 
    /// Usage:
    /// 1. Create a Camera in your Game scene for editing
    /// 2. Name it "EditorCamera_TEMP" (or similar)
    /// 3. Attach this script
    /// 4. Camera will disable automatically in Play Mode
    /// </summary>
    public class EditorOnlyCamera : MonoBehaviour
    {
        [Header("Info")]
        [Tooltip("This camera is only for editing. It disables automatically in Play Mode.")]
        [SerializeField] private bool _showInfo = true;

        private void Awake()
        {
            // Disable in Play Mode - runtime camera takes over
            if (Application.isPlaying)
            {
                Debug.Log($"[EditorOnlyCamera] Disabling {gameObject.name} - runtime camera active");
                gameObject.SetActive(false);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Ensure this camera is not tagged as MainCamera
            if (gameObject.CompareTag("MainCamera"))
            {
                Debug.LogWarning($"[EditorOnlyCamera] {gameObject.name} should NOT be tagged as MainCamera. Removing tag.");
                gameObject.tag = "Untagged";
            }
        }

        private void OnDrawGizmos()
        {
            // Draw camera frustum in Scene View
            UnityEditor.Handles.color = Color.yellow;
            var cam = GetComponent<UnityEngine.Camera>();
            if (cam != null)
            {
                UnityEditor.Handles.Label(transform.position + Vector3.up * 2, "EDITOR ONLY\n(Disabled in Play Mode)");
            }
        }
#endif
    }
}
