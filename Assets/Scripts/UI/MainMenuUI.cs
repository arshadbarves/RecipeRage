using System;
using RecipeRage.Core.GameFramework.State;
using RecipeRage.Core.GameFramework.State.States;
using RecipeRage.Core.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RecipeRage.UI
{
    /// <summary>
    /// Manages the main menu UI.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private GameObject _creditsPanel;
        
        [Header("Main Menu Buttons")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _creditsButton;
        [SerializeField] private Button _quitButton;
        
        [Header("Settings Panel")]
        [SerializeField] private Slider _musicVolumeSlider;
        [SerializeField] private Slider _sfxVolumeSlider;
        [SerializeField] private Toggle _fullscreenToggle;
        [SerializeField] private TMP_Dropdown _resolutionDropdown;
        [SerializeField] private TMP_Dropdown _qualityDropdown;
        [SerializeField] private Button _settingsBackButton;
        
        [Header("Credits Panel")]
        [SerializeField] private Button _creditsBackButton;
        
        [Header("Player Info")]
        [SerializeField] private TMP_InputField _playerNameInput;
        [SerializeField] private TextMeshProUGUI _playerLevelText;
        [SerializeField] private TextMeshProUGUI _playerCoinsText;
        [SerializeField] private TextMeshProUGUI _playerGemsText;
        
        /// <summary>
        /// Reference to the game state manager.
        /// </summary>
        private GameStateManager _gameStateManager;
        
        /// <summary>
        /// Reference to the network manager.
        /// </summary>
        private NetworkManager _networkManager;
        
        /// <summary>
        /// Initialize the main menu UI.
        /// </summary>
        private void Awake()
        {
            // Get references
            _gameStateManager = GameStateManager.Instance;
            _networkManager = NetworkManager.Instance;
            
            // Set up button listeners
            SetupButtonListeners();
            
            // Hide all panels except main
            ShowMainPanel();
        }
        
        /// <summary>
        /// Set up button click listeners.
        /// </summary>
        private void SetupButtonListeners()
        {
            // Main menu buttons
            if (_playButton != null) _playButton.onClick.AddListener(OnPlayButtonClicked);
            if (_settingsButton != null) _settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            if (_creditsButton != null) _creditsButton.onClick.AddListener(OnCreditsButtonClicked);
            if (_quitButton != null) _quitButton.onClick.AddListener(OnQuitButtonClicked);
            
            // Settings panel
            if (_settingsBackButton != null) _settingsBackButton.onClick.AddListener(OnSettingsBackButtonClicked);
            
            // Credits panel
            if (_creditsBackButton != null) _creditsBackButton.onClick.AddListener(OnCreditsBackButtonClicked);
        }
        
        /// <summary>
        /// Show only the main panel.
        /// </summary>
        private void ShowMainPanel()
        {
            if (_mainPanel != null) _mainPanel.SetActive(true);
            if (_settingsPanel != null) _settingsPanel.SetActive(false);
            if (_creditsPanel != null) _creditsPanel.SetActive(false);
        }
        
        /// <summary>
        /// Show only the settings panel.
        /// </summary>
        private void ShowSettingsPanel()
        {
            if (_mainPanel != null) _mainPanel.SetActive(false);
            if (_settingsPanel != null) _settingsPanel.SetActive(true);
            if (_creditsPanel != null) _creditsPanel.SetActive(false);
        }
        
        /// <summary>
        /// Show only the credits panel.
        /// </summary>
        private void ShowCreditsPanel()
        {
            if (_mainPanel != null) _mainPanel.SetActive(false);
            if (_settingsPanel != null) _settingsPanel.SetActive(false);
            if (_creditsPanel != null) _creditsPanel.SetActive(true);
        }
        
        /// <summary>
        /// Handle play button click.
        /// </summary>
        private void OnPlayButtonClicked()
        {
            Debug.Log("[MainMenuUI] Play button clicked");
            
            // Save player name if provided
            if (_playerNameInput != null && !string.IsNullOrEmpty(_playerNameInput.text))
            {
                PlayerPrefs.SetString("PlayerName", _playerNameInput.text);
                PlayerPrefs.Save();
            }
            
            // Transition to matchmaking state
            if (_gameStateManager != null)
            {
                _gameStateManager.ChangeState(new MatchmakingState());
            }
        }
        
        /// <summary>
        /// Handle settings button click.
        /// </summary>
        private void OnSettingsButtonClicked()
        {
            Debug.Log("[MainMenuUI] Settings button clicked");
            ShowSettingsPanel();
        }
        
        /// <summary>
        /// Handle credits button click.
        /// </summary>
        private void OnCreditsButtonClicked()
        {
            Debug.Log("[MainMenuUI] Credits button clicked");
            ShowCreditsPanel();
        }
        
        /// <summary>
        /// Handle quit button click.
        /// </summary>
        private void OnQuitButtonClicked()
        {
            Debug.Log("[MainMenuUI] Quit button clicked");
            
            // Quit the application
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
        
        /// <summary>
        /// Handle settings back button click.
        /// </summary>
        private void OnSettingsBackButtonClicked()
        {
            Debug.Log("[MainMenuUI] Settings back button clicked");
            
            // Save settings
            SaveSettings();
            
            // Return to main panel
            ShowMainPanel();
        }
        
        /// <summary>
        /// Handle credits back button click.
        /// </summary>
        private void OnCreditsBackButtonClicked()
        {
            Debug.Log("[MainMenuUI] Credits back button clicked");
            ShowMainPanel();
        }
        
        /// <summary>
        /// Save settings to PlayerPrefs.
        /// </summary>
        private void SaveSettings()
        {
            // Save music volume
            if (_musicVolumeSlider != null)
            {
                PlayerPrefs.SetFloat("MusicVolume", _musicVolumeSlider.value);
            }
            
            // Save SFX volume
            if (_sfxVolumeSlider != null)
            {
                PlayerPrefs.SetFloat("SFXVolume", _sfxVolumeSlider.value);
            }
            
            // Save fullscreen setting
            if (_fullscreenToggle != null)
            {
                PlayerPrefs.SetInt("Fullscreen", _fullscreenToggle.isOn ? 1 : 0);
                Screen.fullScreen = _fullscreenToggle.isOn;
            }
            
            // Save resolution
            if (_resolutionDropdown != null)
            {
                PlayerPrefs.SetInt("Resolution", _resolutionDropdown.value);
                // Apply resolution (would need to map dropdown index to actual resolution)
            }
            
            // Save quality
            if (_qualityDropdown != null)
            {
                PlayerPrefs.SetInt("Quality", _qualityDropdown.value);
                QualitySettings.SetQualityLevel(_qualityDropdown.value);
            }
            
            // Save all settings
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// Load settings from PlayerPrefs.
        /// </summary>
        private void LoadSettings()
        {
            // Load music volume
            if (_musicVolumeSlider != null)
            {
                _musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
            }
            
            // Load SFX volume
            if (_sfxVolumeSlider != null)
            {
                _sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
            }
            
            // Load fullscreen setting
            if (_fullscreenToggle != null)
            {
                _fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
            }
            
            // Load resolution
            if (_resolutionDropdown != null)
            {
                _resolutionDropdown.value = PlayerPrefs.GetInt("Resolution", 0);
            }
            
            // Load quality
            if (_qualityDropdown != null)
            {
                _qualityDropdown.value = PlayerPrefs.GetInt("Quality", QualitySettings.GetQualityLevel());
            }
        }
        
        /// <summary>
        /// Initialize the UI when enabled.
        /// </summary>
        private void OnEnable()
        {
            // Load settings
            LoadSettings();
            
            // Load player name
            if (_playerNameInput != null)
            {
                _playerNameInput.text = PlayerPrefs.GetString("PlayerName", "Player");
            }
            
            // Load player stats (in a real game, these would come from a player data service)
            if (_playerLevelText != null)
            {
                _playerLevelText.text = $"Level: {PlayerPrefs.GetInt("PlayerLevel", 1)}";
            }
            
            if (_playerCoinsText != null)
            {
                _playerCoinsText.text = $"Coins: {PlayerPrefs.GetInt("PlayerCoins", 0)}";
            }
            
            if (_playerGemsText != null)
            {
                _playerGemsText.text = $"Gems: {PlayerPrefs.GetInt("PlayerGems", 0)}";
            }
        }
    }
}
