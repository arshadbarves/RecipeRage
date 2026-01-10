using Core.Persistence;
using Core.UI;
using Core.UI.Core;
using Core.UI.Interfaces;
using Core.Networking;
using Core.Session;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Gameplay.UI.Screens
{
    /// <summary>
    /// User profile screen - shows player stats and friend code
    /// </summary>
    [UIScreen(UIScreenCategory.Screen, "Screens/ProfileTemplate")]
    public class ProfileScreen : BaseUIScreen
    {
        [Inject] private SessionManager _sessionManager;
        [Inject] private ISaveService _saveService;

        private Label _playerNameLabel;
        private Label _playerLevelLabel;
        private Label _friendCodeLabel;
        private Button _copyCodeButton;
        private Button _changeNameButton;
        private Button _backButton;

        protected override void OnInitialize()
        {
            _playerNameLabel = GetElement<Label>("player-name");
            _playerLevelLabel = GetElement<Label>("player-level");
            _friendCodeLabel = GetElement<Label>("friend-code-value");
            _copyCodeButton = GetElement<Button>("copy-code-button");
            _changeNameButton = GetElement<Button>("change-name-button");
            _backButton = GetElement<Button>("back-button");

            if (_copyCodeButton != null)
                _copyCodeButton.clicked += OnCopyCodeClicked;

            if (_changeNameButton != null)
                _changeNameButton.clicked += OnChangeNameClicked;

            if (_backButton != null)
                _backButton.clicked += OnBackClicked;
        }

        protected override void OnShow()
        {
            UpdatePlayerInfo();
            UpdateFriendCode();
        }

        private void UpdatePlayerInfo()
        {
            if (_saveService == null) return;

            var stats = _saveService.GetPlayerStats();
            var progress = _saveService.GetPlayerProgress();

            if (_playerNameLabel != null)
                _playerNameLabel.text = string.IsNullOrEmpty(stats.PlayerName) ? "GUEST" : stats.PlayerName.ToUpper();

            if (_playerLevelLabel != null)
                _playerLevelLabel.text = $"LEVEL {progress.HighestLevel}";
        }

        private void UpdateFriendCode()
        {
            if (_friendCodeLabel == null)
                return;

            var sessionContainer = _sessionManager?.SessionContainer;
            if (sessionContainer != null)
            {
                var networking = sessionContainer.Resolve<INetworkingServices>();
                var friendsService = networking?.FriendsService;

                if (friendsService != null && friendsService.IsInitialized)
                {
                    _friendCodeLabel.text = friendsService.MyFriendCode;
                }
                else
                {
                    _friendCodeLabel.text = "Loading...";
                }
            }
        }

        private void OnCopyCodeClicked()
        {
            var sessionContainer = _sessionManager?.SessionContainer;
            if (sessionContainer != null)
            {
                var networking = sessionContainer.Resolve<INetworkingServices>();
                var friendsService = networking?.FriendsService;

                if (friendsService != null && friendsService.IsInitialized)
                {
                    GUIUtility.systemCopyBuffer = friendsService.MyFriendCode;
                    UIService?.ShowNotification("Friend Code copied to clipboard!", NotificationType.Success);
                }
            }
        }

        private void OnChangeNameClicked(){
            // TODO: Show username popup - need to create UsernamePopup class or use existing
        }

        private void OnBackClicked()
        {
            UIService?.GoBack();
        }

        protected override void OnDispose()
        {
            if (_copyCodeButton != null)
                _copyCodeButton.clicked -= OnCopyCodeClicked;

            if (_changeNameButton != null)
                _changeNameButton.clicked -= OnChangeNameClicked;

            if (_backButton != null)
                _backButton.clicked -= OnBackClicked;
        }
    }
}