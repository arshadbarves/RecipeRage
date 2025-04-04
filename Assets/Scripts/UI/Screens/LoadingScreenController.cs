using System.Threading.Tasks;
using UnityEngine.UIElements;
using RecipeRage.UI.Core;
using RecipeRage.Modules.Auth;
using RecipeRage.Modules.Logging;
using UnityEngine; // Required for Time.deltaTime
using RecipeRage.UI.Core;

namespace RecipeRage.UI.Screens
{
    public class LoadingScreenController : BaseScreenController
    {
        private Label _statusLabel;
        private VisualElement _spinner;
        private bool _isInitializationComplete = false;
        private bool _isAuthenticationComplete = false;
        private IVisualElementScheduledItem _authCheckSchedule;

        public override void Initialize(UIScreenManager manager, ScreenDefinition definition, VisualElement rootElement)
        {
            base.Initialize(manager, definition, rootElement);
            _statusLabel = RootElement.Q<Label>("StatusLabel");
            _spinner = RootElement.Q<VisualElement>("Spinner");

            if (_statusLabel == null) LogHelper.Warning("LoadingScreenController", "StatusLabel not found.");
            if (_spinner == null) LogHelper.Warning("LoadingScreenController", "Spinner not found.");
        }

        public override Task OnBeforeShowAsync(object data = null)
        {
            LogHelper.Info("LoadingScreenController", "Loading screen showing.");
            // Reset flags
            _isInitializationComplete = false;
            _isAuthenticationComplete = false;
            _authCheckSchedule?.Pause(); // Ensure previous schedule is stopped

            // Start monitoring background tasks
            StartInitializationChecks();
            return Task.CompletedTask;
        }

        public override void OnAfterShow()
        {
            // Start spinner animation (basic rotation example)
            if (_spinner != null)
            {
                // A simple way to animate rotation using the scheduler
                _spinner.schedule.Execute(() =>
                {
                    if (RootElement != null && RootElement.style.display == DisplayStyle.Flex) // Only rotate if visible
                    {
                        _spinner.transform.rotation *= Quaternion.Euler(0, 0, -180 * Time.deltaTime);
                    }
                }).Every(16); // Update roughly every frame
            }
        }

        private void StartInitializationChecks()
        {
            // Example: Check if core systems are ready (expand this as needed)
            // For now, let's assume initialization is quick or handled elsewhere before this screen
            _isInitializationComplete = true; // Placeholder
            UpdateStatus("Core Systems Ready.");

            // Check Authentication Status (using the AuthHelper)
            CheckAuthentication();
        }

        private void CheckAuthentication()
        {
            UpdateStatus("Checking Authentication...");

            // AuthHelper.Initialize is called by GameBootstrap
            // We just need to check IsSignedIn periodically until it resolves
            LogHelper.Info("LoadingScreenController", "Auth expected to be initialized by GameBootstrap. Waiting for sign-in status...");

            // Schedule a recurring check for sign-in status
            _authCheckSchedule = RootElement.schedule.Execute(() =>
            {
                if (AuthHelper.IsSignedIn())
                {
                    LogHelper.Info("LoadingScreenController", "Authentication successful.");
                    _isAuthenticationComplete = true;
                    _authCheckSchedule?.Pause(); // Stop checking
                    CheckCompletion();
                }
                // If AuthHelper *required* interaction and failed auto-login,
                // this check alone might not be sufficient. We might need an event from AuthHelper.
                // For now, assume auto-login or guest login is the path if IsSignedIn becomes true.
                // If it *never* becomes true after a timeout, we might proceed or show an error.

            }).Every(500); // Check every 500ms

            // Optional: Add a timeout for the auth check
            RootElement.schedule.Execute(() =>
            {
                if (!_isAuthenticationComplete)
                {
                    LogHelper.Warning("LoadingScreenController", "Authentication check timed out. Proceeding without confirmed login.");
                    _authCheckSchedule?.Pause(); // Stop checking
                    _isAuthenticationComplete = true; // Mark as complete to proceed
                    CheckCompletion();
                }
            }).StartingIn(10000); // Timeout after 10 seconds

        }

        private void UpdateStatus(string message)
        {
            if (_statusLabel != null)
            {
                _statusLabel.text = message;
                LogHelper.Debug("LoadingScreenController", $"Status Update: {message}");
            }
        }

        private void CheckCompletion()
        {
            if (_isInitializationComplete && _isAuthenticationComplete)
            {
                LogHelper.Info("LoadingScreenController", "Initialization and Authentication complete. Transitioning to Main Screen.");
                // All checks passed, move to the main screen
                _ = ScreenManager.ShowScreenAsync(ScreenId.MainScreen);
            }
        }

        public override Task OnBeforeHideAsync()
        {
            LogHelper.Info("LoadingScreenController", "Loading screen hiding.");
            _authCheckSchedule?.Pause(); // Stop auth check if hiding
            // Stop spinner animation if needed
            if (_spinner != null) _spinner.schedule.Execute(() => { }).Pause(); // Stop scheduled updates
            return Task.CompletedTask;
        }
    }
}
