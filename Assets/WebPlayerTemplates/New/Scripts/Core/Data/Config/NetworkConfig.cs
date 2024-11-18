using System;
using UnityEngine;

namespace Core.Data.Config
{
    [CreateAssetMenu(fileName = "NetworkConfig", menuName = "RecipeRage/Config/NetworkConfig")]
    public class NetworkConfig : ScriptableObject
    {

        public enum NetworkTransport
        {
            Utp,
            Relay,
            Custom
        }

        private static NetworkConfig _instance;

        [Header("Network Settings")]
        public ConnectionSettings connection;
        public SyncSettings sync;
        public MatchmakingSettings matchmaking;

        [Header("Network Transport")]
        public NetworkTransport transportType = NetworkTransport.Utp;
        public string relayServerURL;
        public int port = 7777;

        public NetworkConfig()
        {
            connection = new ConnectionSettings();
            sync = new SyncSettings();
            matchmaking = new MatchmakingSettings();
        }
        public static NetworkConfig Instance {
            get {
                if (_instance == null)
                {
                    _instance = Resources.Load<NetworkConfig>("Configs/NetworkConfig");
                }
                return _instance;
            }
        }

        private void OnValidate()
        {
            ValidateConnectionSettings();
            ValidateSyncSettings();
            ValidateMatchmakingSettings();
        }

        private void ValidateConnectionSettings()
        {
            connection.maxConnections = Mathf.Clamp(connection.maxConnections, 2, 8);
            connection.connectionTimeout = Mathf.Max(5f, connection.connectionTimeout);
            connection.reconnectTimeout = Mathf.Max(1f, connection.reconnectTimeout);
            connection.maxReconnectAttempts = Mathf.Clamp(connection.maxReconnectAttempts, 1, 5);
        }

        private void ValidateSyncSettings()
        {
            sync.tickRate = Mathf.Clamp(sync.tickRate, 10f, 120f);
            sync.snapShotInterpolation = Mathf.Max(0f, sync.snapShotInterpolation);
            sync.bufferSize = Mathf.Clamp(sync.bufferSize, 1, 10);
            sync.maxExtrapolationTime = Mathf.Max(0f, sync.maxExtrapolationTime);
        }

        private void ValidateMatchmakingSettings()
        {
            matchmaking.matchmakingTimeout = Mathf.Max(10f, matchmaking.matchmakingTimeout);
            matchmaking.minPlayersToStart = Mathf.Max(2, matchmaking.minPlayersToStart);
            matchmaking.maxPlayersPerMatch = Mathf.Clamp(matchmaking.maxPlayersPerMatch,
                matchmaking.minPlayersToStart, connection.maxConnections);
        }
        [Serializable]
        public class ConnectionSettings
        {
            public int maxConnections = 4;
            public float connectionTimeout = 10f;
            public float reconnectTimeout = 5f;
            public int maxReconnectAttempts = 3;
        }

        [Serializable]
        public class SyncSettings
        {
            public float tickRate = 60f;
            public float snapShotInterpolation = 0.1f;
            public int bufferSize = 2;
            public float maxExtrapolationTime = 0.1f;
        }

        [Serializable]
        public class MatchmakingSettings
        {
            public float matchmakingTimeout = 30f;
            public int minPlayersToStart = 2;
            public int maxPlayersPerMatch = 4;
            public float skillMatchRange = 100f;
            public float skillMatchExpansionRate = 50f;
        }
    }
}