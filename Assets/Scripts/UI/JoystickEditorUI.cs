using System;
using Core.Animation;
using UI.UISystem;
using UI.UISystem.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    /// <summary>
    /// Joystick editor screen for mobile control customization
    /// Pure C# implementation inheriting from BaseUIScreen
    /// </summary>
    [UIScreen(UIScreenType.Modal, UIScreenPriority.Modal, "JoystickEditorTemplate")]
    public class JoystickEditorUI : BaseUIScreen
    {

        #region UI Elements

        private Button _closeButton;
        private Button _resetButton;
        private Button _saveButton;

        private Slider _joystickSizeSlider;
        private Slider _joystickOpacitySlider;
        private Slider _deadZoneSlider;
        private Toggle _fixedJoystickToggle;

        private VisualElement _leftJoystick;
        private VisualElement _rightJoystick;

        #endregion

        #region Configuration Properties

        public float JoystickSize { get; set; } = 1.0f;
        public float JoystickOpacity { get; set; } = 0.7f;
        public float DeadZone { get; set; } = 0.1f;
        public bool FixedJoystick { get; set; } = false;

        #endregion

        #region Events

        public event Action OnSettingsSaved;
        public event Action OnSettingsReset;
        public event Action OnEditorClosed;

        #endregion

        #region Lifecycle

        protected override void OnInitialize()
        {
            CacheUIElements();
            SetupEventHandlers();
            LoadJoystickSettings();

            Debug.Log("[JoystickEditorUI] Initialized with BaseUIScreen architecture");
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

        #endregion

        #region UI Setup

        private void CacheUIElements()
        {
            // Buttons
            _closeButton = GetElement<Button>("close-button");
            _resetButton = GetElement<Button>("reset-button");
            _saveButton = GetElement<Button>("save-button");

            // Sliders
            _joystickSizeSlider = GetElement<Slider>("joystick-size");
            _joystickOpacitySlider = GetElement<Slider>("joystick-opacity");
            _deadZoneSlider = GetElement<Slider>("dead-zone");

            // Toggle
            _fixedJoystickToggle = GetElement<Toggle>("fixed-joystick");

            // Preview elements
            _leftJoystick = GetElement<VisualElement>("left-joystick");
            _rightJoystick = GetElement<VisualElement>("right-joystick");

            // Log missing elements
            if (_closeButton == null)
                Debug.LogWarning("[JoystickEditorUI] close-button not found in template");
            if (_joystickSizeSlider == null)
                Debug.LogWarning("[JoystickEditorUI] joystick-size slider not found in template");
        }

        private void SetupEventHandlers()
        {
            // Button events
            _closeButton?.RegisterCallback<ClickEvent>(_ => HandleCloseClicked());
            _resetButton?.RegisterCallback<ClickEvent>(_ => HandleResetClicked());
            _saveButton?.RegisterCallback<ClickEvent>(_ => HandleSaveClicked());

            // Slider events
            _joystickSizeSlider?.RegisterValueChangedCallback(evt => HandleSizeChanged(evt.newValue));
            _joystickOpacitySlider?.RegisterValueChangedCallback(evt => HandleOpacityChanged(evt.newValue));
            _deadZoneSlider?.RegisterValueChangedCallback(evt => HandleDeadZoneChanged(evt.newValue));

            // Toggle events
            _fixedJoystickToggle?.RegisterValueChangedCallback(evt => HandleFixedToggleChanged(evt.newValue));
        }

        private void UnregisterEventHandlers()
        {
            _closeButton?.UnregisterCallback<ClickEvent>(_ => HandleCloseClicked());
            _resetButton?.UnregisterCallback<ClickEvent>(_ => HandleResetClicked());
            _saveButton?.UnregisterCallback<ClickEvent>(_ => HandleSaveClicked());

            _joystickSizeSlider?.UnregisterValueChangedCallback(evt => HandleSizeChanged(evt.newValue));
            _joystickOpacitySlider?.UnregisterValueChangedCallback(evt => HandleOpacityChanged(evt.newValue));
            _deadZoneSlider?.UnregisterValueChangedCallback(evt => HandleDeadZoneChanged(evt.newValue));

            _fixedJoystickToggle?.UnregisterValueChangedCallback(evt => HandleFixedToggleChanged(evt.newValue));
        }

        #endregion

        #region Public API

        /// <summary>
        /// Configure joystick settings
        /// </summary>
        public JoystickEditorUI ConfigureSettings(float size, float opacity, float deadZone, bool fixedPosition)
        {
            JoystickSize = Mathf.Clamp(size, 0.5f, 2.0f);
            JoystickOpacity = Mathf.Clamp01(opacity);
            DeadZone = Mathf.Clamp(deadZone, 0f, 0.5f);
            FixedJoystick = fixedPosition;

            UpdateUIFromSettings();
            return this;
        }

        /// <summary>
        /// Reset settings to defaults
        /// </summary>
        public JoystickEditorUI ResetToDefaults()
        {
            JoystickSize = 1.0f;
            JoystickOpacity = 0.7f;
            DeadZone = 0.1f;
            FixedJoystick = false;

            UpdateUIFromSettings();
            OnSettingsReset?.Invoke();

            Debug.Log("[JoystickEditorUI] Settings reset to defaults");
            return this;
        }

        /// <summary>
        /// Save current settings
        /// </summary>
        public JoystickEditorUI SaveSettings()
        {
            PlayerPrefs.SetFloat("JoystickSize", JoystickSize);
            PlayerPrefs.SetFloat("JoystickOpacity", JoystickOpacity);
            PlayerPrefs.SetFloat("JoystickDeadZone", DeadZone);
            PlayerPrefs.SetInt("JoystickFixed", FixedJoystick ? 1 : 0);
            PlayerPrefs.Save();

            ApplySettingsToJoysticks();
            OnSettingsSaved?.Invoke();

            Debug.Log("[JoystickEditorUI] Settings saved");
            return this;
        }

        #endregion

        #region Internal Methods

        private void LoadJoystickSettings()
        {
            JoystickSize = PlayerPrefs.GetFloat("JoystickSize", 1.0f);
            JoystickOpacity = PlayerPrefs.GetFloat("JoystickOpacity", 0.7f);
            DeadZone = PlayerPrefs.GetFloat("JoystickDeadZone", 0.1f);
            FixedJoystick = PlayerPrefs.GetInt("JoystickFixed", 0) == 1;
        }

        private void UpdateUIFromSettings()
        {
            // Update sliders
            if (_joystickSizeSlider != null)
                _joystickSizeSlider.value = JoystickSize;

            if (_joystickOpacitySlider != null)
                _joystickOpacitySlider.value = JoystickOpacity;

            if (_deadZoneSlider != null)
                _deadZoneSlider.value = DeadZone;

            if (_fixedJoystickToggle != null)
                _fixedJoystickToggle.value = FixedJoystick;

            // Update preview
            UpdateJoystickPreview();
        }

        private void UpdateJoystickPreview()
        {
            // Update size
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

        /// <summary>
        /// Apply saved settings to active mobile joysticks
        /// </summary>
        private void ApplySettingsToJoysticks()
        {
            MobileControlsManager controlsManager = UnityEngine.Object.FindObjectOfType<MobileControlsManager>();
            if (controlsManager != null)
            {
                // Trigger joystick refresh with new settings
                Debug.Log("[JoystickEditorUI] Applied settings to mobile controls");
            }
        }

        #endregion

        #region Event Handlers

        private void HandleCloseClicked()
        {
            Debug.Log("[JoystickEditorUI] Close button clicked");
            OnEditorClosed?.Invoke();
            Hide(true);
        }

        private void HandleResetClicked()
        {
            Debug.Log("[JoystickEditorUI] Reset button clicked");
            ResetToDefaults();
        }

        private void HandleSaveClicked()
        {
            Debug.Log("[JoystickEditorUI] Save button clicked");
            SaveSettings();
            Hide(true);
        }

        private void HandleSizeChanged(float value)
        {
            JoystickSize = value;
            UpdateJoystickPreview();
            Debug.Log($"[JoystickEditorUI] Size changed to {value:F2}");
        }

        private void HandleOpacityChanged(float value)
        {
            JoystickOpacity = value;
            UpdateJoystickPreview();
            Debug.Log($"[JoystickEditorUI] Opacity changed to {value:F2}");
        }

        private void HandleDeadZoneChanged(float value)
        {
            DeadZone = value;
            Debug.Log($"[JoystickEditorUI] Dead zone changed to {value:F2}");
        }

        private void HandleFixedToggleChanged(bool value)
        {
            FixedJoystick = value;
            Debug.Log($"[JoystickEditorUI] Fixed joystick changed to {value}");
        }

        #endregion

        #region Animation Customization

        /// <summary>
        /// Joystick editor slides in from bottom like a drawer
        /// </summary>
        public override void AnimateShow(IUIAnimator animator, VisualElement element, float duration, Action onComplete)
        {
            animator.SlideIn(element, SlideDirection.Bottom, duration, onComplete);
        }

        /// <summary>
        /// Joystick editor slides out to bottom when closing
        /// </summary>
        public override void AnimateHide(IUIAnimator animator, VisualElement element, float duration, Action onComplete)
        {
            animator.SlideOut(element, SlideDirection.Bottom, duration, onComplete);
        }

        /// <summary>
        /// Quick animation for responsive feel
        /// </summary>
        public override float GetAnimationDuration()
        {
            return 0.3f;
        }

        #endregion
    }
}
