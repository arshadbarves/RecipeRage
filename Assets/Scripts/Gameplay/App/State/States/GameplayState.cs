using Gameplay.Camera;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using Gameplay.Networking;
using Core.Logging;
using Core.Networking;
using Core.UI.Interfaces;
using Core.Session;
using Core.Shared.Events;
using Gameplay.Shared.Events;
using Gameplay.GameModes;
using VContainer;

namespace Gameplay.App.State.States
{
    /// <summary>
    /// State for gameplay.
    /// Simplified to just load/unload scenes and delegate to GameplayLifetimeScope.
    /// </summary>
    public class GameplayState : BaseState
    {
        private readonly IEventBus _eventBus;
        private readonly IUIService _uiService;
        private readonly SessionManager _sessionManager;
        private readonly IGameModeService _gameModeService;
        private ICameraController _cameraController;

        public GameplayState(IUIService uiService, SessionManager sessionManager, IEventBus eventBus, IGameModeService gameModeService)
        {
            _uiService = uiService;
            _sessionManager = sessionManager;
            _eventBus = eventBus;
            _gameModeService = gameModeService;
        }

        public override void Enter()
        {
            base.Enter();
            InitializeCameraSystem();
            SubscribeToEvents();
            InitializeGameplayAsync().Forget();
        }

        public override void Exit()
        {
            base.Exit();

            UnsubscribeFromEvents();

            // Unload map scene
            _gameModeService?.UnloadCurrentMapAsync().Forget();

            _cameraController?.Dispose();
            _cameraController = null;
        }

        private void SubscribeToEvents()
        {
            _eventBus?.Subscribe<LocalPlayerSpawnedEvent>(OnLocalPlayerSpawned);
            _eventBus?.Subscribe<LocalPlayerDespawnedEvent>(OnLocalPlayerDespawned);
        }

        private void UnsubscribeFromEvents()
        {
            _eventBus?.Unsubscribe<LocalPlayerSpawnedEvent>(OnLocalPlayerSpawned);
            _eventBus?.Unsubscribe<LocalPlayerDespawnedEvent>(OnLocalPlayerDespawned);
        }

        private void OnLocalPlayerSpawned(LocalPlayerSpawnedEvent evt)
        {
            if (_cameraController != null && _cameraController.IsInitialized)
            {
                GameLogger.Log("GameplayState: Linking Camera to Local Player");
                _cameraController.SetFollowTarget(evt.PlayerTransform);
            }
        }

        private void OnLocalPlayerDespawned(LocalPlayerDespawnedEvent evt)
        {
            if (_cameraController != null)
            {
                 GameLogger.Log("GameplayState: Unlinking Camera from Local Player");
                _cameraController.ClearFollowTarget();
            }
        }

        private void InitializeCameraSystem()
        {
            try
            {
                var settings = Resources.Load<CameraSettings>("Data/CameraSettings/CameraSettings") ?? CameraSettings.CreateDefault();
                _cameraController = new CameraController(settings);
                _cameraController.Initialize();
            }
            catch (System.Exception ex)
            {
                GameLogger.LogException(ex);
            }
        }

        private async UniTaskVoid InitializeGameplayAsync()
        {
            // Load base Game scene if not already loaded
            if (SceneManager.GetActiveScene().name != GameConstants.Scenes.Game)
            {
                await SceneManager.LoadSceneAsync(GameConstants.Scenes.Game);
            }

            await UniTask.Yield();

            // Load map scene additively based on selected game mode
            if (!string.IsNullOrEmpty(_gameModeService?.SelectedGameMode?.MapSceneName))
            {
                GameLogger.Log($"Loading map scene: {_gameModeService.SelectedGameMode.MapSceneName}");
                bool mapLoaded = await _gameModeService.LoadMapAsync(_gameModeService.SelectedGameMode.MapSceneName);

                if (!mapLoaded)
                {
                    GameLogger.LogError($"Failed to load map: {_gameModeService.SelectedGameMode.MapSceneName}");
                }
            }
            else
            {
                GameLogger.LogWarning("No map scene specified in game mode");
            }

            await UniTask.Yield();

            // Initialize network (GameplayLifetimeScope will inject dependencies)
            var networkInitializer = Object.FindFirstObjectByType<NetworkInitializer>();
            if (networkInitializer != null)
            {
                var sessionContainer = _sessionManager?.SessionContainer;
                sessionContainer?.Inject(networkInitializer);
                networkInitializer.Initialize();
            }

            await UniTask.Yield();

            // Start game via networking services
            var sessionContainer2 = _sessionManager?.SessionContainer;
            if (sessionContainer2 != null)
            {
                var networkingServices = sessionContainer2.Resolve<INetworkingServices>();
                networkingServices?.GameStarter?.StartGame();
            }

            _uiService?.HideAllScreens(true);
        }

        public override void Update()
        {
            _cameraController?.Update(Time.deltaTime);
        }

        public override void FixedUpdate() { }
    }
}