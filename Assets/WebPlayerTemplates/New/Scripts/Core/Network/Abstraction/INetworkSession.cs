using System;
using System.Collections.Generic;

namespace RecipeRage.Core.Network
{
    /// <summary>
    /// Interface for managing network game sessions
    /// </summary>
    public interface INetworkSession
    {
        #region Properties
        /// <summary>
        /// Unique identifier for the session
        /// </summary>
        string SessionId { get; }

        /// <summary>
        /// Session configuration
        /// </summary>
        NetworkSessionConfig Config { get; }

        /// <summary>
        /// Current session state
        /// </summary>
        NetworkSessionState State { get; }

        /// <summary>
        /// Host player of the session
        /// </summary>
        INetworkPlayer Host { get; }

        /// <summary>
        /// List of all players in the session
        /// </summary>
        IReadOnlyList<INetworkPlayer> Players { get; }
        #endregion

        #region Events
        /// <summary>
        /// Fired when session state changes
        /// </summary>
        event Action<NetworkSessionState> OnStateChanged;

        /// <summary>
        /// Fired when session configuration is updated
        /// </summary>
        event Action<NetworkSessionConfig> OnConfigUpdated;

        /// <summary>
        /// Fired when host migration occurs
        /// </summary>
        event Action<INetworkPlayer> OnHostMigrated;
        #endregion

        #region Session Methods
        /// <summary>
        /// Update session configuration
        /// </summary>
        void UpdateConfig(NetworkSessionConfig newConfig);

        /// <summary>
        /// Start the game session
        /// </summary>
        void StartGame();

        /// <summary>
        /// End the game session
        /// </summary>
        void EndGame();

        /// <summary>
        /// Pause the game session
        /// </summary>
        void PauseGame();

        /// <summary>
        /// Resume the game session
        /// </summary>
        void ResumeGame();
        #endregion

        #region Player Management
        /// <summary>
        /// Add player to session
        /// </summary>
        void AddPlayer(INetworkPlayer player);

        /// <summary>
        /// Remove player from session
        /// </summary>
        void RemovePlayer(string playerId);

        /// <summary>
        /// Get player by ID
        /// </summary>
        INetworkPlayer GetPlayer(string playerId);

        /// <summary>
        /// Check if player is in session
        /// </summary>
        bool HasPlayer(string playerId);
        #endregion

        #region Team Management
        /// <summary>
        /// Get players in team
        /// </summary>
        IReadOnlyList<INetworkPlayer> GetTeamPlayers(int teamId);

        /// <summary>
        /// Assign player to team
        /// </summary>
        void AssignPlayerToTeam(string playerId, int teamId);

        /// <summary>
        /// Get team score
        /// </summary>
        int GetTeamScore(int teamId);

        /// <summary>
        /// Update team score
        /// </summary>
        void UpdateTeamScore(int teamId, int score);
        #endregion

        #region State Management
        /// <summary>
        /// Get session statistics
        /// </summary>
        SessionStats GetStats();

        /// <summary>
        /// Update session state
        /// </summary>
        void UpdateState(NetworkSessionState newState);
        #endregion
    }

    #region Supporting Types
    public struct SessionStats
    {
        public int TotalPlayers;
        public int ActivePlayers;
        public float SessionDuration;
        public Dictionary<int, int> TeamScores;
        public float AverageLatency;
        public string GameMode;
        public string MapName;
    }
    #endregion
}
