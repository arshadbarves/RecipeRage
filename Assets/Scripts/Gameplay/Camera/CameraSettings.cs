using UnityEngine;

namespace Gameplay.Camera
{
    [CreateAssetMenu(fileName = "CameraSettings", menuName = "RecipeRage/Camera/Settings")]
    public class CameraSettings : ScriptableObject
    {
        [Header("Camera - Fixed Third Person")]
        [Range(40f, 90f)] public float fieldOfView = 60f;
        [Range(1f, 10f)] public float cameraHeight = 3f;
        [Range(2f, 15f)] public float cameraDistance = 5f;
        [Range(-2f, 2f)] public float cameraSideOffset = 0f;
        [Range(20f, 90f)] public float cameraAngle = 35f;

        [Header("Follow")]
        [Range(0.01f, 1f)] public float followSmoothTime = 0.3f;

        [Header("Shake")]
        [Range(0.1f, 2f)] public float maxShakeIntensity = 0.5f;
        [Range(10f, 50f)] public float shakeFrequency = 25f;

        [Header("Bounds")]
        [Range(0f, 5f)] public float boundsPadding = 2f;
        public bool enableBounds = true;

        public static CameraSettings CreateDefault()
        {
            return CreateInstance<CameraSettings>();
        }
    }
}
