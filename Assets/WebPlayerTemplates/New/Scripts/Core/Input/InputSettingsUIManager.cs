using System.Collections.Generic;
using Core.Input.Controls;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Input
{
    public class InputSettingsUIManager : MonoBehaviour
    {
        [SerializeField] private InputManager inputManager;
        [SerializeField] private InputConfigurationManager configManager;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Toggle editModeToggle;
        [SerializeField] private Slider sensitivitySlider;

        private bool _isEditMode;

        private void Start()
        {
            editModeToggle.onValueChanged.AddListener(OnEditModeChanged);
            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);

            LoadSettings();
        }

        private void LoadSettings()
        {
            float sensitivity = PlayerPrefs.GetFloat("TouchSensitivity", 1f);
            sensitivitySlider.value = sensitivity;
        }

        private void OnEditModeChanged(bool isEdit)
        {
            _isEditMode = isEdit;
            EnableControlEditing(isEdit);
        }

        private void OnSensitivityChanged(float value)
        {
            PlayerPrefs.SetFloat("TouchSensitivity", value);
            PlayerPrefs.Save();
        }

        private void EnableControlEditing(bool enable)
        {
            IEnumerable<InputControl> controls = inputManager.GetControls<InputControl>();
            foreach (InputControl control in controls)
            {
                // control.SetDraggable(enable);
            }
        }

        public void SaveLayout()
        {
            inputManager.SaveControlLayout();
        }

        public void ResetToDefaults()
        {
            configManager.ResetToDefaults();
            inputManager.LoadControlLayout();
        }
    }
}