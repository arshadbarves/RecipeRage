using Core.Bootstrap;
using Cysharp.Threading.Tasks;
using UI.UISystem;
using UI.UISystem.Screens;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.State.States
{
    /// <summary>
    /// State for the main menu.
    /// </summary>
    public class MainMenuState : BaseState
    {
        private SettingsScreen _settingsScreen;

        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        public override void Enter()
        {
            base.Enter();

            // Load the main menu scene if not already loaded
            if (SceneManager.GetActiveScene().name != "MainMenu")
            {
                SceneManager.LoadScene("MainMenu");
            }

            // Show the main menu UI
            var uiService = GameBootstrap.Services?.UIService;
            if (uiService != null)
            {
                uiService.ShowScreen(UIScreenType.Menu, true, false);

                // Subscribe to settings screen logout event
                _settingsScreen = uiService.GetScreen<SettingsScreen>(UIScreenType.Settings);
                if (_settingsScreen != null)
                {
                    _settingsScreen.OnLogoutRequested += HandleLogoutRequested;
                }
            }
        }

        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        public override void Exit()
        {
            base.Exit();

            // Unsubscribe from settings screen events
            if (_settingsScreen != null)
            {
                _settingsScreen.OnLogoutRequested -= HandleLogoutRequested;
            }

            // Hide the main menu UI
            var uiService = GameBootstrap.Services?.UIService;
            if (uiService != null)
            {
                uiService.HideScreen(UIScreenType.Menu, true);
            }
        }

        private void HandleLogoutRequested()
        {
            Debug.Log("[MainMenuState] Logout requested");

            // Get services
            var authService = GameBootstrap.Services?.AuthenticationService;
            var uiService = GameBootstrap.Services?.UIService;

            if (authService == null || uiService == null)
            {
                Debug.LogError("[MainMenuState] Required services not available for logout");
                return;
            }

            // Logout (async)
            authService.LogoutAsync().Forget();

            // Hide settings screen
            uiService.HideScreen(UIScreenType.Settings, true);

            // Show login screen
            uiService.ShowScreen(UIScreenType.Login, true, false);

            Debug.Log("[MainMenuState] User logged out successfully");
        }

        /// <summary>
        /// Called every frame to update the state.
        /// </summary>
        public override void Update()
        {
            // Main menu update logic
        }

        /// <summary>
        /// Called at fixed intervals for physics updates.
        /// </summary>
        public override void FixedUpdate()
        {
            // Main menu physics update logic
        }
    }
}