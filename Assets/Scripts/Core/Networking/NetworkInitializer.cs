using Core.Bootstrap;
using Core.Logging;
using Core.Networking.Services;
using Unity.Netcode;
using UnityEngine;

namespace Core.Networking
{
    /// <summary>
    /// Initializes network services and handles network callbacks.
    /// Add this component to a GameObject in your Game scene.
    /// </summary>
    public class NetworkInitializer : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool _autoInitialize = true;

        private INetworkGameManager _networkGameManager;
        private IPlayerNetworkManager _playerNetworkManager;
        private ConnectionHandler _connectionHandler;

        /// <summary>
        /// Initialize on awake.
        /// </summary>
        private void Awake()
        {
            if (_autoInitialize)
            {
                Initialize();
            }
        }

        /// <summary>
        /// Initialize network services.
        /// </summary>
        public void Initialize()
        {
            // Get services from ServiceContainer
            var services = GameBootstrap.Services;
            if (services == null)
            {
                GameLogger.LogError("GameBootstrap.Services is null!");
                return;
            }

            _networkGameManager = services.Session?.NetworkGameManager;
            _playerNetworkManager = services.Session?.PlayerNetworkManager;

            // Create connection handler
            _connectionHandler = new ConnectionHandler(
                services.LoggingService,
                _playerNetworkManager,
                _networkGameManager
            );

            // Subscribe to NetworkManager events
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

                GameLogger.Log("Network services initialized");
            }
            else
            {
                GameLogger.LogWarning("NetworkManager.Singleton is null. Make sure NetworkManager exists in the scene.");
            }
        }

        /// <summary>
        /// Clean up on destroy.
        /// </summary>
        private void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }

        /// <summary>
        /// Handle client connected.
        /// </summary>
        private void OnClientConnected(ulong clientId)
        {
            _connectionHandler?.OnClientConnected(clientId);

            // Register player when they spawn
            if (NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId) != null)
            {
                var playerObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
                var playerController = playerObject.GetComponent<Core.Characters.PlayerController>();

                if (playerController != null)
                {
                    _playerNetworkManager?.RegisterPlayer(clientId, playerController);
                }
            }
        }

        /// <summary>
        /// Handle client disconnected.
        /// </summary>
        private void OnClientDisconnected(ulong clientId)
        {
            _connectionHandler?.OnClientDisconnected(clientId);
        }
    }
}
