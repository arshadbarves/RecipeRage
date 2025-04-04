using PlayEveryWare.EpicOnlineServices;
using RecipeRage.Core.Patterns;
using RecipeRage.Modules.Auth;
using RecipeRage.Modules.Logging;
using RecipeRage.Modules.Logging.Interfaces;
using UnityEngine;
using RecipeRage.UI.Core;

namespace RecipeRage.Core
{
    /// <summary>
    /// Central bootstrap class for the entire game.
    /// Initializes all game subsystems including logging, but delegates specific initialization to each system.
    /// This should be the only MonoBehaviour that needs to be attached to a scene object.
    /// Complexity Rating: 3
    /// </summary>
    public class GameBootstrap : MonoBehaviourSingleton<GameBootstrap>
    {
        [Header("Logging Settings")]
        [SerializeField] private LogLevel _consoleLogLevel = LogLevel.Debug;
        [SerializeField] private LogLevel _fileLogLevel = LogLevel.Info;
        [SerializeField] private bool _enableFileLogging = true;

        [Header("Auth Settings")]
        [SerializeField] private bool _initializeAuth = true;
        [SerializeField] private bool _enableGuestLogin = true;
        [SerializeField] private bool _enableFacebookLogin = true;
        [SerializeField] private bool _enableEOSDeviceLogin = true;

        [Header("Epic Online Services")]
        [SerializeField] private bool _initializeEOS = true;
        private bool _eosInitialized;

        // Tracks whether subsystems have been initialized
        private bool _logServiceInitialized;
        private bool _authInitializationRequested = false;

        /// <summary>
        /// Initialize core systems on Awake
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // Initialize logging first so we can log initialization of other systems
            InitializeLogging();

            LogHelper.Info("GameBootstrap", "Game bootstrap initialized");
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
                RequestAuthInitialization();
            }
            else
            {
                _authInitializationRequested = true;
            }

            // Add more subsystem initializations here as needed

            // Now that core bootstrap is done, start the UI flow
            StartUI();

            LogHelper.Info("GameBootstrap", "All core systems initialization requested.");
        }

        private void RequestAuthInitialization()
        {
            if (_authInitializationRequested) return;

            // Use the self-initializing AuthHelper instead of direct initialization
            AuthHelper.Initialize(
                _enableGuestLogin,
                _enableFacebookLogin,
                _enableEOSDeviceLogin && _eosInitialized,
                success =>
                {
                    if (success)
                    {
                        LogHelper.Info("GameBootstrap", "Authentication system auto-login successful");
                    }
                    else
                    {
                        LogHelper.Info("GameBootstrap", "Authentication system initialized, no auto-login or pending.");
                    }
                    // Mark auth init as requested/handled AFTER the callback fires or immediately if sync
                    _authInitializationRequested = true;
                    // Note: The actual sign-in might still be async, Loading screen handles waiting
                }
            );
            // If AuthHelper.Initialize was synchronous, mark immediately.
            // If it's purely async via callback, this might be slightly premature,
            // but the Loading screen waits for IsSignedIn anyway.
            // _authInitializationRequested = true; // Moved inside callback for safety if Initialize itself is async
        }

        private void StartUI()
        {
            // Ensure the UI Manager exists and let it start its flow
            if (UIScreenManager.Instance != null)
            {
                LogHelper.Info("GameBootstrap", "Triggering UI Manager to show initial screen.");
                // Explicitly call the startup method on UIScreenManager
                UIScreenManager.Instance.StartUIManager();
            }
            else
            {
                LogHelper.Error("GameBootstrap", "UIScreenManager instance not found. UI cannot be started.");
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
        }

        /// <summary>
        /// Handle application quit to clean up resources
        /// </summary>
        private void OnApplicationQuit()
        {
            LogHelper.Info("GameBootstrap", "Application quitting, shutting down services");

            // Sign out the current user if any
            if (AuthHelper.IsSignedIn())
            {
                AuthHelper.SignOut();
            }
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
            LogHelper.SetConsoleOutput(true);
            LogHelper.SetFileOutput(_enableFileLogging);
            LogHelper.SetLogLevel(_consoleLogLevel);

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

            LogHelper.Info("GameBootstrap", "Initializing Epic Online Services");

            // Check if EOS is available (we expect the EOS Manager to be initialized elsewhere)
            if (EOSManager.Instance == null)
            {
                LogHelper.Error("GameBootstrap", "EOSManager instance not found. Make sure EOS is properly set up.");
                return;
            }

            _eosInitialized = true;
            LogHelper.Info("GameBootstrap", "Epic Online Services initialized");
        }
    }
}