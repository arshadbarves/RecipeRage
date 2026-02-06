using Gameplay.Characters;
using Gameplay.UI.Components.Tabs;
using Core.Logging;
using Core.UI;
using Core.UI.Core;
using Core.UI.Interfaces;
using UnityEngine.UIElements;
using VContainer;

namespace Gameplay.UI.Features.Character
{
    [UIScreen(UIScreenCategory.Screen, "Screens/CharacterSelectionViewTemplate")]
    public class CharacterSelectionView : BaseUIScreen
    {
        [Inject]
        private ICharacterService _characterService;

        [Inject]
        private IUIService _uiService;

        private CharacterTabComponent _characterGridComponent;
        private Button _backButton;
        private VisualElement _characterGridRoot;

        protected override void OnInitialize()
        {
            _characterGridRoot = GetElement<VisualElement>("root");
            _backButton = GetElement<Button>("back-button");

            if (_backButton != null)
            {
                _backButton.clicked += OnBackClicked;
            }

            InitializeGridComponent();
        }

        private void InitializeGridComponent()
        {
            if (_characterService == null || _characterGridRoot == null)
            {
                GameLogger.LogError("Missing dependencies for Character Grid");
                return;
            }

            // Reuse the existing grid logic from Component
            _characterGridComponent = new CharacterTabComponent(_characterService);

            // We need to inject the component manually since we are creating it here
            // But wait, the component needs IUIService injected.
            // We can pass it or use container.
            // A better way is to let container inject it, but we are inside a Monobehaviour-like lifecycle.
            // For now, let's manually inject the service it needs since we have reference
            // This relies on reflection or public property. CharacterTabComponent uses [Inject].

            // Actually, we can just resolve it if we had the container, but we don't hold the container here easily.
            // Let's modify CharacterTabComponent to allow manual service setting or just replicate the simple grid logic here?
            // Reusing is better. Let's assume DI works if we could use it, but manual is safer here.

            // Hack: Reflection inject or just a public Init method?
            // _characterGridComponent.Initialize(_characterGridRoot);
            // The component expects Injection.

            // Alternative: Re-implement grid logic here since it's simple and specific to this full screen view now.
        }

        // Re-implementing simplified grid logic to avoid DI complexity with helper classes not bound in container
        // Or we can just ask the container to inject providing we have access.
        // BaseUIScreen doesn't expose Container.

        protected override void OnShow()
        {
            if (_characterGridRoot != null)
            {
                // We'll use the existing component logic but we need to ensure it has what it needs.
                // Since we can't easily inject into 'new' objects without reference to container,
                // and we want to move fast, I will replicate the grid building logic here using the ICharacterService we already have.
                // It's cleaner than hacky injection.

                BuildCharacterGrid();
            }
        }

        private void BuildCharacterGrid()
        {
            var gridContainer = GetElement<VisualElement>("character-grid");
            if (gridContainer == null || _characterService == null) return;

            gridContainer.Clear();
            var characters = _characterService.GetAvailableCharacters();

            foreach (var character in characters)
            {
                var slot = CreateCharacterSlot(character);
                gridContainer.Add(slot);
            }
        }

        private VisualElement CreateCharacterSlot(CharacterClass character)
        {
            // Reuse style classes from Skins/Character tabs
            var slot = new Button(() => OnCharacterClicked(character));
            slot.AddToClassList("character-slot");

            bool isUnlocked = _characterService.IsUnlocked(character.Id);
            bool isSelected = _characterService.SelectedCharacter?.Id == character.Id;

            if (!isUnlocked) slot.AddToClassList("character-slot--locked");
            if (isSelected) slot.AddToClassList("character-slot--selected");

            var portrait = new VisualElement();
            portrait.AddToClassList("character-portrait");
            if (character.Icon != null) portrait.style.backgroundImage = new StyleBackground(character.Icon);

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

            var nameLabel = new Label(character.DisplayName);
            nameLabel.AddToClassList("character-name-label");
            slot.Add(nameLabel);

            return slot;
        }

        private void OnCharacterClicked(CharacterClass character)
        {
            // Open Details View
            var detailsView = UIService?.GetScreen<CharacterDetailsView>();
            detailsView?.ShowForCharacter(character);
        }

        private void OnBackClicked()
        {
            UIService?.GoBack();
        }

        protected override void OnDispose()
        {
            if (_backButton != null) _backButton.clicked -= OnBackClicked;
        }
    }
}
