using System;
using Core.Animation;
using Core.Bootstrap;
using Core.Logging;
using Core.SaveSystem;
using UI.Core;
using UI.Popups;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Screens
{
    /// <summary>
    /// Profile screen - displays player stats and allows username changes
    /// </summary>
    [UIScreen(UIScreenType.Profile, UIScreenCategory.Screen, "Screens/ProfileTemplate")]
    public class ProfileScreen : BaseUIScreen
    {
        private ISaveService _saveService;
        private IUIService _uiService;

        // UI Elements
        private Button _backButton;
        private Button _changeNameButton;
        private Button _copyFriendCodeButton;
        private Label _playerNameLabel;
        private Label _playerLevelLabel;
        private Label _friendCodeLabel;
        private Label _gamesPlayedLabel;
        private Label _gamesWonLabel;
        private Label _winRateLabel;
        private Label _ordersCompletedLabel;
        private Label _dishesServedLabel;
        private Label _highestComboLabel;
        private Label _favoriteCharacterLabel;
        private Label _totalPlayTimeLabel;

        protected override void OnInitialize()
        {
            // Get services
            _saveService = GameBootstrap.Services?.SaveService;
            _uiService = GameBootstrap.Services?.UIService;

            // Query elements
            QueryElements();

            // Setup callbacks
            SetupCallbacks();

            // Load and display stats
            RefreshStats();

            GameLogger.Log("Initialized");
        }

        private void QueryElements()
        {
            _backButton = GetElement<Button>("back-button");
            _changeNameButton = GetElement<Button>("change-name-button");
            _copyFriendCodeButton = GetElement<Button>("copy-friend-code-button");
            _playerNameLabel = GetElement<Label>("player-name");
            _playerLevelLabel = GetElement<Label>("player-level");
            _friendCodeLabel = GetElement<Label>("friend-code");
            _gamesPlayedLabel = GetElement<Label>("games-played-value");
            _gamesWonLabel = GetElement<Label>("games-won-value");
            _winRateLabel = GetElement<Label>("win-rate-value");
            _ordersCompletedLabel = GetElement<Label>("orders-completed-value");
            _dishesServedLabel = GetElement<Label>("dishes-served-value");
            _highestComboLabel = GetElement<Label>("highest-combo-value");
            _favoriteCharacterLabel = GetElement<Label>("favorite-character-value");
            _totalPlayTimeLabel = GetElement<Label>("total-playtime-value");
        }

        private void SetupCallbacks()
        {
            if (_backButton != null)
            {
                _backButton.clicked += OnBackClicked;
            }

            if (_changeNameButton != null)
            {
                _changeNameButton.clicked += OnChangeNameClicked;
            }

            if (_copyFriendCodeButton != null)
            {
                _copyFriendCodeButton.clicked += OnCopyFriendCodeClicked;
            }
        }

        private void RefreshStats()
        {
            if (_saveService == null) return;

            var stats = _saveService.GetPlayerStats();

            // Player name and level
            if (_playerNameLabel != null)
            {
                _playerNameLabel.text = string.IsNullOrEmpty(stats.PlayerName) ? "No Name" : stats.PlayerName;
            }

            if (_playerLevelLabel != null)
            {
                _playerLevelLabel.text = $"Level {stats.Level}";
            }

            // Game stats
            if (_gamesPlayedLabel != null)
            {
                _gamesPlayedLabel.text = stats.GamesPlayed.ToString();
            }

            if (_gamesWonLabel != null)
            {
                _gamesWonLabel.text = stats.GamesWon.ToString();
            }

            if (_winRateLabel != null)
            {
                float winRate = stats.GamesPlayed > 0 ? (float)stats.GamesWon / stats.GamesPlayed * 100f : 0f;
                _winRateLabel.text = $"{winRate:F1}%";
            }

            if (_ordersCompletedLabel != null)
            {
                _ordersCompletedLabel.text = stats.TotalOrdersCompleted.ToString();
            }

            if (_dishesServedLabel != null)
            {
                _dishesServedLabel.text = stats.TotalDishesServed.ToString();
            }

            if (_highestComboLabel != null)
            {
                _highestComboLabel.text = stats.HighestCombo.ToString();
            }

            if (_favoriteCharacterLabel != null)
            {
                _favoriteCharacterLabel.text = string.IsNullOrEmpty(stats.FavoriteCharacter) ? "None" : stats.FavoriteCharacter;
            }

            if (_totalPlayTimeLabel != null)
            {
                _totalPlayTimeLabel.text = FormatPlayTime(stats.TotalPlayTime);
            }

            // Friend code
            UpdateFriendCode();
        }

        private void UpdateFriendCode()
        {
            if (_friendCodeLabel == null)
                return;

            var networking = GameBootstrap.Services?.NetworkingServices;
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

        private string FormatPlayTime(float seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);

            if (time.TotalHours >= 1)
            {
                return $"{(int)time.TotalHours}h {time.Minutes}m";
            }
            else if (time.TotalMinutes >= 1)
            {
                return $"{(int)time.TotalMinutes}m";
            }
            else
            {
                return $"{(int)time.TotalSeconds}s";
            }
        }

        private void OnBackClicked()
        {
            // Use GoBack to return to previous screen in navigation stack
            if (_uiService != null)
            {
                bool wentBack = _uiService.GoBack(true);

                if (!wentBack)
                {
                    // If no history, manually show MainMenu
                    _uiService.ShowScreen(UIScreenType.MainMenu, true, false);
                }
            }
        }

        private void OnChangeNameClicked()
        {
            GameLogger.Log("Change name button clicked");
            ShowUsernamePopup(isFirstTime: false);
        }

        private void ShowUsernamePopup(bool isFirstTime)
        {
            if (_uiService == null)
            {
                GameLogger.LogError("UIService is null");
                return;
            }

            if (_saveService == null)
            {
                GameLogger.LogError("SaveService is null");
                return;
            }

            GameLogger.Log("Getting UsernamePopup screen...");

            // Get the username popup screen
            var usernamePopup = _uiService.GetScreen<UsernamePopup>();
            if (usernamePopup != null)
            {
                GameLogger.Log("UsernamePopup found, showing...");

                usernamePopup.ShowForUsername(
                    isFirstTime,
                    onConfirm: (newUsername) =>
                    {
                        GameLogger.Log($"Username changed to: {newUsername}");
                        RefreshStats();

                        // Show success toast
                        _ = _uiService?.ShowNotification($"Username changed to {newUsername}", NotificationType.Success, 2.5f);

                        // Update main menu player name
                        var mainMenu = _uiService.GetScreen<MainMenuScreen>();
                        if (mainMenu != null && mainMenu.IsVisible)
                        {
                            // Main menu will update on next show
                        }

                        // If first time, go back to main menu
                        if (isFirstTime)
                        {
                            _uiService.ShowScreen(UIScreenType.MainMenu, true, false);
                        }
                    },
                    onCancel: () =>
                    {
                        GameLogger.Log("Username change cancelled");
                    }
                );
            }
            else
            {
                GameLogger.LogError("UsernamePopup screen not found in UIService - it may not be registered");
            }
        }

        private void OnCopyFriendCodeClicked()
        {
            var networking = GameBootstrap.Services?.NetworkingServices;
            var friendsService = networking?.FriendsService;

            if (friendsService == null || !friendsService.IsInitialized)
            {
                GameLogger.LogWarning("Friends service not available");
                return;
            }

            var code = friendsService.MyFriendCode;
            GUIUtility.systemCopyBuffer = code;

            var uiService = GameBootstrap.Services?.UIService;
            uiService?.ShowNotification($"Friend Code copied: {code}", NotificationType.Success, 2f);

            GameLogger.Log($"Copied friend code: {code}");
        }

        protected override void OnShow()
        {
            // Refresh stats when screen is shown
            RefreshStats();
        }

        protected override void OnDispose()
        {
            if (_backButton != null)
            {
                _backButton.clicked -= OnBackClicked;
            }

            if (_changeNameButton != null)
            {
                _changeNameButton.clicked -= OnChangeNameClicked;
            }

            if (_copyFriendCodeButton != null)
            {
                _copyFriendCodeButton.clicked -= OnCopyFriendCodeClicked;
            }
        }

        public override void AnimateShow(IUIAnimator animator, VisualElement element, float duration, Action onComplete)
        {
            // Slide in from right
            animator.SlideIn(element, SlideDirection.Right, duration, onComplete);
        }

        public override void AnimateHide(IUIAnimator animator, VisualElement element, float duration, Action onComplete)
        {
            // Slide out to right
            animator.SlideOut(element, SlideDirection.Right, duration, onComplete);
        }
    }
}
