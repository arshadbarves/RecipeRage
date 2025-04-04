using UnityEngine;

namespace RecipeRage.Core.GameFramework.State.States
{
    /// <summary>
    /// State for the main menu of the game.
    /// </summary>
    public class MainMenuState : GameState
    {
        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        public override void Enter()
        {
            base.Enter();
            
            // Show main menu UI
            Debug.Log("[MainMenuState] Showing main menu UI");
            
            // TODO: Implement main menu UI activation
        }
        
        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        public override void Exit()
        {
            base.Exit();
            
            // Hide main menu UI
            Debug.Log("[MainMenuState] Hiding main menu UI");
            
            // TODO: Implement main menu UI deactivation
        }
        
        /// <summary>
        /// Called every frame to update the state.
        /// </summary>
        public override void Update()
        {
            // Handle main menu input and logic
            
            // TODO: Implement main menu input handling and logic
        }
    }
}
