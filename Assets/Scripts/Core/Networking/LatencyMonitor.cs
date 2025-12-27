using Unity.Netcode;
using UnityEngine;
using Core.Logging;
using Unity.Collections;
using System;

namespace Core.Networking
{
    /// <summary>
    /// Monitors Round Trip Time (RTT) during gameplay using Unity Netcode.
    /// Acts as the "Lag Monitor" for the HUD.
    /// Pure C# implementation using CustomMessagingManager - no NetworkObject required.
    /// </summary>
    public class LatencyMonitor : IDisposable
    {
        private const float PING_INTERVAL = 2.0f; // Check every 2 seconds
        
        private float _lastPingTime;
        private float _lastPingSentTime; // The exact time the packet was sent
        private float _currentRtt = 0f;
        private bool _isRunning = false;
        
        // Thresholds for "Bad Connection" (in seconds)
        private const float WARNING_THRESHOLD = 0.3f; // 300ms
        
        private const string PING_MESSAGE_NAME = "RecipeRage_Ping";
        private const string PONG_MESSAGE_NAME = "RecipeRage_Pong";

        public float CurrentRtt => _currentRtt;

        public LatencyMonitor()
        {
            RegisterHandlers();
            _isRunning = true;
        }

        public void Update()
        {
            if (!_isRunning || NetworkManager.Singleton == null || !NetworkManager.Singleton.IsClient) return;

            if (Time.time - _lastPingTime >= PING_INTERVAL)
            {
                SendPing();
                _lastPingTime = Time.time;
            }
        }

        private void RegisterHandlers()
        {
            if (NetworkManager.Singleton == null) return;
            
            // Register handlers
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(PING_MESSAGE_NAME, HandlePingServer);
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(PONG_MESSAGE_NAME, HandlePongClient);
        }

        private void UnregisterHandlers()
        {
            if (NetworkManager.Singleton?.CustomMessagingManager != null)
            {
                NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler(PING_MESSAGE_NAME);
                NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler(PONG_MESSAGE_NAME);
            }
        }

        // ============================================
        // CLIENT LOGIC
        // ============================================

        private void SendPing()
        {
            if (!NetworkManager.Singleton.IsClient) return;

            var writer = new FastBufferWriter(8, Allocator.Temp);
            using (writer)
            {
                writer.WriteValueSafe(Time.time); // Use float time
                
                // If I am active client, send to server
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(
                    PING_MESSAGE_NAME, 
                    NetworkManager.ServerClientId, 
                    writer);
                    
                _lastPingSentTime = Time.time;
            }
        }

        private void HandlePongClient(ulong senderId, FastBufferReader reader)
        {
            // Only process server pong
            if (senderId != NetworkManager.ServerClientId) return;

            float clientSendTime;
            reader.ReadValueSafe(out clientSendTime);

            // Calculate RTT
            _currentRtt = Time.time - clientSendTime;
            
            // Log if lag is bad
            if (_currentRtt > WARNING_THRESHOLD)
            {
                GameLogger.LogWarning($"High Latency Detected: {(_currentRtt * 1000):F0}ms");
            }
        }

        // ============================================
        // SERVER LOGIC
        // ============================================

        private void HandlePingServer(ulong senderId, FastBufferReader reader)
        {
            // Read the time sent by client
            float clientTime;
            reader.ReadValueSafe(out clientTime);

            // Send it back immediately
            var writer = new FastBufferWriter(8, Allocator.Temp);
            using (writer)
            {
                writer.WriteValueSafe(clientTime);
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(
                    PONG_MESSAGE_NAME, 
                    senderId, 
                    writer);
            }
        }

        public void Dispose()
        {
            _isRunning = false;
            UnregisterHandlers();
        }
    }
}
