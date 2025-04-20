using Core.Networking;
using UI;
using UnityEngine;

namespace Core.GameFramework.State.States
{
    /// <summary>
    /// State for the lobby.
    /// </summary>
    public class LobbyState : IState
    {
        /// <summary>
        /// Name of the state for debugging.
        /// </summary>
        public string StateName => GetType().Name;
        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        public void Enter()
        {
            Debug.Log($"[{StateName}] Entered");

            // Show the lobby UI
            UIManager uiManager = Object.FindFirstObjectByType<UIManager>();
            if (uiManager != null)
            {
                uiManager.ShowLobby();
            }
        }

        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        public void Exit()
        {
            Debug.Log($"[{StateName}] Exited");

            // Hide the lobby UI
            UIManager uiManager = Object.FindFirstObjectByType<UIManager>();
            if (uiManager != null)
            {
                uiManager.HideLobby();
            }
        }

        /// <summary>
        /// Called every frame to update the state.
        /// </summary>
        public void Update()
        {
            // Lobby update logic

            // Check if all players are ready and the host has started the game
            RecipeRageNetworkManager networkManager = RecipeRageNetworkManager.Instance;
            if (networkManager != null && networkManager.IsHost && networkManager.AreAllPlayersReady())
            {
                // Transition to the game state
                GameStateManager gameStateManager = GameStateManager.Instance;
                if (gameStateManager != null)
                {
                    gameStateManager.ChangeState(new GameplayState());
                }
            }
        }

        /// <summary>
        /// Called at fixed intervals for physics updates.
        /// </summary>
        public void FixedUpdate()
        {
            // Lobby physics update logic
        }
    }
}