using System.Collections.Generic;
using Core.UI.Animation;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Screens
{
    /// <summary>
    /// Character selection screen
    /// </summary>
    public class CharacterSelectionScreen : UIScreen
    {
        /// <summary>
        /// Character data for display
        /// </summary>
        private class CharacterData
        {
            public string Name;
            public string Class;
            public int Level;
            public float SpeedStat;
            public float CookingStat;
            public float ChoppingStat;
            public float SpecialStat;
            public string AbilityName;
            public string AbilityDescription;
            public bool IsUnlocked;
            public int UnlockCost;
            public int UpgradeCost;
            public Sprite Portrait;
            public Sprite FullModel;
            public CharacterRarity Rarity;

            public enum CharacterRarity
            {
                Common,
                Rare,
                Epic,
                Legendary
            }
        }

        // Header Elements
        private Button _backButton;

        // Character Display Elements
        private VisualElement _characterModel;
        private Label _characterName;
        private Label _characterClass;
        private Label _characterLevel;
        private VisualElement _speedFill;
        private VisualElement _cookingFill;
        private VisualElement _choppingFill;
        private VisualElement _specialFill;
        private Label _abilityName;
        private Label _abilityDescription;

        // Character Selection Elements
        private VisualElement _characterGrid;
        private VisualTreeAsset _characterCardTemplate;

        // Bottom Button Elements
        private Button _upgradeButton;
        private Button _selectButton;
        private Label _upgradeCostText;

        // Character Data
        private List<CharacterData> _characters = new List<CharacterData>();
        private int _selectedCharacterIndex = 0;

        /// <summary>
        /// Initialize the character selection screen
        /// </summary>
        protected override void InitializeScreen()
        {
            // Get references to UI elements
            GetUIReferences();

            // Set up button listeners
            SetupButtonListeners();

            // Load character card template
            _characterCardTemplate = Resources.Load<VisualTreeAsset>("UI/CharacterCard");
            if (_characterCardTemplate == null)
            {
                Debug.LogError("[CharacterSelectionScreen] Failed to load CharacterCard template");
            }

            // Create sample character data
            CreateSampleCharacterData();

            // Create character cards
            CreateCharacterCards();

            // Select first character
            SelectCharacter(0);
        }

        /// <summary>
        /// Get references to UI elements
        /// </summary>
        private void GetUIReferences()
        {
            // Header Elements
            _backButton = _root.Q<Button>("back-button");

            // Character Display Elements
            _characterModel = _root.Q<VisualElement>("character-model");
            _characterName = _root.Q<Label>("character-name");
            _characterClass = _root.Q<Label>("character-class");
            _characterLevel = _root.Q<Label>("character-level");
            _speedFill = _root.Q<VisualElement>("stat-speed-fill");
            _cookingFill = _root.Q<VisualElement>("stat-cooking-fill");
            _choppingFill = _root.Q<VisualElement>("stat-chopping-fill");
            _specialFill = _root.Q<VisualElement>("stat-special-fill");
            _abilityName = _root.Q<Label>("ability-name");
            _abilityDescription = _root.Q<Label>("ability-description");

            // Character Selection Elements
            _characterGrid = _root.Q<VisualElement>("character-grid");

            // Bottom Button Elements
            _upgradeButton = _root.Q<Button>("upgrade-button");
            _selectButton = _root.Q<Button>("select-button");
            _upgradeCostText = _root.Q<Label>("upgrade-cost-text");
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

            // Upgrade button
            if (_upgradeButton != null)
            {
                _upgradeButton.clicked += OnUpgradeButtonClicked;
            }

            // Select button
            if (_selectButton != null)
            {
                _selectButton.clicked += OnSelectButtonClicked;
            }
        }

        /// <summary>
        /// Create sample character data
        /// </summary>
        private void CreateSampleCharacterData()
        {
            _characters.Add(new CharacterData
            {
                Name = "CHEF GORDON",
                Class = "MASTER CHEF",
                Level = 10,
                SpeedStat = 0.8f,
                CookingStat = 0.9f,
                ChoppingStat = 0.7f,
                SpecialStat = 0.85f,
                AbilityName = "FLAME BURST",
                AbilityDescription = "Cook food 50% faster for 10 seconds. Cooldown: 30s",
                IsUnlocked = true,
                UpgradeCost = 1000,
                Rarity = CharacterData.CharacterRarity.Epic
            });

            _characters.Add(new CharacterData
            {
                Name = "CHEF JULIA",
                Class = "PASTRY EXPERT",
                Level = 8,
                SpeedStat = 0.7f,
                CookingStat = 0.85f,
                ChoppingStat = 0.6f,
                SpecialStat = 0.9f,
                AbilityName = "PERFECT TIMING",
                AbilityDescription = "Automatically prevents food from burning for 15 seconds. Cooldown: 45s",
                IsUnlocked = true,
                UpgradeCost = 800,
                Rarity = CharacterData.CharacterRarity.Rare
            });

            _characters.Add(new CharacterData
            {
                Name = "CHEF MARCO",
                Class = "SPEED DEMON",
                Level = 12,
                SpeedStat = 0.95f,
                CookingStat = 0.7f,
                ChoppingStat = 0.8f,
                SpecialStat = 0.75f,
                AbilityName = "LIGHTNING DASH",
                AbilityDescription = "Move 100% faster for 8 seconds. Cooldown: 25s",
                IsUnlocked = true,
                UpgradeCost = 1200,
                Rarity = CharacterData.CharacterRarity.Epic
            });

            _characters.Add(new CharacterData
            {
                Name = "CHEF WOLFGANG",
                Class = "KNIFE MASTER",
                Level = 15,
                SpeedStat = 0.75f,
                CookingStat = 0.8f,
                ChoppingStat = 0.95f,
                SpecialStat = 0.8f,
                AbilityName = "RAPID SLICE",
                AbilityDescription = "Chop ingredients instantly for 5 seconds. Cooldown: 35s",
                IsUnlocked = true,
                UpgradeCost = 1500,
                Rarity = CharacterData.CharacterRarity.Legendary
            });

            _characters.Add(new CharacterData
            {
                Name = "CHEF ISABELLA",
                Class = "SAUCE SPECIALIST",
                Level = 6,
                SpeedStat = 0.65f,
                CookingStat = 0.85f,
                ChoppingStat = 0.7f,
                SpecialStat = 0.8f,
                AbilityName = "FLAVOR BOOST",
                AbilityDescription = "Dishes prepared give 50% more points for 12 seconds. Cooldown: 40s",
                IsUnlocked = true,
                UpgradeCost = 600,
                Rarity = CharacterData.CharacterRarity.Rare
            });

            _characters.Add(new CharacterData
            {
                Name = "CHEF HIRO",
                Class = "SUSHI MASTER",
                Level = 1,
                SpeedStat = 0.7f,
                CookingStat = 0.75f,
                ChoppingStat = 0.9f,
                SpecialStat = 0.7f,
                AbilityName = "PRECISION CUT",
                AbilityDescription = "All chopped ingredients are perfect quality for 10 seconds. Cooldown: 30s",
                IsUnlocked = false,
                UnlockCost = 80,
                UpgradeCost = 200,
                Rarity = CharacterData.CharacterRarity.Epic
            });

            _characters.Add(new CharacterData
            {
                Name = "CHEF PIERRE",
                Class = "FRENCH CUISINE",
                Level = 1,
                SpeedStat = 0.6f,
                CookingStat = 0.95f,
                ChoppingStat = 0.65f,
                SpecialStat = 0.85f,
                AbilityName = "GOURMET TOUCH",
                AbilityDescription = "All dishes are one tier higher quality for 15 seconds. Cooldown: 50s",
                IsUnlocked = false,
                UnlockCost = 120,
                UpgradeCost = 200,
                Rarity = CharacterData.CharacterRarity.Legendary
            });
        }

        /// <summary>
        /// Create character cards
        /// </summary>
        private void CreateCharacterCards()
        {
            if (_characterGrid == null || _characterCardTemplate == null) return;

            // Clear existing cards
            _characterGrid.Clear();

            // Create cards for each character
            for (int i = 0; i < _characters.Count; i++)
            {
                CharacterData character = _characters[i];

                // Instantiate card template
                TemplateContainer cardInstance = _characterCardTemplate.Instantiate();
                VisualElement card = cardInstance.contentContainer.Q<VisualElement>("character-card");

                // Set card data
                Label nameLabel = card.Q<Label>("character-card-name");
                Label levelLabel = card.Q<Label>("character-card-level");
                VisualElement lockedOverlay = card.Q<VisualElement>("locked-overlay");
                Label unlockCostText = card.Q<Label>("unlock-cost-text");

                nameLabel.text = character.Name;
                levelLabel.text = character.Level.ToString();

                // Set locked state
                if (!character.IsUnlocked)
                {
                    lockedOverlay.RemoveFromClassList("hidden");
                    unlockCostText.text = character.UnlockCost.ToString();
                }

                // Set rarity color
                VisualElement cardBackground = card.Q<VisualElement>("card-background");
                switch (character.Rarity)
                {
                    case CharacterData.CharacterRarity.Common:
                        cardBackground.style.backgroundColor = new Color(0.5f, 0.5f, 0.5f);
                        break;
                    case CharacterData.CharacterRarity.Rare:
                        cardBackground.style.backgroundColor = new Color(0.0f, 0.5f, 1.0f);
                        break;
                    case CharacterData.CharacterRarity.Epic:
                        cardBackground.style.backgroundColor = new Color(0.5f, 0.0f, 1.0f);
                        break;
                    case CharacterData.CharacterRarity.Legendary:
                        cardBackground.style.backgroundColor = new Color(1.0f, 0.8f, 0.0f);
                        break;
                }

                // Add click handler
                int characterIndex = i; // Capture index for lambda
                card.RegisterCallback<ClickEvent>(evt => SelectCharacter(characterIndex));

                // Add hover effect
                card.AddHoverEffect(1.1f, 0.2f);

                // Add to grid
                _characterGrid.Add(card);
            }
        }

        /// <summary>
        /// Select a character
        /// </summary>
        /// <param name="index">Character index</param>
        private void SelectCharacter(int index)
        {
            if (index < 0 || index >= _characters.Count) return;

            _selectedCharacterIndex = index;
            CharacterData character = _characters[index];

            // Update character display
            _characterName.text = character.Name;
            _characterClass.text = character.Class;
            _characterLevel.text = $"Level {character.Level}";

            // Update stats
            _speedFill.style.width = new StyleLength(new Length(character.SpeedStat * 100, LengthUnit.Percent));
            _cookingFill.style.width = new StyleLength(new Length(character.CookingStat * 100, LengthUnit.Percent));
            _choppingFill.style.width = new StyleLength(new Length(character.ChoppingStat * 100, LengthUnit.Percent));
            _specialFill.style.width = new StyleLength(new Length(character.SpecialStat * 100, LengthUnit.Percent));

            // Update ability
            _abilityName.text = character.AbilityName;
            _abilityDescription.text = character.AbilityDescription;

            // Update buttons
            _upgradeButton.SetEnabled(character.IsUnlocked);
            _selectButton.SetEnabled(character.IsUnlocked);
            _upgradeCostText.text = character.UpgradeCost.ToString();

            // Animate character model
            UIAnimationSystem.Instance.Animate(
                _characterModel,
                UIAnimationSystem.AnimationType.ScaleIn,
                0.3f,
                0f,
                UIEasing.EaseOutBack
            );

            // Highlight selected card
            for (int i = 0; i < _characterGrid.childCount; i++)
            {
                VisualElement card = _characterGrid[i];
                VisualElement frame = card.Q<VisualElement>("character-frame");

                if (i == index)
                {
                    frame.style.borderTopWidth = 3;
                    frame.style.borderRightWidth = 3;
                    frame.style.borderBottomWidth = 3;
                    frame.style.borderLeftWidth = 3;
                    frame.style.borderTopColor = Color.yellow;
                    frame.style.borderRightColor = Color.yellow;
                    frame.style.borderBottomColor = Color.yellow;
                    frame.style.borderLeftColor = Color.yellow;
                }
                else
                {
                    frame.style.borderTopWidth = 0;
                    frame.style.borderRightWidth = 0;
                    frame.style.borderBottomWidth = 0;
                    frame.style.borderLeftWidth = 0;
                }
            }
        }

        /// <summary>
        /// Show the character selection screen with animations
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
            var characterDisplay = _root.Q<VisualElement>("character-display");
            var characterSelection = _root.Q<VisualElement>("character-selection");
            var bottomButtons = _root.Q<VisualElement>("bottom-buttons");

            header.style.opacity = 0;
            header.transform.position = new Vector2(0, -50);

            characterDisplay.style.opacity = 0;

            characterSelection.style.opacity = 0;
            characterSelection.transform.position = new Vector2(0, 50);

            bottomButtons.style.opacity = 0;
            bottomButtons.transform.position = new Vector2(0, 50);

            // Animate header
            UIAnimationSystem.Instance.Animate(
                header,
                UIAnimationSystem.AnimationType.FadeIn,
                0.5f,
                0.2f,
                UIEasing.EaseOutCubic
            );

            // Animate character display
            UIAnimationSystem.Instance.Animate(
                characterDisplay,
                UIAnimationSystem.AnimationType.FadeIn,
                0.5f,
                0.4f,
                UIEasing.EaseOutCubic
            );

            // Animate character selection
            UIAnimationSystem.Instance.Animate(
                characterSelection,
                UIAnimationSystem.AnimationType.FadeIn,
                0.5f,
                0.6f,
                UIEasing.EaseOutCubic
            );

            // Animate bottom buttons
            UIAnimationSystem.Instance.Animate(
                bottomButtons,
                UIAnimationSystem.AnimationType.FadeIn,
                0.5f,
                0.8f,
                UIEasing.EaseOutCubic
            );

            // Animate character cards
            var cards = _characterGrid.Children();
            List<VisualElement> cardsList = new List<VisualElement>();
            foreach (var card in cards)
            {
                cardsList.Add(card);
            }

            UIAnimationSystem.Instance.AnimateSequence(
                cardsList,
                UIAnimationSystem.AnimationType.ScaleIn,
                0.3f,
                0.05f,
                UIAnimationSystem.EasingType.EaseOutBack
            );
        }

        #region Button Handlers

        /// <summary>
        /// Handle back button click
        /// </summary>
        private void OnBackButtonClicked()
        {
            Debug.Log("[CharacterSelectionScreen] Back button clicked");

            // Hide this screen
            Hide(true);

            // Show main menu screen
            UIManager.Instance.ShowScreen<MainMenuScreen>(true);
        }

        /// <summary>
        /// Handle upgrade button click
        /// </summary>
        private void OnUpgradeButtonClicked()
        {
            Debug.Log("[CharacterSelectionScreen] Upgrade button clicked");

            // TODO: Implement character upgrade logic

            // Animate button
            UIAnimationSystem.Instance.Animate(
                _upgradeButton,
                UIAnimationSystem.AnimationType.Pulse,
                0.5f,
                0f,
                UIEasing.EaseOutElastic
            );
        }

        /// <summary>
        /// Handle select button click
        /// </summary>
        private void OnSelectButtonClicked()
        {
            Debug.Log("[CharacterSelectionScreen] Select button clicked");

            // TODO: Implement character selection logic

            // Animate button
            UIAnimationSystem.Instance.Animate(
                _selectButton,
                UIAnimationSystem.AnimationType.Pulse,
                0.5f,
                0f,
                UIEasing.EaseOutElastic
            );

            // Hide this screen
            Hide(true);

            // Show main menu screen
            UIManager.Instance.ShowScreen<MainMenuScreen>(true);
        }

        #endregion
    }
}
