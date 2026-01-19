using System;
using Core.Networking;
using Core.Networking.Interfaces;
using Core.Networking.Services;
using Gameplay.App.State;
using Core.Auth;
using Core.Logging;
using Core.UI.Interfaces;
using PlayEveryWare.EpicOnlineServices;
using PlayEveryWare.EpicOnlineServices.Samples;
using VContainer;
using Cysharp.Threading.Tasks;

namespace Gameplay.App.Networking
{
    public class NetworkingServiceContainer : INetworkingServices
    {
        #region Properties

        public ILobbyManager LobbyManager { get; private set; }
        public IPlayerManager PlayerManager { get; private set; }
        public IMatchmakingService MatchmakingService { get; private set; }
        public ITeamManager TeamManager { get; private set; }
        public IGameStarter GameStarter { get; private set; }
        public IBotSpawner BotSpawner { get; set; } // Settable - initialized when bot prefab is available
        public IFriendsService FriendsService { get; private set; }

        // P2P networking now handled by Unity Netcode + EOSTransport

        #endregion

        #region Private Fields

        private bool _isInitialized;
        private readonly IUIService _uiService;
        private readonly IGameStateManager _stateManager;
        private readonly IAuthService _authService;

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor
        /// </summary>
        [Inject]
        public NetworkingServiceContainer(IUIService uiService, IGameStateManager stateManager, IAuthService authService)
        {
            _uiService = uiService;
            _stateManager = stateManager;
            _authService = authService;
            Initialize();
        }

        /// <summary>
        /// Initialize all networking services
        /// </summary>
        private void Initialize()
        {
            if (_isInitialized)
            {
                GameLogger.LogWarning("Already initialized");
                return;
            }

            // Wait for EOS to be ready
            var eosLobbyManager = EOSManager.Instance?.GetOrCreateManager<EOSLobbyManager>();

            if (eosLobbyManager == null)
            {
                GameLogger.LogError("EOSLobbyManager not available");
                return;
            }

            GameLogger.Log("Initializing services...");

            // Create services in dependency order

            // 1. Team Manager (no dependencies)
            TeamManager = new TeamManager();

            // 2. Lobby Manager (depends on TeamManager)
            LobbyManager = new LobbyService(eosLobbyManager, TeamManager);
            LobbyManager.Initialize();

            // 3. Player Manager (depends on EOS)
            PlayerManager = new PlayerManager(eosLobbyManager);

            // 4. Matchmaking Service (depends on LobbyManager)
            MatchmakingService = new MatchmakingService(LobbyManager, eosLobbyManager);
            MatchmakingService.Initialize();

            // 5. Game Starter (depends on all services)
            // Uses concrete GameStarter from App.Networking
            GameStarter = new GameStarter(this, _uiService, _stateManager);

            // 6. Bot Spawner (needs bot prefab - will be set later)
            // BotSpawner will be initialized when bot prefab is available
            BotSpawner = null; // Set this in GameplayState when prefab is loaded

            // 7. Initialize Unity Gaming Services (for friends)
            InitializeUGSAsync();

            // 8. Friends Service will be initialized after UGS authentication completes


            _isInitialized = true;
            GameLogger.Log("Initialized successfully");
        }

        /// <summary>
        /// Initialize Unity Gaming Services asynchronously
        /// FLOW: EOS (Primary) → Unity Auth (Secondary)
        /// </summary>
        private async void InitializeUGSAsync()
        {
            try
            {
                // Wait for authentication to be signed in (should be ready by now)
                // However, if we're in some edge case where it's not, we wait briefly
                int retries = 0;
                while (!_authService.IsUgsSignedIn && retries < 10)
                {
                    await UniTask.Delay(500);
                    retries++;
                }

                if (!_authService.IsUgsSignedIn)
                {
                    GameLogger.LogWarning("AuthService UGS not signed in - Friends system will be disabled");
                    return;
                }

                // Load UGS config
                var ugsConfig = UnityEngine.Resources.Load<UGSConfig>("UGSConfig");
                if (ugsConfig == null || !ugsConfig.IsValid())
                {
                    GameLogger.LogWarning("UGSConfig not found or invalid - Friends system disabled");
                    return;
                }

                // Initialize Friends Service
                if (ugsConfig.enableFriendsSystem)
                {
                    FriendsService = new FriendsService(LobbyManager, _authService);
                    FriendsService.Initialize();
                    GameLogger.Log("Unity Friends Service initialized");
                }
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Failed to initialize friends service: {ex.Message}");
            }
        }

        #endregion

        #region Update

        /// <summary>
        /// Update services (should be called from MonoBehaviour Update)
        /// </summary>
        public void Update()
        {
            if (!_isInitialized)
                return;
        }

        #endregion

        #region Disposal

        /// <summary>
        /// Dispose of all networking services
        /// </summary>
        public void Dispose()
        {
            if (!_isInitialized)
                return;

            GameLogger.Log("Disposing - leaving lobbies, closing connections");

            try
            {
                // P2P disconnection handled by Unity Netcode

                // Cancel matchmaking
                if (MatchmakingService?.IsSearching == true)
                {
                    MatchmakingService.CancelMatchmaking();
                }

                // Leave lobbies
                if (LobbyManager?.IsInMatchLobby == true)
                {
                    LobbyManager.LeaveMatchLobby();
                }

                if (LobbyManager?.IsInParty == true)
                {
                    LobbyManager.LeaveParty();
                }
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Error during disposal: {ex.Message}");
            }

            // Despawn bots if any
            BotSpawner?.DespawnAllBots();

            // Clear references
            FriendsService = null;
            BotSpawner = null;
            GameStarter = null;
            MatchmakingService = null;
            LobbyManager = null;
            PlayerManager = null;
            TeamManager = null;

            _isInitialized = false;
            GameLogger.Log("Disposed");
        }

        #endregion
    }
}
