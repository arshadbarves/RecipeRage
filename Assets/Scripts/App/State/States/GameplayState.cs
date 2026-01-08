using Modules.Shared.Interfaces;
using Core.Camera;
using Gameplay.Scoring;
using UI;
using UnityEngine;
using Modules.Networking;
using Gameplay.GameModes;
using VContainer;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using Modules.Logging;
using Modules.UI;
using Gameplay;
using Gameplay.Camera;
using Gameplay.Cooking;

namespace App.State.States
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
        private IMapLoader _mapLoader;
        private IGameModeService _gameModeService;

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
            // Load base Game scene if not already loaded
            if (SceneManager.GetActiveScene().name != "Game")
            {
                await SceneManager.LoadSceneAsync("Game");
            }

            await UniTask.Yield();

            var sessionContainer = _sessionManager?.SessionContainer;

            // Resolve services from session container
            if (sessionContainer != null)
            {
                _mapLoader = sessionContainer.Resolve<IMapLoader>();
                _gameModeService = sessionContainer.Resolve<IGameModeService>();
            }

            // Load map scene additively based on selected game mode
            if (_mapLoader != null && _gameModeService?.SelectedGameMode != null)
            {
                string mapName = _gameModeService.SelectedGameMode.DefaultMap;

                if (!string.IsNullOrEmpty(mapName))
                {
                    GameLogger.Log($"Loading map: {mapName}");
                    bool mapLoaded = await _mapLoader.LoadMapAsync(mapName);

                    if (!mapLoaded)
                    {
                        GameLogger.LogError($"Failed to load map: {mapName}");
                    }
                }
                else
                {
                    GameLogger.LogWarning("No map specified in game mode, proceeding without map");
                }
            }

            await UniTask.Yield();

            var networkInitializer = Object.FindFirstObjectByType<NetworkInitializer>();
            if (networkInitializer != null && sessionContainer != null)
            {
                sessionContainer.Inject(networkInitializer);
                networkInitializer.Initialize();
            }

            await UniTask.Yield();

            if (sessionContainer != null)
            {
                var networkingServices = sessionContainer.Resolve<INetworkingServices>();
                networkingServices?.GameStarter?.StartGame();
            }

            _uiService?.HideAllScreens(true);

            OrderManager orderManager = Object.FindFirstObjectByType<OrderManager>();
            if (orderManager != null && sessionContainer != null)
            {
                sessionContainer.Inject(orderManager);
                orderManager.StartGeneratingOrders();
            }

            ScoreManager scoreManager = Object.FindFirstObjectByType<ScoreManager>();
            if (scoreManager != null && sessionContainer != null)
            {
                sessionContainer.Inject(scoreManager);
                scoreManager.ResetScores();
            }
        }

        public override void Exit()
        {
            base.Exit();

            // Unload map scene
            if (_mapLoader != null)
            {
                _mapLoader.UnloadCurrentMapAsync().Forget();
            }

            _cameraController?.Dispose();
            _cameraController = null;
            GameplayContext.Reset();

            OrderManager orderManager = Object.FindFirstObjectByType<OrderManager>();
            orderManager?.StopGeneratingOrders();

            // Clear references
            _mapLoader = null;
            _gameModeService = null;
        }

        public override void Update()
        {
            _cameraController?.Update(Time.deltaTime);
        }

        public override void FixedUpdate() { }
    }
}