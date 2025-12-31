using Core.Bootstrap;
using Core.Networking.Common;
using Core.Networking.Interfaces;
using UI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Core.Logging;
using Core.Networking;
using VContainer;

namespace UI.Screens
{
    /// <summary>
    /// Dedicated matchmaking screen shown during player search
    /// Replaces the widget overlay approach with a full screen
    /// </summary>
    [UIScreen(UIScreenType.Matchmaking, UIScreenCategory.Screen, "Screens/MatchmakingTemplate")]
    public class MatchmakingScreen : BaseUIScreen
    {
        [Inject]
        private SessionManager _sessionManager;

        // UI Elements
        private Label _statusText;
        private Label _playerCountText;
        private Label _searchTimeText;
        private Button _cancelButton;
        private VisualElement _statusIndicator;
        private VisualElement _playerListContainer;

        // State
        private IMatchmakingService _matchmakingService;
        private float _searchTime;

        protected override void OnInitialize()
        {
            // Get matchmaking service from injected session
            var sessionContainer = _sessionManager?.SessionContainer;
            if (sessionContainer != null)
            {
                var networking = sessionContainer.Resolve<INetworkingServices>();
                _matchmakingService = networking?.MatchmakingService;
            }

            _statusText = GetElement<Label>("status-text");
            _playerCountText = GetElement<Label>("player-count");
            _searchTimeText = GetElement<Label>("search-time");
            _cancelButton = GetElement<Button>("cancel-button");
            _statusIndicator = GetElement<VisualElement>("status-indicator");
            _playerListContainer = GetElement<VisualElement>("player-list");

            if (_cancelButton != null)
            {
                _cancelButton.clicked += OnCancelClicked;
            }

            if (_matchmakingService != null)
            {
                _matchmakingService.OnPlayersFound += OnPlayersFoundUpdated;
                _matchmakingService.OnMatchFound += OnMatchFound;
            }
        }

        protected override void OnShow()
        {
            _searchTime = 0f;
            if (_statusText != null) _statusText.text = "Searching for players...";
            if (_statusIndicator != null)
            {
                _statusIndicator.RemoveFromClassList("found");
                _statusIndicator.AddToClassList("searching");
            }
        }

        public override void Update(float deltaTime)
        {
            if (_matchmakingService != null && _matchmakingService.IsSearching)
            {
                _searchTime += deltaTime;
                UpdateSearchTimeDisplay();
            }
        }

        protected override void OnDispose()
        {
            if (_matchmakingService != null)
            {
                _matchmakingService.OnPlayersFound -= OnPlayersFoundUpdated;
                _matchmakingService.OnMatchFound -= OnMatchFound;
            }

            if (_cancelButton != null)
            {
                _cancelButton.clicked -= OnCancelClicked;
            }
        }

        private void OnCancelClicked()
        {
            _matchmakingService?.CancelMatchmaking();
        }

        private void OnPlayersFoundUpdated(int current, int required)
        {
            if (_playerCountText != null) _playerCountText.text = $"{current}/{required}";
        }

        private void OnMatchFound(LobbyInfo lobbyInfo)
        {
            if (_statusText != null) _statusText.text = "Match Found!";
            if (_statusIndicator != null)
            {
                _statusIndicator.RemoveFromClassList("searching");
                _statusIndicator.AddToClassList("found");
            }
        }

        private void UpdateSearchTimeDisplay()
        {
            if (_searchTimeText != null)
            {
                int minutes = Mathf.FloorToInt(_searchTime / 60f);
                int seconds = Mathf.FloorToInt(_searchTime % 60f);
                _searchTimeText.text = $"{minutes}:{seconds:00}";
            }
        }
    }
}