using Unity.Netcode;
using UnityEngine;
using Unity.Collections;
using System;
using Core.Logging;

namespace KitchenClash.Infrastructure.Network
{
    /// <summary>
    /// Monitors Round Trip Time (RTT) during gameplay using Unity Netcode.
    /// Pure C# implementation using CustomMessagingManager - no NetworkObject required.
    /// </summary>
    public class LatencyMonitor : IDisposable
    {
        private const float PING_INTERVAL = 2.0f;
        private readonly NetworkManager _networkManager;
        
        private float _lastPingTime;
        private float _lastPingSentTime;
        private float _currentRtt = 0f;
        private bool _isRunning = false;
        
        private const float WARNING_THRESHOLD = 0.3f;
        
        private const string PING_MESSAGE_NAME = "RecipeRage_Ping";
        private const string PONG_MESSAGE_NAME = "RecipeRage_Pong";

        public float CurrentRtt => _currentRtt;

        public LatencyMonitor(NetworkManager networkManager)
        {
            _networkManager = networkManager;
            RegisterHandlers();
            _isRunning = _networkManager != null;
        }

        public void Update()
        {
            if (!_isRunning || _networkManager == null || !_networkManager.IsClient) return;

            if (Time.time - _lastPingTime >= PING_INTERVAL)
            {
                SendPing();
                _lastPingTime = Time.time;
            }
        }

        private void RegisterHandlers()
        {
            if (_networkManager == null) return;
            
            _networkManager.CustomMessagingManager.RegisterNamedMessageHandler(PING_MESSAGE_NAME, HandlePingServer);
            _networkManager.CustomMessagingManager.RegisterNamedMessageHandler(PONG_MESSAGE_NAME, HandlePongClient);
        }

        private void UnregisterHandlers()
        {
            if (_networkManager?.CustomMessagingManager != null)
            {
                _networkManager.CustomMessagingManager.UnregisterNamedMessageHandler(PING_MESSAGE_NAME);
                _networkManager.CustomMessagingManager.UnregisterNamedMessageHandler(PONG_MESSAGE_NAME);
            }
        }

        private void SendPing()
        {
            if (_networkManager == null || !_networkManager.IsClient) return;

            var writer = new FastBufferWriter(8, Allocator.Temp);
            using (writer)
            {
                writer.WriteValueSafe(Time.time);
                
                _networkManager.CustomMessagingManager.SendNamedMessage(
                    PING_MESSAGE_NAME, 
                    NetworkManager.ServerClientId, 
                    writer);
                    
                _lastPingSentTime = Time.time;
            }
        }

        private void HandlePongClient(ulong senderId, FastBufferReader reader)
        {
            if (senderId != NetworkManager.ServerClientId) return;

            float clientSendTime;
            reader.ReadValueSafe(out clientSendTime);

            _currentRtt = Time.time - clientSendTime;
            
            if (_currentRtt > WARNING_THRESHOLD)
            {
                GameLogger.LogWarning($"High Latency Detected: {(_currentRtt * 1000):F0}ms");
            }
        }

        private void HandlePingServer(ulong senderId, FastBufferReader reader)
        {
            if (_networkManager == null)
            {
                return;
            }

            float clientTime;
            reader.ReadValueSafe(out clientTime);

            var writer = new FastBufferWriter(8, Allocator.Temp);
            using (writer)
            {
                writer.WriteValueSafe(clientTime);
                _networkManager.CustomMessagingManager.SendNamedMessage(
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
