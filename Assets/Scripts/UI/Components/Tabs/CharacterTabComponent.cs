using System.Collections.Generic;
using Core.Bootstrap;
using Core.Characters;
using Core.Logging;
using UI;
using UI.Screens;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Components.Tabs
{
    /// <summary>
    /// Character tab content component
    /// Displays character grid - clicking opens CharacterDetailsScreen
    /// Matches the style of SkinsTabComponent
    /// </summary>
    public class CharacterTabComponent
    {
        private VisualElement _root;
        private readonly ICharacterService _characterService;
        private readonly IUIService _uiService;

        // UI Elements
        private VisualElement _characterGrid;

        // State
        private readonly Dictionary<int, VisualElement> _characterSlots = new Dictionary<int, VisualElement>();

        public CharacterTabComponent(ICharacterService characterService, IUIService uiService)
        {
            _characterService = characterService;
            _uiService = uiService;
        }

        public void Initialize(VisualElement root)
        {
            GameLogger.Log("Initialize called");

            if (root == null)
            {
                GameLogger.LogError("Root is null!");
                return;
            }

            _root = root;

            if (_characterService == null)
            {
                GameLogger.LogError("CharacterService is null!");
                return;
            }

            GameLogger.Log($"Root element: {_root.name}");

            // Query elements
            QueryElements();

            // Build character grid
            BuildCharacterGrid();

            GameLogger.Log("Initialization complete");
        }

        private void QueryElements()
        {
            _characterGrid = _root.Q<VisualElement>("character-grid");
            GameLogger.Log($"Elements found - Grid: {_characterGrid != null}");
        }

        private void BuildCharacterGrid()
        {
            if (_characterGrid == null || _characterService == null)
            {
                GameLogger.LogError("Character grid or service is null");
                return;
            }

            // Clear existing slots
            _characterGrid.Clear();
            _characterSlots.Clear();

            // Get all available characters
            var characters = _characterService.GetAvailableCharacters();

            GameLogger.Log($"Building grid with {characters.Length} characters");

            // Create a slot for each character
            foreach (var character in characters)
            {
                var slot = CreateCharacterSlot(character);
                _characterGrid.Add(slot);
                _characterSlots[character.Id] = slot;
            }
        }

        private VisualElement CreateCharacterSlot(CharacterClass character)
        {
            // Create slot container (Button for click handling)
            var slot = new Button(() => OnCharacterClicked(character));
            slot.AddToClassList("character-slot");
            slot.name = $"character-slot-{character.Id}";

            // Check if unlocked
            bool isUnlocked = _characterService.IsUnlocked(character.Id);
            bool isSelected = _characterService.SelectedCharacter?.Id == character.Id;

            // Add state classes
            if (!isUnlocked)
            {
                slot.AddToClassList("character-slot--locked");
            }

            if (isSelected)
            {
                slot.AddToClassList("character-slot--selected");
            }

            // Create portrait
            var portrait = new VisualElement();
            portrait.AddToClassList("character-portrait");
            portrait.name = $"character-portrait-{character.Id}";

            // Set background image if icon exists
            if (character.Icon != null)
            {
                portrait.style.backgroundImage = new StyleBackground(character.Icon);
            }

            // Add lock overlay if locked
            if (!isUnlocked)
            {
                var lockOverlay = new VisualElement();
                lockOverlay.AddToClassList("character-lock-overlay");

                var lockIcon = new Label("🔒");
                lockIcon.AddToClassList("character-lock-icon");
                lockOverlay.Add(lockIcon);

                portrait.Add(lockOverlay);
            }

            slot.Add(portrait);

            // Create name label
            var nameLabel = new Label(character.DisplayName);
            nameLabel.AddToClassList("character-name-label");
            slot.Add(nameLabel);

            // Create ability label
            if (character.PrimaryAbilityType != AbilityType.None)
            {
                var abilityLabel = new Label(character.PrimaryAbilityType.ToString());
                abilityLabel.AddToClassList("character-ability-label");
                slot.Add(abilityLabel);
            }

            // Add unlock cost if locked
            if (!isUnlocked)
            {
                var costLabel = new Label($"{character.UnlockCost} 💰");
                costLabel.AddToClassList("character-unlock-cost");
                slot.Add(costLabel);
            }

            return slot;
        }

        private void OnCharacterClicked(CharacterClass character)
        {
            bool isUnlocked = _characterService.IsUnlocked(character.Id);

            if (!isUnlocked)
            {
                GameLogger.Log($"Locked character clicked: {character.DisplayName}");
                _uiService?.ShowNotification($"{character.DisplayName} is locked! Cost: {character.UnlockCost} coins", NotificationType.Info, 3f);
                return;
            }

            GameLogger.Log($"Character clicked: {character.DisplayName} - Opening details screen");

            // Open CharacterDetailsScreen
            if (_uiService != null)
            {
                var detailsScreen = _uiService.GetScreen<CharacterDetailsScreen>();
                if (detailsScreen != null)
                {
                    detailsScreen.ShowForCharacter(character);
                }
                else
                {
                    GameLogger.LogError("CharacterDetailsScreen not found");
                }
            }
        }

        /// <summary>
        /// Refresh the character display (call when returning to this tab)
        /// </summary>
        public void Refresh()
        {
            BuildCharacterGrid();
        }

        /// <summary>
        /// Update method - call from parent if needed
        /// </summary>
        public void Update(float deltaTime)
        {
            // Can be used for animations or updates if needed
        }

        /// <summary>
        /// Cleanup resources
        /// </summary>
        public void Dispose()
        {
            _characterSlots.Clear();
            GameLogger.Log("Disposed");
        }
    }
}
