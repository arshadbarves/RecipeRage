using Core.Input.Data;
using UnityEngine;

namespace Core.Input
{
    public class InputConfigurationManager : MonoBehaviour
    {
        [SerializeField] private InputControlConfig defaultConfig;
        private InputControlConfig _currentConfig;

        private void Awake()
        {
            LoadConfiguration();
        }

        public void LoadConfiguration()
        {
            string savedConfig = PlayerPrefs.GetString("InputConfig", "");
            if (string.IsNullOrEmpty(savedConfig))
            {
                _currentConfig = Instantiate(defaultConfig);
            }
            else
            {
                _currentConfig = ScriptableObject.CreateInstance<InputControlConfig>();
                JsonUtility.FromJsonOverwrite(savedConfig, _currentConfig);
            }
        }

        public void SaveConfiguration()
        {
            if (_currentConfig != null)
            {
                string config = JsonUtility.ToJson(_currentConfig);
                PlayerPrefs.SetString("InputConfig", config);
                PlayerPrefs.Save();
            }
        }

        public void ResetToDefaults()
        {
            _currentConfig = Instantiate(defaultConfig);
            SaveConfiguration();
        }

        public void UpdateJoystickConfig(string joystickId, InputControlConfig.JoystickConfig config)
        {
            // Update specific joystick configuration
            // Implement the update logic based on joystickId
            SaveConfiguration();
        }

        public void UpdateButtonConfig(string buttonId, InputControlConfig.ButtonConfig config)
        {
            // Update specific button configuration
            // Implement the update logic based on buttonId
            SaveConfiguration();
        }
    }
}