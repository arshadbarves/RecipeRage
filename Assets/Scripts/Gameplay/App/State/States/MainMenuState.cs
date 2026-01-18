using Cysharp.Threading.Tasks;
using Gameplay.UI.Features.MainMenu;
using Gameplay.UI.Features.User;
using Gameplay.UI.Features.Loading;
using Gameplay.Persistence;
using Core.Logging;
using Core.UI.Interfaces;
using Core.Session;
using Core.Shared;
using UnityEngine.SceneManagement;
using VContainer;

namespace Gameplay.App.State.States
{
    public class MainMenuState : BaseState
    {
        private readonly IUIService _uiService;
        private readonly SessionManager _sessionManager;

        public MainMenuState(IUIService uiService, SessionManager sessionManager)
        {
            _uiService = uiService;
            _sessionManager = sessionManager;
        }

        public override async void Enter()
        {
            base.Enter();

            // Load MainMenu scene if needed
            if (SceneManager.GetActiveScene().name != GameConstants.Scenes.MainMenu)
            {
                await SceneManager.LoadSceneAsync(GameConstants.Scenes.MainMenu).ToUniTask();
            }

            // Update loading to 100% and hide it
            var loadingScreen = _uiService.GetScreen<LoadingView>();
            loadingScreen?.UpdateProgress(1.0f, "Welcome!");

            // Show MainMenu UI
            _uiService?.Show<MainMenuView>(false, true);

            await UniTask.Delay(1500);
            _uiService?.Hide<LoadingView>();

            // Check for first-time username
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
            _uiService?.Hide<MainMenuView>(true);
        }

        public override void Update() { }
        public override void FixedUpdate() { }
    }
}