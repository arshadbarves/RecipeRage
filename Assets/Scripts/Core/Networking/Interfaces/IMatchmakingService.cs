using System;
using Core.Networking.Models;
using Core.Networking.Common;

namespace Core.Networking.Interfaces
{
    /// <summary>
    /// Interface for matchmaking operations
    /// Handles finding matches for parties and solo players
    /// </summary>
    public interface IMatchmakingService
    {
        #region Events

        /// <summary>
        /// Fired when matchmaking starts
        /// </summary>
        event Action OnMatchmakingStarted; // TODO: Should be triggered on all client to switch to matchmaking screen

        /// <summary>
        /// Fired when matchmaking is cancelled
        /// </summary>
        event Action OnMatchmakingCancelled;

        /// <summary>
        /// Fired when matchmaking fails
        /// </summary>
        event Action<string> OnMatchmakingFailed;

        /// <summary>
        /// Fired when players are found (progress update)
        /// </summary>
        event Action<int, int> OnPlayersFound; // (current, required)

        /// <summary>
        /// Fired when a match is found and ready
        /// </summary>
        event Action<LobbyInfo> OnMatchFound;

        #endregion

        #region Properties

        /// <summary>
        /// Whether matchmaking is currently active
        /// </summary>
        bool IsSearching { get; }

        /// <summary>
        /// Current number of players found
        /// </summary>
        int PlayersFound { get; }

        /// <summary>
        /// Required number of players for the match
        /// </summary>
        int RequiredPlayers { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Initialize the matchmaking service
        /// </summary>
        void Initialize();

        /// <summary>
        /// Start matchmaking for the current party
        /// </summary>
        /// <param name="gameModeId">Game mode ID to search for</param>
        /// <param name="teamSize">Size of each team</param>
        void FindMatch(string gameModeId, int teamSize);

        /// <summary>
        /// Cancel active matchmaking
        /// </summary>
        void CancelMatchmaking();

        /// <summary>
        /// Search for available match lobbies
        /// </summary>
        /// <param name="gameModeId">Game mode ID to search for</param>
        /// <param name="teamSize">Team size</param>
        /// <param name="neededPlayers">Number of players needed</param>
        /// <returns>List of available lobbies</returns>
        void SearchForMatchLobbies(string gameModeId, int teamSize, int neededPlayers);

        /// <summary>
        /// Create a new match lobby and wait for players
        /// </summary>
        /// <param name="gameModeId">Game mode ID</param>
        /// <param name="teamSize">Team size</param>
        void CreateAndWaitForPlayers(string gameModeId, int teamSize);

        /// <summary>
        /// Fill remaining slots with bots and start the match
        /// Called by MatchmakingState when timeout occurs
        /// </summary>
        void FillMatchWithBots();

        /// <summary>
        /// Get active bots in the current match
        /// </summary>
        /// <returns>List of active bot players</returns>
        System.Collections.Generic.List<BotPlayer> GetActiveBots();

        #endregion
    }
}
