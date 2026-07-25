using KitchenClash.Application;
using System.Collections.Generic;
using KitchenClash.Application.Config;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Network.Spawning;
using Unity.Netcode;
using UnityEngine;
using Playcenter.GameFlow;
using Playcenter.Shell;
using Playcenter.UI;
using Playcenter.Services;

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
        private readonly IAppFlow _appFlow;
        private readonly ILocalNetworkIdentity _localNetworkIdentity;
        private readonly INetSession _netSession;
        private readonly IAnalyticsService _analytics;

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
            IAppFlow appFlow,
            ILocalNetworkIdentity localNetworkIdentity,
            INetSession netSession,
            IAnalyticsService analytics = null)
        {
            _lobbyManager = lobbyManager;
            _matchmakingService = matchmakingService;
            _botSpawnerRegistry = botSpawnerRegistry;
            _matchContext = matchContext;
            _uiService = uiService;
            _appFlow = appFlow;
            _localNetworkIdentity = localNetworkIdentity;
            _netSession = netSession ?? throw new System.ArgumentNullException(nameof(netSession));
            _analytics = analytics;
        }

        private NetworkManager NetcodeManager => _matchContext?.NetworkManager;

        public void StartGame()
        {
            ResetBotRuntimeState();

            LobbyInfo matchLobby = _lobbyManager.CurrentMatchLobby;

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

            string localUserId = _localNetworkIdentity?.LocalUserId;
            bool isHost = matchLobby.IsOwner(localUserId);

            GameLogger.Log($"Starting game - IsHost: {isHost}, Lobby: {matchLobby.LobbyId}");

            if (isHost)
            {
                StartAsHost(matchLobby.LobbyId ?? string.Empty);
            }
            else
            {
                StartAsClient(matchLobby.OwnerId);
            }
        }

        private void StartAsHost(string sessionToken)
        {
            GameLogger.Log("Starting as host...");

            _spawnManager = _matchContext?.SpawnManager;

            OverrideAutomaticPlayerSpawning();

            NetcodeManager.ConnectionApprovalCallback = ApprovalCheck;

            try
            {
                // IGameStarter.StartGame is sync; NGO StartHost underneath is sync.
                _netSession.StartAsync(NetRole.Host, sessionToken ?? string.Empty)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"Failed to start as host: {ex.Message}");
                OnGameStartFailed("Failed to start host");
                return;
            }

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
            {
                return;
            }

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

            var botSpawner = new Bot.BotSpawner(_playerPrefab, NetcodeManager, _spawnManager);

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

        private void StartAsClient(string hostUserIdStr)
        {
            GameLogger.Log($"Starting as client, connecting to host: {hostUserIdStr}");

            _spawnManager = _matchContext?.SpawnManager;

            try
            {
                // sessionToken = host product user id; transport + StartClient owned by INetSession.
                _netSession.StartAsync(NetRole.Client, hostUserIdStr ?? string.Empty)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"Failed to start as client: {ex.Message}");
                OnGameStartFailed("Failed to start client");
                return;
            }

            GameLogger.Log("Successfully started as client");
            OnGameStarted(false);
        }

        public void EndGame()
        {
            GameLogger.Log("Ending game...");

            bool wasActive = _isGameActive;
            _isGameActive = false;
            ResetBotRuntimeState();

            UnsubscribeFromNetworkEvents();

            if (_latencyMonitor != null)
            {
                _latencyMonitor.Dispose();
                _latencyMonitor = null;
            }

            // INetSession owns NGO shutdown (via MatchContext.ShutdownNetworkSession when available).
            try
            {
                if (_netSession.IsActive || NetcodeManager != null)
                {
                    _netSession.StopAsync().GetAwaiter().GetResult();
                    GameLogger.Log("Network session stopped via INetSession");
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"INetSession.StopAsync failed: {ex.Message}");
            }

            RestoreAutomaticPlayerSpawning();

            if (wasActive)
            {
                var endParams = new Dictionary<string, object>
                {
                    { AnalyticsEvents.Params.Reason, "end_game" }
                };
                _analytics?.LogEvent(AnalyticsEvents.MatchEnd, endParams);
                _analytics?.LogEvent(AnalyticsEvents.MatchComplete, endParams);
            }

            ReturnToLobby();
        }

        private void ReturnToLobby()
        {
            GameLogger.Log("Returning to lobby...");

            _lobbyManager.LeaveMatchLobby();

            if (_appFlow != null)
            {
                _appFlow.ReturnHome();
            }
            else
            {
                GameLogger.LogError("AppFlow not available - cannot return to Main Menu");
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

            _analytics?.LogEvent(AnalyticsEvents.MatchStart, new Dictionary<string, object>
            {
                { AnalyticsEvents.Params.IsHost, isHost }
            });
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
            {
                return;
            }

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
            BotOrderClaims.Shared.Clear();
        }
    }
}
