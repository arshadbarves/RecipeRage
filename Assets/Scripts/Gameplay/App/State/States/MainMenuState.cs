using Cysharp.Threading.Tasks;
using Gameplay.UI.Features.MainMenu;
using Gameplay.UI.Features.User;
using Gameplay.Persistence;
using Core.Logging;
using Core.UI.Interfaces;
using Core.Session;
using UnityEngine.SceneManagement;
using VContainer;

namespace Gameplay.App.State.States
{
    /// <summary>
    /// State for the main menu.
    /// </summary>
    public class MainMenuState : BaseState
    {
        private readonly IUIService _uiService;
        private readonly SessionManager _sessionManager;

        public MainMenuState(IUIService uiService, SessionManager sessionManager)
        {
            _uiService = uiService;
            _sessionManager = sessionManager;
        }

        public override void Enter()
        {
            base.Enter();

            if (SceneManager.GetActiveScene().name != "MainMenu")
            {
                SceneManager.LoadScene("MainMenu");
            }

            _uiService?.Show<MainMenuScreen>(true, false);
            CheckAndShowUsernamePopupAsync();
        }

        private async void CheckAndShowUsernamePopupAsync()
        {
            if (_sessionManager?.IsSessionActive != true || _uiService == null) return;

            var playerDataService = _sessionManager.SessionContainer?.Resolve<PlayerDataService>();
            if (playerDataService == null) return;

            var stats = playerDataService.GetStats();

            if (string.IsNullOrEmpty(stats?.PlayerName))
            {
                GameLogger.Log("First time user - showing mandatory username popup");

                await UniTask.Delay(500);

                var usernamePopup = _uiService.GetScreen<UsernamePopup>();
                if (usernamePopup != null)
                {
                    usernamePopup.ShowForUsername(
                        isFirstTime: true,
                        onConfirm: (newUsername) => GameLogger.Log($"Username set to: {newUsername}"),
                        onCancel: () => GameLogger.Log("Username setup cancelled")
                    );
                }
                else
                {
                    GameLogger.LogError("UsernamePopup screen not found");
                }
            }
        }

        public override void Exit()
        {
            base.Exit();
            _uiService?.Hide<MainMenuScreen>(true);
        }

        public override void Update() { }
        public override void FixedUpdate() { }
    }
}