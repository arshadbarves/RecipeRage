using UnityEngine.UIElements;
using RecipeRage.UI.Core;
using RecipeRage.Modules.Auth;
using RecipeRage.Modules.Logging;
using System.Threading.Tasks;
using RecipeRage.UI.Core;

namespace RecipeRage.UI.Screens
{
    public class MainScreenController : BaseScreenController
    {
        private Label _welcomeLabel;
        private Button _logoutButton;
        private Button _popupButton;

        // Example state to persist
        private int _timesShown = 0;

        public override void Initialize(UIScreenManager manager, ScreenDefinition definition, VisualElement rootElement)
        {
            base.Initialize(manager, definition, rootElement);

            _welcomeLabel = RootElement.Q<Label>("WelcomeLabel");
            _logoutButton = RootElement.Q<Button>("LogoutButton");
            _popupButton = RootElement.Q<Button>("PopupButton");

            if (_logoutButton != null)
            {
                _logoutButton.clicked += OnLogoutClicked;
            }
            else
            {
                LogHelper.Warning("MainScreenController", "LogoutButton not found.");
            }

            if (_popupButton != null)
            {
                // _popupButton.clicked += OnPopupClicked; // Add this when you have a Popup screen
            }
            else
            {
                LogHelper.Warning("MainScreenController", "PopupButton not found.");
            }
        }

        public override Task OnBeforeShowAsync(object data = null)
        {
            LogHelper.Info("MainScreenController", "Main screen showing.");
            _timesShown++;
            UpdateWelcomeMessage();
            return Task.CompletedTask;
        }

        public override void OnRestored(object restoredData = null)
        {
            LogHelper.Info("MainScreenController", "Main screen restored.");
            if (restoredData is int timesShown)
            {
                _timesShown = timesShown;
            }
            _timesShown++; // Increment even when restored
            UpdateWelcomeMessage();
        }

        public override object OnSaveState()
        {
            LogHelper.Info("MainScreenController", "Saving main screen state.");
            return _timesShown; // Save how many times this screen was shown
        }

        private void UpdateWelcomeMessage()
        {
            if (_welcomeLabel != null)
            {
                // Simplify: Just indicate if signed in or not, as we don't have GetCurrentUserName
                string welcomeText = AuthHelper.IsSignedIn() ? "Welcome!" : "Welcome, Guest!";
                _welcomeLabel.text = $"{welcomeText} (Shown {_timesShown} times)";
            }
        }

        private void OnLogoutClicked()
        {
            LogHelper.Info("MainScreenController", "Logout button clicked.");
            AuthHelper.SignOut();
            // Transition back to a login screen or splash/loading which will handle re-auth
            // Forcing a reload via splash/loading ensures auth state is re-evaluated
            _ = ScreenManager.ShowScreenAsync(ScreenId.SplashScreen, null, false); // Don't add logout to history
        }

        // private void OnPopupClicked()
        // {
        //     LogHelper.Info("MainScreenController", "Show Popup button clicked.");
        //     // Example: Show a popup screen (ensure you create a ScreenDefinition for it first)
        //     // _ = ScreenManager.ShowScreenAsync(ScreenId.YourPopupScreenId, "Data for popup");
        // }

        public override Task OnBeforeHideAsync()
        {
            LogHelper.Info("MainScreenController", "Main screen hiding.");
            return Task.CompletedTask;
        }

        public override void OnDestroy()
        {
            // Unregister callbacks if necessary (though VisualElement handles many automatically)
            if (_logoutButton != null)
            {
                _logoutButton.clicked -= OnLogoutClicked;
            }
            // if (_popupButton != null)
            // {
            //    _popupButton.clicked -= OnPopupClicked;
            // }
            LogHelper.Info("MainScreenController", "Main screen destroyed.");
        }
    }
}
