using UnityEngine;

namespace RecipeRage.Core.GameFramework.State.States
{
    /// <summary>
    /// State for the lobby.
    /// </summary>
    public class LobbyState : IState
    {
        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        public void Enter()
        {
            Debug.Log("[LobbyState] Entered");
            
            // Show the lobby UI
            var uiManager = FindFirstObjectByType<UI.UIManager>();
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
            Debug.Log("[LobbyState] Exited");
            
            // Hide the lobby UI
            var uiManager = FindFirstObjectByType<UI.UIManager>();
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
            var networkManager = Core.Networking.RecipeRageNetworkManager.Instance;
            if (networkManager != null && networkManager.IsHost && networkManager.AreAllPlayersReady())
            {
                // Transition to the game state
                var gameStateManager = GameStateManager.Instance;
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
