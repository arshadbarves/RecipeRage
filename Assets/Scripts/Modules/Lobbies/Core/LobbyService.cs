using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RecipeRage.Logging;

namespace RecipeRage.Lobbies
{
    /// <summary>
    /// Core service for managing lobbies
    /// </summary>
    public class LobbyService : ILobbyService
    {
        private const string LOG_TAG = "LobbyService";

        // List of available providers
        private readonly List<ILobbyProvider> _providers = new List<ILobbyProvider>();

        // Current active provider
        private ILobbyProvider _activeProvider;

        // Initialization status
        private bool _isInitialized = false;

        // Last error message
        private string _lastError = string.Empty;

        // Event declarations
        public event Action<Lobby> OnLobbyCreated;
        public event Action<Lobby> OnLobbyJoined;
        public event Action<string> OnLobbyLeft;
        public event Action<Lobby> OnLobbyUpdated;
        public event Action<LobbyMember, string> OnMemberJoined;
        public event Action<LobbyMember, string> OnMemberLeft;
        public event Action<LobbyMember, string> OnMemberUpdated;
        public event Action<LobbyInvite> OnInviteReceived;
        public event Action<string, bool> OnVoiceChatStatusChanged;
        public event Action<LobbyMember, bool, string> OnPlayerTalking;

        // Properties
        public bool IsInitialized => _isInitialized;
        public string LastError => _lastError;

        /// <summary>
        /// Creates a new instance of the LobbyService
        /// </summary>
        public LobbyService()
        {
            LogHelper.Debug(LOG_TAG, "LobbyService created");
        }

        /// <summary>
        /// Initializes the lobby service
        /// </summary>
        /// <param name="callback">Callback when initialization completes</param>
        public void Initialize(Action<bool> callback)
        {
            if (_isInitialized)
            {
                LogHelper.Warning(LOG_TAG, "LobbyService already initialized");
                callback?.Invoke(true);
                return;
            }

            LogHelper.Info(LOG_TAG, "Initializing LobbyService");

            if (_providers.Count == 0)
            {
                _lastError = "No lobby providers registered";
                LogHelper.Error(LOG_TAG, _lastError);
                callback?.Invoke(false);
                return;
            }

            // Initialize all providers
            var pendingInitCount = _providers.Count;
            var allSuccess = true;

            foreach (var provider in _providers)
            {
                provider.Initialize((success) =>
                {
                    if (!success)
                    {
                        LogHelper.Error(LOG_TAG, $"Failed to initialize provider: {provider.ProviderName}");
                        allSuccess = false;
                    }

                    pendingInitCount--;

                    // When all providers are initialized, set the active provider
                    if (pendingInitCount == 0)
                    {
                        if (allSuccess)
                        {
                            _activeProvider = _providers.FirstOrDefault(p => p.IsAvailable);
                            
                            if (_activeProvider == null)
                            {
                                _lastError = "No available lobby providers found";
                                LogHelper.Error(LOG_TAG, _lastError);
                                _isInitialized = false;
                                callback?.Invoke(false);
                                return;
                            }

                            LogHelper.Info(LOG_TAG, $"Using {_activeProvider.ProviderName} as active lobby provider");
                            _isInitialized = true;
                            callback?.Invoke(true);
                        }
                        else
                        {
                            _lastError = "One or more providers failed to initialize";
                            LogHelper.Error(LOG_TAG, _lastError);
                            _isInitialized = false;
                            callback?.Invoke(false);
                        }
                    }
                });
            }
        }

        /// <summary>
        /// Adds a lobby provider
        /// </summary>
        /// <param name="provider">The provider to add</param>
        /// <returns>True if the provider was added successfully</returns>
        public bool AddProvider(ILobbyProvider provider)
        {
            if (provider == null)
            {
                LogHelper.Error(LOG_TAG, "Cannot add null provider");
                return false;
            }

            // Check if provider already exists
            if (_providers.Any(p => p.ProviderName == provider.ProviderName))
            {
                LogHelper.Warning(LOG_TAG, $"Provider {provider.ProviderName} already exists");
                return false;
            }

            _providers.Add(provider);
            LogHelper.Info(LOG_TAG, $"Added provider: {provider.ProviderName}");
            return true;
        }

        /// <summary>
        /// Gets a lobby provider by name
        /// </summary>
        /// <param name="providerName">Name of the provider</param>
        /// <returns>The provider if found, null otherwise</returns>
        public ILobbyProvider GetProvider(string providerName)
        {
            return _providers.FirstOrDefault(p => p.ProviderName == providerName);
        }

        /// <summary>
        /// Creates a new lobby
        /// </summary>
        /// <param name="lobbyConfig">Configuration for the new lobby</param>
        /// <param name="callback">Callback with the created lobby</param>
        public void CreateLobby(LobbyConfig lobbyConfig, Action<Lobby, bool> callback)
        {
            if (!CheckInitialized(callback))
                return;

            ILobbyProvider provider = GetProviderForOperation(lobbyConfig.ProviderName);
            if (provider == null)
            {
                _lastError = $"No available provider found for creating lobby";
                LogHelper.Error(LOG_TAG, _lastError);
                callback?.Invoke(null, false);
                return;
            }

            LogHelper.Info(LOG_TAG, $"Creating lobby with provider: {provider.ProviderName}");
            provider.CreateLobby(lobbyConfig, (lobby, success) =>
            {
                if (success)
                {
                    LogHelper.Info(LOG_TAG, $"Successfully created lobby: {lobby.Id}");
                    OnLobbyCreated?.Invoke(lobby);
                }
                else
                {
                    _lastError = provider.LastError;
                    LogHelper.Error(LOG_TAG, $"Failed to create lobby: {_lastError}");
                }

                callback?.Invoke(lobby, success);
            });
        }

        /// <summary>
        /// Joins an existing lobby by ID
        /// </summary>
        /// <param name="lobbyId">ID of the lobby to join</param>
        /// <param name="callback">Callback with the joined lobby</param>
        public void JoinLobbyById(string lobbyId, Action<Lobby, bool> callback)
        {
            if (!CheckInitialized(callback))
                return;

            if (string.IsNullOrEmpty(lobbyId))
            {
                _lastError = "Lobby ID cannot be empty";
                LogHelper.Error(LOG_TAG, _lastError);
                callback?.Invoke(null, false);
                return;
            }

            // Use active provider since we don't know which provider the lobby belongs to
            LogHelper.Info(LOG_TAG, $"Joining lobby {lobbyId} with provider: {_activeProvider.ProviderName}");
            _activeProvider.JoinLobbyById(lobbyId, (lobby, success) =>
            {
                if (success)
                {
                    LogHelper.Info(LOG_TAG, $"Successfully joined lobby: {lobby.Id}");
                    OnLobbyJoined?.Invoke(lobby);
                }
                else
                {
                    _lastError = _activeProvider.LastError;
                    LogHelper.Error(LOG_TAG, $"Failed to join lobby: {_lastError}");
                }

                callback?.Invoke(lobby, success);
            });
        }

        /// <summary>
        /// Joins a lobby from an invitation
        /// </summary>
        /// <param name="inviteId">ID of the invitation</param>
        /// <param name="callback">Callback with the joined lobby</param>
        public void JoinLobbyByInvite(string inviteId, Action<Lobby, bool> callback)
        {
            if (!CheckInitialized(callback))
                return;

            if (string.IsNullOrEmpty(inviteId))
            {
                _lastError = "Invite ID cannot be empty";
                LogHelper.Error(LOG_TAG, _lastError);
                callback?.Invoke(null, false);
                return;
            }

            // Use active provider since we don't know which provider the invite belongs to
            LogHelper.Info(LOG_TAG, $"Joining lobby from invite {inviteId} with provider: {_activeProvider.ProviderName}");
            _activeProvider.JoinLobbyByInvite(inviteId, (lobby, success) =>
            {
                if (success)
                {
                    LogHelper.Info(LOG_TAG, $"Successfully joined lobby from invite: {lobby.Id}");
                    OnLobbyJoined?.Invoke(lobby);
                }
                else
                {
                    _lastError = _activeProvider.LastError;
                    LogHelper.Error(LOG_TAG, $"Failed to join lobby from invite: {_lastError}");
                }

                callback?.Invoke(lobby, success);
            });
        }

        /// <summary>
        /// Leaves the current lobby
        /// </summary>
        /// <param name="lobbyId">ID of the lobby to leave</param>
        /// <param name="callback">Callback indicating success or failure</param>
        public void LeaveLobby(string lobbyId, Action<bool> callback)
        {
            if (!CheckInitialized(callback))
                return;

            if (string.IsNullOrEmpty(lobbyId))
            {
                _lastError = "Lobby ID cannot be empty";
                LogHelper.Error(LOG_TAG, _lastError);
                callback?.Invoke(false);
                return;
            }

            // Use active provider 
            LogHelper.Info(LOG_TAG, $"Leaving lobby {lobbyId} with provider: {_activeProvider.ProviderName}");
            _activeProvider.LeaveLobby(lobbyId, (success) =>
            {
                if (success)
                {
                    LogHelper.Info(LOG_TAG, $"Successfully left lobby: {lobbyId}");
                    OnLobbyLeft?.Invoke(lobbyId);
                }
                else
                {
                    _lastError = _activeProvider.LastError;
                    LogHelper.Error(LOG_TAG, $"Failed to leave lobby: {_lastError}");
                }

                callback?.Invoke(success);
            });
        }

        /// <summary>
        /// Searches for lobbies with specific criteria
        /// </summary>
        /// <param name="searchParams">Parameters for the search</param>
        /// <param name="callback">Callback with the search results</param>
        public void SearchLobbies(LobbySearchParams searchParams, Action<List<Lobby>, bool> callback)
        {
            if (!CheckInitialized(callback))
                return;

            ILobbyProvider provider = GetProviderForOperation(searchParams.ProviderName);
            if (provider == null)
            {
                _lastError = $"No available provider found for searching lobbies";
                LogHelper.Error(LOG_TAG, _lastError);
                callback?.Invoke(new List<Lobby>(), false);
                return;
            }

            LogHelper.Info(LOG_TAG, $"Searching lobbies with provider: {provider.ProviderName}");
            provider.SearchLobbies(searchParams, (lobbies, success) =>
            {
                if (success)
                {
                    LogHelper.Info(LOG_TAG, $"Successfully found {lobbies.Count} lobbies");
                }
                else
                {
                    _lastError = provider.LastError;
                    LogHelper.Error(LOG_TAG, $"Failed to search lobbies: {_lastError}");
                }

                callback?.Invoke(lobbies, success);
            });
        }

        /// <summary>
        /// Updates the attributes of a lobby
        /// </summary>
        /// <param name="lobbyId">ID of the lobby to update</param>
        /// <param name="attributes">Attributes to update</param>
        /// <param name="callback">Callback indicating success or failure</param>
        public void UpdateLobbyAttributes(string lobbyId, Dictionary<string, LobbyAttribute> attributes, Action<bool> callback)
        {
            if (!CheckInitialized(callback))
                return;

            if (string.IsNullOrEmpty(lobbyId))
            {
                _lastError = "Lobby ID cannot be empty";
                LogHelper.Error(LOG_TAG, _lastError);
                callback?.Invoke(false);
                return;
            }

            // Use active provider
            LogHelper.Info(LOG_TAG, $"Updating lobby attributes for {lobbyId} with provider: {_activeProvider.ProviderName}");
            _activeProvider.UpdateLobbyAttributes(lobbyId, attributes, (success) =>
            {
                if (success)
                {
                    LogHelper.Info(LOG_TAG, $"Successfully updated lobby attributes: {lobbyId}");
                    
                    // Get the updated lobby to trigger the OnLobbyUpdated event
                    _activeProvider.GetCurrentLobby((lobby) =>
                    {
                        if (lobby != null && lobby.Id == lobbyId)
                        {
                            OnLobbyUpdated?.Invoke(lobby);
                        }
                    });
                }
                else
                {
                    _lastError = _activeProvider.LastError;
                    LogHelper.Error(LOG_TAG, $"Failed to update lobby attributes: {_lastError}");
                }

                callback?.Invoke(success);
            });
        }

        /// <summary>
        /// Updates the attributes of the local user in a lobby
        /// </summary>
        /// <param name="lobbyId">ID of the lobby</param>
        /// <param name="attributes">Attributes to update</param>
        /// <param name="callback">Callback indicating success or failure</param>
        public void UpdateMemberAttributes(string lobbyId, Dictionary<string, LobbyAttribute> attributes, Action<bool> callback)
        {
            if (!CheckInitialized(callback))
                return;

            if (string.IsNullOrEmpty(lobbyId))
            {
                _lastError = "Lobby ID cannot be empty";
                LogHelper.Error(LOG_TAG, _lastError);
                callback?.Invoke(false);
                return;
            }

            // Use active provider
            LogHelper.Info(LOG_TAG, $"Updating member attributes in lobby {lobbyId} with provider: {_activeProvider.ProviderName}");
            _activeProvider.UpdateMemberAttributes(lobbyId, attributes, (success) =>
            {
                if (success)
                {
                    LogHelper.Info(LOG_TAG, $"Successfully updated member attributes in lobby: {lobbyId}");
                    
                    // Get the updated lobby to find the local member
                    _activeProvider.GetCurrentLobby((lobby) =>
                    {
                        if (lobby != null && lobby.Id == lobbyId)
                        {
                            var localMember = lobby.Members.FirstOrDefault(m => m.IsLocalMember);
                            if (localMember != null)
                            {
                                OnMemberUpdated?.Invoke(localMember, lobbyId);
                            }
                        }
                    });
                }
                else
                {
                    _lastError = _activeProvider.LastError;
                    LogHelper.Error(LOG_TAG, $"Failed to update member attributes: {_lastError}");
                }

                callback?.Invoke(success);
            });
        }

        /// <summary>
        /// Gets the current lobby the user is in
        /// </summary>
        /// <param name="callback">Callback with the current lobby</param>
        public void GetCurrentLobby(Action<Lobby> callback)
        {
            if (!CheckInitialized(callback))
                return;

            // Use active provider
            LogHelper.Debug(LOG_TAG, $"Getting current lobby with provider: {_activeProvider.ProviderName}");
            _activeProvider.GetCurrentLobby(callback);
        }

        /// <summary>
        /// Kicks a member from a lobby
        /// </summary>
        /// <param name="lobbyId">ID of the lobby</param>
        /// <param name="memberId">ID of the member to kick</param>
        /// <param name="callback">Callback indicating success or failure</param>
        public void KickMember(string lobbyId, string memberId, Action<bool> callback)
        {
            if (!CheckInitialized(callback))
                return;

            if (string.IsNullOrEmpty(lobbyId))
            {
                _lastError = "Lobby ID cannot be empty";
                LogHelper.Error(LOG_TAG, _lastError);
                callback?.Invoke(false);
                return;
            }

            if (string.IsNullOrEmpty(memberId))
            {
                _lastError = "Member ID cannot be empty";
                LogHelper.Error(LOG_TAG, _lastError);
                callback?.Invoke(false);
                return;
            }

            // Use active provider
            LogHelper.Info(LOG_TAG, $"Kicking member {memberId} from lobby {lobbyId} with provider: {_activeProvider.ProviderName}");
            _activeProvider.KickMember(lobbyId, memberId, (success) =>
            {
                if (success)
                {
                    LogHelper.Info(LOG_TAG, $"Successfully kicked member {memberId} from lobby: {lobbyId}");
                    
                    // Get the updated lobby to trigger the OnMemberLeft event
                    _activeProvider.GetCurrentLobby((lobby) =>
                    {
                        if (lobby != null && lobby.Id == lobbyId)
                        {
                            // Create a temporary member for the event
                            var member = new LobbyMember
                            {
                                Id = memberId,
                                Status = LobbyMemberStatus.Kicked
                            };

                            OnMemberLeft?.Invoke(member, lobbyId);
                        }
                    });
                }
                else
                {
                    _lastError = _activeProvider.LastError;
                    LogHelper.Error(LOG_TAG, $"Failed to kick member: {_lastError}");
                }

                callback?.Invoke(success);
            });
        }

        /// <summary>
        /// Promotes a member to owner of the lobby
        /// </summary>
        /// <param name="lobbyId">ID of the lobby</param>
        /// <param name="memberId">ID of the member to promote</param>
        /// <param name="callback">Callback indicating success or failure</param>
        public void PromoteMember(string lobbyId, string memberId, Action<bool> callback)
        {
            if (!CheckInitialized(callback))
                return;

            if (string.IsNullOrEmpty(lobbyId))
            {
                _lastError = "Lobby ID cannot be empty";
                LogHelper.Error(LOG_TAG, _lastError);
                callback?.Invoke(false);
                return;
            }

            if (string.IsNullOrEmpty(memberId))
            {
                _lastError = "Member ID cannot be empty";
                LogHelper.Error(LOG_TAG, _lastError);
                callback?.Invoke(false);
                return;
            }

            // Use active provider
            LogHelper.Info(LOG_TAG, $"Promoting member {memberId} in lobby {lobbyId} with provider: {_activeProvider.ProviderName}");
            _activeProvider.PromoteMember(lobbyId, memberId, (success) =>
            {
                if (success)
                {
                    LogHelper.Info(LOG_TAG, $"Successfully promoted member {memberId} in lobby: {lobbyId}");
                    
                    // Get the updated lobby to trigger the OnLobbyUpdated event
                    _activeProvider.GetCurrentLobby((lobby) =>
                    {
                        if (lobby != null && lobby.Id == lobbyId)
                        {
                            OnLobbyUpdated?.Invoke(lobby);
                        }
                    });
                }
                else
                {
                    _lastError = _activeProvider.LastError;
                    LogHelper.Error(LOG_TAG, $"Failed to promote member: {_lastError}");
                }

                callback?.Invoke(success);
            });
        }

        /// <summary>
        /// Sends an invitation to join a lobby to another user
        /// </summary>
        /// <param name="lobbyId">ID of the lobby</param>
        /// <param name="userId">ID of the user to invite</param>
        /// <param name="callback">Callback indicating success or failure</param>
        public void SendInvite(string lobbyId, string userId, Action<bool> callback)
        {
            if (!CheckInitialized(callback))
                return;

            if (string.IsNullOrEmpty(lobbyId))
            {
                _lastError = "Lobby ID cannot be empty";
                LogHelper.Error(LOG_TAG, _lastError);
                callback?.Invoke(false);
                return;
            }

            if (string.IsNullOrEmpty(userId))
            {
                _lastError = "User ID cannot be empty";
                LogHelper.Error(LOG_TAG, _lastError);
                callback?.Invoke(false);
                return;
            }

            // Use active provider
            LogHelper.Info(LOG_TAG, $"Sending invite to user {userId} for lobby {lobbyId} with provider: {_activeProvider.ProviderName}");
            _activeProvider.SendInvite(lobbyId, userId, (success) =>
            {
                if (success)
                {
                    LogHelper.Info(LOG_TAG, $"Successfully sent invite to user {userId} for lobby: {lobbyId}");
                }
                else
                {
                    _lastError = _activeProvider.LastError;
                    LogHelper.Error(LOG_TAG, $"Failed to send invite: {_lastError}");
                }

                callback?.Invoke(success);
            });
        }

        /// <summary>
        /// Rejects an invitation to join a lobby
        /// </summary>
        /// <param name="inviteId">ID of the invitation</param>
        /// <param name="callback">Callback indicating success or failure</param>
        public void RejectInvite(string inviteId, Action<bool> callback)
        {
            if (!CheckInitialized(callback))
                return;

            if (string.IsNullOrEmpty(inviteId))
            {
                _lastError = "Invite ID cannot be empty";
                LogHelper.Error(LOG_TAG, _lastError);
                callback?.Invoke(false);
                return;
            }

            // Use active provider
            LogHelper.Info(LOG_TAG, $"Rejecting invite {inviteId} with provider: {_activeProvider.ProviderName}");
            _activeProvider.RejectInvite(inviteId, (success) =>
            {
                if (success)
                {
                    LogHelper.Info(LOG_TAG, $"Successfully rejected invite: {inviteId}");
                }
                else
                {
                    _lastError = _activeProvider.LastError;
                    LogHelper.Error(LOG_TAG, $"Failed to reject invite: {_lastError}");
                }

                callback?.Invoke(success);
            });
        }

        /// <summary>
        /// Queries for pending invitations
        /// </summary>
        /// <param name="callback">Callback with the list of invitations</param>
        public void GetInvites(Action<List<LobbyInvite>, bool> callback)
        {
            if (!CheckInitialized(callback))
                return;

            // Use active provider
            LogHelper.Info(LOG_TAG, $"Getting invites with provider: {_activeProvider.ProviderName}");
            _activeProvider.GetInvites((invites, success) =>
            {
                if (success)
                {
                    LogHelper.Info(LOG_TAG, $"Successfully got {invites.Count} invites");
                    
                    // Register event handlers for these invites
                    foreach (var invite in invites)
                    {
                        OnInviteReceived?.Invoke(invite);
                    }
                }
                else
                {
                    _lastError = _activeProvider.LastError;
                    LogHelper.Error(LOG_TAG, $"Failed to get invites: {_lastError}");
                }

                callback?.Invoke(invites, success);
            });
        }

        /// <summary>
        /// Enables or disables voice chat in the lobby
        /// </summary>
        /// <param name="lobbyId">ID of the lobby</param>
        /// <param name="enabled">Whether to enable or disable voice chat</param>
        /// <param name="callback">Callback indicating success or failure</param>
        public void SetVoiceChatEnabled(string lobbyId, bool enabled, Action<bool> callback)
        {
            if (!CheckInitialized(callback))
                return;

            if (string.IsNullOrEmpty(lobbyId))
            {
                _lastError = "Lobby ID cannot be empty";
                LogHelper.Error(LOG_TAG, _lastError);
                callback?.Invoke(false);
                return;
            }

            // Use active provider
            LogHelper.Info(LOG_TAG, $"{(enabled ? "Enabling" : "Disabling")} voice chat in lobby {lobbyId} with provider: {_activeProvider.ProviderName}");
            _activeProvider.SetVoiceChatEnabled(lobbyId, enabled, (success) =>
            {
                if (success)
                {
                    LogHelper.Info(LOG_TAG, $"Successfully {(enabled ? "enabled" : "disabled")} voice chat in lobby: {lobbyId}");
                    OnVoiceChatStatusChanged?.Invoke(lobbyId, enabled);
                }
                else
                {
                    _lastError = _activeProvider.LastError;
                    LogHelper.Error(LOG_TAG, $"Failed to {(enabled ? "enable" : "disable")} voice chat: {_lastError}");
                }

                callback?.Invoke(success);
            });
        }

        /// <summary>
        /// Mutes or unmutes a user in voice chat
        /// </summary>
        /// <param name="lobbyId">ID of the lobby</param>
        /// <param name="userId">ID of the user to mute or unmute</param>
        /// <param name="mute">Whether to mute or unmute the user</param>
        /// <param name="callback">Callback indicating success or failure</param>
        public void MutePlayer(string lobbyId, string userId, bool mute, Action<bool> callback)
        {
            if (!CheckInitialized(callback))
                return;

            if (string.IsNullOrEmpty(lobbyId))
            {
                _lastError = "Lobby ID cannot be empty";
                LogHelper.Error(LOG_TAG, _lastError);
                callback?.Invoke(false);
                return;
            }

            if (string.IsNullOrEmpty(userId))
            {
                _lastError = "User ID cannot be empty";
                LogHelper.Error(LOG_TAG, _lastError);
                callback?.Invoke(false);
                return;
            }

            // Use active provider
            LogHelper.Info(LOG_TAG, $"{(mute ? "Muting" : "Unmuting")} player {userId} in lobby {lobbyId} with provider: {_activeProvider.ProviderName}");
            _activeProvider.MutePlayer(lobbyId, userId, mute, (success) =>
            {
                if (success)
                {
                    LogHelper.Info(LOG_TAG, $"Successfully {(mute ? "muted" : "unmuted")} player {userId} in lobby: {lobbyId}");

                    // Get the updated lobby to find the muted player
                    _activeProvider.GetCurrentLobby((lobby) =>
                    {
                        if (lobby != null && lobby.Id == lobbyId)
                        {
                            var member = lobby.Members.FirstOrDefault(m => m.Id == userId);
                            if (member != null)
                            {
                                // Update the player's voice state
                                member.VoiceState.IsLocalMuted = mute;
                                OnPlayerTalking?.Invoke(member, false, lobbyId);
                            }
                        }
                    });
                }
                else
                {
                    _lastError = _activeProvider.LastError;
                    LogHelper.Error(LOG_TAG, $"Failed to {(mute ? "mute" : "unmute")} player: {_lastError}");
                }

                callback?.Invoke(success);
            });
        }

        #region Private Helper Methods

        /// <summary>
        /// Checks if the service is initialized
        /// </summary>
        /// <param name="callback">Callback to invoke if not initialized</param>
        /// <returns>True if initialized, false otherwise</returns>
        private bool CheckInitialized<T>(Action<T, bool> callback) where T : class
        {
            if (!_isInitialized)
            {
                _lastError = "LobbyService is not initialized";
                LogHelper.Error(LOG_TAG, _lastError);
                callback?.Invoke(null, false);
                return false;
            }

            if (_activeProvider == null || !_activeProvider.IsAvailable)
            {
                _lastError = "No active lobby provider available";
                LogHelper.Error(LOG_TAG, _lastError);
                callback?.Invoke(null, false);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if the service is initialized
        /// </summary>
        /// <param name="callback">Callback to invoke if not initialized</param>
        /// <returns>True if initialized, false otherwise</returns>
        private bool CheckInitialized(Action<bool> callback)
        {
            if (!_isInitialized)
            {
                _lastError = "LobbyService is not initialized";
                LogHelper.Error(LOG_TAG, _lastError);
                callback?.Invoke(false);
                return false;
            }

            if (_activeProvider == null || !_activeProvider.IsAvailable)
            {
                _lastError = "No active lobby provider available";
                LogHelper.Error(LOG_TAG, _lastError);
                callback?.Invoke(false);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if the service is initialized
        /// </summary>
        /// <param name="callback">Callback to invoke if not initialized</param>
        /// <returns>True if initialized, false otherwise</returns>
        private bool CheckInitialized<T>(Action<List<T>, bool> callback)
        {
            if (!_isInitialized)
            {
                _lastError = "LobbyService is not initialized";
                LogHelper.Error(LOG_TAG, _lastError);
                callback?.Invoke(new List<T>(), false);
                return false;
            }

            if (_activeProvider == null || !_activeProvider.IsAvailable)
            {
                _lastError = "No active lobby provider available";
                LogHelper.Error(LOG_TAG, _lastError);
                callback?.Invoke(new List<T>(), false);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if the service is initialized
        /// </summary>
        /// <param name="callback">Callback to invoke if not initialized</param>
        /// <returns>True if initialized, false otherwise</returns>
        private bool CheckInitialized(Action<Lobby> callback)
        {
            if (!_isInitialized)
            {
                _lastError = "LobbyService is not initialized";
                LogHelper.Error(LOG_TAG, _lastError);
                callback?.Invoke(null);
                return false;
            }

            if (_activeProvider == null || !_activeProvider.IsAvailable)
            {
                _lastError = "No active lobby provider available";
                LogHelper.Error(LOG_TAG, _lastError);
                callback?.Invoke(null);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the provider to use for an operation
        /// </summary>
        /// <param name="providerName">Name of the provider to use, or null to use active provider</param>
        /// <returns>The provider to use</returns>
        private ILobbyProvider GetProviderForOperation(string providerName)
        {
            if (string.IsNullOrEmpty(providerName))
                return _activeProvider;

            var provider = GetProvider(providerName);
            if (provider != null && provider.IsAvailable)
                return provider;

            return _activeProvider;
        }

        #endregion
    }
} 