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
using UI;

namespace UI.Components.Tabs
{
    public class LobbyTabComponent
    {
        private VisualElement _root;
        private readonly IMatchmakingService _matchmakingService;
        private readonly IGameStateManager _stateManager;
        private readonly IUIService _uiService;
        private readonly IAnimationService _animationService;
        private readonly ILoggingService _loggingService;

        private Button _playButton;
        private Button _mapButton;
        private Label _mapNameLabel;
        private Label _mapSubtitleLabel;
        private Label _timerLabel;
        private Label _actionButtonText;
        private VisualElement _teamControls;
        private Label _teamCodeLabel;
        private Button _leaveButton;
        private VisualElement _playerSlotsContainer;
        private VisualTreeAsset _playerSlotTemplate;

        private List<PlayerSlot> _playerSlots = new();
        private int _maxTeamSize = 4;
        private int _currentPlayerCount = 1;
        private bool _isInParty = false;
        private bool _isReady = false;
        private bool _buttonsInitialized = false;
        private MapDatabase _mapDatabase;
        private MapInfo _currentMap;

        public LobbyTabComponent(
            IMatchmakingService matchmakingService,
            IGameStateManager stateManager,
            IUIService uiService,
            IAnimationService animationService,
            ILoggingService loggingService)
        {
            _matchmakingService = matchmakingService;
            _stateManager = stateManager;
            _uiService = uiService;
            _animationService = animationService;
            _loggingService = loggingService;
        }

        public void Initialize(VisualElement root)
        {
            if (root == null) return;
            _root = root;
            LoadTemplate();
            QueryElements();
            CreatePlayerSlots(_maxTeamSize);
            SetupStaticButtons();
            SetupDynamicButtons();
            LoadMapDatabase();
            LoadMapInfo();
            UpdatePartyState();
            UpdateActionButton();
            HideAnimatedElements();
        }

        private void LoadTemplate()
        {
            _playerSlotTemplate = Resources.Load<VisualTreeAsset>("UI/Templates/Components/PlayerSlot");
        }

        public void SetTeamSize(int teamSize)
        {
            if (teamSize < 1 || teamSize > 8) return;
            _maxTeamSize = teamSize;
            CreatePlayerSlots(teamSize);
            SetupDynamicButtons();
        }

        private void QueryElements()
        {
            _playButton = _root.Q<Button>("play-button");
            _mapButton = _root.Q<Button>("map-button");
            _mapNameLabel = _root.Q<Label>("map-name");
            _mapSubtitleLabel = _root.Q<Label>("map-subtitle");
            _timerLabel = _root.Q<Label>("timer-text");
            _actionButtonText = _root.Q<Label>("action-button-text");
            _teamControls = _root.Q<VisualElement>("team-controls");
            _teamCodeLabel = _root.Q<Label>("team-code");
            _leaveButton = _root.Q<Button>("leave-button");
        }

        private void CreatePlayerSlots(int teamSize)
        {
            _playerSlotsContainer = _root.Q<VisualElement>("player-slots");
            if (_playerSlotsContainer == null) return;
            _playerSlotsContainer.Clear();
            _playerSlots.Clear();

            for (int i = 0; i < teamSize; i++)
            {
                var slot = CreatePlayerSlot(i, i == 0);
                _playerSlots.Add(slot);
                _playerSlotsContainer.Add(slot.SlotElement);
            }

            if (_playerSlots.Count > 0) SetPlayerSlotFilled(_playerSlots[0], "You", "Ready to play", false);
        }

        private PlayerSlot CreatePlayerSlot(int index, bool isLocalPlayer)
        {
            if (_playerSlotTemplate == null) return null;
            TemplateContainer slotContainer = _playerSlotTemplate.CloneTree();
            VisualElement slotElement = slotContainer.Q<VisualElement>("player-slot");
            if (slotElement == null) return null;

            var slot = new PlayerSlot {
                SlotIndex = index,
                IsLocalPlayer = isLocalPlayer,
                SlotElement = slotElement,
                AddButton = slotContainer.Q<Button>("add-player-button"),
                EmptyContent = slotContainer.Q<VisualElement>("empty-slot-content"),
                StatsContainer = slotContainer.Q<VisualElement>("player-slot-stats"),
                CharacterContainer = slotContainer.Q<VisualElement>("player-character"),
                NameContainer = slotContainer.Q<VisualElement>("player-name-container"),
                PlayerName = slotContainer.Q<Label>("player-name"),
                PlayerStatus = slotContainer.Q<Label>("player-status"),
                ReadyIndicator = slotContainer.Q<VisualElement>("ready-indicator"),
                CrownIcon = slotContainer.Q<VisualElement>("crown-icon")
            };

            if (isLocalPlayer) {
                slot.EmptyContent?.AddToClassList("hidden");
                if (slot.PlayerName != null) slot.PlayerName.text = "You";
            } else {
                slot.StatsContainer?.AddToClassList("hidden");
            }
            return slot;
        }

        private void SetupStaticButtons()
        {
            if (_buttonsInitialized) return;
            if (_playButton != null) _playButton.clicked += OnPlayClicked;
            if (_mapButton != null) _mapButton.clicked += OnMapClicked;
            if (_leaveButton != null) _leaveButton.clicked += OnLeaveClicked;
            _buttonsInitialized = true;
        }

        private void SetupDynamicButtons()
        {
            foreach (var slot in _playerSlots)
            {
                if (slot.AddButton != null)
                {
                    int slotIndex = slot.SlotIndex;
                    slot.AddButton.clicked += () => OnAddPlayerClicked(slotIndex);
                }
            }
        }

        private void LoadMapDatabase()
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("UI/Data/Maps");
            if (jsonFile != null) _mapDatabase = JsonUtility.FromJson<MapDatabase>(jsonFile.text);
        }

        private void LoadMapInfo()
        {
            if (_mapDatabase == null) return;
            _currentMap = _mapDatabase.GetCurrentMap();
            if (_currentMap != null) {
                UpdateMapDisplay(_currentMap);
                if (_currentMap.maxPlayers > 0) SetTeamSize(_currentMap.maxPlayers);
            }
            UpdateMapTimer();
        }

        private void UpdateMapDisplay(MapInfo map)
        {
            if (_mapNameLabel != null) _mapNameLabel.text = map.name;
            if (_mapSubtitleLabel != null) _mapSubtitleLabel.text = map.subtitle;
        }

        private void UpdateMapTimer()
        {
            if (_mapDatabase == null || _timerLabel == null) return;
            TimeSpan timeRemaining = _mapDatabase.GetTimeUntilRotation();
            _timerLabel.text = timeRemaining.TotalSeconds > 0 ? $"NEW MAP IN : {timeRemaining.Hours}h {timeRemaining.Minutes}m" : "NEW MAP IN : --h --m";
        }

        private void UpdatePartyState()
        {
            if (_teamControls != null) {
                if (_isInParty) _teamControls.RemoveFromClassList("hidden");
                else _teamControls.AddToClassList("hidden");
            }
        }

        private void UpdateActionButton()
        {
            if (_playButton == null || _actionButtonText == null) return;
            if (_isInParty) {
                _actionButtonText.text = _isReady ? "CANCEL" : "I'M READY";
                if (_isReady) _playButton.AddToClassList("ready");
                else _playButton.RemoveFromClassList("ready");
            } else {
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
            if (slot.PlayerName != null) slot.PlayerName.text = playerName;
            if (slot.PlayerStatus != null) slot.PlayerStatus.text = status;
            if (slot.ReadyIndicator != null) {
                if (isReady) slot.ReadyIndicator.RemoveFromClassList("hidden");
                else slot.ReadyIndicator.AddToClassList("hidden");
            }
        }

        private void SetPlayerSlotEmpty(PlayerSlot slot)
        {
            if (slot == null) return;
            slot.SlotElement?.AddToClassList("empty");
            slot.SlotElement?.RemoveFromClassList("filled");
            slot.EmptyContent?.RemoveFromClassList("hidden");
            slot.StatsContainer?.AddToClassList("hidden");
        }

        private void OnPlayClicked()
        {
            if (_isInParty) {
                _isReady = !_isReady;
                UpdateActionButton();
                if (AreAllPlayersReady()) StartPartyMatchmaking();
            } else {
                _stateManager?.ChangeState<MatchmakingState>();
            }
        }

        private void StartPartyMatchmaking() => _stateManager?.ChangeState<MatchmakingState>();

        private void OnMapClicked()
        {
            var mapScreen = _uiService?.GetScreen<MapSelectionScreen>();
            mapScreen?.ShowWithCallback(OnMapSelected);
        }

        private void OnMapSelected(MapInfo map)
        {
            _currentMap = map;
            UpdateMapDisplay(map);
            if (map.maxPlayers > 0) SetTeamSize(map.maxPlayers);
        }

        private void OnLeaveClicked()
        {
            _isInParty = false;
            _isReady = false;
            _currentPlayerCount = 1;
            for (int i = 1; i < _playerSlots.Count; i++) SetPlayerSlotEmpty(_playerSlots[i]);
            UpdatePartyState();
            UpdateActionButton();
        }

        private void OnAddPlayerClicked(int slotIndex) => _uiService?.ShowScreen(UIScreenType.FriendsPopup);

        public bool AreAllPlayersReady()
        {
            if (!_isInParty) return false;
            for (int i = 0; i < _currentPlayerCount; i++) if (_playerSlots[i].ReadyIndicator?.ClassListContains("hidden") != false) return false;
            return true;
        }

        public void Update(float deltaTime) { }

        private void HideAnimatedElements() { }

        public void PlayIntroAnimations(IUIAnimator animator) { }

        public void Dispose()
        {
            if (_playButton != null) _playButton.clicked -= OnPlayClicked;
            if (_mapButton != null) _mapButton.clicked -= OnMapClicked;
            if (_leaveButton != null) _leaveButton.clicked -= OnLeaveClicked;
            _buttonsInitialized = false;
        }

        private class PlayerSlot {
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