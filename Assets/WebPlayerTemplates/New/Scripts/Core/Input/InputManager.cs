using System;
using System.Collections.Generic;
using Core.Input.Controls;
using Core.Input.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;
using InputControl = Core.Input.Controls.InputControl;

namespace Core.Input
{
    public class InputManager : MonoBehaviour, IStartable
    {
        [SerializeField] private InputControlConfig config;
        [SerializeField] private RectTransform controlsContainer;
        [SerializeField] private GameObject touchJoystickPrefab;
        [SerializeField] private GameObject touchButtonPrefab;

        private readonly Dictionary<string, InputControl> _customControls = new Dictionary<string, InputControl>();
        private TouchButton[] _abilityButtons;
        private TouchJoystick _aimJoystick;
        private Action<InputAction.CallbackContext> _aimPerformed;
        private TouchButton _attackButton;
        private Action<InputAction.CallbackContext> _attackPerformed;
        private PlayerInputActions _inputActions;
        private TouchButton _interactButton;
        private Action<InputAction.CallbackContext> _interactPerformed;

        private TouchJoystick _movementJoystick;

        private Action<InputAction.CallbackContext> _movePerformed;

        private void Awake()
        {
            InitializeControls();
            _inputActions = new PlayerInputActions();
        }

        private void OnEnable()
        {
            _inputActions.Enable();
            RegisterInputCallbacks();
        }

        private void OnDisable()
        {
            UnregisterInputCallbacks();
            _inputActions.Disable();
        }
        public void Start()
        {
            Debug.Log("InputManager Started");
        }

        // Events
        public event Action<Vector2> OnMove;
        public event Action<Vector2> OnAim;
        public event Action<int> OnAbilityTriggered;
        public event Action OnAttack;
        public event Action OnInteract;

        private void InitializeControls()
        {
            // Initialize movement joystick
            _movementJoystick = CreateJoystick("MovementJoystick", config.movementJoystick);
            _movementJoystick.OnInputChanged += HandleMovementInput;

            // Initialize aim joystick
            // _aimJoystick = CreateJoystick("AimJoystick", config.aimJoystick);
            // _aimJoystick.OnInputChanged += HandleAimInput;
            //
            // // Initialize attack button
            // _attackButton = CreateButton("AttackButton", config.attackButton);
            // _attackButton.OnInputStart += _ => OnAttack?.Invoke();

            // Initialize ability buttons
            // _abilityButtons = new TouchButton[config.abilityButtons.Length];
            // for (int i = 0; i < config.abilityButtons.Length; i++)
            // {
            //     int index = i;
            //     _abilityButtons[i] = CreateButton($"AbilityButton{i}", config.abilityButtons[i]);
            //     _abilityButtons[i].OnInputStart += _ => OnAbilityTriggered?.Invoke(index);
            // }

            // Initialize interact button
            _interactButton = CreateButton("InteractButton", config.interactButton);
            _interactButton.OnInputStart += _ => OnInteract?.Invoke();
        }

        private TouchJoystick CreateJoystick(string joystickName, InputControlConfig.JoystickConfig joystickConfig)
        {
            GameObject obj = Instantiate(touchJoystickPrefab, controlsContainer);
            obj.name = joystickName;

            TouchJoystick joystick = obj.GetComponent<TouchJoystick>();
            joystick.Initialize(joystickConfig);

            _customControls.Add(joystickName, joystick);
            return joystick;
        }

        private TouchButton CreateButton(string buttonName, InputControlConfig.ButtonConfig buttonConfig)
        {
            GameObject obj = Instantiate(touchButtonPrefab, controlsContainer);
            obj.name = buttonName;

            TouchButton button = obj.GetComponent<TouchButton>();
            button.Initialize(buttonConfig);

            _customControls.Add(buttonName, button);
            return button;
        }

        private void HandleMovementInput(Vector2 input)
        {
            if (input.magnitude < config.deadZone)
                input = Vector2.zero;

            OnMove?.Invoke(input);
        }

        private void HandleAimInput(Vector2 input)
        {
            if (input.magnitude < config.deadZone)
                input = Vector2.zero;

            OnAim?.Invoke(input);
        }

        public void UpdateAbilityCooldown(int index, float progress)
        {
            if (index >= 0 && index < _abilityButtons.Length)
            {
                _abilityButtons[index].UpdateCooldownVisual(progress);
            }
        }

        public void RegisterCustomControl(string controlName, InputControl control)
        {
            _customControls.TryAdd(controlName, control);
        }

        public T GetControl<T>(string controlName) where T : InputControl
        {
            if (_customControls.TryGetValue(controlName, out InputControl control))
            {
                return control as T;
            }
            return null;
        }

        public void SaveControlLayout()
        {
            Dictionary<string, Vector2> layout = new Dictionary<string, Vector2>();
            foreach (KeyValuePair<string, InputControl> control in _customControls)
            {
                layout[control.Key] = control.Value.GetComponent<RectTransform>().anchoredPosition;
            }

            string json = JsonUtility.ToJson(layout);
            PlayerPrefs.SetString("ControlLayout", json);
            PlayerPrefs.Save();
        }

        public void LoadControlLayout()
        {
            if (PlayerPrefs.HasKey("ControlLayout"))
            {
                string json = PlayerPrefs.GetString("ControlLayout");
                Dictionary<string, Vector2> layout = JsonUtility.FromJson<Dictionary<string, Vector2>>(json);

                foreach (KeyValuePair<string, Vector2> item in layout)
                {
                    if (_customControls.TryGetValue(item.Key, out InputControl control))
                    {
                        control.SetPosition(item.Value);
                    }
                }
            }
        }

        public Vector2 GetMovementInput()
        {
            return _inputActions.Gameplay.Move.ReadValue<Vector2>();
        }

        public IEnumerable<T> GetControls<T>() where T : InputControl
        {
            foreach (InputControl control in _customControls.Values)
            {
                if (control is T typedControl)
                {
                    yield return typedControl;
                }
            }
        }

        private void RegisterInputCallbacks()
        {
            _movePerformed = ctx => HandleMovementInput(ctx.ReadValue<Vector2>());
            _aimPerformed = ctx => HandleAimInput(ctx.ReadValue<Vector2>());
            _interactPerformed = ctx => OnInteract?.Invoke();
            _attackPerformed = ctx => OnAttack?.Invoke();

            _inputActions.Gameplay.Move.performed += _movePerformed;
            _inputActions.Gameplay.Aim.performed += _aimPerformed;
            _inputActions.Gameplay.Interact.performed += _interactPerformed;
            _inputActions.Gameplay.Attack.performed += _attackPerformed;
        }

        private void UnregisterInputCallbacks()
        {
            _inputActions.Gameplay.Move.performed -= _movePerformed;
            _inputActions.Gameplay.Aim.performed -= _aimPerformed;
            _inputActions.Gameplay.Interact.performed -= _interactPerformed;
            _inputActions.Gameplay.Attack.performed -= _attackPerformed;
        }
    }
}