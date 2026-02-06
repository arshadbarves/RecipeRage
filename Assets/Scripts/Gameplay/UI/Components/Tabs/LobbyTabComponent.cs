using Gameplay.UI.Features.Lobby;
using Gameplay.UI.Features.Maps;
using Gameplay.UI.Features.Social;
using UnityEngine.UIElements;
using Core.Animation;
using Core.Networking.Common;
using Core.UI.Interfaces;
using Gameplay.GameModes;
using Gameplay.UI.Data;
using Gameplay.UI.Features.Settings;
using Gameplay.UI.Features.Character;
using Gameplay.Characters;
using Gameplay.Skins; // Added
using Gameplay.UI.Extensions;
using Gameplay.UI.Localization;
using VContainer;
using UnityEngine; // For GameObject

namespace Gameplay.UI.Components.Tabs
{
    public class LobbyTabComponent
    {
        [Inject] private IUIService _uiService;
        [Inject] private IAnimationService _animationService;
        [Inject] private Core.Localization.ILocalizationManager _localizationManager;
        [Inject] private ICharacterService _characterService;
        [Inject] private ISkinsService _skinsService; // Added
        [Inject] private Gameplay.Characters.Visuals.CharacterPreviewManager _previewManager;


        private VisualElement _root;
        private readonly LobbyViewModel _viewModel;

        // UI Elements
        private Button _playButton;
        private Button _mapSelector;
        private Label _mapNameLabel;
        private Label _regionInfo;
        private Button _settingsButton;
        private Button _skinsButton;

        // Squad slots
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
            SubscribeEvents(); // Added
        }

        public void RefreshLocalization()
        {
            BindLocalization();
        }

        private void BindLocalization()
        {
            if (_localizationManager == null || _root == null) return;

            // Play Button
            _localizationManager.Bind(_playButton, LocKeys.LobbyPlayButton, this);

            // Region - Register a binding to update when language changes
            _localizationManager.RegisterBinding(this, LocKeys.LobbyRegionPrefix, _ => RefreshRegionInfo());

            // Game Mode label
            var mapInfo = _root.Q<VisualElement>(className: "map-info");
            _localizationManager.Bind(mapInfo?.Q<Label>(className: "map-mode-label"), LocKeys.LobbyGameMode, this);

            // Squad Slots (Invite Text)
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

            // Query squad slots
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

            // Setup empty squad slots to open friends
            for (int i = 1; i < 4; i++) // Skip slot 0 (player)
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
            // Set up slot 0 as the local player
            if (_squadSlots[0] != null)
            {
                _squadSlots[0].RemoveFromClassList("empty");
                _squadSlots[0].AddToClassList("filled");
                UpdateLocalPlayerModel();
            }
        }

        private void SubscribeEvents()
        {
            if (_characterService != null) _characterService.OnCharacterSelected += OnCharacterSelected;
            if (_skinsService != null) _skinsService.OnSkinEquipped += OnSkinEquipped;
        }

        private void UnsubscribeEvents()
        {
            if (_characterService != null) _characterService.OnCharacterSelected -= OnCharacterSelected;
            if (_skinsService != null) _skinsService.OnSkinEquipped -= OnSkinEquipped;
        }

        private void OnCharacterSelected(Gameplay.Characters.CharacterClass character) => UpdateLocalPlayerModel();
        private void OnSkinEquipped(int charId, string skinId) => UpdateLocalPlayerModel();

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
             
             // Check equipped skin
             if (_skinsService != null)
             {
                 var equippedSkin = _skinsService.GetEquippedSkin(selectedCharacter.Id);
                 if (equippedSkin != null) prefabToSpawn = equippedSkin.prefab;
             }
             
             // Fallback to default skin
             if (prefabToSpawn == null)
             {
                 // Assuming accessing Skins list directly if needed, or SkinsService should handle default logic
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
            var mapScreen = _uiService?.GetScreen<MapSelectionView>();
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
            _uiService?.Show<SettingsView>(false);
        }

        private void OnSkinsClicked()
        {
            if (_uiService == null) return;
            _uiService.Show<CharacterSelectionView>();
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
            
            // Should we clear the lobby character on dispose? 
            // Yes, if we leave the main menu (which calls Dispose on MainMenuState -> MainMenuView -> LobbyTabComponent)
            _previewManager?.ClearLobbyCharacter();
        }

        private void RefreshRegionInfo()
        {
            if (_regionInfo == null || _localizationManager == null) return;

            string prefix = _localizationManager.GetText(LocKeys.LobbyRegionPrefix);
            // hardcoding NA for now as per previous logic, but making it localized
            string region = _localizationManager.GetText(LocKeys.LobbyRegionNa) ?? "NORTH AMERICA";

            _regionInfo.text = $"{prefix}: {region} (24ms)";
        }
    }
}