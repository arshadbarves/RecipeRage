using Core.Bootstrap;
using Gameplay.Scoring;
using UI;
using UnityEngine;
using Core.Networking;
using Core.GameModes;
using VContainer;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using Core.Logging;
using Gameplay;
using Gameplay.Camera;
using Gameplay.Cooking;

namespace Core.State.States
{
    /// <summary>
    /// State for gameplay.
    /// Manages gameplay-scoped systems including camera, orders, and scoring.
    /// </summary>
    public class GameplayState : BaseState
    {
        private readonly IUIService _uiService;
        private readonly SessionManager _sessionManager;
        private ICameraController _cameraController;

        public GameplayState(IUIService uiService, SessionManager sessionManager)
        {
            _uiService = uiService;
            _sessionManager = sessionManager;
        }

        public override void Enter()
        {
            base.Enter();
            InitializeCameraSystem();
            InitializeGameplayAsync().Forget();
        }

        private void InitializeCameraSystem()
        {
            try
            {
                var settings = Resources.Load<CameraSettings>("Data/CameraSettings/CameraSettings") ?? CameraSettings.CreateDefault();
                _cameraController = new CameraController(settings);
                _cameraController.Initialize();
                GameplayContext.CameraController = _cameraController;
            }
            catch (System.Exception ex)
            {
                GameLogger.LogException(ex);
            }
        }

        private async UniTaskVoid InitializeGameplayAsync()
        {
            if (SceneManager.GetActiveScene().name != "Game")
            {
                await SceneManager.LoadSceneAsync("Game");
            }

            await UniTask.Yield();

            var networkInitializer = Object.FindFirstObjectByType<NetworkInitializer>();
            networkInitializer?.Initialize();

            await UniTask.Yield();

            var sessionContainer = _sessionManager?.SessionContainer;
            if (sessionContainer != null)
            {
                var networkingServices = sessionContainer.Resolve<INetworkingServices>();
                networkingServices?.GameStarter?.StartGame();

                var gameModeService = sessionContainer.Resolve<IGameModeService>();
            }

            _uiService?.HideAllScreens(true);

            OrderManager orderManager = Object.FindFirstObjectByType<OrderManager>();
            orderManager?.StartGeneratingOrders();

            ScoreManager scoreManager = Object.FindFirstObjectByType<ScoreManager>();
            scoreManager?.ResetScores();
        }

        public override void Exit()
        {
            base.Exit();
            _cameraController?.Dispose();
            _cameraController = null;
            GameplayContext.Reset();

            var sessionContainer = _sessionManager?.SessionContainer;
            if (sessionContainer != null)
            {
                var gameModeService = sessionContainer.Resolve<IGameModeService>();
            }

            OrderManager orderManager = Object.FindFirstObjectByType<OrderManager>();
            orderManager?.StopGeneratingOrders();
        }

        public override void Update()
        {
            _cameraController?.Update(Time.deltaTime);
        }

        public override void FixedUpdate() { }
    }
}