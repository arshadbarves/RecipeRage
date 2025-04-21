using Core.GameFramework.State;
using Core.GameFramework.State.States;
using Core.GameModes;
using UI.Animation;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Screens
{
    /// <summary>
    /// Game mode selection screen
    /// </summary>
    public class GameModeSelectionScreen : UIScreen
    {
        // Header Elements
        private Button _backButton;

        // Game Mode Card Elements
        private VisualElement _classicModeCard;
        private VisualElement _timeAttackModeCard;
        private VisualElement _teamBattleModeCard;
        private VisualElement _challengeModeCard;

        // Play Buttons
        private Button _playClassicButton;
        private Button _playTimeAttackButton;
        private Button _playTeamBattleButton;
        private Button _playChallengeButton;

        // Selected game mode
        private GameModeType _selectedGameMode = GameModeType.Classic;

        /// <summary>
        /// Game mode types
        /// </summary>
        public enum GameModeType
        {
            Classic,
            TimeAttack,
            TeamBattle,
            Challenge
        }

        /// <summary>
        /// Initialize the game mode selection screen
        /// </summary>
        protected override void InitializeScreen()
        {
            // Get references to UI elements
            GetUIReferences();

            // Set up button listeners
            SetupButtonListeners();

            // Add hover effects to cards
            AddCardHoverEffects();
        }

        /// <summary>
        /// Get references to UI elements
        /// </summary>
        private void GetUIReferences()
        {
            // Header Elements
            _backButton = _root.Q<Button>("back-button");

            // Game Mode Card Elements
            _classicModeCard = _root.Q<VisualElement>("game-mode-card-classic");
            _timeAttackModeCard = _root.Q<VisualElement>("game-mode-card-time-attack");
            _teamBattleModeCard = _root.Q<VisualElement>("game-mode-card-team-battle");
            _challengeModeCard = _root.Q<VisualElement>("game-mode-card-challenge");

            // Play Buttons
            _playClassicButton = _root.Q<Button>("play-classic-button");
            _playTimeAttackButton = _root.Q<Button>("play-time-attack-button");
            _playTeamBattleButton = _root.Q<Button>("play-team-battle-button");
            _playChallengeButton = _root.Q<Button>("play-challenge-button");
        }

        /// <summary>
        /// Set up button listeners
        /// </summary>
        private void SetupButtonListeners()
        {
            // Back button
            if (_backButton != null)
            {
                _backButton.clicked += OnBackButtonClicked;
            }

            // Play buttons
            if (_playClassicButton != null)
            {
                _playClassicButton.clicked += () => OnPlayButtonClicked(GameModeType.Classic);
            }

            if (_playTimeAttackButton != null)
            {
                _playTimeAttackButton.clicked += () => OnPlayButtonClicked(GameModeType.TimeAttack);
            }

            if (_playTeamBattleButton != null)
            {
                _playTeamBattleButton.clicked += () => OnPlayButtonClicked(GameModeType.TeamBattle);
            }

            if (_playChallengeButton != null)
            {
                _playChallengeButton.clicked += () => OnPlayButtonClicked(GameModeType.Challenge);
            }

            // Card clicks
            if (_classicModeCard != null)
            {
                _classicModeCard.RegisterCallback<ClickEvent>(evt => SelectGameMode(GameModeType.Classic));
            }

            if (_timeAttackModeCard != null)
            {
                _timeAttackModeCard.RegisterCallback<ClickEvent>(evt => SelectGameMode(GameModeType.TimeAttack));
            }

            if (_teamBattleModeCard != null)
            {
                _teamBattleModeCard.RegisterCallback<ClickEvent>(evt => SelectGameMode(GameModeType.TeamBattle));
            }

            if (_challengeModeCard != null)
            {
                _challengeModeCard.RegisterCallback<ClickEvent>(evt => SelectGameMode(GameModeType.Challenge));
            }
        }

        /// <summary>
        /// Add hover effects to game mode cards
        /// </summary>
        private void AddCardHoverEffects()
        {
            // Add hover effects to cards
            _classicModeCard.AddHoverEffect(1.02f, 0.2f);
            _timeAttackModeCard.AddHoverEffect(1.02f, 0.2f);
            _teamBattleModeCard.AddHoverEffect(1.02f, 0.2f);
            _challengeModeCard.AddHoverEffect(1.02f, 0.2f);

            // Add hover effects to play buttons
            _playClassicButton.AddHoverEffect(1.1f, 0.2f);
            _playTimeAttackButton.AddHoverEffect(1.1f, 0.2f);
            _playTeamBattleButton.AddHoverEffect(1.1f, 0.2f);
            _playChallengeButton.AddHoverEffect(1.1f, 0.2f);
        }

        /// <summary>
        /// Set the border for a card
        /// </summary>
        /// <param name="card">The card to set the border for</param>
        /// <param name="isSelected">Whether the card is selected</param>
        private void SetCardBorder(VisualElement card, bool isSelected)
        {
            if (card == null) return;

            if (isSelected)
            {
                card.style.borderTopWidth = 3;
                card.style.borderRightWidth = 3;
                card.style.borderBottomWidth = 3;
                card.style.borderLeftWidth = 3;
                card.style.borderTopColor = Color.yellow;
                card.style.borderRightColor = Color.yellow;
                card.style.borderBottomColor = Color.yellow;
                card.style.borderLeftColor = Color.yellow;
            }
            else
            {
                card.style.borderTopWidth = 0;
                card.style.borderRightWidth = 0;
                card.style.borderBottomWidth = 0;
                card.style.borderLeftWidth = 0;
            }
        }

        /// <summary>
        /// Select a game mode
        /// </summary>
        /// <param name="gameMode">Game mode type</param>
        private void SelectGameMode(GameModeType gameMode)
        {
            _selectedGameMode = gameMode;

            // Highlight selected card
            SetCardBorder(_classicModeCard, gameMode == GameModeType.Classic);
            SetCardBorder(_timeAttackModeCard, gameMode == GameModeType.TimeAttack);
            SetCardBorder(_teamBattleModeCard, gameMode == GameModeType.TeamBattle);
            SetCardBorder(_challengeModeCard, gameMode == GameModeType.Challenge);

            // Animate selected card
            VisualElement selectedCard = null;
            switch (gameMode)
            {
                case GameModeType.Classic:
                    selectedCard = _classicModeCard;
                    break;
                case GameModeType.TimeAttack:
                    selectedCard = _timeAttackModeCard;
                    break;
                case GameModeType.TeamBattle:
                    selectedCard = _teamBattleModeCard;
                    break;
                case GameModeType.Challenge:
                    selectedCard = _challengeModeCard;
                    break;
            }

            if (selectedCard != null)
            {
                UIAnimationSystem.Instance.Animate(
                    selectedCard,
                    UIAnimationSystem.AnimationType.Pulse,
                    0.5f,
                    0f,
                    UIAnimationSystem.EasingType.EaseOutElastic
                );
            }
        }

        /// <summary>
        /// Show the game mode selection screen with animations
        /// </summary>
        /// <param name="animate">Whether to animate the transition</param>
        public override void Show(bool animate = true)
        {
            base.Show(animate);

            if (animate && _container != null)
            {
                // Animate UI elements
                AnimateUIElements();
            }
        }

        /// <summary>
        /// Animate UI elements when showing the screen
        /// </summary>
        private void AnimateUIElements()
        {
            // Reset elements
            var header = _root.Q<VisualElement>("header");
            var gameModeCards = new VisualElement[]
            {
                _classicModeCard,
                _timeAttackModeCard,
                _teamBattleModeCard,
                _challengeModeCard
            };
            var bottomInfo = _root.Q<VisualElement>("bottom-info");

            header.style.opacity = 0;
            header.transform.position = new Vector2(0, -50);

            foreach (var card in gameModeCards)
            {
                card.style.opacity = 0;
                card.transform.position = new Vector2(-100, 0);
            }

            bottomInfo.style.opacity = 0;
            bottomInfo.transform.position = new Vector2(0, 50);

            // Animate header
            UIAnimationSystem.Instance.Animate(
                header,
                UIAnimationSystem.AnimationType.FadeIn,
                0.5f,
                0.2f,
                UIAnimationSystem.EasingType.EaseOutCubic
            );

            // Animate game mode cards
            for (int i = 0; i < gameModeCards.Length; i++)
            {
                float delay = 0.3f + (i * 0.1f);
                UIAnimationSystem.Instance.Animate(
                    gameModeCards[i],
                    UIAnimationSystem.AnimationType.FadeIn,
                    0.5f,
                    delay,
                    UIAnimationSystem.EasingType.EaseOutCubic
                );

                UIAnimationSystem.Instance.Animate(
                    gameModeCards[i],
                    UIAnimationSystem.AnimationType.SlideInFromLeft,
                    0.5f,
                    delay,
                    UIAnimationSystem.EasingType.EaseOutCubic
                );
            }

            // Animate bottom info
            UIAnimationSystem.Instance.Animate(
                bottomInfo,
                UIAnimationSystem.AnimationType.FadeIn,
                0.5f,
                0.7f,
                UIAnimationSystem.EasingType.EaseOutCubic
            );
        }

        #region Button Handlers

        /// <summary>
        /// Handle back button click
        /// </summary>
        private void OnBackButtonClicked()
        {
            Debug.Log("[GameModeSelectionScreen] Back button clicked");

            // Hide this screen
            Hide(true);

            // Show main menu screen
            UIManager.Instance.ShowScreen<MainMenuScreen>(true);
        }

        /// <summary>
        /// Handle play button click
        /// </summary>
        /// <param name="gameMode">Selected game mode</param>
        private void OnPlayButtonClicked(GameModeType gameMode)
        {
            Debug.Log($"[GameModeSelectionScreen] Play button clicked for {gameMode} mode");

            // Set the selected game mode
            _selectedGameMode = gameMode;

            // Set the game mode in the game mode manager
            GameModeManager gameModeManager = GameModeManager.Instance;
            if (gameModeManager != null)
            {
                GameMode selectedMode = null;

                switch (gameMode)
                {
                    case GameModeType.Classic:
                        selectedMode = gameModeManager.GetGameModeByName("Classic");
                        break;
                    case GameModeType.TimeAttack:
                        selectedMode = gameModeManager.GetGameModeByName("TimeAttack");
                        break;
                    case GameModeType.TeamBattle:
                        selectedMode = gameModeManager.GetGameModeByName("TeamBattle");
                        break;
                    case GameModeType.Challenge:
                        selectedMode = gameModeManager.GetGameModeByName("Challenge");
                        break;
                }

                if (selectedMode != null)
                {
                    gameModeManager.SetSelectedGameMode(selectedMode);
                }
                else
                {
                    Debug.LogWarning($"[GameModeSelectionScreen] Game mode {gameMode} not found");
                }
            }

            // Hide this screen
            Hide(true);

            // Transition to matchmaking state
            GameStateManager.Instance.ChangeState<MatchmakingState>();
        }

        #endregion
    }
}
