using Core.Bootstrap;
using Cysharp.Threading.Tasks;
using UI.UISystem;
using UI.UISystem.Popups;
using UI.UISystem.Screens;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.State.States
{
    /// <summary>
    /// State for the main menu.
    /// Note: Settings is now a tab within MainMenuScreen, not a separate screen
    /// </summary>
    public class MainMenuState : BaseState
    {
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
            }

            // Check if this is first time user (no username set)
            CheckAndShowUsernamePopupAsync();
        }

        private async void CheckAndShowUsernamePopupAsync()
        {
            var saveService = GameBootstrap.Services?.SaveService;
            var uiService = GameBootstrap.Services?.UIService;

            if (saveService == null || uiService == null) return;

            var stats = saveService.GetPlayerStats();

            // If no username set, show mandatory popup
            if (string.IsNullOrEmpty(stats.PlayerName))
            {
                Debug.Log("[MainMenuState] First time user - showing mandatory username popup");

                // Wait a bit for UI to be ready
                await UniTask.Delay(500);

                // Get the username popup screen
                var usernamePopup = uiService.GetScreen<UsernamePopup>();
                if (usernamePopup != null)
                {
                    usernamePopup.ShowForUsername(
                        isFirstTime: true,
                        onConfirm: (newUsername) =>
                        {
                            Debug.Log($"[MainMenuState] Username set to: {newUsername}");
                        },
                        onCancel: () =>
                        {
                            Debug.Log("[MainMenuState] Username setup cancelled");
                        }
                    );
                }
                else
                {
                    Debug.LogError("[MainMenuState] UsernamePopup screen not found");
                }
            }
        }

        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        public override void Exit()
        {
            base.Exit();

            // Hide the main menu UI
            var uiService = GameBootstrap.Services?.UIService;
            if (uiService != null)
            {
                uiService.HideScreen(UIScreenType.Menu, true);
            }
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