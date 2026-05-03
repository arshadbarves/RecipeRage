using KitchenClash.Infrastructure.Persistence;
using KitchenClash.Application.Services;
using KitchenClash.Infrastructure.DI;
using KitchenClash.Presentation;
using KitchenClash.Presentation.Common;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.EOS;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace KitchenClash.Presentation.Screens
{
    /// <summary>
    /// User profile view - shows player stats, rank, and friend code
    /// </summary>
    [UIScreen(UIScreenCategory.Screen, "Screens/ProfileViewTemplate")]
    public class ProfileScreen : BaseUIScreen
    {
        [Inject] private ISessionContext _sessionContext;

        private Label _playerNameLabel;
        private Label _playerLevelLabel;
        private Label _friendCodeLabel;
        private Label _totalMatchesLabel;
        private Label _winsLabel;
        private Label _lossesLabel;
        private Label _winRateLabel;
        private Label _favoriteChefLabel;
        private Label _xpLabel;
        private Label _rankLabel;
        private Button _copyCodeButton;
        private Button _changeNameButton;
        private Button _backButton;

        protected override void OnInitialize()
        {
            _playerNameLabel = GetElement<Label>("player-name");
            _playerLevelLabel = GetElement<Label>("player-level");
            _friendCodeLabel = GetElement<Label>("friend-code-value");
            _totalMatchesLabel = GetElement<Label>("total-matches");
            _winsLabel = GetElement<Label>("wins");
            _lossesLabel = GetElement<Label>("losses");
            _winRateLabel = GetElement<Label>("win-rate");
            _favoriteChefLabel = GetElement<Label>("favorite-chef");
            _xpLabel = GetElement<Label>("xp-progress");
            _rankLabel = GetElement<Label>("rank-tier");
            _copyCodeButton = GetElement<Button>("copy-code-button");
            _changeNameButton = GetElement<Button>("change-name-button");
            _backButton = GetElement<Button>("back-button");

            if (_copyCodeButton != null) _copyCodeButton.clicked += OnCopyCodeClicked;
            if (_changeNameButton != null) _changeNameButton.clicked += OnChangeNameClicked;
            if (_backButton != null) _backButton.clicked += OnBackClicked;
        }

        protected override void OnShow()
        {
            UpdatePlayerInfo();
            UpdateFriendCode();
        }

        private void UpdatePlayerInfo()
        {
            if (!_sessionContext.IsSessionActive) return;

            var playerDataService = _sessionContext.PlayerDataService;
            if (playerDataService == null) return;

            var stats = playerDataService.GetStats();
            var progress = playerDataService.GetProgress();

            if (_playerNameLabel != null)
                _playerNameLabel.text = string.IsNullOrEmpty(stats?.PlayerName) ? "GUEST" : stats.PlayerName.ToUpper();

            if (_playerLevelLabel != null)
                _playerLevelLabel.text = $"LEVEL {stats?.Level ?? 1}";

            if (_xpLabel != null && stats != null)
                _xpLabel.text = $"{stats.Experience} / {stats.ExperienceToNextLevel} XP";

            if (_totalMatchesLabel != null && stats != null)
                _totalMatchesLabel.text = $"{stats.GamesPlayed}";

            if (_winsLabel != null && stats != null)
                _winsLabel.text = $"{stats.GamesWon}";

            if (_lossesLabel != null && stats != null)
                _lossesLabel.text = $"{stats.GamesLost}";

            if (_winRateLabel != null && stats != null)
            {
                float rate = stats.GamesPlayed > 0 ? (float)stats.GamesWon / stats.GamesPlayed * 100f : 0f;
                _winRateLabel.text = $"{rate:F1}%";
            }

            if (_favoriteChefLabel != null && stats != null)
                _favoriteChefLabel.text = string.IsNullOrEmpty(stats.FavoriteCharacter) ? "—" : stats.FavoriteCharacter;

            if (_rankLabel != null)
                _rankLabel.text = "Unranked"; // Ranked system not yet implemented
        }

        private void UpdateFriendCode()
        {
            if (_friendCodeLabel == null) return;

            var friendsService = _sessionContext.FriendsService;
            if (friendsService != null)
            {
                _friendCodeLabel.text = friendsService.IsInitialized ? friendsService.MyFriendCode : "Loading...";
            }
            else
            {
                _friendCodeLabel.text = "Loading...";
            }
        }

        private void OnCopyCodeClicked()
        {
            var friendsService = _sessionContext.FriendsService;
            if (friendsService != null && friendsService.IsInitialized)
            {
                GUIUtility.systemCopyBuffer = friendsService.MyFriendCode;
                UIService?.ShowNotification("Friend Code copied to clipboard!", NotificationType.Success);
            }
        }

        private void OnChangeNameClicked()
        {
            UIService?.PushModal<Overlays.UsernamePopup>();
        }

        private void OnBackClicked() => UIService?.GoBack();

        protected override void OnDispose()
        {
            if (_copyCodeButton != null) _copyCodeButton.clicked -= OnCopyCodeClicked;
            if (_changeNameButton != null) _changeNameButton.clicked -= OnChangeNameClicked;
            if (_backButton != null) _backButton.clicked -= OnBackClicked;
        }
    }
}
