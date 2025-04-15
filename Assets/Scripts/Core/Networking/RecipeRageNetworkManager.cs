using System;
using System.Collections;
using System.Collections.Generic;
using Epic.OnlineServices;
using PlayEveryWare.EpicOnlineServices;
using PlayEveryWare.EpicOnlineServices.Samples;
using RecipeRage.Core.Networking.Common;
using RecipeRage.Core.Networking.EOS;
using UnityEngine;

namespace RecipeRage.Core.Networking
{
    /// <summary>
    /// Central manager for all networking functionality in RecipeRage.
    /// </summary>
    public class RecipeRageNetworkManager : MonoBehaviour
    {
        // Singleton instance
        public static RecipeRageNetworkManager Instance { get; private set; }

        // Component references
        [SerializeField] private RecipeRageSessionManager _sessionManager;
        [SerializeField] private RecipeRageLobbyManager _lobbyManager;
        [SerializeField] private RecipeRageP2PManager _p2pManager;

        // Network state
        private bool _isConnected = false;
        private bool _isHost = false;

        // Properties
        public RecipeRageSessionManager SessionManager => _sessionManager;
        public RecipeRageLobbyManager LobbyManager => _lobbyManager;
        public RecipeRageP2PManager P2PManager => _p2pManager;
        public bool IsConnected => _isConnected;
        public bool IsHost => _isHost;

        // Events
        public event Action OnNetworkStateChanged;
        public event Action<Result> OnGameCreated;
        public event Action<Result> OnGameJoined;
        public event Action<Result> OnGameLeft;
        public event Action OnGameStarted;

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            // Singleton setup
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Create components if not assigned
            if (_sessionManager == null)
            {
                _sessionManager = gameObject.AddComponent<RecipeRageSessionManager>();
            }

            if (_lobbyManager == null)
            {
                _lobbyManager = gameObject.AddComponent<RecipeRageLobbyManager>();
            }

            if (_p2pManager == null)
            {
                _p2pManager = gameObject.AddComponent<RecipeRageP2PManager>();
            }
        }

        /// <summary>
        /// Start is called before the first frame update.
        /// </summary>
        private void Start()
        {
            // Initialize components
            StartCoroutine(InitializeComponents());
        }

        /// <summary>
        /// Initialize all networking components.
        /// </summary>
        private IEnumerator InitializeComponents()
        {
            // Wait for EOSManager to initialize
            while (EOSManager.Instance == null || EOSManager.Instance.GetLocalUserId() == null)
            {
                yield return new WaitForSeconds(0.1f);
            }

            // Initialize components
            _sessionManager.Initialize();
            _lobbyManager.Initialize();
            _p2pManager.Initialize();

            // Subscribe to events
            _sessionManager.OnSessionCreated += OnSessionCreated;
            _sessionManager.OnSessionJoined += OnSessionJoined;
            _sessionManager.OnSessionLeft += OnSessionLeft;
            _lobbyManager.OnLobbyUpdated += OnLobbyUpdated;

            Debug.Log("[RecipeRageNetworkManager] Initialized");
        }

        /// <summary>
        /// Create a new game.
        /// </summary>
        /// <param name="sessionName">The name of the session</param>
        /// <param name="gameMode">The game mode</param>
        /// <param name="mapName">The map name</param>
        /// <param name="maxPlayers">The maximum number of players</param>
        /// <param name="isPrivate">Whether the game is private</param>
        public void CreateGame(string sessionName, GameMode gameMode, string mapName, int maxPlayers = 4, bool isPrivate = false)
        {
            // Create a session
            _sessionManager.CreateGameSession(sessionName, gameMode, mapName, maxPlayers, isPrivate);

            // Create a lobby with the same settings
            _lobbyManager.CreateLobby(sessionName, maxPlayers, isPrivate);

            Debug.Log($"[RecipeRageNetworkManager] Creating game: {sessionName}, GameMode: {gameMode}, Map: {mapName}");
        }

        /// <summary>
        /// Join an existing game.
        /// </summary>
        /// <param name="sessionId">The ID of the session to join</param>
        public void JoinGame(string sessionId)
        {
            // Join the session
            _sessionManager.JoinSession(sessionId);

            Debug.Log($"[RecipeRageNetworkManager] Joining game: {sessionId}");
        }

        /// <summary>
        /// Leave the current game.
        /// </summary>
        public void LeaveGame()
        {
            // Leave the lobby first
            _lobbyManager.LeaveLobby();

            // Then leave the session
            _sessionManager.LeaveSession();

            Debug.Log("[RecipeRageNetworkManager] Leaving game");
        }

        /// <summary>
        /// Start the game.
        /// </summary>
        public void StartGame()
        {
            if (_isHost)
            {
                // Notify all players that the game is starting
                byte[] startData = new byte[0]; // No additional data needed
                _p2pManager.SendToAll(NetworkMessageType.GameState, startData);

                // Notify local listeners
                OnGameStarted?.Invoke();

                Debug.Log("[RecipeRageNetworkManager] Starting game");
            }
            else
            {
                Debug.LogError("[RecipeRageNetworkManager] Only the host can start the game");
            }
        }

        /// <summary>
        /// Check if all players are ready to start the game.
        /// </summary>
        /// <returns>Whether all players are ready</returns>
        public bool AreAllPlayersReady()
        {
            return _lobbyManager.AreAllPlayersReady();
        }

        /// <summary>
        /// Callback for session creation.
        /// </summary>
        /// <param name="result">The result</param>
        private void OnSessionCreated(Result result)
        {
            if (result == Result.Success)
            {
                _isConnected = true;
                _isHost = true;
                OnNetworkStateChanged?.Invoke();
            }

            OnGameCreated?.Invoke(result);
        }

        /// <summary>
        /// Callback for session join.
        /// </summary>
        /// <param name="result">The result</param>
        private void OnSessionJoined(Result result)
        {
            if (result == Result.Success)
            {
                _isConnected = true;
                _isHost = false;
                OnNetworkStateChanged?.Invoke();

                // Join the lobby associated with the session
                GameSessionInfo sessionInfo = _sessionManager.GetCurrentSession();
                if (sessionInfo != null)
                {
                    _lobbyManager.JoinLobby(sessionInfo.SessionId);
                }
            }

            OnGameJoined?.Invoke(result);
        }

        /// <summary>
        /// Callback for session leave.
        /// </summary>
        /// <param name="result">The result</param>
        private void OnSessionLeft(Result result)
        {
            if (result == Result.Success)
            {
                _isConnected = false;
                _isHost = false;
                OnNetworkStateChanged?.Invoke();
            }

            OnGameLeft?.Invoke(result);
        }

        /// <summary>
        /// Callback for lobby updates.
        /// </summary>
        private void OnLobbyUpdated()
        {
            // Check if all players are ready and we're the host
            if (_isHost && AreAllPlayersReady())
            {
                // Auto-start the game if all players are ready
                // This can be changed to require a manual start by the host
                // StartGame();
            }
        }

        /// <summary>
        /// Clean up when destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            // Unsubscribe from events
            if (_sessionManager != null)
            {
                _sessionManager.OnSessionCreated -= OnSessionCreated;
                _sessionManager.OnSessionJoined -= OnSessionJoined;
                _sessionManager.OnSessionLeft -= OnSessionLeft;
            }

            if (_lobbyManager != null)
            {
                _lobbyManager.OnLobbyUpdated -= OnLobbyUpdated;
            }
        }
    }
}
