using Core.Networking;
using UI;
using UnityEngine;

namespace Core.GameFramework.State.States
{
    /// <summary>
    /// State for the lobby.
    /// </summary>
    public class LobbyState : BaseState
    {
        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        public override void Enter()
        {
            base.Enter();

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
        public override void Exit()
        {
            base.Exit();

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
        public override void Update()
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
        public override void FixedUpdate()
        {
            // Lobby physics update logic
        }
    }
}