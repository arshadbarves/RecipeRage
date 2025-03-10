using RecipeRage.Core.Patterns;
using RecipeRage.Core.Services;
using RecipeRage.Modules.Auth;
using UnityEngine;

namespace RecipeRage.Core
{
    /// <summary>
    /// Central bootstrap class for the entire game.
    /// Initializes all game subsystems including logging, but delegates specific initialization to each system.
    /// This should be the only MonoBehaviour that needs to be attached to a scene object.
    /// 
    /// Complexity Rating: 3
    /// </summary>
    public class GameBootstrap : MonoBehaviourSingleton<GameBootstrap>
    {
        [Header("Logging Settings")]
        [SerializeField] private LogSeverity _consoleLogLevel = LogSeverity.Debug;
        [SerializeField] private LogSeverity _fileLogLevel = LogSeverity.Info;
        [SerializeField] private LogSeverity _remoteLogLevel = LogSeverity.Error;
        [SerializeField] private bool _enableFileLogging = true;
        [SerializeField] private bool _enableRemoteLogging = false;
        
        [Header("Auth Settings")]
        [SerializeField] private bool _initializeAuth = true;
        [SerializeField] private bool _enableGuestLogin = true;
        [SerializeField] private bool _enableFacebookLogin = true;
        [SerializeField] private bool _enableEOSDeviceLogin = true;
        
        [Header("Epic Online Services")]
        [SerializeField] private bool _initializeEOS = true;
        
        // Tracks whether subsystems have been initialized
        private bool _logServiceInitialized = false;
        private bool _eosInitialized = false;
        
        /// <summary>
        /// Initialize core systems on Awake
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            
            // Initialize logging first so we can log initialization of other systems
            InitializeLogging();
            
            Logger.Info("GameBootstrap", "Game bootstrap initialized");
        }
        
        /// <summary>
        /// Initialize remaining systems on Start
        /// </summary>
        private void Start()
        {
            // Initialize EOS if needed
            if (_initializeEOS)
            {
                InitializeEOS();
            }
            
            // Initialize authentication system
            if (_initializeAuth)
            {
                // Use the self-initializing AuthHelper instead of direct initialization
                AuthHelper.Initialize(
                    _enableGuestLogin,
                    _enableFacebookLogin,
                    _enableEOSDeviceLogin && _eosInitialized,
                    success => {
                        if (success)
                        {
                            Logger.Info("GameBootstrap", "Authentication system auto-login successful");
                        }
                        else
                        {
                            Logger.Info("GameBootstrap", "Authentication system initialized, no auto-login");
                        }
                    }
                );
            }
            
            // Add more subsystem initializations here as needed
            
            Logger.Info("GameBootstrap", "All systems initialized");
        }
        
        /// <summary>
        /// Initialize the logging service
        /// </summary>
        private void InitializeLogging()
        {
            if (_logServiceInitialized)
            {
                return;
            }
            
            // Initialize the log service
            LogService.Instance.Initialize(
                _consoleLogLevel,
                _fileLogLevel,
                _remoteLogLevel,
                _enableRemoteLogging,
                _enableFileLogging
            );
            
            _logServiceInitialized = true;
            
            // Register for application quit to properly shut down logging
            Application.quitting += OnApplicationQuit;
        }
        
        /// <summary>
        /// Initialize the Epic Online Services SDK
        /// </summary>
        private void InitializeEOS()
        {
            if (_eosInitialized)
            {
                return;
            }
            
            Logger.Info("GameBootstrap", "Initializing Epic Online Services");
            
            // Check if EOS is available (we expect the EOS Manager to be initialized elsewhere)
            if (PlayEveryWare.EpicOnlineServices.EOSManager.Instance == null)
            {
                Logger.Error("GameBootstrap", "EOSManager instance not found. Make sure EOS is properly set up.");
                return;
            }
            
            _eosInitialized = true;
            Logger.Info("GameBootstrap", "Epic Online Services initialized");
        }
        
        /// <summary>
        /// Handle application quit to clean up resources
        /// </summary>
        private void OnApplicationQuit()
        {
            Logger.Info("GameBootstrap", "Application quitting, shutting down services");
            
            // Shut down subsystems in reverse order of initialization
            
            // Sign out the current user if any
            if (AuthHelper.IsSignedIn())
            {
                AuthHelper.SignOut();
            }
            
            // Shut down logging last so we can log the shutdown of other systems
            if (_logServiceInitialized)
            {
                LogService.Instance.Shutdown();
                _logServiceInitialized = false;
            }
        }
        
        /// <summary>
        /// OnDestroy is called when the MonoBehaviour is being destroyed
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            // Unregister application quit handler
            Application.quitting -= OnApplicationQuit;
            
            // Shut down if not already done by OnApplicationQuit
            if (_logServiceInitialized)
            {
                LogService.Instance.Shutdown();
                _logServiceInitialized = false;
            }
        }
    }
} 