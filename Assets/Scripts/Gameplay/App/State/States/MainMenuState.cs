using System;
using Cysharp.Threading.Tasks;
using Gameplay.UI.Features.MainMenu;
using Gameplay.UI.Features.User;
using Gameplay.UI.Features.Loading;
using Core.Logging;
using Core.UI.Interfaces;
using Core.Session;
using UnityEngine.SceneManagement;
namespace Gameplay.App.State.States
{
    public class MainMenuState : BaseState
    {
        private readonly IUIService _uiService;
        private readonly ISessionContext _sessionContext;

        public MainMenuState(IUIService uiService, ISessionContext sessionContext)
        {
            _uiService = uiService;
            _sessionContext = sessionContext;
        }

        public override void Enter()
        {
            base.Enter();
            EnterAsync().Forget();
        }

        private async UniTask EnterAsync()
        {
            try
            {
                // Load MainMenu scene if needed
                if (SceneManager.GetActiveScene().name != GameConstants.Scenes.MainMenu)
                {
                    await SceneManager.LoadSceneAsync(GameConstants.Scenes.MainMenu).ToUniTask();
                }
                if (!IsStateActive) return;

                // Update loading to 100% and hide it
                var loadingScreen = _uiService.GetScreen<LoadingView>();
                loadingScreen?.UpdateProgress(1.0f, "Welcome!");

                // Show MainMenu UI
                _uiService?.SetRootScreen<MainMenuView>(false);

                await UniTask.Delay(1500, cancellationToken: StateCancellationToken);
                if (!IsStateActive) return;
                _uiService?.HideOverlay<LoadingView>();

                // Check for first-time username
                await CheckAndShowUsernamePopupAsync();
            }
            catch (OperationCanceledException)
            {
                GameLogger.Log("[MainMenuState] Enter cancelled");
            }
            catch (Exception ex)
            {
                GameLogger.LogException(ex);
            }
        }

        private async UniTask CheckAndShowUsernamePopupAsync()
        {
            if (!_sessionContext.IsSessionActive || _uiService == null) return;

            var playerDataService = _sessionContext.PlayerDataService;
            if (playerDataService == null) return;

            var stats = playerDataService.GetStats();

            if (string.IsNullOrEmpty(stats?.PlayerName))
            {
                GameLogger.Log("First time user - showing mandatory username popup");

                await UniTask.Delay(500, cancellationToken: StateCancellationToken);
                if (!IsStateActive) return;

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
