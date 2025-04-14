using System;
using System.Collections;
using System.Collections.Generic;
using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.P2P;
using Epic.OnlineServices.Platform;
using Epic.OnlineServices.Sessions;
using UnityEngine;

namespace RecipeRage.Core.Networking
{
    /// <summary>
    /// Manager for Epic Online Services networking functionality.
    /// </summary>
    public class EOSNetworkManager : MonoBehaviour
    {
        [Header("EOS Settings")]
        [SerializeField] private bool _autoLogin = true;
        [SerializeField] private string _defaultDisplayName = "Player";
        
        /// <summary>
        /// Event triggered when the user logs in.
        /// </summary>
        public event Action<bool, string> OnLoggedIn;
        
        /// <summary>
        /// The network service.
        /// </summary>
        private EOSNetworkService _networkService;
        
        /// <summary>
        /// Flag to track if the manager is initialized.
        /// </summary>
        private bool _isInitialized;
        
        /// <summary>
        /// Flag to track if the user is logged in.
        /// </summary>
        private bool _isLoggedIn;
        
        /// <summary>
        /// Initialize the EOS network manager.
        /// </summary>
        private void Awake()
        {
            // Register the network manager with the service locator
            ServiceLocator.Instance.Register<EOSNetworkManager>(this);
            
            Debug.Log("[EOSNetworkManager] EOS network manager initialized");
        }
        
        /// <summary>
        /// Initialize EOS and login if auto-login is enabled.
        /// </summary>
        private void Start()
        {
            // Initialize EOS
            InitializeEOS();
            
            // Auto-login if enabled
            if (_autoLogin && _isInitialized)
            {
                LoginWithDeviceID(_defaultDisplayName);
            }
        }
        
        /// <summary>
        /// Clean up when the object is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            // Shutdown the network service
            _networkService?.Shutdown();
            
            Debug.Log("[EOSNetworkManager] EOS network manager destroyed");
        }
        
        /// <summary>
        /// Initialize EOS.
        /// </summary>
        public void InitializeEOS()
        {
            if (_isInitialized)
            {
                return;
            }
            
            Debug.Log("[EOSNetworkManager] Initializing EOS");
            
            // Initialize the EOS SDK
            if (!EOSAdapter.Init())
            {
                Debug.LogError("[EOSNetworkManager] Failed to initialize EOS SDK");
                return;
            }
            
            // Create the network service
            _networkService = new EOSNetworkService();
            
            // Initialize the network service
            _networkService.Initialize(success =>
            {
                _isInitialized = success;
                
                if (success)
                {
                    Debug.Log("[EOSNetworkManager] EOS initialized successfully");
                }
                else
                {
                    Debug.LogError("[EOSNetworkManager] Failed to initialize EOS");
                }
            });
        }
        
        /// <summary>
        /// Login with device ID.
        /// </summary>
        /// <param name="displayName">The display name to use</param>
        public void LoginWithDeviceID(string displayName)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[EOSNetworkManager] Cannot login: EOS not initialized");
                OnLoggedIn?.Invoke(false, "EOS not initialized");
                return;
            }
            
            if (_isLoggedIn)
            {
                Debug.LogWarning("[EOSNetworkManager] Already logged in");
                OnLoggedIn?.Invoke(true, null);
                return;
            }
            
            Debug.Log($"[EOSNetworkManager] Logging in with device ID: {displayName}");
            
            // Set the logged in callback
            EOSAdapter.SetLoggedInCallback((success, userId, errorMessage) =>
            {
                _isLoggedIn = success;
                
                if (success)
                {
                    Debug.Log($"[EOSNetworkManager] Logged in successfully: {userId}");
                }
                else
                {
                    Debug.LogError($"[EOSNetworkManager] Failed to login: {errorMessage}");
                }
            });
            
            // Login with device ID
            EOSAdapter.StartLoginWithDeviceID(displayName, (success, errorMessage) =>
            {
                if (success)
                {
                    Debug.Log("[EOSNetworkManager] Login completed successfully");
                }
                else
                {
                    Debug.LogError($"[EOSNetworkManager] Login failed: {errorMessage}");
                }
                
                OnLoggedIn?.Invoke(success, errorMessage);
            });
        }
        
        /// <summary>
        /// Get the network service.
        /// </summary>
        /// <returns>The network service</returns>
        public EOSNetworkService GetNetworkService()
        {
            return _networkService;
        }
        
        /// <summary>
        /// Check if the user is logged in.
        /// </summary>
        /// <returns>True if the user is logged in</returns>
        public bool IsLoggedIn()
        {
            return _isLoggedIn;
        }
        
        /// <summary>
        /// Check if EOS is initialized.
        /// </summary>
        /// <returns>True if EOS is initialized</returns>
        public bool IsInitialized()
        {
            return _isInitialized;
        }
    }
}
