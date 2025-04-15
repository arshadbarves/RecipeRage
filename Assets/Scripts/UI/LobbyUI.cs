using System.Collections.Generic;
using RecipeRage.Core.GameFramework.State;
using RecipeRage.Core.GameFramework.State.States;
using RecipeRage.Core.GameModes;
using RecipeRage.Core.Networking;
using RecipeRage.Core.Networking.Common;
using RecipeRage.Core.Networking.EOS;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace RecipeRage.UI
{
    /// <summary>
    /// Manages the lobby UI using UI Toolkit.
    /// </summary>
    public class LobbyUI : MonoBehaviour
    {
        /// <summary>
        /// The UI document component.
        /// </summary>
        private UIDocument _uiDocument;

        /// <summary>
        /// The root visual element.
        /// </summary>
        private VisualElement _root;

        // Panels
        private VisualElement _lobbyPanel;
        private VisualElement _gameModePanel;

        // Lobby Panel
        private ScrollView _playerList;
        private Button _readyButton;
        private Button _leaveButton;
        private Label _lobbyCodeText;
        private Label _lobbyStatusText;
        private Label _selectedGameModeText;
        private Button _changeGameModeButton;

        // Game Mode Panel
        private ScrollView _gameModeList;
        private Button _gameModeBackButton;

        // Team Selection
        private RadioButtonGroup _teamToggle;

        /// <summary>
        /// Reference to the game state manager.
        /// </summary>
        private GameStateManager _gameStateManager;

        /// <summary>
        /// Reference to the network manager.
        /// </summary>
        private RecipeRageNetworkManager _networkManager;

        /// <summary>
        /// Reference to the lobby manager.
        /// </summary>
        private RecipeRageLobbyManager _lobbyManager;

        /// <summary>
        /// Reference to the game mode manager.
        /// </summary>
        private GameModeManager _gameModeManager;

        /// <summary>
        /// The list of player entry elements in the UI.
        /// </summary>
        private readonly List<VisualElement> _playerEntries = new();

        /// <summary>
        /// The list of game mode entry elements in the UI.
        /// </summary>
        private readonly List<VisualElement> _gameModeEntries = new();

        /// <summary>
        /// The UXML template for player entries.
        /// </summary>
        private VisualTreeAsset _playerEntryTemplate;

        /// <summary>
        /// The UXML template for game mode entries.
        /// </summary>
        private VisualTreeAsset _gameModeEntryTemplate;

        /// <summary>
        /// Whether the local player is ready.
        /// </summary>
        private bool _isReady = false;

        /// <summary>
        /// The currently selected game mode.
        /// </summary>
        private RecipeRage.Core.GameModes.GameMode _selectedGameMode;

        /// <summary>
        /// Initialize the lobby UI.
        /// </summary>
        private void Awake()
        {
            // Get references
            _gameStateManager = GameStateManager.Instance;
            _networkManager = RecipeRageNetworkManager.Instance;
            _lobbyManager = _networkManager?.LobbyManager;
            _gameModeManager = GameModeManager.Instance;

            // Get UI Document component
            _uiDocument = GetComponent<UIDocument>();
            if (_uiDocument == null)
            {
                Debug.LogError("[LobbyUI] UIDocument component not found");
                return;
            }

            // Load UXML templates
            _playerEntryTemplate = Resources.Load<VisualTreeAsset>("UI/LobbyPlayerEntry");
            _gameModeEntryTemplate = Resources.Load<VisualTreeAsset>("UI/GameModeEntry");

            if (_playerEntryTemplate == null)
            {
                Debug.LogError("[LobbyUI] Failed to load LobbyPlayerEntry template");
            }

            if (_gameModeEntryTemplate == null)
            {
                Debug.LogError("[LobbyUI] Failed to load GameModeEntry template");
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
            _lobbyPanel = _root.Q<VisualElement>("lobby-panel");
            _gameModePanel = _root.Q<VisualElement>("game-mode-panel");

            // Get lobby panel references
            _playerList = _root.Q<ScrollView>("player-list");
            _readyButton = _root.Q<Button>("ready-button");
            _leaveButton = _root.Q<Button>("leave-button");
            _lobbyCodeText = _root.Q<Label>("lobby-code");
            _lobbyStatusText = _root.Q<Label>("lobby-status");
            _selectedGameModeText = _root.Q<Label>("selected-game-mode");
            _changeGameModeButton = _root.Q<Button>("change-game-mode-button");

            // Get game mode panel references
            _gameModeList = _root.Q<ScrollView>("game-mode-list");
            _gameModeBackButton = _root.Q<Button>("game-mode-back-button");

            // Get team selection references
            _teamToggle = _root.Q<RadioButtonGroup>("team-toggle");

            // Set up button listeners
            SetupButtonListeners();

            // Initialize game mode list
            InitializeGameModeList();

            // Show lobby panel by default
            ShowLobbyPanel();
        }

        /// <summary>
        /// Set up button click listeners.
        /// </summary>
        private void SetupButtonListeners()
        {
            // Lobby panel buttons
            if (_readyButton != null) _readyButton.clicked += OnReadyButtonClicked;
            if (_leaveButton != null) _leaveButton.clicked += OnLeaveButtonClicked;
            if (_changeGameModeButton != null) _changeGameModeButton.clicked += OnChangeGameModeButtonClicked;

            // Game mode panel buttons
            if (_gameModeBackButton != null) _gameModeBackButton.clicked += OnGameModeBackButtonClicked;

            // Team toggle
            if (_teamToggle != null) _teamToggle.RegisterValueChangedCallback(OnTeamToggleChanged);
        }

        /// <summary>
        /// Handle change game mode button click.
        /// </summary>
        private void OnChangeGameModeButtonClicked()
        {
            Debug.Log("[LobbyUI] Change game mode button clicked");
            ShowGameModePanel();
        }

        /// <summary>
        /// Show only the lobby panel.
        /// </summary>
        private void ShowLobbyPanel()
        {
            if (_lobbyPanel != null) _lobbyPanel.RemoveFromClassList("hidden");
            if (_gameModePanel != null) _gameModePanel.AddToClassList("hidden");
        }

        /// <summary>
        /// Show only the game mode panel.
        /// </summary>
        private void ShowGameModePanel()
        {
            if (_lobbyPanel != null) _lobbyPanel.AddToClassList("hidden");
            if (_gameModePanel != null) _gameModePanel.RemoveFromClassList("hidden");
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
                _readyButton.text = _isReady ? "Unready" : "Ready";
            }

            // Notify lobby manager
            if (_lobbyManager != null)
            {
                _lobbyManager.SetPlayerReady(_isReady);

                // Update the UI immediately
                UpdatePlayerList();
            }
        }

        /// <summary>
        /// Handle leave button click.
        /// </summary>
        private void OnLeaveButtonClicked()
        {
            Debug.Log("[LobbyUI] Leave button clicked");

            // Leave the game
            if (_networkManager != null)
            {
                _networkManager.LeaveGame();
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
        /// <param name="evt">The change event</param>
        private void OnTeamToggleChanged(ChangeEvent<int> evt)
        {
            int teamIndex = evt.newValue;
            Debug.Log($"[LobbyUI] Team toggle changed to team {teamIndex + 1}");

            // Notify lobby manager
            if (_lobbyManager != null)
            {
                _lobbyManager.SetPlayerTeam((TeamId)teamIndex);

                // Update the UI immediately
                UpdatePlayerList();
            }
        }

        /// <summary>
        /// Handle game mode selection.
        /// </summary>
        /// <param name="gameMode">The selected game mode</param>
        private void OnGameModeSelected(RecipeRage.Core.GameModes.GameMode gameMode)
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
            if (_lobbyManager != null && _networkManager.IsHost)
            {
                if (Enum.TryParse<RecipeRage.Core.Networking.Common.GameMode>(gameMode.Id, out var gameModeEnum))
                {
                    _lobbyManager.SetGameMode(gameModeEnum);
                }

                // Update the UI immediately
                UpdatePlayerList();
            }

            // Return to lobby panel
            ShowLobbyPanel();
        }

        /// <summary>
        /// Update the player list UI.
        /// </summary>
        public void UpdatePlayerList()
        {
            if (_lobbyManager == null) return;

            // Get players from both teams
            List<PlayerInfo> allPlayers = new();
            allPlayers.AddRange(_lobbyManager.TeamA);
            allPlayers.AddRange(_lobbyManager.TeamB);

            // Clear existing player entries
            ClearPlayerList();

            // Create new player entries
            foreach (var player in allPlayers)
            {
                CreatePlayerEntry(player);
            }

            // Update lobby status
            UpdateLobbyStatus(allPlayers);
        }

        /// <summary>
        /// Clear the player list UI.
        /// </summary>
        private void ClearPlayerList()
        {
            if (_playerList == null) return;

            foreach (var entry in _playerEntries)
            {
                _playerList.Remove(entry);
            }

            _playerEntries.Clear();
        }

        /// <summary>
        /// Create a player entry in the UI.
        /// </summary>
        /// <param name="player">The player info</param>
        private void CreatePlayerEntry(PlayerInfo player)
        {
            if (_playerList == null || _playerEntryTemplate == null) return;

            // Instantiate the template
            TemplateContainer playerEntryInstance = _playerEntryTemplate.Instantiate();
            VisualElement playerEntry = playerEntryInstance.contentContainer.Q<VisualElement>("player-entry");

            // Set player name
            Label nameLabel = playerEntry.Q<Label>("player-name");
            if (nameLabel != null)
            {
                nameLabel.text = player.DisplayName;

                // Add host indicator if player is host
                if (player.IsHost)
                {
                    nameLabel.text += " (Host)";
                }

                // Add local indicator if player is local
                if (player.IsLocal)
                {
                    nameLabel.text += " (You)";
                }
            }

            // Set player level (placeholder)
            Label levelLabel = playerEntry.Q<Label>("player-level");
            if (levelLabel != null)
            {
                levelLabel.text = $"Team {(player.Team == TeamId.TeamA ? "A" : "B")}";
            }

            // Set ready status
            VisualElement readyStatus = playerEntry.Q<VisualElement>("ready-status");
            if (readyStatus != null)
            {
                if (player.IsReady)
                {
                    readyStatus.AddToClassList("ready");
                    readyStatus.RemoveFromClassList("not-ready");
                }
                else
                {
                    readyStatus.AddToClassList("not-ready");
                    readyStatus.RemoveFromClassList("ready");
                }
            }

            // Add to the player list
            _playerList.Add(playerEntry);

            // Add to our tracking list
            _playerEntries.Add(playerEntry);
        }

        /// <summary>
        /// Update the lobby status text.
        /// </summary>
        /// <param name="players">The list of players in the lobby</param>
        private void UpdateLobbyStatus(List<PlayerInfo> players)
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

                // Update game mode text
                if (_selectedGameModeText != null && _lobbyManager != null)
                {
                    _selectedGameModeText.text = $"Selected: {_lobbyManager.CurrentGameMode}";
                }

                // Update lobby code
                if (_lobbyCodeText != null && _networkManager != null && _networkManager.SessionManager != null)
                {
                    GameSessionInfo sessionInfo = _networkManager.SessionManager.GetCurrentSession();
                    if (sessionInfo != null)
                    {
                        _lobbyCodeText.text = $"Lobby Code: {sessionInfo.SessionId}";
                    }
                }
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
                RecipeRage.Core.GameModes.GameMode[] gameModes = _gameModeManager.GetAvailableGameModes();

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
            if (_gameModeList == null) return;

            foreach (var entry in _gameModeEntries)
            {
                _gameModeList.Remove(entry);
            }

            _gameModeEntries.Clear();
        }

        /// <summary>
        /// Create a game mode entry in the UI.
        /// </summary>
        /// <param name="gameMode">The game mode</param>
        private void CreateGameModeEntry(RecipeRage.Core.GameModes.GameMode gameMode)
        {
            if (_gameModeList == null || _gameModeEntryTemplate == null) return;

            // Instantiate the template
            TemplateContainer gameModeEntryInstance = _gameModeEntryTemplate.Instantiate();
            VisualElement gameModeEntry = gameModeEntryInstance.contentContainer.Q<VisualElement>("game-mode-entry");

            // Set game mode name
            Label nameLabel = gameModeEntry.Q<Label>("game-mode-name");
            if (nameLabel != null)
            {
                nameLabel.text = gameMode.DisplayName;
            }

            // Set game mode description
            Label descriptionLabel = gameModeEntry.Q<Label>("game-mode-description");
            if (descriptionLabel != null)
            {
                descriptionLabel.text = gameMode.Description;
            }

            // Set game mode icon (if available)
            VisualElement iconElement = gameModeEntry.Q<VisualElement>("game-mode-icon");
            if (iconElement != null && gameMode.Icon != null)
            {
                // In UI Toolkit, we need to use a background image
                iconElement.style.backgroundImage = new StyleBackground(gameMode.Icon);
            }

            // Set select button click handler
            Button selectButton = gameModeEntry.Q<Button>("select-button");
            if (selectButton != null)
            {
                selectButton.clicked += () => OnGameModeSelected(gameMode);
            }

            // Add to the game mode list
            _gameModeList.Add(gameModeEntry);

            // Add to our tracking list
            _gameModeEntries.Add(gameModeEntry);
        }

        /// <summary>
        /// Initialize the UI when enabled.
        /// </summary>
        private void OnEnable()
        {
            // Subscribe to lobby events
            if (_lobbyManager != null)
            {
                _lobbyManager.OnLobbyUpdated += OnLobbyUpdated;
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
                _lobbyManager.OnLobbyUpdated -= OnLobbyUpdated;
            }
        }

        /// <summary>
        /// Handle lobby updates.
        /// </summary>
        private void OnLobbyUpdated()
        {
            // Update the player list
            UpdatePlayerList();

            // Check if all players are ready
            if (_lobbyManager != null && _lobbyManager.AreAllPlayersReady() && _networkManager.IsHost)
            {
                // Show a message that the game is ready to start
                if (_lobbyStatusText != null)
                {
                    _lobbyStatusText.text = "All players are ready! Starting game...";
                }

                // Start the game after a short delay
                StartCoroutine(StartGameAfterDelay(2.0f));
            }
        }

        /// <summary>
        /// Start the game after a delay.
        /// </summary>
        /// <param name="delay">The delay in seconds</param>
        private System.Collections.IEnumerator StartGameAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            // Start the game
            if (_networkManager != null)
            {
                _networkManager.StartGame();
            }
        }
    }
}
