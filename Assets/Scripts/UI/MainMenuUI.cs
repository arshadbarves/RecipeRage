using System.Collections.Generic;
using Core.State;
using Core.State.States;
using Core.Networking;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    /// <summary>
    /// Manages the main menu UI using UI Toolkit.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {

        // Credits Panel
        private Button _creditsBackButton;
        private Button _creditsButton;
        private VisualElement _creditsPanel;
        private Toggle _fullscreenToggle;

        /// <summary>
        /// Reference to the game state manager.
        /// </summary>
        private GameStateManager _gameStateManager;

        // Panels
        private VisualElement _mainPanel;

        // Settings Panel
        private Slider _musicVolumeSlider;

        /// <summary>
        /// Reference to the network manager.
        /// </summary>
        private RecipeRageNetworkManager _networkManager;

        // Main Menu Buttons
        private Button _playButton;
        private Label _playerCoinsText;
        private Label _playerGemsText;
        private Label _playerLevelText;

        // Player Info
        private TextField _playerNameInput;
        private DropdownField _qualityDropdown;
        private Button _quitButton;
        private DropdownField _resolutionDropdown;

        /// <summary>
        /// The root visual element.
        /// </summary>
        private VisualElement _root;
        private Button _settingsBackButton;
        private Button _settingsButton;
        private VisualElement _settingsPanel;
        private Slider _sfxVolumeSlider;
        /// <summary>
        /// The UI document component.
        /// </summary>
        private UIDocument _uiDocument;

        /// <summary>
        /// Initialize the main menu UI.
        /// </summary>
        private void Awake()
        {
            // Get references
            _gameStateManager = GameStateManager.Instance;
            _networkManager = RecipeRageNetworkManager.Instance;

            // Get UI Document component
            _uiDocument = GetComponent<UIDocument>();
            if (_uiDocument == null)
            {
                Debug.LogError("[MainMenuUI] UIDocument component not found");
                return;
            }

            // Initialize UI when the document is ready
            _uiDocument.rootVisualElement.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        /// <summary>
        /// Called when the UI geometry is initialized.
        /// </summary>
        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            // Unregister the callback to ensure it's only called once
            _uiDocument.rootVisualElement.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            // Get the root element
            _root = _uiDocument.rootVisualElement;

            // Get panel references
            _mainPanel = _root.Q<VisualElement>("main-panel");
            _settingsPanel = _root.Q<VisualElement>("settings-panel");
            _creditsPanel = _root.Q<VisualElement>("credits-panel");

            // Get button references
            _playButton = _root.Q<Button>("play-button");
            _settingsButton = _root.Q<Button>("settings-button");
            _creditsButton = _root.Q<Button>("credits-button");
            _quitButton = _root.Q<Button>("quit-button");

            // Get navigation buttons
            Button shopButton = _root.Q<Button>("shop-button");
            Button brawlersButton = _root.Q<Button>("brawlers-button");
            Button newsButton = _root.Q<Button>("news-button");
            Button friendsButton = _root.Q<Button>("friends-button");
            Button clubButton = _root.Q<Button>("club-button");
            Button chatButton = _root.Q<Button>("chat-button");

            // Set up navigation button listeners
            if (shopButton != null) shopButton.clicked += () => Debug.Log("Shop clicked");
            if (brawlersButton != null) brawlersButton.clicked += () => Debug.Log("Brawlers clicked");
            if (newsButton != null) newsButton.clicked += () => Debug.Log("News clicked");
            if (friendsButton != null) friendsButton.clicked += () => Debug.Log("Friends clicked");
            if (clubButton != null) clubButton.clicked += () => Debug.Log("Club clicked");
            if (chatButton != null) chatButton.clicked += () => Debug.Log("Chat clicked");

            // Get settings panel references
            _musicVolumeSlider = _root.Q<Slider>("music-volume");
            _sfxVolumeSlider = _root.Q<Slider>("sfx-volume");
            _fullscreenToggle = _root.Q<Toggle>("fullscreen-toggle");
            _resolutionDropdown = _root.Q<DropdownField>("resolution-dropdown");
            _qualityDropdown = _root.Q<DropdownField>("quality-dropdown");
            _settingsBackButton = _root.Q<Button>("settings-back-button");

            // Get credits panel references
            _creditsBackButton = _root.Q<Button>("credits-back-button");

            // Get player info references
            _playerNameInput = _root.Q<TextField>("player-name");
            _playerLevelText = _root.Q<Label>("player-level");
            _playerCoinsText = _root.Q<Label>("player-coins");
            _playerGemsText = _root.Q<Label>("player-gems");

            // Initialize dropdown options
            InitializeDropdowns();

            // Set up button listeners
            SetupButtonListeners();

            // Load settings and player info
            LoadSettings();
            LoadPlayerInfo();

            // Show main panel by default
            ShowMainPanel();
        }

        /// <summary>
        /// Initialize dropdown options.
        /// </summary>
        private void InitializeDropdowns()
        {
            // Initialize resolution dropdown
            if (_resolutionDropdown != null)
            {
                _resolutionDropdown.choices = new List<string>
                {
                    "1280x720",
                    "1920x1080",
                    "2560x1440",
                    "3840x2160"
                };
                _resolutionDropdown.index = 1; // Default to 1080p
            }

            // Initialize quality dropdown
            if (_qualityDropdown != null)
            {
                string[] qualityNames = QualitySettings.names;
                _qualityDropdown.choices = new List<string>(qualityNames);
                _qualityDropdown.index = QualitySettings.GetQualityLevel();
            }
        }

        /// <summary>
        /// Load player info from PlayerPrefs.
        /// </summary>
        private void LoadPlayerInfo()
        {
            // Load player name
            if (_playerNameInput != null)
            {
                _playerNameInput.value = PlayerPrefs.GetString("PlayerName", "Fall Guy 2005");
            }

            // Load player level in top bar
            if (_playerLevelText != null)
            {
                _playerLevelText.text = PlayerPrefs.GetInt("PlayerLevel", 1).ToString();
            }

            // Load currencies
            Label trophyCount = _root.Q<Label>("trophy-count");
            if (trophyCount != null)
            {
                trophyCount.text = PlayerPrefs.GetInt("PlayerTrophies", 22166).ToString();
            }

            Label gemCount = _root.Q<Label>("gem-count");
            if (gemCount != null)
            {
                gemCount.text = PlayerPrefs.GetInt("PlayerGems", 1609).ToString();
            }

            Label coinCount = _root.Q<Label>("coin-count");
            if (coinCount != null)
            {
                coinCount.text = PlayerPrefs.GetInt("PlayerCoins", 6103).ToString();
            }

            Label energyCount = _root.Q<Label>("energy-count");
            if (energyCount != null)
            {
                energyCount.text = PlayerPrefs.GetInt("PlayerEnergy", 122).ToString();
            }

            // Load rank
            Label rankNumber = _root.Q<Label>("rank-number");
            if (rankNumber != null)
            {
                rankNumber.text = PlayerPrefs.GetInt("PlayerRank", 6).ToString();
            }

            // Load trophy road progress
            Label trophyRoadCount = _root.Q<Label>("trophy-road-count");
            if (trophyRoadCount != null)
            {
                trophyRoadCount.text = PlayerPrefs.GetInt("TrophyRoadProgress", 506).ToString();
            }
        }

        /// <summary>
        /// Set up button click listeners.
        /// </summary>
        private void SetupButtonListeners()
        {
            // Main menu buttons
            if (_playButton != null) _playButton.clicked += OnPlayButtonClicked;
            if (_settingsButton != null) _settingsButton.clicked += OnSettingsButtonClicked;
            if (_creditsButton != null) _creditsButton.clicked += OnCreditsButtonClicked;
            if (_quitButton != null) _quitButton.clicked += OnQuitButtonClicked;

            // Settings panel
            if (_settingsBackButton != null) _settingsBackButton.clicked += OnSettingsBackButtonClicked;

            // Credits panel
            if (_creditsBackButton != null) _creditsBackButton.clicked += OnCreditsBackButtonClicked;
        }

        /// <summary>
        /// Show only the main panel.
        /// </summary>
        private void ShowMainPanel()
        {
            if (_mainPanel != null) _mainPanel.RemoveFromClassList("hidden");
            if (_settingsPanel != null) _settingsPanel.AddToClassList("hidden");
            if (_creditsPanel != null) _creditsPanel.AddToClassList("hidden");
        }

        /// <summary>
        /// Show only the settings panel.
        /// </summary>
        private void ShowSettingsPanel()
        {
            if (_mainPanel != null) _mainPanel.AddToClassList("hidden");
            if (_settingsPanel != null) _settingsPanel.RemoveFromClassList("hidden");
            if (_creditsPanel != null) _creditsPanel.AddToClassList("hidden");
        }

        /// <summary>
        /// Show only the credits panel.
        /// </summary>
        private void ShowCreditsPanel()
        {
            if (_mainPanel != null) _mainPanel.AddToClassList("hidden");
            if (_settingsPanel != null) _settingsPanel.AddToClassList("hidden");
            if (_creditsPanel != null) _creditsPanel.RemoveFromClassList("hidden");
        }

        /// <summary>
        /// Handle play button click.
        /// </summary>
        private void OnPlayButtonClicked()
        {
            Debug.Log("[MainMenuUI] Play button clicked");

            // Save player name if provided
            if (_playerNameInput != null && !string.IsNullOrEmpty(_playerNameInput.value))
            {
                PlayerPrefs.SetString("PlayerName", _playerNameInput.value);
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
            EditorApplication.isPlaying = false;
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
                PlayerPrefs.SetInt("Fullscreen", _fullscreenToggle.value ? 1 : 0);
                Screen.fullScreen = _fullscreenToggle.value;
            }

            // Save resolution
            if (_resolutionDropdown != null)
            {
                PlayerPrefs.SetInt("Resolution", _resolutionDropdown.index);
                // Apply resolution (would need to map dropdown index to actual resolution)
            }

            // Save quality
            if (_qualityDropdown != null)
            {
                PlayerPrefs.SetInt("Quality", _qualityDropdown.index);
                QualitySettings.SetQualityLevel(_qualityDropdown.index);
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
                _fullscreenToggle.value = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
            }

            // Load resolution
            if (_resolutionDropdown != null)
            {
                _resolutionDropdown.index = PlayerPrefs.GetInt("Resolution", 1); // Default to 1080p
            }

            // Load quality
            if (_qualityDropdown != null)
            {
                _qualityDropdown.index = PlayerPrefs.GetInt("Quality", QualitySettings.GetQualityLevel());
            }
        }
    }
}