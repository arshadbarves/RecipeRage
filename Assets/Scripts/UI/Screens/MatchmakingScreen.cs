using Core.Bootstrap;
using Core.Networking.Common;
using Core.Networking.Interfaces;
using UI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Core.Logging;

namespace UI.Screens
{
    /// <summary>
    /// Dedicated matchmaking screen shown during player search
    /// Replaces the widget overlay approach with a full screen
    /// </summary>
    [UIScreen(UIScreenType.Matchmaking, UIScreenCategory.Screen, "Screens/MatchmakingTemplate")]
    public class MatchmakingScreen : BaseUIScreen
    {
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
            // Get matchmaking service
            _matchmakingService = GameBootstrap.Services?.Session?.NetworkingServices?.MatchmakingService;

            // Query UI elements
            _statusText = GetElement<Label>("status-text");
            _playerCountText = GetElement<Label>("player-count");
            _searchTimeText = GetElement<Label>("search-time");
            _cancelButton = GetElement<Button>("cancel-button");
            _statusIndicator = GetElement<VisualElement>("status-indicator");
            _playerListContainer = GetElement<VisualElement>("player-list");

            // Setup cancel button
            if (_cancelButton != null)
            {
                _cancelButton.clicked += OnCancelClicked;
            }

            // Subscribe to matchmaking events
            if (_matchmakingService != null)
            {
                _matchmakingService.OnPlayersFound += OnPlayersFoundUpdated;
                _matchmakingService.OnMatchFound += OnMatchFound;
            }

            GameLogger.Log("Initialized");
        }

        protected override void OnShow()
        {
            _searchTime = 0f;

            // Set initial state
            if (_statusText != null)
            {
                _statusText.text = "Searching for players...";
            }

            if (_statusIndicator != null)
            {
                _statusIndicator.RemoveFromClassList("found");
                _statusIndicator.AddToClassList("searching");
            }

            GameLogger.Log("Shown");
        }

        protected override void OnHide()
        {
            GameLogger.Log("Hidden");
        }

        public override void Update(float deltaTime)
        {
            // Update search time display
            if (_matchmakingService != null && _matchmakingService.IsSearching)
            {
                _searchTime += deltaTime;
                UpdateSearchTimeDisplay();
            }
        }

        protected override void OnDispose()
        {
            // Unsubscribe from events
            if (_matchmakingService != null)
            {
                _matchmakingService.OnPlayersFound -= OnPlayersFoundUpdated;
                _matchmakingService.OnMatchFound -= OnMatchFound;
            }

            // Remove button callback
            if (_cancelButton != null)
            {
                _cancelButton.clicked -= OnCancelClicked;
            }

            GameLogger.Log("Disposed");
        }

        #region Event Handlers

        private void OnCancelClicked()
        {
            GameLogger.Log("Cancel button clicked");

            // Cancel matchmaking via service
            if (_matchmakingService != null)
            {
                _matchmakingService.CancelMatchmaking();
            }

            // Return to main menu (MatchmakingState will handle this via event)
        }

        private void OnPlayersFoundUpdated(int current, int required)
        {
            if (_playerCountText != null)
            {
                _playerCountText.text = $"{current}/{required}";
            }

            GameLogger.Log($"Players: {current}/{required}");
        }

        private void OnMatchFound(LobbyInfo lobbyInfo)
        {
            GameLogger.Log($"Match found! Lobby: {lobbyInfo.LobbyId}");

            if (_statusText != null)
            {
                _statusText.text = "Match Found!";
            }

            if (_statusIndicator != null)
            {
                _statusIndicator.RemoveFromClassList("searching");
                _statusIndicator.AddToClassList("found");
            }

            // Note: State transition is handled by MatchmakingState
        }

        #endregion

        #region Helper Methods

        private void UpdateSearchTimeDisplay()
        {
            if (_searchTimeText != null)
            {
                int minutes = Mathf.FloorToInt(_searchTime / 60f);
                int seconds = Mathf.FloorToInt(_searchTime % 60f);
                _searchTimeText.text = $"{minutes}:{seconds:00}";
            }
        }

        #endregion
    }
}
