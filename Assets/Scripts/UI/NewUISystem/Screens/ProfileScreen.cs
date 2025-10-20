using System;
using Core.Animation;
using Core.Bootstrap;
using Core.SaveSystem;
using UI.UISystem.Core;
using UI.UISystem.Popups;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.UISystem.Screens
{
    /// <summary>
    /// Profile screen - displays player stats and allows username changes
    /// </summary>
    [UIScreen(UIScreenType.Profile, UIScreenPriority.Menu, "ProfileTemplate")]
    public class ProfileScreen : BaseUIScreen
    {
        private ISaveService _saveService;
        private IUIService _uiService;

        // UI Elements
        private Button _backButton;
        private Button _changeNameButton;
        private Label _playerNameLabel;
        private Label _playerLevelLabel;
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

            Debug.Log("[ProfileScreen] Initialized");
        }

        private void QueryElements()
        {
            _backButton = GetElement<Button>("back-button");
            _changeNameButton = GetElement<Button>("change-name-button");
            _playerNameLabel = GetElement<Label>("player-name");
            _playerLevelLabel = GetElement<Label>("player-level");
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
            // Use UI navigation history to go back
            if (_uiService != null)
            {
                bool wentBack = _uiService.GoBack(true);

                // if (!wentBack)
                // {
                //     // If no history, go to main menu
                //     _uiService.ShowScreen(UIScreenType.Menu, true, false);
                // }

                _uiService.HideScreen(this.ScreenType, true);
            }
        }

        private void OnChangeNameClicked()
        {
            Debug.Log("[ProfileScreen] Change name button clicked");
            ShowUsernamePopup(isFirstTime: false);
        }

        private void ShowUsernamePopup(bool isFirstTime)
        {
            if (_uiService == null)
            {
                Debug.LogError("[ProfileScreen] UIService is null");
                return;
            }

            if (_saveService == null)
            {
                Debug.LogError("[ProfileScreen] SaveService is null");
                return;
            }

            Debug.Log("[ProfileScreen] Getting UsernamePopup screen...");

            // Get the username popup screen
            var usernamePopup = _uiService.GetScreen<UsernamePopup>();
            if (usernamePopup != null)
            {
                Debug.Log("[ProfileScreen] UsernamePopup found, showing...");

                usernamePopup.ShowForUsername(
                    isFirstTime,
                    onConfirm: (newUsername) =>
                    {
                        Debug.Log($"[ProfileScreen] Username changed to: {newUsername}");
                        RefreshStats();

                        // Show success toast
                        _ = _uiService?.ShowToast($"Username changed to {newUsername}", ToastType.Success, 2.5f);

                        // Update main menu player name
                        var mainMenu = _uiService.GetScreen<MainMenuScreen>();
                        if (mainMenu != null && mainMenu.IsVisible)
                        {
                            // Main menu will update on next show
                        }

                        // If first time, go back to main menu
                        if (isFirstTime)
                        {
                            _uiService.ShowScreen(UIScreenType.Menu, true, false);
                        }
                    },
                    onCancel: () =>
                    {
                        Debug.Log("[ProfileScreen] Username change cancelled");
                    }
                );
            }
            else
            {
                Debug.LogError("[ProfileScreen] UsernamePopup screen not found in UIService - it may not be registered");
            }
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
