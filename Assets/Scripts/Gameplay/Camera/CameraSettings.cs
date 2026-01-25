using UnityEngine;

namespace Gameplay.Camera
{
    [CreateAssetMenu(fileName = "CameraSettings", menuName = "RecipeRage/Camera/Settings")]
    public class CameraSettings : ScriptableObject
    {
        [Header("Camera Perspective")]
        [Range(20f, 90f)] public float cameraAngle = 90f;
        public bool useOrthographic = false;
        [Range(5f, 20f)] public float orthographicSize = 8f;
        [Range(40f, 90f)] public float fieldOfView = 60f;

        [Header("Position")]
        [Range(5f, 50f)] public float cameraHeight = 20f;
        [Range(0f, 30f)] public float cameraDistance = 0f;

        [Header("Follow")]
        [Range(0.01f, 1f)] public float followSmoothTime = 0.2f;
        public Vector3 followOffset = new Vector3(0f, 20f, 0f);

        [Header("Zoom")]
        [Range(0.5f, 1f)] public float minZoom = 0.8f;
        [Range(1f, 2f)] public float maxZoom = 1.5f;
        [Range(0.5f, 2f)] public float defaultZoom = 1f;

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
