using System.Collections.Generic;
using RecipeRage.Core.GameFramework.State;
using RecipeRage.Core.GameFramework.State.States;
using RecipeRage.Core.GameModes;
using RecipeRage.Core.Networking;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace RecipeRage.UI
{
    /// <summary>
    /// Manages the lobby UI.
    /// </summary>
    public class LobbyUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject _lobbyPanel;
        [SerializeField] private GameObject _gameModePanel;
        
        [Header("Lobby Panel")]
        [SerializeField] private Transform _playerListContent;
        [SerializeField] private GameObject _playerEntryPrefab;
        [SerializeField] private Button _readyButton;
        [SerializeField] private Button _leaveButton;
        [SerializeField] private TextMeshProUGUI _lobbyCodeText;
        [SerializeField] private TextMeshProUGUI _lobbyStatusText;
        
        [Header("Game Mode Panel")]
        [SerializeField] private Transform _gameModeListContent;
        [SerializeField] private GameObject _gameModeEntryPrefab;
        [SerializeField] private Button _gameModeBackButton;
        [SerializeField] private TextMeshProUGUI _selectedGameModeText;
        
        [Header("Team Selection")]
        [SerializeField] private Toggle _teamToggle;
        [SerializeField] private Transform _team1Content;
        [SerializeField] private Transform _team2Content;
        
        /// <summary>
        /// Reference to the game state manager.
        /// </summary>
        private GameStateManager _gameStateManager;
        
        /// <summary>
        /// Reference to the network lobby manager.
        /// </summary>
        private NetworkLobbyManager _lobbyManager;
        
        /// <summary>
        /// Reference to the game mode manager.
        /// </summary>
        private GameModeManager _gameModeManager;
        
        /// <summary>
        /// The list of player entries in the UI.
        /// </summary>
        private List<GameObject> _playerEntries = new List<GameObject>();
        
        /// <summary>
        /// The list of game mode entries in the UI.
        /// </summary>
        private List<GameObject> _gameModeEntries = new List<GameObject>();
        
        /// <summary>
        /// Whether the local player is ready.
        /// </summary>
        private bool _isReady = false;
        
        /// <summary>
        /// The currently selected game mode.
        /// </summary>
        private GameMode _selectedGameMode;
        
        /// <summary>
        /// Initialize the lobby UI.
        /// </summary>
        private void Awake()
        {
            // Get references
            _gameStateManager = GameStateManager.Instance;
            _lobbyManager = NetworkLobbyManager.Instance;
            _gameModeManager = GameModeManager.Instance;
            
            // Set up button listeners
            SetupButtonListeners();
            
            // Show lobby panel by default
            ShowLobbyPanel();
        }
        
        /// <summary>
        /// Set up button click listeners.
        /// </summary>
        private void SetupButtonListeners()
        {
            // Lobby panel buttons
            if (_readyButton != null) _readyButton.onClick.AddListener(OnReadyButtonClicked);
            if (_leaveButton != null) _leaveButton.onClick.AddListener(OnLeaveButtonClicked);
            
            // Game mode panel buttons
            if (_gameModeBackButton != null) _gameModeBackButton.onClick.AddListener(OnGameModeBackButtonClicked);
            
            // Team toggle
            if (_teamToggle != null) _teamToggle.onValueChanged.AddListener(OnTeamToggleChanged);
        }
        
        /// <summary>
        /// Show only the lobby panel.
        /// </summary>
        private void ShowLobbyPanel()
        {
            if (_lobbyPanel != null) _lobbyPanel.SetActive(true);
            if (_gameModePanel != null) _gameModePanel.SetActive(false);
        }
        
        /// <summary>
        /// Show only the game mode panel.
        /// </summary>
        private void ShowGameModePanel()
        {
            if (_lobbyPanel != null) _lobbyPanel.SetActive(false);
            if (_gameModePanel != null) _gameModePanel.SetActive(true);
        }
        
        /// <summary>
        /// Handle ready button click.
        /// </summary>
        private void OnReadyButtonClicked()
        {
            Debug.Log("[LobbyUI] Ready button clicked");
            
            // Toggle ready state
            _isReady = !_isReady;
            
            // Update button text
            if (_readyButton != null)
            {
                _readyButton.GetComponentInChildren<TextMeshProUGUI>().text = _isReady ? "Unready" : "Ready";
            }
            
            // Notify lobby manager
            if (_lobbyManager != null)
            {
                _lobbyManager.SetPlayerReady(_isReady);
            }
        }
        
        /// <summary>
        /// Handle leave button click.
        /// </summary>
        private void OnLeaveButtonClicked()
        {
            Debug.Log("[LobbyUI] Leave button clicked");
            
            // Leave the lobby
            if (_lobbyManager != null)
            {
                _lobbyManager.LeaveLobby();
            }
            
            // Return to main menu
            if (_gameStateManager != null)
            {
                _gameStateManager.ChangeState(new MainMenuState());
            }
        }
        
        /// <summary>
        /// Handle game mode back button click.
        /// </summary>
        private void OnGameModeBackButtonClicked()
        {
            Debug.Log("[LobbyUI] Game mode back button clicked");
            ShowLobbyPanel();
        }
        
        /// <summary>
        /// Handle team toggle change.
        /// </summary>
        /// <param name="isTeam2">Whether team 2 is selected</param>
        private void OnTeamToggleChanged(bool isTeam2)
        {
            Debug.Log($"[LobbyUI] Team toggle changed to team {(isTeam2 ? 2 : 1)}");
            
            // Notify lobby manager
            if (_lobbyManager != null)
            {
                _lobbyManager.SetPlayerTeam(isTeam2 ? 1 : 0);
            }
        }
        
        /// <summary>
        /// Handle game mode selection.
        /// </summary>
        /// <param name="gameMode">The selected game mode</param>
        private void OnGameModeSelected(GameMode gameMode)
        {
            Debug.Log($"[LobbyUI] Game mode selected: {gameMode.DisplayName}");
            
            // Set selected game mode
            _selectedGameMode = gameMode;
            
            // Update UI
            if (_selectedGameModeText != null)
            {
                _selectedGameModeText.text = $"Selected: {gameMode.DisplayName}";
            }
            
            // Notify lobby manager (if host)
            if (_lobbyManager != null && _lobbyManager.IsHost)
            {
                _lobbyManager.SetGameMode(gameMode.Id);
            }
            
            // Return to lobby panel
            ShowLobbyPanel();
        }
        
        /// <summary>
        /// Update the player list UI.
        /// </summary>
        /// <param name="players">The list of players in the lobby</param>
        public void UpdatePlayerList(List<NetworkLobbyManager.LobbyPlayerInfo> players)
        {
            // Clear existing player entries
            ClearPlayerList();
            
            // Create new player entries
            foreach (var player in players)
            {
                CreatePlayerEntry(player);
            }
            
            // Update lobby status
            UpdateLobbyStatus(players);
        }
        
        /// <summary>
        /// Clear the player list UI.
        /// </summary>
        private void ClearPlayerList()
        {
            foreach (var entry in _playerEntries)
            {
                Destroy(entry);
            }
            
            _playerEntries.Clear();
        }
        
        /// <summary>
        /// Create a player entry in the UI.
        /// </summary>
        /// <param name="player">The player info</param>
        private void CreatePlayerEntry(NetworkLobbyManager.LobbyPlayerInfo player)
        {
            // Determine which team content to use
            Transform parentTransform = player.Team == 0 ? _team1Content : _team2Content;
            
            // If team content is not available, use the player list content
            if (parentTransform == null)
            {
                parentTransform = _playerListContent;
            }
            
            // Create player entry
            if (_playerEntryPrefab != null && parentTransform != null)
            {
                GameObject entry = Instantiate(_playerEntryPrefab, parentTransform);
                
                // Set player name
                TextMeshProUGUI nameText = entry.GetComponentInChildren<TextMeshProUGUI>();
                if (nameText != null)
                {
                    nameText.text = player.PlayerName;
                }
                
                // Set ready status
                Image readyImage = entry.GetComponentInChildren<Image>();
                if (readyImage != null)
                {
                    readyImage.color = player.IsReady ? Color.green : Color.red;
                }
                
                // Add to list
                _playerEntries.Add(entry);
            }
        }
        
        /// <summary>
        /// Update the lobby status text.
        /// </summary>
        /// <param name="players">The list of players in the lobby</param>
        private void UpdateLobbyStatus(List<NetworkLobbyManager.LobbyPlayerInfo> players)
        {
            if (_lobbyStatusText != null)
            {
                int readyCount = 0;
                foreach (var player in players)
                {
                    if (player.IsReady)
                    {
                        readyCount++;
                    }
                }
                
                _lobbyStatusText.text = $"Players: {players.Count} | Ready: {readyCount}/{players.Count}";
            }
        }
        
        /// <summary>
        /// Update the lobby code text.
        /// </summary>
        /// <param name="lobbyCode">The lobby code</param>
        public void UpdateLobbyCode(string lobbyCode)
        {
            if (_lobbyCodeText != null)
            {
                _lobbyCodeText.text = $"Lobby Code: {lobbyCode}";
            }
        }
        
        /// <summary>
        /// Initialize the game mode list.
        /// </summary>
        private void InitializeGameModeList()
        {
            // Clear existing game mode entries
            ClearGameModeList();
            
            // Get available game modes
            if (_gameModeManager != null)
            {
                GameMode[] gameModes = _gameModeManager.GetAvailableGameModes();
                
                // Create game mode entries
                foreach (var gameMode in gameModes)
                {
                    CreateGameModeEntry(gameMode);
                }
            }
        }
        
        /// <summary>
        /// Clear the game mode list UI.
        /// </summary>
        private void ClearGameModeList()
        {
            foreach (var entry in _gameModeEntries)
            {
                Destroy(entry);
            }
            
            _gameModeEntries.Clear();
        }
        
        /// <summary>
        /// Create a game mode entry in the UI.
        /// </summary>
        /// <param name="gameMode">The game mode</param>
        private void CreateGameModeEntry(GameMode gameMode)
        {
            if (_gameModeEntryPrefab != null && _gameModeListContent != null)
            {
                GameObject entry = Instantiate(_gameModeEntryPrefab, _gameModeListContent);
                
                // Set game mode name
                TextMeshProUGUI nameText = entry.GetComponentInChildren<TextMeshProUGUI>();
                if (nameText != null)
                {
                    nameText.text = gameMode.DisplayName;
                }
                
                // Set game mode description
                TextMeshProUGUI descriptionText = entry.GetComponentsInChildren<TextMeshProUGUI>()[1];
                if (descriptionText != null)
                {
                    descriptionText.text = gameMode.Description;
                }
                
                // Set game mode image
                Image image = entry.GetComponentInChildren<Image>();
                if (image != null && gameMode.Icon != null)
                {
                    image.sprite = gameMode.Icon;
                }
                
                // Set click listener
                Button button = entry.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(() => OnGameModeSelected(gameMode));
                }
                
                // Add to list
                _gameModeEntries.Add(entry);
            }
        }
        
        /// <summary>
        /// Initialize the UI when enabled.
        /// </summary>
        private void OnEnable()
        {
            // Initialize game mode list
            InitializeGameModeList();
            
            // Subscribe to lobby events
            if (_lobbyManager != null)
            {
                _lobbyManager.OnLobbyPlayersUpdated += UpdatePlayerList;
                _lobbyManager.OnLobbyCodeUpdated += UpdateLobbyCode;
            }
        }
        
        /// <summary>
        /// Clean up when disabled.
        /// </summary>
        private void OnDisable()
        {
            // Unsubscribe from lobby events
            if (_lobbyManager != null)
            {
                _lobbyManager.OnLobbyPlayersUpdated -= UpdatePlayerList;
                _lobbyManager.OnLobbyCodeUpdated -= UpdateLobbyCode;
            }
        }
    }
}
