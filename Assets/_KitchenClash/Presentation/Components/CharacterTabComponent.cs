using System.Collections.Generic;
using KitchenClash.Presentation.Common;
using KitchenClash.Presentation.Screens;
using Gameplay.Characters;
using Core.Logging;
using UnityEngine.UIElements;
using VContainer;

namespace KitchenClash.Presentation.Components
{
    /// <summary>
    /// Character tab content component
    /// Displays character grid - clicking opens CharacterDetailsScreen
    /// </summary>
    public class CharacterTabComponent
    {
        [Inject]
        private IUIService _uiService;

        private VisualElement _root;
        private readonly ICharacterService _characterService;

        private VisualElement _characterGrid;

        private readonly Dictionary<int, VisualElement> _characterSlots = new Dictionary<int, VisualElement>();

        public CharacterTabComponent(ICharacterService characterService)
        {
            _characterService = characterService;
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

            QueryElements();
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

            _characterGrid.Clear();
            _characterSlots.Clear();

            var characters = _characterService.GetAvailableCharacters();

            GameLogger.Log($"Building grid with {characters.Length} characters");

            foreach (var character in characters)
            {
                var slot = CreateCharacterSlot(character);
                _characterGrid.Add(slot);
                _characterSlots[character.Id] = slot;
            }
        }

        private VisualElement CreateCharacterSlot(CharacterClass character)
        {
            var slot = new Button(() => OnCharacterClicked(character));
            slot.AddToClassList("character-slot");
            slot.name = $"character-slot-{character.Id}";

            bool isUnlocked = _characterService.IsUnlocked(character.Id);
            bool isSelected = _characterService.SelectedCharacter?.Id == character.Id;

            if (!isUnlocked)
            {
                slot.AddToClassList("character-slot--locked");
            }

            if (isSelected)
            {
                slot.AddToClassList("character-slot--selected");
            }

            var portrait = new VisualElement();
            portrait.AddToClassList("character-portrait");
            portrait.name = $"character-portrait-{character.Id}";

            if (character.Icon != null)
            {
                portrait.style.backgroundImage = new StyleBackground(character.Icon);
            }

            if (!isUnlocked)
            {
                var lockOverlay = new VisualElement();
                lockOverlay.AddToClassList("character-lock-overlay");

                var lockIcon = new Label("\ud83d\udd12");
                lockIcon.AddToClassList("character-lock-icon");
                lockOverlay.Add(lockIcon);

                portrait.Add(lockOverlay);
            }

            slot.Add(portrait);

            var nameLabel = new Label(character.DisplayName);
            nameLabel.AddToClassList("character-name-label");
            slot.Add(nameLabel);

            if (character.PrimaryAbility.AbilityType != AbilityType.None)
            {
                var abilityLabel = new Label(character.PrimaryAbility.AbilityType.ToString());
                abilityLabel.AddToClassList("character-ability-label");
                slot.Add(abilityLabel);
            }

            if (!isUnlocked)
            {
                var costLabel = new Label($"{character.UnlockData.UnlockCost} \ud83d\udcb0");
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
                _uiService?.ShowNotification($"{character.DisplayName} is locked! Cost: {character.UnlockData.UnlockCost} coins", NotificationType.Info, 3f);
                return;
            }

            GameLogger.Log($"Character clicked: {character.DisplayName} - Opening details screen");

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

        public void Refresh()
        {
            BuildCharacterGrid();
        }

        public void Update(float deltaTime)
        {
        }

        public void Dispose()
        {
            _characterSlots.Clear();
            GameLogger.Log("Disposed");
        }
    }
}
