using System.Collections;
using Epic.OnlineServices;
using PlayEveryWare.EpicOnlineServices;
using UI.UISystem;
using UI.UISystem.Core;
using UI.UISystem.Screens;
using UnityEngine;

namespace Core.Networking
{
    /// <summary>
    /// Bootstrap class for initializing the networking system.
    /// Handles login flow before network initialization.
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
        /// Whether the user is logged in.
        /// </summary>
        public bool IsLoggedIn { get; private set; }

        /// <summary>
        /// The network manager instance.
        /// </summary>
        private RecipeRageNetworkManager _networkManager;

        /// <summary>
        /// The login screen instance.
        /// </summary>
        private LoginScreen _loginScreen;

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
        /// This will show the login screen and wait for successful login before proceeding.
        /// </summary>
        private IEnumerator InitializeNetworking()
        {
            Debug.Log("[NetworkBootstrap] Starting initialization");

            // Wait for EOSManager to initialize
            Debug.Log("[NetworkBootstrap] Waiting for EOSManager...");
            while (EOSManager.Instance == null)
            {
                yield return new WaitForSeconds(0.1f);
            }

            Debug.Log("[NetworkBootstrap] EOSManager initialized");

            // Wait for UIManager to initialize
            Debug.Log("[NetworkBootstrap] Waiting for UIManager...");
            while (UIManager.Instance == null)
            {
                yield return new WaitForSeconds(0.1f);
            }

            Debug.Log("[NetworkBootstrap] UIManager initialized");

            // Show login screen and wait for login
            yield return HandleLoginFlow();

            // Initialize the network manager after successful login
            Debug.Log("[NetworkBootstrap] Initializing network manager...");
            yield return new WaitForSeconds(0.5f);

            // Mark as initialized
            IsInitialized = true;

            Debug.Log("[NetworkBootstrap] Networking system fully initialized");
        }

        /// <summary>
        /// Handle the login flow - attempt auto-login first, show UI only if needed.
        /// This is a critical flow with no timeout.
        /// </summary>
        private IEnumerator HandleLoginFlow()
        {
            Debug.Log("[NetworkBootstrap] Starting login flow");

            // Wait for AuthenticationManager to be available
            while (Core.Authentication.AuthenticationManager.Instance == null)
            {
                yield return new WaitForSeconds(0.1f);
            }

            // Check if user is already logged in
            ProductUserId productUserId = EOSManager.Instance.GetProductUserId();
            if (productUserId != null && productUserId.IsValid())
            {
                Debug.Log("[NetworkBootstrap] User already logged in");
                IsLoggedIn = true;
                yield break;
            }

            // Attempt auto-login first (without showing UI)
            Debug.Log("[NetworkBootstrap] Attempting auto-login...");
            yield return Core.Authentication.AuthenticationManager.Instance.AttemptAutoLogin();

            // Check if auto-login succeeded
            if (Core.Authentication.AuthenticationManager.Instance.IsLoggedIn)
            {
                Debug.Log("[NetworkBootstrap] Auto-login successful");
                IsLoggedIn = true;
                yield break;
            }

            // Auto-login failed, show login screen for manual login
            Debug.Log("[NetworkBootstrap] Auto-login failed, showing login screen");
            yield return ShowLoginScreenAndWait();
        }

        /// <summary>
        /// Show login screen and wait for user to login manually.
        /// </summary>
        private IEnumerator ShowLoginScreenAndWait()
        {
            // Hide loading screen temporarily to show login screen
            LoadingScreen loadingScreen = UIManager.Instance.GetScreen<LoadingScreen>();

            // Get login screen
            _loginScreen = UIManager.Instance.GetScreen<LoginScreen>();

            if (_loginScreen == null)
            {
                Debug.LogError("[NetworkBootstrap] LoginScreen not found! Make sure it's registered in UIManager.");
                yield break;
            }

            // Subscribe to authentication events
            bool loginCompleted = false;
            bool loginSuccessful = false;

            Core.Authentication.AuthenticationManager.Instance.OnLoginSuccess += () =>
            {
                loginCompleted = true;
                loginSuccessful = true;
                Debug.Log("[NetworkBootstrap] Login successful");
            };

            Core.Authentication.AuthenticationManager.Instance.OnLoginFailed += (error) =>
            {
                Debug.LogWarning($"[NetworkBootstrap] Login failed: {error}");
                // Don't set loginCompleted - user can retry
            };

            // Show login screen
            Debug.Log("[NetworkBootstrap] Showing login screen");
            _loginScreen.Show();

            // Wait for successful login (no timeout - this is critical)
            while (!loginCompleted)
            {
                yield return null;
            }

            if (loginSuccessful)
            {
                IsLoggedIn = true;
                Debug.Log("[NetworkBootstrap] Login flow completed successfully");

                // Hide login screen
                _loginScreen.Hide(true);
            }
            else
            {
                Debug.LogError("[NetworkBootstrap] Login flow failed");
            }
        }

        /// <summary>
        /// Get the network manager instance.
        /// </summary>
        /// <returns>The network manager instance</returns>
        public RecipeRageNetworkManager GetNetworkManager()
        {
            return _networkManager;
        }

        /// <summary>
        /// Logout the current user and show login screen again.
        /// </summary>
        public void Logout()
        {
            Debug.Log("[NetworkBootstrap] Logging out user");

            IsLoggedIn = false;

            // Logout from AuthenticationManager
            if (Core.Authentication.AuthenticationManager.Instance != null)
            {
                Core.Authentication.AuthenticationManager.Instance.Logout();
            }

            // Show login screen again
            StartCoroutine(HandleLoginFlow());
        }
    }
}
