using System.Collections.Generic;
using Gameplay.App.State.States;
using Gameplay.Networking.Bot;
using Gameplay.Shared;
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
        private readonly ILobbyManager _lobbyManager;
        private readonly IMatchmakingService _matchmakingService;
        private readonly IBotSpawnerRegistry _botSpawnerRegistry;
        private readonly IMatchContext _matchContext;
        private readonly IUIService _uiService;
        private readonly IGameStateManager _stateManager;

        private bool _isGameActive;
        private SpawnManager _spawnManager;
        private GameObject _playerPrefab;
        private GameObject _originalPlayerPrefab;
        private bool _didDisableAutomaticPlayerSpawning;
        private LatencyMonitor _latencyMonitor;
        private int _nextHumanTeamId;

        /// <summary>
        /// Constructor with dependencies
        /// </summary>
        public GameStarter(
            ILobbyManager lobbyManager,
            IMatchmakingService matchmakingService,
            IBotSpawnerRegistry botSpawnerRegistry,
            IMatchContext matchContext,
            IUIService uiService,
            IGameStateManager stateManager)
        {
            _lobbyManager = lobbyManager;
            _matchmakingService = matchmakingService;
            _botSpawnerRegistry = botSpawnerRegistry;
            _matchContext = matchContext;
            _uiService = uiService;
            _stateManager = stateManager;
        }

        private NetworkManager NetcodeManager => _matchContext?.NetworkManager;

        /// <summary>
        /// Start the game from match lobby
        /// </summary>
        public void StartGame()
        {
            ResetBotRuntimeState();

            var matchLobby = _lobbyManager.CurrentMatchLobby;

            if (matchLobby == null)
            {
                GameLogger.LogError("No match lobby found!");
                return;
            }

            if (NetcodeManager == null)
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
            _spawnManager = _matchContext?.SpawnManager;

            // Store player prefab before disabling automatic spawning (needed for bots)
            OverrideAutomaticPlayerSpawning();

            // Subscribe to connection approval for manual spawning
            NetcodeManager.ConnectionApprovalCallback = ApprovalCheck;

            // Start Unity Netcode as host
            bool success = NetcodeManager.StartHost();

            if (success)
            {
                GameLogger.Log("Successfully started as host");
                if (_matchContext?.KitchenSupportRuntime != null)
                {
                    _matchContext.KitchenSupportRuntime.EnsureKitchenSupportStations();
                }
                else
                {
                    GameLogger.LogWarning("Kitchen support runtime not available. Skipping support station bootstrap.");
                }

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
            if (NetcodeManager?.IsServer != true)
                return;

            GameLogger.Log($"Spawning player for client {clientId}");
            int assignedTeamId = ReserveNextHumanTeam();
            TeamCategory teamCategory = ToTeamCategory(assignedTeamId);

            // Use SpawnManager if available
            if (_spawnManager != null)
            {
                bool spawned = _spawnManager.SpawnPlayer(
                    clientId,
                    teamCategory,
                    assignedTeamId,
                    ignoreSpawnCooldown: true);
                if (spawned)
                {
                    GameLogger.Log($"Player {clientId} spawned via SpawnManager on team {assignedTeamId}");
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
            List<BotPlayer> bots = _matchmakingService.GetActiveBots();
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
            var botSpawner = new Gameplay.Networking.Bot.BotSpawner(_playerPrefab, NetcodeManager, _spawnManager);

            _botSpawnerRegistry.BotSpawner = botSpawner;

            // Spawn bots immediately (using SpawnManager if available)
            botSpawner.SpawnBots(bots);

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
            _latencyMonitor = new LatencyMonitor(NetcodeManager);

            GameLogger.Log("LatencyMonitor initialized (Pure C#)");
        }



        /// <summary>
        /// Start as client
        /// </summary>
        private void StartAsClient(ProductUserId hostUserId)
        {
            GameLogger.Log($"Starting as client, connecting to host: {hostUserId}");

            // Get SpawnManager (clients need it too for reference)
            _spawnManager = _matchContext?.SpawnManager;

            // Get EOSTransport component
            var transport = NetcodeManager?.GetComponent<EOSTransport>();

            if (transport == null)
            {
                GameLogger.LogError("EOSTransport component not found on NetworkManager!");
                OnGameStartFailed("EOSTransport not configured");
                return;
            }

            // Set the host to connect to
            transport.ServerUserIdToConnectTo = hostUserId;

            // Start Unity Netcode as client
            bool success = NetcodeManager.StartClient();

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
            ResetBotRuntimeState();

            // Unsubscribe from events
            UnsubscribeFromNetworkEvents();

            if (_latencyMonitor != null)
            {
                _latencyMonitor.Dispose();
                _latencyMonitor = null;
            }

            if (NetcodeManager != null)
            {
                // Shutdown Unity Netcode
                _matchContext.ShutdownNetworkSession();
                GameLogger.Log("NetworkManager shutdown");
            }

            RestoreAutomaticPlayerSpawning();

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
            _lobbyManager.LeaveMatchLobby();

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

        private int ReserveNextHumanTeam()
        {
            int assignedTeamId = _nextHumanTeamId;
            _nextHumanTeamId = (_nextHumanTeamId + 1) % 2;
            return assignedTeamId;
        }

        private static TeamCategory ToTeamCategory(int teamId)
        {
            return teamId == 1 ? TeamCategory.TeamB : TeamCategory.TeamA;
        }

        /// <summary>
        /// Subscribe to network events for disconnect handling
        /// </summary>
        private void SubscribeToNetworkEvents()
        {
            if (NetcodeManager != null)
            {
                NetcodeManager.OnClientDisconnectCallback += OnClientDisconnected;
            }
        }

        /// <summary>
        /// Unsubscribe from network events
        /// </summary>
        private void UnsubscribeFromNetworkEvents()
        {
            if (NetcodeManager != null)
            {
                NetcodeManager.OnClientDisconnectCallback -= OnClientDisconnected;
                NetcodeManager.ConnectionApprovalCallback = null;
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
            if (_spawnManager != null && NetcodeManager?.IsServer == true)
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
            ResetBotRuntimeState();
            RestoreAutomaticPlayerSpawning();

            // Show error message
            _uiService?.ShowNotification("Game Start Failed", reason, NotificationType.Error, 4f);

            // Return to lobby
            ReturnToLobby();
        }

        private void OverrideAutomaticPlayerSpawning()
        {
            if (NetcodeManager?.NetworkConfig == null)
            {
                return;
            }

            _originalPlayerPrefab = NetcodeManager.NetworkConfig.PlayerPrefab;
            _playerPrefab = _originalPlayerPrefab;
            NetcodeManager.NetworkConfig.PlayerPrefab = null;
            _didDisableAutomaticPlayerSpawning = true;

            GameLogger.Log("Disabled automatic player spawning - using manual spawning");
        }

        private void RestoreAutomaticPlayerSpawning()
        {
            if (!_didDisableAutomaticPlayerSpawning || NetcodeManager?.NetworkConfig == null)
            {
                return;
            }

            NetcodeManager.NetworkConfig.PlayerPrefab = _originalPlayerPrefab;
            _didDisableAutomaticPlayerSpawning = false;

            GameLogger.Log("Restored automatic player spawning configuration");
        }

        private static void ResetBotRuntimeState()
        {
            BotClaimRegistry.Shared.Clear();
        }
    }
}
