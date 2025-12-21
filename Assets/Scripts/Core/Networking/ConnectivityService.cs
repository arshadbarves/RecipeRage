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

        public ConnectivityService(IEventBus eventBus)
        {
            _eventBus = eventBus;
            StartMonitoring().Forget();
        }

        /// <summary>
        /// Called after all services are constructed.
        /// </summary>
        public void Initialize()
        {
            // ConnectivityService doesn't need cross-service setup
        }

        private async UniTaskVoid StartMonitoring()
        {
            _isRunning = true;
            
            while (_isRunning)
            {
                // 1. PASSIVE CHECK (Fast)
                // If the OS says "No Signal", we definitely have no internet.
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    if (_activeInternetStatus) // Only update if status changed
                    {
                        UpdateStatus(false);
                    }
                }
                else
                {
                    // 2. ACTIVE CHECK (Slow/Reliable)
                    // Only poll if enough time has passed
                    _timeSinceLastActiveCheck += PASSIVE_CHECK_INTERVAL;
                    
                    if (_timeSinceLastActiveCheck >= ACTIVE_CHECK_INTERVAL)
                    {
                        await ForceCheckAsync();
                    }
                }

                await UniTask.Delay(TimeSpan.FromSeconds(PASSIVE_CHECK_INTERVAL));
            }
        }

        public async UniTask<bool> ForceCheckAsync()
        {
            _timeSinceLastActiveCheck = 0f;
            bool result = await CheckConnectionInternal();
            
            // Only fire event if status actually changed
            if (_activeInternetStatus != result)
            {
                UpdateStatus(result);
            }
            
            return result;
        }

        private async UniTask<bool> CheckConnectionInternal()
        {
            try
            {
                // Simple HEAD request to a reliable server
                using (var request = UnityWebRequest.Head(PING_URL))
                {
                    request.timeout = 5; // 5 second timeout
                    await request.SendWebRequest();

                    bool success = request.result == UnityWebRequest.Result.Success;
                    return success;
                }
            }
            catch
            {
                return false;
            }
        }

        private void UpdateStatus(bool isConnected)
        {
            _activeInternetStatus = isConnected;
            GameLogger.Log(isConnected ? "ConnectivityService: Internet Restored" : "ConnectivityService: Internet Lost");
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
