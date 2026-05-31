using KitchenClash.Presentation.Common;
using KitchenClash.Domain;
using UnityEngine.UIElements;
using VContainer;

namespace KitchenClash.Presentation.Screens
{
    /// <summary>
    /// Standalone store screen stub - full shop UI is embedded in HomeScreen via ShopTabComponent.
    /// </summary>
    [UIScreen(UIScreenCategory.Screen, "Screens/StoreViewTemplate")]
    public class StoreScreen : BaseUIScreen
    {
        private Button _backButton;
        private Label _titleLabel;

        protected override void OnInitialize()
        {
            _backButton = GetElement<Button>("back-button");
            _titleLabel = GetElement<Label>("store-title");

            if (_backButton != null) _backButton.clicked += OnBackClicked;
        }

        protected override void OnShow()
        {
            if (_titleLabel != null) _titleLabel.text = "STORE";
        }

        private void OnBackClicked() => UIService?.GoBack();

        protected override void OnDispose()
        {
            if (_backButton != null) _backButton.clicked -= OnBackClicked;
        }
    }
}
