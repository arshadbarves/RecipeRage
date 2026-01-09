using System;
using Core.Core.Networking.Common;
using Epic.OnlineServices;

namespace Core.Core.Networking.Interfaces
{
    /// <summary>
    /// Interface for lobby management operations
    /// Handles both Party Lobbies (persistent) and Match Lobbies (temporary)
    /// </summary>
    public interface ILobbyManager
    {
        #region Events

        // Party Lobby Events
        event Action<Result, LobbyInfo> OnPartyCreated;
        event Action<PlayerInfo> OnPartyMemberJoined;
        event Action<PlayerInfo> OnPartyMemberLeft;
        event Action OnPartyUpdated;

        // Match Lobby Events
        event Action<Result, LobbyInfo> OnMatchLobbyCreated;
        event Action<Result, LobbyInfo> OnMatchLobbyJoined;
        event Action OnMatchLobbyLeft;
        event Action OnMatchLobbyUpdated;

        // General Events
        event Action<LobbyState> OnLobbyStateChanged;
        event Action<string> OnError;

        #endregion

        #region Properties

        /// <summary>
        /// Current party lobby (persistent)
        /// </summary>
        LobbyInfo CurrentPartyLobby { get; }

        /// <summary>
        /// Current match lobby (temporary)
        /// </summary>
        LobbyInfo CurrentMatchLobby { get; }

        /// <summary>
        /// Current lobby state
        /// </summary>
        LobbyState CurrentState { get; }

        /// <summary>
        /// Whether the local player is in a party
        /// </summary>
        bool IsInParty { get; }

        /// <summary>
        /// Whether the local player is in a match lobby
        /// </summary>
        bool IsInMatchLobby { get; }

        /// <summary>
        /// Whether the local player is the party leader
        /// </summary>
        bool IsPartyLeader { get; }

        /// <summary>
        /// Whether the local player is the match lobby owner
        /// </summary>
        bool IsMatchLobbyOwner { get; }

        #endregion

        #region Methods - Initialization

        /// <summary>
        /// Initialize the lobby manager
        /// </summary>
        void Initialize();

        #endregion

        #region Methods - Party Lobby (Persistent)

        /// <summary>
        /// Create a new party lobby
        /// </summary>
        /// <param name="config">Lobby configuration</param>
        void CreatePartyLobby(LobbyConfig config);

        /// <summary>
        /// Invite a friend to the party
        /// </summary>
        /// <param name="friendId">Friend's product user ID</param>
        void InviteToParty(ProductUserId friendId);

        /// <summary>
        /// Leave the current party
        /// </summary>
        void LeaveParty();

        /// <summary>
        /// Update party lobby settings
        /// </summary>
        /// <param name="config">Updated configuration</param>
        void UpdatePartySettings(LobbyConfig config);

        #endregion

        #region Methods - Match Lobby (Temporary)

        /// <summary>
        /// Create a new match lobby (usually called by matchmaking)
        /// </summary>
        /// <param name="config">Lobby configuration</param>
        void CreateMatchLobby(LobbyConfig config);

        /// <summary>
        /// Join an existing match lobby
        /// </summary>
        /// <param name="lobbyId">Match lobby ID</param>
        void JoinMatchLobby(string lobbyId);

        /// <summary>
        /// Leave the current match lobby
        /// </summary>
        void LeaveMatchLobby();

        /// <summary>
        /// Destroy the current match lobby (owner only)
        /// </summary>
        void DestroyMatchLobby();

        #endregion

        #region Methods - Game Settings

        /// <summary>
        /// Set the game mode (party leader only)
        /// </summary>
        /// <param name="gameMode">Game mode to set</param>
        void SetGameMode(GameMode gameMode);

        /// <summary>
        /// Set the map name (party leader only)
        /// </summary>
        /// <param name="mapName">Map name to set</param>
        void SetMapName(string mapName);

        #endregion

        #region Methods - Utility

        /// <summary>
        /// Check if all players in the lobby are ready
        /// </summary>
        /// <returns>True if all players are ready</returns>
        bool AreAllPlayersReady();

        /// <summary>
        /// Get lobby info by ID
        /// </summary>
        /// <param name="lobbyId">Lobby ID</param>
        /// <returns>Lobby info or null if not found</returns>
        LobbyInfo GetLobbyInfo(string lobbyId);

        #endregion
    }
}
