using Gameplay.Characters;
using Core.Logging;
using Gameplay.Skins;
using Gameplay.Skins.Data;
using Core.UI;
using Core.UI.Core;
using Core.UI.Interfaces;
using Core.Session;
using Gameplay.UI.Features.MainMenu;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Gameplay.UI.Features.Character
{
    /// <summary>
    /// Character details view - shows character info and available skins
    /// Matches the style of SkinsTabComponent
    /// </summary>
    [UIScreen(UIScreenCategory.Screen, "Screens/CharacterDetailsViewTemplate")]
    public class CharacterDetailsView : BaseUIScreen
    {
        [Inject]
        private SessionManager _sessionManager;

        [Inject]
        private IUIService _uiService;

        private ICharacterService _characterService;
        private ISkinsService _skinsService;

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
        private string _selectedSkinId;

        protected override void OnInitialize()
        {
            // Query elements
            QueryElements();

            // Setup callbacks
            SetupCallbacks();

            TransitionType = UITransitionType.SlideRight;

            GameLogger.Log("Initialized");
        }

        private void ResolveServices()
        {
            if (_characterService != null && _skinsService != null) return;

            var sessionContainer = _sessionManager?.SessionContainer;
            if (sessionContainer != null)
            {
                _characterService = sessionContainer.Resolve<ICharacterService>();
                _skinsService = sessionContainer.Resolve<ISkinsService>();
            }

            if (_skinsService == null)
            {
                GameLogger.LogError("SkinsService is null!");
            }
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

        public void ShowForCharacter(CharacterClass character)
        {
            ResolveServices();
            _currentCharacter = character;

            var equippedSkin = _skinsService?.GetEquippedSkin(character.Id);
            _selectedSkinId = equippedSkin?.id;

            UpdateCharacterDisplay();
            PopulateSkins();
            Show(animate: true, addToHistory: true);
        }

        private void UpdateCharacterDisplay()
        {
            if (_currentCharacter == null) return;

            if (_characterNameLabel != null)
            {
                _characterNameLabel.text = _currentCharacter.DisplayName.ToUpper();
            }

            if (_characterDescriptionLabel != null)
            {
                _characterDescriptionLabel.text = _currentCharacter.Description;
            }

            if (_characterPreview != null && _currentCharacter.Icon != null)
            {
                _characterPreview.style.backgroundImage = new StyleBackground(_currentCharacter.Icon);
            }

            UpdateCharacterStats();
            UpdateSelectButton();
        }

        private void UpdateCharacterStats()
        {
            if (_characterStats == null || _currentCharacter == null) return;

            _characterStats.Clear();

            AddStatRow("Speed", _currentCharacter.MovementSpeedModifier);
            AddStatRow("Skill", _currentCharacter.InteractionSpeedModifier);
            AddStatRow("Capacity", _currentCharacter.CarryingCapacityModifier);

            if (_currentCharacter.PrimaryAbilityType != AbilityType.None)
            {
                var abilitySection = new VisualElement();
                abilitySection.style.marginTop = 15;
                abilitySection.style.paddingTop = 15;
                abilitySection.style.borderTopWidth = 2;
                abilitySection.style.borderTopColor = new Color(40f/255f, 30f/255f, 25f/255f);

                var abilityTitle = new Label("SPECIAL ABILITY");
                abilityTitle.style.fontSize = 16;
                abilityTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
                abilitySection.Add(abilityTitle);

                var abilityName = new Label(_currentCharacter.PrimaryAbilityType.ToString());
                abilityName.style.fontSize = 14;
                abilityName.style.unityFontStyleAndWeight = FontStyle.Bold;
                abilitySection.Add(abilityName);

                if (!string.IsNullOrEmpty(_currentCharacter.PrimaryAbilityDescription))
                {
                    var abilityDesc = new Label(_currentCharacter.PrimaryAbilityDescription);
                    abilityDesc.style.fontSize = 12;
                    abilityDesc.style.whiteSpace = WhiteSpace.Normal;
                    abilitySection.Add(abilityDesc);
                }

                var cooldownLabel = new Label($"Cooldown: {_currentCharacter.PrimaryAbilityCooldown}s");
                cooldownLabel.style.fontSize = 11;
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
            statRow.Add(nameLabel);

            var valueLabel = new Label($"{value:F1}x");
            valueLabel.style.fontSize = 14;
            valueLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            statRow.Add(valueLabel);

            _characterStats.Add(statRow);
        }

        private void UpdateSelectButton()
        {
            if (_selectButton == null || _currentCharacter == null || _characterService == null) return;

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
            if (_skinsGrid == null || _skinsService == null || _currentCharacter == null) return;

            _skinsGrid.Clear();

            var characterSkins = _skinsService.GetSkinsForCharacter(_currentCharacter.Id);

            foreach (SkinItem skin in characterSkins)
            {
                VisualElement skinItem = CreateSkinItem(skin);
                _skinsGrid.Add(skinItem);
            }

            GameLogger.Log($"Populated {characterSkins.Count} skins for {_currentCharacter.DisplayName}");
        }

        private VisualElement CreateSkinItem(SkinItem skin)
        {
            bool isUnlocked = _skinsService.IsSkinUnlocked(skin.id);
            bool isEquipped = _skinsService.GetEquippedSkin(_currentCharacter.Id)?.id == skin.id;

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

                Label costLabel = new Label($"{skin.Price} 💰");
                costLabel.style.position = Position.Absolute;
                costLabel.style.bottom = 5;
                costLabel.style.width = new Length(100, LengthUnit.Percent);
                costLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                costLabel.style.color = new Color(1f, 0.8f, 0.2f);
                costLabel.style.fontSize = 12;
                costLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                image.Add(costLabel);
            }

            Label nameLabel = new Label(skin.name.ToUpper());
            nameLabel.AddToClassList("skin-item-name");
            skinItem.Add(nameLabel);

            // Add rarity badge
            Label rarityLabel = new Label(skin.rarity.ToString().ToUpper());
            rarityLabel.AddToClassList("skin-rarity-badge");
            rarityLabel.AddToClassList($"rarity-{skin.rarity.ToString().ToLower()}");
            rarityLabel.style.fontSize = 10;
            rarityLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            rarityLabel.style.marginTop = 2;
            rarityLabel.style.unityTextAlign = TextAnchor.MiddleCenter;

            // Set rarity color
            Color rarityColor = skin.rarity switch
            {
                SkinRarity.Common => new Color(0.7f, 0.7f, 0.7f),
                SkinRarity.Rare => new Color(0.3f, 0.6f, 1f),
                SkinRarity.Epic => new Color(0.8f, 0.3f, 1f),
                SkinRarity.Legendary => new Color(1f, 0.7f, 0.2f),
                _ => Color.white
            };
            rarityLabel.style.color = rarityColor;
            skinItem.Add(rarityLabel);

            if (isEquipped)
            {
                Label equippedLabel = new Label("EQUIPPED");
                equippedLabel.style.fontSize = 10;
                equippedLabel.style.color = new Color(0.4f, 0.8f, 0.4f);
                equippedLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                equippedLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                skinItem.Add(equippedLabel);
            }

            return skinItem;
        }

        private void OnSkinSelected(SkinItem skin)
        {
            bool isUnlocked = _skinsService.IsSkinUnlocked(skin.id);

            if (!isUnlocked)
            {
                GameLogger.Log($"Skin is locked: {skin.name}");
                _uiService?.ShowNotification($"{skin.name} is locked! Cost: {skin.Price} coins", NotificationType.Info, 3f);
                return;
            }

            bool success = _skinsService.EquipSkin(_currentCharacter.Id, skin.id);

            if (success)
            {
                _selectedSkinId = skin.id;
                _uiService?.ShowNotification($"Equipped {skin.name}", NotificationType.Success, 2f);
                PopulateSkins();
                GameLogger.Log($"Skin equipped: {skin.name}");
            }
            else
            {
                _uiService?.ShowNotification("Failed to equip skin", NotificationType.Error, 2f);
            }
        }

        private void OnBackClicked()
        {
            if (_uiService != null)
            {
                bool wentBack = _uiService.GoBack(true);

                if (!wentBack)
                    _uiService.Show<MainMenuView>(true, false);
            }
        }

        private void OnSelectClicked()
        {
            if (_currentCharacter == null)
            {
                GameLogger.LogWarning("No character to select");
                return;
            }

            bool success = _characterService.SelectCharacter(_currentCharacter.Id);

            if (success)
            {
                GameLogger.Log($"Character selected: {_currentCharacter.DisplayName}");
                _uiService?.ShowNotification($"Selected {_currentCharacter.DisplayName}", NotificationType.Success, 2f);
                UpdateSelectButton();
                OnBackClicked();
            }
            else
            {
                GameLogger.LogError($"Failed to select character: {_currentCharacter.DisplayName}");
                _uiService?.ShowNotification("Failed to select character", NotificationType.Error, 2f);
            }
        }

        protected override void OnShow()
        {
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

    }
}