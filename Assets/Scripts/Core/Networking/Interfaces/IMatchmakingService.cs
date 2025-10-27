using System;
using System.Collections.Generic;
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
        event Action OnMatchmakingStarted;
        
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
        
        /// <summary>
        /// Fired when matchmaking state changes
        /// </summary>
        event Action<MatchmakingState> OnStateChanged;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Whether matchmaking is currently active
        /// </summary>
        bool IsSearching { get; }
        
        /// <summary>
        /// Current matchmaking state
        /// </summary>
        MatchmakingState CurrentState { get; }
        
        /// <summary>
        /// Time spent searching (in seconds)
        /// </summary>
        float SearchTime { get; }
        
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
        /// <param name="gameMode">Game mode to search for</param>
        /// <param name="teamSize">Size of each team</param>
        void FindMatch(GameMode gameMode, int teamSize);
        
        /// <summary>
        /// Cancel active matchmaking
        /// </summary>
        void CancelMatchmaking();
        
        /// <summary>
        /// Search for available match lobbies
        /// </summary>
        /// <param name="gameMode">Game mode to search for</param>
        /// <param name="teamSize">Team size</param>
        /// <param name="neededPlayers">Number of players needed</param>
        /// <returns>List of available lobbies</returns>
        void SearchForMatchLobbies(GameMode gameMode, int teamSize, int neededPlayers);
        
        /// <summary>
        /// Create a new match lobby and wait for players
        /// </summary>
        /// <param name="gameMode">Game mode</param>
        /// <param name="teamSize">Team size</param>
        void CreateAndWaitForPlayers(GameMode gameMode, int teamSize);
        
        #endregion
    }
}
