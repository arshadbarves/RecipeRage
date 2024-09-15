using System.Collections.Generic;
using UnityEngine;

namespace Core.TouchInputSystem
{
    public class TouchInputManager : MonoBehaviour
    {
        [SerializeField] private List<TouchControlConfig> controlConfigs;
        [SerializeField] private Canvas targetCanvas;

        private readonly Dictionary<string, BaseTouchControl> _controls = new Dictionary<string, BaseTouchControl>();
        private TouchLayoutSaver _layoutSaver;

        private void Awake()
        {
            _layoutSaver = new TouchLayoutSaver();
            InitializeControls();
        }

        private void InitializeControls()
        {
            foreach (var config in controlConfigs)
            {
                GameObject controlObject = new GameObject(config.controlName);
                controlObject.transform.SetParent(targetCanvas.transform, false);

                BaseTouchControl control;
                if (config.controlType == TouchControlConfig.TouchControlType.Joystick)
                {
                    control = controlObject.AddComponent<JoystickControl>();
                }
                else
                {
                    control = controlObject.AddComponent<ButtonControl>();
                }

                control.Initialize(config);
                _controls.Add(config.controlName, control);
            }

            _layoutSaver.LoadLayout(_controls);
        }

        public void StartLayoutEditor()
        {
            foreach (var control in _controls.Values)
            {
                control.StartEdit();
            }
        }

        public void StopLayoutEditor()
        {
            foreach (var control in _controls.Values)
            {
                control.StopEdit();
            }

            _layoutSaver.SaveLayout(_controls);
        }

        public Vector2 GetJoystickValue(string joystickName)
        {
            if (_controls.TryGetValue(joystickName, out BaseTouchControl control) &&
                control is JoystickControl joystick)
            {
                return joystick.Value;
            }

            return Vector2.zero;
        }

        public bool GetButtonValue(string buttonName)
        {
            if (_controls.TryGetValue(buttonName, out BaseTouchControl control) && control is ButtonControl button)
            {
                return button.IsPressed;
            }

            return false;
        }
    }
}