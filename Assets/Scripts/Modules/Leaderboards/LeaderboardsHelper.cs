using System;
using System.Collections.Generic;
using RecipeRage.Modules.Logging;

namespace RecipeRage.Leaderboards
{
    /// <summary>
    /// Static helper class for easy access to leaderboard functionality
    /// </summary>
    public static class LeaderboardsHelper
    {
        private static readonly ILeaderboardsService _leaderboardsService;
        private static bool _isInitialized;

        /// <summary>
        /// Static constructor for LeaderboardsHelper
        /// </summary>
        static LeaderboardsHelper()
        {
            // Create the service if it doesn't exist
            if (_leaderboardsService == null)
            {
                _leaderboardsService = new LeaderboardsService();
                LogHelper.Debug("LeaderboardsHelper", "LeaderboardsService created");
            }
        }

        /// <summary>
        /// Initializes the leaderboards system
        /// </summary>
        /// <param name="callback"> Optional callback when initialization completes </param>
        public static void Initialize(Action<bool> callback = null)
        {
            if (_isInitialized)
            {
                LogHelper.Warning("LeaderboardsHelper", "LeaderboardsHelper already initialized");
                callback?.Invoke(true);
                return;
            }

            LogHelper.Info("LeaderboardsHelper", "Initializing LeaderboardsHelper");

            // Add the EOS provider
            var eosProvider = new EOSLeaderboardsProvider();
            _leaderboardsService.AddProvider(eosProvider);

            // Initialize the service
            _leaderboardsService.Initialize(success =>
            {
                _isInitialized = success;

                if (success)
                    LogHelper.Info("LeaderboardsHelper", "LeaderboardsHelper initialized successfully");
                else
                    LogHelper.Error("LeaderboardsHelper",
                        $"Failed to initialize LeaderboardsHelper: {_leaderboardsService.LastError}");

                callback?.Invoke(success);
            });
        }

        /// <summary>
        /// Gets all available leaderboard definitions
        /// </summary>
        /// <param name="callback"> Callback with the list of leaderboard definitions </param>
        public static void GetLeaderboardDefinitions(Action<List<LeaderboardDefinition>> callback)
        {
            if (!CheckInitialized())
            {
                callback?.Invoke(new List<LeaderboardDefinition>());
                return;
            }

            _leaderboardsService.GetLeaderboardDefinitions(callback);
        }

        /// <summary>
        /// Gets entries for a specific leaderboard
        /// </summary>
        /// <param name="leaderboardId"> ID of the leaderboard to query </param>
        /// <param name="startRank"> Starting rank to query (1-based) </param>
        /// <param name="count"> Number of entries to retrieve </param>
        /// <param name="callback"> Callback with the list of leaderboard entries </param>
        public static void GetLeaderboardEntries(string leaderboardId, int startRank, int count,
            Action<List<LeaderboardEntry>> callback)
        {
            if (!CheckInitialized())
            {
                callback?.Invoke(new List<LeaderboardEntry>());
                return;
            }

            _leaderboardsService.GetLeaderboardEntries(leaderboardId, startRank, count, callback);
        }

        /// <summary>
        /// Gets entries for a specific leaderboard filtered to the user's friends
        /// </summary>
        /// <param name="leaderboardId"> ID of the leaderboard to query </param>
        /// <param name="callback"> Callback with the list of leaderboard entries </param>
        public static void GetLeaderboardEntriesForFriends(string leaderboardId,
            Action<List<LeaderboardEntry>> callback)
        {
            if (!CheckInitialized())
            {
                callback?.Invoke(new List<LeaderboardEntry>());
                return;
            }

            _leaderboardsService.GetLeaderboardEntriesForFriends(leaderboardId, callback);
        }

        /// <summary>
        /// Gets a specific user's entry in a leaderboard
        /// </summary>
        /// <param name="leaderboardId"> ID of the leaderboard to query </param>
        /// <param name="userId"> User ID to look up </param>
        /// <param name="callback"> Callback with the user's leaderboard entry (null if not found) </param>
        public static void GetUserLeaderboardEntry(string leaderboardId, string userId,
            Action<LeaderboardEntry> callback)
        {
            if (!CheckInitialized())
            {
                callback?.Invoke(null);
                return;
            }

            _leaderboardsService.GetUserLeaderboardEntry(leaderboardId, userId, callback);
        }

        /// <summary>
        /// Gets the current user's entry in a leaderboard
        /// </summary>
        /// <param name="leaderboardId"> ID of the leaderboard to query </param>
        /// <param name="callback"> Callback with the user's leaderboard entry (null if not found) </param>
        public static void GetCurrentUserLeaderboardEntry(string leaderboardId, Action<LeaderboardEntry> callback)
        {
            if (!CheckInitialized())
            {
                callback?.Invoke(null);
                return;
            }

            _leaderboardsService.GetCurrentUserLeaderboardEntry(leaderboardId, callback);
        }

        /// <summary>
        /// Submits a score to a leaderboard
        /// </summary>
        /// <param name="leaderboardId"> ID of the leaderboard to submit to </param>
        /// <param name="score"> Score value to submit </param>
        /// <param name="callback"> Optional callback indicating success or failure </param>
        public static void SubmitScore(string leaderboardId, long score, Action<bool> callback = null)
        {
            if (!CheckInitialized())
            {
                callback?.Invoke(false);
                return;
            }

            _leaderboardsService.SubmitScore(leaderboardId, score, callback);
        }

        /// <summary>
        /// Submits a score to a leaderboard with additional metadata
        /// </summary>
        /// <param name="leaderboardId"> ID of the leaderboard to submit to </param>
        /// <param name="score"> Score value to submit </param>
        /// <param name="metadata"> Additional metadata for the score (display info, etc.) </param>
        /// <param name="callback"> Optional callback indicating success or failure </param>
        public static void SubmitScoreWithMetadata(string leaderboardId, long score, string metadata,
            Action<bool> callback = null)
        {
            if (!CheckInitialized())
            {
                callback?.Invoke(false);
                return;
            }

            _leaderboardsService.SubmitScoreWithMetadata(leaderboardId, score, metadata, callback);
        }

        /// <summary>
        /// Opens the platform-specific UI for viewing leaderboards (if supported)
        /// </summary>
        /// <param name="leaderboardId"> ID of the leaderboard to display </param>
        /// <returns> True if the UI was opened successfully </returns>
        public static bool DisplayLeaderboardUI(string leaderboardId)
        {
            if (!CheckInitialized()) return false;

            return _leaderboardsService.DisplayLeaderboardUI(leaderboardId);
        }

        /// <summary>
        /// Registers for leaderboard query events
        /// </summary>
        /// <param name="callback"> Callback to invoke when a leaderboard is queried </param>
        public static void RegisterOnLeaderboardQueried(Action<string, List<LeaderboardEntry>> callback)
        {
            if (!CheckInitialized())
                return;

            _leaderboardsService.OnLeaderboardQueried += callback;
        }

        /// <summary>
        /// Unregisters from leaderboard query events
        /// </summary>
        /// <param name="callback"> Callback to remove </param>
        public static void UnregisterOnLeaderboardQueried(Action<string, List<LeaderboardEntry>> callback)
        {
            if (_leaderboardsService != null) _leaderboardsService.OnLeaderboardQueried -= callback;
        }

        /// <summary>
        /// Registers for score submission events
        /// </summary>
        /// <param name="callback"> Callback to invoke when a score is submitted </param>
        public static void RegisterOnScoreSubmitted(Action<string, long, bool> callback)
        {
            if (!CheckInitialized())
                return;

            _leaderboardsService.OnScoreSubmitted += callback;
        }

        /// <summary>
        /// Unregisters from score submission events
        /// </summary>
        /// <param name="callback"> Callback to remove </param>
        public static void UnregisterOnScoreSubmitted(Action<string, long, bool> callback)
        {
            if (_leaderboardsService != null) _leaderboardsService.OnScoreSubmitted -= callback;
        }

        /// <summary>
        /// Registers for score submission failure events
        /// </summary>
        /// <param name="callback"> Callback to invoke when a score submission fails </param>
        public static void RegisterOnScoreSubmissionFailed(Action<string, long, string> callback)
        {
            if (!CheckInitialized())
                return;

            _leaderboardsService.OnScoreSubmissionFailed += callback;
        }

        /// <summary>
        /// Unregisters from score submission failure events
        /// </summary>
        /// <param name="callback"> Callback to remove </param>
        public static void UnregisterOnScoreSubmissionFailed(Action<string, long, string> callback)
        {
            if (_leaderboardsService != null) _leaderboardsService.OnScoreSubmissionFailed -= callback;
        }

        /// <summary>
        /// Associates a stat with a leaderboard for automatic score submission
        /// </summary>
        /// <param name="statName"> Name of the stat </param>
        /// <param name="leaderboardId"> ID of the leaderboard </param>
        /// <param name="autoSubmit"> Whether to automatically submit scores when the stat changes </param>
        /// <param name="transformType"> Function to apply to the stat value before submission </param>
        /// <param name="transformValue"> Value to use in the transform function </param>
        public static void RegisterStatForLeaderboard(
            string statName,
            string leaderboardId,
            bool autoSubmit = true,
            LeaderboardStatTransformType transformType = LeaderboardStatTransformType.None,
            float transformValue = 1.0f)
        {
            if (!CheckInitialized())
                return;

            var statInfo = new LeaderboardStatInfo
            {
                StatName = statName,
                LeaderboardId = leaderboardId,
                AutoSubmit = autoSubmit,
                TransformType = transformType,
                TransformValue = transformValue
            };

            ((LeaderboardsService)_leaderboardsService).RegisterStatMapping(statInfo);
        }

        /// <summary>
        /// Updates a stat value and potentially submits to associated leaderboards
        /// </summary>
        /// <param name="statName"> Name of the stat to update </param>
        /// <param name="statValue"> New value of the stat </param>
        public static void UpdateStat(string statName, double statValue)
        {
            if (!CheckInitialized())
                return;

            ((LeaderboardsService)_leaderboardsService).HandleStatUpdate(statName, statValue);
        }

        /// <summary>
        /// Checks if leaderboard system is initialized
        /// </summary>
        /// <returns> True if initialized </returns>
        private static bool CheckInitialized()
        {
            if (!_isInitialized)
            {
                LogHelper.Warning("LeaderboardsHelper", "LeaderboardsHelper is not initialized");
                return false;
            }

            return true;
        }
    }
}