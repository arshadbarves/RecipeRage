using System.Collections;
using PlayEveryWare.EpicOnlineServices;
using UnityEngine;

namespace Core.Networking
{
    /// <summary>
    /// Bootstrap class for initializing the networking system.
    /// </summary>
    public class NetworkBootstrap : MonoBehaviour
    {
        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static NetworkBootstrap Instance { get; private set; }

        /// <summary>
        /// Whether the networking system is initialized.
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// The network manager instance.
        /// </summary>
        private RecipeRageNetworkManager _networkManager;

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            // Singleton setup
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Create the network manager
            _networkManager = gameObject.AddComponent<RecipeRageNetworkManager>();

            // Initialize the networking system
            StartCoroutine(InitializeNetworking());
        }

        /// <summary>
        /// Initialize the networking system.
        /// </summary>
        private IEnumerator InitializeNetworking()
        {
            Debug.Log("[NetworkBootstrap] Initializing networking system");

            // Wait for EOSManager to initialize
            while (EOSManager.Instance == null || EOSManager.Instance.GetLocalUserId() == null)
            {
                Debug.Log("[NetworkBootstrap] Waiting for EOSManager to initialize");
                yield return new WaitForSeconds(1.0f);
            }

            Debug.Log("[NetworkBootstrap] EOSManager initialized");

            // Initialize the network manager
            yield return new WaitForSeconds(0.5f);

            // Mark as initialized
            IsInitialized = true;

            Debug.Log("[NetworkBootstrap] Networking system initialized");
        }

        /// <summary>
        /// Get the network manager instance.
        /// </summary>
        /// <returns>The network manager instance</returns>
        public RecipeRageNetworkManager GetNetworkManager()
        {
            return _networkManager;
        }
    }
}
