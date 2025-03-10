using System;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeRage.Lobbies
{
    /// <summary>
    /// Interface for lobby provider implementations (EOS, Steam, etc.)
    /// </summary>
    public interface ILobbyProvider
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
        /// Initializes the lobby provider
        /// </summary>
        /// <param name="callback">Callback when initialization completes</param>
        void Initialize(Action<bool> callback);

        /// <summary>
        /// Creates a new lobby
        /// </summary>
        /// <param name="lobbyConfig">Configuration for the new lobby</param>
        /// <param name="callback">Callback with the created lobby</param>
        void CreateLobby(LobbyConfig lobbyConfig, Action<Lobby, bool> callback);

        /// <summary>
        /// Joins an existing lobby by ID
        /// </summary>
        /// <param name="lobbyId">ID of the lobby to join</param>
        /// <param name="callback">Callback with the joined lobby</param>
        void JoinLobbyById(string lobbyId, Action<Lobby, bool> callback);

        /// <summary>
        /// Joins a lobby from an invitation
        /// </summary>
        /// <param name="inviteId">ID of the invitation</param>
        /// <param name="callback">Callback with the joined lobby</param>
        void JoinLobbyByInvite(string inviteId, Action<Lobby, bool> callback);

        /// <summary>
        /// Leaves the current lobby
        /// </summary>
        /// <param name="callback">Callback indicating success or failure</param>
        void LeaveLobby(string lobbyId, Action<bool> callback);

        /// <summary>
        /// Searches for lobbies with specific criteria
        /// </summary>
        /// <param name="searchParams">Parameters for the search</param>
        /// <param name="callback">Callback with the search results</param>
        void SearchLobbies(LobbySearchParams searchParams, Action<List<Lobby>, bool> callback);

        /// <summary>
        /// Updates the attributes of a lobby
        /// </summary>
        /// <param name="lobbyId">ID of the lobby to update</param>
        /// <param name="attributes">Attributes to update</param>
        /// <param name="callback">Callback indicating success or failure</param>
        void UpdateLobbyAttributes(string lobbyId, Dictionary<string, LobbyAttribute> attributes, Action<bool> callback);

        /// <summary>
        /// Updates the attributes of the local user in a lobby
        /// </summary>
        /// <param name="lobbyId">ID of the lobby</param>
        /// <param name="attributes">Attributes to update</param>
        /// <param name="callback">Callback indicating success or failure</param>
        void UpdateMemberAttributes(string lobbyId, Dictionary<string, LobbyAttribute> attributes, Action<bool> callback);

        /// <summary>
        /// Gets the current lobby the user is in
        /// </summary>
        /// <param name="callback">Callback with the current lobby</param>
        void GetCurrentLobby(Action<Lobby> callback);

        /// <summary>
        /// Kicks a member from a lobby
        /// </summary>
        /// <param name="lobbyId">ID of the lobby</param>
        /// <param name="memberId">ID of the member to kick</param>
        /// <param name="callback">Callback indicating success or failure</param>
        void KickMember(string lobbyId, string memberId, Action<bool> callback);

        /// <summary>
        /// Promotes a member to owner of the lobby
        /// </summary>
        /// <param name="lobbyId">ID of the lobby</param>
        /// <param name="memberId">ID of the member to promote</param>
        /// <param name="callback">Callback indicating success or failure</param>
        void PromoteMember(string lobbyId, string memberId, Action<bool> callback);

        /// <summary>
        /// Sends an invitation to join a lobby to another user
        /// </summary>
        /// <param name="lobbyId">ID of the lobby</param>
        /// <param name="userId">ID of the user to invite</param>
        /// <param name="callback">Callback indicating success or failure</param>
        void SendInvite(string lobbyId, string userId, Action<bool> callback);

        /// <summary>
        /// Rejects an invitation to join a lobby
        /// </summary>
        /// <param name="inviteId">ID of the invitation</param>
        /// <param name="callback">Callback indicating success or failure</param>
        void RejectInvite(string inviteId, Action<bool> callback);

        /// <summary>
        /// Queries for pending invitations
        /// </summary>
        /// <param name="callback">Callback with the list of invitations</param>
        void GetInvites(Action<List<LobbyInvite>, bool> callback);

        /// <summary>
        /// Enables or disables voice chat in the lobby
        /// </summary>
        /// <param name="lobbyId">ID of the lobby</param>
        /// <param name="enabled">Whether to enable or disable voice chat</param>
        /// <param name="callback">Callback indicating success or failure</param>
        void SetVoiceChatEnabled(string lobbyId, bool enabled, Action<bool> callback);

        /// <summary>
        /// Mutes or unmutes a user in voice chat
        /// </summary>
        /// <param name="lobbyId">ID of the lobby</param>
        /// <param name="userId">ID of the user to mute or unmute</param>
        /// <param name="mute">Whether to mute or unmute the user</param>
        /// <param name="callback">Callback indicating success or failure</param>
        void MutePlayer(string lobbyId, string userId, bool mute, Action<bool> callback);
    }
} 