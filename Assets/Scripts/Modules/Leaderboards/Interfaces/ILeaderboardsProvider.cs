using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace RecipeRage.Leaderboards
{
    /// <summary>
    /// Interface for leaderboard providers (e.g., EOS, Steam, etc.)
    /// </summary>
    public interface ILeaderboardsProvider
    {
        /// <summary>
        /// Gets the name of the provider
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// Indicates if the provider is available and initialized
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// Gets the last error message if any operation failed
        /// </summary>
        string LastError { get; }

        /// <summary>
        /// Initializes the leaderboard provider
        /// </summary>
        /// <param name="callback">Callback when initialization completes</param>
        void Initialize(Action<bool> callback);

        /// <summary>
        /// Queries leaderboard definitions from the provider
        /// </summary>
        /// <param name="callback">Callback with the list of leaderboard definitions</param>
        void QueryLeaderboardDefinitions(Action<List<LeaderboardDefinition>, bool> callback);

        /// <summary>
        /// Queries entries for a specific leaderboard
        /// </summary>
        /// <param name="leaderboardId">ID of the leaderboard to query</param>
        /// <param name="startRank">Starting rank to query (1-based)</param>
        /// <param name="count">Number of entries to retrieve</param>
        /// <param name="callback">Callback with the list of leaderboard entries</param>
        void QueryLeaderboardEntries(string leaderboardId, int startRank, int count, Action<List<LeaderboardEntry>, bool> callback);

        /// <summary>
        /// Queries entries for a specific leaderboard filtered to the user's friends
        /// </summary>
        /// <param name="leaderboardId">ID of the leaderboard to query</param>
        /// <param name="callback">Callback with the list of leaderboard entries</param>
        void QueryLeaderboardEntriesForFriends(string leaderboardId, Action<List<LeaderboardEntry>, bool> callback);

        /// <summary>
        /// Queries a specific user's entry in a leaderboard
        /// </summary>
        /// <param name="leaderboardId">ID of the leaderboard to query</param>
        /// <param name="userId">User ID to look up</param>
        /// <param name="callback">Callback with the user's leaderboard entry (null if not found)</param>
        void QueryLeaderboardUserEntry(string leaderboardId, string userId, Action<LeaderboardEntry, bool> callback);

        /// <summary>
        /// Queries the current user's entry in a leaderboard
        /// </summary>
        /// <param name="leaderboardId">ID of the leaderboard to query</param>
        /// <param name="callback">Callback with the user's leaderboard entry (null if not found)</param>
        void QueryLeaderboardCurrentUserEntry(string leaderboardId, Action<LeaderboardEntry, bool> callback);

        /// <summary>
        /// Submits a score to a leaderboard
        /// </summary>
        /// <param name="leaderboardId">ID of the leaderboard to submit to</param>
        /// <param name="score">Score value to submit</param>
        /// <param name="callback">Callback indicating success or failure</param>
        void SubmitScore(string leaderboardId, long score, Action<bool> callback);

        /// <summary>
        /// Submits a score to a leaderboard with additional metadata
        /// </summary>
        /// <param name="leaderboardId">ID of the leaderboard to submit to</param>
        /// <param name="score">Score value to submit</param>
        /// <param name="metadata">Additional metadata for the score (display info, etc.)</param>
        /// <param name="callback">Callback indicating success or failure</param>
        void SubmitScoreWithMetadata(string leaderboardId, long score, string metadata, Action<bool> callback);

        /// <summary>
        /// Opens the platform-specific UI for viewing leaderboards (if supported)
        /// </summary>
        /// <param name="leaderboardId">ID of the leaderboard to display</param>
        /// <returns>True if the UI was opened successfully</returns>
        bool DisplayLeaderboardUI(string leaderboardId);
    }
} 