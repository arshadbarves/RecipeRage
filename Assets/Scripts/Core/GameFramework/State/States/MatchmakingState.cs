using System;
using RecipeRage.Core.Networking;
using RecipeRage.UI;
using UnityEngine;

namespace RecipeRage.Core.GameFramework.State.States
{
    /// <summary>
    /// State for matchmaking and finding players for a game.
    /// </summary>
    public class MatchmakingState : GameState
    {
        /// <summary>
        /// Event triggered when matchmaking is complete.
        /// </summary>
        public event Action<bool> OnMatchmakingComplete;

        /// <summary>
        /// Reference to the lobby UI prefab.
        /// </summary>
        private GameObject _lobbyUIPrefab;

        /// <summary>
        /// Reference to the instantiated lobby UI.
        /// </summary>
        private GameObject _lobbyUIInstance;

        /// <summary>
        /// Reference to the network lobby manager.
        /// </summary>
        private NetworkLobbyManager _lobbyManager;

        /// <summary>
        /// Flag to track if matchmaking is in progress.
        /// </summary>
        private bool _isMatchmakingInProgress;

        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        public override void Enter()
        {
            base.Enter();

            // Get reference to the network lobby manager
            _lobbyManager = NetworkLobbyManager.Instance;

            // Reset matchmaking state
            _isMatchmakingInProgress = true;

            // Start matchmaking process
            Debug.Log("[MatchmakingState] Starting matchmaking process");

            // Show lobby UI
            ShowLobbyUI();

            // Subscribe to lobby events
            if (_lobbyManager != null)
            {
                _lobbyManager.OnLobbyJoined += HandleLobbyJoined;
                _lobbyManager.OnLobbyLeft += HandleLobbyLeft;
                _lobbyManager.OnGameStarted += HandleGameStarted;

                // Start or join a lobby
                if (_lobbyManager.HasLobbyCode())
                {
                    // Join an existing lobby with the saved code
                    _lobbyManager.JoinLobby(_lobbyManager.GetLobbyCode());
                }
                else
                {
                    // Create a new lobby
                    _lobbyManager.CreateLobby();
                }
            }
            else
            {
                Debug.LogError("[MatchmakingState] NetworkLobbyManager instance not found");
                CompleteMatchmaking(false);
            }
        }

        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        public override void Exit()
        {
            base.Exit();

            // Unsubscribe from lobby events
            if (_lobbyManager != null)
            {
                _lobbyManager.OnLobbyJoined -= HandleLobbyJoined;
                _lobbyManager.OnLobbyLeft -= HandleLobbyLeft;
                _lobbyManager.OnGameStarted -= HandleGameStarted;
            }

            // Cancel matchmaking if still in progress
            if (_isMatchmakingInProgress)
            {
                CancelMatchmaking();
            }

            // Hide lobby UI
            HideLobbyUI();
        }

        /// <summary>
        /// Called every frame to update the state.
        /// </summary>
        public override void Update()
        {
            // Matchmaking logic is now handled by the NetworkLobbyManager
        }

        /// <summary>
        /// Show the lobby UI.
        /// </summary>
        private void ShowLobbyUI()
        {
            // Load the lobby UI prefab if not already loaded
            if (_lobbyUIPrefab == null)
            {
                _lobbyUIPrefab = Resources.Load<GameObject>("UI/LobbyUI");

                if (_lobbyUIPrefab == null)
                {
                    Debug.LogError("[MatchmakingState] Failed to load LobbyUI prefab from Resources/UI/LobbyUI");
                    return;
                }
            }

            // Instantiate the lobby UI
            _lobbyUIInstance = GameObject.Instantiate(_lobbyUIPrefab);

            // Make sure it persists across scene loads
            GameObject.DontDestroyOnLoad(_lobbyUIInstance);
        }

        /// <summary>
        /// Hide the lobby UI.
        /// </summary>
        private void HideLobbyUI()
        {
            // Destroy the lobby UI instance
            if (_lobbyUIInstance != null)
            {
                GameObject.Destroy(_lobbyUIInstance);
                _lobbyUIInstance = null;
            }
        }

        /// <summary>
        /// Handle lobby joined event.
        /// </summary>
        /// <param name="success">Whether joining the lobby was successful</param>
        private void HandleLobbyJoined(bool success)
        {
            Debug.Log($"[MatchmakingState] Lobby joined. Success: {success}");

            if (!success)
            {
                CompleteMatchmaking(false);
            }
        }

        /// <summary>
        /// Handle lobby left event.
        /// </summary>
        private void HandleLobbyLeft()
        {
            Debug.Log("[MatchmakingState] Lobby left");

            // Return to main menu state
            GameStateManager.Instance.ChangeState(new MainMenuState());
        }

        /// <summary>
        /// Handle game started event.
        /// </summary>
        private void HandleGameStarted()
        {
            Debug.Log("[MatchmakingState] Game started");

            // Complete matchmaking successfully
            CompleteMatchmaking(true);

            // Transition to gameplay state
            GameStateManager.Instance.ChangeState(new GameplayState());
        }

        /// <summary>
        /// Called when matchmaking is complete.
        /// </summary>
        /// <param name="success">Whether matchmaking was successful</param>
        private void CompleteMatchmaking(bool success)
        {
            if (!_isMatchmakingInProgress)
            {
                return;
            }

            _isMatchmakingInProgress = false;
            Debug.Log($"[MatchmakingState] Matchmaking complete. Success: {success}");

            // Trigger the matchmaking complete event
            OnMatchmakingComplete?.Invoke(success);
        }

        /// <summary>
        /// Cancels the matchmaking process.
        /// </summary>
        public void CancelMatchmaking()
        {
            if (!_isMatchmakingInProgress)
            {
                return;
            }

            _isMatchmakingInProgress = false;
            Debug.Log("[MatchmakingState] Matchmaking canceled");

            // Leave the lobby if we're in one
            if (_lobbyManager != null && _lobbyManager.IsInLobby)
            {
                _lobbyManager.LeaveLobby();
            }

            // Trigger the matchmaking complete event with failure
            OnMatchmakingComplete?.Invoke(false);
        }
    }
}
