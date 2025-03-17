using System;
using System.Collections.Generic;
using System.Timers;
using RecipeRage.Modules.Lobbies.Data;
using RecipeRage.Modules.Lobbies.Interfaces;
using RecipeRage.Modules.Logging;
using UnityEngine;

namespace RecipeRage.Modules.Lobbies.Core
{
    /// <summary>
    /// Provides matchmaking services using the lobby system
    /// </summary>
    public class MatchmakingService : IMatchmakingService
    {

        #region Constructor

        /// <summary>
        /// Create a new MatchmakingService using the specified lobby service
        /// </summary>
        /// <param name="lobbyService"> Lobby service to use for matchmaking </param>
        public MatchmakingService(ILobbyService lobbyService)
        {
            _lobbyService = lobbyService ?? throw new ArgumentNullException(nameof(lobbyService));

            CurrentStatus = new MatchmakingStatus
            {
                State = MatchmakingState.Inactive,
                PlayersFound = 0,
                PlayersNeeded = 0,
                EstimatedTimeRemainingSeconds = -1,
                StartTime = DateTime.MinValue
            };

            // Set up status update timer
            _statusUpdateTimer = new Timer(5000); // 5 seconds
            _statusUpdateTimer.Elapsed += (sender, e) => UpdateMatchmakingStatus();
            _statusUpdateTimer.AutoReset = true;

            LogHelper.Info("MatchmakingService", "Created MatchmakingService");
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the matchmaking service
        /// </summary>
        /// <param name="onComplete"> Callback invoked when initialization is complete </param>
        public void Initialize(Action<bool> onComplete = null)
        {
            if (IsInitialized)
            {
                LogHelper.Warning("MatchmakingService", "MatchmakingService is already initialized");
                onComplete?.Invoke(true);
                return;
            }

            LogHelper.Info("MatchmakingService", "Initializing MatchmakingService");

            if (_lobbyService == null)
            {
                LastError = "LobbyService is null";
                LogHelper.Error("MatchmakingService", LastError);
                onComplete?.Invoke(false);
                return;
            }

            if (!_lobbyService.IsInitialized)
            {
                LastError = "LobbyService is not initialized";
                LogHelper.Error("MatchmakingService", LastError);
                onComplete?.Invoke(false);
                return;
            }

            // Subscribe to lobby events
            _lobbyService.OnLobbyCreated += HandleLobbyCreated;
            _lobbyService.OnLobbyJoined += HandleLobbyJoined;
            _lobbyService.OnLobbyLeft += HandleLobbyLeft;

            // Get the active provider from the lobby service
            var availableProvider = _lobbyService.GetProvider("EOSLobby"); // Default to EOS provider
            if (availableProvider == null)
            {
                LastError = "No available lobby provider";
                LogHelper.Error("MatchmakingService", LastError);
                onComplete?.Invoke(false);
                return;
            }

            _activeProvider = availableProvider;
            IsInitialized = true;
            LogHelper.Info("MatchmakingService", $"MatchmakingService initialized with provider: {_activeProvider.ProviderName}");

            onComplete?.Invoke(true);
        }

        #endregion
        #region Events

        /// <summary>
        /// Event triggered when matchmaking starts
        /// </summary>
        public event Action<MatchmakingOptions> OnMatchmakingStarted;

        /// <summary>
        /// Event triggered when matchmaking is canceled
        /// </summary>
        public event Action OnMatchmakingCanceled;

        /// <summary>
        /// Event triggered when matchmaking completes successfully
        /// </summary>
        public event Action<LobbyInfo> OnMatchmakingComplete;

        /// <summary>
        /// Event triggered when matchmaking fails
        /// </summary>
        public event Action<string> OnMatchmakingFailed;

        /// <summary>
        /// Event triggered when matchmaking status updates (e.g., players found)
        /// </summary>
        public event Action<MatchmakingStatus> OnMatchmakingStatusUpdated;

        #endregion

        #region Properties

        /// <summary>
        /// Gets whether matchmaking is currently in progress
        /// </summary>
        public bool IsMatchmaking { get; private set; }

        /// <summary>
        /// Gets the current matchmaking options, or null if not matchmaking
        /// </summary>
        public MatchmakingOptions CurrentMatchmakingOptions { get; private set; }

        /// <summary>
        /// Gets the current matchmaking status
        /// </summary>
        public MatchmakingStatus CurrentStatus { get; private set; }

        /// <summary>
        /// Gets whether the service is initialized
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Gets the last error message from the service
        /// </summary>
        public string LastError { get; private set; }

        #endregion

        #region Private Fields

        private readonly ILobbyService _lobbyService;
        private bool _isCancelingMatchmaking;
        private string _matchmakingTicketId;
        private ILobbyProvider _activeProvider;
        private readonly Timer _statusUpdateTimer;
        private readonly object _lockObject = new object();

        #endregion

        #region Matchmaking Methods

        /// <summary>
        /// Start matchmaking with the given options
        /// </summary>
        /// <param name="options"> Matchmaking options to use </param>
        /// <param name="onComplete"> Callback invoked when matchmaking starts </param>
        public void StartMatchmaking(MatchmakingOptions options, Action<bool> onComplete = null)
        {
            if (!CheckInitialized("StartMatchmaking", onComplete))
            {
                return;
            }

            if (options == null)
            {
                LastError = "Matchmaking options cannot be null";
                LogHelper.Error("MatchmakingService", LastError);
                onComplete?.Invoke(false);
                return;
            }

            if (IsMatchmaking)
            {
                LastError = "Matchmaking is already in progress";
                LogHelper.Warning("MatchmakingService", LastError);
                onComplete?.Invoke(false);
                return;
            }

            if (_lobbyService.CurrentLobby != null)
            {
                LastError = "Already in a lobby. Leave the current lobby before starting matchmaking.";
                LogHelper.Warning("MatchmakingService", LastError);
                onComplete?.Invoke(false);
                return;
            }

            LogHelper.Info("MatchmakingService", $"Starting matchmaking with options: minPlayers={options.MinPlayers}, maxPlayers={options.MaxPlayers}, gameMode={options.GameMode}");

            IsMatchmaking = true;
            CurrentMatchmakingOptions = options;

            // Update status
            CurrentStatus = new MatchmakingStatus
            {
                State = MatchmakingState.Initializing,
                PlayersFound = 0,
                PlayersNeeded = options.MinPlayers,
                EstimatedTimeRemainingSeconds = options.TimeoutSeconds,
                StartTime = DateTime.UtcNow,
                CurrentRegion = options.PreferredRegions.Count > 0 ? options.PreferredRegions[0] : "unknown"
            };

            OnMatchmakingStatusUpdated?.Invoke(CurrentStatus);

            // Start the status update timer
            _statusUpdateTimer.Start();

            // Start matchmaking with the provider
            _activeProvider.StartMatchmaking(options, (success, ticketId) =>
            {
                if (success)
                {
                    LogHelper.Info("MatchmakingService", $"Matchmaking started successfully with ticket ID: {ticketId}");
                    _matchmakingTicketId = ticketId;

                    // Update status
                    CurrentStatus.State = MatchmakingState.Searching;
                    CurrentStatus.TicketId = ticketId;
                    OnMatchmakingStatusUpdated?.Invoke(CurrentStatus);

                    // Trigger event
                    OnMatchmakingStarted?.Invoke(options);
                }
                else
                {
                    LogHelper.Error("MatchmakingService", $"Failed to start matchmaking: {_activeProvider.LastError}");
                    LastError = $"Failed to start matchmaking: {_activeProvider.LastError}";
                    IsMatchmaking = false;

                    // Update status
                    CurrentStatus.State = MatchmakingState.Failed;
                    CurrentStatus.ErrorMessage = LastError;
                    OnMatchmakingStatusUpdated?.Invoke(CurrentStatus);

                    // Stop the status update timer
                    _statusUpdateTimer.Stop();

                    // Trigger event
                    OnMatchmakingFailed?.Invoke(LastError);
                }

                onComplete?.Invoke(success);
            });
        }

        /// <summary>
        /// Cancel the current matchmaking operation
        /// </summary>
        /// <param name="onComplete"> Callback invoked when cancellation is complete </param>
        public void CancelMatchmaking(Action<bool> onComplete = null)
        {
            if (!CheckInitialized("CancelMatchmaking", onComplete))
            {
                return;
            }

            if (!IsMatchmaking)
            {
                LastError = "No matchmaking in progress to cancel";
                LogHelper.Warning("MatchmakingService", LastError);
                onComplete?.Invoke(false);
                return;
            }

            if (_isCancelingMatchmaking)
            {
                LastError = "Already canceling matchmaking";
                LogHelper.Warning("MatchmakingService", LastError);
                onComplete?.Invoke(false);
                return;
            }

            if (string.IsNullOrEmpty(_matchmakingTicketId))
            {
                LastError = "No matchmaking ticket to cancel";
                LogHelper.Warning("MatchmakingService", LastError);

                // Clean up anyway
                CleanupMatchmaking();
                onComplete?.Invoke(true);
                return;
            }

            LogHelper.Info("MatchmakingService", $"Canceling matchmaking with ticket ID: {_matchmakingTicketId}");
            _isCancelingMatchmaking = true;

            // Update status
            CurrentStatus.State = MatchmakingState.Canceled;
            OnMatchmakingStatusUpdated?.Invoke(CurrentStatus);

            // Cancel matchmaking with the provider
            _activeProvider.CancelMatchmaking(_matchmakingTicketId, success =>
            {
                _isCancelingMatchmaking = false;

                if (success)
                {
                    LogHelper.Info("MatchmakingService", "Matchmaking canceled successfully");

                    // Clean up
                    CleanupMatchmaking();

                    // Trigger event
                    OnMatchmakingCanceled?.Invoke();
                }
                else
                {
                    LogHelper.Warning("MatchmakingService", $"Failed to cancel matchmaking: {_activeProvider.LastError}");
                    LastError = $"Failed to cancel matchmaking: {_activeProvider.LastError}";

                    // Force cleanup anyway
                    CleanupMatchmaking();

                    // Trigger event anyway
                    OnMatchmakingCanceled?.Invoke();
                }

                onComplete?.Invoke(true); // Return true even if provider failed, as we've cleaned up locally
            });
        }

        /// <summary>
        /// Set player attributes for matchmaking
        /// </summary>
        /// <param name="attributes"> The attributes to set </param>
        /// <param name="onComplete"> Callback invoked when attributes are set </param>
        public void SetPlayerAttributes(Dictionary<string, string> attributes, Action<bool> onComplete = null)
        {
            if (!CheckInitialized("SetPlayerAttributes", onComplete))
            {
                return;
            }

            // This is a no-op as we don't support setting matchmaking attributes separately from lobby attributes
            LogHelper.Info("MatchmakingService", "SetPlayerAttributes is not supported directly. Use lobby attributes instead.");
            onComplete?.Invoke(true);
        }

        /// <summary>
        /// Set the skill rating for the player
        /// </summary>
        /// <param name="skillRating"> The skill rating to set </param>
        /// <param name="onComplete"> Callback invoked when skill rating is set </param>
        public void SetSkillRating(float skillRating, Action<bool> onComplete = null)
        {
            if (!CheckInitialized("SetSkillRating", onComplete))
            {
                return;
            }

            LogHelper.Info("MatchmakingService", $"Setting skill rating: {skillRating}");

            // Store in player prefs for now
            PlayerPrefs.SetFloat("MatchmakingSkillRating", skillRating);
            PlayerPrefs.Save();

            onComplete?.Invoke(true);
        }

        /// <summary>
        /// Get the estimated wait time for matchmaking
        /// </summary>
        /// <param name="options"> Matchmaking options to estimate for </param>
        /// <param name="onComplete"> Callback invoked with the estimated wait time in seconds </param>
        public void GetEstimatedWaitTime(MatchmakingOptions options, Action<bool, float> onComplete)
        {
            if (!CheckInitialized("GetEstimatedWaitTime", success => onComplete?.Invoke(success, 0)))
            {
                return;
            }

            if (options == null)
            {
                LastError = "Matchmaking options cannot be null";
                LogHelper.Error("MatchmakingService", LastError);
                onComplete?.Invoke(false, 0);
                return;
            }

            // For now, provide a reasonable default estimate based on the game mode and time of day
            float estimateSeconds = 30.0f; // Default 30 seconds

            // Adjust based on game mode
            if (options.GameMode == "competitive")
            {
                estimateSeconds = 60.0f; // Competitive takes longer
            }
            else if (options.GameMode == "casual")
            {
                estimateSeconds = 20.0f; // Casual is quicker
            }

            // Adjust based on min players needed
            estimateSeconds += (options.MinPlayers - 2) * 10.0f; // Add 10 seconds per additional player over 2

            LogHelper.Info("MatchmakingService", $"Estimated wait time for {options.GameMode} mode: {estimateSeconds} seconds");
            onComplete?.Invoke(true, estimateSeconds);
        }

        /// <summary>
        /// Set matchmaking region preferences
        /// </summary>
        /// <param name="regionPreferences"> Ordered list of region preferences </param>
        /// <param name="onComplete"> Callback invoked when preferences are set </param>
        public void SetRegionPreferences(List<string> regionPreferences, Action<bool> onComplete = null)
        {
            if (!CheckInitialized("SetRegionPreferences", onComplete))
            {
                return;
            }

            if (regionPreferences == null || regionPreferences.Count == 0)
            {
                LastError = "Region preferences cannot be null or empty";
                LogHelper.Warning("MatchmakingService", LastError);
                onComplete?.Invoke(false);
                return;
            }

            LogHelper.Info("MatchmakingService", $"Setting region preferences: {string.Join(", ", regionPreferences)}");

            // Store in player prefs as comma-separated string
            PlayerPrefs.SetString("MatchmakingRegionPreferences", string.Join(",", regionPreferences));
            PlayerPrefs.Save();

            // If we're currently matchmaking, update the current options
            if (IsMatchmaking && CurrentMatchmakingOptions != null)
            {
                CurrentMatchmakingOptions.PreferredRegions.Clear();
                CurrentMatchmakingOptions.PreferredRegions.AddRange(regionPreferences);

                // Update status
                if (regionPreferences.Count > 0)
                {
                    CurrentStatus.CurrentRegion = regionPreferences[0];
                    OnMatchmakingStatusUpdated?.Invoke(CurrentStatus);
                }
            }

            onComplete?.Invoke(true);
        }

        #endregion

        #region Event Handlers

        private void HandleLobbyCreated(LobbyInfo lobby)
        {
            // Check if this is a matchmaking lobby
            if (IsMatchmaking && lobby.IsMatchmakingLobby)
            {
                LogHelper.Info("MatchmakingService", $"Matchmaking lobby created: {lobby.Name} ({lobby.LobbyId})");

                // Update status
                CurrentStatus.State = MatchmakingState.MatchFound;
                OnMatchmakingStatusUpdated?.Invoke(CurrentStatus);

                // Trigger event
                OnMatchmakingComplete?.Invoke(lobby);

                // Clean up
                CleanupMatchmaking();
            }
        }

        private void HandleLobbyJoined(LobbyInfo lobby)
        {
            // Check if this is a matchmaking lobby
            if (IsMatchmaking && lobby.IsMatchmakingLobby)
            {
                LogHelper.Info("MatchmakingService", $"Matchmaking lobby joined: {lobby.Name} ({lobby.LobbyId})");

                // Update status
                CurrentStatus.State = MatchmakingState.Completed;
                OnMatchmakingStatusUpdated?.Invoke(CurrentStatus);

                // Trigger event
                OnMatchmakingComplete?.Invoke(lobby);

                // Clean up
                CleanupMatchmaking();
            }
        }

        private void HandleLobbyLeft(string lobbyId)
        {
            // If we're matchmaking and leave a lobby, it's probably due to an error
            if (IsMatchmaking)
            {
                LogHelper.Warning("MatchmakingService", $"Left lobby during matchmaking: {lobbyId}");

                LastError = "Left lobby during matchmaking";

                // Update status
                CurrentStatus.State = MatchmakingState.Failed;
                CurrentStatus.ErrorMessage = LastError;
                OnMatchmakingStatusUpdated?.Invoke(CurrentStatus);

                // Trigger event
                OnMatchmakingFailed?.Invoke(LastError);

                // Clean up
                CleanupMatchmaking();
            }
        }

        #endregion

        #region Helper Methods

        private void UpdateMatchmakingStatus()
        {
            if (!IsMatchmaking || CurrentStatus.State != MatchmakingState.Searching)
            {
                return;
            }

            // Calculate elapsed time
            var elapsed = DateTime.UtcNow - CurrentStatus.StartTime;

            // Check for timeout
            if (CurrentMatchmakingOptions != null && elapsed.TotalSeconds >= CurrentMatchmakingOptions.TimeoutSeconds)
            {
                LogHelper.Warning("MatchmakingService", "Matchmaking timed out");

                LastError = "Matchmaking timed out";

                // Update status
                CurrentStatus.State = MatchmakingState.TimedOut;
                CurrentStatus.ErrorMessage = LastError;
                OnMatchmakingStatusUpdated?.Invoke(CurrentStatus);

                // Trigger event
                OnMatchmakingFailed?.Invoke(LastError);

                // Cancel matchmaking
                CancelMatchmaking();
                return;
            }

            // Update estimated time remaining
            if (CurrentMatchmakingOptions != null)
            {
                CurrentStatus.EstimatedTimeRemainingSeconds = Math.Max(0, (float)(CurrentMatchmakingOptions.TimeoutSeconds - elapsed.TotalSeconds));
            }

            // Simulate players found increasing over time (normally this would come from the provider)
            if (CurrentMatchmakingOptions != null && CurrentStatus.PlayersFound < CurrentMatchmakingOptions.MinPlayers)
            {
                // Increment players found proportional to elapsed time
                float progressFactor = (float)(elapsed.TotalSeconds / CurrentMatchmakingOptions.TimeoutSeconds);
                int expectedPlayersFound = Math.Min(CurrentMatchmakingOptions.MinPlayers, (int)(CurrentMatchmakingOptions.MinPlayers * progressFactor) + 1);

                if (CurrentStatus.PlayersFound < expectedPlayersFound)
                {
                    CurrentStatus.PlayersFound = expectedPlayersFound;
                    LogHelper.Debug("MatchmakingService", $"Players found: {CurrentStatus.PlayersFound}/{CurrentStatus.PlayersNeeded}");
                    OnMatchmakingStatusUpdated?.Invoke(CurrentStatus);
                }
            }
        }

        private void CleanupMatchmaking()
        {
            IsMatchmaking = false;
            _isCancelingMatchmaking = false;
            _matchmakingTicketId = null;
            CurrentMatchmakingOptions = null;

            // Stop the status update timer
            _statusUpdateTimer.Stop();

            // Reset status
            CurrentStatus = new MatchmakingStatus
            {
                State = MatchmakingState.Inactive,
                PlayersFound = 0,
                PlayersNeeded = 0,
                EstimatedTimeRemainingSeconds = -1,
                StartTime = DateTime.MinValue
            };
        }

        private bool CheckInitialized(string methodName, Action<bool> callback = null)
        {
            if (!IsInitialized)
            {
                LastError = "MatchmakingService is not initialized";
                LogHelper.Error("MatchmakingService", $"{methodName} failed: {LastError}");
                callback?.Invoke(false);
                return false;
            }

            if (_activeProvider == null || !_activeProvider.IsAvailable)
            {
                LastError = "No available lobby provider";
                LogHelper.Error("MatchmakingService", $"{methodName} failed: {LastError}");
                callback?.Invoke(false);
                return false;
            }

            return true;
        }

        private bool CheckInitialized<T1, T2>(string methodName, Action<bool, T2> callback = null)
        {
            if (!IsInitialized)
            {
                LastError = "MatchmakingService is not initialized";
                LogHelper.Error("MatchmakingService", $"{methodName} failed: {LastError}");
                callback?.Invoke(false, default);
                return false;
            }

            if (_activeProvider == null || !_activeProvider.IsAvailable)
            {
                LastError = "No available lobby provider";
                LogHelper.Error("MatchmakingService", $"{methodName} failed: {LastError}");
                callback?.Invoke(false, default);
                return false;
            }

            return true;
        }

        #endregion
    }
}