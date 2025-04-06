using System;
using System.Collections.Generic;
using RecipeRage.Core.Patterns;
using RecipeRage.Gameplay;
using UnityEngine;
using Unity.Netcode;

namespace RecipeRage.Core.Networking
{
    /// <summary>
    /// Manages the game-specific networking functionality for RecipeRage.
    /// </summary>
    public class NetworkGameManager : MonoBehaviourSingleton<NetworkGameManager>
    {
        [Header("Game Settings")]
        [SerializeField] private float _gameTime = 300f; // 5 minutes
        [SerializeField] private int _targetScore = 1000;
        
        [Header("References")]
        [SerializeField] private NetworkManager _networkManager;
        [SerializeField] private NetworkLobbyManager _lobbyManager;
        
        /// <summary>
        /// Event triggered when the game state changes.
        /// </summary>
        public event Action<GameState> OnGameStateChanged;
        
        /// <summary>
        /// Event triggered when the game time changes.
        /// </summary>
        public event Action<float> OnGameTimeChanged;
        
        /// <summary>
        /// Event triggered when a team's score changes.
        /// </summary>
        public event Action<int, int> OnTeamScoreChanged;
        
        /// <summary>
        /// Event triggered when the game ends.
        /// </summary>
        public event Action<int> OnGameEnded;
        
        /// <summary>
        /// The current game state.
        /// </summary>
        public GameState GameState { get; private set; }
        
        /// <summary>
        /// The current game time.
        /// </summary>
        public float GameTime { get; private set; }
        
        /// <summary>
        /// The team scores.
        /// </summary>
        public int[] TeamScores { get; private set; }
        
        /// <summary>
        /// The winning team.
        /// </summary>
        public int WinningTeam { get; private set; }
        
        /// <summary>
        /// Flag to track if the game manager is initialized.
        /// </summary>
        private bool _isInitialized;
        
        /// <summary>
        /// Flag to track if the game is paused.
        /// </summary>
        private bool _isPaused;
        
        /// <summary>
        /// Dictionary of message handlers.
        /// </summary>
        private Dictionary<byte, Action<NetworkMessage>> _messageHandlers;
        
        /// <summary>
        /// Initialize the game manager.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            
            // Find the network manager if not set
            if (_networkManager == null)
            {
                _networkManager = FindObjectOfType<NetworkManager>();
            }
            
            // Find the lobby manager if not set
            if (_lobbyManager == null)
            {
                _lobbyManager = FindObjectOfType<NetworkLobbyManager>();
            }
            
            // Initialize values
            GameState = GameState.Inactive;
            GameTime = _gameTime;
            TeamScores = new int[2]; // Two teams
            WinningTeam = -1;
            _isPaused = false;
            _messageHandlers = new Dictionary<byte, Action<NetworkMessage>>();
            
            // Register the game manager with the service locator
            ServiceLocator.Instance.Register<NetworkGameManager>(this);
            
            Debug.Log("[NetworkGameManager] Game manager initialized");
        }
        
        /// <summary>
        /// Subscribe to network manager and lobby manager events.
        /// </summary>
        private void Start()
        {
            if (_networkManager != null && _lobbyManager != null)
            {
                // Subscribe to network manager events
                _networkManager.OnConnectionStateChanged += HandleConnectionStateChanged;
                
                // Subscribe to lobby manager events
                _lobbyManager.OnLobbyStateChanged += HandleLobbyStateChanged;
                
                // Register message handlers
                RegisterMessageHandlers();
                
                _isInitialized = true;
            }
            else
            {
                Debug.LogError("[NetworkGameManager] Network manager or lobby manager not found");
            }
        }
        
        /// <summary>
        /// Update the game manager.
        /// </summary>
        private void Update()
        {
            // Update game time
            if (GameState == GameState.InProgress && !_isPaused)
            {
                GameTime -= Time.deltaTime;
                OnGameTimeChanged?.Invoke(GameTime);
                
                // Check if time is up
                if (GameTime <= 0f)
                {
                    GameTime = 0f;
                    EndGame();
                }
            }
        }
        
        /// <summary>
        /// Clean up when the object is destroyed.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (_networkManager != null)
            {
                // Unsubscribe from network manager events
                _networkManager.OnConnectionStateChanged -= HandleConnectionStateChanged;
                
                // Unregister message handlers
                UnregisterMessageHandlers();
            }
            
            if (_lobbyManager != null)
            {
                // Unsubscribe from lobby manager events
                _lobbyManager.OnLobbyStateChanged -= HandleLobbyStateChanged;
            }
            
            Debug.Log("[NetworkGameManager] Game manager destroyed");
        }
        
        /// <summary>
        /// Start the game.
        /// </summary>
        public void StartGame()
        {
            if (!_isInitialized)
            {
                Debug.LogError("[NetworkGameManager] Cannot start game: manager not initialized");
                return;
            }
            
            if (GameState != GameState.Inactive && GameState != GameState.Ended)
            {
                Debug.LogError("[NetworkGameManager] Cannot start game: game already in progress");
                return;
            }
            
            // Only the host can start the game
            if (!_networkManager.LocalPlayer.IsHost)
            {
                Debug.LogError("[NetworkGameManager] Cannot start game: not the host");
                return;
            }
            
            Debug.Log("[NetworkGameManager] Starting game");
            
            // Reset game values
            GameTime = _gameTime;
            TeamScores[0] = 0;
            TeamScores[1] = 0;
            WinningTeam = -1;
            _isPaused = false;
            
            // Set game state to starting
            SetGameState(GameState.Starting);
            
            // Notify other players
            SendGameStateMessage(GameState.Starting);
            
            // Start the game after a short delay
            Invoke(nameof(SetGameInProgress), 3f);
        }
        
        /// <summary>
        /// Pause the game.
        /// </summary>
        public void PauseGame()
        {
            if (!_isInitialized)
            {
                Debug.LogError("[NetworkGameManager] Cannot pause game: manager not initialized");
                return;
            }
            
            if (GameState != GameState.InProgress)
            {
                Debug.LogError("[NetworkGameManager] Cannot pause game: game not in progress");
                return;
            }
            
            // Only the host can pause the game
            if (!_networkManager.LocalPlayer.IsHost)
            {
                Debug.LogError("[NetworkGameManager] Cannot pause game: not the host");
                return;
            }
            
            Debug.Log("[NetworkGameManager] Pausing game");
            
            // Set pause flag
            _isPaused = true;
            
            // Set game state to paused
            SetGameState(GameState.Paused);
            
            // Notify other players
            SendGameStateMessage(GameState.Paused);
        }
        
        /// <summary>
        /// Resume the game.
        /// </summary>
        public void ResumeGame()
        {
            if (!_isInitialized)
            {
                Debug.LogError("[NetworkGameManager] Cannot resume game: manager not initialized");
                return;
            }
            
            if (GameState != GameState.Paused)
            {
                Debug.LogError("[NetworkGameManager] Cannot resume game: game not paused");
                return;
            }
            
            // Only the host can resume the game
            if (!_networkManager.LocalPlayer.IsHost)
            {
                Debug.LogError("[NetworkGameManager] Cannot resume game: not the host");
                return;
            }
            
            Debug.Log("[NetworkGameManager] Resuming game");
            
            // Clear pause flag
            _isPaused = false;
            
            // Set game state to in progress
            SetGameState(GameState.InProgress);
            
            // Notify other players
            SendGameStateMessage(GameState.InProgress);
        }
        
        /// <summary>
        /// End the game.
        /// </summary>
        public void EndGame()
        {
            if (!_isInitialized)
            {
                Debug.LogError("[NetworkGameManager] Cannot end game: manager not initialized");
                return;
            }
            
            if (GameState != GameState.InProgress && GameState != GameState.Paused)
            {
                Debug.LogError("[NetworkGameManager] Cannot end game: game not in progress or paused");
                return;
            }
            
            // Only the host can end the game
            if (!_networkManager.LocalPlayer.IsHost)
            {
                Debug.LogError("[NetworkGameManager] Cannot end game: not the host");
                return;
            }
            
            Debug.Log("[NetworkGameManager] Ending game");
            
            // Determine the winning team
            DetermineWinner();
            
            // Set game state to ended
            SetGameState(GameState.Ended);
            
            // Notify other players
            SendGameEndedMessage(WinningTeam);
            
            // Trigger game ended event
            OnGameEnded?.Invoke(WinningTeam);
        }
        
        /// <summary>
        /// Add points to a team's score.
        /// </summary>
        /// <param name="teamId">The team ID</param>
        /// <param name="points">The points to add</param>
        public void AddTeamPoints(int teamId, int points)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[NetworkGameManager] Cannot add team points: manager not initialized");
                return;
            }
            
            if (GameState != GameState.InProgress)
            {
                Debug.LogError("[NetworkGameManager] Cannot add team points: game not in progress");
                return;
            }
            
            if (teamId < 0 || teamId >= TeamScores.Length)
            {
                Debug.LogError($"[NetworkGameManager] Cannot add team points: invalid team ID ({teamId})");
                return;
            }
            
            Debug.Log($"[NetworkGameManager] Adding {points} points to team {teamId}");
            
            // Add points to the team's score
            TeamScores[teamId] += points;
            
            // Notify other players
            SendTeamScoreMessage(teamId, TeamScores[teamId]);
            
            // Trigger team score changed event
            OnTeamScoreChanged?.Invoke(teamId, TeamScores[teamId]);
            
            // Check if the team has reached the target score
            if (TeamScores[teamId] >= _targetScore)
            {
                EndGame();
            }
        }
        
        /// <summary>
        /// Set the game state to in progress.
        /// </summary>
        private void SetGameInProgress()
        {
            // Set game state to in progress
            SetGameState(GameState.InProgress);
            
            // Notify other players
            SendGameStateMessage(GameState.InProgress);
        }
        
        /// <summary>
        /// Determine the winning team.
        /// </summary>
        private void DetermineWinner()
        {
            // Find the team with the highest score
            int highestScore = -1;
            int winningTeam = -1;
            
            for (int i = 0; i < TeamScores.Length; i++)
            {
                if (TeamScores[i] > highestScore)
                {
                    highestScore = TeamScores[i];
                    winningTeam = i;
                }
            }
            
            // Check for a tie
            bool isTie = false;
            for (int i = 0; i < TeamScores.Length; i++)
            {
                if (i != winningTeam && TeamScores[i] == highestScore)
                {
                    isTie = true;
                    break;
                }
            }
            
            // Set the winning team
            WinningTeam = isTie ? -1 : winningTeam;
        }
        
        /// <summary>
        /// Handle connection state changed event.
        /// </summary>
        /// <param name="state">The new connection state</param>
        private void HandleConnectionStateChanged(NetworkConnectionState state)
        {
            Debug.Log($"[NetworkGameManager] Connection state changed to {state}");
            
            if (state == NetworkConnectionState.Disconnected || state == NetworkConnectionState.Failed)
            {
                // Reset game state
                SetGameState(GameState.Inactive);
            }
        }
        
        /// <summary>
        /// Handle lobby state changed event.
        /// </summary>
        /// <param name="state">The new lobby state</param>
        private void HandleLobbyStateChanged(LobbyState state)
        {
            Debug.Log($"[NetworkGameManager] Lobby state changed to {state}");
            
            if (state == LobbyState.InGame)
            {
                // Start the game if we're the host
                if (_networkManager.LocalPlayer.IsHost)
                {
                    StartGame();
                }
            }
            else if (state == LobbyState.Inactive)
            {
                // Reset game state
                SetGameState(GameState.Inactive);
            }
        }
        
        /// <summary>
        /// Register message handlers.
        /// </summary>
        private void RegisterMessageHandlers()
        {
            if (_networkManager == null)
            {
                return;
            }
            
            // Register message handlers
            _networkManager.RegisterMessageHandler(NetworkMessageType.GameState, HandleGameStateMessage);
            _networkManager.RegisterMessageHandler(NetworkMessageType.TeamScore, HandleTeamScoreMessage);
            _networkManager.RegisterMessageHandler(NetworkMessageType.GameEnded, HandleGameEndedMessage);
        }
        
        /// <summary>
        /// Unregister message handlers.
        /// </summary>
        private void UnregisterMessageHandlers()
        {
            if (_networkManager == null)
            {
                return;
            }
            
            // Unregister message handlers
            _networkManager.UnregisterMessageHandler(NetworkMessageType.GameState);
            _networkManager.UnregisterMessageHandler(NetworkMessageType.TeamScore);
            _networkManager.UnregisterMessageHandler(NetworkMessageType.GameEnded);
        }
        
        /// <summary>
        /// Handle game state message.
        /// </summary>
        /// <param name="message">The network message</param>
        private void HandleGameStateMessage(NetworkMessage message)
        {
            // Extract the game state from the message
            GameState gameState = (GameState)message.Data[0];
            
            Debug.Log($"[NetworkGameManager] Received game state message: {gameState}");
            
            // Set the game state
            SetGameState(gameState);
            
            // Update pause flag
            _isPaused = (gameState == GameState.Paused);
        }
        
        /// <summary>
        /// Handle team score message.
        /// </summary>
        /// <param name="message">The network message</param>
        private void HandleTeamScoreMessage(NetworkMessage message)
        {
            // Extract the team ID and score from the message
            int teamId = message.Data[0];
            int score = BitConverter.ToInt32(message.Data, 1);
            
            Debug.Log($"[NetworkGameManager] Received team score message: Team {teamId}, Score {score}");
            
            // Update the team score
            if (teamId >= 0 && teamId < TeamScores.Length)
            {
                TeamScores[teamId] = score;
                OnTeamScoreChanged?.Invoke(teamId, score);
            }
        }
        
        /// <summary>
        /// Handle game ended message.
        /// </summary>
        /// <param name="message">The network message</param>
        private void HandleGameEndedMessage(NetworkMessage message)
        {
            // Extract the winning team from the message
            int winningTeam = message.Data[0];
            
            Debug.Log($"[NetworkGameManager] Received game ended message: Winning Team {winningTeam}");
            
            // Set the winning team
            WinningTeam = winningTeam;
            
            // Set the game state to ended
            SetGameState(GameState.Ended);
            
            // Trigger game ended event
            OnGameEnded?.Invoke(WinningTeam);
        }
        
        /// <summary>
        /// Send a game state message.
        /// </summary>
        /// <param name="gameState">The game state to send</param>
        private void SendGameStateMessage(GameState gameState)
        {
            if (_networkManager == null)
            {
                return;
            }
            
            // Create the message data
            byte[] data = new byte[1];
            data[0] = (byte)gameState;
            
            // Send the message
            _networkManager.SendToAll(NetworkMessageType.GameState, data);
        }
        
        /// <summary>
        /// Send a team score message.
        /// </summary>
        /// <param name="teamId">The team ID</param>
        /// <param name="score">The team score</param>
        private void SendTeamScoreMessage(int teamId, int score)
        {
            if (_networkManager == null)
            {
                return;
            }
            
            // Create the message data
            byte[] data = new byte[5];
            data[0] = (byte)teamId;
            BitConverter.GetBytes(score).CopyTo(data, 1);
            
            // Send the message
            _networkManager.SendToAll(NetworkMessageType.TeamScore, data);
        }
        
        /// <summary>
        /// Send a game ended message.
        /// </summary>
        /// <param name="winningTeam">The winning team</param>
        private void SendGameEndedMessage(int winningTeam)
        {
            if (_networkManager == null)
            {
                return;
            }
            
            // Create the message data
            byte[] data = new byte[1];
            data[0] = (byte)winningTeam;
            
            // Send the message
            _networkManager.SendToAll(NetworkMessageType.GameEnded, data);
        }
        
        /// <summary>
        /// Set the game state and trigger the event.
        /// </summary>
        /// <param name="state">The new game state</param>
        private void SetGameState(GameState state)
        {
            if (GameState != state)
            {
                GameState = state;
                OnGameStateChanged?.Invoke(GameState);
            }
        }
    }
    
    /// <summary>
    /// Enum for game states.
    /// </summary>
    public enum GameState
    {
        Inactive,
        Starting,
        InProgress,
        Paused,
        Ended
    }
}
