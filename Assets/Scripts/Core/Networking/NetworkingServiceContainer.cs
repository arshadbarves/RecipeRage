using System;
using Core.Networking.EOS;
using Core.Networking.Interfaces;
using Core.Networking.Services;
using Core.Networking.Strategies;
using PlayEveryWare.EpicOnlineServices;
using PlayEveryWare.EpicOnlineServices.Samples;
using UnityEngine;

namespace Core.Networking
{
    /// <summary>
    /// Container for all networking services - no MonoBehaviour
    /// </summary>
    public class NetworkingServiceContainer : INetworkingServices
    {
        public ILobbyManager LobbyManager { get; private set; }
        public IPlayerManager PlayerManager { get; private set; }
        public IMatchmakingService MatchmakingService { get; private set; }
        public ITeamManager TeamManager { get; private set; }

        public NetworkingServiceContainer()
        {
            Initialize();
        }

        private void Initialize()
        {
            // Wait for EOS to be ready (this should be called after EOS initialization)
            var eosLobbyManager = EOSManager.Instance?.GetOrCreateManager<EOSLobbyManager>();
            
            if (eosLobbyManager == null)
            {
                Debug.LogError("[NetworkingServiceContainer] EOSLobbyManager not available");
                return;
            }

            // Create services in dependency order
            TeamManager = new TeamManager();
            LobbyManager = new LobbyStateManager(eosLobbyManager, TeamManager);
            PlayerManager = new PlayerManager(eosLobbyManager);
            
            // Create matchmaking with strategy
            var strategy = new QuickMatchStrategy(LobbyManager);
            MatchmakingService = new MatchmakingService(LobbyManager, strategy);

            // Initialize
            LobbyManager.Initialize();

            Debug.Log("[NetworkingServiceContainer] Initialized");
        }

        public void Dispose()
        {
            // Clean up if needed
        }
    }
}
