using System;
using System.Collections.Generic;
using Core.Animation;
using Core.Bootstrap;
using Core.Networking.Common;
using Core.Networking.Interfaces;
using Core.State;
using Core.State.States;
using DG.Tweening;
using UI.Data;
using UI.Screens;
using UnityEngine;
using UnityEngine.UIElements;
using Core.Logging;

namespace UI.Components.Tabs
{
    /// <summary>
    /// Lobby/Home tab with 4-player party support
    /// Brawl Stars-style lobby with ADD buttons and context-sensitive action button
    /// </summary>
    public class LobbyTabComponent
    {
        private VisualElement _root;
        private readonly IMatchmakingService _matchmakingService;
        private readonly IGameStateManager _stateManager;

        // UI Elements - Map/Event
        private Button _playButton;
        private Button _mapButton;
        private Label _mapNameLabel;
        private Label _mapSubtitleLabel;
        private Label _timerLabel;
        private Label _actionButtonText;

        // UI Elements - Party
        private VisualElement _teamControls;
        private Label _teamCodeLabel;
        private Button _leaveButton;

        // Player Slots (dynamic based on team size)
        private List<PlayerSlot> _playerSlots = new List<PlayerSlot>();
        private VisualElement _playerSlotsContainer;

        // Map data
        private MapDatabase _mapDatabase;
        private MapInfo _currentMap;

        // Party state
        private bool _isInParty = false;
        private bool _isReady = false;
        private int _currentPlayerCount = 1;
        private int _maxTeamSize = 4; // Default, can be changed based on game mode
        private bool _buttonsInitialized = false;

        // Template
        private VisualTreeAsset _playerSlotTemplate;

        public LobbyTabComponent(
            IMatchmakingService matchmakingService,
            IGameStateManager stateManager)
        {
            _matchmakingService = matchmakingService ?? throw new ArgumentNullException(nameof(matchmakingService));
            _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
        }

        public void Initialize(VisualElement root)
        {
            GameLogger.Log("Initialize called");

            if (root == null)
            {
                GameLogger.LogError("Root is null!");
                return;
            }

            _root = root;
            GameLogger.Log($"Root element: {_root.name}");

            LoadTemplate();                    // Load PlayerSlot template
            QueryElements();
            CreatePlayerSlots(_maxTeamSize);  // Create slots from template
            SetupStaticButtons();              // Setup static buttons once
            SetupDynamicButtons();             // Setup dynamic ADD buttons
            LoadMapDatabase();
            LoadMapInfo();
            UpdatePartyState();
            UpdateActionButton();
            HideAnimatedElements();            // Hide elements for intro animation

            GameLogger.Log("Initialization complete");
        }

        private void LoadTemplate()
        {
            _playerSlotTemplate = Resources.Load<VisualTreeAsset>("UI/Templates/Components/PlayerSlot");
            if (_playerSlotTemplate == null)
            {
                GameLogger.LogError("Failed to load PlayerSlot template!");
            }
            else
            {
                GameLogger.Log("PlayerSlot template loaded successfully");
            }
        }

        /// <summary>
        /// Set the maximum team size and recreate slots
        /// Call this when changing game modes
        /// </summary>
        public void SetTeamSize(int teamSize)
        {
            if (teamSize < 1 || teamSize > 8)
            {
                GameLogger.LogWarning($"Invalid team size: {teamSize}. Using default.");
                return;
            }

            _maxTeamSize = teamSize;
            CreatePlayerSlots(teamSize);
            SetupDynamicButtons(); // Only re-setup dynamic ADD buttons

            GameLogger.Log($"Team size set to {teamSize}");
        }

        private void QueryElements()
        {
            GameLogger.Log("Querying UI elements");

            // Map/Event elements
            _playButton = _root.Q<Button>("play-button");
            _mapButton = _root.Q<Button>("map-button");
            _mapNameLabel = _root.Q<Label>("map-name");
            _mapSubtitleLabel = _root.Q<Label>("map-subtitle");
            _timerLabel = _root.Q<Label>("timer-text");
            _actionButtonText = _root.Q<Label>("action-button-text");

            // Party elements
            _teamControls = _root.Q<VisualElement>("team-controls");
            _teamCodeLabel = _root.Q<Label>("team-code");
            _leaveButton = _root.Q<Button>("leave-button");

            GameLogger.Log($"Query complete - Play: {_playButton != null}, Map: {_mapButton != null}");
        }

        /// <summary>
        /// Dynamically create player slots based on team size
        /// Slot 0 is always the local player (centered, larger)
        /// Other slots are teammates (smaller, on sides)
        /// </summary>
        private void CreatePlayerSlots(int teamSize)
        {
            _playerSlots.Clear();

            // Get the container
            _playerSlotsContainer = _root.Q<VisualElement>("player-slots");
            if (_playerSlotsContainer == null)
            {
                GameLogger.LogError("Player slots container not found!");
                return;
            }

            // Clear existing slots
            _playerSlotsContainer.Clear();

            // Create slots dynamically
            for (int i = 0; i < teamSize; i++)
            {
                var slot = CreatePlayerSlot(i, i == 0); // First slot is local player
                _playerSlots.Add(slot);
                _playerSlotsContainer.Add(slot.SlotElement);
            }

            // Set local player (slot 0) as filled with "You"
            if (_playerSlots.Count > 0)
            {
                SetPlayerSlotFilled(_playerSlots[0], "You", "Ready to play", false);
            }

            GameLogger.Log($"Created {_playerSlots.Count} player slots for team size {teamSize}");
        }

        /// <summary>
        /// Create a single player slot element from template
        /// </summary>
        private PlayerSlot CreatePlayerSlot(int index, bool isLocalPlayer)
        {
            if (_playerSlotTemplate == null)
            {
                GameLogger.LogError("PlayerSlot template is null! Cannot create slot.");
                return null;
            }

            // Clone template
            TemplateContainer slotContainer = _playerSlotTemplate.CloneTree();
            VisualElement slotElement = slotContainer.Q<VisualElement>("player-slot");

            if (slotElement == null)
            {
                GameLogger.LogError("Failed to find 'player-slot' in template!");
                return null;
            }

            // Add index-specific name
            slotElement.name = $"player-slot-{index}";

            // Add local player class for larger styling
            if (isLocalPlayer)
            {
                slotElement.AddToClassList("local-player");
            }

            // Get elements from template
            Button addButton = slotContainer.Q<Button>("add-player-button");
            VisualElement emptyContent = slotContainer.Q<VisualElement>("empty-slot-content");
            VisualElement statsContainer = slotContainer.Q<VisualElement>("player-slot-stats");
            VisualElement characterContainer = slotContainer.Q<VisualElement>("player-character");
            VisualElement nameContainer = slotContainer.Q<VisualElement>("player-name-container");
            Label playerName = slotContainer.Q<Label>("player-name");
            Label playerStatus = slotContainer.Q<Label>("player-status");
            VisualElement readyIndicator = slotContainer.Q<VisualElement>("ready-indicator");
            VisualElement crownIcon = slotContainer.Q<VisualElement>("crown-icon");

            // Set initial visibility based on slot type
            if (isLocalPlayer)
            {
                // Local player: Show player info, hide ADD button
                emptyContent?.AddToClassList("hidden");
                statsContainer?.RemoveFromClassList("hidden");
                characterContainer?.RemoveFromClassList("hidden");
                nameContainer?.RemoveFromClassList("hidden");
                crownIcon?.RemoveFromClassList("hidden");

                if (playerName != null)
                    playerName.text = "You";
            }
            else
            {
                // Empty slot: Show ADD button, hide player info
                emptyContent?.RemoveFromClassList("hidden");
                statsContainer?.AddToClassList("hidden");
                characterContainer?.AddToClassList("hidden");
                nameContainer?.AddToClassList("hidden");
                crownIcon?.AddToClassList("hidden");
            }

            // Create PlayerSlot data structure
            return new PlayerSlot
            {
                SlotIndex = index,
                IsLocalPlayer = isLocalPlayer,
                SlotElement = slotElement,
                AddButton = addButton,
                EmptyContent = emptyContent,
                StatsContainer = statsContainer,
                CharacterContainer = characterContainer,
                NameContainer = nameContainer,
                PlayerName = playerName,
                PlayerStatus = playerStatus,
                ReadyIndicator = readyIndicator,
                CrownIcon = crownIcon
            };
        }

        /// <summary>
        /// Setup static buttons (play, map, leave) - only called once during initialization
        /// </summary>
        private void SetupStaticButtons()
        {
            if (_buttonsInitialized)
                return;

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

            if (_leaveButton != null)
            {
                _leaveButton.clicked += OnLeaveClicked;
                GameLogger.Log("Leave button listener added");
            }

            _buttonsInitialized = true;
        }

        /// <summary>
        /// Setup dynamic ADD buttons for player slots - called when slots are recreated
        /// </summary>
        private void SetupDynamicButtons()
        {
            // Clean up old handlers first
            CleanupDynamicButtons();

            // Setup new ADD buttons for current player slots
            foreach (var slot in _playerSlots)
            {
                if (slot.AddButton != null)
                {
                    int slotIndex = slot.SlotIndex;
                    slot.AddButton.clicked += () => OnAddPlayerClicked(slotIndex);
                }
            }
        }

        /// <summary>
        /// Cleanup dynamic button handlers before recreating slots
        /// </summary>
        private void CleanupDynamicButtons()
        {
            foreach (var slot in _playerSlots)
            {
                if (slot.AddButton != null)
                {
                    // Note: Can't remove lambda handlers directly, but clearing slots handles this
                    // This is here for documentation and future-proofing
                }
            }
        }

        private void LoadMapDatabase()
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("UI/Data/Maps");
            if (jsonFile != null)
            {
                _mapDatabase = JsonUtility.FromJson<MapDatabase>(jsonFile.text);
                int totalMaps = 0;
                if (_mapDatabase.categories != null)
                {
                    foreach (var category in _mapDatabase.categories)
                    {
                        totalMaps += category.maps?.Count ?? 0;
                    }
                }
                GameLogger.Log($"Loaded map database with {totalMaps} maps from {_mapDatabase.categories?.Count ?? 0} categories");
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

            _currentMap = _mapDatabase.GetCurrentMap();

            if (_currentMap != null)
            {
                UpdateMapDisplay(_currentMap);

                // Set team size based on map's maxPlayers
                if (_currentMap.maxPlayers > 0)
                {
                    SetTeamSize(_currentMap.maxPlayers);
                }
            }
            else
            {
                string mapName = PlayerPrefs.GetString("CurrentMap", "CRUMB HAVEN");
                string mapSubtitle = PlayerPrefs.GetString("CurrentMapSubtitle", "COOPERATIVE CHAOS");

                if (_mapNameLabel != null)
                    _mapNameLabel.text = mapName;

                if (_mapSubtitleLabel != null)
                    _mapSubtitleLabel.text = mapSubtitle;
            }

            UpdateMapTimer();
        }

        private void UpdateMapDisplay(MapInfo map)
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
                _timerLabel.text = "NEW MAP IN : 30h 12m";
            }
        }

        private void UpdatePartyState()
        {
            if (_teamControls != null)
            {
                if (_isInParty)
                {
                    _teamControls.RemoveFromClassList("hidden");
                }
                else
                {
                    _teamControls.AddToClassList("hidden");
                }
            }
        }

        private void UpdateActionButton()
        {
            if (_playButton == null || _actionButtonText == null) return;

            if (_isInParty)
            {
                // In party: Show "I'M READY" or "CANCEL"
                _actionButtonText.text = _isReady ? "CANCEL" : "I'M READY";

                if (_isReady)
                {
                    _playButton.AddToClassList("ready");
                }
                else
                {
                    _playButton.RemoveFromClassList("ready");
                }
            }
            else
            {
                // Solo: Show "PLAY!"
                _actionButtonText.text = "PLAY!";
                _playButton.RemoveFromClassList("ready");
            }
        }

        private void SetPlayerSlotFilled(PlayerSlot slot, string playerName, string status, bool isReady)
        {
            if (slot == null) return;

            slot.SlotElement?.RemoveFromClassList("empty");
            slot.SlotElement?.AddToClassList("filled");
            slot.EmptyContent?.AddToClassList("hidden");
            slot.StatsContainer?.RemoveFromClassList("hidden");
            slot.CharacterContainer?.RemoveFromClassList("hidden");
            slot.NameContainer?.RemoveFromClassList("hidden");

            if (slot.PlayerName != null) slot.PlayerName.text = playerName;
            if (slot.PlayerStatus != null) slot.PlayerStatus.text = status;

            if (slot.ReadyIndicator != null)
            {
                if (isReady)
                {
                    slot.ReadyIndicator.RemoveFromClassList("hidden");
                }
                else
                {
                    slot.ReadyIndicator.AddToClassList("hidden");
                }
            }
        }

        private void SetPlayerSlotEmpty(PlayerSlot slot)
        {
            if (slot == null) return;

            slot.SlotElement?.AddToClassList("empty");
            slot.SlotElement?.RemoveFromClassList("filled");
            slot.EmptyContent?.RemoveFromClassList("hidden");
            slot.StatsContainer?.AddToClassList("hidden");
            slot.CharacterContainer?.AddToClassList("hidden");
            slot.NameContainer?.AddToClassList("hidden");
        }

        // Button Callbacks
        private void OnPlayClicked()
        {
            if (_isInParty)
            {
                // Toggle ready state
                _isReady = !_isReady;
                UpdateActionButton();

                // Update ready indicator for current player
                if (_playerSlots.Count > 0)
                {
                    var firstSlot = _playerSlots[0];
                    if (firstSlot.ReadyIndicator != null)
                    {
                        if (_isReady)
                        {
                            firstSlot.ReadyIndicator.RemoveFromClassList("hidden");
                        }
                        else
                        {
                            firstSlot.ReadyIndicator.AddToClassList("hidden");
                        }
                    }
                }

                GameLogger.Log($"Ready state: {_isReady}");

                // If all players ready, start matchmaking
                if (AreAllPlayersReady())
                {
                    GameLogger.Log("All players ready - starting matchmaking");
                    StartPartyMatchmaking();
                }
            }
            else
            {
                // Solo play - start matchmaking
                GameLogger.Log("Play button clicked - Starting solo matchmaking");
                StartSoloMatchmaking();
            }
        }

        private void StartSoloMatchmaking()
        {
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

        private void StartPartyMatchmaking()
        {
            // Party matchmaking logic implementation
            GameLogger.Log($"Starting party matchmaking with {_currentPlayerCount} players");

            if (_stateManager != null)
            {
                var matchmakingState = new MatchmakingState(GameMode.Classic, teamSize: _currentPlayerCount);
                _stateManager.ChangeState(matchmakingState);
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

        private void OnMapSelected(MapInfo map)
        {
            GameLogger.Log($"Map selected: {map.name}");

            // Validate map selection against current party size
            if (_isInParty && map.maxPlayers < _currentPlayerCount)
            {
                // Cannot select this map - party too large
                string message = $"Cannot select {map.name}. Map supports {map.maxPlayers} players but you have {_currentPlayerCount} in party.";
                GameLogger.LogWarning(message);

                var uiService = GameBootstrap.Services?.UIService;
                if (uiService != null)
                {
                    uiService.ShowNotification("Map Selection Failed", message, NotificationType.Error, 4f);
                }
                return;
            }

            _currentMap = map;
            UpdateMapDisplay(map);
            UpdateMapTimer();

            // Update team size based on new map
            if (map.maxPlayers > 0)
            {
                SetTeamSize(map.maxPlayers);
            }
        }

        private void OnLeaveClicked()
        {
            GameLogger.Log("Leave party clicked");

            _isInParty = false;
            _isReady = false;
            _currentPlayerCount = 1;

            // Clear all slots except first
            for (int i = 1; i < _playerSlots.Count; i++)
            {
                SetPlayerSlotEmpty(_playerSlots[i]);
            }

            UpdatePartyState();
            UpdateActionButton();
        }

        private void OnAddPlayerClicked(int slotIndex)
        {
            GameLogger.Log($"Add player to slot {slotIndex} clicked");
            OpenFriendsPopup();
        }

        private void OpenFriendsPopup()
        {
            GameLogger.Log("Opening Friends popup");

            var uiService = GameBootstrap.Services?.UIService;
            if (uiService != null)
            {
                // Use ShowScreen to properly display the popup
                uiService.ShowScreen(UIScreenType.FriendsPopup, animate: true, addToHistory: true);
            }
            else
            {
                GameLogger.LogError("UIService not available");
            }
        }

        // Public API for external systems
        public void AddPlayerToParty(string playerName, int level, int trophies)
        {
            for (int i = 1; i < _playerSlots.Count; i++)
            {
                var slot = _playerSlots[i];
                if (slot.SlotElement?.ClassListContains("empty") == true)
                {
                    SetPlayerSlotFilled(slot, playerName, "Joined", false);
                    _currentPlayerCount++;
                    _isInParty = true;
                    UpdatePartyState();
                    UpdateActionButton();

                    // Animate slot
                    var animationService = GameBootstrap.Services?.AnimationService;
                    if (animationService != null && slot.SlotElement != null)
                    {
                        animationService.UI.Pulse(slot.SlotElement, 0.3f);
                    }

                    GameLogger.Log($"Added {playerName} to party (slot {i})");
                    break;
                }
            }
        }

        public void RemovePlayerFromParty(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < _playerSlots.Count)
            {
                SetPlayerSlotEmpty(_playerSlots[slotIndex]);
                _currentPlayerCount--;

                if (_currentPlayerCount <= 1)
                {
                    _isInParty = false;
                    _isReady = false;
                    UpdatePartyState();
                    UpdateActionButton();
                }

                GameLogger.Log($"Removed player from slot {slotIndex}");
            }
        }

        public void SetPlayerReady(int slotIndex, bool isReady)
        {
            if (slotIndex >= 0 && slotIndex < _playerSlots.Count)
            {
                var slot = _playerSlots[slotIndex];
                if (slot.ReadyIndicator != null)
                {
                    if (isReady)
                    {
                        slot.ReadyIndicator.RemoveFromClassList("hidden");
                    }
                    else
                    {
                        slot.ReadyIndicator.AddToClassList("hidden");
                    }
                }
            }
        }

        public bool AreAllPlayersReady()
        {
            if (!_isInParty) return false;

            for (int i = 0; i < _currentPlayerCount; i++)
            {
                var slot = _playerSlots[i];
                if (slot.ReadyIndicator?.ClassListContains("hidden") != false)
                {
                    return false;
                }
            }

            return true;
        }

        public int GetPartySize() => _currentPlayerCount;
        public bool IsInParty() => _isInParty;
        public bool IsReady() => _isReady;

        public void Update(float deltaTime)
        {
            // Update timer if needed
        }

        /// <summary>
        /// Hide all animated elements before intro animations
        /// </summary>
        private void HideAnimatedElements()
        {
            if (_root == null) return;

            // Left panel elements
            HideElement(_root.Q<VisualElement>("news-btn"));
            HideElement(_root.Q<VisualElement>("bp-widget"));

            // Right panel elements
            HideElement(_root.Q<VisualElement>("lobby-header"));
            HideElement(_root.Q<VisualElement>("squad-row"));
            HideElement(_root.Q<VisualElement>("map-box"));
            HideElement(_root.Q<VisualElement>("play-btn"));

            // Character model
            HideElement(_root.Q<VisualElement>("character-model"));
        }

        private void HideElement(VisualElement element)
        {
            if (element == null) return;
            element.style.opacity = 0f;
        }

        /// <summary>
        /// Play intro animations for lobby elements
        /// Called by MainMenuScreen when screen becomes visible
        /// </summary>
        public void PlayIntroAnimations(IUIAnimator animator)
        {
            if (_root == null || animator == null) return;

            const float animDuration = 0.6f;

            // === LEFT PANEL (Slide from Left) ===
            var leftPanel = _root.Q<VisualElement>("left-panel");
            var newsBtn = _root.Q<VisualElement>("news-btn");
            var bpWidget = _root.Q<VisualElement>("bp-widget");

            AnimateSlideFromLeft(newsBtn, 0.4f, animDuration);
            AnimateSlideFromLeft(bpWidget, 0.5f, animDuration);

            // === RIGHT PANEL (Slide from Right) ===
            var lobbyHeader = _root.Q<VisualElement>("lobby-header");
            var squadRow = _root.Q<VisualElement>("squad-row");
            var mapBox = _root.Q<VisualElement>("map-box");
            var playBtn = _root.Q<VisualElement>("play-btn");

            AnimateSlideFromRight(lobbyHeader, 0.5f, animDuration);
            AnimateSlideFromRight(squadRow, 0.6f, animDuration);
            AnimateSlideFromRight(mapBox, 0.7f, animDuration);
            AnimateSlideFromRight(playBtn, 0.8f, animDuration);

            // === CHARACTER MODEL (Scale up with fade) ===
            var characterModel = _root.Q<VisualElement>("character-model");
            AnimateScaleIn(characterModel, 0.3f, 1.0f);
        }

        private void AnimateSlideFromLeft(VisualElement element, float delay, float duration)
        {
            if (element == null) return;

            // Set initial state
            element.style.opacity = 0f;
            element.style.translate = new StyleTranslate(new Translate(-50, 0, 0));

            DOVirtual.DelayedCall(delay, () =>
            {
                DOTween.To(() => 0f, x => element.style.opacity = x, 1f, duration)
                    .SetEase(Ease.OutCubic);
                DOTween.To(() => -50f, x => element.style.translate = new StyleTranslate(new Translate(x, 0, 0)), 0f, duration)
                    .SetEase(Ease.OutCubic);
            });
        }

        private void AnimateSlideFromRight(VisualElement element, float delay, float duration)
        {
            if (element == null) return;

            // Set initial state
            element.style.opacity = 0f;
            element.style.translate = new StyleTranslate(new Translate(50, 0, 0));

            DOVirtual.DelayedCall(delay, () =>
            {
                DOTween.To(() => 0f, x => element.style.opacity = x, 1f, duration)
                    .SetEase(Ease.OutCubic);
                DOTween.To(() => 50f, x => element.style.translate = new StyleTranslate(new Translate(x, 0, 0)), 0f, duration)
                    .SetEase(Ease.OutCubic);
            });
        }

        private void AnimateScaleIn(VisualElement element, float delay, float duration)
        {
            if (element == null) return;

            // Set initial state
            element.style.opacity = 0f;
            element.style.scale = new StyleScale(new Vector2(0.95f, 0.95f));
            element.style.translate = new StyleTranslate(new Translate(0, 50, 0));

            DOVirtual.DelayedCall(delay, () =>
            {
                DOTween.To(() => 0f, x => element.style.opacity = x, 1f, duration * 0.5f)
                    .SetEase(Ease.OutCubic);
                DOTween.To(() => 0.95f, x => element.style.scale = new StyleScale(new Vector2(x, x)), 1f, duration)
                    .SetEase(Ease.OutBack);
                DOTween.To(() => 50f, y => element.style.translate = new StyleTranslate(new Translate(0, y, 0)), 0f, duration)
                    .SetEase(Ease.OutBack);
            });
        }

        /// <summary>
        /// Cleanup event handlers - call when disposing component
        /// </summary>
        public void Dispose()
        {
            if (_playButton != null)
                _playButton.clicked -= OnPlayClicked;

            if (_mapButton != null)
                _mapButton.clicked -= OnMapClicked;

            if (_leaveButton != null)
                _leaveButton.clicked -= OnLeaveClicked;

            CleanupDynamicButtons();

            _buttonsInitialized = false;

            GameLogger.Log("Disposed");
        }

        // Helper class
        private class PlayerSlot
        {
            public int SlotIndex;
            public bool IsLocalPlayer;
            public VisualElement SlotElement;
            public Button AddButton;
            public VisualElement EmptyContent;
            public VisualElement StatsContainer;
            public VisualElement CharacterContainer;
            public VisualElement NameContainer;
            public Label PlayerName;
            public Label PlayerStatus;
            public VisualElement ReadyIndicator;
            public VisualElement CrownIcon;
        }
    }
}
