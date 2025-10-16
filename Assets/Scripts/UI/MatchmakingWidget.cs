using Core.Networking;
using Core.Networking.Common;
using Core.Networking.EOS;
using Core.Networking.Interfaces;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    /// <summary>
    /// Small matchmaking widget that overlays the main menu (PUBG/Fortnite style)
    /// Shows on main menu when matchmaking starts
    /// </summary>
    public class MatchmakingWidget : MonoBehaviour
    {
        private VisualElement _root;
        private VisualElement _widget;
        private Label _statusText;
        private Label _playerCountText;
        private Label _searchTimeText;
        private Button _cancelButton;
        private VisualElement _statusIndicator;

        private IMatchmakingService _matchmakingService;
        private bool _isVisible = false;

        /// <summary>
        /// Initialize the widget with a root element (usually main menu root)
        /// </summary>
        public void Initialize(VisualElement rootElement)
        {
            _root = rootElement;

            // Load widget template
            VisualTreeAsset widgetTemplate = Resources.Load<VisualTreeAsset>("UI/Templates/MatchmakingWidget");
            StyleSheet widgetStyles = Resources.Load<StyleSheet>("UI/MatchmakingWidget");

            if (widgetTemplate != null)
            {
                TemplateContainer widgetContainer = widgetTemplate.Instantiate();
                _widget = widgetContainer.Q<VisualElement>("matchmaking-widget");

                if (widgetStyles != null)
                {
                    _widget.styleSheets.Add(widgetStyles);
                }

                // Add to root
                _root.Add(_widget);

                // Get references
                _statusText = _widget.Q<Label>("status-text");
                _playerCountText = _widget.Q<Label>("player-count");
                _searchTimeText = _widget.Q<Label>("search-time");
                _cancelButton = _widget.Q<Button>("cancel-button");
                _statusIndicator = _widget.Q<VisualElement>("status-indicator");

                // Setup button
                if (_cancelButton != null)
                {
                    _cancelButton.clicked += OnCancelClicked;
                }

                // Hide by default
                Hide();

                Debug.Log("[MatchmakingWidget] Initialized");
            }
            else
            {
                Debug.LogError("[MatchmakingWidget] Failed to load widget template");
            }

            // Get matchmaking service (using interface for DIP)
            RecipeRageNetworkManager networkManager = RecipeRageNetworkManager.Instance;
            if (networkManager != null)
            {
                _matchmakingService = networkManager.LobbyManager;

                if (_matchmakingService != null)
                {
                    // Subscribe to events
                    _matchmakingService.OnMatchmakingStarted += OnMatchmakingStarted;
                    _matchmakingService.OnMatchmakingCancelled += OnMatchmakingCancelled;
                    _matchmakingService.OnPlayersFoundUpdated += OnPlayersFoundUpdated;
                    _matchmakingService.OnMatchFound += OnMatchFound;
                }
            }
        }

        /// <summary>
        /// Show the widget
        /// </summary>
        public void Show()
        {
            if (_widget != null)
            {
                _widget.style.display = DisplayStyle.Flex;
                _isVisible = true;
                Debug.Log("[MatchmakingWidget] Shown");
            }
        }

        /// <summary>
        /// Hide the widget
        /// </summary>
        public void Hide()
        {
            if (_widget != null)
            {
                _widget.style.display = DisplayStyle.None;
                _isVisible = false;
                Debug.Log("[MatchmakingWidget] Hidden");
            }
        }

        /// <summary>
        /// Update search time display
        /// </summary>
        private void Update()
        {
            if (_isVisible && _matchmakingService != null && _matchmakingService.IsSearchingForMatch)
            {
                float searchTime = _matchmakingService.SearchTime;
                int minutes = Mathf.FloorToInt(searchTime / 60f);
                int seconds = Mathf.FloorToInt(searchTime % 60f);

                if (_searchTimeText != null)
                {
                    _searchTimeText.text = $"{minutes}:{seconds:00}";
                }
            }
        }

        /// <summary>
        /// Handle cancel button click
        /// </summary>
        private void OnCancelClicked()
        {
            Debug.Log("[MatchmakingWidget] Cancel clicked");

            if (_matchmakingService != null)
            {
                _matchmakingService.CancelMatchmaking();
            }
        }

        /// <summary>
        /// Handle matchmaking started
        /// </summary>
        private void OnMatchmakingStarted()
        {
            Show();

            if (_statusText != null)
            {
                _statusText.text = "Searching for players...";
            }

            if (_statusIndicator != null)
            {
                _statusIndicator.RemoveFromClassList("found");
                _statusIndicator.AddToClassList("searching");
            }
        }

        /// <summary>
        /// Handle matchmaking cancelled
        /// </summary>
        private void OnMatchmakingCancelled()
        {
            Hide();
        }

        /// <summary>
        /// Handle players found updated
        /// </summary>
        private void OnPlayersFoundUpdated(int playerCount)
        {
            if (_playerCountText != null)
            {
                _playerCountText.text = $"{playerCount}/4";
            }
        }

        /// <summary>
        /// Handle match found
        /// </summary>
        private void OnMatchFound()
        {
            if (_statusText != null)
            {
                _statusText.text = "Match Found!";
            }

            if (_statusIndicator != null)
            {
                _statusIndicator.RemoveFromClassList("searching");
                _statusIndicator.AddToClassList("found");
            }

            // Hide after a short delay
            Invoke(nameof(Hide), 2f);
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (_matchmakingService != null)
            {
                _matchmakingService.OnMatchmakingStarted -= OnMatchmakingStarted;
                _matchmakingService.OnMatchmakingCancelled -= OnMatchmakingCancelled;
                _matchmakingService.OnPlayersFoundUpdated -= OnPlayersFoundUpdated;
                _matchmakingService.OnMatchFound -= OnMatchFound;
            }

            // Remove from root
            if (_widget != null && _root != null)
            {
                _root.Remove(_widget);
            }
        }
    }
}
