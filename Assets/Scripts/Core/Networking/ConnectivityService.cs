using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Core.Logging;
using Core.Events;
using Core.Bootstrap;

namespace Core.Networking
{
    public interface IConnectivityService : IInitializable
    {
        bool IsInternetAvailable { get; }
        event Action<bool> OnConnectionStatusChanged;
        UniTask<bool> ForceCheckAsync();
    }

    public class ConnectivityService : IConnectivityService, IDisposable
    {
        // Check "Active" internet (Ping) every 30 seconds
        private const float ACTIVE_CHECK_INTERVAL = 30f;
        // Check "Passive" signal (Wifi/Data) every 1 second
        private const float PASSIVE_CHECK_INTERVAL = 1f;
        
        // Google Public DNS - highly reliable usage
        private const string PING_URL = "https://clients3.google.com/generate_204";

        private bool _activeInternetStatus = true;
        private float _timeSinceLastActiveCheck = 0f;
        private bool _isRunning = false;
        
        public bool IsInternetAvailable => _activeInternetStatus;
        public event Action<bool> OnConnectionStatusChanged;

        private readonly IEventBus _eventBus;
        private readonly ILoggingService _logger;

        public ConnectivityService(IEventBus eventBus, ILoggingService logger)
        {
            _eventBus = eventBus;
            _logger = logger;
            StartMonitoring().Forget();
        }

        /// <summary>
        /// Called after all services are constructed.
        /// </summary>
        public void Initialize()
        {
            // ConnectivityService doesn't need cross-service setup
        }
// ... (omitting StartMonitoring and ForceCheckAsync)
        private void UpdateStatus(bool isConnected)
        {
            _activeInternetStatus = isConnected;
            _logger.LogInfo(isConnected ? "Internet Restored" : "Internet Lost", "ConnectivityService");
            OnConnectionStatusChanged?.Invoke(isConnected);
            
            // Also publish via EventBus for loosely coupled systems
            // _eventBus?.Publish(new ConnectionStatusChangedEvent { IsConnected = isConnected });
        }

        public void Dispose()
        {
            _isRunning = false;
        }
    }
}
