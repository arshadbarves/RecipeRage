using System;
using Modules.Animation;
using Modules.Logging;
using Modules.UI;
using UI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace UI
{
    /// <summary>
    /// Joystick editor screen for mobile control customization
    /// </summary>
    [UIScreen(UIScreenType.Modal, UIScreenCategory.Modal, "Components/JoystickEditorTemplate")]
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
            _closeButton?.RegisterCallback<ClickEvent>(_ => HandleCloseClicked());
            _resetButton?.RegisterCallback<ClickEvent>(_ => HandleResetClicked());
            _saveButton?.RegisterCallback<ClickEvent>(_ => HandleSaveClicked());
            _joystickSizeSlider?.RegisterValueChangedCallback(evt => HandleSizeChanged(evt.newValue));
            _joystickOpacitySlider?.RegisterValueChangedCallback(evt => HandleOpacityChanged(evt.newValue));
            _deadZoneSlider?.RegisterValueChangedCallback(evt => HandleDeadZoneChanged(evt.newValue));
            _fixedJoystickToggle?.RegisterValueChangedCallback(evt => HandleFixedToggleChanged(evt.newValue));
        }

        private void UnregisterEventHandlers()
        {
            _closeButton?.UnregisterCallback<ClickEvent>(_ => HandleCloseClicked());
            _resetButton?.UnregisterCallback<ClickEvent>(_ => HandleResetClicked());
            _saveButton?.UnregisterCallback<ClickEvent>(_ => HandleSaveClicked());
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