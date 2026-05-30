using KitchenClash.Application;
using System;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using KitchenClash.Application.State;
using KitchenClash.Infrastructure.EOS;
using PlayEveryWare.EpicOnlineServices.Samples;
using VContainer;
using Cysharp.Threading.Tasks;

namespace KitchenClash.Infrastructure.Network
{
    public class NetworkingServiceContainer : INetworkingServices, IBotSpawnerRegistry
    {
        #region Properties

        public ILobbyManager LobbyManager { get; private set; }
        public IPlayerManager PlayerManager { get; private set; }
        public IMatchmakingService MatchmakingService { get; private set; }
        public ITeamManager TeamManager { get; private set; }
        public IGameStarter GameStarter { get; private set; }
        public IBotSpawner BotSpawner { get; set; }
        public IFriendsService FriendsService { get; private set; }

        #endregion

        #region Private Fields

        private bool _isInitialized;
        private readonly IUIService _uiService;
        private readonly IGameStateManager _stateManager;
        private readonly IAuthService _authService;
        private readonly IMatchContext _matchContext;
        private readonly ILobbyManager _lobbyManager;
        private readonly IPlayerManager _playerManager;
        private readonly IMatchmakingService _matchmakingService;
        private readonly ITeamManager _teamManager;
        private readonly EOSLobbyManager _eosLobbyManager;

        #endregion

        #region Initialization

        [Inject]
        public NetworkingServiceContainer(
            IUIService uiService,
            IGameStateManager stateManager,
            IAuthService authService,
            IMatchContext matchContext,
            ILobbyManager lobbyManager,
            IPlayerManager playerManager,
            IMatchmakingService matchmakingService,
            ITeamManager teamManager,
            EOSLobbyManager eosLobbyManager)
        {
            _uiService = uiService;
            _stateManager = stateManager;
            _authService = authService;
            _matchContext = matchContext;
            _lobbyManager = lobbyManager;
            _playerManager = playerManager;
            _matchmakingService = matchmakingService;
            _teamManager = teamManager;
            _eosLobbyManager = eosLobbyManager;
            Initialize();
        }

        private void Initialize()
        {
            if (_isInitialized)
            {
                GameLogger.LogWarning("Already initialized");
                return;
            }

            if (_eosLobbyManager == null)
            {
                GameLogger.LogError("EOSLobbyManager not available");
                return;
            }

            GameLogger.Log("Initializing services...");

            TeamManager = _teamManager;
            LobbyManager = _lobbyManager;
            PlayerManager = _playerManager;
            MatchmakingService = _matchmakingService;

            GameStarter = new GameStarter(LobbyManager, MatchmakingService, this, _matchContext, _uiService, _stateManager);

            BotSpawner = null;

            InitializeUGSAsync();

            _isInitialized = true;
            GameLogger.Log("Initialized successfully");
        }

        private async void InitializeUGSAsync()
        {
            try
            {
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

                var ugsConfig = UnityEngine.Resources.Load<UGSConfig>("UGSConfig");
                if (ugsConfig == null || !ugsConfig.IsValid())
                {
                    GameLogger.LogWarning("UGSConfig not found or invalid - Friends system disabled");
                    return;
                }

                if (ugsConfig.enableFriendsSystem)
                {
                    FriendsService = new EOSFriendsService(LobbyManager, _authService);
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

        public void Update()
        {
            if (!_isInitialized)
                return;
        }

        #endregion

        #region Disposal

        public void Dispose()
        {
            if (!_isInitialized)
                return;

            GameLogger.Log("Disposing - leaving lobbies, closing connections");

            try
            {
                if (MatchmakingService?.IsSearching == true)
                {
                    MatchmakingService.CancelMatchmaking();
                }

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

            BotSpawner?.DespawnAllBots();

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
