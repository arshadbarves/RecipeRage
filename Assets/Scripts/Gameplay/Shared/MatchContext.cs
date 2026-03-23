using System;
using Core.Shared.Events;
using Gameplay.Characters;
using Gameplay.Cooking;
using Gameplay.GameModes;
using Gameplay.Scoring;
using Gameplay.Spawning;
using Gameplay.UI;
using Gameplay.Shared.Events;
using Unity.Netcode;

namespace Gameplay.Shared
{
    /// <summary>
    /// App-scoped read model for the currently active match runtime.
    /// Scene objects are registered by a dedicated scene binder as gameplay scenes load.
    /// </summary>
    public class MatchContext : IMatchContext, IMatchRuntimeRegistry, IDisposable
    {
        private readonly NetworkManager _networkManager;
        private readonly IEventBus _eventBus;

        private NetworkScoreManager _networkScoreManager;
        private RoundTimer _roundTimer;
        private GamePhaseSync _gamePhaseSync;
        private OrderManager _orderManager;
        private ScoreManager _scoreManager;
        private MobileControlsManager _mobileControlsManager;
        private SpawnManager _spawnManager;
        private PlayerController _localPlayer;

        public MatchContext(NetworkManager networkManager, IEventBus eventBus)
        {
            _networkManager = networkManager;
            _eventBus = eventBus;

            _eventBus?.Subscribe<LocalPlayerSpawnedEvent>(HandleLocalPlayerSpawned);
            _eventBus?.Subscribe<LocalPlayerDespawnedEvent>(HandleLocalPlayerDespawned);
        }

        public NetworkManager NetworkManager => _networkManager;
        public ulong? LocalClientId => NetworkManager?.LocalClient != null ? NetworkManager.LocalClientId : null;
        public bool IsHost => NetworkManager?.IsHost == true;
        public bool IsServer => NetworkManager?.IsServer == true;
        public bool IsClient => NetworkManager?.IsClient == true;
        public PlayerController LocalPlayer => _localPlayer;

        public NetworkScoreManager NetworkScoreManager => _networkScoreManager;
        public RoundTimer RoundTimer => _roundTimer;
        public GamePhaseSync GamePhaseSync => _gamePhaseSync;
        public OrderManager OrderManager => _orderManager;
        public ScoreManager ScoreManager => _scoreManager;
        public MobileControlsManager MobileControlsManager => _mobileControlsManager;
        public SpawnManager SpawnManager => _spawnManager;

        public void Refresh()
        {
            if (_localPlayer == null)
            {
                return;
            }

            if (!_localPlayer || !_localPlayer.IsSpawned)
            {
                _localPlayer = null;
            }
        }

        public void ShutdownNetworkSession()
        {
            if (NetworkManager != null)
            {
                NetworkManager.Shutdown();
            }

            _localPlayer = null;
        }

        public void RegisterSceneRuntime(
            OrderManager orderManager,
            ScoreManager scoreManager,
            GamePhaseSync gamePhaseSync,
            RoundTimer roundTimer,
            NetworkScoreManager networkScoreManager,
            MobileControlsManager mobileControlsManager,
            SpawnManager spawnManager)
        {
            if (orderManager != null) _orderManager = orderManager;
            if (scoreManager != null) _scoreManager = scoreManager;
            if (gamePhaseSync != null) _gamePhaseSync = gamePhaseSync;
            if (roundTimer != null) _roundTimer = roundTimer;
            if (networkScoreManager != null) _networkScoreManager = networkScoreManager;
            if (mobileControlsManager != null) _mobileControlsManager = mobileControlsManager;
            if (spawnManager != null) _spawnManager = spawnManager;
        }

        public void ClearSceneRuntime()
        {
            _networkScoreManager = null;
            _roundTimer = null;
            _gamePhaseSync = null;
            _orderManager = null;
            _scoreManager = null;
            _mobileControlsManager = null;
            _spawnManager = null;
        }

        public void Dispose()
        {
            _eventBus?.Unsubscribe<LocalPlayerSpawnedEvent>(HandleLocalPlayerSpawned);
            _eventBus?.Unsubscribe<LocalPlayerDespawnedEvent>(HandleLocalPlayerDespawned);
        }

        private void HandleLocalPlayerSpawned(LocalPlayerSpawnedEvent evt)
        {
            _localPlayer = evt?.PlayerObject != null
                ? evt.PlayerObject.GetComponent<PlayerController>()
                : null;
        }

        private void HandleLocalPlayerDespawned(LocalPlayerDespawnedEvent evt)
        {
            _localPlayer = null;
        }
    }
}
