using System;
using System.Collections.Generic;
using RecipeRage.Modules.Lobbies.Data;

namespace RecipeRage.Modules.Lobbies.Interfaces
{
    /// <summary>
    /// Interface for matchmaking services
    /// </summary>
    public interface IMatchmakingService
    {
        #region Events

        /// <summary>
        /// Event triggered when matchmaking starts
        /// </summary>
        event Action<MatchmakingOptions> OnMatchmakingStarted;

        /// <summary>
        /// Event triggered when matchmaking is canceled
        /// </summary>
        event Action OnMatchmakingCanceled;

        /// <summary>
        /// Event triggered when matchmaking completes successfully
        /// </summary>
        event Action<LobbyInfo> OnMatchmakingComplete;

        /// <summary>
        /// Event triggered when matchmaking fails
        /// </summary>
        event Action<string> OnMatchmakingFailed;

        /// <summary>
        /// Event triggered when matchmaking status updates (e.g., players found)
        /// </summary>
        event Action<MatchmakingStatus> OnMatchmakingStatusUpdated;

        #endregion

        #region Properties

        /// <summary>
        /// Gets whether matchmaking is currently in progress
        /// </summary>
        bool IsMatchmaking { get; }

        /// <summary>
        /// Gets the current matchmaking options, or null if not matchmaking
        /// </summary>
        MatchmakingOptions CurrentMatchmakingOptions { get; }

        /// <summary>
        /// Gets the current matchmaking status
        /// </summary>
        MatchmakingStatus CurrentStatus { get; }

        /// <summary>
        /// Gets whether the service is initialized
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Gets the last error message from the service
        /// </summary>
        string LastError { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Initialize the matchmaking service
        /// </summary>
        /// <param name="onComplete">Callback invoked when initialization is complete</param>
        void Initialize(Action<bool> onComplete = null);

        /// <summary>
        /// Start matchmaking with the given options
        /// </summary>
        /// <param name="options">Matchmaking options to use</param>
        /// <param name="onComplete">Callback invoked when matchmaking starts</param>
        void StartMatchmaking(MatchmakingOptions options, Action<bool> onComplete = null);

        /// <summary>
        /// Cancel the current matchmaking operation
        /// </summary>
        /// <param name="onComplete">Callback invoked when cancellation is complete</param>
        void CancelMatchmaking(Action<bool> onComplete = null);

        /// <summary>
        /// Set player attributes for matchmaking
        /// </summary>
        /// <param name="attributes">The attributes to set</param>
        /// <param name="onComplete">Callback invoked when attributes are set</param>
        void SetPlayerAttributes(Dictionary<string, string> attributes, Action<bool> onComplete = null);

        /// <summary>
        /// Set the skill rating for the player
        /// </summary>
        /// <param name="skillRating">The skill rating to set</param>
        /// <param name="onComplete">Callback invoked when skill rating is set</param>
        void SetSkillRating(float skillRating, Action<bool> onComplete = null);

        /// <summary>
        /// Get the estimated wait time for matchmaking
        /// </summary>
        /// <param name="options">Matchmaking options to estimate for</param>
        /// <param name="onComplete">Callback invoked with the estimated wait time in seconds</param>
        void GetEstimatedWaitTime(MatchmakingOptions options, Action<bool, float> onComplete);

        /// <summary>
        /// Set matchmaking region preferences
        /// </summary>
        /// <param name="regionPreferences">Ordered list of region preferences</param>
        /// <param name="onComplete">Callback invoked when preferences are set</param>
        void SetRegionPreferences(List<string> regionPreferences, Action<bool> onComplete = null);

        #endregion
    }
    
    /// <summary>
    /// Status information for ongoing matchmaking
    /// </summary>
    public class MatchmakingStatus
    {
        /// <summary>
        /// Current status of matchmaking
        /// </summary>
        public MatchmakingState State { get; set; }
        
        /// <summary>
        /// Number of players found so far
        /// </summary>
        public int PlayersFound { get; set; }
        
        /// <summary>
        /// Total number of players needed
        /// </summary>
        public int PlayersNeeded { get; set; }
        
        /// <summary>
        /// Estimated time remaining in seconds, or -1 if unknown
        /// </summary>
        public float EstimatedTimeRemainingSeconds { get; set; }
        
        /// <summary>
        /// Current region being searched
        /// </summary>
        public string CurrentRegion { get; set; }
        
        /// <summary>
        /// Ticket ID for the matchmaking request
        /// </summary>
        public string TicketId { get; set; }
        
        /// <summary>
        /// Time when matchmaking started
        /// </summary>
        public DateTime StartTime { get; set; }
        
        /// <summary>
        /// Error message if there was an error
        /// </summary>
        public string ErrorMessage { get; set; }
    }
    
    /// <summary>
    /// State of the matchmaking process
    /// </summary>
    public enum MatchmakingState
    {
        /// <summary>Not matchmaking</summary>
        Inactive,
        
        /// <summary>Preparing to start matchmaking</summary>
        Initializing,
        
        /// <summary>Actively searching for a match</summary>
        Searching,
        
        /// <summary>Match found, preparing to join</summary>
        MatchFound,
        
        /// <summary>Joining the matched lobby</summary>
        Joining,
        
        /// <summary>Matchmaking completed successfully</summary>
        Completed,
        
        /// <summary>Matchmaking canceled by user</summary>
        Canceled,
        
        /// <summary>Matchmaking failed with an error</summary>
        Failed,
        
        /// <summary>Matchmaking timed out</summary>
        TimedOut
    }
} 