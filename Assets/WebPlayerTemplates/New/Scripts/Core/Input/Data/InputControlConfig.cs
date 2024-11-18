using System;
using UnityEngine;

namespace Core.Input.Data
{
    [CreateAssetMenu(fileName = "InputControlConfig", menuName = "Input/Control Config")]
    public class InputControlConfig : ScriptableObject
    {
        public JoystickConfig movementJoystick;
        public JoystickConfig aimJoystick;
        public ButtonConfig[] abilityButtons;
        public ButtonConfig attackButton;
        public ButtonConfig interactButton;

        public float deadZone = 0.1f;
        public float tapThreshold = 0.2f;
        public float holdThreshold = 0.5f;
        public float dragThreshold = 20f;
        public bool vibrateOnPress = true;
        [Serializable]
        public class JoystickConfig
        {
            public Sprite backgroundSprite;
            public Sprite handleSprite;
            public float radius = 50f;
            public Vector2 defaultPosition = new Vector2(150, 150);
            public Color normalColor = Color.white;
            public Color pressedColor = Color.gray;
            public bool isDraggable;
            public bool isResizable;
            public Vector2 minMaxSize = new Vector2(50, 300);
        }

        [Serializable]
        public class ButtonConfig
        {
            public Sprite normalSprite;
            public Sprite pressedSprite;
            public Vector2 defaultPosition = new Vector2(150, 150);
            public Vector2 size = new Vector2(80, 80);
            public Color normalColor = Color.white;
            public Color pressedColor = Color.gray;
            public float cooldownIndicatorSize = 0.8f;
            public bool isDraggable;
        }
    }
}