using System.Collections;
using Core.GameFramework.State;
using Core.GameFramework.State.States;
using UI.Animation;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Screens
{
    /// <summary>
    /// Main menu screen that serves as the lobby
    /// </summary>
    public class MainMenuScreen : UIScreen
    {
        // Top Bar Elements
        private VisualElement _playerAvatar;
        private Label _playerLevel;
        private Label _trophyCount;
        private Label _gemCount;
        private Label _coinCount;
        private Label _energyCount;
        private Button _settingsButton;

        // Middle Content Elements
        private Label _rankNumber;
        private Label _trophyRoadCount;
        private VisualElement _character1;
        private VisualElement _character2;
        private VisualElement _character3;
        private Label _playerName;
        private Label _teamCode;
        private Label _eventTitle;
        private Label _eventSubtitle;
        private Label _timerText;

        // Bottom Navigation Elements
        private Button _shopButton;
        private Button _brawlersButton;
        private Button _newsButton;
        private Button _playButton;
        private Button _friendsButton;
        private Button _clubButton;
        private Button _chatButton;

        // Pass Progress Elements
        private VisualElement _passProgressFill;
        private Label _passProgressText;
        private Label _passLevelText;

        // Character rotation coroutine
        private Coroutine _characterRotationCoroutine;

        /// <summary>
        /// Initialize the main menu screen
        /// </summary>
        protected override void InitializeScreen()
        {
            // Get references to UI elements
            GetUIReferences();

            // Set up button listeners
            SetupButtonListeners();

            // Set initial values
            SetInitialValues();
        }

        /// <summary>
        /// Get references to UI elements
        /// </summary>
        private void GetUIReferences()
        {
            // Top Bar Elements
            _playerAvatar = _root.Q<VisualElement>("player-avatar");
            _playerLevel = _root.Q<Label>("player-level");
            _trophyCount = _root.Q<Label>("trophy-count");
            _gemCount = _root.Q<Label>("gem-count");
            _coinCount = _root.Q<Label>("coin-count");
            _energyCount = _root.Q<Label>("energy-count");
            _settingsButton = _root.Q<Button>("settings-button");

            // Middle Content Elements
            _rankNumber = _root.Q<Label>("rank-number");
            _trophyRoadCount = _root.Q<Label>("trophy-road-count");
            _character1 = _root.Q<VisualElement>("character-1");
            _character2 = _root.Q<VisualElement>("character-2");
            _character3 = _root.Q<VisualElement>("character-3");
            _playerName = _root.Q<Label>("player-name");
            _teamCode = _root.Q<Label>("team-code");
            _eventTitle = _root.Q<Label>("event-title");
            _eventSubtitle = _root.Q<Label>("event-subtitle");
            _timerText = _root.Q<Label>("timer-text");

            // Bottom Navigation Elements
            _shopButton = _root.Q<Button>("shop-button");
            _brawlersButton = _root.Q<Button>("brawlers-button");
            _newsButton = _root.Q<Button>("news-button");
            _playButton = _root.Q<Button>("play-button");
            _friendsButton = _root.Q<Button>("friends-button");
            _clubButton = _root.Q<Button>("club-button");
            _chatButton = _root.Q<Button>("chat-button");

            // Pass Progress Elements
            _passProgressFill = _root.Q<VisualElement>("pass-progress-fill");
            _passProgressText = _root.Q<Label>("pass-progress-text");
            _passLevelText = _root.Q<Label>("pass-level-text");
        }

        /// <summary>
        /// Set up button listeners
        /// </summary>
        private void SetupButtonListeners()
        {
            // Settings button
            if (_settingsButton != null)
            {
                _settingsButton.clicked += OnSettingsButtonClicked;
            }

            // Shop button
            if (_shopButton != null)
            {
                _shopButton.clicked += OnShopButtonClicked;
            }

            // Brawlers button
            if (_brawlersButton != null)
            {
                _brawlersButton.clicked += OnBrawlersButtonClicked;
            }

            // News button
            if (_newsButton != null)
            {
                _newsButton.clicked += OnNewsButtonClicked;
            }

            // Play button
            if (_playButton != null)
            {
                _playButton.clicked += OnPlayButtonClicked;
            }

            // Friends button
            if (_friendsButton != null)
            {
                _friendsButton.clicked += OnFriendsButtonClicked;
            }

            // Club button
            if (_clubButton != null)
            {
                _clubButton.clicked += OnClubButtonClicked;
            }

            // Chat button
            if (_chatButton != null)
            {
                _chatButton.clicked += OnChatButtonClicked;
            }

            // Character slots
            if (_character1 != null)
            {
                _character1.RegisterCallback<ClickEvent>(evt => OnCharacterClicked(0));
            }

            if (_character2 != null)
            {
                _character2.RegisterCallback<ClickEvent>(evt => OnCharacterClicked(1));
            }

            if (_character3 != null)
            {
                _character3.RegisterCallback<ClickEvent>(evt => OnCharacterClicked(2));
            }
        }

        /// <summary>
        /// Set initial values for UI elements
        /// </summary>
        private void SetInitialValues()
        {
            // Player info
            _playerLevel.text = "477 elite";
            _playerName.text = "Chef" + Random.Range(1000, 9999);

            // Generate random team code
            string teamCode = "";
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            for (int i = 0; i < 8; i++)
            {
                teamCode += chars[Random.Range(0, chars.Length)];
            }
            _teamCode.text = teamCode;

            // Currencies
            _trophyCount.text = Random.Range(10000, 50000).ToString();
            _gemCount.text = Random.Range(5000, 10000).ToString();
            _coinCount.text = Random.Range(10000, 20000).ToString();
            _energyCount.text = Random.Range(10, 30).ToString();

            // Rank
            _rankNumber.text = Random.Range(20, 50).ToString();
            _trophyRoadCount.text = Random.Range(1000, 2000).ToString();

            // Event
            _eventTitle.text = "COOK OFF";
            _eventSubtitle.text = "Gourmet Challenge";
            _timerText.text = "5h 23m";

            // Pass progress
            float progressPercent = Random.Range(0.7f, 0.9f);
            int maxProgress = 600;
            int currentProgress = Mathf.RoundToInt(progressPercent * maxProgress);
            _passProgressFill.style.width = new StyleLength(new Length(progressPercent * 100, LengthUnit.Percent));
            _passProgressText.text = $"{currentProgress}/{maxProgress}";
            _passLevelText.text = Random.Range(30, 50).ToString();

            // Add hover effects to buttons
            AddButtonHoverEffects();
        }

        /// <summary>
        /// Add hover effects to buttons
        /// </summary>
        private void AddButtonHoverEffects()
        {
            // Add hover effects to all buttons
            var buttons = _root.Query<Button>().ToList();
            foreach (var button in buttons)
            {
                button.AddHoverEffect(1.05f, 0.1f);
                button.AddPressEffect(0.95f, 0.1f);
            }

            // Add hover effects to characters
            _character1.AddHoverEffect(1.1f, 0.2f);
            _character2.AddHoverEffect(1.1f, 0.2f);
            _character3.AddHoverEffect(1.1f, 0.2f);
        }

        /// <summary>
        /// Show the main menu screen with animations
        /// </summary>
        /// <param name="animate">Whether to animate the transition</param>
        public override void Show(bool animate = true)
        {
            base.Show(animate);

            if (animate && _container != null)
            {
                // Animate UI elements
                AnimateUIElements();

                // Start character rotation
                if (_characterRotationCoroutine != null)
                {
                    StopCoroutine(_characterRotationCoroutine);
                }
                _characterRotationCoroutine = StartCoroutine(RotateCharactersCoroutine());
            }
        }

        /// <summary>
        /// Hide the main menu screen
        /// </summary>
        /// <param name="animate">Whether to animate the transition</param>
        public override void Hide(bool animate = true)
        {
            // Stop character rotation
            if (_characterRotationCoroutine != null)
            {
                StopCoroutine(_characterRotationCoroutine);
                _characterRotationCoroutine = null;
            }

            base.Hide(animate);
        }

        /// <summary>
        /// Animate UI elements when showing the screen
        /// </summary>
        private void AnimateUIElements()
        {
            // Reset elements
            var topBar = _root.Q<VisualElement>("top-bar");
            var middleContent = _root.Q<VisualElement>("middle-content");
            var bottomNavigation = _root.Q<VisualElement>("bottom-navigation");
            var passProgress = _root.Q<VisualElement>("pass-progress");

            topBar.style.opacity = 0;
            topBar.transform.position = new Vector2(0, -50);

            middleContent.style.opacity = 0;

            bottomNavigation.style.opacity = 0;
            bottomNavigation.transform.position = new Vector2(0, 50);

            passProgress.style.opacity = 0;

            // Animate top bar
            UIAnimationSystem.Instance.Animate(
                topBar,
                UIAnimationSystem.AnimationType.FadeIn,
                0.5f,
                0.2f,
                UIAnimationSystem.EasingType.EaseOutCubic
            );

            // Animate middle content
            UIAnimationSystem.Instance.Animate(
                middleContent,
                UIAnimationSystem.AnimationType.FadeIn,
                0.5f,
                0.4f,
                UIAnimationSystem.EasingType.EaseOutCubic
            );

            // Animate bottom navigation
            UIAnimationSystem.Instance.Animate(
                bottomNavigation,
                UIAnimationSystem.AnimationType.FadeIn,
                0.5f,
                0.6f,
                UIAnimationSystem.EasingType.EaseOutCubic
            );

            // Animate pass progress
            UIAnimationSystem.Instance.Animate(
                passProgress,
                UIAnimationSystem.AnimationType.FadeIn,
                0.5f,
                0.8f,
                UIAnimationSystem.EasingType.EaseOutCubic
            );

            // Animate characters
            UIAnimationSystem.Instance.Animate(
                _character1,
                UIAnimationSystem.AnimationType.ScaleIn,
                0.5f,
                0.5f,
                UIAnimationSystem.EasingType.EaseOutBack
            );

            UIAnimationSystem.Instance.Animate(
                _character2,
                UIAnimationSystem.AnimationType.ScaleIn,
                0.5f,
                0.6f,
                UIAnimationSystem.EasingType.EaseOutBack
            );

            UIAnimationSystem.Instance.Animate(
                _character3,
                UIAnimationSystem.AnimationType.ScaleIn,
                0.5f,
                0.7f,
                UIAnimationSystem.EasingType.EaseOutBack
            );

            // Animate play button with bounce
            UIAnimationSystem.Instance.Animate(
                _playButton,
                UIAnimationSystem.AnimationType.Bounce,
                1.0f,
                1.0f,
                UIAnimationSystem.EasingType.EaseOutElastic
            );
        }

        /// <summary>
        /// Coroutine to rotate characters slightly for idle animation
        /// </summary>
        private IEnumerator RotateCharactersCoroutine()
        {
            while (true)
            {
                // Slightly rotate characters back and forth
                float time = 0f;
                float duration = 2f;
                float maxRotation = 5f;

                while (time < duration)
                {
                    time += Time.deltaTime;
                    float normalizedTime = time / duration;
                    float angle = Mathf.Sin(normalizedTime * Mathf.PI * 2) * maxRotation;

                    _character1.style.rotate = new Rotate(angle);
                    _character2.style.rotate = new Rotate(-angle * 0.5f);
                    _character3.style.rotate = new Rotate(angle);

                    yield return null;
                }
            }
        }

        #region Button Handlers

        /// <summary>
        /// Handle settings button click
        /// </summary>
        private void OnSettingsButtonClicked()
        {
            Debug.Log("[MainMenuScreen] Settings button clicked");
            // TODO: Show settings screen
        }

        /// <summary>
        /// Handle shop button click
        /// </summary>
        private void OnShopButtonClicked()
        {
            Debug.Log("[MainMenuScreen] Shop button clicked");
            // TODO: Show shop screen
        }

        /// <summary>
        /// Handle brawlers button click
        /// </summary>
        private void OnBrawlersButtonClicked()
        {
            Debug.Log("[MainMenuScreen] Brawlers button clicked");
            // TODO: Show character selection screen
        }

        /// <summary>
        /// Handle news button click
        /// </summary>
        private void OnNewsButtonClicked()
        {
            Debug.Log("[MainMenuScreen] News button clicked");
            // TODO: Show news screen
        }

        /// <summary>
        /// Handle play button click
        /// </summary>
        private void OnPlayButtonClicked()
        {
            Debug.Log("[MainMenuScreen] Play button clicked");

            // Transition to matchmaking state
            GameStateManager.Instance.ChangeState<MatchmakingState>();
        }

        /// <summary>
        /// Handle friends button click
        /// </summary>
        private void OnFriendsButtonClicked()
        {
            Debug.Log("[MainMenuScreen] Friends button clicked");
            // TODO: Show friends screen
        }

        /// <summary>
        /// Handle club button click
        /// </summary>
        private void OnClubButtonClicked()
        {
            Debug.Log("[MainMenuScreen] Club button clicked");
            // TODO: Show club screen
        }

        /// <summary>
        /// Handle chat button click
        /// </summary>
        private void OnChatButtonClicked()
        {
            Debug.Log("[MainMenuScreen] Chat button clicked");
            // TODO: Show chat screen
        }

        /// <summary>
        /// Handle character click
        /// </summary>
        /// <param name="index">Character index (0-2)</param>
        private void OnCharacterClicked(int index)
        {
            Debug.Log($"[MainMenuScreen] Character {index} clicked");

            // Animate the clicked character
            VisualElement character = index == 0 ? _character1 : (index == 1 ? _character2 : _character3);

            UIAnimationSystem.Instance.Animate(
                character,
                UIAnimationSystem.AnimationType.Bounce,
                0.5f,
                0f,
                UIAnimationSystem.EasingType.EaseOutElastic
            );

            // TODO: Show character details or selection screen
        }

        #endregion
    }
}
