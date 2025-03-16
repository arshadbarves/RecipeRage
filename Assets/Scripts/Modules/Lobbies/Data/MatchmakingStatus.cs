using System;

namespace RecipeRage.Modules.Lobbies.Data
{
    /// <summary>
    /// Represents the current state of a matchmaking operation
    /// </summary>
    [Serializable]
    public class MatchmakingStatus
    {
        /// <summary>
        /// The current state of the matchmaking process
        /// </summary>
        public MatchmakingState State { get; set; } = MatchmakingState.Inactive;
        
        /// <summary>
        /// Number of players found so far
        /// </summary>
        public int PlayersFound { get; set; } = 0;
        
        /// <summary>
        /// Number of players needed for the match
        /// </summary>
        public int PlayersNeeded { get; set; } = 0;
        
        /// <summary>
        /// Estimated time remaining in seconds (-1 if unknown)
        /// </summary>
        public float EstimatedTimeRemainingSeconds { get; set; } = -1;
        
        /// <summary>
        /// The time when matchmaking started
        /// </summary>
        public DateTime StartTime { get; set; } = DateTime.MinValue;
        
        /// <summary>
        /// Time elapsed since matchmaking started
        /// </summary>
        public TimeSpan ElapsedTime => DateTime.UtcNow - StartTime;
        
        /// <summary>
        /// The matchmaking ticket ID if available
        /// </summary>
        public string TicketId { get; set; } = string.Empty;
        
        /// <summary>
        /// Error message if matchmaking failed
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;
        
        /// <summary>
        /// The current server region being searched
        /// </summary>
        public string CurrentRegion { get; set; } = string.Empty;
        
        /// <summary>
        /// Progress percentage (0-100) based on players found vs. needed
        /// </summary>
        public float ProgressPercentage => PlayersNeeded > 0 ? (float)PlayersFound / PlayersNeeded * 100 : 0;
        
        /// <summary>
        /// Indicates if matchmaking is currently active
        /// </summary>
        public bool IsActive => State == MatchmakingState.Initializing || 
                                State == MatchmakingState.Searching || 
                                State == MatchmakingState.MatchFound;

        /// <summary>
        /// Create a string representation of the matchmaking status
        /// </summary>
        public override string ToString()
        {
            return $"MatchmakingStatus[State={State}, Players={PlayersFound}/{PlayersNeeded}, Region={CurrentRegion}]";
        }
    }

    /// <summary>
    /// Enumeration of possible matchmaking states
    /// </summary>
    public enum MatchmakingState
    {
        /// <summary>
        /// No matchmaking is in progress
        /// </summary>
        Inactive,
        
        /// <summary>
        /// Matchmaking is being initialized
        /// </summary>
        Initializing,
        
        /// <summary>
        /// Actively searching for a match
        /// </summary>
        Searching,
        
        /// <summary>
        /// A match has been found and is being finalized
        /// </summary>
        MatchFound,
        
        /// <summary>
        /// Matchmaking has been completed successfully
        /// </summary>
        Completed,
        
        /// <summary>
        /// Matchmaking was canceled by user
        /// </summary>
        Canceled,
        
        /// <summary>
        /// Matchmaking timed out
        /// </summary>
        TimedOut,
        
        /// <summary>
        /// Matchmaking failed due to an error
        /// </summary>
        Failed
    }
} 