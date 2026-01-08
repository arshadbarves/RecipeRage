using Cysharp.Threading.Tasks;
using UI;
using UI.Popups;
using UnityEngine.SceneManagement;
using Modules.Logging;
using Modules.Persistence;
using Modules.UI;
using UI.Core;

namespace App.State.States
{
    /// <summary>
    /// State for the main menu.
    /// Note: Settings is now a tab within MainMenuScreen, not a separate screen
    /// </summary>
    public class MainMenuState : BaseState
    {
        private readonly IUIService _uiService;
        private readonly ISaveService _saveService;

        public MainMenuState(IUIService uiService, ISaveService saveService)
        {
            _uiService = uiService;
            _saveService = saveService;
        }

        public override void Enter()
        {
            base.Enter();

            // Load the main menu scene if not already loaded
            if (SceneManager.GetActiveScene().name != "MainMenu")
            {
                SceneManager.LoadScene("MainMenu");
            }

            // Show the main menu UI
            _uiService?.ShowScreen(UIScreenType.MainMenu, true, false);

            // Check if this is first time user (no username set)
            CheckAndShowUsernamePopupAsync();
        }

        private async void CheckAndShowUsernamePopupAsync()
        {
            if (_saveService == null || _uiService == null) return;

            var stats = _saveService.GetPlayerStats();

            // If no username set, show mandatory popup
            if (string.IsNullOrEmpty(stats.PlayerName))
            {
                GameLogger.Log("First time user - showing mandatory username popup");

                // Wait a bit for UI to be ready
                await UniTask.Delay(500);

                // Get the username popup screen
                var usernamePopup = _uiService.GetScreen<UsernamePopup>();
                if (usernamePopup != null)
                {
                    usernamePopup.ShowForUsername(
                        isFirstTime: true,
                        onConfirm: (newUsername) =>
                        {
                            GameLogger.Log($"Username set to: {newUsername}");
                        },
                        onCancel: () =>
                        {
                            GameLogger.Log("Username setup cancelled");
                        }
                    );
                }
                else
                {
                    GameLogger.LogError("UsernamePopup screen not found");
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
            _uiService?.HideScreen(UIScreenType.MainMenu, true);
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