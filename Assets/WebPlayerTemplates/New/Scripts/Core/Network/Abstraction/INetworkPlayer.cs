using UnityEngine;
using System;

namespace RecipeRage.Core.Network
{
    /// <summary>
    /// Interface for networked player functionality
    /// </summary>
    public interface INetworkPlayer
    {
        #region Properties
        /// <summary>
        /// Unique identifier for the player
        /// </summary>
        string PlayerId { get; }

        /// <summary>
        /// Player's display name
        /// </summary>
        string PlayerName { get; }

        /// <summary>
        /// Player's current state
        /// </summary>
        PlayerNetworkState State { get; }

        /// <summary>
        /// Whether this is the local player
        /// </summary>
        bool IsLocalPlayer { get; }

        /// <summary>
        /// Whether this player is the host
        /// </summary>
        bool IsHost { get; }

        /// <summary>
        /// Player's current latency
        /// </summary>
        float Latency { get; }
        #endregion

        #region Events
        /// <summary>
        /// Fired when player state changes
        /// </summary>
        event Action<PlayerNetworkState> OnStateChanged;

        /// <summary>
        /// Fired when player's transform is updated
        /// </summary>
        event Action<Vector3, Quaternion> OnTransformUpdated;

        /// <summary>
        /// Fired when player's stats are updated
        /// </summary>
        event Action<PlayerStats> OnStatsUpdated;
        #endregion

        #region Network Methods
        /// <summary>
        /// Send data to this player
        /// </summary>
        void SendData(byte[] data, NetworkDelivery delivery);

        /// <summary>
        /// Kick player from session
        /// </summary>
        void Kick(string reason);
        #endregion

        #region State Management
        /// <summary>
        /// Get player's current statistics
        /// </summary>
        PlayerStats GetStats();

        /// <summary>
        /// Update player's transform
        /// </summary>
        void UpdateTransform(Vector3 position, Quaternion rotation);

        /// <summary>
        /// Update player's state
        /// </summary>
        void UpdateState(PlayerNetworkState newState);
        #endregion
    }

    #region Supporting Types
    public enum PlayerNetworkState
    {
        Connecting,
        Connected,
        Loading,
        Ready,
        Playing,
        Spectating,
        Disconnected
    }

    public enum NetworkDelivery
    {
        Unreliable,
        UnreliableSequenced,
        Reliable,
        ReliableSequenced,
        ReliableFragmented
    }

    public struct PlayerStats
    {
        public int Score;
        public int Kills;
        public int Deaths;
        public float DamageDealt;
        public float DamageTaken;
        public int ObjectivesCompleted;
        public float CookingScore;
        public float AccuracyPercentage;
    }
    #endregion
}