using System;
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
        public IP2PService P2PService { get; private set; }
        
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
                Debug.LogWarning("[NetworkingServiceContainer] Already initialized");
                return;
            }
            
            // Wait for EOS to be ready
            var eosLobbyManager = EOSManager.Instance?.GetOrCreateManager<EOSLobbyManager>();
            var eosP2PManager = EOSManager.Instance?.GetOrCreateManager<EOSPeer2PeerManager>();
            
            if (eosLobbyManager == null)
            {
                Debug.LogError("[NetworkingServiceContainer] EOSLobbyManager not available");
                return;
            }
            
            if (eosP2PManager == null)
            {
                Debug.LogError("[NetworkingServiceContainer] EOSPeer2PeerManager not available");
                return;
            }
            
            Debug.Log("[NetworkingServiceContainer] Initializing services...");
            
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
            
            // 5. P2P Service (depends on LobbyManager)
            P2PService = new P2PService(eosP2PManager, LobbyManager);
            P2PService.Initialize();
            
            _isInitialized = true;
            Debug.Log("[NetworkingServiceContainer] Initialized successfully");
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
            
            // Update matchmaking (for search timeout)
            if (MatchmakingService is MatchmakingService matchmaking)
            {
                matchmaking.Update();
            }
            
            // Update P2P (for message processing)
            if (P2PService is P2PService p2p)
            {
                p2p.Update();
            }
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
            
            Debug.Log("[NetworkingServiceContainer] Disposing - leaving lobbies, closing connections");
            
            try
            {
                // Disconnect P2P
                P2PService?.Disconnect();
                
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
                Debug.LogError($"[NetworkingServiceContainer] Error during disposal: {ex.Message}");
            }
            
            // Clear references
            P2PService = null;
            MatchmakingService = null;
            LobbyManager = null;
            PlayerManager = null;
            TeamManager = null;
            
            _isInitialized = false;
            Debug.Log("[NetworkingServiceContainer] Disposed");
        }
        
        #endregion
    }
}
