using System;
using System.Collections.Generic;
using Epic.OnlineServices;
using Epic.OnlineServices.Leaderboards;
using Epic.OnlineServices.Stats;
using PlayEveryWare.EpicOnlineServices;
using RecipeRage.Modules.Logging;

namespace RecipeRage.Leaderboards
{
    /// <summary>
    /// Provider for leaderboards using Epic Online Services
    /// </summary>
    public class EOSLeaderboardsProvider : ILeaderboardsProvider
    {
        private const string PROVIDER_NAME = "EOSLeaderboards";
        private const int DEFAULT_PAGE_SIZE = 25;
        private const string LOG_TAG = "EOSLeaderboardsProvider";

        // Cache for leaderboard definitions
        private readonly Dictionary<string, LeaderboardDefinition> _leaderboardDefinitions = new Dictionary<string, LeaderboardDefinition>();

        // Initialization status
        private bool _isInitialized;

        // EOS Leaderboards interface
        private LeaderboardsInterface _leaderboardsInterface;

        /// <summary>
        /// Creates a new EOS leaderboards provider
        /// </summary>
        public EOSLeaderboardsProvider()
        {
            LogHelper.Debug(LOG_TAG, "EOSLeaderboardsProvider created");
        }

        /// <summary>
        /// Gets the name of the provider
        /// </summary>
        public string ProviderName => PROVIDER_NAME;

        /// <summary>
        /// Indicates if the provider is available and initialized
        /// </summary>
        public bool IsAvailable => _isInitialized && IsUserLoggedIn();

        /// <summary>
        /// Gets the last error message if any operation failed
        /// </summary>
        public string LastError { get; private set; }

        /// <summary>
        /// Initializes the leaderboard provider
        /// </summary>
        /// <param name="callback"> Callback when initialization completes </param>
        public void Initialize(Action<bool> callback)
        {
            if (_isInitialized)
            {
                LogHelper.Warning(LOG_TAG, "EOSLeaderboardsProvider already initialized");
                callback?.Invoke(true);
                return;
            }

            LogHelper.Info(LOG_TAG, "Initializing EOSLeaderboardsProvider");

            try
            {
                // Check if the EOS manager exists
                if (EOSManager.Instance == null)
                {
                    LastError = "EOS Manager is not available";
                    LogHelper.Error(LOG_TAG, LastError);
                    callback?.Invoke(false);
                    return;
                }

                // Get the leaderboards interface
                _leaderboardsInterface = EOSManager.Instance.GetEOSPlatformInterface().GetLeaderboardsInterface();
                if (_leaderboardsInterface == null)
                {
                    LastError = "Failed to get EOS Leaderboards interface";
                    LogHelper.Error(LOG_TAG, LastError);
                    callback?.Invoke(false);
                    return;
                }

                // Check if user is signed in
                if (!IsUserLoggedIn())
                {
                    LastError = "User is not logged in to EOS";
                    LogHelper.Warning(LOG_TAG, LastError);
                    // We still mark as initialized, but will check IsAvailable before operations
                    _isInitialized = true;
                    callback?.Invoke(true);
                    return;
                }

                _isInitialized = true;
                LogHelper.Info(LOG_TAG, "EOSLeaderboardsProvider initialized successfully");
                callback?.Invoke(true);
            }
            catch (Exception ex)
            {
                LastError = $"Error initializing EOSLeaderboardsProvider: {ex.Message}";
                LogHelper.Exception(LOG_TAG, ex, "Failed to initialize EOSLeaderboardsProvider");
                callback?.Invoke(false);
            }
        }

        /// <summary>
        /// Queries leaderboard definitions from the provider
        /// </summary>
        /// <param name="callback"> Callback with the list of leaderboard definitions </param>
        public void QueryLeaderboardDefinitions(Action<List<LeaderboardDefinition>, bool> callback)
        {
            if (!CheckAvailability())
            {
                callback?.Invoke(new List<LeaderboardDefinition>(), false);
                return;
            }

            LogHelper.Debug(LOG_TAG, "Querying leaderboard definitions from EOS");

            try
            {
                var options = new QueryLeaderboardDefinitionsOptions();

                if (EOSManager.Instance.GetProductUserId() != null)
                {
                    options.LocalUserId = EOSManager.Instance.GetProductUserId();
                }
                else
                {
                    LastError = "No local user ID available";
                    LogHelper.Error(LOG_TAG, LastError);
                    callback?.Invoke(new List<LeaderboardDefinition>(), false);
                    return;
                }

                _leaderboardsInterface.QueryLeaderboardDefinitions(ref options, null,
                    (ref QueryLeaderboardDefinitionsCallbackInfo callbackInfo) =>
                    {
                        if (callbackInfo.ResultCode == Result.Success)
                        {
                            LogHelper.Info(LOG_TAG, "Successfully queried leaderboard definitions");

                            // Get the count of definitions
                            var countOptions = new GetLeaderboardDefinitionCountOptions();
                            uint count = _leaderboardsInterface.GetLeaderboardDefinitionCount(ref countOptions);

                            LogHelper.Debug(LOG_TAG, $"Found {count} leaderboard definitions");

                            var definitions = new List<LeaderboardDefinition>();

                            // Retrieve each definition
                            for (uint i = 0; i < count; i++)
                            {
                                var defOptions = new CopyLeaderboardDefinitionByIndexOptions { LeaderboardIndex = i };
                                var result =
                                    _leaderboardsInterface.CopyLeaderboardDefinitionByIndex(ref defOptions,
                                        out Definition eosDefinition);

                                if (result == Result.Success)
                                {
                                    // Convert EOS definition to our model
                                    var definition = new LeaderboardDefinition
                                    {
                                        LeaderboardId = eosDefinition.LeaderboardId,
                                        StatName = eosDefinition.StatName,
                                        DisplayName =
                                            eosDefinition.LeaderboardId, // EOS doesn't provide a separate display name
                                        ProviderName = PROVIDER_NAME,
                                        IsActive = true,
                                        OrderingType = eosDefinition.StartTime == null
                                            ? LeaderboardOrderingType.Descending
                                            : LeaderboardOrderingType.Ascending // Guess based on stat type
                                    };

                                    definitions.Add(definition);

                                    // Cache the definition
                                    _leaderboardDefinitions[definition.LeaderboardId] = definition;

                                    LogHelper.Debug(LOG_TAG,
                                        $"Added leaderboard definition: {definition.LeaderboardId}, Stat: {definition.StatName}");
                                }
                                else
                                {
                                    LogHelper.Warning(LOG_TAG,
                                        $"Failed to copy leaderboard definition at index {i}: {result}");
                                }
                            }

                            callback?.Invoke(definitions, true);
                        }
                        else
                        {
                            LastError = $"Failed to query leaderboard definitions: {callbackInfo.ResultCode}";
                            LogHelper.Error(LOG_TAG, LastError);
                            callback?.Invoke(new List<LeaderboardDefinition>(), false);
                        }
                    });
            }
            catch (Exception ex)
            {
                LastError = $"Error querying leaderboard definitions: {ex.Message}";
                LogHelper.Exception(LOG_TAG, ex, "Failed to query leaderboard definitions");
                callback?.Invoke(new List<LeaderboardDefinition>(), false);
            }
        }

        /// <summary>
        /// Queries entries for a specific leaderboard
        /// </summary>
        /// <param name="leaderboardId"> ID of the leaderboard to query </param>
        /// <param name="startRank"> Starting rank to query (1-based) </param>
        /// <param name="count"> Number of entries to retrieve </param>
        /// <param name="callback"> Callback with the list of leaderboard entries </param>
        public void QueryLeaderboardEntries(string leaderboardId, int startRank, int count,
            Action<List<LeaderboardEntry>, bool> callback)
        {
            if (!CheckAvailability())
            {
                callback?.Invoke(new List<LeaderboardEntry>(), false);
                return;
            }

            if (string.IsNullOrEmpty(leaderboardId))
            {
                LastError = "Leaderboard ID cannot be empty";
                LogHelper.Error(LOG_TAG, LastError);
                callback?.Invoke(new List<LeaderboardEntry>(), false);
                return;
            }

            LogHelper.Debug(LOG_TAG,
                $"Querying leaderboard entries for {leaderboardId}, start rank: {startRank}, count: {count}");

            try
            {
                var options = new QueryLeaderboardRanksOptions
                {
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    LeaderboardId = leaderboardId
                };

                _leaderboardsInterface.QueryLeaderboardRanks(ref options, null,
                    (ref QueryLeaderboardRanksCallbackInfo callbackInfo) =>
                    {
                        if (callbackInfo.ResultCode == Result.Success)
                        {
                            LogHelper.Info(LOG_TAG, $"Successfully queried leaderboard ranks for {leaderboardId}");

                            // Get the count of ranks/records
                            var countOptions = new GetLeaderboardRecordCountOptions();
                            uint recordCount = _leaderboardsInterface.GetLeaderboardRecordCount(ref countOptions);

                            LogHelper.Debug(LOG_TAG, $"Found {recordCount} leaderboard records");

                            var entries = new List<LeaderboardEntry>();

                            // Apply paging (EOS gives us all records, we need to filter)
                            uint startIndex = (uint)Math.Max(0, startRank - 1); // Convert 1-based to 0-based
                            uint endIndex = Math.Min(startIndex + (uint)count, recordCount);

                            // Get the definition for this leaderboard (if available)
                            LeaderboardDefinition definition = null;
                            _leaderboardDefinitions.TryGetValue(leaderboardId, out definition);

                            // Retrieve records within the requested range
                            for (uint i = startIndex; i < endIndex; i++)
                            {
                                var recordOptions = new CopyLeaderboardRecordByIndexOptions
                                {
                                    LeaderboardRecordIndex = i
                                };

                                var result = _leaderboardsInterface.CopyLeaderboardRecordByIndex(ref recordOptions,
                                    out LeaderboardRecord record);

                                if (result == Result.Success)
                                {
                                    var entry = new LeaderboardEntry
                                    {
                                        LeaderboardId = leaderboardId,
                                        UserId = record.UserId.ToString(),
                                        DisplayName = GetDisplayNameFromUserId(record.UserId),
                                        Rank = (int)record.Rank + 1, // EOS ranks are 0-based, we use 1-based
                                        Score = record.Score,
                                        ProviderName = PROVIDER_NAME,
                                        Timestamp = DateTime.UtcNow, // EOS doesn't provide a timestamp
                                        IsCurrentUser = record.UserId.ToString() ==
                                                        EOSManager.Instance.GetProductUserId().ToString()
                                    };

                                    entries.Add(entry);

                                    LogHelper.Debug(LOG_TAG,
                                        $"Added leaderboard entry: User: {entry.UserId}, Rank: {entry.Rank}, Score: {entry.Score}");
                                }
                                else
                                {
                                    LogHelper.Warning(LOG_TAG,
                                        $"Failed to copy leaderboard record at index {i}: {result}");
                                }
                            }

                            callback?.Invoke(entries, true);
                        }
                        else
                        {
                            LastError = $"Failed to query leaderboard ranks: {callbackInfo.ResultCode}";
                            LogHelper.Error(LOG_TAG, LastError);
                            callback?.Invoke(new List<LeaderboardEntry>(), false);
                        }
                    });
            }
            catch (Exception ex)
            {
                LastError = $"Error querying leaderboard entries: {ex.Message}";
                LogHelper.Exception(LOG_TAG, ex, "Failed to query leaderboard entries");
                callback?.Invoke(new List<LeaderboardEntry>(), false);
            }
        }

        /// <summary>
        /// Queries entries for a specific leaderboard filtered to the user's friends
        /// </summary>
        /// <param name="leaderboardId"> ID of the leaderboard to query </param>
        /// <param name="callback"> Callback with the list of leaderboard entries </param>
        public void QueryLeaderboardEntriesForFriends(string leaderboardId,
            Action<List<LeaderboardEntry>, bool> callback)
        {
            if (!CheckAvailability())
            {
                callback?.Invoke(new List<LeaderboardEntry>(), false);
                return;
            }

            if (string.IsNullOrEmpty(leaderboardId))
            {
                LastError = "Leaderboard ID cannot be empty";
                LogHelper.Error(LOG_TAG, LastError);
                callback?.Invoke(new List<LeaderboardEntry>(), false);
                return;
            }

            LogHelper.Debug(LOG_TAG, $"Querying leaderboard entries for friends for {leaderboardId}");

            try
            {
                var options = new QueryLeaderboardUserScoresOptions
                {
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    UserIds = new[] { EOSManager.Instance.GetProductUserId() }, // We'll need to expand this
                    LeaderboardIds = new[] { leaderboardId }
                };

                // We need to get the friend list from EOS
                // For now, just query the full leaderboard and mark friends if possible
                QueryLeaderboardEntries(leaderboardId, 1, 100, (entries, success) =>
                {
                    if (success)
                    {
                        // For future implementation:
                        // 1. Get friend list from EOS
                        // 2. Filter entries to only include friends
                        // 3. Return the filtered list

                        LogHelper.Info(LOG_TAG,
                            $"Successfully queried leaderboard entries for friends for {leaderboardId}");
                        callback?.Invoke(entries, true);
                    }
                    else
                    {
                        callback?.Invoke(new List<LeaderboardEntry>(), false);
                    }
                });
            }
            catch (Exception ex)
            {
                LastError = $"Error querying leaderboard entries for friends: {ex.Message}";
                LogHelper.Exception(LOG_TAG, ex, "Failed to query leaderboard entries for friends");
                callback?.Invoke(new List<LeaderboardEntry>(), false);
            }
        }

        /// <summary>
        /// Queries a specific user's entry in a leaderboard
        /// </summary>
        /// <param name="leaderboardId"> ID of the leaderboard to query </param>
        /// <param name="userId"> User ID to look up </param>
        /// <param name="callback"> Callback with the user's leaderboard entry (null if not found) </param>
        public void QueryLeaderboardUserEntry(string leaderboardId, string userId,
            Action<LeaderboardEntry, bool> callback)
        {
            if (!CheckAvailability())
            {
                callback?.Invoke(null, false);
                return;
            }

            if (string.IsNullOrEmpty(leaderboardId))
            {
                LastError = "Leaderboard ID cannot be empty";
                LogHelper.Error(LOG_TAG, LastError);
                callback?.Invoke(null, false);
                return;
            }

            if (string.IsNullOrEmpty(userId))
            {
                LastError = "User ID cannot be empty";
                LogHelper.Error(LOG_TAG, LastError);
                callback?.Invoke(null, false);
                return;
            }

            LogHelper.Debug(LOG_TAG, $"Querying leaderboard entry for user {userId} in leaderboard {leaderboardId}");

            try
            {
                ProductUserId productUserId;
                if (!ProductUserId.TryParse(userId, out productUserId))
                {
                    LastError = "Invalid user ID format";
                    LogHelper.Error(LOG_TAG, LastError);
                    callback?.Invoke(null, false);
                    return;
                }

                var options = new QueryLeaderboardUserScoresOptions
                {
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    UserIds = new[] { productUserId },
                    LeaderboardIds = new[] { leaderboardId }
                };

                _leaderboardsInterface.QueryLeaderboardUserScores(ref options, null,
                    (ref QueryLeaderboardUserScoresCallbackInfo callbackInfo) =>
                    {
                        if (callbackInfo.ResultCode == Result.Success)
                        {
                            LogHelper.Info(LOG_TAG,
                                $"Successfully queried leaderboard user score for {userId} in {leaderboardId}");

                            var userScoreOptions = new CopyLeaderboardUserScoreByUserIdOptions
                            {
                                UserId = productUserId,
                                LeaderboardId = leaderboardId,
                                StatName = null // We don't know the stat name, EOS will figure it out
                            };

                            var result = _leaderboardsInterface.CopyLeaderboardUserScoreByUserId(ref userScoreOptions,
                                out LeaderboardUserScore userScore);

                            if (result == Result.Success)
                            {
                                var entry = new LeaderboardEntry
                                {
                                    LeaderboardId = leaderboardId,
                                    UserId = userId,
                                    DisplayName = GetDisplayNameFromUserId(productUserId),
                                    Score = userScore.Score,
                                    ProviderName = PROVIDER_NAME,
                                    Timestamp = DateTime.UtcNow, // EOS doesn't provide a timestamp
                                    IsCurrentUser = userId == EOSManager.Instance.GetProductUserId().ToString()
                                };

                                LogHelper.Debug(LOG_TAG, $"Found user entry: Score: {entry.Score}");
                                callback?.Invoke(entry, true);
                            }
                            else
                            {
                                LastError = $"User not found on leaderboard: {result}";
                                LogHelper.Warning(LOG_TAG, LastError);
                                callback?.Invoke(null, false);
                            }
                        }
                        else
                        {
                            LastError = $"Failed to query leaderboard user score: {callbackInfo.ResultCode}";
                            LogHelper.Error(LOG_TAG, LastError);
                            callback?.Invoke(null, false);
                        }
                    });
            }
            catch (Exception ex)
            {
                LastError = $"Error querying leaderboard user entry: {ex.Message}";
                LogHelper.Exception(LOG_TAG, ex, "Failed to query leaderboard user entry");
                callback?.Invoke(null, false);
            }
        }

        /// <summary>
        /// Queries the current user's entry in a leaderboard
        /// </summary>
        /// <param name="leaderboardId"> ID of the leaderboard to query </param>
        /// <param name="callback"> Callback with the user's leaderboard entry (null if not found) </param>
        public void QueryLeaderboardCurrentUserEntry(string leaderboardId, Action<LeaderboardEntry, bool> callback)
        {
            if (!CheckAvailability())
            {
                callback?.Invoke(null, false);
                return;
            }

            string userId = EOSManager.Instance.GetProductUserId().ToString();
            QueryLeaderboardUserEntry(leaderboardId, userId, callback);
        }

        /// <summary>
        /// Submits a score to a leaderboard
        /// </summary>
        /// <param name="leaderboardId"> ID of the leaderboard to submit to </param>
        /// <param name="score"> Score value to submit </param>
        /// <param name="callback"> Callback indicating success or failure </param>
        public void SubmitScore(string leaderboardId, long score, Action<bool> callback)
        {
            if (!CheckAvailability())
            {
                callback?.Invoke(false);
                return;
            }

            if (string.IsNullOrEmpty(leaderboardId))
            {
                LastError = "Leaderboard ID cannot be empty";
                LogHelper.Error(LOG_TAG, LastError);
                callback?.Invoke(false);
                return;
            }

            LogHelper.Debug(LOG_TAG, $"Submitting score {score} to leaderboard {leaderboardId}");

            // Find the stat name for this leaderboard
            string statName = null;

            if (_leaderboardDefinitions.TryGetValue(leaderboardId, out var definition))
            {
                statName = definition.StatName;
            }
            else
            {
                // Query leaderboard definitions first
                QueryLeaderboardDefinitions((definitions, success) =>
                {
                    if (success)
                    {
                        // Try to find the definition again
                        if (_leaderboardDefinitions.TryGetValue(leaderboardId, out definition))
                        {
                            statName = definition.StatName;
                            SubmitScoreInternal(leaderboardId, statName, score, callback);
                        }
                        else
                        {
                            LastError = $"Leaderboard definition not found for {leaderboardId}";
                            LogHelper.Error(LOG_TAG, LastError);
                            callback?.Invoke(false);
                        }
                    }
                    else
                    {
                        callback?.Invoke(false);
                    }
                });
                return;
            }

            SubmitScoreInternal(leaderboardId, statName, score, callback);
        }

        /// <summary>
        /// Submits a score to a leaderboard with additional metadata
        /// </summary>
        /// <param name="leaderboardId"> ID of the leaderboard to submit to </param>
        /// <param name="score"> Score value to submit </param>
        /// <param name="metadata"> Additional metadata for the score (display info, etc.) </param>
        /// <param name="callback"> Callback indicating success or failure </param>
        public void SubmitScoreWithMetadata(string leaderboardId, long score, string metadata, Action<bool> callback)
        {
            // EOS doesn't support metadata, so just submit the score
            SubmitScore(leaderboardId, score, callback);
        }

        /// <summary>
        /// Opens the platform-specific UI for viewing leaderboards (if supported)
        /// </summary>
        /// <param name="leaderboardId"> ID of the leaderboard to display </param>
        /// <returns> True if the UI was opened successfully </returns>
        public bool DisplayLeaderboardUI(string leaderboardId)
        {
            if (!CheckAvailability()) return false;

            // EOS doesn't have a direct method to display leaderboards UI
            // Game should implement its own UI using the leaderboard data

            LastError = "EOS doesn't support displaying leaderboard UI directly";
            LogHelper.Warning(LOG_TAG, LastError);
            return false;
        }

        /// <summary>
        /// Submits a score to a leaderboard (internal implementation)
        /// </summary>
        /// <param name="leaderboardId"> ID of the leaderboard </param>
        /// <param name="statName"> Name of the stat </param>
        /// <param name="score"> Score value </param>
        /// <param name="callback"> Callback indicating success or failure </param>
        private void SubmitScoreInternal(string leaderboardId, string statName, long score, Action<bool> callback)
        {
            if (string.IsNullOrEmpty(statName))
            {
                LastError = $"No stat name found for leaderboard {leaderboardId}";
                LogHelper.Error(LOG_TAG, LastError);
                callback?.Invoke(false);
                return;
            }

            try
            {
                // EOS doesn't have a direct "submit score" method
                // Instead, we need to update the stat for the leaderboard

                // Get the Stats interface
                var statsInterface = EOSManager.Instance.GetEOSPlatformInterface().GetStatsInterface();
                if (statsInterface == null)
                {
                    LastError = "Failed to get EOS Stats interface";
                    LogHelper.Error(LOG_TAG, LastError);
                    callback?.Invoke(false);
                    return;
                }

                // Create the stat to ingest
                var stat = new IngestData
                {
                    StatName = statName,
                    IngestAmount = score
                };

                var options = new IngestStatOptions
                {
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    Stats = new[] { stat },
                    TargetUserId = EOSManager.Instance.GetProductUserId()
                };

                statsInterface.IngestStat(ref options, null, (ref IngestStatCompleteCallbackInfo callbackInfo) =>
                {
                    if (callbackInfo.ResultCode == Result.Success)
                    {
                        LogHelper.Info(LOG_TAG,
                            $"Successfully submitted score {score} to leaderboard {leaderboardId} (stat {statName})");
                        callback?.Invoke(true);
                    }
                    else
                    {
                        LastError = $"Failed to submit score: {callbackInfo.ResultCode}";
                        LogHelper.Error(LOG_TAG, LastError);
                        callback?.Invoke(false);
                    }
                });
            }
            catch (Exception ex)
            {
                LastError = $"Error submitting score: {ex.Message}";
                LogHelper.Exception(LOG_TAG, ex, "Failed to submit score");
                callback?.Invoke(false);
            }
        }

        /// <summary>
        /// Checks if a user is currently logged in to EOS
        /// </summary>
        /// <returns> True if a user is logged in </returns>
        private bool IsUserLoggedIn()
        {
            return EOSManager.Instance != null &&
                   EOSManager.Instance.GetEOSPlatformInterface() != null &&
                   EOSManager.Instance.GetProductUserId() != null;
        }

        /// <summary>
        /// Checks if the provider is available for use
        /// </summary>
        /// <returns> True if the provider is available </returns>
        private bool CheckAvailability()
        {
            if (!_isInitialized)
            {
                LastError = "EOSLeaderboardsProvider is not initialized";
                LogHelper.Error(LOG_TAG, LastError);
                return false;
            }

            if (!IsUserLoggedIn())
            {
                LastError = "User is not logged in to EOS";
                LogHelper.Warning(LOG_TAG, LastError);
                return false;
            }

            if (_leaderboardsInterface == null)
            {
                LastError = "EOS Leaderboards interface is not available";
                LogHelper.Error(LOG_TAG, LastError);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets a display name for a user ID
        /// </summary>
        /// <param name="userId"> User ID to get the name for </param>
        /// <returns> Display name if available, otherwise the user ID </returns>
        private string GetDisplayNameFromUserId(ProductUserId userId)
        {
            if (userId == null)
                return "Unknown";

            // This would be implemented with EOS Connect interface to get display names
            // For now, just use the user ID as the display name
            return userId.ToString();
        }
    }
}