using System.Collections.Generic;
using Gameplay.App.State.States;
using Gameplay.Spawning;
using Unity.Netcode;
using UnityEngine;
using Gameplay.App.State;
using Core.Networking;
using Core.Networking.Interfaces;
using Epic.OnlineServices;
using Core.Logging;
using Core.Networking.Models;
using Core.Shared.Enums;
using Core.UI.Interfaces;
using PlayEveryWare.EpicOnlineServices;
using PlayEveryWare.EpicOnlineServices.Samples.Network;

namespace Gameplay.App.Networking
{
    /// <summary>
    /// Service for starting/ending Unity Netcode games with EOS transport
    /// Integrates with our lobby system
    /// Handles host disconnection (returns to lobby - no host migration)
    /// </summary>
    public class GameStarter : IGameStarter
    {
        private readonly INetworkingServices _networkingServices;
        private readonly IUIService _uiService;
        private readonly IGameStateManager _stateManager;

        private bool _isGameActive;
        private SpawnManager _spawnManager;
        private GameObject _playerPrefab; // Store player prefab for bot spawning
        private LatencyMonitor _latencyMonitor;

        /// <summary>
        /// Constructor with dependencies
        /// </summary>
        public GameStarter(
            INetworkingServices networkingServices,
            IUIService uiService,
            IGameStateManager stateManager)
        {
            _networkingServices = networkingServices;
            _uiService = uiService;
            _stateManager = stateManager;
        }

        /// <summary>
        /// Start the game from match lobby
        /// </summary>
        public void StartGame()
        {
            var matchLobby = _networkingServices.LobbyManager.CurrentMatchLobby;

            if (matchLobby == null)
            {
                GameLogger.LogError("No match lobby found!");
                return;
            }

            if (NetworkManager.Singleton == null)
            {
                GameLogger.LogError("NetworkManager not found in scene!");
                return;
            }

            // Determine if we're the host (lobby owner)
            var localUserId = EOSManager.Instance.GetProductUserId();
            bool isHost = matchLobby.IsOwner(localUserId);

            GameLogger.Log($"Starting game - IsHost: {isHost}, Lobby: {matchLobby.LobbyId}");

            if (isHost)
            {
                StartAsHost();
            }
            else
            {
                StartAsClient(matchLobby.OwnerId);
            }
        }

        /// <summary>
        /// Start as host
        /// </summary>
        private void StartAsHost()
        {
            GameLogger.Log("Starting as host...");

            // Get SpawnManager
            _spawnManager = SpawnManagerIntegration.GetSpawnManager();

            // Store player prefab before disabling automatic spawning (needed for bots)
            if (NetworkManager.Singleton.NetworkConfig != null)
            {
                _playerPrefab = NetworkManager.Singleton.NetworkConfig.PlayerPrefab;

                // Disable automatic spawning by clearing player prefab
                NetworkManager.Singleton.NetworkConfig.PlayerPrefab = null;

                GameLogger.Log("Disabled automatic player spawning - using manual spawning");
            }

            // Subscribe to connection approval for manual spawning
            NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;

            // Start Unity Netcode as host
            bool success = NetworkManager.Singleton.StartHost();

            if (success)
            {
                GameLogger.Log("Successfully started as host");

                // Spawn the host player (client ID 0)
                // ConnectionApprovalCallback is NOT called for the host, so we spawn manually
                ulong hostClientId = NetworkManager.ServerClientId;
                GameLogger.Log($"Spawning host player (client ID: {hostClientId})");
                SpawnPlayerForClient(hostClientId);

                // Spawn bots immediately
                SpawnBotsIfNeeded();

                // Spawn latency monitor
                SpawnLatencyMonitor();

                OnGameStarted(true);
            }
            else
            {
                GameLogger.LogError("Failed to start as host");
                OnGameStartFailed("Failed to start host");
            }
        }

        /// <summary>
        /// Connection approval callback - approves all connections and spawns players manually
        /// </summary>
        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            // Approve the connection
            response.Approved = true;
            response.CreatePlayerObject = false; // We'll spawn manually
            response.Pending = false;

            GameLogger.Log($"Connection approved for client {request.ClientNetworkId}");

            // Spawn player manually after approval
            SpawnPlayerForClient(request.ClientNetworkId);
        }

        /// <summary>
        /// Spawn player for a specific client
        /// </summary>
        private void SpawnPlayerForClient(ulong clientId)
        {
            if (!NetworkManager.Singleton.IsServer)
                return;

            GameLogger.Log($"Spawning player for client {clientId}");

            // Use SpawnManager if available
            if (_spawnManager != null)
            {
                bool spawned = _spawnManager.SpawnPlayer(clientId, TeamCategory.Neutral);
                if (spawned)
                {
                    GameLogger.Log($"Player {clientId} spawned via SpawnManager");
                }
                else
                {
                    GameLogger.LogError($"Failed to spawn player {clientId} via SpawnManager");
                }
            }
            else
            {
                GameLogger.LogError("SpawnManager not found - cannot spawn player!");
            }
        }

        /// <summary>
        /// Spawn bots if there are any in the match
        /// </summary>
        private void SpawnBotsIfNeeded()
        {
            List<BotPlayer> bots = _networkingServices.MatchmakingService.GetActiveBots();
            if (bots.Count == 0)
            {
                GameLogger.Log("No bots to spawn");
                return;
            }

            GameLogger.Log($"Spawning {bots.Count} bots immediately with players");

            // Use stored player prefab (we cleared it from NetworkManager for manual spawning)
            if (_playerPrefab == null)
            {
                GameLogger.LogError("Player prefab not available - cannot spawn bots");
                return;
            }

            // Create and initialize BotSpawner
            // Uses Gameplay implementation
            var botSpawner = new Gameplay.Networking.Bot.BotSpawner(_playerPrefab);

            // Set in networking services (cast to container)
            // Note: INetworkingServices exposes IBotSpawner, NetworkingServiceContainer implements it
            if (_networkingServices is NetworkingServiceContainer container)
            {
                container.BotSpawner = botSpawner;
            }

            // Spawn bots immediately (using SpawnManager if available)
            botSpawner.SpawnBots(bots, TeamCategory.Neutral);

            GameLogger.Log($"Spawned {bots.Count} bots - players won't know who's a bot!");
        }

        /// <summary>
        /// Spawns the latency monitor for RTT tracking
        /// </summary>
        private void SpawnLatencyMonitor()
        {
            // Clean up previous instance if any
            if (_latencyMonitor != null)
            {
                _latencyMonitor.Dispose();
                _latencyMonitor = null;
            }

            // Create pure C# monitor - works for both host and client
            // It will hook into CustomMessagingManager internally
            _latencyMonitor = new LatencyMonitor();

            GameLogger.Log("LatencyMonitor initialized (Pure C#)");
        }



        /// <summary>
        /// Start as client
        /// </summary>
        private void StartAsClient(ProductUserId hostUserId)
        {
            GameLogger.Log($"Starting as client, connecting to host: {hostUserId}");

            // Get SpawnManager (clients need it too for reference)
            _spawnManager = SpawnManagerIntegration.GetSpawnManager();

            // Get EOSTransport component
            var transport = NetworkManager.Singleton.GetComponent<EOSTransport>();

            if (transport == null)
            {
                GameLogger.LogError("EOSTransport component not found on NetworkManager!");
                OnGameStartFailed("EOSTransport not configured");
                return;
            }

            // Set the host to connect to
            transport.ServerUserIdToConnectTo = hostUserId;

            // Start Unity Netcode as client
            bool success = NetworkManager.Singleton.StartClient();

            if (success)
            {
                GameLogger.Log("Successfully started as client");
                OnGameStarted(false);
            }
            else
            {
                GameLogger.LogError("Failed to start as client");
                OnGameStartFailed("Failed to start client");
            }
        }

        /// <summary>
        /// End the current game
        /// </summary>
        public void EndGame()
        {
            GameLogger.Log("Ending game...");

            _isGameActive = false;

            // Unsubscribe from events
            UnsubscribeFromNetworkEvents();

            if (_latencyMonitor != null)
            {
                _latencyMonitor.Dispose();
                _latencyMonitor = null;
            }

            if (NetworkManager.Singleton != null)
            {
                // Shutdown Unity Netcode
                NetworkManager.Singleton.Shutdown();
                GameLogger.Log("NetworkManager shutdown");
            }

            // Leave match lobby and return to party
            ReturnToLobby();
        }

        /// <summary>
        /// Return to lobby after game ends
        /// </summary>
        private void ReturnToLobby()
        {
            GameLogger.Log("Returning to lobby...");

            // Leave match lobby
            _networkingServices.LobbyManager.LeaveMatchLobby();

            // Return to main menu
            if (_stateManager != null)
            {
                _stateManager.ChangeState<MainMenuState>();
            }
            else
            {
                GameLogger.LogError("StateManager not available - cannot return to Main Menu");
            }
        }

        /// <summary>
        /// Called when game starts successfully
        /// </summary>
        private void OnGameStarted(bool isHost)
        {
            GameLogger.Log($"Game started successfully - IsHost: {isHost}");

            _isGameActive = true;

            // Subscribe to disconnect events
            SubscribeToNetworkEvents();

            // Client needs monitor too!
            if (!isHost)
            {
                SpawnLatencyMonitor();
            }

            // Note: We're already in GameplayState (transitioned from MatchmakingState)
            // No need to transition again
        }

        /// <summary>
        /// Subscribe to network events for disconnect handling
        /// </summary>
        private void SubscribeToNetworkEvents()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            }
        }

        /// <summary>
        /// Unsubscribe from network events
        /// </summary>
        private void UnsubscribeFromNetworkEvents()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
                NetworkManager.Singleton.ConnectionApprovalCallback = null;
            }
        }

        /// <summary>
        /// Handle client disconnect
        /// </summary>
        private void OnClientDisconnected(ulong clientId)
        {
            if (!_isGameActive)
                return;

            // Release spawn point if using SpawnManager
            if (_spawnManager != null && NetworkManager.Singleton.IsServer)
            {
                _spawnManager.ReleaseSpawnPoint(clientId);
            }

            // Host disconnected?
            if (clientId == NetworkManager.ServerClientId)
            {
                GameLogger.LogWarning("Host disconnected - ending match for all players");

                // Show message to players
                _uiService?.ShowNotification("Host left the match. Returning to lobby...", NotificationType.Info);

                // End game and return to lobby
                EndGame();
            }
            else
            {
                GameLogger.Log($"Client {clientId} disconnected");
                // Client disconnect handled by game logic (remove player, etc.)
            }
        }

        /// <summary>
        /// Called when game start fails
        /// </summary>
        private void OnGameStartFailed(string reason)
        {
            GameLogger.LogError($"Game start failed: {reason}");

            // Show error message
            _uiService?.ShowNotification("Game Start Failed", reason, NotificationType.Error, 4f);

            // Return to lobby
            ReturnToLobby();
        }
    }
}