using System;
using UnityEngine;
using RecipeRage.Modules.Lobbies.Interfaces;
using RecipeRage.Modules.Logging;

namespace RecipeRage.Modules.Lobbies.Core
{
    /// <summary>
    /// Main module for the lobby system, serving as the entry point
    /// </summary>
    public class LobbyModule : MonoBehaviour
    {
        #region Singleton

        /// <summary>
        /// Singleton instance of the LobbyModule
        /// </summary>
        public static LobbyModule Instance { get; private set; }

        #endregion

        #region Services

        /// <summary>
        /// The lobby service for creating and managing lobbies
        /// </summary>
        public ILobbyService LobbyService { get; private set; }

        /// <summary>
        /// The matchmaking service for finding matches
        /// </summary>
        public IMatchmakingService MatchmakingService { get; private set; }

        #endregion

        #region Properties

        /// <summary>
        /// Whether the module is initialized
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// The last error message from the module
        /// </summary>
        public string LastError { get; private set; }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            LogHelper.Info("LobbyModule", "LobbyModule created");
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            
            LogHelper.Info("LobbyModule", "LobbyModule destroyed");
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the lobby module
        /// </summary>
        /// <param name="onComplete">Callback invoked when initialization is complete</param>
        public void Initialize(Action<bool> onComplete = null)
        {
            if (IsInitialized)
            {
                LogHelper.Warning("LobbyModule", "LobbyModule is already initialized");
                onComplete?.Invoke(true);
                return;
            }
            
            LogHelper.Info("LobbyModule", "Initializing LobbyModule");
            
            // Create and initialize the lobby service
            LobbyService = new LobbyService();
            LobbyService.Initialize(lobbyServiceInitialized =>
            {
                if (!lobbyServiceInitialized)
                {
                    LastError = $"Failed to initialize LobbyService: {LobbyService.LastError}";
                    LogHelper.Error("LobbyModule", LastError);
                    onComplete?.Invoke(false);
                    return;
                }
                
                LogHelper.Info("LobbyModule", "LobbyService initialized successfully");
                
                // Create and initialize the matchmaking service
                MatchmakingService = new MatchmakingService(LobbyService);
                MatchmakingService.Initialize(matchmakingServiceInitialized =>
                {
                    if (!matchmakingServiceInitialized)
                    {
                        LastError = $"Failed to initialize MatchmakingService: {MatchmakingService.LastError}";
                        LogHelper.Error("LobbyModule", LastError);
                        onComplete?.Invoke(false);
                        return;
                    }
                    
                    LogHelper.Info("LobbyModule", "MatchmakingService initialized successfully");
                    
                    // Everything is initialized
                    IsInitialized = true;
                    LogHelper.Info("LobbyModule", "LobbyModule initialized successfully");
                    onComplete?.Invoke(true);
                });
            });
        }

        #endregion

        #region Module Management

        /// <summary>
        /// Shutdown the lobby module
        /// </summary>
        /// <param name="onComplete">Callback invoked when shutdown is complete</param>
        public void Shutdown(Action<bool> onComplete = null)
        {
            if (!IsInitialized)
            {
                LogHelper.Warning("LobbyModule", "LobbyModule is not initialized");
                onComplete?.Invoke(true);
                return;
            }
            
            LogHelper.Info("LobbyModule", "Shutting down LobbyModule");
            
            // Shutdown all services
            // We need to track each service's shutdown state to know when all are done
            bool matchmakingShutdown = false;
            bool lobbyShutdown = false;
            
            // Function to check if all services are shut down
            void CheckAllShutdown()
            {
                if (matchmakingShutdown && lobbyShutdown)
                {
                    IsInitialized = false;
                    LogHelper.Info("LobbyModule", "LobbyModule shut down successfully");
                    onComplete?.Invoke(true);
                }
            }
            
            // Shutdown matchmaking service first
            if (MatchmakingService != null && MatchmakingService.IsInitialized)
            {
                // If we're matchmaking, cancel it first
                if (MatchmakingService.IsMatchmaking)
                {
                    MatchmakingService.CancelMatchmaking(_ =>
                    {
                        // We don't care about the result, just mark it as shut down
                        matchmakingShutdown = true;
                        CheckAllShutdown();
                    });
                }
                else
                {
                    // Not matchmaking, so just mark it as shut down
                    matchmakingShutdown = true;
                    CheckAllShutdown();
                }
            }
            else
            {
                // No matchmaking service or not initialized
                matchmakingShutdown = true;
            }
            
            // Shutdown lobby service
            if (LobbyService != null && LobbyService.IsInitialized)
            {
                // If we're in a lobby, leave it first
                if (LobbyService.CurrentLobby != null)
                {
                    LobbyService.LeaveLobby(_ =>
                    {
                        // We don't care about the result, just mark it as shut down
                        lobbyShutdown = true;
                        CheckAllShutdown();
                    });
                }
                else
                {
                    // Not in a lobby, so just mark it as shut down
                    lobbyShutdown = true;
                    CheckAllShutdown();
                }
            }
            else
            {
                // No lobby service or not initialized
                lobbyShutdown = true;
            }
            
            // Check in case all were already shut down
            CheckAllShutdown();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get whether the player is in a lobby
        /// </summary>
        /// <returns>True if the player is in a lobby</returns>
        public bool IsInLobby()
        {
            return IsInitialized && LobbyService != null && LobbyService.CurrentLobby != null;
        }

        /// <summary>
        /// Get whether the player is currently matchmaking
        /// </summary>
        /// <returns>True if the player is matchmaking</returns>
        public bool IsMatchmaking()
        {
            return IsInitialized && MatchmakingService != null && MatchmakingService.IsMatchmaking;
        }

        #endregion
    }
} 