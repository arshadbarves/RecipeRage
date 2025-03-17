using System;
using System.Collections.Generic;
using System.Linq;
using RecipeRage.Modules.Logging;

namespace RecipeRage.Leaderboards
{
    /// <summary>
    /// Main service for managing leaderboards across different providers
    /// </summary>
    public class LeaderboardsService : ILeaderboardsService
    {
        // Cached leaderboard definitions
        private readonly Dictionary<string, LeaderboardDefinition> _leaderboardDefinitions = new Dictionary<string, LeaderboardDefinition>();

        // Cached leaderboard entries for recent queries
        private readonly Dictionary<string, List<LeaderboardEntry>> _leaderboardEntries = new Dictionary<string, List<LeaderboardEntry>>();

        // Lock for thread safety
        private readonly object _lock = new object();

        // List of registered providers
        private readonly List<ILeaderboardsProvider> _providers = new List<ILeaderboardsProvider>();

        // Mappings from stats to leaderboards
        private readonly Dictionary<string, List<LeaderboardStatInfo>> _statToLeaderboardMappings = new Dictionary<string, List<LeaderboardStatInfo>>();

        /// <summary>
        /// Creates a new leaderboards service
        /// </summary>
        public LeaderboardsService()
        {
            LogHelper.Debug("LeaderboardsService", "LeaderboardsService created");
        }

        /// <summary>
        /// Event triggered when a leaderboard is queried
        /// </summary>
        public event Action<string, List<LeaderboardEntry>> OnLeaderboardQueried;

        /// <summary>
        /// Event triggered when a score is submitted
        /// </summary>
        public event Action<string, long, bool> OnScoreSubmitted;

        /// <summary>
        /// Event triggered when a score submission fails
        /// </summary>
        public event Action<string, long, string> OnScoreSubmissionFailed;

        /// <summary>
        /// Gets whether the service is initialized
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Gets the last error message if any operation failed
        /// </summary>
        public string LastError { get; private set; }

        /// <summary>
        /// Initializes the leaderboards service and all available providers
        /// </summary>
        /// <param name="callback"> Callback when initialization completes </param>
        public void Initialize(Action<bool> callback)
        {
            if (IsInitialized)
            {
                LogHelper.Warning("LeaderboardsService", "LeaderboardsService already initialized");
                callback?.Invoke(true);
                return;
            }

            LogHelper.Info("LeaderboardsService", "Initializing LeaderboardsService");

            if (_providers.Count == 0)
            {
                LogHelper.Warning("LeaderboardsService", "No leaderboard providers registered");
                LastError = "No leaderboard providers registered";
                callback?.Invoke(false);
                return;
            }

            int providersToInitialize = _providers.Count;
            int providersInitialized = 0;
            bool anySuccess = false;

            foreach (var provider in _providers)
                provider.Initialize(success =>
                {
                    lock (_lock)
                    {
                        providersInitialized++;
                        if (success)
                        {
                            anySuccess = true;
                            LogHelper.Info("LeaderboardsService",
                                $"Provider {provider.ProviderName} initialized successfully");
                        }
                        else
                        {
                            LogHelper.Warning("LeaderboardsService",
                                $"Provider {provider.ProviderName} failed to initialize: {provider.LastError}");
                        }

                        if (providersInitialized >= providersToInitialize)
                        {
                            if (anySuccess)
                            {
                                // Load leaderboard definitions from each provider
                                LoadLeaderboardDefinitions(() =>
                                {
                                    IsInitialized = true;
                                    LogHelper.Info("LeaderboardsService",
                                        "LeaderboardsService initialized successfully");
                                    callback?.Invoke(true);
                                });
                            }
                            else
                            {
                                LastError = "All providers failed to initialize";
                                LogHelper.Error("LeaderboardsService", LastError);
                                callback?.Invoke(false);
                            }
                        }
                    }
                });
        }

        /// <summary>
        /// Adds a leaderboard provider to the service
        /// </summary>
        /// <param name="provider"> Provider to add </param>
        /// <returns> True if the provider was added successfully </returns>
        public bool AddProvider(ILeaderboardsProvider provider)
        {
            if (provider == null)
            {
                LogHelper.Error("LeaderboardsService", "Cannot add null provider");
                return false;
            }

            lock (_lock)
            {
                // Check if a provider with the same name already exists
                if (_providers.Any(p => p.ProviderName == provider.ProviderName))
                {
                    LogHelper.Warning("LeaderboardsService", $"Provider {provider.ProviderName} is already registered");
                    return false;
                }

                _providers.Add(provider);
                LogHelper.Info("LeaderboardsService", $"Provider {provider.ProviderName} registered");
                return true;
            }
        }

        /// <summary>
        /// Gets a leaderboard provider by name
        /// </summary>
        /// <param name="providerName"> Name of the provider to get </param>
        /// <returns> The provider instance, or null if not found </returns>
        public ILeaderboardsProvider GetProvider(string providerName)
        {
            lock (_lock)
            {
                return _providers.FirstOrDefault(p => p.ProviderName == providerName);
            }
        }

        /// <summary>
        /// Gets all available leaderboard definitions
        /// </summary>
        /// <param name="callback"> Callback with the list of leaderboard definitions </param>
        public void GetLeaderboardDefinitions(Action<List<LeaderboardDefinition>> callback)
        {
            if (!CheckInitialized())
            {
                callback?.Invoke(new List<LeaderboardDefinition>());
                return;
            }

            // If we already have cached definitions, return them
            if (_leaderboardDefinitions.Count > 0)
            {
                callback?.Invoke(_leaderboardDefinitions.Values.ToList());
                return;
            }

            // Otherwise, load definitions from providers
            LoadLeaderboardDefinitions(() => callback?.Invoke(_leaderboardDefinitions.Values.ToList()));
        }

        /// <summary>
        /// Gets entries for a specific leaderboard
        /// </summary>
        /// <param name="leaderboardId"> ID of the leaderboard to query </param>
        /// <param name="startRank"> Starting rank to query (1-based) </param>
        /// <param name="count"> Number of entries to retrieve </param>
        /// <param name="callback"> Callback with the list of leaderboard entries </param>
        public void GetLeaderboardEntries(string leaderboardId, int startRank, int count,
            Action<List<LeaderboardEntry>> callback)
        {
            if (!CheckInitialized())
            {
                callback?.Invoke(new List<LeaderboardEntry>());
                return;
            }

            if (string.IsNullOrEmpty(leaderboardId))
            {
                LogHelper.Error("LeaderboardsService", "Leaderboard ID cannot be empty");
                LastError = "Leaderboard ID cannot be empty";
                callback?.Invoke(new List<LeaderboardEntry>());
                return;
            }

            // Get the provider for this leaderboard
            var provider = GetProviderForLeaderboard(leaderboardId);
            if (provider == null)
            {
                LogHelper.Error("LeaderboardsService", $"No provider available for leaderboard {leaderboardId}");
                LastError = $"No provider available for leaderboard {leaderboardId}";
                callback?.Invoke(new List<LeaderboardEntry>());
                return;
            }

            LogHelper.Debug("LeaderboardsService",
                $"Querying leaderboard entries for {leaderboardId} from {provider.ProviderName}");

            provider.QueryLeaderboardEntries(leaderboardId, startRank, count, (entries, success) =>
            {
                if (success)
                {
                    lock (_lock)
                    {
                        // Cache the results
                        _leaderboardEntries[leaderboardId] = entries;
                    }

                    LogHelper.Debug("LeaderboardsService",
                        $"Retrieved {entries.Count} entries for leaderboard {leaderboardId}");
                    OnLeaderboardQueried?.Invoke(leaderboardId, entries);
                    callback?.Invoke(entries);
                }
                else
                {
                    LastError = provider.LastError;
                    LogHelper.Error("LeaderboardsService", $"Failed to query leaderboard {leaderboardId}: {LastError}");
                    callback?.Invoke(new List<LeaderboardEntry>());
                }
            });
        }

        /// <summary>
        /// Gets entries for a specific leaderboard filtered to the user's friends
        /// </summary>
        /// <param name="leaderboardId"> ID of the leaderboard to query </param>
        /// <param name="callback"> Callback with the list of leaderboard entries </param>
        public void GetLeaderboardEntriesForFriends(string leaderboardId, Action<List<LeaderboardEntry>> callback)
        {
            if (!CheckInitialized())
            {
                callback?.Invoke(new List<LeaderboardEntry>());
                return;
            }

            if (string.IsNullOrEmpty(leaderboardId))
            {
                LogHelper.Error("LeaderboardsService", "Leaderboard ID cannot be empty");
                LastError = "Leaderboard ID cannot be empty";
                callback?.Invoke(new List<LeaderboardEntry>());
                return;
            }

            // Get the provider for this leaderboard
            var provider = GetProviderForLeaderboard(leaderboardId);
            if (provider == null)
            {
                LogHelper.Error("LeaderboardsService", $"No provider available for leaderboard {leaderboardId}");
                LastError = $"No provider available for leaderboard {leaderboardId}";
                callback?.Invoke(new List<LeaderboardEntry>());
                return;
            }

            LogHelper.Debug("LeaderboardsService",
                $"Querying friend leaderboard entries for {leaderboardId} from {provider.ProviderName}");

            provider.QueryLeaderboardEntriesForFriends(leaderboardId, (entries, success) =>
            {
                if (success)
                {
                    LogHelper.Debug("LeaderboardsService",
                        $"Retrieved {entries.Count} friend entries for leaderboard {leaderboardId}");
                    OnLeaderboardQueried?.Invoke(leaderboardId, entries);
                    callback?.Invoke(entries);
                }
                else
                {
                    LastError = provider.LastError;
                    LogHelper.Error("LeaderboardsService",
                        $"Failed to query friends leaderboard {leaderboardId}: {LastError}");
                    callback?.Invoke(new List<LeaderboardEntry>());
                }
            });
        }

        /// <summary>
        /// Gets a specific user's entry in a leaderboard
        /// </summary>
        /// <param name="leaderboardId"> ID of the leaderboard to query </param>
        /// <param name="userId"> User ID to look up </param>
        /// <param name="callback"> Callback with the user's leaderboard entry (null if not found) </param>
        public void GetUserLeaderboardEntry(string leaderboardId, string userId, Action<LeaderboardEntry> callback)
        {
            if (!CheckInitialized())
            {
                callback?.Invoke(null);
                return;
            }

            if (string.IsNullOrEmpty(leaderboardId))
            {
                LogHelper.Error("LeaderboardsService", "Leaderboard ID cannot be empty");
                LastError = "Leaderboard ID cannot be empty";
                callback?.Invoke(null);
                return;
            }

            if (string.IsNullOrEmpty(userId))
            {
                LogHelper.Error("LeaderboardsService", "User ID cannot be empty");
                LastError = "User ID cannot be empty";
                callback?.Invoke(null);
                return;
            }

            // Get the provider for this leaderboard
            var provider = GetProviderForLeaderboard(leaderboardId);
            if (provider == null)
            {
                LogHelper.Error("LeaderboardsService", $"No provider available for leaderboard {leaderboardId}");
                LastError = $"No provider available for leaderboard {leaderboardId}";
                callback?.Invoke(null);
                return;
            }

            LogHelper.Debug("LeaderboardsService",
                $"Querying user entry for {userId} on leaderboard {leaderboardId} from {provider.ProviderName}");

            provider.QueryLeaderboardUserEntry(leaderboardId, userId, (entry, success) =>
            {
                if (success && entry != null)
                {
                    LogHelper.Debug("LeaderboardsService",
                        $"Retrieved entry for user {userId} on leaderboard {leaderboardId}: Rank {entry.Rank}, Score {entry.Score}");
                    callback?.Invoke(entry);
                }
                else
                {
                    LastError = provider.LastError;
                    LogHelper.Warning("LeaderboardsService",
                        $"Failed to query user entry or user not on leaderboard {leaderboardId}: {LastError}");
                    callback?.Invoke(null);
                }
            });
        }

        /// <summary>
        /// Gets the current user's entry in a leaderboard
        /// </summary>
        /// <param name="leaderboardId"> ID of the leaderboard to query </param>
        /// <param name="callback"> Callback with the user's leaderboard entry (null if not found) </param>
        public void GetCurrentUserLeaderboardEntry(string leaderboardId, Action<LeaderboardEntry> callback)
        {
            if (!CheckInitialized())
            {
                callback?.Invoke(null);
                return;
            }

            if (string.IsNullOrEmpty(leaderboardId))
            {
                LogHelper.Error("LeaderboardsService", "Leaderboard ID cannot be empty");
                LastError = "Leaderboard ID cannot be empty";
                callback?.Invoke(null);
                return;
            }

            // Get the provider for this leaderboard
            var provider = GetProviderForLeaderboard(leaderboardId);
            if (provider == null)
            {
                LogHelper.Error("LeaderboardsService", $"No provider available for leaderboard {leaderboardId}");
                LastError = $"No provider available for leaderboard {leaderboardId}";
                callback?.Invoke(null);
                return;
            }

            LogHelper.Debug("LeaderboardsService",
                $"Querying current user entry on leaderboard {leaderboardId} from {provider.ProviderName}");

            provider.QueryLeaderboardCurrentUserEntry(leaderboardId, (entry, success) =>
            {
                if (success && entry != null)
                {
                    LogHelper.Debug("LeaderboardsService",
                        $"Retrieved entry for current user on leaderboard {leaderboardId}: Rank {entry.Rank}, Score {entry.Score}");
                    callback?.Invoke(entry);
                }
                else
                {
                    LastError = provider.LastError;
                    LogHelper.Warning("LeaderboardsService",
                        $"Failed to query current user entry or user not on leaderboard {leaderboardId}: {LastError}");
                    callback?.Invoke(null);
                }
            });
        }

        /// <summary>
        /// Submits a score to a leaderboard
        /// </summary>
        /// <param name="leaderboardId"> ID of the leaderboard to submit to </param>
        /// <param name="score"> Score value to submit </param>
        /// <param name="callback"> Optional callback indicating success or failure </param>
        public void SubmitScore(string leaderboardId, long score, Action<bool> callback = null)
        {
            if (!CheckInitialized())
            {
                callback?.Invoke(false);
                return;
            }

            if (string.IsNullOrEmpty(leaderboardId))
            {
                LogHelper.Error("LeaderboardsService", "Leaderboard ID cannot be empty");
                LastError = "Leaderboard ID cannot be empty";
                OnScoreSubmissionFailed?.Invoke(leaderboardId, score, LastError);
                callback?.Invoke(false);
                return;
            }

            // Get the provider for this leaderboard
            var provider = GetProviderForLeaderboard(leaderboardId);
            if (provider == null)
            {
                LogHelper.Error("LeaderboardsService", $"No provider available for leaderboard {leaderboardId}");
                LastError = $"No provider available for leaderboard {leaderboardId}";
                OnScoreSubmissionFailed?.Invoke(leaderboardId, score, LastError);
                callback?.Invoke(false);
                return;
            }

            LogHelper.Debug("LeaderboardsService",
                $"Submitting score {score} to leaderboard {leaderboardId} via {provider.ProviderName}");

            provider.SubmitScore(leaderboardId, score, success =>
            {
                if (success)
                {
                    LogHelper.Info("LeaderboardsService",
                        $"Successfully submitted score {score} to leaderboard {leaderboardId}");
                    OnScoreSubmitted?.Invoke(leaderboardId, score, true);
                    callback?.Invoke(true);
                }
                else
                {
                    LastError = provider.LastError;
                    LogHelper.Error("LeaderboardsService",
                        $"Failed to submit score {score} to leaderboard {leaderboardId}: {LastError}");
                    OnScoreSubmissionFailed?.Invoke(leaderboardId, score, LastError);
                    callback?.Invoke(false);
                }
            });
        }

        /// <summary>
        /// Submits a score to a leaderboard with additional metadata
        /// </summary>
        /// <param name="leaderboardId"> ID of the leaderboard to submit to </param>
        /// <param name="score"> Score value to submit </param>
        /// <param name="metadata"> Additional metadata for the score (display info, etc.) </param>
        /// <param name="callback"> Optional callback indicating success or failure </param>
        public void SubmitScoreWithMetadata(string leaderboardId, long score, string metadata,
            Action<bool> callback = null)
        {
            if (!CheckInitialized())
            {
                callback?.Invoke(false);
                return;
            }

            if (string.IsNullOrEmpty(leaderboardId))
            {
                LogHelper.Error("LeaderboardsService", "Leaderboard ID cannot be empty");
                LastError = "Leaderboard ID cannot be empty";
                OnScoreSubmissionFailed?.Invoke(leaderboardId, score, LastError);
                callback?.Invoke(false);
                return;
            }

            // Get the provider for this leaderboard
            var provider = GetProviderForLeaderboard(leaderboardId);
            if (provider == null)
            {
                LogHelper.Error("LeaderboardsService", $"No provider available for leaderboard {leaderboardId}");
                LastError = $"No provider available for leaderboard {leaderboardId}";
                OnScoreSubmissionFailed?.Invoke(leaderboardId, score, LastError);
                callback?.Invoke(false);
                return;
            }

            LogHelper.Debug("LeaderboardsService",
                $"Submitting score {score} with metadata to leaderboard {leaderboardId} via {provider.ProviderName}");

            provider.SubmitScoreWithMetadata(leaderboardId, score, metadata, success =>
            {
                if (success)
                {
                    LogHelper.Info("LeaderboardsService",
                        $"Successfully submitted score {score} with metadata to leaderboard {leaderboardId}");
                    OnScoreSubmitted?.Invoke(leaderboardId, score, true);
                    callback?.Invoke(true);
                }
                else
                {
                    LastError = provider.LastError;
                    LogHelper.Error("LeaderboardsService",
                        $"Failed to submit score {score} with metadata to leaderboard {leaderboardId}: {LastError}");
                    OnScoreSubmissionFailed?.Invoke(leaderboardId, score, LastError);
                    callback?.Invoke(false);
                }
            });
        }

        /// <summary>
        /// Opens the platform-specific UI for viewing leaderboards (if supported)
        /// </summary>
        /// <param name="leaderboardId"> ID of the leaderboard to display </param>
        /// <returns> True if the UI was opened successfully </returns>
        public bool DisplayLeaderboardUI(string leaderboardId)
        {
            if (!CheckInitialized()) return false;

            if (string.IsNullOrEmpty(leaderboardId))
            {
                LogHelper.Error("LeaderboardsService", "Leaderboard ID cannot be empty");
                LastError = "Leaderboard ID cannot be empty";
                return false;
            }

            // Get the provider for this leaderboard
            var provider = GetProviderForLeaderboard(leaderboardId);
            if (provider == null)
            {
                LogHelper.Error("LeaderboardsService", $"No provider available for leaderboard {leaderboardId}");
                LastError = $"No provider available for leaderboard {leaderboardId}";
                return false;
            }

            LogHelper.Debug("LeaderboardsService",
                $"Displaying UI for leaderboard {leaderboardId} via {provider.ProviderName}");
            bool result = provider.DisplayLeaderboardUI(leaderboardId);

            if (!result)
            {
                LastError = provider.LastError;
                LogHelper.Warning("LeaderboardsService",
                    $"Failed to display UI for leaderboard {leaderboardId}: {LastError}");
            }

            return result;
        }

        /// <summary>
        /// Checks if the service is initialized
        /// </summary>
        /// <returns> True if initialized, false otherwise </returns>
        private bool CheckInitialized()
        {
            if (!IsInitialized)
            {
                LogHelper.Error("LeaderboardsService", "LeaderboardsService is not initialized");
                LastError = "LeaderboardsService is not initialized";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Loads all leaderboard definitions from all providers
        /// </summary>
        /// <param name="callback"> Callback when loading completes </param>
        private void LoadLeaderboardDefinitions(Action callback = null)
        {
            if (_providers.Count == 0)
            {
                LogHelper.Warning("LeaderboardsService", "No providers to load leaderboard definitions from");
                callback?.Invoke();
                return;
            }

            int providersToQuery = _providers.Count(p => p.IsAvailable);
            if (providersToQuery == 0)
            {
                LogHelper.Warning("LeaderboardsService", "No available providers to load leaderboard definitions from");
                callback?.Invoke();
                return;
            }

            int providersQueried = 0;

            foreach (var provider in _providers.Where(p => p.IsAvailable))
                provider.QueryLeaderboardDefinitions((definitions, success) =>
                {
                    lock (_lock)
                    {
                        providersQueried++;

                        if (success && definitions != null && definitions.Count > 0)
                        {
                            LogHelper.Info("LeaderboardsService",
                                $"Loaded {definitions.Count} leaderboard definitions from {provider.ProviderName}");

                            // Add definitions to cache
                            foreach (var definition in definitions)
                            {
                                // Make sure the provider name is set
                                definition.ProviderName = provider.ProviderName;
                                _leaderboardDefinitions[definition.LeaderboardId] = definition;

                                // Create mapping from stat to leaderboard if needed
                                if (!string.IsNullOrEmpty(definition.StatName))
                                {
                                    if (!_statToLeaderboardMappings.TryGetValue(definition.StatName, out List<LeaderboardStatInfo> mappings))
                                    {
                                        mappings = new List<LeaderboardStatInfo>();
                                        _statToLeaderboardMappings[definition.StatName] = mappings;
                                    }

                                    // Add a new mapping if one doesn't already exist
                                    if (!mappings.Any(m => m.LeaderboardId == definition.LeaderboardId))
                                        mappings.Add(new LeaderboardStatInfo(definition.LeaderboardId,
                                            definition.StatName));
                                }
                            }
                        }
                        else
                        {
                            LogHelper.Warning("LeaderboardsService",
                                $"Failed to load leaderboard definitions from {provider.ProviderName}: {provider.LastError}");
                        }

                        if (providersQueried >= providersToQuery)
                        {
                            LogHelper.Info("LeaderboardsService",
                                $"Finished loading leaderboard definitions. Total: {_leaderboardDefinitions.Count}");
                            callback?.Invoke();
                        }
                    }
                });
        }

        /// <summary>
        /// Gets the appropriate provider for a leaderboard
        /// </summary>
        /// <param name="leaderboardId"> ID of the leaderboard </param>
        /// <returns> The provider instance, or null if not found or not available </returns>
        private ILeaderboardsProvider GetProviderForLeaderboard(string leaderboardId)
        {
            lock (_lock)
            {
                // Try to find the leaderboard definition
                if (_leaderboardDefinitions.TryGetValue(leaderboardId, out var definition))
                {
                    // Get the provider for this definition
                    var provider = _providers.FirstOrDefault(p =>
                        p.ProviderName == definition.ProviderName && p.IsAvailable);

                    if (provider != null) return provider;
                }

                // If definition not found or provider not available, try the first available provider
                return _providers.FirstOrDefault(p => p.IsAvailable);
            }
        }

        /// <summary>
        /// Handles a stat update, submitting scores to any associated leaderboards
        /// </summary>
        /// <param name="statName"> Name of the stat that was updated </param>
        /// <param name="statValue"> New value of the stat </param>
        public void HandleStatUpdate(string statName, double statValue)
        {
            if (!IsInitialized || string.IsNullOrEmpty(statName))
                return;

            lock (_lock)
            {
                // Check if we have any leaderboard mappings for this stat
                if (_statToLeaderboardMappings.TryGetValue(statName, out List<LeaderboardStatInfo> mappings))
                    foreach (var mapping in mappings.Where(m => m.AutoSubmit))
                        if (_leaderboardDefinitions.TryGetValue(mapping.LeaderboardId, out var definition))
                        {
                            // Transform stat value to leaderboard score
                            long score = mapping.TransformStatToScore(statValue);

                            // Submit the score to the leaderboard
                            LogHelper.Debug("LeaderboardsService",
                                $"Auto-submitting score {score} to leaderboard {mapping.LeaderboardId} for stat {statName}");
                            SubmitScore(mapping.LeaderboardId, score);
                        }
            }
        }

        /// <summary>
        /// Registers a mapping from a stat to a leaderboard
        /// </summary>
        /// <param name="statInfo"> Stat mapping information </param>
        public void RegisterStatMapping(LeaderboardStatInfo statInfo)
        {
            if (statInfo == null || string.IsNullOrEmpty(statInfo.StatName) ||
                string.IsNullOrEmpty(statInfo.LeaderboardId))
            {
                LogHelper.Error("LeaderboardsService", "Cannot register invalid stat mapping");
                return;
            }

            lock (_lock)
            {
                if (!_statToLeaderboardMappings.TryGetValue(statInfo.StatName, out List<LeaderboardStatInfo> mappings))
                {
                    mappings = new List<LeaderboardStatInfo>();
                    _statToLeaderboardMappings[statInfo.StatName] = mappings;
                }

                // Replace existing mapping or add new one
                int existingIndex = mappings.FindIndex(m => m.LeaderboardId == statInfo.LeaderboardId);
                if (existingIndex >= 0)
                    mappings[existingIndex] = statInfo;
                else
                    mappings.Add(statInfo);

                LogHelper.Debug("LeaderboardsService",
                    $"Registered mapping from stat {statInfo.StatName} to leaderboard {statInfo.LeaderboardId}");
            }
        }
    }
}