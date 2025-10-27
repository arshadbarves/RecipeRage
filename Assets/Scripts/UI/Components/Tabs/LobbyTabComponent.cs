using System;
using Core.Bootstrap;
using Core.Networking.Common;
using Core.Networking.Interfaces;
using Core.State;
using UI.Data;
using UI.Screens;
using UnityEngine;
using UnityEngine.UIElements;

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

        // Matchmaking widget
        private MatchmakingWidgetComponent _matchmakingWidget;

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
            Debug.Log("[LobbyTabComponent] Initialize called");

            if (root == null)
            {
                Debug.LogError("[LobbyTabComponent] Root is null!");
                return;
            }

            _root = root;
            Debug.Log($"[LobbyTabComponent] Root element: {_root.name}");

            // Cache UI elements
            _playButton = _root.Q<Button>("play-button");
            _mapButton = _root.Q<Button>("map-button");
            _friendsButton = _root.Q<Button>("friends-button");
            _mapNameLabel = _root.Q<Label>("map-name");
            _mapSubtitleLabel = _root.Q<Label>("map-subtitle");
            _timerLabel = _root.Q<Label>("timer-text");
            _partyDisplay = _root.Q<VisualElement>("party-display");

            Debug.Log($"[LobbyTabComponent] Elements found - Play: {_playButton != null}, Map: {_mapButton != null}, Friends: {_friendsButton != null}");

            SetupButtons();
            LoadMapDatabase();
            LoadMapInfo();
            SetupMatchmakingWidget();
        }

        private void SetupMatchmakingWidget()
        {
            // Create matchmaking widget component (pure C#)
            _matchmakingWidget = new MatchmakingWidgetComponent(_matchmakingService);
            _matchmakingWidget.Initialize(_root);

            Debug.Log("[LobbyTabComponent] Matchmaking widget setup complete");
        }

        private void SetupButtons()
        {
            if (_playButton != null)
            {
                _playButton.clicked += OnPlayClicked;
                Debug.Log("[LobbyTabComponent] Play button listener added");
            }

            if (_mapButton != null)
            {
                _mapButton.clicked += OnMapClicked;
                Debug.Log("[LobbyTabComponent] Map button listener added");
            }

            if (_friendsButton != null)
            {
                _friendsButton.clicked += OnFriendsClicked;
                Debug.Log("[LobbyTabComponent] Friends button listener added");
            }
        }

        private void LoadMapDatabase()
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("Data/Maps");
            if (jsonFile != null)
            {
                _mapDatabase = JsonUtility.FromJson<MapDatabase>(jsonFile.text);
                Debug.Log($"[LobbyTabComponent] Loaded map database with {_mapDatabase.maps.Count} maps");
            }
            else
            {
                Debug.LogError("[LobbyTabComponent] Failed to load Maps.json");
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

            Debug.Log($"[LobbyTabComponent] Updated map display: {map.name}");
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
            Debug.Log("[LobbyTabComponent] Play button clicked - Starting matchmaking");

            // Start matchmaking using injected service (follows DIP)
            _matchmakingService.FindMatch(GameMode.Classic, teamSize: 4);
        }

        private void OnMapClicked()
        {
            Debug.Log("[LobbyTabComponent] Map button clicked - Opening map selection screen");

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
                    Debug.LogError("[LobbyTabComponent] MapSelectionScreen not found");
                }
            }
        }

        private void OnMapSelected(
            MapInfo map)
        {
            Debug.Log($"[LobbyTabComponent] Map selected: {map.name}");

            _currentMap = map;
            UpdateMapDisplay(map);
            UpdateMapTimer();
        }

        private void OnFriendsClicked()
        {
            Debug.Log("[LobbyTabComponent] Friends button clicked - Opening friends popup");

            var uiService = GameBootstrap.Services?.UIService;
            if (uiService != null)
            {
                uiService.ShowScreen(UIScreenType.Popup,
                    true,
                    true);

                // TODO: Get FriendsPopup specifically
                // For now, just show a toast
                uiService.ShowToast("Friends system coming soon!",
                    ToastType.Info,
                    2f);
            }
        }

        /// <summary>
        /// Update method - call from parent if needed
        /// </summary>
        public void Update(
            float deltaTime)
        {
            _matchmakingWidget?.Update(deltaTime);
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

            _matchmakingWidget?.Dispose();

            Debug.Log("[LobbyTabComponent] Disposed");
        }
    }
}