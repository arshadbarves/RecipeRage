using KitchenClash.Application.Models;
using KitchenClash.Application;
using KitchenClash.Presentation;
using KitchenClash.Presentation.Common;
using KitchenClash.Presentation.Overlays;
using KitchenClash.Presentation.Screens;
using KitchenClash.Presentation.ViewModels;
using KitchenClash.Infrastructure.Animation;
using KitchenClash.Infrastructure.Network;
using KitchenClash.Infrastructure.Localization;
using KitchenClash.Domain;
using KitchenClash.Application.Services;
using KitchenClash.Presentation.Extensions;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace KitchenClash.Presentation.Components
{
    public class LobbyTabComponent
    {
        [Inject] private IUIService _uiService;
        [Inject] private IAnimationService _animationService;
        [Inject] private ILocalizationManager _localizationManager;
        [Inject] private ICharacterService _characterService;
        [Inject] private ISkinsService _skinsService;
        [Inject] private CharacterPreviewManager _previewManager;
        [Inject] private IEventBus _eventBus;

        private VisualElement _root;
        private readonly LobbyViewModel _viewModel;

        private Button _playButton;
        private Button _mapSelector;
        private Label _mapNameLabel;
        private Label _regionInfo;
        private Button _settingsButton;
        private Button _skinsButton;

        private Button[] _squadSlots = new Button[4];
        private Label[] _playerNames = new Label[4];
        private Label[] _playerStatuses = new Label[4];

        private bool _buttonsInitialized = false;

        public LobbyTabComponent(LobbyViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public void Initialize(VisualElement root)
        {
            if (root == null) return;
            _root = root;
            QueryElements();
            SetupButtons();
            BindViewModel();
            SetupLocalPlayer();
            BindLocalization();
            SubscribeEvents();
        }

        public void RefreshLocalization()
        {
            BindLocalization();
        }

        private void BindLocalization()
        {
            if (_localizationManager == null || _root == null) return;

            _localizationManager.Bind(_playButton, LocKeys.LobbyPlayButton, this);
            _localizationManager.RegisterBinding(this, LocKeys.LobbyRegionPrefix, _ => RefreshRegionInfo());

            var mapInfo = _root.Q<VisualElement>(className: "map-info");
            _localizationManager.Bind(mapInfo?.Q<Label>(className: "map-mode-label"), LocKeys.LobbyGameMode, this);

            for (int i = 1; i < 4; i++)
            {
                var slot = _squadSlots[i];
                if (slot != null)
                {
                    _localizationManager.Bind(slot.Q<Label>(className: "invite-label"), LocKeys.LobbyInvite, this);
                }
            }
        }

        private void QueryElements()
        {
            _playButton = _root.Q<Button>("play-button");
            _mapSelector = _root.Q<Button>("map-selector");
            _mapNameLabel = _root.Q<Label>("map-name");
            _regionInfo = _root.Q<Label>("region-info");
            _settingsButton = _root.Q<Button>("settings-btn");
            _skinsButton = _root.Q<Button>("skins-btn");

            for (int i = 0; i < 4; i++)
            {
                _squadSlots[i] = _root.Q<Button>($"squad-slot-{i + 1}");
                _playerNames[i] = _root.Q<Label>($"player-name-{i + 1}");
                _playerStatuses[i] = _root.Q<Label>($"player-status-{i + 1}");
            }
        }

        private void SetupButtons()
        {
            if (_buttonsInitialized) return;

            if (_playButton != null) _playButton.clicked += OnPlayClicked;
            if (_mapSelector != null) _mapSelector.clicked += OnMapClicked;
            if (_settingsButton != null) _settingsButton.clicked += OnSettingsClicked;
            if (_skinsButton != null) _skinsButton.clicked += OnSkinsClicked;

            for (int i = 1; i < 4; i++)
            {
                int slotIndex = i;
                if (_squadSlots[i] != null && _squadSlots[i].ClassListContains("empty"))
                {
                    _squadSlots[i].clicked += () => OnInviteClicked(slotIndex);
                }
            }

            _buttonsInitialized = true;
        }

        private void BindViewModel()
        {
            _viewModel.MapName.Bind(name => { if (_mapNameLabel != null) _mapNameLabel.text = name; });
        }

        private void SetupLocalPlayer()
        {
            if (_squadSlots[0] != null)
            {
                _squadSlots[0].RemoveFromClassList("empty");
                _squadSlots[0].AddToClassList("filled");
                UpdateLocalPlayerModel();
            }
        }

        private void SubscribeEvents()
        {
            if (_eventBus == null) return;
            _eventBus.Subscribe<CharacterSelectedEvent>(OnCharacterSelected);
            _eventBus.Subscribe<SkinEquippedEvent>(OnSkinEquipped);
        }

        private void UnsubscribeEvents()
        {
            if (_eventBus == null) return;
            _eventBus.Unsubscribe<CharacterSelectedEvent>(OnCharacterSelected);
            _eventBus.Unsubscribe<SkinEquippedEvent>(OnSkinEquipped);
        }

        private void OnCharacterSelected(CharacterSelectedEvent evt) => UpdateLocalPlayerModel();
        private void OnSkinEquipped(SkinEquippedEvent evt) => UpdateLocalPlayerModel();

        private void UpdateLocalPlayerModel()
        {
            if (_characterService == null || _previewManager == null) return;
             
            var selectedCharacter = _characterService.SelectedCharacter;
            if (selectedCharacter == null) 
            {
                _previewManager.ClearLobbyCharacter(0);
                return;
            }

            GameObject prefabToSpawn = null;
             
            if (_skinsService != null)
            {
                var equippedSkin = _skinsService.GetEquippedSkin(selectedCharacter.Id);
                if (equippedSkin != null) prefabToSpawn = equippedSkin.prefab;
            }
             
            if (prefabToSpawn == null)
            {
                var defaultSkin = selectedCharacter.Skins.Find(s => s.isDefault);
                if (defaultSkin != null) prefabToSpawn = defaultSkin.prefab;
            }

            if (prefabToSpawn != null)
            {
                _previewManager.ShowLobbyCharacter(0, prefabToSpawn);
            }
        }

        private void OnPlayClicked()
        {
            _viewModel.Play();
        }

        private void OnMapClicked()
        {
            var mapScreen = _uiService?.GetScreen<MapSelectionScreen>();
            mapScreen?.ShowWithCallback(OnGameModeSelected);
        }

        private void OnGameModeSelected(GameMode mode)
        {
            if (_mapNameLabel != null) _mapNameLabel.text = mode.DisplayName.ToUpper();
        }

        private void OnInviteClicked(int slotIndex)
        {
            _uiService?.Show<FriendsPopup>();
        }

        private void OnSettingsClicked()
        {
            _uiService?.Show<SettingsScreen>(false);
        }

        private void OnSkinsClicked()
        {
            if (_uiService == null) return;
            _uiService.Show<ChefSelectScreen>();
        }

        public void Update(float deltaTime) { }

        public void PlayIntroAnimations(IUIAnimator animator) { }

        public void Dispose()
        {
            if (_playButton != null) _playButton.clicked -= OnPlayClicked;
            if (_mapSelector != null) _mapSelector.clicked -= OnMapClicked;
            if (_settingsButton != null) _settingsButton.clicked -= OnSettingsClicked;
            if (_skinsButton != null) _skinsButton.clicked -= OnSkinsClicked;
            
            UnsubscribeEvents();
            _localizationManager?.UnregisterAll(this);
            _buttonsInitialized = false;
            
            _previewManager?.ClearLobbyCharacter();
        }

        private void RefreshRegionInfo()
        {
            if (_regionInfo == null || _localizationManager == null) return;

            string prefix = _localizationManager.GetText(LocKeys.LobbyRegionPrefix);
            string region = _localizationManager.GetText(LocKeys.LobbyRegionNa) ?? "NORTH AMERICA";

            _regionInfo.text = $"{prefix}: {region} (24ms)";
        }
    }
}
