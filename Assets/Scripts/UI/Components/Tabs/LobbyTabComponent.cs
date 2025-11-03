using System;
using Core.Bootstrap;
using Core.Networking.Common;
using Core.Networking.Interfaces;
using Core.State;
using Core.State.States;
using UI.Data;
using UI.Popups;
using UI.Screens;
using UnityEngine;
using UnityEngine.UIElements;
using Core.Logging;

namespace UI.Components.Tabs
{
    /// <summary>
    /// Lobby/Home tab content component
    /// Pure C# class with proper dependency injection
    /// Renamed from MainMenuUI for clarity
    /// </summary>
    public class LobbyTabComponent
    {
        private VisualElement _root;
        private readonly IMatchmakingService _matchmakingService;
        private readonly IGameStateManager _stateManager;

        // UI Elements
        private Button _playButton;
        private Button _mapButton;
        private Button _friendsButton;
        private Label _mapNameLabel;
        private Label _mapSubtitleLabel;
        private Label _timerLabel;
        private VisualElement _partyDisplay;

        // Map data
        private MapDatabase _mapDatabase;
        private MapInfo _currentMap;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public LobbyTabComponent(
            IMatchmakingService matchmakingService,
            IGameStateManager stateManager)
        {
            _matchmakingService = matchmakingService ?? throw new System.ArgumentNullException(nameof(matchmakingService));
            _stateManager = stateManager ?? throw new System.ArgumentNullException(nameof(stateManager));
        }

        public void Initialize(
            VisualElement root)
        {
            GameLogger.Log("Initialize called");

            if (root == null)
            {
                GameLogger.LogError("Root is null!");
                return;
            }

            _root = root;
            GameLogger.Log($"Root element: {_root.name}");

            // Query all elements first
            QueryElements();

            GameLogger.Log($"Elements found - Play: {_playButton != null}, Map: {_mapButton != null}, Friends: {_friendsButton != null}");

            // Setup in order
            SetupButtons();
            LoadMapDatabase();
            LoadMapInfo();

            GameLogger.Log("Initialization complete");
        }

        private void QueryElements()
        {
            GameLogger.Log("Querying UI elements");

            // Cache UI elements
            _playButton = _root.Q<Button>("play-button");
            _mapButton = _root.Q<Button>("map-button");
            _friendsButton = _root.Q<Button>("friends-button");
            _mapNameLabel = _root.Q<Label>("map-name");
            _mapSubtitleLabel = _root.Q<Label>("map-subtitle");
            _timerLabel = _root.Q<Label>("timer-text");
            _partyDisplay = _root.Q<VisualElement>("party-display");

            GameLogger.Log($"Query complete - Play: {_playButton != null}, Map: {_mapButton != null}, Friends: {_friendsButton != null}, MapName: {_mapNameLabel != null}, Timer: {_timerLabel != null}, Party: {_partyDisplay != null}");
        }

        private void SetupButtons()
        {
            if (_playButton != null)
            {
                _playButton.clicked += OnPlayClicked;
                GameLogger.Log("Play button listener added");
            }

            if (_mapButton != null)
            {
                _mapButton.clicked += OnMapClicked;
                GameLogger.Log("Map button listener added");
            }

            if (_friendsButton != null)
            {
                _friendsButton.clicked += OnFriendsClicked;
                GameLogger.Log("Friends button listener added");
            }
        }

        private void LoadMapDatabase()
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("UI/Data/Maps");
            if (jsonFile != null)
            {
                _mapDatabase = JsonUtility.FromJson<MapDatabase>(jsonFile.text);
                GameLogger.Log($"Loaded map database with {_mapDatabase.maps.Count} maps");
            }
            else
            {
                GameLogger.LogError("Failed to load Maps.json");
            }
        }

        private void LoadMapInfo()
        {
            if (_mapDatabase == null)
                return;

            // Get current map from database
            _currentMap = _mapDatabase.GetCurrentMap();

            if (_currentMap != null)
            {
                UpdateMapDisplay(_currentMap);
            }
            else
            {
                // Fallback to PlayerPrefs
                string mapName = PlayerPrefs.GetString("CurrentMap",
                    "CRUMB HAVEN");
                string mapSubtitle = PlayerPrefs.GetString("CurrentMapSubtitle",
                    "COOPERATIVE CHAOS");

                if (_mapNameLabel != null)
                    _mapNameLabel.text = mapName;

                if (_mapSubtitleLabel != null)
                    _mapSubtitleLabel.text = mapSubtitle;
            }

            UpdateMapTimer();
        }

        private void UpdateMapDisplay(
            MapInfo map)
        {
            if (_mapNameLabel != null)
                _mapNameLabel.text = map.name;

            if (_mapSubtitleLabel != null)
                _mapSubtitleLabel.text = map.subtitle;

            GameLogger.Log($"Updated map display: {map.name}");
        }

        private void UpdateMapTimer()
        {
            if (_mapDatabase == null || _timerLabel == null)
                return;

            TimeSpan timeRemaining = _mapDatabase.GetTimeUntilRotation();

            if (timeRemaining.TotalSeconds > 0)
            {
                int hours = (int)timeRemaining.TotalHours;
                int minutes = timeRemaining.Minutes;
                _timerLabel.text = $"NEW MAP IN : {hours}h {minutes}m";
            }
            else
            {
                _timerLabel.text = "NEW MAP IN : 30h 12m"; // Fallback
            }
        }

        private void OnPlayClicked()
        {
            GameLogger.Log("Play button clicked - Transitioning to MatchmakingState");

            // Transition to MatchmakingState (it will handle everything)
            if (_stateManager != null)
            {
                var matchmakingState = new MatchmakingState(GameMode.Classic, teamSize: 4);
                _stateManager.ChangeState(matchmakingState);
            }
            else
            {
                GameLogger.LogError("StateManager not available!");
            }
        }

        private void OnMapClicked()
        {
            GameLogger.Log("Map button clicked - Opening map selection screen");

            var uiService = GameBootstrap.Services?.UIService;
            if (uiService != null)
            {
                var mapScreen = uiService.GetScreen<MapSelectionScreen>();
                if (mapScreen != null)
                {
                    mapScreen.ShowWithCallback(OnMapSelected);
                }
                else
                {
                    GameLogger.LogError("MapSelectionScreen not found");
                }
            }
        }

        private void OnMapSelected(
            MapInfo map)
        {
            GameLogger.Log($"Map selected: {map.name}");

            _currentMap = map;
            UpdateMapDisplay(map);
            UpdateMapTimer();
        }

        private void OnFriendsClicked()
        {
            GameLogger.Log("Friends button clicked - Opening friends popup");

            var uiService = GameBootstrap.Services?.UIService;
            if (uiService != null)
            {
                // Get the FriendsPopup screen
                var friendsPopup = uiService.GetScreen<FriendsPopup>();
                if (friendsPopup != null)
                {
                    friendsPopup.Show(true, true);
                }
                else
                {
                    GameLogger.LogError("FriendsPopup not found");
                    uiService.ShowNotification("Friends popup not available",
                        NotificationType.Error,
                        2f);
                }
            }
        }

        /// <summary>
        /// Update method - call from parent if needed
        /// </summary>
        public void Update(
            float deltaTime)
        {
        }

        /// <summary>
        /// Cleanup resources
        /// </summary>
        public void Dispose()
        {
            if (_playButton != null)
            {
                _playButton.clicked -= OnPlayClicked;
            }

            if (_mapButton != null)
            {
                _mapButton.clicked -= OnMapClicked;
            }

            if (_friendsButton != null)
            {
                _friendsButton.clicked -= OnFriendsClicked;
            }

            GameLogger.Log("Disposed");
        }
    }
}