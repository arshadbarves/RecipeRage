using System.Collections.Generic;
using KitchenClash.Application.State.States;
using KitchenClash.Infrastructure.Network.Bot;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Network.Spawning;
using Unity.Netcode;
using UnityEngine;
using KitchenClash.Application.State;
using KitchenClash.Infrastructure.Network;
using Epic.OnlineServices;
using PlayEveryWare.EpicOnlineServices;
using PlayEveryWare.EpicOnlineServices.Samples.Network;

namespace KitchenClash.Infrastructure.Network
{
    /// <summary>
    /// Service for starting/ending Unity Netcode games with EOS transport
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

        private void StartAsHost()
        {
            GameLogger.Log("Starting as host...");

            _spawnManager = _matchContext?.SpawnManager;

            OverrideAutomaticPlayerSpawning();

            NetcodeManager.ConnectionApprovalCallback = ApprovalCheck;

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

                ulong hostClientId = NetworkManager.ServerClientId;
                GameLogger.Log($"Spawning host player (client ID: {hostClientId})");
                SpawnPlayerForClient(hostClientId);

                SpawnBotsIfNeeded();

                SpawnLatencyMonitor();

                OnGameStarted(true);
            }
            else
            {
                GameLogger.LogError("Failed to start as host");
                OnGameStartFailed("Failed to start host");
            }
        }

        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            response.Approved = true;
            response.CreatePlayerObject = false;
            response.Pending = false;

            GameLogger.Log($"Connection approved for client {request.ClientNetworkId}");

            SpawnPlayerForClient(request.ClientNetworkId);
        }

        private void SpawnPlayerForClient(ulong clientId)
        {
            if (NetcodeManager?.IsServer != true)
                return;

            GameLogger.Log($"Spawning player for client {clientId}");
            int assignedTeamId = ReserveNextHumanTeam();
            TeamCategory teamCategory = ToTeamCategory(assignedTeamId);

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

        private void SpawnBotsIfNeeded()
        {
            List<BotPlayer> bots = _matchmakingService.GetActiveBots();
            if (bots.Count == 0)
            {
                GameLogger.Log("No bots to spawn");
                return;
            }

            GameLogger.Log($"Spawning {bots.Count} bots immediately with players");

            if (_playerPrefab == null)
            {
                GameLogger.LogError("Player prefab not available - cannot spawn bots");
                return;
            }

            var botSpawner = new Gameplay.Networking.Bot.BotSpawner(_playerPrefab, NetcodeManager, _spawnManager);

            _botSpawnerRegistry.BotSpawner = botSpawner;

            botSpawner.SpawnBots(bots);

            GameLogger.Log($"Spawned {bots.Count} bots - players won't know who's a bot!");
        }

        private void SpawnLatencyMonitor()
        {
            if (_latencyMonitor != null)
            {
                _latencyMonitor.Dispose();
                _latencyMonitor = null;
            }

            _latencyMonitor = new LatencyMonitor(NetcodeManager);

            GameLogger.Log("LatencyMonitor initialized (Pure C#)");
        }

        private void StartAsClient(ProductUserId hostUserId)
        {
            GameLogger.Log($"Starting as client, connecting to host: {hostUserId}");

            _spawnManager = _matchContext?.SpawnManager;

            var transport = NetcodeManager?.GetComponent<EOSTransport>();

            if (transport == null)
            {
                GameLogger.LogError("EOSTransport component not found on NetworkManager!");
                OnGameStartFailed("EOSTransport not configured");
                return;
            }

            transport.ServerUserIdToConnectTo = hostUserId;

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

        public void EndGame()
        {
            GameLogger.Log("Ending game...");

            _isGameActive = false;
            ResetBotRuntimeState();

            UnsubscribeFromNetworkEvents();

            if (_latencyMonitor != null)
            {
                _latencyMonitor.Dispose();
                _latencyMonitor = null;
            }

            if (NetcodeManager != null)
            {
                _matchContext.ShutdownNetworkSession();
                GameLogger.Log("NetworkManager shutdown");
            }

            RestoreAutomaticPlayerSpawning();

            ReturnToLobby();
        }

        private void ReturnToLobby()
        {
            GameLogger.Log("Returning to lobby...");

            _lobbyManager.LeaveMatchLobby();

            if (_stateManager != null)
            {
                _stateManager.ChangeState<MainMenuState>();
            }
            else
            {
                GameLogger.LogError("StateManager not available - cannot return to Main Menu");
            }
        }

        private void OnGameStarted(bool isHost)
        {
            GameLogger.Log($"Game started successfully - IsHost: {isHost}");

            _isGameActive = true;

            SubscribeToNetworkEvents();

            if (!isHost)
            {
                SpawnLatencyMonitor();
            }
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

        private void SubscribeToNetworkEvents()
        {
            if (NetcodeManager != null)
            {
                NetcodeManager.OnClientDisconnectCallback += OnClientDisconnected;
            }
        }

        private void UnsubscribeFromNetworkEvents()
        {
            if (NetcodeManager != null)
            {
                NetcodeManager.OnClientDisconnectCallback -= OnClientDisconnected;
                NetcodeManager.ConnectionApprovalCallback = null;
            }
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (!_isGameActive)
                return;

            if (_spawnManager != null && NetcodeManager?.IsServer == true)
            {
                _spawnManager.ReleaseSpawnPoint(clientId);
            }

            if (clientId == NetworkManager.ServerClientId)
            {
                GameLogger.LogWarning("Host disconnected - ending match for all players");

                _uiService?.ShowNotification("Host left the match. Returning to lobby...", NotificationType.Info);

                EndGame();
            }
            else
            {
                GameLogger.Log($"Client {clientId} disconnected");
            }
        }

        private void OnGameStartFailed(string reason)
        {
            GameLogger.LogError($"Game start failed: {reason}");
            ResetBotRuntimeState();
            RestoreAutomaticPlayerSpawning();

            _uiService?.ShowNotification("Game Start Failed", reason, NotificationType.Error, 4f);

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
