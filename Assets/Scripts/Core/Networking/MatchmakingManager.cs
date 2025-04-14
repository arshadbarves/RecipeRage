using System;
using System.Collections.Generic;
using RecipeRage.Core.Patterns;
using RecipeRage.Core.GameModes;
using RecipeRage.Core.Networking.Common;
using UnityEngine;

namespace RecipeRage.Core.Networking
{
    /// <summary>
    /// Manages matchmaking for RecipeRage.
    /// </summary>
    public class MatchmakingManager : MonoBehaviourSingleton<MatchmakingManager>
    {
        [Header("Matchmaking Settings")]
        [SerializeField] private float _matchmakingTimeout = 60f;
        [SerializeField] private float _matchFoundCountdown = 10f;
        [SerializeField] private int _maxMatchmakingAttempts = 3;

        [Header("References")]
        [SerializeField] private NetworkManager _networkManager;
        [SerializeField] private NetworkLobbyManager _lobbyManager;
        [SerializeField] private GameModeManager _gameModeManager;

        /// <summary>
        /// Event triggered when the matchmaking state changes.
        /// </summary>
        public event Action<MatchmakingState> OnMatchmakingStateChanged;

        /// <summary>
        /// Event triggered when a match is found.
        /// </summary>
        public event Action<NetworkSessionInfo> OnMatchFound;

        /// <summary>
        /// Event triggered when the match found countdown changes.
        /// </summary>
        public event Action<float> OnMatchFoundCountdownChanged;

        /// <summary>
        /// Event triggered when matchmaking fails.
        /// </summary>
        public event Action<string> OnMatchmakingFailed;

        /// <summary>
        /// The current matchmaking state.
        /// </summary>
        public MatchmakingState MatchmakingState { get; private set; }

        /// <summary>
        /// The current match found countdown.
        /// </summary>
        public float MatchFoundCountdown { get; private set; }

        /// <summary>
        /// The current matchmaking session info.
        /// </summary>
        public NetworkSessionInfo CurrentMatchInfo { get; private set; }

        /// <summary>
        /// The current matchmaking game mode.
        /// </summary>
        public GameMode MatchmakingGameMode { get; private set; }

        /// <summary>
        /// The current matchmaking map.
        /// </summary>
        public string MatchmakingMap { get; private set; }

        /// <summary>
        /// Flag to track if the matchmaking manager is initialized.
        /// </summary>
        private bool _isInitialized;

        /// <summary>
        /// Flag to track if the match found countdown is active.
        /// </summary>
        private bool _isCountingDown;

        /// <summary>
        /// The current matchmaking timer.
        /// </summary>
        private float _matchmakingTimer;

        /// <summary>
        /// The current matchmaking attempt.
        /// </summary>
        private int _matchmakingAttempt;

        /// <summary>
        /// The list of available sessions for matchmaking.
        /// </summary>
        private List<NetworkSessionInfo> _availableSessions = new List<NetworkSessionInfo>();

        /// <summary>
        /// The player's matchmaking preferences.
        /// </summary>
        private MatchmakingPreferences _preferences = new MatchmakingPreferences();

        /// <summary>
        /// Initialize the matchmaking manager.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // Find the network manager if not set
            if (_networkManager == null)
            {
                _networkManager = FindFirstObjectByType<NetworkManager>();
            }

            // Find the lobby manager if not set
            if (_lobbyManager == null)
            {
                _lobbyManager = FindFirstObjectByType<NetworkLobbyManager>();
            }

            // Find the game mode manager if not set
            if (_gameModeManager == null)
            {
                _gameModeManager = FindFirstObjectByType<GameModeManager>();
            }

            // Initialize values
            MatchmakingState = MatchmakingState.Inactive;
            MatchFoundCountdown = _matchFoundCountdown;
            _isCountingDown = false;

            // Register the matchmaking manager with the service locator
            ServiceLocator.Instance.Register<MatchmakingManager>(this);

            Debug.Log("[MatchmakingManager] Matchmaking manager initialized");
        }

        /// <summary>
        /// Subscribe to network manager and lobby manager events.
        /// </summary>
        private void Start()
        {
            if (_networkManager != null && _lobbyManager != null && _gameModeManager != null)
            {
                // Subscribe to network manager events
                _networkManager.OnConnectionStateChanged += HandleConnectionStateChanged;
                _networkManager.OnSessionJoined += HandleSessionJoined;

                // Subscribe to lobby manager events
                _lobbyManager.OnLobbyStateChanged += HandleLobbyStateChanged;

                _isInitialized = true;
            }
            else
            {
                Debug.LogError("[MatchmakingManager] Network manager, lobby manager, or game mode manager not found");
            }
        }

        /// <summary>
        /// Update the matchmaking manager.
        /// </summary>
        private void Update()
        {
            // Update matchmaking timer
            if (MatchmakingState == MatchmakingState.Searching)
            {
                _matchmakingTimer -= Time.deltaTime;

                if (_matchmakingTimer <= 0f)
                {
                    // Timeout reached, try again or fail
                    if (_matchmakingAttempt >= _maxMatchmakingAttempts)
                    {
                        // Max attempts reached, fail matchmaking
                        FailMatchmaking("Matchmaking timeout reached");
                    }
                    else
                    {
                        // Try again
                        _matchmakingAttempt++;
                        _matchmakingTimer = _matchmakingTimeout;

                        Debug.Log($"[MatchmakingManager] Matchmaking attempt {_matchmakingAttempt}/{_maxMatchmakingAttempts}");

                        // Search for sessions again
                        SearchForSessions();
                    }
                }
            }

            // Update match found countdown
            if (_isCountingDown && MatchmakingState == MatchmakingState.MatchFound)
            {
                MatchFoundCountdown -= Time.deltaTime;
                OnMatchFoundCountdownChanged?.Invoke(MatchFoundCountdown);

                if (MatchFoundCountdown <= 0f)
                {
                    // Countdown finished, join the match
                    JoinMatch();
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
                _networkManager.OnSessionJoined -= HandleSessionJoined;
            }

            if (_lobbyManager != null)
            {
                // Unsubscribe from lobby manager events
                _lobbyManager.OnLobbyStateChanged -= HandleLobbyStateChanged;
            }

            Debug.Log("[MatchmakingManager] Matchmaking manager destroyed");
        }

        /// <summary>
        /// Start matchmaking with the specified preferences.
        /// </summary>
        /// <param name="preferences">Matchmaking preferences</param>
        public void StartMatchmaking(MatchmakingPreferences preferences)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[MatchmakingManager] Cannot start matchmaking: manager not initialized");
                return;
            }

            if (MatchmakingState != MatchmakingState.Inactive)
            {
                Debug.LogError("[MatchmakingManager] Cannot start matchmaking: already matchmaking");
                return;
            }

            Debug.Log("[MatchmakingManager] Starting matchmaking");

            // Store preferences
            _preferences = preferences ?? new MatchmakingPreferences();

            // Set the game mode
            if (!string.IsNullOrEmpty(_preferences.GameModeId))
            {
                MatchmakingGameMode = _gameModeManager.GetGameMode(_preferences.GameModeId);
            }
            else
            {
                MatchmakingGameMode = _gameModeManager.SelectedGameMode;
            }

            // Set the map
            if (!string.IsNullOrEmpty(_preferences.MapName) && MatchmakingGameMode != null &&
                MatchmakingGameMode.AvailableMaps.Contains(_preferences.MapName))
            {
                MatchmakingMap = _preferences.MapName;
            }
            else if (MatchmakingGameMode != null)
            {
                MatchmakingMap = MatchmakingGameMode.DefaultMap;
            }
            else
            {
                MatchmakingMap = "Kitchen";
            }

            // Reset matchmaking values
            _matchmakingTimer = _matchmakingTimeout;
            _matchmakingAttempt = 1;
            _availableSessions.Clear();

            // Set matchmaking state to searching
            SetMatchmakingState(MatchmakingState.Searching);

            // Start searching for sessions
            SearchForSessions();
        }

        /// <summary>
        /// Cancel matchmaking.
        /// </summary>
        public void CancelMatchmaking()
        {
            if (!_isInitialized)
            {
                Debug.LogError("[MatchmakingManager] Cannot cancel matchmaking: manager not initialized");
                return;
            }

            if (MatchmakingState == MatchmakingState.Inactive)
            {
                Debug.LogError("[MatchmakingManager] Cannot cancel matchmaking: not matchmaking");
                return;
            }

            Debug.Log("[MatchmakingManager] Canceling matchmaking");

            // Reset matchmaking
            ResetMatchmaking();
        }

        /// <summary>
        /// Accept the found match.
        /// </summary>
        public void AcceptMatch()
        {
            if (!_isInitialized)
            {
                Debug.LogError("[MatchmakingManager] Cannot accept match: manager not initialized");
                return;
            }

            if (MatchmakingState != MatchmakingState.MatchFound)
            {
                Debug.LogError("[MatchmakingManager] Cannot accept match: no match found");
                return;
            }

            Debug.Log("[MatchmakingManager] Accepting match");

            // Set matchmaking state to joining
            SetMatchmakingState(MatchmakingState.Joining);

            // Join the match
            JoinMatch();
        }

        /// <summary>
        /// Decline the found match.
        /// </summary>
        public void DeclineMatch()
        {
            if (!_isInitialized)
            {
                Debug.LogError("[MatchmakingManager] Cannot decline match: manager not initialized");
                return;
            }

            if (MatchmakingState != MatchmakingState.MatchFound)
            {
                Debug.LogError("[MatchmakingManager] Cannot decline match: no match found");
                return;
            }

            Debug.Log("[MatchmakingManager] Declining match");

            // Reset matchmaking
            ResetMatchmaking();
        }

        /// <summary>
        /// Search for available sessions.
        /// </summary>
        private void SearchForSessions()
        {
            if (_networkManager == null)
            {
                FailMatchmaking("Network manager not found");
                return;
            }

            Debug.Log("[MatchmakingManager] Searching for sessions");

            // Find sessions with the network manager
            _networkManager.FindSessions(sessions =>
            {
                _availableSessions.Clear();

                if (sessions == null || sessions.Count == 0)
                {
                    Debug.Log("[MatchmakingManager] No sessions found, creating a new one");

                    // No sessions found, create a new one
                    CreateNewSession();
                    return;
                }

                // Filter sessions based on preferences
                foreach (var session in sessions)
                {
                    // Check if the session matches our preferences
                    if (IsSessionMatch(session))
                    {
                        _availableSessions.Add(session);
                    }
                }

                if (_availableSessions.Count > 0)
                {
                    // Found matching sessions, select the best one
                    SelectBestSession();
                }
                else
                {
                    Debug.Log("[MatchmakingManager] No matching sessions found, creating a new one");

                    // No matching sessions found, create a new one
                    CreateNewSession();
                }
            });
        }

        /// <summary>
        /// Check if a session matches our preferences.
        /// </summary>
        /// <param name="session">The session to check</param>
        /// <returns>True if the session matches our preferences</returns>
        private bool IsSessionMatch(NetworkSessionInfo session)
        {
            // Check if the session is full
            if (session.PlayerCount >= session.MaxPlayers)
            {
                return false;
            }

            // Check if the session is private and we don't want private sessions
            if (session.IsPrivate && !_preferences.AllowPrivate)
            {
                return false;
            }

            // Check if the game mode matches
            if (!string.IsNullOrEmpty(_preferences.GameModeId) &&
                !string.Equals(session.GameMode, _preferences.GameModeId, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Check if the map matches
            if (!string.IsNullOrEmpty(_preferences.MapName) &&
                !string.Equals(session.MapName, _preferences.MapName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Select the best session from the available sessions.
        /// </summary>
        private void SelectBestSession()
        {
            if (_availableSessions.Count == 0)
            {
                FailMatchmaking("No available sessions");
                return;
            }

            // Sort sessions by player count (descending) to join the most populated session
            _availableSessions.Sort((a, b) => b.PlayerCount.CompareTo(a.PlayerCount));

            // Select the first session
            CurrentMatchInfo = _availableSessions[0];

            Debug.Log($"[MatchmakingManager] Selected session: {CurrentMatchInfo.SessionName} ({CurrentMatchInfo.SessionId})");

            // Set matchmaking state to match found
            SetMatchmakingState(MatchmakingState.MatchFound);

            // Start the countdown
            MatchFoundCountdown = _matchFoundCountdown;
            _isCountingDown = true;
            OnMatchFoundCountdownChanged?.Invoke(MatchFoundCountdown);

            // Trigger match found event
            OnMatchFound?.Invoke(CurrentMatchInfo);
        }

        /// <summary>
        /// Create a new session.
        /// </summary>
        private void CreateNewSession()
        {
            if (_lobbyManager == null)
            {
                FailMatchmaking("Lobby manager not found");
                return;
            }

            Debug.Log("[MatchmakingManager] Creating a new session");

            // Generate a session name
            string sessionName = $"{MatchmakingGameMode?.DisplayName ?? "Game"} - {DateTime.Now:yyyyMMddHHmmss}";

            // Set matchmaking state to creating
            SetMatchmakingState(MatchmakingState.Creating);

            // Create a lobby with the lobby manager
            // First, set the game mode in the GameModeManager if available
            if (MatchmakingGameMode != null && _gameModeManager != null)
            {
                _gameModeManager.SelectGameMode(MatchmakingGameMode);
            }

            // Then create the lobby
            _lobbyManager.CreateLobby(sessionName, false);
        }

        /// <summary>
        /// Join the selected match.
        /// </summary>
        private void JoinMatch()
        {
            if (_lobbyManager == null || CurrentMatchInfo == null)
            {
                FailMatchmaking("Lobby manager not found or no match selected");
                return;
            }

            Debug.Log($"[MatchmakingManager] Joining match: {CurrentMatchInfo.SessionName} ({CurrentMatchInfo.SessionId})");

            // Set matchmaking state to joining
            SetMatchmakingState(MatchmakingState.Joining);

            // Join the lobby with the lobby manager
            _lobbyManager.JoinLobby(CurrentMatchInfo.SessionId);
        }

        /// <summary>
        /// Fail matchmaking with an error message.
        /// </summary>
        /// <param name="errorMessage">The error message</param>
        private void FailMatchmaking(string errorMessage)
        {
            Debug.LogError($"[MatchmakingManager] Matchmaking failed: {errorMessage}");

            // Set matchmaking state to failed
            SetMatchmakingState(MatchmakingState.Failed);

            // Trigger matchmaking failed event
            OnMatchmakingFailed?.Invoke(errorMessage);

            // Reset matchmaking after a delay
            Invoke(nameof(ResetMatchmaking), 3f);
        }

        /// <summary>
        /// Reset matchmaking.
        /// </summary>
        private void ResetMatchmaking()
        {
            // Reset matchmaking values
            _matchmakingTimer = 0f;
            _matchmakingAttempt = 0;
            _availableSessions.Clear();
            CurrentMatchInfo = null;
            MatchmakingGameMode = null;
            MatchmakingMap = null;
            _isCountingDown = false;
            MatchFoundCountdown = _matchFoundCountdown;

            // Set matchmaking state to inactive
            SetMatchmakingState(MatchmakingState.Inactive);
        }

        /// <summary>
        /// Handle connection state changed event.
        /// </summary>
        /// <param name="state">The new connection state</param>
        private void HandleConnectionStateChanged(NetworkConnectionState state)
        {
            Debug.Log($"[MatchmakingManager] Connection state changed to {state}");

            if (state == NetworkConnectionState.Failed)
            {
                // Connection failed, fail matchmaking
                FailMatchmaking("Connection failed");
            }
            else if (state == NetworkConnectionState.Disconnected)
            {
                // Disconnected, reset matchmaking
                ResetMatchmaking();
            }
        }

        /// <summary>
        /// Handle session joined event.
        /// </summary>
        /// <param name="success">Whether the session was joined successfully</param>
        /// <param name="sessionId">The session ID</param>
        private void HandleSessionJoined(bool success, string sessionId)
        {
            if (MatchmakingState != MatchmakingState.Joining)
            {
                return;
            }

            if (success)
            {
                Debug.Log($"[MatchmakingManager] Session joined: {sessionId}");

                // Set matchmaking state to joined
                SetMatchmakingState(MatchmakingState.Joined);

                // Reset matchmaking after a delay
                Invoke(nameof(ResetMatchmaking), 3f);
            }
            else
            {
                Debug.LogError($"[MatchmakingManager] Failed to join session: {sessionId}");

                // Fail matchmaking
                FailMatchmaking($"Failed to join session: {sessionId}");
            }
        }

        /// <summary>
        /// Handle lobby state changed event.
        /// </summary>
        /// <param name="state">The new lobby state</param>
        private void HandleLobbyStateChanged(LobbyState state)
        {
            Debug.Log($"[MatchmakingManager] Lobby state changed to {state}");

            if (state == LobbyState.Inactive && MatchmakingState != MatchmakingState.Inactive)
            {
                // Lobby inactive, reset matchmaking
                ResetMatchmaking();
            }
        }

        /// <summary>
        /// Set the matchmaking state and trigger the event.
        /// </summary>
        /// <param name="state">The new matchmaking state</param>
        private void SetMatchmakingState(MatchmakingState state)
        {
            if (MatchmakingState != state)
            {
                MatchmakingState = state;
                OnMatchmakingStateChanged?.Invoke(MatchmakingState);
            }
        }
    }

    /// <summary>
    /// Enum for matchmaking states.
    /// </summary>
    public enum MatchmakingState
    {
        Inactive,
        Searching,
        MatchFound,
        Creating,
        Joining,
        Joined,
        Failed
    }

    /// <summary>
    /// Matchmaking preferences.
    /// </summary>
    [Serializable]
    public class MatchmakingPreferences
    {
        /// <summary>
        /// The preferred game mode ID.
        /// </summary>
        public string GameModeId;

        /// <summary>
        /// The preferred map name.
        /// </summary>
        public string MapName;

        /// <summary>
        /// Whether to allow private sessions.
        /// </summary>
        public bool AllowPrivate;

        /// <summary>
        /// The preferred team ID.
        /// </summary>
        public int PreferredTeam = -1;

        /// <summary>
        /// The preferred character type.
        /// </summary>
        public int PreferredCharacter = -1;
    }
}
