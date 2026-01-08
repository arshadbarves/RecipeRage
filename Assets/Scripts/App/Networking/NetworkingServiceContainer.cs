using System;
using Modules.Logging;
using Modules.Networking;
using Modules.Networking.Interfaces;
using Modules.Networking.Services;
using Modules.Auth;
using App.State;
using Modules.UI;
using PlayEveryWare.EpicOnlineServices;
using PlayEveryWare.EpicOnlineServices.Samples;
using VContainer;

namespace App.Networking
{
    /// <summary>
    /// Container for all networking services
    /// Implements IDisposable for proper cleanup on logout
    /// PUBG-style architecture with Party + Match lobbies
    /// </summary>
    public class NetworkingServiceContainer : INetworkingServices, IDisposable
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
        private UGSAuthenticationManager _ugsAuthManager;

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor
        /// </summary>
        [Inject]
        public NetworkingServiceContainer(IUIService uiService, IGameStateManager stateManager)
        {
            _uiService = uiService;
            _stateManager = stateManager;
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

            // P2P networking now handled by Unity Netcode + EOSTransport
            // No need for custom P2PService

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
                // Load UGS config
                var ugsConfig = UnityEngine.Resources.Load<UGSConfig>("UGSConfig");
                if (ugsConfig == null || !ugsConfig.IsValid())
                {
                    GameLogger.LogWarning("UGSConfig not found or invalid - Friends system disabled");
                    return;
                }

                // 1. Initialize Unity Authentication
                _ugsAuthManager = new UGSAuthenticationManager(ugsConfig);
                var initialized = await _ugsAuthManager.InitializeAsync();

                if (!initialized)
                {
                    GameLogger.LogError("Failed to initialize Unity Authentication");
                    return;
                }

                GameLogger.Log("Unity Authentication initialized");

                // 2. Sign in to Unity using EOS ProductUserId (EOS is PRIMARY)
                var signedIn = await _ugsAuthManager.SignInWithEOSAsync();

                if (!signedIn)
                {
                    GameLogger.LogError("Failed to sign in to Unity with EOS identity");
                    return;
                }

                // 3. At this point, EOS ProductUserId → Unity PlayerId mapping is stored SERVER-SIDE
                // NO PlayerPrefs needed - works across all devices!
                GameLogger.Log($"✅ Authentication complete:");
                GameLogger.Log($"   EOS ProductUserId: {_ugsAuthManager.EosProductUserId} (PRIMARY)");
                GameLogger.Log($"   Unity PlayerId: {_ugsAuthManager.PlayerId} (SECONDARY)");

                // 4. Initialize Friends Service
                if (ugsConfig.enableFriendsSystem)
                {
                    FriendsService = new FriendsService(LobbyManager, _ugsAuthManager);
                    FriendsService.Initialize();
                    GameLogger.Log("Unity Friends Service initialized");
                }
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"Failed to initialize authentication: {ex.Message}");
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

            // Note: Matchmaking timeout is now handled by MatchmakingState.Update()
            // No need to update service here
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

            // Dispose UGS authentication
            _ugsAuthManager?.Dispose();

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
