using System;
using Core.Animation;
using Core.Bootstrap;
using Core.Characters;
using Core.Logging;
using UI.Core;
using UI.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Screens
{
    /// <summary>
    /// Character details screen - shows character info and available skins
    /// Matches the style of SkinsTabComponent
    /// </summary>
    [UIScreen(UIScreenType.CharacterSelection, UIScreenCategory.Screen, "Screens/CharacterDetailsTemplate")]
    public class CharacterDetailsScreen : BaseUIScreen
    {
        private ICharacterService _characterService;
        private IUIService _uiService;

        // Current character
        private CharacterClass _currentCharacter;

        // UI Elements
        private Button _backButton;
        private Label _characterNameLabel;
        private Label _characterDescriptionLabel;
        private VisualElement _characterPreview;
        private VisualElement _characterStats;
        private Button _selectButton;
        private VisualElement _skinsGrid;

        // Skins data
        private SkinsData _skinsData;
        private string _selectedSkinId;
        private string _equippedSkinId;

        protected override void OnInitialize()
        {
            // Get services
            _characterService = GameBootstrap.Services?.CharacterService;
            _uiService = GameBootstrap.Services?.UIService;

            // Query elements
            QueryElements();

            // Setup callbacks
            SetupCallbacks();

            // Load skins data
            LoadSkinsData();

            GameLogger.Log("Initialized");
        }

        private void QueryElements()
        {
            _backButton = GetElement<Button>("back-button");
            _characterNameLabel = GetElement<Label>("character-name");
            _characterDescriptionLabel = GetElement<Label>("character-description");
            _characterPreview = GetElement<VisualElement>("character-preview-model");
            _characterStats = GetElement<VisualElement>("character-stats");
            _selectButton = GetElement<Button>("select-button");
            _skinsGrid = GetElement<VisualElement>("skins-grid");
        }

        private void SetupCallbacks()
        {
            if (_backButton != null)
            {
                _backButton.clicked += OnBackClicked;
            }

            if (_selectButton != null)
            {
                _selectButton.clicked += OnSelectClicked;
            }
        }

        private void LoadSkinsData()
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("Data/Skins");
            if (jsonFile != null)
            {
                _skinsData = JsonUtility.FromJson<SkinsData>(jsonFile.text);
                GameLogger.Log($"Loaded {_skinsData.skins.Count} skins");
            }
            else
            {
                GameLogger.LogError("Failed to load Skins.json from Resources/Data");
            }

            _equippedSkinId = PlayerPrefs.GetString("EquippedSkin", "classic_chef");
            _selectedSkinId = _equippedSkinId;
        }

        /// <summary>
        /// Show this screen for a specific character
        /// </summary>
        public void ShowForCharacter(CharacterClass character)
        {
            _currentCharacter = character;
            UpdateCharacterDisplay();
            PopulateSkins();
            Show(animate: true, addToHistory: true);
        }

        private void UpdateCharacterDisplay()
        {
            if (_currentCharacter == null) return;

            // Update character name
            if (_characterNameLabel != null)
            {
                _characterNameLabel.text = _currentCharacter.DisplayName.ToUpper();
            }

            // Update description
            if (_characterDescriptionLabel != null)
            {
                _characterDescriptionLabel.text = _currentCharacter.Description;
            }

            // Update preview icon
            if (_characterPreview != null && _currentCharacter.Icon != null)
            {
                _characterPreview.style.backgroundImage = new StyleBackground(_currentCharacter.Icon);
            }

            // Update stats
            UpdateCharacterStats();

            // Update select button
            UpdateSelectButton();
        }

        private void UpdateCharacterStats()
        {
            if (_characterStats == null || _currentCharacter == null) return;

            _characterStats.Clear();

            // Add stats
            AddStatRow("Speed", _currentCharacter.MovementSpeedModifier);
            AddStatRow("Skill", _currentCharacter.InteractionSpeedModifier);
            AddStatRow("Capacity", _currentCharacter.CarryingCapacityModifier);

            // Add ability info
            if (_currentCharacter.PrimaryAbilityType != AbilityType.None)
            {
                var abilitySection = new VisualElement();
                abilitySection.style.marginTop = 15;
                abilitySection.style.paddingTop = 15;
                abilitySection.style.borderTopWidth = 2;
                abilitySection.style.borderTopColor = new Color(40f/255f, 30f/255f, 25f/255f);

                var abilityTitle = new Label("SPECIAL ABILITY");
                abilityTitle.style.fontSize = 16;
                abilityTitle.style.color = new Color(40f/255f, 30f/255f, 25f/255f);
                abilityTitle.style.marginBottom = 8;
                abilityTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
                abilitySection.Add(abilityTitle);

                var abilityName = new Label(_currentCharacter.PrimaryAbilityType.ToString());
                abilityName.style.fontSize = 14;
                abilityName.style.color = new Color(40f/255f, 30f/255f, 25f/255f);
                abilityName.style.unityFontStyleAndWeight = FontStyle.Bold;
                abilitySection.Add(abilityName);

                if (!string.IsNullOrEmpty(_currentCharacter.PrimaryAbilityDescription))
                {
                    var abilityDesc = new Label(_currentCharacter.PrimaryAbilityDescription);
                    abilityDesc.style.fontSize = 12;
                    abilityDesc.style.color = new Color(100f/255f, 80f/255f, 70f/255f);
                    abilityDesc.style.whiteSpace = WhiteSpace.Normal;
                    abilityDesc.style.marginTop = 5;
                    abilitySection.Add(abilityDesc);
                }

                var cooldownLabel = new Label($"Cooldown: {_currentCharacter.PrimaryAbilityCooldown}s");
                cooldownLabel.style.fontSize = 11;
                cooldownLabel.style.color = new Color(100f/255f, 80f/255f, 70f/255f);
                cooldownLabel.style.marginTop = 5;
                abilitySection.Add(cooldownLabel);

                _characterStats.Add(abilitySection);
            }
        }

        private void AddStatRow(string statName, float value)
        {
            var statRow = new VisualElement();
            statRow.style.flexDirection = FlexDirection.Row;
            statRow.style.justifyContent = Justify.SpaceBetween;
            statRow.style.marginBottom = 8;

            var nameLabel = new Label(statName);
            nameLabel.style.fontSize = 14;
            nameLabel.style.color = new Color(40f/255f, 30f/255f, 25f/255f);
            statRow.Add(nameLabel);

            var valueLabel = new Label($"{value:F1}x");
            valueLabel.style.fontSize = 14;
            valueLabel.style.color = new Color(100f/255f, 80f/255f, 70f/255f);
            valueLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            statRow.Add(valueLabel);

            _characterStats.Add(statRow);
        }

        private void UpdateSelectButton()
        {
            if (_selectButton == null || _currentCharacter == null) return;

            bool isCurrentlySelected = _characterService.SelectedCharacter?.Id == _currentCharacter.Id;

            if (isCurrentlySelected)
            {
                _selectButton.text = "SELECTED";
                _selectButton.AddToClassList("selected");
            }
            else
            {
                _selectButton.text = "SELECT";
                _selectButton.RemoveFromClassList("selected");
            }
        }

        private void PopulateSkins()
        {
            if (_skinsGrid == null || _skinsData == null) return;

            _skinsGrid.Clear();

            foreach (SkinItem skin in _skinsData.skins)
            {
                VisualElement skinItem = CreateSkinItem(skin);
                _skinsGrid.Add(skinItem);
            }

            GameLogger.Log($"Populated {_skinsData.skins.Count} skins");
        }

        private VisualElement CreateSkinItem(SkinItem skin)
        {
            bool isUnlocked = skin.unlocked || PlayerPrefs.GetInt($"Unlocked_{skin.id}", 0) == 1;

            Button skinItem = new Button(() => OnSkinSelected(skin));
            skinItem.AddToClassList("skin-item");

            if (skin.id == _selectedSkinId)
            {
                skinItem.AddToClassList("selected");
            }

            if (!isUnlocked)
            {
                skinItem.AddToClassList("skin-locked");
            }

            VisualElement image = new VisualElement();
            image.AddToClassList("skin-item-image");
            skinItem.Add(image);

            if (!isUnlocked)
            {
                VisualElement lockIcon = new VisualElement();
                lockIcon.AddToClassList("lock-icon");
                image.Add(lockIcon);
            }

            Label nameLabel = new Label(skin.name.ToUpper());
            nameLabel.AddToClassList("skin-item-name");
            skinItem.Add(nameLabel);

            return skinItem;
        }

        private void OnSkinSelected(SkinItem skin)
        {
            bool isUnlocked = skin.unlocked || PlayerPrefs.GetInt($"Unlocked_{skin.id}", 0) == 1;

            if (!isUnlocked)
            {
                GameLogger.Log($"Skin is locked: {skin.name}");
                _uiService?.ShowToast($"{skin.name} is locked!", ToastType.Info, 2f);
                return;
            }

            _selectedSkinId = skin.id;
            PopulateSkins();

            GameLogger.Log($"Skin selected: {skin.name}");
        }

        private void OnBackClicked()
        {
            if (_uiService != null)
            {
                // Use GoBack to return to previous screen in history
                bool wentBack = _uiService.GoBack(true);
                
                if (!wentBack)
                {
                    // If no history, manually show MainMenu
                    _uiService.ShowScreen(UIScreenType.MainMenu, true, false);
                }
            }
        }

        private void OnSelectClicked()
        {
            if (_currentCharacter == null)
            {
                GameLogger.LogWarning("No character to select");
                return;
            }

            // Select the character in the service
            bool success = _characterService.SelectCharacter(_currentCharacter.Id);

            if (success)
            {
                GameLogger.Log($"Character selected: {_currentCharacter.DisplayName}");
                _uiService?.ShowToast($"Selected {_currentCharacter.DisplayName}", ToastType.Success, 2f);

                // Update button state
                UpdateSelectButton();

                // Go back
                OnBackClicked();
            }
            else
            {
                GameLogger.LogError($"Failed to select character: {_currentCharacter.DisplayName}");
                _uiService?.ShowToast("Failed to select character", ToastType.Error, 2f);
            }
        }

        protected override void OnShow()
        {
            // Refresh when shown
            if (_currentCharacter != null)
            {
                UpdateCharacterDisplay();
                PopulateSkins();
            }
        }

        protected override void OnDispose()
        {
            if (_backButton != null)
            {
                _backButton.clicked -= OnBackClicked;
            }

            if (_selectButton != null)
            {
                _selectButton.clicked -= OnSelectClicked;
            }
        }

        public override void AnimateShow(IUIAnimator animator, VisualElement element, float duration, Action onComplete)
        {
            animator.SlideIn(element, SlideDirection.Right, duration, onComplete);
        }

        public override void AnimateHide(IUIAnimator animator, VisualElement element, float duration, Action onComplete)
        {
            animator.SlideOut(element, SlideDirection.Right, duration, onComplete);
        }
    }
}
