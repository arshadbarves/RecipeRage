using System;
using KitchenClash.Presentation.Common;
using Core.Logging;
using UnityEngine;
using UnityEngine.UIElements;

namespace KitchenClash.Presentation.Controls
{
    /// <summary>
    /// Joystick editor screen for mobile control customization
    /// </summary>
    [UIScreen(UIScreenCategory.Modal, "Components/JoystickEditorTemplate")]
    public class JoystickEditorUI : BaseUIScreen
    {
        private Button _closeButton;
        private Button _resetButton;
        private Button _saveButton;

        private Slider _joystickSizeSlider;
        private Slider _joystickOpacitySlider;
        private Slider _deadZoneSlider;
        private Toggle _fixedJoystickToggle;

        private VisualElement _leftJoystick;
        private VisualElement _rightJoystick;
        private EventCallback<ClickEvent> _closeClick;
        private EventCallback<ClickEvent> _resetClick;
        private EventCallback<ClickEvent> _saveClick;
        private EventCallback<ChangeEvent<float>> _sizeChanged;
        private EventCallback<ChangeEvent<float>> _opacityChanged;
        private EventCallback<ChangeEvent<float>> _deadZoneChanged;
        private EventCallback<ChangeEvent<bool>> _fixedToggleChanged;

        public float JoystickSize { get; set; } = 1.0f;
        public float JoystickOpacity { get; set; } = 0.7f;
        public float DeadZone { get; set; } = 0.1f;
        public bool FixedJoystick { get; set; } = false;

        public event Action OnSettingsSaved;
        public event Action OnSettingsReset;
        public event Action OnEditorClosed;

        protected override void OnInitialize()
        {
            CacheUIElements();
            SetupEventHandlers();
            LoadJoystickSettings();
            TransitionType = UITransitionType.SlideUp;
            GameLogger.Log("Initialized");
        }

        protected override void OnShow()
        {
            LoadJoystickSettings();
            UpdateUIFromSettings();
        }

        protected override void OnDispose()
        {
            UnregisterEventHandlers();
        }

        private void CacheUIElements()
        {
            _closeButton = GetElement<Button>("close-button");
            _resetButton = GetElement<Button>("reset-button");
            _saveButton = GetElement<Button>("save-button");
            _joystickSizeSlider = GetElement<Slider>("joystick-size");
            _joystickOpacitySlider = GetElement<Slider>("joystick-opacity");
            _deadZoneSlider = GetElement<Slider>("dead-zone");
            _fixedJoystickToggle = GetElement<Toggle>("fixed-joystick");
            _leftJoystick = GetElement<VisualElement>("left-joystick");
            _rightJoystick = GetElement<VisualElement>("right-joystick");
        }

        private void SetupEventHandlers()
        {
            _closeClick = _ => HandleCloseClicked();
            _resetClick = _ => HandleResetClicked();
            _saveClick = _ => HandleSaveClicked();
            _sizeChanged = evt => HandleSizeChanged(evt.newValue);
            _opacityChanged = evt => HandleOpacityChanged(evt.newValue);
            _deadZoneChanged = evt => HandleDeadZoneChanged(evt.newValue);
            _fixedToggleChanged = evt => HandleFixedToggleChanged(evt.newValue);

            _closeButton?.RegisterCallback(_closeClick);
            _resetButton?.RegisterCallback(_resetClick);
            _saveButton?.RegisterCallback(_saveClick);
            _joystickSizeSlider?.RegisterValueChangedCallback(_sizeChanged);
            _joystickOpacitySlider?.RegisterValueChangedCallback(_opacityChanged);
            _deadZoneSlider?.RegisterValueChangedCallback(_deadZoneChanged);
            _fixedJoystickToggle?.RegisterValueChangedCallback(_fixedToggleChanged);
        }

        private void UnregisterEventHandlers()
        {
            if (_closeClick != null) _closeButton?.UnregisterCallback(_closeClick);
            if (_resetClick != null) _resetButton?.UnregisterCallback(_resetClick);
            if (_saveClick != null) _saveButton?.UnregisterCallback(_saveClick);
            if (_sizeChanged != null) _joystickSizeSlider?.UnregisterValueChangedCallback(_sizeChanged);
            if (_opacityChanged != null) _joystickOpacitySlider?.UnregisterValueChangedCallback(_opacityChanged);
            if (_deadZoneChanged != null) _deadZoneSlider?.UnregisterValueChangedCallback(_deadZoneChanged);
            if (_fixedToggleChanged != null) _fixedJoystickToggle?.UnregisterValueChangedCallback(_fixedToggleChanged);
        }

        public JoystickEditorUI ConfigureSettings(float size, float opacity, float deadZone, bool fixedPosition)
        {
            JoystickSize = Mathf.Clamp(size, 0.5f, 2.0f);
            JoystickOpacity = Mathf.Clamp01(opacity);
            DeadZone = Mathf.Clamp(deadZone, 0f, 0.5f);
            FixedJoystick = fixedPosition;
            UpdateUIFromSettings();
            return this;
        }

        public JoystickEditorUI ResetToDefaults()
        {
            JoystickSize = 1.0f;
            JoystickOpacity = 0.7f;
            DeadZone = 0.1f;
            FixedJoystick = false;
            UpdateUIFromSettings();
            OnSettingsReset?.Invoke();
            return this;
        }

        public JoystickEditorUI SaveSettings()
        {
            PlayerPrefs.SetFloat("JoystickSize", JoystickSize);
            PlayerPrefs.SetFloat("JoystickOpacity", JoystickOpacity);
            PlayerPrefs.SetFloat("JoystickDeadZone", DeadZone);
            PlayerPrefs.SetInt("JoystickFixed", FixedJoystick ? 1 : 0);
            PlayerPrefs.Save();
            OnSettingsSaved?.Invoke();
            return this;
        }

        private void LoadJoystickSettings()
        {
            JoystickSize = PlayerPrefs.GetFloat("JoystickSize", 1.0f);
            JoystickOpacity = PlayerPrefs.GetFloat("JoystickOpacity", 0.7f);
            DeadZone = PlayerPrefs.GetFloat("JoystickDeadZone", 0.1f);
            FixedJoystick = PlayerPrefs.GetInt("JoystickFixed", 0) == 1;
        }

        private void UpdateUIFromSettings()
        {
            if (_joystickSizeSlider != null) _joystickSizeSlider.value = JoystickSize;
            if (_joystickOpacitySlider != null) _joystickOpacitySlider.value = JoystickOpacity;
            if (_deadZoneSlider != null) _deadZoneSlider.value = DeadZone;
            if (_fixedJoystickToggle != null) _fixedJoystickToggle.value = FixedJoystick;
            UpdateJoystickPreview();
        }

        private void UpdateJoystickPreview()
        {
            if (_leftJoystick != null)
            {
                _leftJoystick.style.width = 150 * JoystickSize;
                _leftJoystick.style.height = 150 * JoystickSize;
                _leftJoystick.style.opacity = JoystickOpacity;
            }
            if (_rightJoystick != null)
            {
                _rightJoystick.style.width = 150 * JoystickSize;
                _rightJoystick.style.height = 150 * JoystickSize;
                _rightJoystick.style.opacity = JoystickOpacity;
            }
        }

        private void HandleCloseClicked()
        {
            OnEditorClosed?.Invoke();
            Hide(true);
        }

        private void HandleResetClicked() => ResetToDefaults();
        private void HandleSaveClicked() { SaveSettings(); Hide(true); }
        private void HandleSizeChanged(float value) { JoystickSize = value; UpdateJoystickPreview(); }
        private void HandleOpacityChanged(float value) { JoystickOpacity = value; UpdateJoystickPreview(); }
        private void HandleDeadZoneChanged(float value) { DeadZone = value; }
        private void HandleFixedToggleChanged(bool value) { FixedJoystick = value; }

        public override float GetAnimationDuration() => 0.3f;
    }
}
