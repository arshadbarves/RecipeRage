using System;
using System.Collections.Generic;
using System.Linq;
using RecipeRage.Modules.Lobbies.Data;
using RecipeRage.Modules.Lobbies.Interfaces;
using RecipeRage.Modules.Logging;

namespace RecipeRage.Modules.Lobbies.Core
{
    /// <summary>
    /// Main service for lobby management, implementing the ILobbyService interface.
    /// </summary>
    public class LobbyService : ILobbyService
    {
        #region Constructor

        /// <summary>
        /// Create a new LobbyService
        /// </summary>
        public LobbyService()
        {
            LogHelper.Info("LobbyService", "Creating LobbyService");
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the lobby service
        /// </summary>
        /// <param name="onComplete"> Callback invoked when initialization is complete </param>
        public void Initialize(Action<bool> onComplete = null)
        {
            if (IsInitialized)
            {
                LogHelper.Warning("LobbyService", "LobbyService is already initialized");
                onComplete?.Invoke(true);
                return;
            }

            LogHelper.Info("LobbyService", "Initializing LobbyService");

            if (_providers.Count == 0)
            {
                LastError = "No lobby providers registered";
                LogHelper.Error("LobbyService", LastError);
                onComplete?.Invoke(false);
                return;
            }

            int providersInitialized = 0;
            int totalProviders = _providers.Count;

            // Initialize each provider
            foreach (var provider in _providers)
            {
                provider.OnLobbyCreated += HandleLobbyCreated;
                provider.OnLobbyJoined += HandleLobbyJoined;
                provider.OnLobbyLeft += HandleLobbyLeft;
                provider.OnLobbyUpdated += HandleLobbyUpdated;
                provider.OnMemberJoined += HandleMemberJoined;
                provider.OnMemberLeft += HandleMemberLeft;
                provider.OnMemberUpdated += HandleMemberUpdated;
                provider.OnInviteReceived += HandleInviteReceived;

                provider.Initialize(success =>
                {
                    if (success)
                    {
                        LogHelper.Info("LobbyService", $"Provider {provider.ProviderName} initialized successfully");
                        providersInitialized++;
                    }
                    else
                    {
                        LogHelper.Warning("LobbyService",
                            $"Provider {provider.ProviderName} failed to initialize: {provider.LastError}");
                    }

                    // If all providers are initialized or failed, complete initialization
                    if (providersInitialized + (totalProviders - providersInitialized) == totalProviders)
                    {
                        bool success = providersInitialized > 0;
                        IsInitialized = success;

                        if (success)
                        {
                            LogHelper.Info("LobbyService",
                                $"LobbyService initialized with {providersInitialized} providers");

                            // Set the first available provider as active
                            var availableProvider = _providers.FirstOrDefault(p => p.IsAvailable);
                            if (availableProvider != null)
                            {
                                _activeProvider = availableProvider;
                                LogHelper.Info("LobbyService",
                                    $"Using {_activeProvider.ProviderName} as the active provider");
                            }
                            else
                            {
                                LogHelper.Warning("LobbyService", "No available providers found after initialization");
                            }
                        }
                        else
                        {
                            LastError = "Failed to initialize any providers";
                            LogHelper.Error("LobbyService", LastError);
                        }

                        onComplete?.Invoke(success);
                    }
                });
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Event triggered when a lobby is created
        /// </summary>
        public event Action<LobbyInfo> OnLobbyCreated;

        /// <summary>
        /// Event triggered when a lobby is joined
        /// </summary>
        public event Action<LobbyInfo> OnLobbyJoined;

        /// <summary>
        /// Event triggered when a lobby is left
        /// </summary>
        public event Action<string> OnLobbyLeft;

        /// <summary>
        /// Event triggered when a lobby is updated
        /// </summary>
        public event Action<LobbyInfo> OnLobbyUpdated;

        /// <summary>
        /// Event triggered when a member joins the lobby
        /// </summary>
        public event Action<LobbyMember> OnMemberJoined;

        /// <summary>
        /// Event triggered when a member leaves the lobby
        /// </summary>
        public event Action<LobbyMember> OnMemberLeft;

        /// <summary>
        /// Event triggered when a member is updated (e.g., status change)
        /// </summary>
        public event Action<LobbyMember> OnMemberUpdated;

        /// <summary>
        /// Event triggered when lobby search results are received
        /// </summary>
        public event Action<List<LobbySearchResult>> OnLobbySearchCompleted;

        /// <summary>
        /// Event triggered when an invite is received
        /// </summary>
        public event Action<string, string> OnInviteReceived;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the currently active lobby, or null if not in a lobby
        /// </summary>
        public LobbyInfo CurrentLobby { get; private set; }

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

        private readonly List<ILobbyProvider> _providers = new List<ILobbyProvider>();
        private ILobbyProvider _activeProvider;
        private bool _isJoiningLobby;
        private bool _isLeavingLobby;
        private readonly object _lockObject = new object();

        #endregion

        #region Core Lobby Operations

        /// <summary>
        /// Create a new lobby
        /// </summary>
        /// <param name="settings"> Settings for the lobby </param>
        /// <param name="onComplete"> Callback invoked when creation is complete </param>
        public void CreateLobby(LobbySettings settings, Action<bool, LobbyInfo> onComplete = null)
        {
            if (!CheckInitialized("CreateLobby", onComplete)) return;

            if (CurrentLobby != null)
            {
                LastError = "Already in a lobby. Leave the current lobby before creating a new one.";
                LogHelper.Warning("LobbyService", LastError);
                onComplete?.Invoke(false, null);
                return;
            }

            if (settings == null)
            {
                LastError = "Lobby settings cannot be null";
                LogHelper.Error("LobbyService", LastError);
                onComplete?.Invoke(false, null);
                return;
            }

            LogHelper.Info("LobbyService",
                $"Creating lobby with name: {settings.Name}, max players: {settings.MaxPlayers}");

            _activeProvider.CreateLobby(settings, (success, lobbyInfo) =>
            {
                if (success)
                {
                    LogHelper.Info("LobbyService",
                        $"Lobby created successfully: {lobbyInfo.Name} ({lobbyInfo.LobbyId})");
                    CurrentLobby = lobbyInfo;
                    // The event will be raised by the event handler
                }
                else
                {
                    LastError = $"Failed to create lobby: {_activeProvider.LastError}";
                    LogHelper.Error("LobbyService", LastError);
                }

                onComplete?.Invoke(success, lobbyInfo);
            });
        }

        /// <summary>
        /// Join an existing lobby by its ID
        /// </summary>
        /// <param name="lobbyId"> ID of the lobby to join </param>
        /// <param name="onComplete"> Callback invoked when join is complete </param>
        public void JoinLobby(string lobbyId, Action<bool, LobbyInfo> onComplete = null)
        {
            if (!CheckInitialized("JoinLobby", onComplete)) return;

            if (string.IsNullOrEmpty(lobbyId))
            {
                LastError = "Lobby ID cannot be null or empty";
                LogHelper.Error("LobbyService", LastError);
                onComplete?.Invoke(false, null);
                return;
            }

            if (CurrentLobby != null)
            {
                LastError = "Already in a lobby. Leave the current lobby before joining a new one.";
                LogHelper.Warning("LobbyService", LastError);
                onComplete?.Invoke(false, null);
                return;
            }

            if (_isJoiningLobby)
            {
                LastError = "Already joining a lobby";
                LogHelper.Warning("LobbyService", LastError);
                onComplete?.Invoke(false, null);
                return;
            }

            LogHelper.Info("LobbyService", $"Joining lobby with ID: {lobbyId}");
            _isJoiningLobby = true;

            _activeProvider.JoinLobby(lobbyId, (success, lobbyInfo) =>
            {
                _isJoiningLobby = false;

                if (success)
                {
                    LogHelper.Info("LobbyService",
                        $"Joined lobby successfully: {lobbyInfo.Name} ({lobbyInfo.LobbyId})");
                    CurrentLobby = lobbyInfo;
                    // The event will be raised by the event handler
                }
                else
                {
                    LastError = $"Failed to join lobby: {_activeProvider.LastError}";
                    LogHelper.Error("LobbyService", LastError);
                }

                onComplete?.Invoke(success, lobbyInfo);
            });
        }

        /// <summary>
        /// Join an existing lobby using a join token (e.g., from an invite)
        /// </summary>
        /// <param name="joinToken"> Token to join the lobby </param>
        /// <param name="onComplete"> Callback invoked when join is complete </param>
        public void JoinLobbyByToken(string joinToken, Action<bool, LobbyInfo> onComplete = null)
        {
            if (!CheckInitialized("JoinLobbyByToken", onComplete)) return;

            if (string.IsNullOrEmpty(joinToken))
            {
                LastError = "Join token cannot be null or empty";
                LogHelper.Error("LobbyService", LastError);
                onComplete?.Invoke(false, null);
                return;
            }

            if (CurrentLobby != null)
            {
                LastError = "Already in a lobby. Leave the current lobby before joining a new one.";
                LogHelper.Warning("LobbyService", LastError);
                onComplete?.Invoke(false, null);
                return;
            }

            if (_isJoiningLobby)
            {
                LastError = "Already joining a lobby";
                LogHelper.Warning("LobbyService", LastError);
                onComplete?.Invoke(false, null);
                return;
            }

            LogHelper.Info("LobbyService", "Joining lobby with token");
            _isJoiningLobby = true;

            _activeProvider.JoinLobbyByToken(joinToken, (success, lobbyInfo) =>
            {
                _isJoiningLobby = false;

                if (success)
                {
                    LogHelper.Info("LobbyService",
                        $"Joined lobby successfully using token: {lobbyInfo.Name} ({lobbyInfo.LobbyId})");
                    CurrentLobby = lobbyInfo;
                    // The event will be raised by the event handler
                }
                else
                {
                    LastError = $"Failed to join lobby using token: {_activeProvider.LastError}";
                    LogHelper.Error("LobbyService", LastError);
                }

                onComplete?.Invoke(success, lobbyInfo);
            });
        }

        /// <summary>
        /// Leave the current lobby
        /// </summary>
        /// <param name="onComplete"> Callback invoked when leave is complete </param>
        public void LeaveLobby(Action<bool> onComplete = null)
        {
            if (!CheckInitialized("LeaveLobby", onComplete)) return;

            if (CurrentLobby == null)
            {
                LastError = "Not in a lobby";
                LogHelper.Warning("LobbyService", LastError);
                onComplete?.Invoke(false);
                return;
            }

            if (_isLeavingLobby)
            {
                LastError = "Already leaving a lobby";
                LogHelper.Warning("LobbyService", LastError);
                onComplete?.Invoke(false);
                return;
            }

            string lobbyId = CurrentLobby.LobbyId;
            LogHelper.Info("LobbyService", $"Leaving lobby: {CurrentLobby.Name} ({lobbyId})");
            _isLeavingLobby = true;

            _activeProvider.LeaveLobby(lobbyId, success =>
            {
                _isLeavingLobby = false;

                if (success)
                {
                    LogHelper.Info("LobbyService", $"Left lobby successfully: {lobbyId}");
                    // The event will be raised by the event handler
                    // CurrentLobby is set to null in the event handler
                }
                else
                {
                    LastError = $"Failed to leave lobby: {_activeProvider.LastError}";
                    LogHelper.Error("LobbyService", LastError);

                    // Force the CurrentLobby to null if the provider failed but we want to clean up
                    CurrentLobby = null;
                }

                onComplete?.Invoke(success);
            });
        }

        /// <summary>
        /// Refresh the current lobby information
        /// </summary>
        /// <param name="onComplete"> Callback invoked when refresh is complete </param>
        public void RefreshLobby(Action<bool> onComplete = null)
        {
            if (!CheckInitialized("RefreshLobby", onComplete)) return;

            if (CurrentLobby == null)
            {
                LastError = "Not in a lobby";
                LogHelper.Warning("LobbyService", LastError);
                onComplete?.Invoke(false);
                return;
            }

            string lobbyId = CurrentLobby.LobbyId;
            LogHelper.Debug("LobbyService", $"Refreshing lobby: {CurrentLobby.Name} ({lobbyId})");

            _activeProvider.RefreshLobby(lobbyId, (success, lobbyInfo) =>
            {
                if (success)
                {
                    LogHelper.Debug("LobbyService",
                        $"Refreshed lobby successfully: {lobbyInfo.Name} ({lobbyInfo.LobbyId})");
                    CurrentLobby = lobbyInfo;
                    OnLobbyUpdated?.Invoke(lobbyInfo);
                }
                else
                {
                    LastError = $"Failed to refresh lobby: {_activeProvider.LastError}";
                    LogHelper.Warning("LobbyService", LastError);
                }

                onComplete?.Invoke(success);
            });
        }

        #endregion

        #region Lobby and Player Management

        /// <summary>
        /// Update lobby attributes
        /// </summary>
        /// <param name="attributes"> The attributes to update </param>
        /// <param name="onComplete"> Callback invoked when update is complete </param>
        public void UpdateLobbyAttributes(Dictionary<string, string> attributes, Action<bool> onComplete = null)
        {
            if (!CheckInitialized("UpdateLobbyAttributes", onComplete)) return;

            if (CurrentLobby == null)
            {
                LastError = "Not in a lobby";
                LogHelper.Warning("LobbyService", LastError);
                onComplete?.Invoke(false);
                return;
            }

            if (attributes == null || attributes.Count == 0)
            {
                LastError = "Attributes cannot be null or empty";
                LogHelper.Warning("LobbyService", LastError);
                onComplete?.Invoke(false);
                return;
            }

            string lobbyId = CurrentLobby.LobbyId;
            LogHelper.Debug("LobbyService", $"Updating attributes for lobby {lobbyId}");

            _activeProvider.UpdateLobbyAttributes(lobbyId, attributes, success =>
            {
                if (success)
                {
                    LogHelper.Debug("LobbyService", $"Updated lobby attributes successfully for {lobbyId}");

                    // Update local attributes
                    foreach (KeyValuePair<string, string> kvp in attributes) CurrentLobby.Attributes[kvp.Key] = kvp.Value;

                    CurrentLobby.LastUpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    LastError = $"Failed to update lobby attributes: {_activeProvider.LastError}";
                    LogHelper.Warning("LobbyService", LastError);
                }

                onComplete?.Invoke(success);
            });
        }

        /// <summary>
        /// Update a player's attributes
        /// </summary>
        /// <param name="attributes"> The attributes to update </param>
        /// <param name="onComplete"> Callback invoked when update is complete </param>
        public void UpdatePlayerAttributes(Dictionary<string, string> attributes, Action<bool> onComplete = null)
        {
            if (!CheckInitialized("UpdatePlayerAttributes", onComplete)) return;

            if (CurrentLobby == null)
            {
                LastError = "Not in a lobby";
                LogHelper.Warning("LobbyService", LastError);
                onComplete?.Invoke(false);
                return;
            }

            if (attributes == null || attributes.Count == 0)
            {
                LastError = "Attributes cannot be null or empty";
                LogHelper.Warning("LobbyService", LastError);
                onComplete?.Invoke(false);
                return;
            }

            string lobbyId = CurrentLobby.LobbyId;
            LogHelper.Debug("LobbyService", $"Updating player attributes in lobby {lobbyId}");

            _activeProvider.UpdatePlayerAttributes(lobbyId, attributes, success =>
            {
                if (success)
                {
                    LogHelper.Debug("LobbyService", $"Updated player attributes successfully in lobby {lobbyId}");

                    // This will be handled by the member updated event
                }
                else
                {
                    LastError = $"Failed to update player attributes: {_activeProvider.LastError}";
                    LogHelper.Warning("LobbyService", LastError);
                }

                onComplete?.Invoke(success);
            });
        }

        /// <summary>
        /// Update the lobby settings (max players, permissions, etc.)
        /// </summary>
        /// <param name="settings"> The new settings </param>
        /// <param name="onComplete"> Callback invoked when update is complete </param>
        public void UpdateLobbySettings(LobbySettings settings, Action<bool> onComplete = null)
        {
            if (!CheckInitialized("UpdateLobbySettings", onComplete)) return;

            if (CurrentLobby == null)
            {
                LastError = "Not in a lobby";
                LogHelper.Warning("LobbyService", LastError);
                onComplete?.Invoke(false);
                return;
            }

            if (!CurrentLobby.IsOwner(CurrentLobby.OwnerId))
            {
                LastError = "Only the lobby owner can update lobby settings";
                LogHelper.Warning("LobbyService", LastError);
                onComplete?.Invoke(false);
                return;
            }

            if (settings == null)
            {
                LastError = "Settings cannot be null";
                LogHelper.Error("LobbyService", LastError);
                onComplete?.Invoke(false);
                return;
            }

            string lobbyId = CurrentLobby.LobbyId;
            LogHelper.Info("LobbyService", $"Updating settings for lobby {lobbyId}");

            _activeProvider.UpdateLobbySettings(lobbyId, settings, success =>
            {
                if (success)
                {
                    LogHelper.Info("LobbyService", $"Updated lobby settings successfully for {lobbyId}");

                    // The updated lobby will be received through the lobby updated event
                }
                else
                {
                    LastError = $"Failed to update lobby settings: {_activeProvider.LastError}";
                    LogHelper.Error("LobbyService", LastError);
                }

                onComplete?.Invoke(success);
            });
        }

        /// <summary>
        /// Kick a player from the lobby
        /// </summary>
        /// <param name="playerId"> ID of the player to kick </param>
        /// <param name="onComplete"> Callback invoked when kick is complete </param>
        public void KickPlayer(string playerId, Action<bool> onComplete = null)
        {
            if (!CheckInitialized("KickPlayer", onComplete)) return;

            if (CurrentLobby == null)
            {
                LastError = "Not in a lobby";
                LogHelper.Warning("LobbyService", LastError);
                onComplete?.Invoke(false);
                return;
            }

            if (!CurrentLobby.IsOwner(CurrentLobby.OwnerId))
            {
                LastError = "Only the lobby owner can kick players";
                LogHelper.Warning("LobbyService", LastError);
                onComplete?.Invoke(false);
                return;
            }

            if (string.IsNullOrEmpty(playerId))
            {
                LastError = "Player ID cannot be null or empty";
                LogHelper.Error("LobbyService", LastError);
                onComplete?.Invoke(false);
                return;
            }

            if (playerId == CurrentLobby.OwnerId)
            {
                LastError = "Cannot kick the lobby owner";
                LogHelper.Warning("LobbyService", LastError);
                onComplete?.Invoke(false);
                return;
            }

            if (!CurrentLobby.IsMember(playerId))
            {
                LastError = "Player is not a member of the lobby";
                LogHelper.Warning("LobbyService", LastError);
                onComplete?.Invoke(false);
                return;
            }

            string lobbyId = CurrentLobby.LobbyId;
            LogHelper.Info("LobbyService", $"Kicking player {playerId} from lobby {lobbyId}");

            _activeProvider.KickPlayer(lobbyId, playerId, success =>
            {
                if (success)
                {
                    LogHelper.Info("LobbyService", $"Kicked player {playerId} successfully from lobby {lobbyId}");

                    // Player removal will be handled by the member left event
                }
                else
                {
                    LastError = $"Failed to kick player: {_activeProvider.LastError}";
                    LogHelper.Error("LobbyService", LastError);
                }

                onComplete?.Invoke(success);
            });
        }

        /// <summary>
        /// Promote a player to lobby owner
        /// </summary>
        /// <param name="playerId"> ID of the player to promote </param>
        /// <param name="onComplete"> Callback invoked when promotion is complete </param>
        public void PromotePlayer(string playerId, Action<bool> onComplete = null)
        {
            if (!CheckInitialized("PromotePlayer", onComplete)) return;

            if (CurrentLobby == null)
            {
                LastError = "Not in a lobby";
                LogHelper.Warning("LobbyService", LastError);
                onComplete?.Invoke(false);
                return;
            }

            if (!CurrentLobby.IsOwner(CurrentLobby.OwnerId))
            {
                LastError = "Only the lobby owner can promote players";
                LogHelper.Warning("LobbyService", LastError);
                onComplete?.Invoke(false);
                return;
            }

            if (string.IsNullOrEmpty(playerId))
            {
                LastError = "Player ID cannot be null or empty";
                LogHelper.Error("LobbyService", LastError);
                onComplete?.Invoke(false);
                return;
            }

            if (playerId == CurrentLobby.OwnerId)
            {
                LastError = "Player is already the lobby owner";
                LogHelper.Warning("LobbyService", LastError);
                onComplete?.Invoke(false);
                return;
            }

            if (!CurrentLobby.IsMember(playerId))
            {
                LastError = "Player is not a member of the lobby";
                LogHelper.Warning("LobbyService", LastError);
                onComplete?.Invoke(false);
                return;
            }

            string lobbyId = CurrentLobby.LobbyId;
            LogHelper.Info("LobbyService", $"Promoting player {playerId} to owner in lobby {lobbyId}");

            _activeProvider.PromotePlayer(lobbyId, playerId, success =>
            {
                if (success)
                {
                    LogHelper.Info("LobbyService", $"Promoted player {playerId} successfully in lobby {lobbyId}");

                    // Update the owner in the local lobby
                    string currentOwnerId = CurrentLobby.OwnerId;
                    CurrentLobby.OwnerId = playerId;

                    // Update the owner flags in the member list
                    var currentOwnerMember = CurrentLobby.GetMember(currentOwnerId);
                    if (currentOwnerMember != null) currentOwnerMember.IsOwner = false;

                    var newOwnerMember = CurrentLobby.GetMember(playerId);
                    if (newOwnerMember != null)
                    {
                        newOwnerMember.IsOwner = true;
                        OnMemberUpdated?.Invoke(newOwnerMember);
                    }

                    // Trigger a lobby update
                    OnLobbyUpdated?.Invoke(CurrentLobby);
                }
                else
                {
                    LastError = $"Failed to promote player: {_activeProvider.LastError}";
                    LogHelper.Error("LobbyService", LastError);
                }

                onComplete?.Invoke(success);
            });
        }

        #endregion

        #region Search and Invite Methods

        /// <summary>
        /// Search for lobbies matching the given criteria
        /// </summary>
        /// <param name="searchOptions"> Search options to use </param>
        /// <param name="onComplete"> Callback invoked when search is complete </param>
        public void SearchLobbies(LobbySearchOptions searchOptions,
            Action<bool, List<LobbySearchResult>> onComplete = null)
        {
            if (!CheckInitialized("SearchLobbies", onComplete)) return;

            if (searchOptions == null)
            {
                searchOptions = new LobbySearchOptions();
                LogHelper.Warning("LobbyService", "SearchOptions was null, using default values");
            }

            LogHelper.Info("LobbyService",
                $"Searching for lobbies with maxResults={searchOptions.MaxResults}, joinableOnly={searchOptions.JoinableOnly}");

            _activeProvider.SearchLobbies(searchOptions, (success, results) =>
            {
                if (success)
                {
                    LogHelper.Info("LobbyService", $"Found {results.Count} lobbies");
                    OnLobbySearchCompleted?.Invoke(results);
                }
                else
                {
                    LastError = $"Failed to search for lobbies: {_activeProvider.LastError}";
                    LogHelper.Error("LobbyService", LastError);
                    OnLobbySearchCompleted?.Invoke(new List<LobbySearchResult>());
                }

                onComplete?.Invoke(success, results);
            });
        }

        /// <summary>
        /// Send an invite to a player
        /// </summary>
        /// <param name="playerId"> ID of the player to invite </param>
        /// <param name="onComplete"> Callback invoked when invitation is sent </param>
        public void SendInvite(string playerId, Action<bool> onComplete = null)
        {
            if (!CheckInitialized("SendInvite", onComplete)) return;

            if (CurrentLobby == null)
            {
                LastError = "Not in a lobby";
                LogHelper.Warning("LobbyService", LastError);
                onComplete?.Invoke(false);
                return;
            }

            if (string.IsNullOrEmpty(playerId))
            {
                LastError = "Player ID cannot be null or empty";
                LogHelper.Error("LobbyService", LastError);
                onComplete?.Invoke(false);
                return;
            }

            if (CurrentLobby.IsMember(playerId))
            {
                LastError = "Player is already a member of the lobby";
                LogHelper.Warning("LobbyService", LastError);
                onComplete?.Invoke(false);
                return;
            }

            string lobbyId = CurrentLobby.LobbyId;
            LogHelper.Info("LobbyService", $"Sending invite to player {playerId} for lobby {lobbyId}");

            _activeProvider.SendInvite(lobbyId, playerId, success =>
            {
                if (success)
                {
                    LogHelper.Info("LobbyService",
                        $"Sent invite to player {playerId} successfully for lobby {lobbyId}");
                }
                else
                {
                    LastError = $"Failed to send invite: {_activeProvider.LastError}";
                    LogHelper.Error("LobbyService", LastError);
                }

                onComplete?.Invoke(success);
            });
        }

        /// <summary>
        /// Accept an invite
        /// </summary>
        /// <param name="inviteId"> ID of the invite to accept </param>
        /// <param name="onComplete"> Callback invoked when acceptance is complete </param>
        public void AcceptInvite(string inviteId, Action<bool, LobbyInfo> onComplete = null)
        {
            if (!CheckInitialized("AcceptInvite", onComplete)) return;

            if (string.IsNullOrEmpty(inviteId))
            {
                LastError = "Invite ID cannot be null or empty";
                LogHelper.Error("LobbyService", LastError);
                onComplete?.Invoke(false, null);
                return;
            }

            if (CurrentLobby != null)
            {
                LastError = "Already in a lobby. Leave the current lobby before accepting an invite.";
                LogHelper.Warning("LobbyService", LastError);
                onComplete?.Invoke(false, null);
                return;
            }

            LogHelper.Info("LobbyService", $"Accepting invite {inviteId}");

            _activeProvider.AcceptInvite(inviteId, (success, lobbyInfo) =>
            {
                if (success)
                {
                    LogHelper.Info("LobbyService",
                        $"Accepted invite successfully: {inviteId}, joined lobby {lobbyInfo.Name} ({lobbyInfo.LobbyId})");
                    CurrentLobby = lobbyInfo;
                    // The event will be raised by the event handler
                }
                else
                {
                    LastError = $"Failed to accept invite: {_activeProvider.LastError}";
                    LogHelper.Error("LobbyService", LastError);
                }

                onComplete?.Invoke(success, lobbyInfo);
            });
        }

        /// <summary>
        /// Reject an invite
        /// </summary>
        /// <param name="inviteId"> ID of the invite to reject </param>
        /// <param name="onComplete"> Callback invoked when rejection is complete </param>
        public void RejectInvite(string inviteId, Action<bool> onComplete = null)
        {
            if (!CheckInitialized("RejectInvite", onComplete)) return;

            if (string.IsNullOrEmpty(inviteId))
            {
                LastError = "Invite ID cannot be null or empty";
                LogHelper.Error("LobbyService", LastError);
                onComplete?.Invoke(false);
                return;
            }

            LogHelper.Info("LobbyService", $"Rejecting invite {inviteId}");

            _activeProvider.RejectInvite(inviteId, success =>
            {
                if (success)
                {
                    LogHelper.Info("LobbyService", $"Rejected invite successfully: {inviteId}");
                }
                else
                {
                    LastError = $"Failed to reject invite: {_activeProvider.LastError}";
                    LogHelper.Warning("LobbyService", LastError);
                }

                onComplete?.Invoke(success);
            });
        }

        /// <summary>
        /// Get the list of pending invites
        /// </summary>
        /// <returns> List of pending invites </returns>
        public List<string> GetPendingInvites()
        {
            if (!IsInitialized || _activeProvider == null || !_activeProvider.IsAvailable)
            {
                LogHelper.Warning("LobbyService",
                    "Cannot get pending invites: service not initialized or no available provider");
                return new List<string>();
            }

            return _activeProvider.GetPendingInvites();
        }

        #endregion

        #region Provider Management

        /// <summary>
        /// Add a provider for lobby services
        /// </summary>
        /// <param name="provider"> The provider to add </param>
        /// <returns> True if the provider was added successfully </returns>
        public bool AddProvider(ILobbyProvider provider)
        {
            if (provider == null)
            {
                LogHelper.Error("LobbyService", "Cannot add null provider");
                return false;
            }

            string providerName = provider.ProviderName;
            if (string.IsNullOrEmpty(providerName))
            {
                LogHelper.Error("LobbyService", "Provider has null or empty name");
                return false;
            }

            lock (_lockObject)
            {
                // Check if the provider is already registered
                if (_providers.Any(p => p.ProviderName == providerName))
                {
                    LogHelper.Warning("LobbyService", $"Provider {providerName} is already registered");
                    return false;
                }

                LogHelper.Info("LobbyService", $"Adding provider: {providerName}");
                _providers.Add(provider);

                // If this is the first provider, or we don't have an active provider, set this as active
                if (_activeProvider == null && provider.IsAvailable)
                {
                    _activeProvider = provider;
                    LogHelper.Info("LobbyService", $"Set {providerName} as active provider");
                }

                return true;
            }
        }

        /// <summary>
        /// Get a provider by name
        /// </summary>
        /// <param name="providerName"> Name of the provider </param>
        /// <returns> The provider, or null if not found </returns>
        public ILobbyProvider GetProvider(string providerName)
        {
            if (string.IsNullOrEmpty(providerName))
            {
                LogHelper.Error("LobbyService", "Cannot get provider with null or empty name");
                return null;
            }

            lock (_lockObject)
            {
                var provider = _providers.FirstOrDefault(p => p.ProviderName == providerName);
                if (provider == null) LogHelper.Warning("LobbyService", $"Provider {providerName} not found");

                return provider;
            }
        }

        /// <summary>
        /// Set a provider as active
        /// </summary>
        /// <param name="providerName"> Name of the provider to set as active </param>
        /// <returns> True if the provider was set as active successfully </returns>
        private bool SetActiveProvider(string providerName)
        {
            if (string.IsNullOrEmpty(providerName))
            {
                LogHelper.Error("LobbyService", "Cannot set active provider with null or empty name");
                return false;
            }

            lock (_lockObject)
            {
                var provider = _providers.FirstOrDefault(p => p.ProviderName == providerName);
                if (provider == null)
                {
                    LogHelper.Warning("LobbyService", $"Provider {providerName} not found");
                    return false;
                }

                if (!provider.IsAvailable)
                {
                    LogHelper.Warning("LobbyService", $"Provider {providerName} is not available");
                    return false;
                }

                // Cannot change provider while in a lobby
                if (CurrentLobby != null)
                {
                    LogHelper.Warning("LobbyService", "Cannot change provider while in a lobby");
                    return false;
                }

                _activeProvider = provider;
                LogHelper.Info("LobbyService", $"Set {providerName} as active provider");
                return true;
            }
        }

        /// <summary>
        /// Remove a provider
        /// </summary>
        /// <param name="providerName"> Name of the provider to remove </param>
        /// <returns> True if the provider was removed successfully </returns>
        private bool RemoveProvider(string providerName)
        {
            if (string.IsNullOrEmpty(providerName))
            {
                LogHelper.Error("LobbyService", "Cannot remove provider with null or empty name");
                return false;
            }

            lock (_lockObject)
            {
                var provider = _providers.FirstOrDefault(p => p.ProviderName == providerName);
                if (provider == null)
                {
                    LogHelper.Warning("LobbyService", $"Provider {providerName} not found");
                    return false;
                }

                // Cannot remove active provider while in a lobby
                if (_activeProvider == provider && CurrentLobby != null)
                {
                    LogHelper.Warning("LobbyService", "Cannot remove active provider while in a lobby");
                    return false;
                }

                // Unregister events
                provider.OnLobbyCreated -= HandleLobbyCreated;
                provider.OnLobbyJoined -= HandleLobbyJoined;
                provider.OnLobbyLeft -= HandleLobbyLeft;
                provider.OnLobbyUpdated -= HandleLobbyUpdated;
                provider.OnMemberJoined -= HandleMemberJoined;
                provider.OnMemberLeft -= HandleMemberLeft;
                provider.OnMemberUpdated -= HandleMemberUpdated;
                provider.OnInviteReceived -= HandleInviteReceived;

                _providers.Remove(provider);
                LogHelper.Info("LobbyService", $"Removed provider: {providerName}");

                // If we removed the active provider, set another one as active if available
                if (_activeProvider == provider)
                {
                    _activeProvider = _providers.FirstOrDefault(p => p.IsAvailable);
                    if (_activeProvider != null)
                        LogHelper.Info("LobbyService", $"Set {_activeProvider.ProviderName} as active provider");
                    else
                        LogHelper.Warning("LobbyService", "No available providers after removal");
                }

                return true;
            }
        }

        #endregion

        #region Event Handlers

        private void HandleLobbyCreated(LobbyInfo lobby)
        {
            LogHelper.Info("LobbyService", $"Lobby created: {lobby.Name} ({lobby.LobbyId})");
            CurrentLobby = lobby;
            OnLobbyCreated?.Invoke(lobby);
        }

        private void HandleLobbyJoined(LobbyInfo lobby)
        {
            LogHelper.Info("LobbyService", $"Joined lobby: {lobby.Name} ({lobby.LobbyId})");
            CurrentLobby = lobby;
            _isJoiningLobby = false;
            OnLobbyJoined?.Invoke(lobby);
        }

        private void HandleLobbyLeft(string lobbyId)
        {
            LogHelper.Info("LobbyService", $"Left lobby: {lobbyId}");
            if (CurrentLobby != null && CurrentLobby.LobbyId == lobbyId) CurrentLobby = null;
            _isLeavingLobby = false;
            OnLobbyLeft?.Invoke(lobbyId);
        }

        private void HandleLobbyUpdated(LobbyInfo lobby)
        {
            LogHelper.Debug("LobbyService", $"Lobby updated: {lobby.Name} ({lobby.LobbyId})");
            if (CurrentLobby != null && CurrentLobby.LobbyId == lobby.LobbyId) CurrentLobby = lobby;
            OnLobbyUpdated?.Invoke(lobby);
        }

        private void HandleMemberJoined(LobbyMember member)
        {
            LogHelper.Info("LobbyService", $"Member joined: {member.DisplayName} ({member.PlayerId})");
            if (CurrentLobby != null && !CurrentLobby.IsMember(member.PlayerId))
            {
                CurrentLobby.Members.Add(member);
                CurrentLobby.CurrentPlayers = CurrentLobby.Members.Count;
            }

            OnMemberJoined?.Invoke(member);
        }

        private void HandleMemberLeft(LobbyMember member)
        {
            LogHelper.Info("LobbyService", $"Member left: {member.DisplayName} ({member.PlayerId})");
            if (CurrentLobby != null)
            {
                var existingMember = CurrentLobby.GetMember(member.PlayerId);
                if (existingMember != null)
                {
                    CurrentLobby.Members.Remove(existingMember);
                    CurrentLobby.CurrentPlayers = CurrentLobby.Members.Count;
                }
            }

            OnMemberLeft?.Invoke(member);
        }

        private void HandleMemberUpdated(LobbyMember member)
        {
            LogHelper.Debug("LobbyService", $"Member updated: {member.DisplayName} ({member.PlayerId})");
            if (CurrentLobby != null)
            {
                var existingMember = CurrentLobby.GetMember(member.PlayerId);
                if (existingMember != null)
                {
                    // Update the member properties
                    var index = CurrentLobby.Members.IndexOf(existingMember);
                    if (index != -1) CurrentLobby.Members[index] = member;
                }
            }

            OnMemberUpdated?.Invoke(member);
        }

        private void HandleInviteReceived(string inviteId, string senderId)
        {
            LogHelper.Info("LobbyService", $"Invite received: {inviteId} from {senderId}");
            OnInviteReceived?.Invoke(inviteId, senderId);
        }

        #endregion

        #region Helper Methods

        private bool CheckInitialized<T>(string methodName, Action<bool, T> callback = null)
        {
            if (!IsInitialized)
            {
                LastError = "LobbyService is not initialized";
                LogHelper.Error("LobbyService", $"{methodName} failed: {LastError}");
                callback?.Invoke(false, default);
                return false;
            }

            if (_activeProvider == null || !_activeProvider.IsAvailable)
            {
                LastError = "No available lobby provider";
                LogHelper.Error("LobbyService", $"{methodName} failed: {LastError}");
                callback?.Invoke(false, default);
                return false;
            }

            return true;
        }

        private bool CheckInitialized(string methodName, Action<bool> callback = null)
        {
            if (!IsInitialized)
            {
                LastError = "LobbyService is not initialized";
                LogHelper.Error("LobbyService", $"{methodName} failed: {LastError}");
                callback?.Invoke(false);
                return false;
            }

            if (_activeProvider == null || !_activeProvider.IsAvailable)
            {
                LastError = "No available lobby provider";
                LogHelper.Error("LobbyService", $"{methodName} failed: {LastError}");
                callback?.Invoke(false);
                return false;
            }

            return true;
        }

        #endregion
    }
}