using Core.UI;
using Gameplay.UI.ViewModels;
using Core.UI.Core;
using Core.UI.Interfaces;
using UnityEngine.UIElements;
using VContainer;

namespace Gameplay.UI.Screens
{
    /// <summary>
    /// Dedicated matchmaking screen shown during player search
    /// Replaces the widget overlay approach with a full screen
    /// </summary>
    [UIScreen(UIScreenCategory.Screen, "Screens/MatchmakingTemplate")]
    public class MatchmakingScreen : BaseUIScreen
    {
        [Inject] private MatchmakingViewModel _viewModel;

        // UI Elements
        private Label _statusText;
        private Label _playerCountText;
        private Label _searchTimeText;
        private Button _cancelButton;
        private VisualElement _statusIndicator;

        protected override void OnInitialize()
        {
            _statusText = GetElement<Label>("status-text");
            _playerCountText = GetElement<Label>("player-count");
            _searchTimeText = GetElement<Label>("search-time");
            _cancelButton = GetElement<Button>("cancel-button");
            _statusIndicator = GetElement<VisualElement>("status-indicator");

            TransitionType = UITransitionType.Fade;

            if (_cancelButton != null)
            {
                _cancelButton.clicked += OnCancelClicked;
            }

            BindViewModel();
        }

        private void BindViewModel()
        {
            if (_viewModel == null) return;

            _viewModel.Initialize();

            _viewModel.StatusText.Bind(text => { if (_statusText != null) _statusText.text = text; });
            _viewModel.PlayerCountText.Bind(text => { if (_playerCountText != null) _playerCountText.text = text; });
            _viewModel.SearchTimeText.Bind(text => { if (_searchTimeText != null) _searchTimeText.text = text; });
            
            _viewModel.IsMatchFound.Bind(found => 
            {
                if (_statusIndicator != null)
                {
                    if (found)
                    {
                        _statusIndicator.RemoveFromClassList("searching");
                        _statusIndicator.AddToClassList("found");
                    }
                    else
                    {
                        _statusIndicator.RemoveFromClassList("found");
                        _statusIndicator.AddToClassList("searching");
                    }
                }
            });
        }

        private void OnCancelClicked()
        {
            _viewModel?.CancelMatchmaking();
        }

        protected override void OnDispose()
        {
            if (_cancelButton != null)
            {
                _cancelButton.clicked -= OnCancelClicked;
            }
            _viewModel?.Dispose();
        }
    }
}