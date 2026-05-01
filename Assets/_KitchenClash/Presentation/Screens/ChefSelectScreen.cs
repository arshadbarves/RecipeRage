using Gameplay.Characters;
using Gameplay.UI.Components.Tabs;
using Core.Logging;
using Core.UI;
using Core.UI.Core;
using UnityEngine.UIElements;
using VContainer;

namespace KitchenClash.Presentation.Screens
{
    [UIScreen(UIScreenCategory.Screen, "Screens/CharacterSelectionViewTemplate")]
    public class ChefSelectScreen : BaseUIScreen
    {
        [Inject]
        private ICharacterService _characterService;

        [Inject]
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
            if (_characterService == null || _characterGridRoot == null || _characterGridComponent == null)
            {
                GameLogger.LogError("Missing dependencies for Character Grid");
                return;
            }

            _characterGridComponent.Initialize(_characterGridRoot);
        }

        protected override void OnShow()
        {
            _characterGridComponent?.Refresh();
        }

        private void OnBackClicked()
        {
            UIService?.GoBack();
        }

        protected override void OnDispose()
        {
            if (_backButton != null) _backButton.clicked -= OnBackClicked;
            _characterGridComponent?.Dispose();
        }
    }
}
