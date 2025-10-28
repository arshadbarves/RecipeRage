using Core.Networking.Common;
using Core.Networking.Interfaces;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Components
{
    /// <summary>
    /// Pure C# matchmaking widget component (PUBG/Fortnite style)
    /// Shows on main menu when matchmaking starts
    /// No MonoBehaviour dependency - uses pure C# with dependency injection
    /// </summary>
    public class MatchmakingWidgetComponent
    {
        private VisualElement _root;
        private VisualElement _widget;
        private Label _statusText;
        private Label _playerCountText;
        private Label _searchTimeText;
        private Button _cancelButton;
        private VisualElement _statusIndicator;

        private readonly IMatchmakingService _matchmakingService;
        private bool _isVisible = false;
        private float _searchTime = 0f;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public MatchmakingWidgetComponent(IMatchmakingService matchmakingService)
        {
            _matchmakingService = matchmakingService ?? throw new System.ArgumentNullException(nameof(matchmakingService));
        }

        /// <summary>
        /// Initialize the widget with a root element
        /// </summary>
        public void Initialize(VisualElement rootElement)
        {
            _root = rootElement;

            // Load widget template
            VisualTreeAsset widgetTemplate = Resources.Load<VisualTreeAsset>("UI/Templates/Components/MatchmakingWidget");
            StyleSheet widgetStyles = Resources.Load<StyleSheet>("UI/Styles/Components/MatchmakingWidget");

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

                Debug.Log("[MatchmakingWidgetComponent] Initialized");
            }
            else
            {
                Debug.LogError("[MatchmakingWidgetComponent] Failed to load widget template");
            }

            // Subscribe to matchmaking events
            _matchmakingService.OnMatchmakingStarted += OnMatchmakingStarted;
            _matchmakingService.OnMatchmakingCancelled += OnMatchmakingCancelled;
            _matchmakingService.OnPlayersFound += OnPlayersFoundUpdated;
            _matchmakingService.OnMatchFound += OnMatchFound;
        }

        /// <summary>
        /// Update method - call this from parent component's Update
        /// </summary>
        public void Update(float deltaTime)
        {
            if (_isVisible && _matchmakingService.IsSearching)
            {
                _searchTime += deltaTime;
                int minutes = Mathf.FloorToInt(_searchTime / 60f);
                int seconds = Mathf.FloorToInt(_searchTime % 60f);

                if (_searchTimeText != null)
                {
                    _searchTimeText.text = $"{minutes}:{seconds:00}";
                }
            }
        }

        private void Show()
        {
            if (_widget != null)
            {
                _widget.style.display = DisplayStyle.Flex;
                _isVisible = true;
                _searchTime = 0f;
                Debug.Log("[MatchmakingWidgetComponent] Shown");
            }
        }

        private void Hide()
        {
            if (_widget != null)
            {
                _widget.style.display = DisplayStyle.None;
                _isVisible = false;
                _searchTime = 0f;
                Debug.Log("[MatchmakingWidgetComponent] Hidden");
            }
        }

        private void OnCancelClicked()
        {
            Debug.Log("[MatchmakingWidgetComponent] Cancel clicked");
            _matchmakingService.CancelMatchmaking();
        }

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

        private void OnMatchmakingCancelled()
        {
            Hide();
        }

        private void OnPlayersFoundUpdated(int current, int required)
        {
            if (_playerCountText != null)
            {
                _playerCountText.text = $"{current}/{required}";
            }
        }

        private void OnMatchFound(LobbyInfo lobbyInfo)
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

            // Hide after a short delay (using a simple timer)
            // In a real implementation, you might want to use UniTask.Delay
        }

        public void Dispose()
        {
            // Unsubscribe from events
            _matchmakingService.OnMatchmakingStarted -= OnMatchmakingStarted;
            _matchmakingService.OnMatchmakingCancelled -= OnMatchmakingCancelled;
            _matchmakingService.OnPlayersFound -= OnPlayersFoundUpdated;
            _matchmakingService.OnMatchFound -= OnMatchFound;

            // Remove button callback
            if (_cancelButton != null)
            {
                _cancelButton.clicked -= OnCancelClicked;
            }

            // Remove from root
            if (_widget != null && _root != null)
            {
                _root.Remove(_widget);
            }

            Debug.Log("[MatchmakingWidgetComponent] Disposed");
        }
    }
}
