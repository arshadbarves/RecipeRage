using System;
using System.Collections.Generic;

namespace RecipeRage.Leaderboards
{
    /// <summary>
    /// Interface for the leaderboards service, which manages leaderboard providers
    /// and provides a unified API for working with leaderboards.
    /// </summary>
    public interface ILeaderboardsService
    {
        /// <summary>
        /// Gets whether the service is initialized
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Gets the last error message if any operation failed
        /// </summary>
        string LastError { get; }

        /// <summary>
        /// Event triggered when a leaderboard is queried
        /// </summary>
        event Action<string, List<LeaderboardEntry>> OnLeaderboardQueried;

        /// <summary>
        /// Event triggered when a score is submitted
        /// </summary>
        event Action<string, long, bool> OnScoreSubmitted;

        /// <summary>
        /// Event triggered when a score submission fails
        /// </summary>
        event Action<string, long, string> OnScoreSubmissionFailed;

        /// <summary>
        /// Initializes the leaderboards service and all available providers
        /// </summary>
        /// <param name="callback"> Callback when initialization completes </param>
        void Initialize(Action<bool> callback);

        /// <summary>
        /// Adds a leaderboard provider to the service
        /// </summary>
        /// <param name="provider"> Provider to add </param>
        /// <returns> True if the provider was added successfully </returns>
        bool AddProvider(ILeaderboardsProvider provider);

        /// <summary>
        /// Gets a leaderboard provider by name
        /// </summary>
        /// <param name="providerName"> Name of the provider to get </param>
        /// <returns> The provider instance, or null if not found </returns>
        ILeaderboardsProvider GetProvider(string providerName);

        /// <summary>
        /// Gets all available leaderboard definitions
        /// </summary>
        /// <param name="callback"> Callback with the list of leaderboard definitions </param>
        void GetLeaderboardDefinitions(Action<List<LeaderboardDefinition>> callback);

        /// <summary>
        /// Gets entries for a specific leaderboard
        /// </summary>
        /// <param name="leaderboardId"> ID of the leaderboard to query </param>
        /// <param name="startRank"> Starting rank to query (1-based) </param>
        /// <param name="count"> Number of entries to retrieve </param>
        /// <param name="callback"> Callback with the list of leaderboard entries </param>
        void GetLeaderboardEntries(string leaderboardId, int startRank, int count,
            Action<List<LeaderboardEntry>> callback);

        /// <summary>
        /// Gets entries for a specific leaderboard filtered to the user's friends
        /// </summary>
        /// <param name="leaderboardId"> ID of the leaderboard to query </param>
        /// <param name="callback"> Callback with the list of leaderboard entries </param>
        void GetLeaderboardEntriesForFriends(string leaderboardId, Action<List<LeaderboardEntry>> callback);

        /// <summary>
        /// Gets a specific user's entry in a leaderboard
        /// </summary>
        /// <param name="leaderboardId"> ID of the leaderboard to query </param>
        /// <param name="userId"> User ID to look up </param>
        /// <param name="callback"> Callback with the user's leaderboard entry (null if not found) </param>
        void GetUserLeaderboardEntry(string leaderboardId, string userId, Action<LeaderboardEntry> callback);

        /// <summary>
        /// Gets the current user's entry in a leaderboard
        /// </summary>
        /// <param name="leaderboardId"> ID of the leaderboard to query </param>
        /// <param name="callback"> Callback with the user's leaderboard entry (null if not found) </param>
        void GetCurrentUserLeaderboardEntry(string leaderboardId, Action<LeaderboardEntry> callback);

        /// <summary>
        /// Submits a score to a leaderboard
        /// </summary>
        /// <param name="leaderboardId"> ID of the leaderboard to submit to </param>
        /// <param name="score"> Score value to submit </param>
        /// <param name="callback"> Optional callback indicating success or failure </param>
        void SubmitScore(string leaderboardId, long score, Action<bool> callback = null);

        /// <summary>
        /// Submits a score to a leaderboard with additional metadata
        /// </summary>
        /// <param name="leaderboardId"> ID of the leaderboard to submit to </param>
        /// <param name="score"> Score value to submit </param>
        /// <param name="metadata"> Additional metadata for the score (display info, etc.) </param>
        /// <param name="callback"> Optional callback indicating success or failure </param>
        void SubmitScoreWithMetadata(string leaderboardId, long score, string metadata, Action<bool> callback = null);

        /// <summary>
        /// Opens the platform-specific UI for viewing leaderboards (if supported)
        /// </summary>
        /// <param name="leaderboardId"> ID of the leaderboard to display </param>
        /// <returns> True if the UI was opened successfully </returns>
        bool DisplayLeaderboardUI(string leaderboardId);
    }
}