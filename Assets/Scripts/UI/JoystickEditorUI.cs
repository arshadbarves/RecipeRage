using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class JoystickEditorUI : MonoBehaviour
    {
        private UIDocument _uiDocument;
        private VisualElement _root;
        
        private Slider _joystickSizeSlider;
        private Slider _joystickOpacitySlider;
        private Slider _deadZoneSlider;
        private Toggle _fixedJoystickToggle;
        
        private VisualElement _leftJoystick;
        private VisualElement _rightJoystick;
        
        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
            if (_uiDocument == null)
            {
                Debug.LogError("[JoystickEditorUI] UIDocument component not found");
                return;
            }
            
            _uiDocument.rootVisualElement.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }
        
        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            _uiDocument.rootVisualElement.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            
            _root = _uiDocument.rootVisualElement;
            
            _joystickSizeSlider = _root.Q<Slider>("joystick-size");
            _joystickOpacitySlider = _root.Q<Slider>("joystick-opacity");
            _deadZoneSlider = _root.Q<Slider>("dead-zone");
            _fixedJoystickToggle = _root.Q<Toggle>("fixed-joystick");
            
            _leftJoystick = _root.Q<VisualElement>("left-joystick");
            _rightJoystick = _root.Q<VisualElement>("right-joystick");
            
            SetupButtons();
            LoadJoystickSettings();
            SetupValueChangeCallbacks();
        }
        
        private void SetupButtons()
        {
            Button closeButton = _root.Q<Button>("close-button");
            Button resetButton = _root.Q<Button>("reset-button");
            Button saveButton = _root.Q<Button>("save-button");
            
            if (closeButton != null) closeButton.clicked += OnCloseClicked;
            if (resetButton != null) resetButton.clicked += OnResetClicked;
            if (saveButton != null) saveButton.clicked += OnSaveClicked;
        }
        
        private void SetupValueChangeCallbacks()
        {
            if (_joystickSizeSlider != null)
            {
                _joystickSizeSlider.RegisterValueChangedCallback(evt => UpdateJoystickSize(evt.newValue));
            }
            
            if (_joystickOpacitySlider != null)
            {
                _joystickOpacitySlider.RegisterValueChangedCallback(evt => UpdateJoystickOpacity(evt.newValue));
            }
            
            if (_deadZoneSlider != null)
            {
                _deadZoneSlider.RegisterValueChangedCallback(evt => 
                {
                    Debug.Log($"[JoystickEditorUI] Dead zone: {evt.newValue}");
                });
            }
            
            if (_fixedJoystickToggle != null)
            {
                _fixedJoystickToggle.RegisterValueChangedCallback(evt => 
                {
                    Debug.Log($"[JoystickEditorUI] Fixed joystick: {evt.newValue}");
                });
            }
        }
        
        private void UpdateJoystickSize(float size)
        {
            if (_leftJoystick != null)
            {
                _leftJoystick.style.width = 150 * size;
                _leftJoystick.style.height = 150 * size;
            }
            
            if (_rightJoystick != null)
            {
                _rightJoystick.style.width = 150 * size;
                _rightJoystick.style.height = 150 * size;
            }
        }
        
        private void UpdateJoystickOpacity(float opacity)
        {
            if (_leftJoystick != null)
            {
                _leftJoystick.style.opacity = opacity;
            }
            
            if (_rightJoystick != null)
            {
                _rightJoystick.style.opacity = opacity;
            }
        }
        
        private void LoadJoystickSettings()
        {
            if (_joystickSizeSlider != null)
                _joystickSizeSlider.value = PlayerPrefs.GetFloat("JoystickSize", 1.0f);
            
            if (_joystickOpacitySlider != null)
                _joystickOpacitySlider.value = PlayerPrefs.GetFloat("JoystickOpacity", 0.7f);
            
            if (_deadZoneSlider != null)
                _deadZoneSlider.value = PlayerPrefs.GetFloat("JoystickDeadZone", 0.1f);
            
            if (_fixedJoystickToggle != null)
                _fixedJoystickToggle.value = PlayerPrefs.GetInt("JoystickFixed", 0) == 1;
        }
        
        private void OnCloseClicked()
        {
            Debug.Log("[JoystickEditorUI] Closing editor");
            gameObject.SetActive(false);
        }
        
        private void OnResetClicked()
        {
            Debug.Log("[JoystickEditorUI] Resetting to defaults");
            
            if (_joystickSizeSlider != null) _joystickSizeSlider.value = 1.0f;
            if (_joystickOpacitySlider != null) _joystickOpacitySlider.value = 0.7f;
            if (_deadZoneSlider != null) _deadZoneSlider.value = 0.1f;
            if (_fixedJoystickToggle != null) _fixedJoystickToggle.value = false;
        }
        
        private void OnSaveClicked()
        {
            Debug.Log("[JoystickEditorUI] Saving settings");
            
            if (_joystickSizeSlider != null)
                PlayerPrefs.SetFloat("JoystickSize", _joystickSizeSlider.value);
            
            if (_joystickOpacitySlider != null)
                PlayerPrefs.SetFloat("JoystickOpacity", _joystickOpacitySlider.value);
            
            if (_deadZoneSlider != null)
                PlayerPrefs.SetFloat("JoystickDeadZone", _deadZoneSlider.value);
            
            if (_fixedJoystickToggle != null)
                PlayerPrefs.SetInt("JoystickFixed", _fixedJoystickToggle.value ? 1 : 0);
            
            PlayerPrefs.Save();
            
            gameObject.SetActive(false);
        }
    }
}
