using UnityEngine;
using System;
using System.Threading.Tasks;

namespace RecipeRage.Core.Network
{
    /// <summary>
    /// Interface for managing network connections and sessions
    /// </summary>
    public interface INetworkManager
    {
        #region Properties
        /// <summary>
        /// Current connection state
        /// </summary>
        NetworkConnectionState ConnectionState { get; }

        /// <summary>
        /// Current session information
        /// </summary>
        INetworkSession CurrentSession { get; }

        /// <summary>
        /// Local player information
        /// </summary>
        INetworkPlayer LocalPlayer { get; }
        #endregion

        #region Events
        /// <summary>
        /// Fired when connection state changes
        /// </summary>
        event Action<NetworkConnectionState> OnConnectionStateChanged;

        /// <summary>
        /// Fired when a player joins the session
        /// </summary>
        event Action<INetworkPlayer> OnPlayerJoined;

        /// <summary>
        /// Fired when a player leaves the session
        /// </summary>
        event Action<INetworkPlayer> OnPlayerLeft;

        /// <summary>
        /// Fired when session state changes
        /// </summary>
        event Action<NetworkSessionState> OnSessionStateChanged;
        #endregion

        #region Connection Methods
        /// <summary>
        /// Initialize the network manager
        /// </summary>
        Task<bool> Initialize();

        /// <summary>
        /// Start hosting a new session
        /// </summary>
        Task<bool> StartHost(NetworkSessionConfig config);

        /// <summary>
        /// Join an existing session
        /// </summary>
        Task<bool> JoinSession(string sessionId);

        /// <summary>
        /// Leave the current session
        /// </summary>
        Task LeaveSession();

        /// <summary>
        /// Disconnect from the network
        /// </summary>
        Task Disconnect();
        #endregion

        #region Matchmaking
        /// <summary>
        /// Start matchmaking process
        /// </summary>
        Task<bool> StartMatchmaking(MatchmakingConfig config);

        /// <summary>
        /// Cancel ongoing matchmaking
        /// </summary>
        Task CancelMatchmaking();
        #endregion

        #region State Management
        /// <summary>
        /// Get current network statistics
        /// </summary>
        NetworkStats GetNetworkStats();

        /// <summary>
        /// Get list of active players in session
        /// </summary>
        INetworkPlayer[] GetActivePlayers();
        #endregion
    }

    #region Supporting Types
    public enum NetworkConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        Reconnecting,
        Failed
    }

    public enum NetworkSessionState
    {
        None,
        Initializing,
        Lobby,
        Loading,
        InGame,
        EndGame
    }

    public struct NetworkSessionConfig
    {
        public string SessionName;
        public int MaxPlayers;
        public bool IsPrivate;
        public string GameMode;
        public string MapName;
    }

    public struct MatchmakingConfig
    {
        public string[] GameModes;
        public int MinPlayers;
        public int MaxPlayers;
        public int SkillLevel;
        public string[] Regions;
    }

    public struct NetworkStats
    {
        public float Latency;
        public float PacketLoss;
        public int ConnectedPlayers;
        public string Region;
    }
    #endregion
}