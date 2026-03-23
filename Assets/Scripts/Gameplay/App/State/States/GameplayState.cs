using System;
using Gameplay.Camera;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using Core.Logging;
using Core.UI.Interfaces;
using Core.Session;
using Core.Shared.Events;
using Gameplay.Shared.Events;
using Gameplay.GameModes;
using Gameplay.UI.Features.Gameplay;
using VContainer;

namespace Gameplay.App.State.States
{
    public class GameplayState : BaseState
    {
        private readonly IEventBus _eventBus;
        private readonly IUIService _uiService;
        private readonly ISessionContext _sessionContext;
        private readonly IGameModeService _gameModeService;
        private ICameraController _cameraController;
        private CameraShakeService _cameraShakeService;

        public GameplayState(IUIService uiService, ISessionContext sessionContext, IEventBus eventBus, IGameModeService gameModeService)
        {
            _uiService = uiService;
            _sessionContext = sessionContext;
            _eventBus = eventBus;
            _gameModeService = gameModeService;
        }

        public override void Enter()
        {
            base.Enter();
            SubscribeToEvents();
            InitializeGameplayAsync().Forget();
        }

        public override void Exit()
        {
            base.Exit();

            UnsubscribeFromEvents();
            _uiService?.HideHud<GameplayHudView>(false);

            // Unload map scene
            _gameModeService?.UnloadCurrentMapAsync().Forget();

            _cameraShakeService?.Dispose();
            _cameraShakeService = null;

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
                GameLogger.Log("GameplayState: Linking Camera to Local Player (dynamic follow)");
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

                _cameraShakeService = new CameraShakeService(_eventBus);
                _cameraShakeService.SetCameraController(_cameraController);

                GameLogger.Log("Camera system initialized - will follow player when spawned");
            }
            catch (System.Exception ex)
            {
                GameLogger.LogException(ex);
            }
        }

        private async UniTask InitializeGameplayAsync()
        {
            try
            {
                // Load base Game scene if not already loaded
                if (SceneManager.GetActiveScene().name != GameConstants.Scenes.Game)
                {
                    await SceneManager.LoadSceneAsync(GameConstants.Scenes.Game).ToUniTask();
                }
                if (!IsStateActive) return;

                // Initialize camera system AFTER scene loads
                InitializeCameraSystem();

                await UniTask.Yield(cancellationToken: StateCancellationToken);
                if (!IsStateActive) return;

                // Load map scene additively based on selected game mode
                if (!string.IsNullOrEmpty(_gameModeService?.SelectedGameMode?.MapSceneName))
                {
                    GameLogger.Log($"Loading map scene: {_gameModeService.SelectedGameMode.MapSceneName}");
                    bool mapLoaded = await _gameModeService.LoadMapAsync(_gameModeService.SelectedGameMode.MapSceneName);
                    if (!IsStateActive) return;

                    if (!mapLoaded)
                    {
                        GameLogger.LogError($"Failed to load map: {_gameModeService.SelectedGameMode.MapSceneName}");
                    }
                }
                else
                {
                    GameLogger.LogWarning("No map scene specified in game mode");
                }

                await UniTask.Yield(cancellationToken: StateCancellationToken);
                if (!IsStateActive) return;

                // Start game via networking services
                _sessionContext.GameStarter?.StartGame();

                _uiService?.HideAllModals(false);
                _uiService?.HideAllPopups(false);
                _uiService?.ShowHud<GameplayHudView>(false);
            }
            catch (OperationCanceledException)
            {
                GameLogger.Log("[GameplayState] Initialization cancelled");
            }
            catch (Exception ex)
            {
                GameLogger.LogException(ex);
            }
        }

        public override void Update()
        {
            _cameraController?.Update(Time.deltaTime);
        }

        public override void FixedUpdate() { }
    }
}
