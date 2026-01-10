using Gameplay.UI.Features.Lobby;
using Gameplay.UI.Features.Maps;
using Gameplay.UI.Features.Social;
using UnityEngine.UIElements;
using Core.Animation;
using Core.UI.Interfaces;
using Gameplay.UI.Data;
using VContainer;

namespace Gameplay.UI.Components.Tabs
{
    public class LobbyTabComponent
    {
        [Inject] private IUIService _uiService;
        [Inject] private IAnimationService _animationService;


        private VisualElement _root;
        private readonly LobbyViewModel _viewModel;

        // UI Elements
        private Button _playButton;
        private Button _mapSelector;
        private Label _mapNameLabel;
        private Label _regionInfo;

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
        }

        private void QueryElements()
        {
            _playButton = _root.Q<Button>("play-button");
            _mapSelector = _root.Q<Button>("map-selector");
            _mapNameLabel = _root.Q<Label>("map-name");
            _regionInfo = _root.Q<Label>("region-info");

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
            }
        }

        private void OnPlayClicked()
        {
            _viewModel.Play();
        }

        private void OnMapClicked()
        {
            var mapScreen = _uiService?.GetScreen<MapSelectionScreen>();
            mapScreen?.ShowWithCallback(OnMapSelected);
        }

        private void OnMapSelected(MapInfo map)
        {
            if (_mapNameLabel != null) _mapNameLabel.text = map.name.ToUpper();
        }

        private void OnInviteClicked(int slotIndex)
        {
            _uiService?.Show<FriendsPopup>();
        }

        public void Update(float deltaTime) { }

        public void PlayIntroAnimations(IUIAnimator animator) { }

        public void Dispose()
        {
            if (_playButton != null) _playButton.clicked -= OnPlayClicked;
            if (_mapSelector != null) _mapSelector.clicked -= OnMapClicked;
            _buttonsInitialized = false;
        }
    }
}