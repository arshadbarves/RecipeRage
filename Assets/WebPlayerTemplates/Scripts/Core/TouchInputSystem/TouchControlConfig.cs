using UnityEngine;

namespace Core.TouchInputSystem
{
    [CreateAssetMenu(fileName = "New Touch Control", menuName = "Input System/Touch Control Config")]
    public class TouchControlConfig : ScriptableObject
    {
        public enum TouchControlType
        {
            Button,
            Joystick,
            Touchpad
        }

        [Header("General")]
        public string controlName;
        public TouchControlType controlType = TouchControlType.Button;
        public string actionName;
        public GameObject controlPrefab;
        public Sprite icon;
        [Header("Button Specific")]
        public Vector2 defaultPosition;
        public Vector2 defaultSize = new Vector2(100, 100);
        [Header("Haptic Feedback")]
        public bool enableHapticFeedback = true;
        [Range(0f, 1f)]
        public float hapticIntensity = 0.5f;
        [Header("Joystick Specific")]
        public float deadZone = 0.1f;
        public float sensitivity = 1f;
    }
}