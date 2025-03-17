using System;
using System.Collections.Generic;
using RecipeRage.Modules.Lobbies.Data;

namespace RecipeRage.Modules.Lobbies.Interfaces
{
    /// <summary>
    /// Interface for lobby service, handling lobby creation, joining, and management.
    /// </summary>
    public interface ILobbyService
    {
        /// <summary>
        /// Gets the currently active lobby, or null if not in a lobby
        /// </summary>
        LobbyInfo CurrentLobby { get; }

        /// <summary>
        /// Gets whether the service is initialized
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Gets the last error message from the service
        /// </summary>
        string LastError { get; }

        /// <summary>
        /// Event triggered when a lobby is created
        /// </summary>
        event Action<LobbyInfo> OnLobbyCreated;

        /// <summary>
        /// Event triggered when a lobby is joined
        /// </summary>
        event Action<LobbyInfo> OnLobbyJoined;

        /// <summary>
        /// Event triggered when a lobby is left
        /// </summary>
        event Action<string> OnLobbyLeft;

        /// <summary>
        /// Event triggered when a lobby is updated
        /// </summary>
        event Action<LobbyInfo> OnLobbyUpdated;

        /// <summary>
        /// Event triggered when a member joins the lobby
        /// </summary>
        event Action<LobbyMember> OnMemberJoined;

        /// <summary>
        /// Event triggered when a member leaves the lobby
        /// </summary>
        event Action<LobbyMember> OnMemberLeft;

        /// <summary>
        /// Event triggered when a member is updated (e.g., status change)
        /// </summary>
        event Action<LobbyMember> OnMemberUpdated;

        /// <summary>
        /// Event triggered when lobby search results are received
        /// </summary>
        event Action<List<LobbySearchResult>> OnLobbySearchCompleted;

        /// <summary>
        /// Event triggered when an invite is received
        /// </summary>
        event Action<string, string> OnInviteReceived;

        /// <summary>
        /// Initialize the lobby service
        /// </summary>
        /// <param name="onComplete"> Callback invoked when initialization is complete </param>
        void Initialize(Action<bool> onComplete = null);

        /// <summary>
        /// Create a new lobby
        /// </summary>
        /// <param name="settings"> Settings for the lobby </param>
        /// <param name="onComplete"> Callback invoked when creation is complete </param>
        void CreateLobby(LobbySettings settings, Action<bool, LobbyInfo> onComplete = null);

        /// <summary>
        /// Join an existing lobby by its ID
        /// </summary>
        /// <param name="lobbyId"> ID of the lobby to join </param>
        /// <param name="onComplete"> Callback invoked when join is complete </param>
        void JoinLobby(string lobbyId, Action<bool, LobbyInfo> onComplete = null);

        /// <summary>
        /// Join an existing lobby using a join token (e.g., from an invite)
        /// </summary>
        /// <param name="joinToken"> Token to join the lobby </param>
        /// <param name="onComplete"> Callback invoked when join is complete </param>
        void JoinLobbyByToken(string joinToken, Action<bool, LobbyInfo> onComplete = null);

        /// <summary>
        /// Leave the current lobby
        /// </summary>
        /// <param name="onComplete"> Callback invoked when leave is complete </param>
        void LeaveLobby(Action<bool> onComplete = null);

        /// <summary>
        /// Update lobby attributes
        /// </summary>
        /// <param name="attributes"> The attributes to update </param>
        /// <param name="onComplete"> Callback invoked when update is complete </param>
        void UpdateLobbyAttributes(Dictionary<string, string> attributes, Action<bool> onComplete = null);

        /// <summary>
        /// Update a player's attributes
        /// </summary>
        /// <param name="attributes"> The attributes to update </param>
        /// <param name="onComplete"> Callback invoked when update is complete </param>
        void UpdatePlayerAttributes(Dictionary<string, string> attributes, Action<bool> onComplete = null);

        /// <summary>
        /// Update the lobby settings (max players, permissions, etc.)
        /// </summary>
        /// <param name="settings"> The new settings </param>
        /// <param name="onComplete"> Callback invoked when update is complete </param>
        void UpdateLobbySettings(LobbySettings settings, Action<bool> onComplete = null);

        /// <summary>
        /// Kick a player from the lobby
        /// </summary>
        /// <param name="playerId"> ID of the player to kick </param>
        /// <param name="onComplete"> Callback invoked when kick is complete </param>
        void KickPlayer(string playerId, Action<bool> onComplete = null);

        /// <summary>
        /// Promote a player to lobby owner
        /// </summary>
        /// <param name="playerId"> ID of the player to promote </param>
        /// <param name="onComplete"> Callback invoked when promotion is complete </param>
        void PromotePlayer(string playerId, Action<bool> onComplete = null);

        /// <summary>
        /// Search for lobbies matching the given criteria
        /// </summary>
        /// <param name="searchOptions"> Search options to use </param>
        /// <param name="onComplete"> Callback invoked when search is complete </param>
        void SearchLobbies(LobbySearchOptions searchOptions, Action<bool, List<LobbySearchResult>> onComplete = null);

        /// <summary>
        /// Send an invite to a player
        /// </summary>
        /// <param name="playerId"> ID of the player to invite </param>
        /// <param name="onComplete"> Callback invoked when invitation is sent </param>
        void SendInvite(string playerId, Action<bool> onComplete = null);

        /// <summary>
        /// Accept an invite
        /// </summary>
        /// <param name="inviteId"> ID of the invite to accept </param>
        /// <param name="onComplete"> Callback invoked when acceptance is complete </param>
        void AcceptInvite(string inviteId, Action<bool, LobbyInfo> onComplete = null);

        /// <summary>
        /// Reject an invite
        /// </summary>
        /// <param name="inviteId"> ID of the invite to reject </param>
        /// <param name="onComplete"> Callback invoked when rejection is complete </param>
        void RejectInvite(string inviteId, Action<bool> onComplete = null);

        /// <summary>
        /// Get the list of pending invites
        /// </summary>
        /// <returns> List of pending invites </returns>
        List<string> GetPendingInvites();

        /// <summary>
        /// Refresh the current lobby information
        /// </summary>
        /// <param name="onComplete"> Callback invoked when refresh is complete </param>
        void RefreshLobby(Action<bool> onComplete = null);

        /// <summary>
        /// Add a provider for lobby services
        /// </summary>
        /// <param name="provider"> The provider to add </param>
        /// <returns> True if the provider was added successfully </returns>
        bool AddProvider(ILobbyProvider provider);

        /// <summary>
        /// Get a provider by name
        /// </summary>
        /// <param name="providerName"> Name of the provider </param>
        /// <returns> The provider, or null if not found </returns>
        ILobbyProvider GetProvider(string providerName);
    }
}