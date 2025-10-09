using System;
using Core.GameFramework.State;
using Core.GameFramework.State.States;
using Core.UI.Animation;
using UI.UISystem.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.UISystem.Screens
{
    /// <summary>
    /// Main menu screen with player info and navigation
    /// Pure C# implementation with programmatic configuration
    /// </summary>
    [UIScreen(UIScreenType.Menu, UIScreenPriority.Menu, "MainMenuTemplate")]
    public class MainMenuScreen : BaseUIScreen
    {
        #region Configuration Properties

        public string PlayerName { get; set; } = "Chef";
        public int PlayerLevel { get; set; } = 1;
        public int TrophyCount { get; set; } = 0;
        public int GemCount { get; set; } = 0;
        public int CoinCount { get; set; } = 0;
        public float CharacterRotationSpeed { get; set; } = 2f;
        public float CharacterRotationAmount { get; set; } = 5f;
        public int SelectedCharacterIndex { get; set; } = 0;

        #endregion

        #region UI Elements

        private Button _playButton;
        private Button _characterSelectionButton;
        private Button _settingsButton;
        private Button _quitButton;
        private Label _gameTitle;
        private Label _playerNameLabel;
        private Label _playerLevelLabel;
        private Label _trophyCountLabel;
        private Label _gemCountLabel;
        private Label _coinCountLabel;

        // Character elements
        private VisualElement _character1;
        private VisualElement _character2;
        private VisualElement _character3;

        #endregion

        #region Animation State

        private bool _isAnimatingCharacters;
        private float _characterAnimationTime;

        #endregion

        #region Events

        public event Action OnPlayButtonClicked;
        public event Action OnCharacterSelectionClicked;
        public event Action OnSettingsClicked;
        public event Action OnQuitClicked;
        public event Action<int> OnCharacterClicked;

        #endregion

        #region Lifecycle

        protected override void OnInitialize()
        {
            CacheUIElements();
            SetupEventHandlers();
            UpdateUI();
            
            Debug.Log("[MainMenuScreen] Initialized with pure C# implementation");
        }

        protected override void OnShow()
        {
            StartCharacterAnimations();
            AnimateUIElementsEntrance();
            UpdateUI();
        }

        protected override void OnHide()
        {
            StopCharacterAnimations();
        }

        public override void Update(float deltaTime)
        {
            if (_isAnimatingCharacters)
            {
                UpdateCharacterAnimations(deltaTime);
            }
        }

        protected override void OnDispose()
        {
            UnregisterEventHandlers();
        }

        #endregion

        #region UI Setup

        private void CacheUIElements()
        {
            // Cache buttons
            _playButton = GetElement<Button>("play-button");
            _characterSelectionButton = GetElement<Button>("character-selection-button");
            _settingsButton = GetElement<Button>("settings-button");
            _quitButton = GetElement<Button>("quit-button");

            // Cache labels
            _gameTitle = GetElement<Label>("game-title");
            _playerNameLabel = GetElement<Label>("player-name");
            _playerLevelLabel = GetElement<Label>("player-level");
            _trophyCountLabel = GetElement<Label>("trophy-count");
            _gemCountLabel = GetElement<Label>("gem-count");
            _coinCountLabel = GetElement<Label>("coin-count");

            // Cache character elements
            _character1 = GetElement<VisualElement>("character-1");
            _character2 = GetElement<VisualElement>("character-2");
            _character3 = GetElement<VisualElement>("character-3");

            // Log missing elements for debugging
            if (_playButton == null)
            {
                Debug.LogWarning("[MainMenuScreen] play-button not found in template");
            }
            if (_gameTitle == null)
            {
                Debug.LogWarning("[MainMenuScreen] game-title not found in template");
            }
        }

        private void SetupEventHandlers()
        {
            _playButton?.RegisterCallback<ClickEvent>(_ => HandlePlayClicked());
            _characterSelectionButton?.RegisterCallback<ClickEvent>(_ => HandleCharacterSelectionClicked());
            _settingsButton?.RegisterCallback<ClickEvent>(_ => HandleSettingsClicked());
            _quitButton?.RegisterCallback<ClickEvent>(_ => HandleQuitClicked());

            // Character click events
            _character1?.RegisterCallback<ClickEvent>(_ => HandleCharacterClicked(0));
            _character2?.RegisterCallback<ClickEvent>(_ => HandleCharacterClicked(1));
            _character3?.RegisterCallback<ClickEvent>(_ => HandleCharacterClicked(2));
        }

        private void UnregisterEventHandlers()
        {
            _playButton?.UnregisterCallback<ClickEvent>(_ => HandlePlayClicked());
            _characterSelectionButton?.UnregisterCallback<ClickEvent>(_ => HandleCharacterSelectionClicked());
            _settingsButton?.UnregisterCallback<ClickEvent>(_ => HandleSettingsClicked());
            _quitButton?.UnregisterCallback<ClickEvent>(_ => HandleQuitClicked());

            _character1?.UnregisterCallback<ClickEvent>(_ => HandleCharacterClicked(0));
            _character2?.UnregisterCallback<ClickEvent>(_ => HandleCharacterClicked(1));
            _character3?.UnregisterCallback<ClickEvent>(_ => HandleCharacterClicked(2));
        }

        #endregion

        #region Public API

        /// <summary>
        /// Configure the main menu with player data
        /// </summary>
        public MainMenuScreen ConfigurePlayer(string name, int level, int trophies, int gems = 0, int coins = 0)
        {
            PlayerName = name;
            PlayerLevel = level;
            TrophyCount = trophies;
            GemCount = gems;
            CoinCount = coins;
            
            UpdateUI();
            return this;
        }

        /// <summary>
        /// Set the selected character
        /// </summary>
        public MainMenuScreen SetSelectedCharacter(int characterIndex)
        {
            SelectedCharacterIndex = Mathf.Clamp(characterIndex, 0, 2);
            HighlightSelectedCharacter();
            return this;
        }

        /// <summary>
        /// Configure character animation settings
        /// </summary>
        public MainMenuScreen ConfigureCharacterAnimation(float speed, float amount)
        {
            CharacterRotationSpeed = speed;
            CharacterRotationAmount = amount;
            return this;
        }

        #endregion

        #region UI Updates

        private void UpdateUI()
        {
            // Update game title
            if (_gameTitle != null)
            {
                _gameTitle.text = "Recipe Rage";
            }

            // Update player info
            if (_playerNameLabel != null)
            {
                _playerNameLabel.text = PlayerName;
            }
            if (_playerLevelLabel != null)
            {
                _playerLevelLabel.text = PlayerLevel.ToString();
            }
            if (_trophyCountLabel != null)
            {
                _trophyCountLabel.text = TrophyCount.ToString();
            }
            if (_gemCountLabel != null)
            {
                _gemCountLabel.text = GemCount.ToString();
            }
            if (_coinCountLabel != null)
            {
                _coinCountLabel.text = CoinCount.ToString();
            }

            HighlightSelectedCharacter();
        }

        private void HighlightSelectedCharacter()
        {
            // Remove highlight from all characters
            _character1?.RemoveFromClassList("character-selected");
            _character2?.RemoveFromClassList("character-selected");
            _character3?.RemoveFromClassList("character-selected");

            // Highlight selected character
            VisualElement selectedCharacter = SelectedCharacterIndex switch
            {
                0 => _character1,
                1 => _character2,
                2 => _character3,
                _ => null
            };

            selectedCharacter?.AddToClassList("character-selected");
        }

        #endregion

        #region Animations

        private void StartCharacterAnimations()
        {
            _isAnimatingCharacters = true;
            _characterAnimationTime = 0f;
        }

        private void StopCharacterAnimations()
        {
            _isAnimatingCharacters = false;
        }

        private void UpdateCharacterAnimations(float deltaTime)
        {
            _characterAnimationTime += deltaTime;
            float normalizedTime = _characterAnimationTime / CharacterRotationSpeed;
            float angle = Mathf.Sin(normalizedTime * Mathf.PI * 2) * CharacterRotationAmount;

            // Animate characters with slight variations
            if (_character1 != null)
            {
                _character1.style.rotate = new Rotate(angle);
            }
            if (_character2 != null)
            {
                _character2.style.rotate = new Rotate(-angle * 0.5f);
            }
            if (_character3 != null)
            {
                _character3.style.rotate = new Rotate(angle * 0.8f);
            }
        }

        private void AnimateUIElementsEntrance()
        {
            // Animate play button with bounce
            if (_playButton != null)
            {
                UnityNativeUIAnimationSystem.Animate(
                    _playButton,
                    UnityNativeUIAnimationSystem.AnimationType.Bounce,
                    0.8f,
                    0.5f
                );
            }

            // Animate characters with staggered scale-in
            if (_character1 != null)
            {
                UnityNativeUIAnimationSystem.Animate(
                    _character1,
                    UnityNativeUIAnimationSystem.AnimationType.ScaleIn,
                    0.5f,
                    0.2f
                );
            }

            if (_character2 != null)
            {
                UnityNativeUIAnimationSystem.Animate(
                    _character2,
                    UnityNativeUIAnimationSystem.AnimationType.ScaleIn,
                    0.5f,
                    0.4f
                );
            }

            if (_character3 != null)
            {
                UnityNativeUIAnimationSystem.Animate(
                    _character3,
                    UnityNativeUIAnimationSystem.AnimationType.ScaleIn,
                    0.5f,
                    0.6f
                );
            }
        }

        #endregion

        #region Event Handlers

        private void HandlePlayClicked()
        {
            Debug.Log("[MainMenuScreen] Play button clicked");

            // Animate button press
            if (_playButton != null)
            {
                UnityNativeUIAnimationSystem.Animate(
                    _playButton,
                    UnityNativeUIAnimationSystem.AnimationType.Pulse,
                    0.2f,
                    0f
                );
            }

            OnPlayButtonClicked?.Invoke();

            // Default behavior: transition to matchmaking
            GameStateManager.Instance?.ChangeState<MatchmakingState>();
        }

        private void HandleCharacterSelectionClicked()
        {
            Debug.Log("[MainMenuScreen] Character selection clicked");
            OnCharacterSelectionClicked?.Invoke();

            // Default behavior: show character selection screen
            UIManager.Instance.ShowScreen(UIScreenType.CharacterSelection, true, true);
        }

        private void HandleSettingsClicked()
        {
            Debug.Log("[MainMenuScreen] Settings clicked");
            OnSettingsClicked?.Invoke();

            // Default behavior: show settings screen
            UIManager.Instance.ShowScreen(UIScreenType.Settings, true, true);
        }

        private void HandleQuitClicked()
        {
            Debug.Log("[MainMenuScreen] Quit clicked");
            OnQuitClicked?.Invoke();

            // Default behavior: show quit confirmation
            ShowQuitConfirmation();
        }

        private void HandleCharacterClicked(int characterIndex)
        {
            Debug.Log($"[MainMenuScreen] Character {characterIndex} clicked");

            SetSelectedCharacter(characterIndex);
            OnCharacterClicked?.Invoke(characterIndex);

            // Animate the clicked character
            VisualElement character = characterIndex switch
            {
                0 => _character1,
                1 => _character2,
                2 => _character3,
                _ => null
            };

            if (character != null)
            {
                UnityNativeUIAnimationSystem.Animate(
                    character,
                    UnityNativeUIAnimationSystem.AnimationType.Bounce,
                    0.5f,
                    0f
                );
            }
        }

        private void ShowQuitConfirmation()
        {
            // Show quit confirmation popup
            PopupScreen popupScreen = UIManager.Instance.GetScreen<PopupScreen>();
            if (popupScreen != null)
            {
                popupScreen
                    .SetTitle("Quit Game")
                    .SetMessage("Are you sure you want to quit Recipe Rage?")
                    .SetConfirmAction(() => Application.Quit())
                    .Show();
            }
        }

        #endregion
    }
}