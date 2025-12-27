using Core.Bootstrap;
using Core.Networking;
using Cysharp.Threading.Tasks;
using Gameplay;
using Gameplay.Camera;
using Gameplay.Cooking;
using Gameplay.Scoring;
using UnityEngine;
using UnityEngine.SceneManagement;
using Core.Logging;

namespace Core.State.States
{
    /// <summary>
    /// State for gameplay.
    /// Manages gameplay-scoped systems including camera, orders, and scoring.
    /// </summary>
    public class GameplayState : BaseState
    {
        private ICameraController _cameraController;
        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        public override void Enter()
        {
            base.Enter();

            // Initialize camera system
            InitializeCameraSystem();

            // Load the game scene and initialize asynchronously
            InitializeGameplayAsync().Forget();
        }

        /// <summary>
        /// Initialize the camera system for gameplay
        /// </summary>
        private void InitializeCameraSystem()
        {
            try
            {
                // Load camera settings from Resources (or use default)
                var settings = Resources.Load<CameraSettings>("Data/CameraSettings/CameraSettings");
                if (settings == null)
                {
                    GameLogger.LogWarning("CameraSettings not found in Resources/Data/CameraSettings/CameraSettings, using defaults");
                    settings = CameraSettings.CreateDefault();
                }

                // Create and initialize camera controller
                _cameraController = new CameraController(settings);
                _cameraController.Initialize();

                // Make it accessible to gameplay systems
                GameplayContext.CameraController = _cameraController;
            }
            catch (System.Exception ex)
            {
                GameLogger.LogException(ex);
                GameLogger.LogError("Failed to initialize camera system");
            }
        }

        /// <summary>
        /// Initialize gameplay asynchronously
        /// </summary>
        private async UniTaskVoid InitializeGameplayAsync()
        {
            // Load the game scene if not already loaded
            if (SceneManager.GetActiveScene().name != "Game")
            {
                GameLogger.Log("Loading Game scene...");
                await SceneManager.LoadSceneAsync("Game");
            }

            // Wait one frame for scene objects to initialize
            await UniTask.Yield();

            // Ensure NetworkInitializer exists and is initialized
            var networkInitializer = Object.FindFirstObjectByType<NetworkInitializer>();
            if (networkInitializer != null)
            {
                networkInitializer.Initialize();
            }
            else
            {
                GameLogger.LogWarning("NetworkInitializer not found in Game scene - network features may not work");
            }

            // Wait another frame for NetworkInitializer to complete
            await UniTask.Yield();

            // Start network connection (host or client)
            var networkingServices = GameBootstrap.Services?.Session?.NetworkingServices;
            if (networkingServices?.GameStarter != null)
            {
                networkingServices.GameStarter.StartGame();
            }
            else
            {
                GameLogger.LogError("GameStarter not available - cannot start network game");
            }

            // Hide all UI screens and show only the HUD
            var uiService = GameBootstrap.Services?.UIService;
            if (uiService != null)
            {
                uiService.HideAllScreens(true);
            }

            // Initialize the game mode
            var gameModeService = GameBootstrap.Services?.Session?.GameModeService;

            // Initialize the order system
            OrderManager orderManager = Object.FindFirstObjectByType<OrderManager>();
            if (orderManager != null)
            {
                orderManager.StartGeneratingOrders();
            }

            // Initialize the scoring system
            ScoreManager scoreManager = Object.FindFirstObjectByType<ScoreManager>();
            if (scoreManager != null)
            {
                scoreManager.ResetScores();
            }

            // Log bot information if any
            if (networkingServices != null)
            {
                var bots = networkingServices.MatchmakingService.GetActiveBots();
                if (bots.Count > 0)
                {
                    GameLogger.Log($"Game started with {bots.Count} bots (spawned by GameStarter)");
                }
            }
        }

        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        public override void Exit()
        {
            base.Exit();

            // Dispose camera system
            _cameraController?.Dispose();
            _cameraController = null;

            // Clear gameplay context
            GameplayContext.Reset();

            // Stop the game mode
            var gameModeService = GameBootstrap.Services?.Session?.GameModeService;
            if (gameModeService != null)
            {
                // Stop the current game mode if available
            }

            // Stop the order system
            OrderManager orderManager = Object.FindFirstObjectByType<OrderManager>();
            if (orderManager != null)
            {
                orderManager.StopGeneratingOrders();
            }
        }

        /// <summary>
        /// Called every frame to update the state.
        /// </summary>
        public override void Update()
        {
            // Update camera system
            _cameraController?.Update(Time.deltaTime);

            // Gameplay update logic
        }

        /// <summary>
        /// Called at fixed intervals for physics updates.
        /// </summary>
        public override void FixedUpdate()
        {
            // Gameplay physics update logic
        }
    }
}