using System;
using Core.Logging;
using Core.Networking.Bot;
using Core.Networking.Interfaces;
using Core.Networking.Services;
using PlayEveryWare.EpicOnlineServices;
using PlayEveryWare.EpicOnlineServices.Samples;
using UnityEngine;

namespace Core.Networking
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
        public GameStarter GameStarter { get; private set; }
        public BotSpawner BotSpawner { get; set; } // Settable - initialized when bot prefab is available
        public IFriendsService FriendsService { get; private set; }

        // P2P networking now handled by Unity Netcode + EOSTransport

        #endregion

        #region Private Fields

        private bool _isInitialized;

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor
        /// </summary>
        public NetworkingServiceContainer()
        {
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
            GameStarter = new GameStarter(this);

            // 6. Bot Spawner (needs bot prefab - will be set later)
            // BotSpawner will be initialized when bot prefab is available
            BotSpawner = null; // Set this in GameplayState when prefab is loaded

            // 7. Friends Service (depends on LobbyManager and Supabase)
            var supabaseConfig = UnityEngine.Resources.Load<SupabaseConfig>("SupabaseConfig");
            if (supabaseConfig != null && supabaseConfig.IsValid())
            {
                FriendsService = new FriendsService(LobbyManager, supabaseConfig);
                FriendsService.Initialize();
            }
            else
            {
                GameLogger.LogWarning("SupabaseConfig not found or invalid - Friends system disabled");
            }

            // P2P networking now handled by Unity Netcode + EOSTransport
            // No need for custom P2PService

            _isInitialized = true;
            GameLogger.Log("Initialized successfully");
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
