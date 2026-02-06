using Gameplay.Characters;
using Core.Logging;
using Gameplay.Skins;
using Gameplay.Skins.Data;
using Core.UI;
using Core.UI.Core;
using Core.UI.Interfaces;
using Core.Session;
using Gameplay.Economy;
using Gameplay.Persistence;
using Gameplay.Characters.Visuals; // Added
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Gameplay.UI.Features.Character
{
    [UIScreen(UIScreenCategory.Screen, "Screens/CharacterDetailsViewTemplate")]
    public class CharacterDetailsView : BaseUIScreen
    {
        [Inject]
        private SessionManager _sessionManager;

        private ICharacterService _characterService;
        private ISkinsService _skinsService;
        private PlayerDataService _playerDataService;
        private EconomyService _economyService;
        private CharacterPreviewManager _previewManager; // New Dependency

        private CharacterClass _currentCharacter;
        private string _selectedSkinId;

        // UI Elements
        private Button _backButton;
        private Label _characterNameLabel;
        private Label _characterDescriptionLabel;
        private VisualElement _characterPreview;
        private VisualElement _characterStats;
        private VisualElement _skinsGrid;
        
        // Actions
        private Button _selectButton;
        private Button _upgradeButton;
        private Button _unlockCharacterButton;
        private Label _currentLevelLabel;
        private Label _upgradeCostLabel;
        private Label _unlockCostLabel;

        protected override void OnInitialize()
        {
            QueryElements();
            SetupCallbacks();
            TransitionType = UITransitionType.SlideRight;
            GameLogger.Log("Initialized");
        }

        private void ResolveServices()
        {
            if (_characterService != null && _skinsService != null && _playerDataService != null && _economyService != null && _previewManager != null) return;

            var sessionContainer = _sessionManager?.SessionContainer;
            if (sessionContainer != null)
            {
                _characterService = sessionContainer.Resolve<ICharacterService>();
                _skinsService = sessionContainer.Resolve<ISkinsService>();
                _playerDataService = sessionContainer.Resolve<PlayerDataService>();
                _economyService = sessionContainer.Resolve<EconomyService>();
                _previewManager = sessionContainer.Resolve<CharacterPreviewManager>();
            }
        }

        private void QueryElements()
        {
            _backButton = GetElement<Button>("back-button");
            _characterNameLabel = GetElement<Label>("character-name");
            _characterDescriptionLabel = GetElement<Label>("character-description");
            _characterPreview = GetElement<VisualElement>("character-preview-model");
            _characterStats = GetElement<VisualElement>("character-stats");
            
            _skinsGrid = GetElement<VisualElement>("skins-grid");

            _selectButton = GetElement<Button>("select-button");
            _upgradeButton = GetElement<Button>("upgrade-button");
            _unlockCharacterButton = GetElement<Button>("unlock-character-button");
            
            _currentLevelLabel = GetElement<Label>("current-level-label");
            _upgradeCostLabel = GetElement<Label>("upgrade-cost-label");
            _unlockCostLabel = GetElement<Label>("unlock-cost-label");
        }

        private void SetupCallbacks()
        {
            if (_backButton != null) _backButton.clicked += OnBackClicked;
            if (_selectButton != null) _selectButton.clicked += OnSelectClicked;
            if (_upgradeButton != null) _upgradeButton.clicked += OnUpgradeClicked;
            if (_unlockCharacterButton != null) _unlockCharacterButton.clicked += OnUnlockCharacterClicked;
        }

        public void ShowForCharacter(CharacterClass character)
        {
            ResolveServices();
            _currentCharacter = character;

            var equippedSkin = _skinsService?.GetEquippedSkin(character.Id);
            _selectedSkinId = equippedSkin?.id;

            UpdateView();
            Show(animate: true, addToHistory: true);
        }

        private void UpdateView()
        {
            if (_currentCharacter == null) return;

            // Info
            if (_characterNameLabel != null) _characterNameLabel.text = _currentCharacter.DisplayName.ToUpper();
            if (_characterDescriptionLabel != null) _characterDescriptionLabel.text = _currentCharacter.Description;
            
            // 3D Preview logic
            Show3DPreview();

            // Logic
            bool isUnlocked = _characterService.IsUnlocked(_currentCharacter.Id);
            int currentLevel = _playerDataService?.GetCharacterLevel(_currentCharacter.Id.ToString()) ?? 1;

            if (_currentLevelLabel != null) _currentLevelLabel.text = currentLevel.ToString();

            // Stats
            UpdateCharacterStats(currentLevel);

            // Actions State
            UpdateActionButtons(isUnlocked, currentLevel);
            UpdateSelectButton();

            // Skins
            PopulateSkins(isUnlocked);
        }

        private void Show3DPreview()
        {
            if (_previewManager == null) return;
            
            // Determine which prefab to show (Skin vs Default)
            GameObject prefabToShow = null;
            if (!string.IsNullOrEmpty(_selectedSkinId))
            {
                var skin = _skinsService.GetSkin(_selectedSkinId);
                if (skin != null) prefabToShow = skin.prefab;
            }
            
            // Fallback to default skin if no specific skin selected or found
            if (prefabToShow == null)
            {
                 var defaultSkin = _currentCharacter.Skins.Find(s => s.isDefault);
                 if (defaultSkin != null) prefabToShow = defaultSkin.prefab;
            }

            if (prefabToShow != null)
            {
                _previewManager.ShowPreview(prefabToShow);
                // Ensure UI element is transparent/placeholder effectively
                if (_characterPreview != null) _characterPreview.style.backgroundImage = null; 
            }
        }

        private void UpdateActionButtons(bool isUnlocked, int level)
        {
            if (_unlockCharacterButton != null) _unlockCharacterButton.style.display = isUnlocked ? DisplayStyle.None : DisplayStyle.Flex;
            if (_upgradeButton != null) _upgradeButton.style.display = isUnlocked ? DisplayStyle.Flex : DisplayStyle.None;
            if (_selectButton != null) _selectButton.style.display = isUnlocked ? DisplayStyle.Flex : DisplayStyle.None;

            if (!isUnlocked)
            {
                if (_unlockCostLabel != null) _unlockCostLabel.text = $"{_currentCharacter.UnlockData.UnlockCost} 💰";
            }
            else
            {
                int upgradeCost = level * 100;
                if (_upgradeCostLabel != null) _upgradeCostLabel.text = $"{upgradeCost} 💰";
            }
        }

        private void UpdateCharacterStats(int level)
        {
            if (_characterStats == null || _currentCharacter == null) return;

            _characterStats.Clear();

            float levelMultiplier = 1f + (level - 1) * 0.1f;

            AddStatRow("Speed", _currentCharacter.Stats.MovementSpeedModifier * levelMultiplier);
            AddStatRow("Skill", _currentCharacter.Stats.InteractionSpeedModifier * levelMultiplier);
            AddStatRow("Capacity", _currentCharacter.Stats.CarryingCapacityModifier * levelMultiplier);

            if (_currentCharacter.PrimaryAbility.AbilityType != AbilityType.None)
            {
                var abilitySection = new VisualElement();
                abilitySection.style.marginTop = 15;
                
                var abilityTitle = new Label("SPECIAL ABILITY");
                abilityTitle.AddToClassList("stat-name"); 
                abilityTitle.style.fontSize = 14; 
                abilitySection.Add(abilityTitle);
                
                var abilityName = new Label(_currentCharacter.PrimaryAbility.AbilityType.ToString());
                abilityName.AddToClassList("stat-value");
                abilitySection.Add(abilityName);
                
                _characterStats.Add(abilitySection);
            }
        }

         private void AddStatRow(string statName, float value)
        {
            var statRow = new VisualElement();
            statRow.AddToClassList("stat-row");

            var nameLabel = new Label(statName);
            nameLabel.AddToClassList("stat-name");
            statRow.Add(nameLabel);

            var valueLabel = new Label($"{value:F1}x");
            valueLabel.AddToClassList("stat-value");
            statRow.Add(valueLabel);

            _characterStats.Add(statRow);
        }

        private void UpdateSelectButton()
        {
            if (_selectButton == null || _currentCharacter == null || _characterService == null) return;
            bool isCurrentlySelected = _characterService.SelectedCharacter?.Id == _currentCharacter.Id;
            
            _selectButton.text = isCurrentlySelected ? "SELECTED" : "SELECT";
            _selectButton.SetEnabled(!isCurrentlySelected);
        }
        
        // --- Actions ---

        private void OnUpgradeClicked()
        {
             if (_playerDataService == null || _economyService == null) return;
             
             int currentLevel = _playerDataService.GetCharacterLevel(_currentCharacter.Id.ToString());
             int cost = currentLevel * 100;
             string currencyId = EconomyKeys.CurrencyCoins;

             if (_economyService.Purchase($"Upgrade_{_currentCharacter.Id}_{currentLevel}", cost, currencyId))
             {
                 if (_playerDataService.UpgradeCharacter(_currentCharacter.Id.ToString(), cost)) 
                 {
                     UIService?.ShowNotification($"Upgraded to Level {currentLevel + 1}!", NotificationType.Success, 2f);
                     UpdateView();
                 }
             }
        }

        private void OnUnlockCharacterClicked()
        {
            if (_playerDataService == null || _economyService == null) return;
            
            int cost = _currentCharacter.UnlockData.UnlockCost;
             string currencyId = EconomyKeys.CurrencyCoins; 

            if (_economyService.Purchase($"Unlock_{_currentCharacter.Id}", cost, currencyId))
            {
                 _playerDataService.UnlockCharacter(_currentCharacter.Id.ToString());
                UIService?.ShowNotification($"Character Unlocked!", NotificationType.Success, 2f);
                UpdateView();
            }
        }

         private void OnSelectClicked()
        {
            if (_currentCharacter == null) return;

            if (_characterService.SelectCharacter(_currentCharacter.Id))
            {
                GameLogger.Log($"Character selected: {_currentCharacter.DisplayName}");
                UIService?.ShowNotification($"Selected {_currentCharacter.DisplayName}", NotificationType.Success, 2f);
                UpdateSelectButton();
                OnBackClicked(); // Return to lobby on select
            }
        }

        // --- Skins Logic ---
        
        private void PopulateSkins(bool isCharacterUnlocked)
        {
            if (_skinsGrid == null || _skinsService == null || _currentCharacter == null) return;
            _skinsGrid.Clear();
            
            if (!isCharacterUnlocked)
            {
                _skinsGrid.Add(new Label("Unlock character to view skins") { style = { color = Color.gray } });
                return;
            }

            var characterSkins = _skinsService.GetSkinsForCharacter(_currentCharacter.Id);

            foreach (SkinItem skin in characterSkins)
            {
                VisualElement skinItem = CreateSkinItem(skin);
                _skinsGrid.Add(skinItem);
            }
        }
        
        private VisualElement CreateSkinItem(SkinItem skin)
        {
            bool isUnlocked = _skinsService.IsSkinUnlocked(skin.id);
            Button skinItem = new Button(() => OnSkinSelected(skin));
            skinItem.AddToClassList("grid-slot");
            skinItem.style.width = 120; 
            skinItem.style.height = 150;

            if (skin.id == _selectedSkinId) skinItem.AddToClassList("selected");
            
            // Image
            VisualElement image = new VisualElement();
            image.AddToClassList("slot-icon");
            if (skin.icon != null) image.style.backgroundImage = new StyleBackground(skin.icon);
            skinItem.Add(image);
            
            if (!isUnlocked) {
                 VisualElement overlay = new VisualElement();
                 overlay.AddToClassList("locked-overlay");
                 skinItem.Add(overlay);

                 Label costLabel = new Label($"{skin.Price} 💰");
                 costLabel.style.color = Color.yellow;
                 costLabel.style.fontSize = 14;
                 overlay.Add(costLabel);
            }
            
            skinItem.Add(new Label(skin.name.ToUpper()) { style = { fontSize = 12, color = Color.white } });
            return skinItem;
        }

        private void OnSkinSelected(SkinItem skin)
        {
             // Preview Update
            _selectedSkinId = skin.id;
            Show3DPreview(); // Update preview immediately on click
            
            if (_economyService == null) return;

             bool isUnlocked = _skinsService.IsSkinUnlocked(skin.id);
             if (!isUnlocked)
             {
                 if (_economyService.Purchase(skin.id, skin.Price, EconomyKeys.CurrencyCoins))
                 {
                      if (_skinsService.UnlockSkin(skin.id))
                      {
                          UIService?.ShowNotification($"Unlocked {skin.name}!", NotificationType.Success, 2f);
                          if (_skinsService.EquipSkin(_currentCharacter.Id, skin.id)) _selectedSkinId = skin.id;
                          PopulateSkins(true);
                      }
                 }
             }
             else
             {
                 if (_skinsService.EquipSkin(_currentCharacter.Id, skin.id))
                 {
                     _selectedSkinId = skin.id;
                      UIService?.ShowNotification($"Equipped {skin.name}", NotificationType.Success, 1f);
                     PopulateSkins(true);
                 }
             }
        }

        private void OnBackClicked()
        {
            _previewManager?.ClearPreview(); // Clear 3D model
            UIService.GoBack();
        }

        protected override void OnDispose()
        {
            if (_backButton != null) _backButton.clicked -= OnBackClicked;
            if (_selectButton != null) _selectButton.clicked -= OnSelectClicked;
            if (_upgradeButton != null) _upgradeButton.clicked -= OnUpgradeClicked;
            if (_unlockCharacterButton != null) _unlockCharacterButton.clicked -= OnUnlockCharacterClicked;
            
            _previewManager?.ClearPreview();
        }
    }
}